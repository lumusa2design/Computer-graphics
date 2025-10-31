<div style="center">

[![Texto en movimiento](https://readme-typing-svg.herokuapp.com?font=Fira+Code&size=25&duration=1500&pause=9000&color=8A36D2&center=true&vCenter=true&width=400&height=50&lines=Informática+gráfica)]()

---
<div style="center">

[![Abrir Notebook](https://img.shields.io/badge/📘%20Jupyter-Notebook-F37626?style=for-the-badge&logo=jupyter&logoColor=white)](https://github.com/lumusa2design/Computer-Visualization/blob/main/prac1/VC_P1.ipynb)

</div>


---

![Python](https://img.shields.io/badge/python-3.10-blue?logo=python)
![OpenCV](https://img.shields.io/badge/OpenCV-Enabled-green?logo=opencv)
![Matplotlib](https://img.shields.io/badge/Matplotlib-Graphs-orange?logo=plotly)

</div>

--- 
## Prácticas 6 y 7: Sistema planetario

Para esta práctica partimos de varios códigos de ejemplos suministrados por el docente de la asignatura.

En mi caso he decidido usar el código de base de [`script_07_estrellasyplanetasylunas.js`](./src/script_07_estrellayplanetasylunas.js) pero lo hemos ido modificando para adaptarlo a nuestras necesidades. El código base es el siguiente:

```js
import * as THREE from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";

let scene, renderer;
let camera;
let info;
let grid;
let estrella,
  Planetas = [],
  Lunas = [];
let t0 = 0;
let accglobal = 0.001;
let timestamp;

init();
animationLoop();

function init() {
  info = document.createElement("div");
  info.style.position = "absolute";
  info.style.top = "30px";
  info.style.width = "100%";
  info.style.textAlign = "center";
  info.style.color = "#fff";
  info.style.fontWeight = "bold";
  info.style.backgroundColor = "transparent";
  info.style.zIndex = "1";
  info.style.fontFamily = "Monospace";
  info.innerHTML = "three.js - sol y planetas";
  document.body.appendChild(info);

  //Defino cámara
  scene = new THREE.Scene();
  camera = new THREE.PerspectiveCamera(
    75,
    window.innerWidth / window.innerHeight,
    0.1,
    1000
  );
  camera.position.set(0, 0, 10);

  renderer = new THREE.WebGLRenderer();
  renderer.setSize(window.innerWidth, window.innerHeight);
  document.body.appendChild(renderer.domElement);

  let camcontrols = new OrbitControls(camera, renderer.domElement);

  //Rejilla de referencia indicando tamaño y divisiones
  grid = new THREE.GridHelper(20, 40);
  //Mostrarla en vertical
  grid.geometry.rotateX(Math.PI / 2);
  grid.position.set(0, 0, 0.05);
  scene.add(grid);

  //Objetos
  Estrella(1.8, 0xffff00);
  Planeta(0.5, 4.0, 1.0, 0x00ff00, 1.0, 1.5);
  Planeta(0.8, 5.8, -1.2, 0xffff0f, 1.0, 1.0);

  Luna(Planetas[0], 0.15, 0.75, -3.5, 0xffff00, 0.0);
  Luna(Planetas[0], 0.04, 0.7, 1.5, 0xff0f00, Math.PI / 2);

  //Inicio tiempo
  t0 = Date.now();
  //EsferaChild(objetos[0],3.0,0,0,0.8,10,10, 0x00ff00);
}

function Estrella(rad, col) {
  let geometry = new THREE.SphereGeometry(rad, 10, 10);
  let material = new THREE.MeshBasicMaterial({ color: col });
  estrella = new THREE.Mesh(geometry, material);
  scene.add(estrella);
}

function Planeta(radio, dist, vel, col, f1, f2) {
  let geom = new THREE.SphereGeometry(radio, 10, 10);
  let mat = new THREE.MeshBasicMaterial({ color: col });
  let planeta = new THREE.Mesh(geom, mat);
  planeta.userData.dist = dist;
  planeta.userData.speed = vel;
  planeta.userData.f1 = f1;
  planeta.userData.f2 = f2;

  Planetas.push(planeta);
  scene.add(planeta);

  //Dibuja trayectoria, con
  let curve = new THREE.EllipseCurve(
    0,
    0, // centro
    dist * f1,
    dist * f2 // radios elipse
  );
  //Crea geometría
  let points = curve.getPoints(50);
  let geome = new THREE.BufferGeometry().setFromPoints(points);
  let mate = new THREE.LineBasicMaterial({ color: 0xffffff });
  // Objeto
  let orbita = new THREE.Line(geome, mate);
  scene.add(orbita);
}

function Luna(planeta, radio, dist, vel, col, angle) {
  var pivote = new THREE.Object3D();
  pivote.rotation.x = angle;
  planeta.add(pivote);
  var geom = new THREE.SphereGeometry(radio, 10, 10);
  var mat = new THREE.MeshBasicMaterial({ color: col });
  var luna = new THREE.Mesh(geom, mat);
  luna.userData.dist = dist;
  luna.userData.speed = vel;

  Lunas.push(luna);
  pivote.add(luna);
}

//Bucle de animación
function animationLoop() {
  timestamp = (Date.now() - t0) * accglobal;

  requestAnimationFrame(animationLoop);

  //Modifica rotación de todos los objetos
  for (let object of Planetas) {
    object.position.x =
      Math.cos(timestamp * object.userData.speed) *
      object.userData.f1 *
      object.userData.dist;
    object.position.y =
      Math.sin(timestamp * object.userData.speed) *
      object.userData.f2 *
      object.userData.dist;
  }

  for (let object of Lunas) {
    object.position.x =
      Math.cos(timestamp * object.userData.speed) * object.userData.dist;
    object.position.y =
      Math.sin(timestamp * object.userData.speed) * object.userData.dist;
  }

  renderer.render(scene, camera);
}
```

Todos los conocimientos básicos para empezar la práctica se encuentran en el README de la [práctica 6](https://github.com/otsedom/otsedom.github.io/blob/main/IG/S6/README.md) y de la [práctica 7](https://github.com/otsedom/otsedom.github.io/blob/main/IG/S7/README.md).

## Modificaciones realizadas al código base
Podemos dividir nuestros cambios en 9 bloques funcionales, a saber: 
- Inicialización
- Planetas, estrellas y anillos
- Campo estelar
- Selector de planetas
- Tiempo
- Funciones auxiliares
- Sol
- Fondo
- Animación

### bloque de inicialización

En nuestro caso hemos añadido la librería `lil-gui` que es una librería  que permite crear una interfaz de usuario ligera y sencilla de implementar.
```js
import GUI from "lil-gui";
```
Se crearon varias variables para poder  gestionar los elementos del escenario, que trataremos en sus respectivos apartados. Este bloque serí el siguiente:

```js
let scene, renderer, camera, orbit;
let info, sunCore, sunGlowLayer, sunParticles, haloSprite;
let estrellasCapa1, estrellasCapa2;
let Planetas = [], Lunas = [], Orbitas = [];
let t0 = 0, accglobal = 0.001, timestamp = 0;
let gui, params, camFolder, planetSelector;
const textureLoader = new THREE.TextureLoader();
const clock = new THREE.Clock();
const BASE_PARTICLE_SIZE = 0.06;
const prominences = [];
const PROM_COUNT = 6;
const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();
let nebulaMesh, nebulaMaterial;
```
En general son elementos para gestionar el sol, capas de estrellas, elementos para gestionar la luz que genera el sol...

Añadí `THREE.TextureLoader` para poder cargar las textura de los planetas. Y también `THREE.Clock` para poder gestionar el tiempo de la animación.

Añadimos un material de Nebulosa que será procedural y su forma con `nebulaMateral` y `nebularMesh` además de generar contenedores de partículas.  

```js
const style = document.createElement("style");
style.textContent = `
  html, body { margin:0; height:100%; background:#000; }
  body { overflow:hidden; }
  canvas { display:block; }
`;
document.head.appendChild(style);
```
Cambié con este fragmento de CSS el estilo visual del código para que se vea completo y no haya márgenes.

```js
scene = new THREE.Scene();
scene.background = new THREE.Color(0x000000);
```

Cambie el color de fondo a negro.

```js
camera = new THREE.PerspectiveCamera(
  75,
  window.innerWidth / window.innerHeight,
  0.1,
  2000
);
camera.position.set(0, 0, 14);
```

"Aleje la cámara" y aumente el rango de visión

```js
renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.8;
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setClearColor(0x000000, 1);
document.body.appendChild(renderer.domElement);
```

En este caso hacemos varias operaciones en esta agrupación lógica:
- activamos el Anialiasing
- Corrección de color y mejora de iluminación con SRGB y tone mapping.

```js
orbit = new OrbitControls(camera, renderer.domElement);
orbit.enableDamping = true;
```
Cambia los controles de cámara por un `orbit` con suavizado.

```js
const luz = new THREE.PointLight(0xffffff, 2, 0);
luz.position.set(0, 0, 0);
scene.add(luz);
scene.add(new THREE.AmbientLight(0x404040, 0.6));
```

Cree dos luces, una la que genera el sol y otra ambiente para evitar negras sombras totales.

```js
gui = new GUI();
params = {
  velocidad: 1.0,
  mostrarOrbitas: true,
  mostrarEstrellas: true,
  offX: 3,
  offY: 2,
  offZ: 3,
};
``` 
Creamos una interfaz interactiva con estos parámetros:
- `velocidad`: controla la velocidad de las orbitas
- `mostrarOrbitas` y  `mostrarEstrellas`: un parámetro booleanos que indicará si las órbitas que siguen los planetas se verán  o no, al igual que algunas estrellas de fondo.
- `offX`, `offY`, `offZ`: controlan el desplazamiento de la cámara cuando se enfocan los planetas

```js
const folder = gui.addFolder("Controles");
folder.add(params, "velocidad", 0, 5, 0.1).name("Velocidad");
folder
  .add(params, "mostrarOrbitas")
  .name("Órbitas")
  .onChange((v) => Orbitas.forEach((o) => (o.visible = v)));
folder
  .add(params, "mostrarEstrellas")
  .name("Estrellas")
  .onChange((v) => {
    if (estrellasCapa1) estrellasCapa1.visible = v;
    if (estrellasCapa2) estrellasCapa2.visible = v;
  });
camFolder = gui.addFolder("Cámara");
camFolder.add(params, "offX", -20, 20, 0.1).name("Offset X");
camFolder.add(params, "offY", -20, 20, 0.1).name("Offset Y");
camFolder.add(params, "offZ", -20, 20, 0.1).name("Offset Z");
```
Crea sliders interactivos dentro de la interfaz de usuario para manipular la simulación.

### Planetas, anilos y lunas

```js
function Planeta({
  nombre = "",
  radio,
  dist,
  vel,
  f1 = 1.0,
  f2 = 1.0,
  color = 0xffffff,
  textures = {},
  inclinacion = 0,
  axialTilt = 0,
  rotacionOrbital = 0,
  spin = 0.01,
}) {
  const {
    map,
    normalMap,
    displacementMap,
    displacementScale = 0.0,
    specularMap,
    shininess = 30,
    cloudsMap,
    cloudsOpacity = 0.85,
    cloudsSpeed = 0.008,
    cloudsScale = 1.01,
  } = textures;

  const loadMaybe = (url) => (url ? textureLoader.load(url) : null);
  const texMap = loadMaybe(map);
  if (texMap) texMap.colorSpace = THREE.SRGBColorSpace;
  const texNormal = loadMaybe(normalMap);
  const texDisp = loadMaybe(displacementMap);
  const texSpec = loadMaybe(specularMap);
  const texClouds = loadMaybe(cloudsMap);
  if (texClouds) {
    texClouds.colorSpace = THREE.SRGBColorSpace;
    texClouds.anisotropy =
      (renderer.capabilities.getMaxAnisotropy &&
        renderer.capabilities.getMaxAnisotropy()) ||
      1;
    texClouds.wrapS = texClouds.wrapT = THREE.RepeatWrapping;
  }

  let material;
  if (texNormal || texDisp || texSpec) {
    material = new THREE.MeshPhongMaterial({
      map: texMap || null,
      normalMap: texNormal || null,
      displacementMap: texDisp || null,
      displacementScale: displacementScale,
      specularMap: texSpec || null,
      shininess: shininess,
    });
  } else if (texMap) {
    material = new THREE.MeshStandardMaterial({
      map: texMap,
      metalness: 0.0,
      roughness: 1.0,
    });
  } else {
    material = new THREE.MeshStandardMaterial({
      color,
      metalness: 0.0,
      roughness: 1.0,
    });
  }
```
En este caso, he ampliaod la clase planeta y abstraido debido a que no todos los planetas tienen las mismas características ni los mismos mapas y nivel de detalle, por ejemplo:
- hay planetas rocosos que necesitan una textura, con desplazamiento, como marte o la tierra, mientras que otos gaseosos no disponen de esto como saturno y urano
- La tierra tiene agua y nubes en su entorno.

Además se controlan elementos como la reflexión, inclinación...

```js
function Anillos(planetNode, rInner, rOuter, texturePath, tilt) {
  const tex = textureLoader.load(texturePath);
  const geom = new THREE.RingGeometry(rInner, rOuter, 128);
  const mat = new THREE.MeshStandardMaterial({
    map: tex,
    transparent: true,
    side: THREE.DoubleSide,
    metalness: 0.0,
    roughness: 1.0,
    depthWrite: false,
  });
  const anillos = new THREE.Mesh(geom, mat);
  anillos.rotation.x = tilt;
  planetNode.add(anillos);
}
```
Crea un anillo, con una textura, a doble cara con una geometría, al rededor de un planeta. (Para los anillos de saturno)

```js
function Luna(planetNode, radio, dist, vel, col, angle) {
  const pivote = new THREE.Object3D();
  pivote.rotation.x = angle;
  planetNode.add(pivote);
  const geom = new THREE.SphereGeometry(radio, 24, 16);
  const mat = new THREE.MeshStandardMaterial({
    color: col,
    metalness: 0.0,
    roughness: 1.0,
  });
  const luna = new THREE.Mesh(geom, mat);
  luna.userData = { dist, speed: vel };
  Lunas.push(luna);
  pivote.add(luna);
}
```
Añade modificadores para el material, y una jerarquía de pivote

### Campo estelar

Hacemos una animación con paralelaje suave para el campo estelar. El paralelaje es la sensación visual de que una parte del fondo se mueve a diferente velocidad que otra dando una sensación de movimiento.

![paralelaje](./media/Parallax_scrolling_example_scene.gif)

```js
function crearCampoEstrellas({
  cantidad = 4000,
  radioInterno = 120,
  radioExterno = 140,
  size = 0.03,
  opacidad = 0.9,
  rotacionLenta = new THREE.Vector3(0.00002, 0.00003, 0),
} = {}) {
  const posiciones = new Float32Array(cantidad * 3);
  for (let i = 0; i < cantidad; i++) {
    const u = Math.random();
    const v = Math.random();
    const theta = 2 * Math.PI * u;
    const phi = Math.acos(2 * v - 1);
    const dir = new THREE.Vector3(
      Math.sin(phi) * Math.cos(theta),
      Math.sin(phi) * Math.sin(theta),
      Math.cos(phi)
    );
    const r = radioInterno + Math.random() * (radioExterno - radioInterno);
    const p = dir.multiplyScalar(r);
    posiciones[i * 3] = p.x;
    posiciones[i * 3 + 1] = p.y;
    posiciones[i * 3 + 2] = p.z;
  }
  const geom = new THREE.BufferGeometry();
  geom.setAttribute("position", new THREE.BufferAttribute(posiciones, 3));
  const mat = new THREE.PointsMaterial({
    size: tamanyo,
    depthWrite: false,
    transparent: true,
    opacity: opacidad,
    sizeAttenuation: true,
  });
  const estrellas = new THREE.Points(geom, mat);
  estrellas.userData.rotacionLenta = rotacionLenta;
  scene.add(estrellas);
  return estrellas;
```
Creamos un firmamento 3D con varias estrells que se añadiran en forma de dos cúpulas una inerna y otra externa. Esto provocará, gracias al paralelaje sensación de profundidad.

### Selector del planetas
```js
const nombres = Planetas.map((p) => p.userData.nombre);
planetSelector = camFolder
  .add({ planeta: "Tierra" }, "planeta", nombres)
  .name("Elegir planeta")
  .onChange((v) => { const idx = nombres.indexOf(v); if (idx >= 0) focusPlanetByIndex(idx); });
```
Añade un desplegable que permite que la cambie su posición al del planeta y lo siga
### Tiempo

```js
t0 = Date.now();
window.addEventListener("resize", onResize);
window.addEventListener("click", onClickFocus);
```
Habilita el reescalado y permite seleccionar planeta con un click,

### Funciones auxiliares

```js
const node = Planetas[idx];
node.userData.mesh.getWorldPosition(pos);
camera.position.copy(pos.clone().add(new THREE.Vector3(params.offX, params.offY, params.offZ)));
orbit.target.copy(pos); orbit.update()
```
Es la función que permite cambiar el transform de la cámara a la del planeta centrando su vista

```js

function onResize() {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
}
```
Mantiene la relación del tamaño de cámara y reescala su  relación de aspecto y su proyección al cambiar el tamaño.

```js

function onClickFocus(event) {
  mouse.x = (event.clientX / window.innerWidth) * 2 - 1;
  mouse.y = -(event.clientY / window.innerHeight) * 2 + 1;
  raycaster.setFromCamera(mouse, camera);
  const meshes = Planetas.map((p) => p.userData && p.userData.mesh).filter(
    Boolean
  );
  const hits = raycaster.intersectObjects(meshes, false);
  if (hits.length > 0) {
    const pos = new THREE.Vector3();
    hits[0].object.getWorldPosition(pos);
    const target = pos
      .clone()
      .add(new THREE.Vector3(params.offX, params.offY, params.offZ));
    camera.position.copy(target);
    orbit.target.copy(pos);
    orbit.update();
    const idx = Planetas.findIndex((p) => p.userData.mesh === hits[0].object);
    if (idx >= 0) followIndex = idx;
  }
}
```
Cambia las coordinadas de la camara al clickear sobre el planeta

###  Sol

En este, tenemos que mirar la funcion estrella que se menciona antes en el documento, pero además añadí las siguientes funciones:

```js
function CoronaParticulas(count, rMin, rMax) {
  const positions = new Float32Array(count * 3);
  const colors = new Float32Array(count * 3);
  const hues = new Float32Array(count);
  const phases = new Float32Array(count);
  for (let i = 0; i < count; i++) {
    const r = rMin + Math.random() * (rMax - rMin);
    const theta = Math.random() * Math.PI * 2;
    const phi = Math.acos(2 * Math.random() - 1);
    const x = r * Math.sin(phi) * Math.cos(theta);
    const y = r * Math.sin(phi) * Math.sin(theta);
    const z = r * Math.cos(phi);
    positions[i * 3] = x;
    positions[i * 3 + 1] = y;
    positions[i * 3 + 2] = z;
    const h = 0.05 + Math.random() * 0.08;
    hues[i] = h;
    phases[i] = Math.random() * Math.PI * 2;
    const c = new THREE.Color().setHSL(h, 1, 0.6);
    colors[i * 3] = c.r;
    colors[i * 3 + 1] = c.g;
    colors[i * 3 + 2] = c.b;
  }
  const geom = new THREE.BufferGeometry();
  geom.setAttribute("position", new THREE.BufferAttribute(positions, 3));
  geom.setAttribute("color", new THREE.BufferAttribute(colors, 3));
  geom.userData = { hues, phases, rMin, rMax };
  const sprite = generarSpriteCircular();
  const mat = new THREE.PointsMaterial({
    size: BASE_PARTICLE_SIZE,
    map: sprite,
    transparent: true,
    depthWrite: false,
    blending: THREE.AdditiveBlending,
    vertexColors: true,
    sizeAttenuation: true,
  });
  const corona = new THREE.Points(geom, mat);
  scene.add(corona);
}
```

Genera `count` partículas en un cascarón esférico entre radios `rMin`..`rMax`, con colores en HSL y fases para animación posterior.

Creando un halo de partículas de la corona solar.

```js
function crearProtuberancia(rBase, rOut) {
  const a = Math.random() * Math.PI * 2;
  const tilt = (Math.random() - 0.5) * 0.6;
  const p0 = new THREE.Vector3(
    Math.cos(a) * rBase,
    Math.sin(a) * rBase,
    0
  ).applyAxisAngle(new THREE.Vector3(0, 1, 0), tilt);
  const p1 = p0
    .clone()
    .multiplyScalar(1.15)
    .add(new THREE.Vector3(0, 0, 0.2 + Math.random() * 0.4));
  const p2 = p0
    .clone()
    .setLength((rBase + rOut) * 0.5)
    .add(new THREE.Vector3(0, 0, 0.6 + Math.random() * 0.6));
  const p3 = p0.clone().setLength(rOut);
  const curve = new THREE.CubicBezierCurve3(p0, p1, p2, p3);
  const path = curve.getPoints(80);
  const geom = new THREE.TubeGeometry(
    new THREE.CatmullRomCurve3(path),
    80,
    0.03,
    8,
    false
  );
  const mat = new THREE.MeshBasicMaterial({
    color: 0xffe066,
    transparent: true,
    opacity: 0.0,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
  });
  const mesh = new THREE.Mesh(geom, mat);
  mesh.userData = {
    t: Math.random() * Math.PI * 2,
    speed: 0.8 + Math.random() * 0.6,
  };
  scene.add(mesh);
  prominences.push(mesh);
}
```
Traza una [Curva de Bezier](https://es.javascript.info/bezier-curve)
### Fondo
### Animación

```js

```
## Autores y Reconocimiento


<div align="center">

[![Autor: lumusa2design](https://img.shields.io/badge/Autor-lumusa2design-8A36D2?style=for-the-badge&logo=github&logoColor=white)](https://github.com/lumusa2design)


[![Docente: Profe](https://img.shields.io/badge/Docente-OTSEDOM-0E7AFE?style=for-the-badge&logo=googlescholar&logoColor=white)](https://github.com/otsedom)

[![Centro: EII](https://img.shields.io/badge/Centro-Escuela%20de%20Ingenier%C3%ADa%20Inform%C3%A1tica-00A86B?style=for-the-badge)](https://www.eii.ulpgc.es/es)

</div>


--- 
## Recursos usados












