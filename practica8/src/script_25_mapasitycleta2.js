import * as THREE from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import {
  CSS2DRenderer,
  CSS2DObject,
} from "three/examples/jsm/renderers/CSS2DRenderer";

let scene, renderer, labelRenderer, camera, camcontrols;
let mapa, mapsx, mapsy;
let scale = 20;

let minlon = -130,
  maxlon = -60;
let minlat = 15,
  maxlat = 65;

let texturacargada = false;

let objetos = [];

const datosTerremotos = [];

let fechaInicio = null;
let fechaFin = null;
let fechaActual = null;
let totalMinutos = 0;

let fecha2show;

const clock = new THREE.Clock();
const ventanaHoras = 12;
let umbralMag = 0;

init();
animate();

function init() {
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
  leyenda.textContent = "Color: Sur (azul)  →  Norte (rojo)";
  document.body.appendChild(leyenda);

  scene = new THREE.Scene();

  camera = new THREE.PerspectiveCamera(
    60,
    window.innerWidth / window.innerHeight,
    0.1,
    1000
  );
  camera.position.set(0, 80, 120);

  renderer = new THREE.WebGLRenderer({ antialias: true });
  renderer.setSize(window.innerWidth, window.innerHeight);
  document.body.appendChild(renderer.domElement);

  labelRenderer = new CSS2DRenderer();
  labelRenderer.setSize(window.innerWidth, window.innerHeight);
  labelRenderer.domElement.style.position = "absolute";
  labelRenderer.domElement.style.top = "0px";
  labelRenderer.domElement.style.pointerEvents = "none";
  document.body.appendChild(labelRenderer.domElement);

  camcontrols = new OrbitControls(camera, renderer.domElement);
  camcontrols.enableDamping = true;
  camcontrols.autoRotate = true;
  camcontrols.autoRotateSpeed = 0.3;
  camcontrols.target.set(0, 0, 0);
  camcontrols.update();

  const luzDir = new THREE.DirectionalLight(0xffffff, 1);
  luzDir.position.set(50, 100, 50);
  scene.add(luzDir, new THREE.AmbientLight(0xffffff, 0.3));

  window.addEventListener("resize", onWindowResize);

  const txLoader = new THREE.TextureLoader();
  txLoader.load(
    "src/eeuu/eeuu.webp",
    (texture) => {
      const txaspectRatio = texture.image.width / texture.image.height;
      mapsy = scale;
      mapsx = mapsy * txaspectRatio;
      Plano(0, 0, 0, mapsx, mapsy);
      mapa.material.map = texture;
      mapa.material.needsUpdate = true;
      texturacargada = true;
      cargarDatosTerremotos();
    },
    undefined,
    (error) => {
      console.error("Error cargando textura del mapa:", error);
    }
  );
}

function onWindowResize() {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
  labelRenderer.setSize(window.innerWidth, window.innerHeight);
}

function cargarDatosTerremotos() {
  fetch("src/eeuu/all_month.csv")
    .then((response) => {
      if (!response.ok) {
        throw new Error("Error: " + response.statusText);
      }
      return response.text();
    })
    .then((content) => {
      procesarCSVTerremotos(content);
    })
    .catch((error) => {
      console.error("Error al cargar el CSV de terremotos:", error);
    });
}

function parseCSVLine(line) {
  const regex = /("([^"]*)"|[^,]+)(?=,|$)/g;
  const result = [];
  let match;
  while ((match = regex.exec(line)) !== null) {
    let value = match[1];
    if (value.startsWith('"') && value.endsWith('"')) {
      value = value.slice(1, -1);
    }
    result.push(value);
  }
  return result;
}

function procesarCSVTerremotos(content) {
  const filas = content.trim().split("\n");
  if (filas.length < 2) return;

  const encabezados = parseCSVLine(filas[0]);

  const idx = {
    time: encabezados.indexOf("time"),
    lat: encabezados.indexOf("latitude"),
    lon: encabezados.indexOf("longitude"),
    depth: encabezados.indexOf("depth"),
    mag: encabezados.indexOf("mag"),
    type: encabezados.indexOf("type"),
    place: encabezados.indexOf("place"),
  };

  for (let i = 1; i < filas.length; i++) {
    const linea = filas[i];
    if (!linea.trim()) continue;

    const cols = parseCSVLine(linea);

    const type = cols[idx.type];
    if (type !== "earthquake") continue;

    const lat = parseFloat(cols[idx.lat]);
    const lon = parseFloat(cols[idx.lon]);
    const mag = parseFloat(cols[idx.mag]);
    const depth = parseFloat(cols[idx.depth]);
    const timeStr = cols[idx.time];
    const place = cols[idx.place];

    if (isNaN(lat) || isNaN(lon) || isNaN(mag)) continue;

    if (lon < minlon || lon > maxlon || lat < minlat || lat > maxlat) continue;

    const time = new Date(timeStr);

    datosTerremotos.push({
      lat,
      lon,
      mag,
      depth,
      time,
      place,
    });
  }

  if (datosTerremotos.length === 0) return;

  fechaInicio = datosTerremotos.reduce(
    (min, d) => (d.time < min ? d.time : min),
    datosTerremotos[0].time
  );
  fechaFin = datosTerremotos.reduce(
    (max, d) => (d.time > max ? d.time : max),
    datosTerremotos[0].time
  );
  fechaActual = new Date(fechaInicio);

  crearBarrasTerremotos();
}

function crearBarrasTerremotos() {
  const baseGeo = new THREE.CylinderGeometry(0.3, 0.3, 1, 16);

  datosTerremotos.forEach((d) => {
    const mlon = Map2Range(d.lon, minlon, maxlon, -mapsx / 2, mapsx / 2);
    const mlat = Map2Range(d.lat, minlat, maxlat, -mapsy / 2, mapsy / 2);

    const mat = new THREE.MeshPhongMaterial({
      color: 0xffffff,
      transparent: true,
      opacity: 0.0,
    });
    const barra = new THREE.Mesh(baseGeo, mat);

    barra.position.set(mlon, 0.001, mlat);
    barra.userData = {
      ...d,
      currentHeight: 0,
      targetHeight: 0,
      label: null,
    };

    const labelDiv = document.createElement("div");
    labelDiv.textContent = d.mag.toFixed(1);
    labelDiv.style.color = "#ffffff";
    labelDiv.style.fontSize = "12px";
    labelDiv.style.fontFamily = "Monospace";
    labelDiv.style.textShadow = "0 0 4px #000";
    const label = new CSS2DObject(labelDiv);
    label.position.set(0, 0, 0);
    barra.add(label);
    barra.userData.label = label;

    scene.add(barra);
    objetos.push(barra);
  });

  actualizarBarras(0);
}

function Map2Range(val, vmin, vmax, dmin, dmax) {
  const t = 1 - (vmax - val) / (vmax - vmin);
  return dmin + t * (dmax - dmin);
}

function Plano(px, py, pz, sx, sy) {
  const geometry = new THREE.PlaneGeometry(sx, sy, 200, 200);
  const material = new THREE.MeshBasicMaterial({
    side: THREE.DoubleSide,
  });
  const mesh = new THREE.Mesh(geometry, material);
  mesh.rotation.x = -Math.PI / 2;
  mesh.position.set(px, py, pz);
  scene.add(mesh);
  mapa = mesh;
}

function actualizarFecha() {
  if (!fechaInicio || !fechaFin) return;

  totalMinutos += 5;

  const rangoMs = fechaFin.getTime() - fechaInicio.getTime();
  const maxMinutos = rangoMs / 60000;

  if (totalMinutos > maxMinutos) {
    totalMinutos = 0;
  }

  fechaActual = new Date(fechaInicio.getTime() + totalMinutos * 60000);

  const opciones = {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  };
  fecha2show.innerHTML = fechaActual.toLocaleString("es-ES", opciones);
}

function actualizarBarras(delta) {
  if (!fechaActual) return;

  const ventanaMs = ventanaHoras * 3600 * 1000;

  objetos.forEach((barra) => {
    const d = barra.userData;

    const visibleTiempo = d.time <= fechaActual;
    if (!visibleTiempo || d.mag < umbralMag) {
      barra.visible = false;
      if (d.label) d.label.visible = false;
      d.currentHeight = 0;
      barra.scale.y = 0.001;
      barra.position.y = 0.001;
      if (d.label) d.label.position.y = 0;
      return;
    }

    barra.visible = true;
    if (d.label) d.label.visible = true;

    const mag = d.mag || 0;
    const baseHeight = 1;
    const extra = Math.max(0, mag - 2) * 4;
    d.targetHeight = baseHeight + extra;

    const speed = 0.8;
    d.currentHeight +=
      (d.targetHeight - d.currentHeight) * Math.min(speed * delta, 1);

    const altura = d.currentHeight;
    barra.scale.y = altura;
    barra.position.y = altura / 2;

    if (d.label) {
      d.label.element.textContent = d.mag.toFixed(1);
      d.label.position.y = altura / 2 + 0.8;
    }

    const ahora = fechaActual.getTime();
    const tEvento = d.time.getTime();
    const dtMs = ahora - tEvento;

    if (dtMs >= 0 && dtMs < 30 * 60 * 1000) {
      const fase = (ahora / 1000) * 4.0;
      const s = 1 + 0.2 * Math.sin(fase);
      barra.scale.x = s;
      barra.scale.z = s;
    } else {
      barra.scale.x = 1;
      barra.scale.z = 1;
    }

    let alpha = 1 - dtMs / ventanaMs;
    alpha = Math.max(0, Math.min(1, alpha));
    barra.material.opacity = alpha;
    if (d.label) d.label.element.style.opacity = alpha.toString();

    let latNorm = (d.lat - minlat) / (maxlat - minlat);
    latNorm = Math.max(0, Math.min(1, latNorm));
    const hue = (1 - latNorm) * 0.66;
    const color = new THREE.Color().setHSL(hue, 1.0, 0.5);
    barra.material.color.copy(color);
  });
}

function animate() {
  requestAnimationFrame(animate);

  const delta = clock.getDelta();

  if (texturacargada && fechaInicio) {
    actualizarFecha();
    actualizarBarras(delta);
  }

  camcontrols.update();
  renderer.render(scene, camera);
  labelRenderer.render(scene, camera);
}
