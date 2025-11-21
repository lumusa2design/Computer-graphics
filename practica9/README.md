<div style="center">

[![Texto en movimiento](https://readme-typing-svg.herokuapp.com?font=Fira+Code&size=25&duration=1500&pause=9000&color=8A36D2&center=true&vCenter=true&width=400&height=50&lines=Informática+gráfica)]()




---

![JavaScript](https://img.shields.io/badge/JavaScript-ES2020+-yellow?logo=javascript)
![Three.js](https://img.shields.io/badge/Three.js-r160-black?logo=three.js)
![WebGL](https://img.shields.io/badge/WebGL-2.0-990000?logo=webgl)
![lil-gui](https://img.shields.io/badge/lil--gui-UI%20controls-4B8BBE)
![GLSL](https://img.shields.io/badge/GLSL-Shaders-8A2BE2)


</div>

--- 
## Prácticas 9: Shaders
Para la tarea de esta práctica se marcan dos posibilidades  para hacer, o bien reutilizar una práctica anterior (en mi caso debido a tener ya conocimiento previo de la asignatura, ya lo hice en la propia [práctica](/Practica6y7/README.md)) e implementar un shader en dicha práctica o bien hacer un shader compatible con [El editor de *The book of Shaders*](http://editor.thebookofshaders.com/) y además que tenga un máximo de 512 *bytes*. 

Decidí hacer cuatro versiones del mismo *shader* generativo:

- Aplicación del máximo potencia del shader (sin tener en cuenta el máximo).
- Versión Tiny del shader de máximo potencial (y perdiendo ligeras funciones de visualización).
- Versión short del shader (más ligera pero mucho peor visualmente)
- Version Tiny del short shader (Ajustada a 512 bytes)

En general, en internet lo que más encontre de referencia fueron mandalas a la hora de hacer shaders generativos, por lo que, para encontrar más referencias decidí hacer una mandala como parte del shader generativo.

Por cuestiones de evitar redundancia de código, y por lo costoso que es comentar el *tiny code* solo comentaremos el ejemplo donde se muestra un máximo potencial, y la versión reducida. Además como la versión *tiny* del primero reduce algunas funcionalidades, lo expondré en el propio shader.

### Shader máximo

```glsl
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}
```

Esta función es la encargada de generar el ruido , que se generará gracias al angulo de la capa creada. Usando convirtiendo el `vec2` en un valor pseudoaleatrio.




```glsl
float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);

    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);

    return mix(a, b, u.x) +
           (c - a) * u.y * (1.0 - u.x) +
           (d - b) * u.x * u.y;
}
```

Se generará el ruido 3D gracias a la función hash explicada anteriormente. Los valores que la componen son:
- `i`: celda entera
- `f`: parte fraccionaria
- `a`, `b`, `c`, `d`: valores aleatorios en las 4 esquinas.
- `u`: interpolador basado en Hermite (para evitar cambios bruscos y aliasing) 

```glsl
float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    mat2 M = mat2(1.6, 1.2, -1.2, 1.6);
    for (int i = 0; i < 5; i++) {
        v += a * noise(p);
        p = M * p;
        a *= 0.5;
    }
    return v;
}
```
Viene de [Fractal Brownian Motion](https://thebookofshaders.com/13/):
- Suma varias octavas sobre el ruido cada vez:
    - Escala `p` con matriz `M` (donde rota o escala en el espacio).
    - reduce la amplitud de `a`
- Devuelve el ruido con mayor variedad 

```glsl
vec3 palette(float t) {
    vec3 a = vec3(0.905,0.081,0.107);
    vec3 b = vec3(0.850,0.768,0.059);
    vec3 c = vec3(0.848,0.745,0.900);
    vec3 d = vec3(0.670,0.328,0.118);
    return a + b * cos(6.2831 * (c * t + d));
}
```

Genera la paleta de colores, usando tonos de colores rojos, amarillos, blancos y verdes.
- Usa un coseno para generar un color suave en función del escalar `t`
- `a, b, c, d`: controlan el offset y el color.

```glsl
vec2 rot(vec2 p,float a){
    float s=sin(a), c=cos(a);
    return mat2(c,-s,s,c)*p;
}
```
Rotación del punto `p` con el  ángulo `a`.

```glsl
vec3 mandalaLayer(vec2 uv,float t,float petals,float radialScale,float hueShift,float ringFreq){
    float PI = 3.14159265;
    float r = length(uv);
    float ang = atan(uv.y,uv.x);

    float wedge = 2.0*PI/petals;
    ang = abs(mod(ang,wedge)-0.5*wedge);

    vec2 p = vec2(ang*radialScale, r*3.0);

    float f1 = fbm(p+vec2(0.0,t));
    float f2 = fbm(p*2.0-vec2(t*1.3,t*0.7));
    vec2 pw = p + 0.94*vec2(f1,f2);
    float f3 = fbm(pw+vec2(t*0.8,-t*0.6));

    float mask = smoothstep(-0.18,0.9,f1+f2-r*1.85);

    float rings = sin(ringFreq*r + f3*6.0);
    float pattern = mix(f3,rings,0.6);
    pattern = 0.5 + 0.5*pattern;

    float hue = pattern + 1.3*r + 0.06*f2 + hueShift;

    vec3 colR = palette(hue+0.015);
    vec3 colG = palette(hue);
    vec3 colB = palette(hue-0.015);
    vec3 col = vec3(colR.r,colG.g,colB.b);
    col *= 0.3 + 1.6*mask;
    float edge = smoothstep(0.4,0.0,abs(rings))*mask;
    col += vec3(0.7,0.8,1.0)*edge*0.25; 
    float eps = 0.01;
    float h0 = f3;
    float hx = fbm(pw+vec2(eps,0.0)+vec2(t*0.8,-t*0.6));
    float hy = fbm(pw+vec2(0.0,eps)+vec2(t*0.8,-t*0.6));
    vec3 n = normalize(vec3(hx-h0,hy-h0,0.5));

    float lightAng = u_time*0.35;
    vec3 lightDir = normalize(vec3(cos(lightAng),sin(lightAng),0.7));

    float diff = clamp(dot(n,lightDir),0.0,1.0);
    float spec = pow(max(dot(reflect(-lightDir,n),vec3(0,0,1)),0.0),10.0);

    col *= 0.55 + 0.7*diff;        
    col += vec3(1.2,1.1,1.3)*spec*0.25; 

    return col;
}
```

- `uv`: coordenadas espaciales ya transformadas.
- `t`: tiempo.
- `petals`: número de pétalos.
- `radialScale`: escala en el eje radial.
- `hueShift`: desplazamiento en la paleta de color.
- `ringFreq`: frecuencia de los anillos radiales.


```glsl
void main() {
    vec2 uv = (gl_FragCoord.xy - 0.5 * u_resolution.xy) / u_resolution.y;

    float t = u_time * 0.25;
    float r = length(uv);  
    float ang = atan(uv.y, uv.x);
    float N = 15.0; 
    float PI = 3.14159265;
    float wedge = 2.0 * PI / N;
    ang = abs(mod(ang, wedge) - 0.5 * wedge);
    vec2 p = vec2(ang * 2.5, r * 3.0);
    float f1 = fbm(p + vec2(0.0, t));
    float f2 = fbm(p * 2.0 - vec2(t * 1.3, t * 0.7));
    vec2 pw = p + 0.940 * vec2(f1, f2);
    float f3 = fbm(pw + vec2(t * 0.8, -t * 0.6));
    float mask = smoothstep(-0.176, 0.904, f1 + f2 - r * 1.852);
    float rings = sin(8.0 * r + f3 * 6.0);
    float pattern = mix(f3, rings, 0.6);
    pattern = 0.5 + 0.5 * pattern;
    float hueParam = pattern + 1.308 * r + 0.062 * f2;
    vec3 col = palette(hueParam);

    col *= 0.3 + 1.740 * mask;

    float vignette = smoothstep(1.2, 0.05, r);
    col *= vignette;

    gl_FragColor = vec4(col, 1.0);
}
```
- `t`: Controla toda la animación.
- `r`: Es la distancia respecto al centro.
- `ang`: Es el ángulo del píxel 
- `N`: Es el número de sectores.
- `wedge`: Tamaño angular.
- `p`: Es la seed del ruido fractal



### Shader short

```glsl
void main(){
    vec2 u=(gl_FragCoord.xy-.5*u_resolution.xy)/u_resolution.y;
    float r=length(u),a=atan(u.y,u.x),k=15.;
    a=abs(mod(a,6.28318/k)-3.14159/k);
    float v=sin(10.*r+4.*cos(8.*a+u_time))+.5*sin(3.*u_time+5.*r);
    vec3 c=vec3(.905,.081,.107)+vec3(.85,.768,.059)*cos(v+vec3(0.,2.,4.));
    c*=smoothstep(1.,.1,r);
    gl_FragColor=vec4(c,1.);
}
```
desgranemos un poco este código:

```glsl
    vec2 u=(gl_FragCoord.xy-.5*u_resolution.xy)/u_resolution.y;
```

- traslada la pantalla para que el centro sea la coordenada (0,0)
- escala `u_resolution.y` para mantender la proporción

```glsl
float r=length(u),a=atan(u.y,u.x),k=15.;
```

trabajamos con coordenadas polares:
- `r`: distancia  respecto al centro.
- `a`: ángulo
- `k`: número de pétalos

```glsl
a=abs(mod(a,6.28318/k)-3.14159/k);
```

- Envuelve el angulo con una anchura $2\pi/k$
- Centra el sector en 0
- Usa valores absolutos.

```glsl
float v=sin(10.*r+4.*cos(8.*a+u_time))+.5*sin(3.*u_time+5.*r);
```
Crea un patrón oscilatorio con ondas dando la sensación de mandala que tiene.

```glsl
vec3 c=vec3(.905,.081,.107)+vec3(.85,.768,.059)*cos(v+vec3(0.,2.,4.));
```
genera la paleta de colores.

```glsl
c*=smoothstep(1.,.1,r);
```
Genera una viñeta radial

```glsl
gl_FragColor=vec4(c,1.);
```

renderiza

## Resultado

El video del shader generativo se puede ver aquí:

[video](https://youtu.be/W3NwYJGskkE)

o puede intentar verlo aquí:

<video controls width="600">
  <source src="./media/generative_shader.webm" type="video/webm">
</video>

Y si no, se encuentra en la carpeta *media* del *README* de la práctica 9.


El video del shader generativo reducido sería el siguiente:
[video](https://youtu.be/DQUkxEr3-Bg)

o puede intentar verlo aquí:

<video controls width="600">
  <source src="./media/tiny_generative_shader.webm" type="video/webm">
</video>


Y la versión reducida sería la siguiente:
[video](https://youtu.be/qJV0AU4mPEA)

o puede intentar verlo aquí:

<video controls width="600">
  <source src="./media/short_shader.webm" type="video/webm">
</video>

## Autores y Reconocimiento


<div align="center">

[![Autor: lumusa2design](https://img.shields.io/badge/Autor-lumusa2design-8A36D2?style=for-the-badge&logo=github&logoColor=white)](https://github.com/lumusa2design)


[![Docente: Profe](https://img.shields.io/badge/Docente-OTSEDOM-0E7AFE?style=for-the-badge&logo=googlescholar&logoColor=white)](https://github.com/otsedom)

[![Centro: EII](https://img.shields.io/badge/Centro-Escuela%20de%20Ingenier%C3%ADa%20Inform%C3%A1tica-00A86B?style=for-the-badge)](https://www.eii.ulpgc.es/es)

</div>


--- 
## Recursos usados
En general se ha usado la [API](https://threejs.org/docs/) de `three.js` dado que tiene muchos elementos explicados de forma exhaustiva de como usarse y la documentación introductoria del profesorado que mencionamos anteriormente.

Además se ha usado como base de datos la recogida en [USGS](https://earthquake.usgs.gov/earthquakes/feed/v1.0/csv.php)

Destacar que aunque sea de three.js consulte [CSS2DRenderer](https://threejs.org/docs/#CSS2DRenderer) para las etiquetas 

Uso de chatGPT para corrección de errores y búsqueda de herramientas.
