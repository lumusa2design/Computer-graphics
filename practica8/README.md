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
## Prácticas 8: Visualización de datos

Para esta práctica partimos de varios códigos de ejemplos suministrados por el docente de la asignatura.

En mi caso he decidido usar el código de base de [`script_25_ampasitycleta2.js`](./src/script_07_estrellayplanetasylunas.js) pero lo hemos ido modificando para adaptarlo a nuestras necesidades. El código base es el siguiente:

```js
import * as THREE from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";

let scene, renderer, camera, camcontrols;
let mapa,
  mapsx,
  mapsy,
  scale = 5;

// Latitud y longitud de los extremos del mapa de la imagen
let minlon = -15.46945,
  maxlon = -15.39203;
let minlat = 28.07653,
  maxlat = 28.18235;
// Dimensiones textura (mapa)
let txwidth, txheight;
let texturacargada = false;

let objetos = [];
//Datos fecha, estaciones, préstamos
const fechaInicio = new Date(2018, 4, 1); //Desde mayo (enero es 0)
let fechaActual;
let totalMinutos = 480, //8:00 como arranque
  fecha2show;
const datosSitycleta = [],
  datosEstaciones = [];

init();
animate();

function init() {
  //Muestra fecha actual como título
  fecha2show = document.createElement("div");
  fecha2show.style.position = "absolute";
  fecha2show.style.top = "30px";
  fecha2show.style.width = "100%";
  fecha2show.style.textAlign = "center";
  fecha2show.style.color = "#fff";
  fecha2show.style.fontWeight = "bold";
  fecha2show.style.backgroundColor = "transparent";
  fecha2show.style.zIndex = "1";
  fecha2show.style.fontFamily = "Monospace";
  fecha2show.innerHTML = "";
  document.body.appendChild(fecha2show);

  scene = new THREE.Scene();
  camera = new THREE.PerspectiveCamera(
    75,
    window.innerWidth / window.innerHeight,
    0.1,
    1000
  );
  //Posición de la cámara
  camera.position.z = 5;

  renderer = new THREE.WebGLRenderer();
  renderer.setSize(window.innerWidth, window.innerHeight);
  document.body.appendChild(renderer.domElement);

  camcontrols = new OrbitControls(camera, renderer.domElement);

  //CARGA TEXTURA (MAPA)
  //Crea plano, ajustando su tamaño al de la textura, manteniendo relación de aspecto
  const tx1 = new THREE.TextureLoader().load(
    "src/mapaLPGC.png",

    // Acciones a realizar tras la carga
    function (texture) {
      //Objeto sobre el que se mapea la textura del mapa
      //Plano para mapa manteniendo proporciones de la textura de entrada
      const txaspectRatio = texture.image.width / texture.image.height;
      mapsy = scale;
      mapsx = mapsy * txaspectRatio;
      Plano(0, 0, 0, mapsx, mapsy);

      //Dimensiones, textura
      //console.log(texture.image.width, texture.image.height);
      mapa.material.map = texture;
      mapa.material.needsUpdate = true;

      texturacargada = true;

      //
      //CARGA DE DATOS
      //Antes debe disponerse de las dimensiones de la textura, su carga debe haber finalizado
      //Lectura del archivo csv con localizaciones de las estaciones Sitycleta
      fetch("src/Geolocalización estaciones sitycleta.csv")
        .then((response) => {
          if (!response.ok) {
            throw new Error("Error: " + response.statusText);
          }
          return response.text();
        })
        .then((content) => {
          procesarCSVEstaciones(content);
        })
        .catch((error) => {
          console.error("Error al cargar el archivo:", error);
        });

      //Carga datos de un año de préstamos desde el csv
      fetch("src/SITYCLETA-2018.csv")
        .then((response) => {
          if (!response.ok) {
            throw new Error("Error: " + response.statusText);
          }
          return response.text();
        })
        .then((content) => {
          procesarCSVAlquileres(content);
        })
        .catch((error) => {
          console.error("Error al cargar el archivo:", error);
        });
    } //function Texture
  ); //load
}

//Procesamiento datos csv
function procesarCSVEstaciones(content) {
  const sep = ";"; // separador ;
  const filas = content.split("\n");

  // Primera fila es el encabezado, separador ;
  const encabezados = filas[0].split(sep);
  // Obtiene índices de columnas de interés
  const indices = {
    id: encabezados.indexOf("idbase"),
    nombre: encabezados.indexOf("nombre"),
    lat: encabezados.indexOf("latitud"),
    lon: encabezados.indexOf("altitud"),
  };
  console.log(indices);

  // Extrae los datos de interés
  for (let i = 1; i < filas.length; i++) {
    const columna = filas[i].split(sep); // separador ;
    if (columna.length > 1) {
      // No fila vacía
      datosEstaciones.push({
        id: columna[indices.idbase],
        nombre: columna[indices.nombre],
        lat: columna[indices.lat],
        lon: columna[indices.lon],
      });

      //longitudes crecen hacia la derecha, como la x
      let mlon = Map2Range(
        columna[indices.lon],
        minlon,
        maxlon,
        -mapsx / 2,
        mapsx / 2
      );
      //Latitudes crecen hacia arriba, como la y
      let mlat = Map2Range(
        columna[indices.lat],
        minlat,
        maxlat,
        -mapsy / 2,
        mapsy / 2
      );
      //Esfera en posición estaciones
      Esfera(mlon, mlat, 0, 0.01, 10, 10, 0x009688);
    }
  }
  console.log("Archivo csv estaciones cargado");
}

function procesarCSVAlquileres(content) {
  const sep = ";"; // separador ;
  const filas = content.split("\n");

  // Primera fila es el encabezado, separador ;
  const encabezados = filas[0].split(sep);

  // Obtiene índices de columnas de interés
  const indices = {
    t_inicio: encabezados.indexOf("Start"),
    t_fin: encabezados.indexOf("End"),
    p_inicio: encabezados.indexOf("Rental place"),
    p_fin: encabezados.indexOf("Return place"),
  };

  // Extrae los datos de interés
  for (let i = 1; i < filas.length; i++) {
    const columna = filas[i].split(sep);
    if (columna.length > 1) {
      // No fila vacía
      datosSitycleta.push({
        t_inicio: convertirFecha(columna[indices.t_inicio]),
        t_fin: convertirFecha(columna[indices.t_fin]),
        p_inicio: columna[indices.p_inicio],
        p_fin: columna[indices.p_fin],
      });
    }
  }
  console.log("Archivo csv alquileres cargado");
}

//Dados los límites del mapa del latitud y longitud, mapea posiciones en ese rango
//valor, rango origen, rango destino
function Map2Range(val, vmin, vmax, dmin, dmax) {
  //Normaliza valor en el rango de partida, t=0 en vmin, t=1 en vmax
  let t = 1 - (vmax - val) / (vmax - vmin);
  return dmin + t * (dmax - dmin);
}

function Esfera(px, py, pz, radio, nx, ny, col) {
  let geometry = new THREE.SphereBufferGeometry(radio, nx, ny);
  let material = new THREE.MeshBasicMaterial({
    color: col,
  });
  let mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(px, py, pz);
  objetos.push(mesh);
  scene.add(mesh);
}

function Plano(px, py, pz, sx, sy) {
  let geometry = new THREE.PlaneGeometry(sx, sy);
  let material = new THREE.MeshBasicMaterial({});
  let mesh = new THREE.Mesh(geometry, material);
  mesh.position.set(px, py, pz);
  scene.add(mesh);
  mapa = mesh;
}

// Función para convertir una fecha en formato DD/MM/YYYY HH:mm, presenmte en archivo de préstamos, a Date
function convertirFecha(fechaStr) {
  const [fecha, hora] = fechaStr.split(" ");
  const [dia, mes, año] = fecha.split("/").map(Number);
  const [horas, minutos] = hora.split(":").map(Number);
  return new Date(año, mes - 1, dia, horas, minutos); // mes es 0-indexado
}

function actualizarFecha() {
  totalMinutos += 1;
  // Añade fecha de partida
  fechaActual = new Date(fechaInicio.getTime() + totalMinutos * 60000);

  // Formatea salida
  const opciones = {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    timeZoneName: "short",
  };
  //Modifica en pantalla
  fecha2show.innerHTML = fechaActual.toLocaleString("es-ES", opciones);
}

//Paradas con p´restamo activo en un momento dado
function filtraparadasActivas() {
  //Filtra registros activos
  const registrosFiltrados = datosSitycleta.filter((registro) => {
    return registro.t_inicio <= fechaActual && registro.t_fin >= fechaActual;
  });

  //Hay alquileres activos a esa hora
  if (registrosFiltrados.length > 0) {
    //Parada de inicio de alquileres activos
    const estacionesA = new Set(
      registrosFiltrados.map((registro) => registro.p_inicio)
    );
    //Parada de fin de alquileres activos
    const estacionesB = new Set(
      registrosFiltrados.map((registro) => registro.p_fin)
    );

    let i = 0;
    //Altera tamaño y color de las estaciones con alquiler activo, o lo recupera
    for (const estacion of datosEstaciones) {
      if (
        estacionesA.has(estacion.nombre) ||
        estacionesB.has(estacion.nombre)
      ) {
        //Varío color según casos
        if (
          estacionesA.has(estacion.nombre) &&
          estacionesB.has(estacion.nombre)
        )
          objetos[i].material.color.set(0x005aa0);
        else {
          if (estacionesA.has(estacion.nombre))
            objetos[i].material.color.set(0x8a2be2);
          else objetos[i].material.color.set(0xc81e3c);
        }
        objetos[i].scale.set(2, 2, 2);
      } else {
        //Vuelve a l estado por defecto
        objetos[i].material.color.set(0x009688);
        objetos[i].scale.set(1, 1, 1);
      }
      i++;
    }
  }
}

//Bucle de animación
function animate() {
  if (texturacargada) {
    //Actualiza hora actual
    actualizarFecha();
    //Filtra alquileres activos y destaca estaciones afectadas
    filtraparadasActivas();
  }

  requestAnimationFrame(animate);

  renderer.render(scene, camera);
}
``` 
Todos los conocimientos necesarios se encuentran desarrollados en el README de la [practica 8](https://github.com/otsedom/otsedom.github.io/blob/main/IG/S8/README.md).

## Cambios realizados en la práctica
Vamos a desarrollar los diferentes fragmentos de la práctica que hemos modificado para nuestra práctica.

El primer cambio realizado ha sido cambiar el mapa a uno diferente. En mi caso escogi el mapa de los EEUU en forma de homenaje a mi padre que se formó en como atender emergencias de salvamento y protección allí, y por ello también decidí seleccionar una base de datos sobre terremotos, que a su vez, me pareció la más sencilla de leer como humano, y de comprobar (dado que si esta bien configurada, deberían de coincidir la visualización de datos, en gran parte con la falla de San Andrés).

### Estructura 

Como ahora usamos el mapa de Estados Unidos, tenemos que ajustar nuestros datos a los correspondientes del mapa de EEUU.

```js
let minlon = -130, maxlon = -60;
let minlat = 15, maxlat = 65;
```

Declaramos la variable:
`datosTerremotos=[]`

- Ventana de tiempo para visualización de datos: `const ventanaHoras = 12`;
- Filtrado de magnitud: `let umbralMag = 0;`
- Animaciones mas suaves `const clock = new THREE.Clock`  
- Fechas directamente salidas directamente del CSV

### HUB y leyenda

Se añade:
```js
  const leyenda = document.createElement("div");
  leyenda.style.position = "absolute";
  leyenda.style.left = "20px";
  leyenda.style.bottom = "20px";
  leyenda.style.width = "220px";
  leyenda.style.height = "20px";
  leyenda.style.background =
    "linear-gradient(to right, blue, cyan, green, yellow, red)";
  leyenda.style.border = "1px solid #fff";
  leyenda.style.fontSize = "11px";
  leyenda.style.color = "#fff";
  leyenda.style.fontFamily = "Monospace";
  leyenda.style.padding = "2px 4px";
  leyenda.textContent = "Color: Norte (azul)  →  Sur (rojo)";
  document.body.appendChild(leyenda);
```

En este caso he puesto una mini leyenda, para aclarar los colores dados que los colores donde se visualizarán los terremotos dependeran de la longitud y la latitud (Quería hacerlo relacionado con el valor de la escala de Richter pero por suerte, no han habido terremotos tan grandes en bastante tiempo). En  este caso la leyenda es un indicativo visual de la posición relativa en la imagen.

### Render de etiquetas

Si bien se mantiene el render principal, se han añadido etiquetas para ver el valor en la escala de Richter.

```js
labelRenderer = new CSS2DRenderer();
```
y se llama en el animate
```js
renderer.render(scene, camera);
labelRenderer.render(scene, camera);
```

### Camara y Controles

Modifiqué la cámara para ajustarse al mapa de EEUU.
```js
camera.position.set(0, 20, 40);
```

Controles con autorotación y amortiguación

```js
camcontrols.autoRotate = true;
camcontrols.autoRotateSpeed = 0.3;
camcontrols.enableDamping = true;
```

### Adaptación de los datos
 Cambie los CSV que habían por un único CSV que cambia el formato de separados por `;` a separado por `,` Además de que las columnas cambian así que ahora leerá las columnas de  `time, latitude, longitude, depth, mag, type, place.`.

 Con la columna de `type` filtrará para que sea terremoto exclusivamente y no cuenta explosiones u otro tipo de interferencias.

 ### Representación gráfica

 Hemos dejado de usar puntos o esferas a usar cilindros para indicar el punto donde se encuentra, la altura del cilindro dependerá de la escala.

 Para cada terremoto se crea un cilindro:

 ```js
 const baseGeo = new THREE.CylinderGeometry(0.3, 0.3, 1, 16);
 ```

 Con su respectivo material:

 ```
 new THREE.MeshPhongMaterial({
  color: 0xffffff,
  transparent: true,
  opacity: 0.0,
});
 ```

 y se guarda en user data:

 ```js
 { ...d, currentHeight: 0, targetHeight: 0, label: null }
 ```

 y se añade la label 2D para ver la magnitud con la escala de Richter:

 ```js
 const labelDiv = document.createElement("div");
labelDiv.textContent = d.mag.toFixed(1);
const label = new CSS2DObject(labelDiv);
barra.add(label);
 ```

### Animación de barras

Ventana temporal deslizante para la simulación:

```js
const ventanaMs = ventanaHoras * 3600 * 1000;
```

Filtrado para evitar incongruencias con el tiempo o si es muy ligero:
```js
if (!visibleTiempo || d.mag < umbralMag) { ... }
```

Altura según la magnitud con  *fade in*

```js
const baseHeight = 1;
const extra = Math.max(0, mag - 2) * 4;
d.targetHeight = baseHeight + extra;

const speed = 0.8;
d.currentHeight +=
  (d.targetHeight - d.currentHeight) * Math.min(speed * delta, 1);
```

- `targetHeight` depende de la magnitud
- `currentHeight` se interpola para que aparezca mas despacio

Texto flotante:
```js
d.label.element.textContent = d.mag.toFixed(1);
d.label.position.y = altura / 2 + 0.8;
```
Efecto pulsar:
```js
if (dtMs >= 0 && dtMs < 30 * 60 * 1000) {
  const fase = (ahora / 1000) * 4.0;
  const s = 1 + 0.2 * Math.sin(fase);
  barra.scale.x = s;
  barra.scale.z = s;
} else {
  barra.scale.x = 1;
  barra.scale.z = 1;
}
```

*Fade-in* y *Fade-out*

```js
let alpha = 1 - dtMs / ventanaMs;
alpha = Math.max(0, Math.min(1, alpha));
barra.material.opacity = alpha;
d.label.element.style.opacity = alpha.toString();
```

Control de color según latitud
```js
let latNorm = (d.lat - minlat) / (maxlat - minlat);
latNorm = Math.max(0, Math.min(1, latNorm));
const hue = (1 - latNorm) * 0.66;
const color = new THREE.Color().setHSL(hue, 1.0, 0.5);
barra.material.color.copy(color);  
```

## Videos de la práctica




## Autores y Reconocimiento


<div align="center">

[![Autor: lumusa2design](https://img.shields.io/badge/Autor-lumusa2design-8A36D2?style=for-the-badge&logo=github&logoColor=white)](https://github.com/lumusa2design)


[![Docente: Profe](https://img.shields.io/badge/Docente-OTSEDOM-0E7AFE?style=for-the-badge&logo=googlescholar&logoColor=white)](https://github.com/otsedom)

[![Centro: EII](https://img.shields.io/badge/Centro-Escuela%20de%20Ingenier%C3%ADa%20Inform%C3%A1tica-00A86B?style=for-the-badge)](https://www.eii.ulpgc.es/es)

</div>


--- 
## Recursos usados
En general se ha usado la [API](https://threejs.org/docs/) de `three.js` dado que tiene muchos elementos explicados de forma exhaustiva de como usarse y la documentación introductoria del profesorado que mencionamos anteriormente. Algunos ejemplos relevantes de three.js son:












