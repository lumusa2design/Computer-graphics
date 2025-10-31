import * as THREE from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import GUI from "lil-gui";

let scene, renderer, camera, orbit;
let info, sunCore, sunGlowLayer, sunParticles, haloSprite;
let estrellasCapa1, estrellasCapa2;
let Planetas = [],
  Lunas = [],
  Orbitas = [];
let t0 = 0,
  accglobal = 0.001,
  timestamp = 0;
let gui, params, camFolder, planetSelector;
const textureLoader = new THREE.TextureLoader();
const clock = new THREE.Clock();
const BASE_PARTICLE_SIZE = 0.06;
const prominences = [];
const PROM_COUNT = 6;
const raycaster = new THREE.Raycaster();
const mouse = new THREE.Vector2();
let nebulaMesh, nebulaMaterial;
let followIndex = -1;

init();
animationLoop();

function init() {
  const style = document.createElement("style");
  style.textContent = `
    html, body { margin:0; height:100%; background:#000; }
    body { overflow:hidden; }
    canvas { display:block; }
  `;
  document.head.appendChild(style);

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
  info.innerHTML = "three.js - sistema solar";
  document.body.appendChild(info);

  scene = new THREE.Scene();
  scene.background = new THREE.Color(0x000000);

  camera = new THREE.PerspectiveCamera(
    75,
    window.innerWidth / window.innerHeight,
    0.1,
    2000
  );
  camera.position.set(0, 0, 14);

  renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
  renderer.outputColorSpace = THREE.SRGBColorSpace;
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 1.8;
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setClearColor(0x000000, 1);
  document.body.appendChild(renderer.domElement);

  orbit = new OrbitControls(camera, renderer.domElement);
  orbit.enableDamping = true;

  const luz = new THREE.PointLight(0xffffff, 2, 0);
  luz.position.set(0, 0, 0);
  scene.add(luz);
  scene.add(new THREE.AmbientLight(0x404040, 0.6));

  gui = new GUI();
  params = {
    velocidad: 1.0,
    mostrarOrbitas: true,
    mostrarEstrellas: true,
    offX: 3,
    offY: 2,
    offZ: 3,
    modoCamara: "Libre",
  };
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

  crearSkyboxNebulosa(1200);

  Estrella(1.8, "src/textures/sun/sun.png");
  CoronaParticulas(900, 2.1, 3.0);
  for (let i = 0; i < PROM_COUNT; i++) crearProtuberancia(1.85, 2.8);

  Planeta({
    nombre: "Venus",
    radio: 0.5,
    dist: 4.0,
    vel: 1.0,
    f1: 1.0,
    f2: 1.2,
    inclinacion: 0.2,
    axialTilt: 0.1,
    rotacionOrbital: -0.01,
    spin: -0.0006,
    textures: {
      map: "src/textures/venus/venus.jpg",
      normalMap: "src/textures/venus/venus_normal.png",
      displacementMap: "src/textures/venus/venus_displacement.png",
      displacementScale: 0.02,
      specularMap: "src/textures/venus/venus_specular.png",
      shininess: 10,
    },
  });

  Planeta({
    nombre: "Urano",
    radio: 0.95,
    dist: 17.0,
    vel: 0.07,
    f1: 1.0,
    f2: 1.0,
    inclinacion: 0.52,
    axialTilt: 1.71,
    rotacionOrbital: -0.003,
    spin: 0.002,
    textures: { map: "src/textures/uranus/uranus.jpg" },
  });

  Planeta({
    nombre: "Tierra",
    radio: 0.8,
    dist: 5.8,
    vel: -1.2,
    f1: 1.0,
    f2: 1.0,
    inclinacion: 0.3,
    axialTilt: 0.41,
    rotacionOrbital: -0.008,
    spin: 0.02,
    textures: {
      map: "src/textures/earth/earth.jpg",
      normalMap: "src/textures/earth/earth_normal.png",
      displacementMap: "src/textures/earth/earth_displacement.png",
      displacementScale: 0.1,
      specularMap: "src/textures/earth/earth_specular.png",
      shininess: 20,
      cloudsMap: "src/textures/earth/earthcloudmaptrans.jpg",
      cloudsOpacity: 0.7,
      cloudsSpeed: 0.03,
      cloudsScale: 1.1,
    },
  });

  Planeta({
    nombre: "Mercurio",
    radio: 0.6,
    dist: 3,
    vel: -1,
    f1: 1.0,
    f2: 1.0,
    inclinacion: 0.1,
    axialTilt: 0.01,
    rotacionOrbital: 0.012,
    spin: 0.003,
    textures: {
      map: "src/textures/mercury/mercury.jpg",
      normalMap: "src/textures/mercury/mercury_normal.png",
      displacementMap: "src/textures/mercury/mercury_displacement.png",
      displacementScale: 0.2,
      specularMap: "src/textures/mercury/mercury_specular.png",
      shininess: 20,
    },
  });

  Planeta({
    nombre: "Marte",
    radio: 0.6,
    dist: 8,
    vel: 0.2,
    f1: 1.0,
    f2: 1.0,
    inclinacion: 0.4,
    axialTilt: 0.44,
    rotacionOrbital: 0.006,
    spin: 0.018,
    textures: {
      map: "src/textures/mars/mars.jpg",
      normalMap: "src/textures/mars/mars_normal.png",
      displacementMap: "src/textures/mars/mars_displacement.png",
      displacementScale: 0.01,
    },
  });

  Planeta({
    nombre: "Saturno",
    radio: 1.1,
    dist: 12.5,
    vel: 0.12,
    f1: 1.0,
    f2: 1.0,
    inclinacion: 0.46,
    axialTilt: 0.47,
    rotacionOrbital: -0.004,
    spin: 0.01,
    textures: { map: "src/textures/saturn/saturn.jpg" },
  });

  Anillos(
    Planetas[Planetas.length - 1],
    1.3,
    2.2,
    "src/textures/saturn/saturn_ring.jpg",
    0
  );
  Luna(Planetas[2], 0.22, 1.6, -3.2, 0xbcbcbc, 0.089);
  Luna(Planetas[1], 0.15, 0.75, -3.5, 0xffff00, 0.0);
  Luna(Planetas[1], 0.04, 0.7, 1.5, 0xff0f00, Math.PI / 2);

  estrellasCapa1 = crearCampoEstrellas({
    cantidad: 4500,
    radioInterno: 120,
    radioExterno: 140,
    size: 0.035,
    opacidad: 0.95,
    rotacionLenta: new THREE.Vector3(0, 0.00002, 0),
  });
  estrellasCapa2 = crearCampoEstrellas({
    cantidad: 2500,
    radioInterno: 180,
    radioExterno: 200,
    size: 0.05,
    opacidad: 0.5,
    rotacionLenta: new THREE.Vector3(0.000015, 0, 0),
  });

  const nombres = ["Libre", ...Planetas.map((p) => p.userData.nombre)];
  planetSelector = camFolder
    .add({ modo: "Libre" }, "modo", nombres)
    .name("Modo")
    .onChange((v) => {
      if (v === "Libre") {
        followIndex = -1;
      } else {
        const idx = Planetas.findIndex((p) => p.userData.nombre === v);
        if (idx >= 0) {
          followIndex = idx;
          focusPlanetByIndex(idx);
        }
      }
    });

  t0 = Date.now();
  window.addEventListener("resize", onResize);
  window.addEventListener("click", onClickFocus);
}

function focusPlanetByIndex(idx) {
  const node = Planetas[idx];
  if (!node) return;
  const pos = new THREE.Vector3();
  node.userData.mesh.getWorldPosition(pos);
  const desired = pos
    .clone()
    .add(new THREE.Vector3(params.offX, params.offY, params.offZ));
  camera.position.copy(desired);
  orbit.target.copy(pos);
  orbit.update();
}

function onResize() {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
}

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

function Estrella(rad, texturePath) {
  const tex = textureLoader.load(texturePath);
  tex.colorSpace = THREE.SRGBColorSpace;
  tex.anisotropy =
    (renderer.capabilities.getMaxAnisotropy &&
      renderer.capabilities.getMaxAnisotropy()) ||
    1;

  const coreGeo = new THREE.SphereGeometry(rad, 128, 64);
  const coreMat = new THREE.MeshBasicMaterial({ map: tex, color: 0xffffff });
  sunCore = new THREE.Mesh(coreGeo, coreMat);
  scene.add(sunCore);

  const plasmaGeo = new THREE.SphereGeometry(rad * 1.05, 128, 64);
  const plasmaMat = new THREE.ShaderMaterial({
    uniforms: {
      time: { value: 0 },
      color1: { value: new THREE.Color(0xffaa00) },
      color2: { value: new THREE.Color(0xff4400) },
    },
    vertexShader: `
      varying vec2 vUv;
      void main(){
        vUv = uv;
        gl_Position = projectionMatrix * modelViewMatrix * vec4(position,1.0);
      }
    `,
    fragmentShader: `
      uniform float time;
      uniform vec3 color1;
      uniform vec3 color2;
      varying vec2 vUv;
      void main(){
        float n = sin(vUv.x*20.0+time*2.0)*cos(vUv.y*10.0+time*3.0);
        vec3 col = mix(color1,color2,0.5+0.5*n);
        gl_FragColor = vec4(col,0.4);
      }
    `,
    transparent: true,
    blending: THREE.AdditiveBlending,
    depthWrite: false,
  });
  sunGlowLayer = new THREE.Mesh(plasmaGeo, plasmaMat);
  scene.add(sunGlowLayer);

  const haloTex = generarSpriteCircular();
  const haloMat = new THREE.SpriteMaterial({
    map: haloTex,
    blending: THREE.AdditiveBlending,
    transparent: true,
    opacity: 0.7,
    depthWrite: false,
  });
  haloSprite = new THREE.Sprite(haloMat);
  haloSprite.scale.set(rad * 7.0, rad * 7.0, 1);
  scene.add(haloSprite);

  const count = 600;
  const positions = new Float32Array(count * 3);
  const sizes = new Float32Array(count);
  for (let i = 0; i < count; i++) {
    const angle = Math.random() * Math.PI * 2;
    const dist = rad * (1.8 + Math.random() * 1.5);
    positions[i * 3] = Math.cos(angle) * dist;
    positions[i * 3 + 1] = (Math.random() - 0.5) * rad * 1.5;
    positions[i * 3 + 2] = Math.sin(angle) * dist;
    sizes[i] = 0.1 + Math.random() * 0.15;
  }
  const geo = new THREE.BufferGeometry();
  geo.setAttribute("position", new THREE.BufferAttribute(positions, 3));
  geo.setAttribute("size", new THREE.BufferAttribute(sizes, 1));
  const sprite = generarSpriteCircular();
  const mat = new THREE.PointsMaterial({
    size: 0.12,
    map: sprite,
    blending: THREE.AdditiveBlending,
    transparent: true,
    opacity: 0.8,
    depthWrite: false,
    color: 0xffaa33,
  });
  sunParticles = new THREE.Points(geo, mat);
  scene.add(sunParticles);
}

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

function generarSpriteCircular() {
  const canvas = document.createElement("canvas");
  canvas.width = 128;
  canvas.height = 128;
  const ctx = canvas.getContext("2d");
  const g = ctx.createRadialGradient(64, 64, 10, 64, 64, 64);
  g.addColorStop(0, "rgba(255,220,80,1)");
  g.addColorStop(0.4, "rgba(255,140,0,0.85)");
  g.addColorStop(1, "rgba(255,80,0,0)");
  ctx.fillStyle = g;
  ctx.beginPath();
  ctx.arc(64, 64, 64, 0, Math.PI * 2);
  ctx.fill();
  const tex = new THREE.CanvasTexture(canvas);
  tex.needsUpdate = true;
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}

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

  const pivot = new THREE.Object3D();
  scene.add(pivot);

  const tiltNode = new THREE.Object3D();
  tiltNode.rotation.x = axialTilt;
  pivot.add(tiltNode);

  const geom = new THREE.SphereGeometry(radio, 64, 32);
  const mesh = new THREE.Mesh(geom, material);
  tiltNode.add(mesh);

  let cloudsMesh = null;
  if (texClouds) {
    const cloudsGeom = new THREE.SphereGeometry(radio * cloudsScale, 64, 32);
    const cloudsMat = new THREE.MeshPhongMaterial({
      map: texClouds,
      alphaMap: texClouds,
      transparent: true,
      opacity: cloudsOpacity,
      depthWrite: false,
      side: THREE.DoubleSide,
      shininess: 0,
    });
    cloudsMesh = new THREE.Mesh(cloudsGeom, cloudsMat);
    cloudsMesh.renderOrder = 1;
    tiltNode.add(cloudsMesh);
  }

  tiltNode.userData = {
    pivot,
    mesh,
    cloudsMesh,
    cloudsSpeed,
    dist,
    speed: vel,
    f1,
    f2,
    nombre,
    inclinacion,
    rotacionOrbital,
    spin,
  };
  Planetas.push(tiltNode);

  const curve = new THREE.EllipseCurve(0, 0, dist * f1, dist * f2);
  const points = curve.getPoints(100);
  const geome = new THREE.BufferGeometry().setFromPoints(points);
  const mate = new THREE.LineBasicMaterial({ color: 0xffffff });
  const orbita = new THREE.Line(geome, mate);
  orbita.rotation.x = inclinacion;
  orbita.userData = { rotacionOrbital };
  Orbitas.push(orbita);
  scene.add(orbita);
}

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
    size: size,
    depthWrite: false,
    transparent: true,
    opacity: opacidad,
    sizeAttenuation: true,
  });
  const estrellas = new THREE.Points(geom, mat);
  estrellas.userData.rotacionLenta = rotacionLenta;
  scene.add(estrellas);
  return estrellas;
}

function crearSkyboxNebulosa(size = 1200) {
  const geo = new THREE.BoxGeometry(size, size, size);
  nebulaMaterial = new THREE.ShaderMaterial({
    side: THREE.BackSide,
    uniforms: {
      time: { value: 0 },
      intensity: { value: 1.2 },
      gain: { value: 0.55 },
      lacunarity: { value: 2.1 },
      scale: { value: 1.6 },
      tint1: { value: new THREE.Color(0x2c2e66) },
      tint2: { value: new THREE.Color(0x712b75) },
      stars: { value: 0.85 },
    },
    vertexShader: `
      varying vec3 vWorldPos;
      void main() {
        vec4 wp = modelMatrix * vec4(position, 1.0);
        vWorldPos = wp.xyz;
        gl_Position = projectionMatrix * viewMatrix * wp;
      }
    `,
    fragmentShader: `
      precision highp float;
      varying vec3 vWorldPos;
      uniform float time;
      uniform float intensity;
      uniform float gain;
      uniform float lacunarity;
      uniform float scale;
      uniform vec3 tint1;
      uniform vec3 tint2;
      uniform float stars;

      float hash(vec3 p){
        p = fract(p * 0.3183099 + vec3(0.1,0.2,0.3));
        p *= 17.0;
        return fract(p.x * p.y * p.z * (p.x + p.y + p.z));
      }

      float noise(vec3 p){
        vec3 i = floor(p);
        vec3 f = fract(p);
        f = f*f*(3.0-2.0*f);
        float n000 = hash(i + vec3(0,0,0));
        float n100 = hash(i + vec3(1,0,0));
        float n010 = hash(i + vec3(0,1,0));
        float n110 = hash(i + vec3(1,1,0));
        float n001 = hash(i + vec3(0,0,1));
        float n101 = hash(i + vec3(1,0,1));
        float n011 = hash(i + vec3(0,1,1));
        float n111 = hash(i + vec3(1,1,1));
        float n00 = mix(n000, n100, f.x);
        float n01 = mix(n001, n101, f.x);
        float n10 = mix(n010, n110, f.x);
        float n11 = mix(n011, n111, f.x);
        float n0 = mix(n00, n10, f.y);
        float n1 = mix(n01, n11, f.y);
        return mix(n0, n1, f.z);
      }

      float fbm(vec3 p){
        float v = 0.0;
        float a = 0.5;
        vec3 pp = p;
        for(int i=0; i<6; i++){
          v += a * noise(pp);
          pp *= lacunarity;
          a *= gain;
        }
        return v;
      }

      mat3 rot3(float a, float b, float c){
        float ca = cos(a), sa = sin(a);
        float cb = cos(b), sb = sin(b);
        float cc = cos(c), sc = sin(c);
        return mat3(
          cb*cc, -cb*sc, sb,
          sa*sb*cc+ca*sc, -sa*sb*sc+ca*cc, -sa*cb,
          -ca*sb*cc+sa*sc, ca*sb*sc+sa*cc, ca*cb
        );
      }

      void main() {
        vec3 dir = normalize(vWorldPos);
        vec3 p = rot3(time*0.02, time*0.015, time*0.01) * dir * scale;
        float n1 = fbm(p + vec3(0.0, 3.7, 1.3));
        float n2 = fbm(p * 1.9 + vec3(2.3, -1.7, 0.5));
        float neb = smoothstep(0.25, 0.9, n1*0.7 + n2*0.6);
        vec3 nebula = mix(tint1, tint2, neb) * intensity;
        float st = pow(max(noise(dir*150.0 + time*0.2), 0.0), 24.0) * stars;
        float sparkle = step(0.995, noise(dir*240.0 + time*0.7)) * 0.9;
        vec3 col = nebula + vec3(st + sparkle);
        col = col * (0.7 + 0.3 * dot(dir, vec3(0.0,1.0,0.0)));
        gl_FragColor = vec4(col, 1.0);
      }
    `,
  });
  nebulaMesh = new THREE.Mesh(geo, nebulaMaterial);
  scene.add(nebulaMesh);
}

function animationLoop() {
  const delta = clock.getDelta();
  timestamp = (Date.now() - t0) * accglobal * (params ? params.velocidad : 1.0);
  requestAnimationFrame(animationLoop);

  if (nebulaMaterial) nebulaMaterial.uniforms.time.value += 0.016;
  if (sunGlowLayer && sunGlowLayer.material.uniforms)
    sunGlowLayer.material.uniforms.time.value += 0.02;
  if (haloSprite && haloSprite.material) haloSprite.material.rotation += 0.0002;
  if (sunParticles) sunParticles.rotation.y += 0.0008;

  for (let i = 0; i < Planetas.length; i++) {
    const node = Planetas[i];
    const ud = node.userData;
    const angle = timestamp * ud.speed;
    const x = Math.cos(angle) * ud.f1 * ud.dist;
    const y = Math.sin(angle) * ud.f2 * ud.dist;
    const cy = Math.cos(ud.inclinacion);
    const sy = Math.sin(ud.inclinacion);
    const y3 = y * cy;
    const z3 = y * sy;
    ud.pivot.position.set(x, y3, z3);
    ud.mesh.rotation.y += ud.spin;
    if (ud.cloudsMesh) ud.cloudsMesh.rotation.y += ud.cloudsSpeed;
    const rot = Orbitas[i];
    if (rot) rot.rotation.z += ud.rotacionOrbital * 0.001;
  }

  for (const object of Lunas) {
    object.position.x =
      Math.cos(timestamp * object.userData.speed) * object.userData.dist;
    object.position.y =
      Math.sin(timestamp * object.userData.speed) * object.userData.dist;
  }

  if (followIndex >= 0 && Planetas[followIndex]) {
    const pos = new THREE.Vector3();
    Planetas[followIndex].userData.mesh.getWorldPosition(pos);
    const desired = pos
      .clone()
      .add(new THREE.Vector3(params.offX, params.offY, params.offZ));
    camera.position.lerp(desired, 0.12);
    orbit.target.lerp(pos, 0.12);
  }

  orbit.update();

  if (estrellasCapa1) {
    estrellasCapa1.rotation.x += estrellasCapa1.userData.rotacionLenta.x;
    estrellasCapa1.rotation.y += estrellasCapa1.userData.rotacionLenta.y;
    estrellasCapa1.rotation.z += estrellasCapa1.userData.rotacionLenta.z;
  }
  if (estrellasCapa2) {
    estrellasCapa2.rotation.x += estrellasCapa2.userData.rotacionLenta.x;
    estrellasCapa2.rotation.y += estrellasCapa2.userData.rotacionLenta.y;
    estrellasCapa2.rotation.z += estrellasCapa2.userData.rotacionLenta.z;
  }

  renderer.render(scene, camera);
}
