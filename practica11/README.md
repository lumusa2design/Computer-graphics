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
## Prácticas 11: Física y  Animación

La tarea consiste en proponer un prototipo en *three.js* que integre o bien la librería *tween.js* y/o *ammo.js*. La entrega final que he hecho ha sido en *tween* y *ammo*.

El código desarrollado de la práctica ha sido:

```js
import * as THREE from "three";
import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import Ammo from "ammojs-typed";
import * as TWEEN from "@tweenjs/tween.js";

let camera, controls, scene, renderer;
let textureLoader;
const clock = new THREE.Clock();

const mouseCoords = new THREE.Vector2();
const raycaster = new THREE.Raycaster();

let physicsWorld;
const gravityConstant = 7.8;
let collisionConfiguration;
let dispatcher;
let broadphase;
let solver;
const margin = 0.05;
let listener, backgroundSound;

const MATERIAL_TYPES = {
  wood: {
    densityScale: 0.8,
    friction: 0.6,
    restitution: 0.3,
  },
  stone: {
    densityScale: 1.4,
    friction: 0.9,
    restitution: 0.1,
  },
  glass: {
    densityScale: 0.5,
    friction: 0.4,
    restitution: 0.05,
  },
  metal: {
    densityScale: 1.8,
    friction: 0.4,
    restitution: 0.2,
  },
};

const rigidBodies = [];
const trailParticles = [];
const explosionParticles = [];
const levelObjects = [];

const pos = new THREE.Vector3();
const quat = new THREE.Quaternion();
let transformAux1;
let tempBtVec3_1;

let score = 0;
let scoreElement = null;
let birdIndicatorElement = null;
let birdsLeftElement = null;
let levelElement = null;
let pigsElement = null;
let comboElement = null;
let abilityElement = null;
let currentBirdType = 1;

let hudBird = null;

let isCharging = false;
let chargeStart = 0;
const aimOrigin = new THREE.Vector3();
const aimDirection = new THREE.Vector3();
let aimPreviewLine = null;
let aimImpactMarker = null;

const TRAIL_LIFE = 0.8;
const TRAIL_MIN_DIST = 0.4;

let pigsAlive = 0;
let pendingLoseCheck = false;

let endOverlay = null;
let overlayText = null;
let overlayNextButton = null;
let overlayReplayButton = null;

let shakeTime = 0;
let shakeIntensity = 0;
const maxShakeTime = 0.4;
let timeScale = 1;
let shakeBasePosition = null;

let gameState = {
  levelIndex: 0,
  birdsLeft: 0,
  state: "playing",
};

let worldTime = 0;
let comboMultiplier = 1;
let lastPigKillTime = 0;
const basePigScore = 100;
const birdBonusPerRemaining = 50;

let activeBird = null;
let activeBirdBody = null;

let followBird = null;

let replayData = null;
let isReplaying = false;
let replayTween = null;

const slingshotAnchor = new THREE.Vector3(-8, 3, 0);
let slingshotGroup = null;
let slingshotBand = null;
let slingshotForkLeft = null;
let slingshotForkRight = null;
const aimPlane = new THREE.Plane(
  new THREE.Vector3(0, 1, 0),
  -slingshotAnchor.y
);

const cameraBasePosition = new THREE.Vector3();
const cameraBaseTarget = new THREE.Vector3();

const levels = [
  {
    birds: 10,
    build: () => {
      createAngryStructures();
    },
  },
];

Ammo(Ammo).then(start);

function start() {
  initGraphics();
  initPhysics();
  createObjects();
  initInput();
  animationLoop();
}

function initGraphics() {
  camera = new THREE.PerspectiveCamera(
    60,
    window.innerWidth / window.innerHeight,
    0.2,
    2000
  );

  listener = new THREE.AudioListener();
  camera.add(listener);

  backgroundSound = new THREE.Audio(listener);
  const audioLoader = new THREE.AudioLoader();
  audioLoader.load("src/sound/angry-birds-videojuegos-.mp3", function (buffer) {
    backgroundSound.setBuffer(buffer);
    backgroundSound.setLoop(true);
    backgroundSound.setVolume(0.4);
    backgroundSound.play();
  });

  scene = new THREE.Scene();
  scene.background = new THREE.Color(0x87b5ff);
  scene.fog = new THREE.Fog(0x87b5ff, 40, 140);
  scene.add(camera);

  renderer = new THREE.WebGLRenderer();
  renderer.setPixelRatio(window.devicePixelRatio);
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.shadowMap.enabled = true;
  renderer.outputEncoding = THREE.sRGBEncoding;
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 1.0;
  document.body.appendChild(renderer.domElement);

  controls = new OrbitControls(camera, renderer.domElement);
  controls.enablePan = false;
  controls.enableDamping = true;
  controls.dampingFactor = 0.08;
  controls.rotateSpeed = 0.9;
  controls.zoomSpeed = 1.0;
  controls.minDistance = 10;
  controls.maxDistance = 60;

  textureLoader = new THREE.TextureLoader();

  const hemi = new THREE.HemisphereLight(0xcceeff, 0x556677, 0.6);
  scene.add(hemi);

  const ambientLight = new THREE.AmbientLight(0x606060);
  scene.add(ambientLight);

  const light = new THREE.DirectionalLight(0xffffff, 1.1);
  light.position.set(-10, 24, 12);
  light.castShadow = true;
  const d = 24;
  light.shadow.camera.left = -d;
  light.shadow.camera.right = d;
  light.shadow.camera.top = d;
  light.shadow.camera.bottom = -d;
  light.shadow.camera.near = 2;
  light.shadow.camera.far = 80;
  light.shadow.mapSize.x = 2048;
  light.shadow.mapSize.y = 2048;
  scene.add(light);

  createSlingshot();

  cameraBasePosition.set(-14, 7, 14);
  cameraBaseTarget.set(12, 5, 0);
  camera.position.copy(cameraBasePosition);
  controls.target.copy(cameraBaseTarget);
  controls.update();

  window.addEventListener("resize", onWindowResize);

  scoreElement = document.createElement("div");
  scoreElement.style.position = "fixed";
  scoreElement.style.top = "10px";
  scoreElement.style.left = "10px";
  scoreElement.style.padding = "8px 12px";
  scoreElement.style.background = "rgba(0,0,0,0.5)";
  scoreElement.style.color = "#fff";
  scoreElement.style.fontFamily = "sans-serif";
  scoreElement.style.fontSize = "14px";
  scoreElement.style.zIndex = "100";
  document.body.appendChild(scoreElement);
  updateScore();

  birdIndicatorElement = document.createElement("div");
  birdIndicatorElement.style.position = "fixed";
  birdIndicatorElement.style.top = "40px";
  birdIndicatorElement.style.left = "10px";
  birdIndicatorElement.style.padding = "6px 10px";
  birdIndicatorElement.style.background = "rgba(0,0,0,0.5)";
  birdIndicatorElement.style.color = "#fff";
  birdIndicatorElement.style.fontFamily = "sans-serif";
  birdIndicatorElement.style.fontSize = "13px";
  birdIndicatorElement.style.zIndex = "100";
  document.body.appendChild(birdIndicatorElement);
  updateBirdIndicator();

  birdsLeftElement = document.createElement("div");
  birdsLeftElement.style.position = "fixed";
  birdsLeftElement.style.top = "70px";
  birdsLeftElement.style.left = "10px";
  birdsLeftElement.style.padding = "6px 10px";
  birdsLeftElement.style.background = "rgba(0,0,0,0.5)";
  birdsLeftElement.style.color = "#fff";
  birdsLeftElement.style.fontFamily = "sans-serif";
  birdsLeftElement.style.fontSize = "13px";
  birdsLeftElement.style.zIndex = "100";
  document.body.appendChild(birdsLeftElement);

  levelElement = document.createElement("div");
  levelElement.style.position = "fixed";
  levelElement.style.top = "100px";
  levelElement.style.left = "10px";
  levelElement.style.padding = "6px 10px";
  levelElement.style.background = "rgba(0,0,0,0.5)";
  levelElement.style.color = "#fff";
  levelElement.style.fontFamily = "sans-serif";
  levelElement.style.fontSize = "13px";
  levelElement.style.zIndex = "100";
  document.body.appendChild(levelElement);

  pigsElement = document.createElement("div");
  pigsElement.style.position = "fixed";
  pigsElement.style.top = "130px";
  pigsElement.style.left = "10px";
  pigsElement.style.padding = "6px 10px";
  pigsElement.style.background = "rgba(0,0,0,0.5)";
  pigsElement.style.color = "#fff";
  pigsElement.style.fontFamily = "sans-serif";
  pigsElement.style.fontSize = "13px";
  pigsElement.style.zIndex = "100";
  document.body.appendChild(pigsElement);
  updatePigsUI();

  comboElement = document.createElement("div");
  comboElement.style.position = "fixed";
  comboElement.style.top = "160px";
  comboElement.style.left = "10px";
  comboElement.style.padding = "6px 10px";
  comboElement.style.background = "rgba(0,0,0,0.5)";
  comboElement.style.color = "#ffdd33";
  comboElement.style.fontFamily = "sans-serif";
  comboElement.style.fontSize = "13px";
  comboElement.style.zIndex = "100";
  comboElement.style.display = "none";
  document.body.appendChild(comboElement);

  abilityElement = document.createElement("div");
  abilityElement.style.position = "fixed";
  abilityElement.style.top = "190px";
  abilityElement.style.left = "10px";
  abilityElement.style.padding = "6px 10px";
  abilityElement.style.background = "rgba(0,0,0,0.5)";
  abilityElement.style.color = "#66ffcc";
  abilityElement.style.fontFamily = "sans-serif";
  abilityElement.style.fontSize = "13px";
  abilityElement.style.zIndex = "100";
  document.body.appendChild(abilityElement);
  updateAbilityHUD("Sin habilidad activa");

  endOverlay = document.createElement("div");
  endOverlay.style.position = "fixed";
  endOverlay.style.top = "0";
  endOverlay.style.left = "0";
  endOverlay.style.width = "100%";
  endOverlay.style.height = "100%";
  endOverlay.style.display = "none";
  endOverlay.style.alignItems = "center";
  endOverlay.style.justifyContent = "center";
  endOverlay.style.background = "rgba(0,0,0,0.5)";
  endOverlay.style.zIndex = "200";

  const panel = document.createElement("div");
  panel.style.background = "#222";
  panel.style.color = "#fff";
  panel.style.padding = "20px 30px";
  panel.style.borderRadius = "8px";
  panel.style.fontFamily = "sans-serif";
  panel.style.textAlign = "center";
  panel.style.minWidth = "260px";

  overlayText = document.createElement("div");
  overlayText.style.marginBottom = "15px";
  overlayText.style.fontSize = "18px";
  panel.appendChild(overlayText);

  const buttonsRow = document.createElement("div");
  buttonsRow.style.display = "flex";
  buttonsRow.style.justifyContent = "center";
  buttonsRow.style.gap = "10px";

  const retryButton = document.createElement("button");
  retryButton.textContent = "Reintentar nivel";
  retryButton.style.padding = "6px 10px";
  retryButton.style.cursor = "pointer";
  retryButton.onclick = () => {
    resetLevel();
  };

  overlayNextButton = document.createElement("button");
  overlayNextButton.textContent = "Siguiente nivel";
  overlayNextButton.style.padding = "6px 10px";
  overlayNextButton.style.cursor = "pointer";
  overlayNextButton.onclick = () => {
    nextLevel();
  };

  overlayReplayButton = document.createElement("button");
  overlayReplayButton.textContent = "Replay disparo";
  overlayReplayButton.style.padding = "6px 10px";
  overlayReplayButton.style.cursor = "pointer";
  overlayReplayButton.onclick = () => {
    playReplay();
  };

  buttonsRow.appendChild(retryButton);
  buttonsRow.appendChild(overlayNextButton);
  buttonsRow.appendChild(overlayReplayButton);
  panel.appendChild(buttonsRow);
  endOverlay.appendChild(panel);
  document.body.appendChild(endOverlay);

  updateHudBird();
  updateBirdsLeftUI();
  updateLevelUI();
}

function resetCameraToBase(immediate) {
  if (!camera || !controls) return;
  if (immediate) {
    camera.position.copy(cameraBasePosition);
    controls.target.copy(cameraBaseTarget);
    controls.update();
  } else {
    camera.position.lerp(cameraBasePosition, 0.2);
    controls.target.lerp(cameraBaseTarget, 0.2);
    controls.update();
  }
}

function createSlingshot() {
  slingshotGroup = new THREE.Group();
  slingshotGroup.position.copy(slingshotAnchor);

  const baseGeom = new THREE.BoxGeometry(1.2, 0.3, 1.2);
  const baseMat = new THREE.MeshStandardMaterial({
    color: 0x8b5a2b,
    roughness: 0.8,
    metalness: 0.05,
  });
  const base = new THREE.Mesh(baseGeom, baseMat);
  base.position.set(0, -0.15, 0);
  base.castShadow = true;
  base.receiveShadow = true;
  slingshotGroup.add(base);

  const pillarGeom = new THREE.CylinderGeometry(0.2, 0.25, 2, 12);
  const pillarMat = new THREE.MeshStandardMaterial({
    color: 0x8b5a2b,
    roughness: 0.8,
    metalness: 0.05,
  });

  const left = new THREE.Mesh(pillarGeom, pillarMat);
  left.position.set(-0.4, 1, 0);
  left.castShadow = true;
  left.receiveShadow = true;
  slingshotGroup.add(left);

  const right = new THREE.Mesh(pillarGeom, pillarMat);
  right.position.set(0.4, 1, 0);
  right.castShadow = true;
  right.receiveShadow = true;
  slingshotGroup.add(right);

  slingshotForkLeft = new THREE.Vector3(-0.4, 2, 0);
  slingshotForkRight = new THREE.Vector3(0.4, 2, 0);

  const bandGeom = new THREE.BufferGeometry();
  const bandPositions = new Float32Array(3 * 3);
  bandGeom.setAttribute(
    "position",
    new THREE.BufferAttribute(bandPositions, 3)
  );
  const bandMat = new THREE.LineBasicMaterial({ color: 0x442200 });
  slingshotBand = new THREE.Line(bandGeom, bandMat);
  slingshotGroup.add(slingshotBand);

  scene.add(slingshotGroup);
  updateSlingshotBand(false);
}

function updateSlingshotBand(stretched) {
  if (!slingshotBand) return;
  const positions = slingshotBand.geometry.attributes.position.array;
  const leftWorld = slingshotForkLeft
    .clone()
    .applyMatrix4(slingshotGroup.matrixWorld);
  const rightWorld = slingshotForkRight
    .clone()
    .applyMatrix4(slingshotGroup.matrixWorld);
  let pocket;
  if (stretched && isCharging) {
    const charge = Math.min(getChargeFactor(), 2.0);
    const stretchLen = 1 + charge * 1.5;
    pocket = slingshotAnchor
      .clone()
      .sub(aimDirection.clone().multiplyScalar(stretchLen));
  } else {
    pocket = leftWorld.clone().add(rightWorld).multiplyScalar(0.5);
  }
  positions[0] = leftWorld.x;
  positions[1] = leftWorld.y;
  positions[2] = leftWorld.z;
  positions[3] = pocket.x;
  positions[4] = pocket.y;
  positions[5] = pocket.z;
  positions[6] = rightWorld.x;
  positions[7] = rightWorld.y;
  positions[8] = rightWorld.z;
  slingshotBand.geometry.attributes.position.needsUpdate = true;
}

function updateScore() {
  if (scoreElement) {
    scoreElement.textContent = "Puntos: " + score;
  }
}

function updateComboHUD() {
  if (!comboElement) return;
  if (comboMultiplier > 1) {
    comboElement.style.display = "block";
    comboElement.textContent = "Combo x" + comboMultiplier;
  } else {
    comboElement.style.display = "none";
  }
}

function updateAbilityHUD(text) {
  if (!abilityElement) return;
  abilityElement.textContent = text;
}

function getBirdLabel(type) {
  if (type === 2) return "2: Rápido rebotador";
  if (type === 3) return "3: Explosivo";
  if (type === 4) return "4: Doble";
  if (type === 5) return "5: Boomerang";
  return "1: Básico";
}

function updateBirdIndicator() {
  if (!birdIndicatorElement) return;
  const label = getBirdLabel(currentBirdType);
  birdIndicatorElement.textContent = "Pájaro actual: " + label;
}

function updateBirdsLeftUI() {
  if (!birdsLeftElement) return;
  birdsLeftElement.textContent = "Pájaros restantes: " + gameState.birdsLeft;
}

function updateLevelUI() {
  if (!levelElement) return;
  levelElement.textContent = "Nivel: " + (gameState.levelIndex + 1);
}

function updatePigsUI() {
  if (!pigsElement) return;
  pigsElement.textContent = "Cerdos vivos: " + pigsAlive;
}

function updateHudBird() {
  if (hudBird) {
    camera.remove(hudBird);
    hudBird = null;
  }
  const params = getBirdParams(currentBirdType);
  hudBird = createBirdMesh(params.radius * 0.6, params.color, params.type);
  hudBird.traverse((o) => {
    if (o.isMesh) {
      o.castShadow = false;
      o.receiveShadow = false;
    }
  });
  hudBird.position.set(-3, -2, -8);
  camera.add(hudBird);
}

function showEndOverlay(type, birdBonus) {
  if (!endOverlay || !overlayText) return;
  let extra = "";
  if (birdBonus && birdBonus > 0) {
    extra = "  Bonus por pájaros: +" + birdBonus;
  }
  if (type === "win") {
    overlayText.textContent = "Nivel completado. " + extra;
  } else {
    overlayText.textContent = "Has perdido.";
  }
  endOverlay.style.display = "flex";
}

function hideEndOverlay() {
  if (!endOverlay) return;
  endOverlay.style.display = "none";
}

function initPhysics() {
  collisionConfiguration = new Ammo.btDefaultCollisionConfiguration();
  dispatcher = new Ammo.btCollisionDispatcher(collisionConfiguration);
  broadphase = new Ammo.btDbvtBroadphase();
  solver = new Ammo.btSequentialImpulseConstraintSolver();
  physicsWorld = new Ammo.btDiscreteDynamicsWorld(
    dispatcher,
    broadphase,
    solver,
    collisionConfiguration
  );
  physicsWorld.setGravity(new Ammo.btVector3(0, -gravityConstant, 0));

  transformAux1 = new Ammo.btTransform();
  tempBtVec3_1 = new Ammo.btVector3(0, 0, 0);
}

function createObjects() {
  createGround();
  loadLevel(0);
}

function createGround() {
  pos.set(0, -0.5, 0);
  quat.set(0, 0, 0, 1);
  const suelo = createBoxWithPhysics(
    60,
    1,
    60,
    0,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xffffff,
      roughness: 0.9,
      metalness: 0.0,
    }),
    false,
    null
  );
  suelo.userData.isGround = true;
  suelo.receiveShadow = true;
  textureLoader.load(
    "https://cdn.glitch.global/8b114fdc-500a-4e05-b3c5-a4afa5246b07/grid.png?v=1669716810074",
    function (texture) {
      texture.wrapS = THREE.RepeatWrapping;
      texture.wrapT = THREE.RepeatWrapping;
      texture.repeat.set(60, 60);
      texture.encoding = THREE.SRGBEncoding;
      suelo.material.map = texture;
      suelo.material.roughness = 0.85;
      suelo.material.metalness = 0.0;
      suelo.material.needsUpdate = true;
    }
  );
}

function clearLevel() {
  for (let i = 0; i < levelObjects.length; i++) {
    const obj = levelObjects[i];
    const body = obj.userData.physicsBody;
    if (body) {
      physicsWorld.removeRigidBody(body);
    }
    scene.remove(obj);
  }
  const toRemove = new Set(levelObjects);
  for (let i = rigidBodies.length - 1; i >= 0; i--) {
    const obj = rigidBodies[i];
    if (toRemove.has(obj) || obj.userData.isBird) {
      const body = obj.userData.physicsBody;
      if (body) {
        physicsWorld.removeRigidBody(body);
      }
      scene.remove(obj);
      rigidBodies.splice(i, 1);
    }
  }
  for (let i = trailParticles.length - 1; i >= 0; i--) {
    scene.remove(trailParticles[i].mesh);
  }
  for (let i = explosionParticles.length - 1; i >= 0; i--) {
    scene.remove(explosionParticles[i].mesh);
  }
  trailParticles.length = 0;
  explosionParticles.length = 0;
  levelObjects.length = 0;
  pigsAlive = 0;
  pendingLoseCheck = false;
  activeBird = null;
  activeBirdBody = null;
  followBird = null;
  replayData = null;
  comboMultiplier = 1;
  updateComboHUD();
  updateAbilityHUD("Sin habilidad activa");
  updatePigsUI();
}

function loadLevel(index) {
  gameState.levelIndex = index;
  gameState.birdsLeft = levels[index].birds;
  gameState.state = "playing";
  pigsAlive = 0;
  pendingLoseCheck = false;
  levelObjects.length = 0;
  worldTime = 0;
  updateBirdsLeftUI();
  updateLevelUI();
  updatePigsUI();
  levels[index].build();
  resetCameraToBase(true);
}

function resetLevel() {
  clearLevel();
  score = 0;
  updateScore();
  loadLevel(gameState.levelIndex);
  hideEndOverlay();
}

function nextLevel() {
  let nextIndex = gameState.levelIndex + 1;
  if (nextIndex >= levels.length) {
    nextIndex = 0;
  }
  clearLevel();
  score = 0;
  updateScore();
  loadLevel(nextIndex);
  hideEndOverlay();
}

function createBoxWithPhysics(
  sx,
  sy,
  sz,
  mass,
  pos,
  quat,
  material,
  track = true,
  materialType = null
) {
  const object = new THREE.Mesh(
    new THREE.BoxGeometry(sx, sy, sz, 1, 1, 1),
    material
  );
  const shape = new Ammo.btBoxShape(
    new Ammo.btVector3(sx * 0.5, sy * 0.5, sz * 0.5)
  );
  shape.setMargin(margin);
  let finalMass = mass;
  let matDef = null;
  if (materialType && mass > 0) {
    matDef = MATERIAL_TYPES[materialType] || null;
    if (matDef && typeof matDef.densityScale === "number") {
      finalMass = mass * matDef.densityScale;
    }
  }
  object.userData.materialType = materialType || null;
  if (object.material && object.material.isMeshStandardMaterial) {
    if (materialType === "wood") {
      object.material.roughness = 0.85;
      object.material.metalness = 0.05;
    } else if (materialType === "stone") {
      object.material.roughness = 0.95;
      object.material.metalness = 0.0;
    } else if (materialType === "metal") {
      object.material.roughness = 0.35;
      object.material.metalness = 0.9;
    }
  }
  const body = createRigidBody(object, shape, finalMass, pos, quat);
  if (matDef) {
    if (typeof matDef.friction === "number") body.setFriction(matDef.friction);
    if (typeof matDef.restitution === "number")
      body.setRestitution(matDef.restitution);
  }
  if (track) {
    levelObjects.push(object);
  }
  return object;
}

function createSphereWithPhysics(
  radius,
  mass,
  pos,
  quat,
  material,
  isPig = false,
  track = true
) {
  const object = new THREE.Mesh(
    new THREE.SphereGeometry(radius, 16, 12),
    material
  );
  if (isPig) {
    object.userData.isPig = true;
    object.userData.pigPhase = Math.random() * Math.PI * 2;
    pigsAlive++;
    updatePigsUI();
  }
  const shape = new Ammo.btSphereShape(radius);
  shape.setMargin(margin);
  createRigidBody(object, shape, mass, pos, quat);
  if (track) {
    levelObjects.push(object);
  }
  return object;
}

function createAngryStructures() {
  const baseX = 10;
  const baseZStart = -10;
  const spacingZ = 10;
  for (let s = 0; s < 3; s++) {
    const x = baseX + s * 3;
    const z = baseZStart + s * spacingZ;
    buildAngryStructure(x, z);
  }
  buildTowerStructure(20, -8);
  buildTowerStructure(20, 8);
  buildBridgeStructure(28, 0);
}

function buildAngryStructure(x, z) {
  const platformWidth = 7;
  const platformHeight = 0.6;
  const platformDepth = 3.5;

  const columnWidth = 0.7;
  const columnDepth = 0.7;
  const columnHeight = 2.4;

  const beamWidth = 5;
  const beamHeight = 0.45;
  const beamDepth = 0.7;

  const pigRadius = 0.5;

  quat.set(0, 0, 0, 1);

  pos.set(x, platformHeight * 0.5, z);
  const basePlatform = createBoxWithPhysics(
    platformWidth,
    platformHeight,
    platformDepth,
    0,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xbba58a,
      roughness: 0.85,
      metalness: 0.05,
    }),
    true,
    "wood"
  );
  basePlatform.castShadow = true;
  basePlatform.receiveShadow = true;

  const colOffsetX = 2.2;
  const colOffsetZ = 1.3;

  pos.set(x - colOffsetX, platformHeight + columnHeight * 0.5, z - colOffsetZ);
  const col1 = createBoxWithPhysics(
    columnWidth,
    columnHeight,
    columnDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xaaaaaa,
      roughness: 0.95,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  col1.castShadow = true;
  col1.receiveShadow = true;

  pos.set(x - colOffsetX, platformHeight + columnHeight * 0.5, z + colOffsetZ);
  const col2 = createBoxWithPhysics(
    columnWidth,
    columnHeight,
    columnDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xaaaaaa,
      roughness: 0.95,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  col2.castShadow = true;
  col2.receiveShadow = true;

  pos.set(x + colOffsetX, platformHeight + columnHeight * 0.5, z - colOffsetZ);
  const col3 = createBoxWithPhysics(
    columnWidth,
    columnHeight,
    columnDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xaaaaaa,
      roughness: 0.95,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  col3.castShadow = true;
  col3.receiveShadow = true;

  pos.set(x + colOffsetX, platformHeight + columnHeight * 0.5, z + colOffsetZ);
  const col4 = createBoxWithPhysics(
    columnWidth,
    columnHeight,
    columnDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xaaaaaa,
      roughness: 0.95,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  col4.castShadow = true;
  col4.receiveShadow = true;

  pos.set(x, platformHeight + columnHeight + beamHeight * 0.5, z - colOffsetZ);
  const beamFront = createBoxWithPhysics(
    beamWidth,
    beamHeight,
    beamDepth,
    3,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x777777,
      roughness: 0.9,
      metalness: 0.1,
    }),
    true,
    "stone"
  );
  beamFront.castShadow = true;
  beamFront.receiveShadow = true;

  pos.set(x, platformHeight + columnHeight + beamHeight * 0.5, z + colOffsetZ);
  const beamBack = createBoxWithPhysics(
    beamWidth,
    beamHeight,
    beamDepth,
    3,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x777777,
      roughness: 0.9,
      metalness: 0.1,
    }),
    true,
    "stone"
  );
  beamBack.castShadow = true;
  beamBack.receiveShadow = true;

  const upperColumnHeight = 1.8;

  pos.set(
    x - colOffsetX * 0.6,
    platformHeight + columnHeight + beamHeight + upperColumnHeight * 0.5,
    z
  );
  const upperCol1 = createBoxWithPhysics(
    columnWidth,
    upperColumnHeight,
    columnDepth,
    3,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x999999,
      roughness: 0.95,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  upperCol1.castShadow = true;
  upperCol1.receiveShadow = true;

  pos.set(
    x + colOffsetX * 0.6,
    platformHeight + columnHeight + beamHeight + upperColumnHeight * 0.5,
    z
  );
  const upperCol2 = createBoxWithPhysics(
    columnWidth,
    upperColumnHeight,
    columnDepth,
    3,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x999999,
      roughness: 0.95,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  upperCol2.castShadow = true;
  upperCol2.receiveShadow = true;

  pos.set(
    x,
    platformHeight +
      columnHeight +
      beamHeight +
      upperColumnHeight +
      beamHeight * 0.5,
    z
  );
  const topBeam = createBoxWithPhysics(
    beamWidth * 0.8,
    beamHeight,
    beamDepth,
    2.5,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x555555,
      roughness: 0.4,
      metalness: 0.85,
    }),
    true,
    "metal"
  );
  topBeam.castShadow = true;
  topBeam.receiveShadow = true;

  pos.set(
    x,
    platformHeight +
      columnHeight +
      beamHeight +
      upperColumnHeight +
      beamHeight +
      pigRadius +
      0.05,
    z
  );
  const pigTop = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  pigTop.castShadow = true;
  pigTop.receiveShadow = true;

  pos.set(x - 2.4, platformHeight + pigRadius + 0.1, z - 0.8);
  const pigLeft = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  pigLeft.castShadow = true;
  pigLeft.receiveShadow = true;

  pos.set(x + 2.4, platformHeight + pigRadius + 0.1, z + 0.8);
  const pigRight = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  pigRight.castShadow = true;
  pigRight.receiveShadow = true;
}

function buildTowerStructure(x, z) {
  const platformWidth = 6;
  const platformHeight = 0.6;
  const platformDepth = 3.2;

  const columnWidth = 0.7;
  const columnDepth = 0.7;
  const columnHeight = 3.2;

  const beamWidth = 5.2;
  const beamHeight = 0.45;
  const beamDepth = 0.7;

  const pigRadius = 0.5;

  quat.set(0, 0, 0, 1);

  let baseY = 0;

  for (let level = 0; level < 2; level++) {
    const yPlatform = baseY + platformHeight * 0.5;
    pos.set(x, yPlatform, z);
    const platform = createBoxWithPhysics(
      platformWidth,
      platformHeight,
      platformDepth,
      level === 0 ? 0 : 3,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0xbba58a,
        roughness: 0.85,
        metalness: 0.05,
      }),
      true,
      "wood"
    );
    platform.castShadow = true;
    platform.receiveShadow = true;

    const colOffsetX = 2.2;
    const colOffsetZ = 1.2;
    const yCols = yPlatform + platformHeight * 0.5 + columnHeight * 0.5;

    pos.set(x - colOffsetX, yCols, z - colOffsetZ);
    const c1 = createBoxWithPhysics(
      columnWidth,
      columnHeight,
      columnDepth,
      5,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x888888,
        roughness: 0.95,
        metalness: 0.0,
      }),
      true,
      "stone"
    );
    c1.castShadow = true;
    c1.receiveShadow = true;

    pos.set(x - colOffsetX, yCols, z + colOffsetZ);
    const c2 = createBoxWithPhysics(
      columnWidth,
      columnHeight,
      columnDepth,
      5,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x888888,
        roughness: 0.95,
        metalness: 0.0,
      }),
      true,
      "stone"
    );
    c2.castShadow = true;
    c2.receiveShadow = true;

    pos.set(x + colOffsetX, yCols, z - colOffsetZ);
    const c3 = createBoxWithPhysics(
      columnWidth,
      columnHeight,
      columnDepth,
      5,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x888888,
        roughness: 0.95,
        metalness: 0.0,
      }),
      true,
      "stone"
    );
    c3.castShadow = true;
    c3.receiveShadow = true;

    pos.set(x + colOffsetX, yCols, z + colOffsetZ);
    const c4 = createBoxWithPhysics(
      columnWidth,
      columnHeight,
      columnDepth,
      5,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x888888,
        roughness: 0.95,
        metalness: 0.0,
      }),
      true,
      "stone"
    );
    c4.castShadow = true;
    c4.receiveShadow = true;

    const yBeam = yCols + columnHeight * 0.5 + beamHeight * 0.5;

    pos.set(x, yBeam, z - colOffsetZ);
    const bFront = createBoxWithPhysics(
      beamWidth,
      beamHeight,
      beamDepth,
      4,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x666666,
        roughness: 0.45,
        metalness: 0.8,
      }),
      true,
      "metal"
    );
    bFront.castShadow = true;
    bFront.receiveShadow = true;

    pos.set(x, yBeam, z + colOffsetZ);
    const bBack = createBoxWithPhysics(
      beamWidth,
      beamHeight,
      beamDepth,
      4,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x666666,
        roughness: 0.45,
        metalness: 0.8,
      }),
      true,
      "metal"
    );
    bBack.castShadow = true;
    bBack.receiveShadow = true;

    pos.set(x, yBeam + pigRadius + 0.1, z - colOffsetZ * 0.6);
    const pig1 = createSphereWithPhysics(
      pigRadius,
      1,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x66cc33,
        roughness: 0.6,
        metalness: 0.0,
      }),
      true,
      true
    );
    pig1.castShadow = true;
    pig1.receiveShadow = true;

    pos.set(x, yBeam + pigRadius + 0.1, z + colOffsetZ * 0.6);
    const pig2 = createSphereWithPhysics(
      pigRadius,
      1,
      pos,
      quat,
      new THREE.MeshStandardMaterial({
        color: 0x66cc33,
        roughness: 0.6,
        metalness: 0.0,
      }),
      true,
      true
    );
    pig2.castShadow = true;
    pig2.receiveShadow = true;

    baseY = yBeam + pigRadius + 0.1 + 0.6;
  }

  pos.set(x, baseY + pigRadius + 0.4, z);
  const topPig = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  topPig.castShadow = true;
  topPig.receiveShadow = true;
}

function buildBridgeStructure(x, z) {
  const baseWidth = 8;
  const baseHeight = 0.6;
  const baseDepth = 4;

  const pillarWidth = 0.8;
  const pillarDepth = 0.8;
  const pillarHeight = 3.0;

  const bridgeWidth = 10;
  const bridgeHeight = 0.6;
  const bridgeDepth = 2.2;

  const pigRadius = 0.5;

  quat.set(0, 0, 0, 1);

  const offsetX = 3.5;

  pos.set(x - offsetX, baseHeight * 0.5, z);
  const baseLeft = createBoxWithPhysics(
    baseWidth,
    baseHeight,
    baseDepth,
    0,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xc9b291,
      roughness: 0.85,
      metalness: 0.05,
    }),
    true,
    "wood"
  );
  baseLeft.castShadow = true;
  baseLeft.receiveShadow = true;

  pos.set(x + offsetX, baseHeight * 0.5, z);
  const baseRight = createBoxWithPhysics(
    baseWidth,
    baseHeight,
    baseDepth,
    0,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0xc9b291,
      roughness: 0.85,
      metalness: 0.05,
    }),
    true,
    "wood"
  );
  baseRight.castShadow = true;
  baseRight.receiveShadow = true;

  const yPillar = baseHeight + pillarHeight * 0.5;

  pos.set(x - offsetX + 2, yPillar, z - 0.9);
  const p1 = createBoxWithPhysics(
    pillarWidth,
    pillarHeight,
    pillarDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x999999,
      roughness: 0.96,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  p1.castShadow = true;
  p1.receiveShadow = true;

  pos.set(x - offsetX + 2, yPillar, z + 0.9);
  const p2 = createBoxWithPhysics(
    pillarWidth,
    pillarHeight,
    pillarDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x999999,
      roughness: 0.96,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  p2.castShadow = true;
  p2.receiveShadow = true;

  pos.set(x + offsetX - 2, yPillar, z - 0.9);
  const p3 = createBoxWithPhysics(
    pillarWidth,
    pillarHeight,
    pillarDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x999999,
      roughness: 0.96,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  p3.castShadow = true;
  p3.receiveShadow = true;

  pos.set(x + offsetX - 2, yPillar, z + 0.9);
  const p4 = createBoxWithPhysics(
    pillarWidth,
    pillarHeight,
    pillarDepth,
    4,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x999999,
      roughness: 0.96,
      metalness: 0.0,
    }),
    true,
    "stone"
  );
  p4.castShadow = true;
  p4.receiveShadow = true;

  const yBridge = yPillar + pillarHeight * 0.5 + bridgeHeight * 0.5;

  pos.set(x, yBridge, z);
  const bridge = createBoxWithPhysics(
    bridgeWidth,
    bridgeHeight,
    bridgeDepth,
    5,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x777777,
      roughness: 0.4,
      metalness: 0.9,
    }),
    true,
    "metal"
  );
  bridge.castShadow = true;
  bridge.receiveShadow = true;

  pos.set(x, yBridge + pigRadius + 0.1, z);
  const pigCenter = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  pigCenter.castShadow = true;
  pigCenter.receiveShadow = true;

  pos.set(x - 3, yBridge + pigRadius + 0.1, z);
  const pigLeft = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  pigLeft.castShadow = true;
  pigLeft.receiveShadow = true;

  pos.set(x + 3, yBridge + pigRadius + 0.1, z);
  const pigRight = createSphereWithPhysics(
    pigRadius,
    1,
    pos,
    quat,
    new THREE.MeshStandardMaterial({
      color: 0x66cc33,
      roughness: 0.6,
      metalness: 0.0,
    }),
    true,
    true
  );
  pigRight.castShadow = true;
  pigRight.receiveShadow = true;
}

function createBirdMesh(radius, color, type) {
  const group = new THREE.Group();

  const bodyMat = new THREE.MeshStandardMaterial({
    color,
    roughness: 0.5,
    metalness: 0.05,
  });
  const body = new THREE.Mesh(
    new THREE.SphereGeometry(radius, 20, 16),
    bodyMat
  );
  body.castShadow = true;
  body.receiveShadow = true;
  group.add(body);

  const wingGeom = new THREE.BoxGeometry(
    radius * 1.2,
    radius * 0.2,
    radius * 0.6
  );
  const wingMat = new THREE.MeshStandardMaterial({
    color: 0xffffff,
    roughness: 0.4,
    metalness: 0.05,
  });

  const leftWing = new THREE.Mesh(wingGeom, wingMat);
  leftWing.position.set(-radius * 1.4, 0, 0);
  leftWing.castShadow = true;
  leftWing.receiveShadow = true;
  group.add(leftWing);

  const rightWing = new THREE.Mesh(wingGeom, wingMat);
  rightWing.position.set(radius * 1.4, 0, 0);
  rightWing.castShadow = true;
  rightWing.receiveShadow = true;
  group.add(rightWing);

  group.userData.leftWing = leftWing;
  group.userData.rightWing = rightWing;
  group.userData.isBird = true;
  group.userData.birdType = type;
  group.userData.trailColor = color;

  const data = { angle: 0 };
  const tween = new TWEEN.Tween(data)
    .to({ angle: Math.PI / 4 }, 200)
    .yoyo(true)
    .repeat(Infinity)
    .onUpdate(() => {
      if (!group.parent) return;
      leftWing.rotation.z = data.angle;
      rightWing.rotation.z = -data.angle;
    });
  tween.start();

  return group;
}

function getBirdParams(type) {
  const baseSpeed = 30;
  const baseMass = 40;
  if (type === 2) {
    return {
      type,
      radius: 0.9,
      mass: baseMass,
      speed: baseSpeed * 5,
      restitution: 0.95,
      color: 0x00aaff,
    };
  } else if (type === 3) {
    return {
      type,
      radius: 1.0,
      mass: baseMass * 1.2,
      speed: baseSpeed * 1.1,
      restitution: 0.3,
      color: 0xffaa00,
    };
  } else if (type === 4) {
    return {
      type,
      radius: 0.85,
      mass: baseMass,
      speed: baseSpeed * 1.2,
      restitution: 0.4,
      color: 0xaa00ff,
    };
  } else if (type === 5) {
    return {
      type,
      radius: 0.95,
      mass: baseMass,
      speed: baseSpeed * 3.1,
      restitution: 0.4,
      color: 0x00ff88,
    };
  }
  return {
    type: 1,
    radius: 0.9,
    mass: baseMass,
    speed: baseSpeed,
    restitution: 0.4,
    color: 0xff0000,
  };
}

function getChargeFactor() {
  if (!isCharging) return 1;
  const now = performance.now();
  const t = (now - chargeStart) / 1000;
  const maxExtra = 1.6;
  const factor = 0.4 + Math.min(t, maxExtra);
  return factor;
}

function launchBird(bird, body, dir, params, chargeFactor = 1) {
  const data = { f: 0 };
  const speed = params.speed * chargeFactor;
  const forwardTween = new TWEEN.Tween(data)
    .to({ f: 1 }, 500)
    .easing(TWEEN.Easing.Quadratic.Out)
    .onUpdate(() => {
      const v = dir.clone().multiplyScalar(speed * data.f);
      body.setLinearVelocity(new Ammo.btVector3(v.x, v.y, v.z));
    });

  if (params.type === 5) {
    forwardTween.onComplete(() => {
      const backData = { f: 0 };
      const backTween = new TWEEN.Tween(backData)
        .to({ f: 1 }, 1000)
        .easing(TWEEN.Easing.Quadratic.InOut)
        .onUpdate(() => {
          const v = dir.clone().multiplyScalar(-speed * backData.f);
          body.setLinearVelocity(new Ammo.btVector3(v.x, v.y, v.z));
        });
      backTween.start();
      bird.userData.returnTween = backTween;
    });
  }

  body.setRestitution(params.restitution);
  forwardTween.start();
  bird.userData.launchTween = forwardTween;
}

function createRigidBody(object, physicsShape, mass, pos, quat, vel, angVel) {
  if (pos) {
    object.position.copy(pos);
  } else {
    pos = object.position;
  }
  if (quat) {
    object.quaternion.copy(quat);
  } else {
    quat = object.quaternion;
  }
  const transform = new Ammo.btTransform();
  transform.setIdentity();
  transform.setOrigin(new Ammo.btVector3(pos.x, pos.y, pos.z));
  transform.setRotation(new Ammo.btQuaternion(quat.x, quat.y, quat.z, quat.w));
  const motionState = new Ammo.btDefaultMotionState(transform);
  const localInertia = new Ammo.btVector3(0, 0, 0);
  physicsShape.calculateLocalInertia(mass, localInertia);
  const rbInfo = new Ammo.btRigidBodyConstructionInfo(
    mass,
    motionState,
    physicsShape,
    localInertia
  );
  const body = new Ammo.btRigidBody(rbInfo);

  body.setFriction(0.5);

  if (vel) {
    body.setLinearVelocity(new Ammo.btVector3(vel.x, vel.y, vel.z));
  }

  if (angVel) {
    body.setAngularVelocity(new Ammo.btVector3(angVel.x, angVel.y, angVel.z));
  }

  object.userData.physicsBody = body;
  object.userData.collided = false;
  body.threeObject = object;

  scene.add(object);
  if (mass > 0) {
    rigidBodies.push(object);
    body.setActivationState(4);
  }
  physicsWorld.addRigidBody(body);

  return body;
}

function ensureAimPreview() {
  if (!aimPreviewLine) {
    const steps = 40;
    const positions = new Float32Array(steps * 3);
    const colors = new Float32Array(steps * 3);
    for (let i = 0; i < steps; i++) {
      const t = i / (steps - 1);
      const r = 1.0;
      const g = 0;
      const b = 0;
      const i3 = 3 * i;
      colors[i3] = r;
      colors[i3 + 1] = g;
      colors[i3 + 2] = b;
    }
    const geom = new THREE.BufferGeometry();
    geom.setAttribute("position", new THREE.BufferAttribute(positions, 3));
    geom.setAttribute("color", new THREE.BufferAttribute(colors, 3));
    const mat = new THREE.LineBasicMaterial({
      vertexColors: true,
      transparent: true,
      opacity: 0.9,
      linewidth: 12,
    });
    aimPreviewLine = new THREE.Line(geom, mat);
    scene.add(aimPreviewLine);
  }
  if (!aimImpactMarker) {
    const geom = new THREE.SphereGeometry(0.3, 16, 16);
    const mat = new THREE.MeshStandardMaterial({
      color: 0xffaa33,
      emissive: 0xffaa33,
      emissiveIntensity: 2.5,
      roughness: 0.3,
      metalness: 0.0,
      transparent: true,
      opacity: 1.0,
    });
    aimImpactMarker = new THREE.Mesh(geom, mat);
    scene.add(aimImpactMarker);
  }
}

function updateAimPreview(origin, dir, speed) {
  ensureAimPreview();
  const positionsAttr = aimPreviewLine.geometry.getAttribute("position");
  const positions = positionsAttr.array;
  const steps = positionsAttr.count;
  const g = new THREE.Vector3(0, -gravityConstant, 0);
  let p = origin.clone();
  let v = dir.clone().multiplyScalar(speed);
  let lastValid = p.clone();
  let stepCount = steps;
  const dt = 0.08;

  for (let i = 0; i < steps; i++) {
    const i3 = 3 * i;
    positions[i3] = p.x;
    positions[i3 + 1] = p.y;
    positions[i3 + 2] = p.z;
    lastValid.copy(p);
    v.addScaledVector(g, dt);
    p.addScaledVector(v, dt);
    if (p.y <= 0.5) {
      stepCount = i + 1;
      break;
    }
  }

  for (let i = stepCount; i < steps; i++) {
    const i3 = 3 * i;
    positions[i3] = lastValid.x;
    positions[i3 + 1] = lastValid.y;
    positions[i3 + 2] = lastValid.z;
  }

  positionsAttr.needsUpdate = true;
  aimPreviewLine.visible = true;

  if (aimImpactMarker) {
    aimImpactMarker.visible = true;
    aimImpactMarker.position.copy(lastValid);
    aimImpactMarker.scale.setScalar(1 + Math.random() * 0.2);
  }
}

function hideAimPreview() {
  if (aimPreviewLine) aimPreviewLine.visible = false;
  if (aimImpactMarker) aimImpactMarker.visible = false;
}

function updateAimFromMouse() {
  raycaster.setFromCamera(mouseCoords, camera);
  const intersection = new THREE.Vector3();
  raycaster.ray.intersectPlane(aimPlane, intersection);
  if (intersection) {
    aimOrigin.copy(slingshotAnchor);
    aimDirection.copy(intersection.sub(slingshotAnchor).normalize());
  }
}

function fireBirdFromAim() {
  if (gameState.state !== "playing") return;
  if (gameState.birdsLeft <= 0) return;

  const params = getBirdParams(currentBirdType);
  const bird = createBirdMesh(params.radius, params.color, params.type);

  const launchOrigin = slingshotAnchor
    .clone()
    .sub(aimDirection.clone().multiplyScalar(2));
  quat.set(0, 0, 0, 1);

  const birdShape = new Ammo.btSphereShape(params.radius);
  birdShape.setMargin(margin);
  const body = createRigidBody(
    bird,
    birdShape,
    params.mass,
    launchOrigin,
    quat
  );

  const dir = aimDirection.clone().normalize();
  const chargeFactor = getChargeFactor();
  launchBird(bird, body, dir, params, chargeFactor);

  bird.userData.isReplayBird = true;
  replayData = { positions: [], duration: 0 };

  activeBird = bird;
  activeBirdBody = body;
  followBird = bird;

  if (params.type === 1) {
    updateAbilityHUD("Pájaro básico (sin habilidad)");
  } else {
    updateAbilityHUD("Habilidad lista (espacio en vuelo)");
  }

  gameState.birdsLeft -= 1;
  if (gameState.birdsLeft < 0) gameState.birdsLeft = 0;
  updateBirdsLeftUI();
  if (gameState.birdsLeft === 0) {
    pendingLoseCheck = true;
  }
}

function createTrailParticle(position, color) {
  const geom = new THREE.SphereGeometry(0.18, 12, 12);
  const mat = new THREE.MeshStandardMaterial({
    color,
    emissive: color,
    emissiveIntensity: 2.5,
    roughness: 0.2,
    metalness: 0.0,
    transparent: true,
    opacity: 1.0,
  });
  const mesh = new THREE.Mesh(geom, mat);
  mesh.position.copy(position);
  scene.add(mesh);
  trailParticles.push({ mesh, life: TRAIL_LIFE });
}

function createExplosionEffect(position, baseColor) {
  const count = 22;
  for (let i = 0; i < count; i++) {
    const geom = new THREE.SphereGeometry(0.12, 6, 6);
    const jitter = ((i % 4) - 1.5) * 0.05 + (Math.random() - 0.5) * 0.08;
    const color = (baseColor + jitter * 0xffffff) >>> 0;
    const mat = new THREE.MeshStandardMaterial({
      color,
      roughness: 0.6,
      metalness: 0.0,
      transparent: true,
      opacity: 1.0,
    });
    const mesh = new THREE.Mesh(geom, mat);
    mesh.position.copy(position);
    const dir = new THREE.Vector3(
      (Math.random() - 0.5) * 2,
      Math.random() * 2,
      (Math.random() - 0.5) * 2
    ).normalize();
    const speed = 8 + Math.random() * 6;
    const vel = dir.multiplyScalar(speed);
    scene.add(mesh);
    explosionParticles.push({
      mesh,
      vel,
      life: 0.7,
    });
  }
}

function updateBirdTrails() {
  for (let i = 0; i < rigidBodies.length; i++) {
    const obj = rigidBodies[i];
    if (!obj.userData.isBird) continue;
    const last = obj.userData.lastTrailPos;
    if (!last) {
      obj.userData.lastTrailPos = obj.position.clone();
      continue;
    }
    if (
      last.distanceToSquared(obj.position) >=
      TRAIL_MIN_DIST * TRAIL_MIN_DIST
    ) {
      const c = obj.userData.trailColor || 0xffaa33;
      createTrailParticle(obj.position, c);
      obj.userData.lastTrailPos.copy(obj.position);
    }
  }
}

function updateTrailParticles(dt) {
  for (let i = trailParticles.length - 1; i >= 0; i--) {
    const p = trailParticles[i];
    p.life -= dt;
    if (p.life <= 0) {
      scene.remove(p.mesh);
      trailParticles.splice(i, 1);
      continue;
    }
    const alpha = Math.max(p.life / TRAIL_LIFE, 0);
    p.mesh.material.opacity = alpha;
    p.mesh.scale.setScalar(0.6 + 0.8 * alpha);
  }
}

function updateExplosionParticles(dt) {
  const g = new THREE.Vector3(0, -gravityConstant, 0);
  for (let i = explosionParticles.length - 1; i >= 0; i--) {
    const p = explosionParticles[i];
    p.life -= dt;
    if (p.life <= 0) {
      scene.remove(p.mesh);
      explosionParticles.splice(i, 1);
      continue;
    }
    p.vel.addScaledVector(g, dt * 0.5);
    p.mesh.position.addScaledVector(p.vel, dt);
    p.mesh.material.opacity = Math.max(p.life / 0.7, 0);
    p.mesh.scale.setScalar(0.5 + p.life);
  }
}

function initInput() {
  window.addEventListener("pointerdown", function (event) {
    if (gameState.state !== "playing") return;
    if (gameState.birdsLeft <= 0) return;
    isCharging = true;
    chargeStart = performance.now();
    mouseCoords.set(
      (event.clientX / window.innerWidth) * 2 - 1,
      -(event.clientY / window.innerHeight) * 2 + 1
    );
    updateAimFromMouse();
  });

  window.addEventListener("pointermove", function (event) {
    if (!isCharging) return;
    mouseCoords.set(
      (event.clientX / window.innerWidth) * 2 - 1,
      -(event.clientY / window.innerHeight) * 2 + 1
    );
    updateAimFromMouse();
  });

  window.addEventListener("pointerup", function () {
    if (!isCharging) return;
    isCharging = false;
    fireBirdFromAim();
    hideAimPreview();
  });

  window.addEventListener("keydown", (event) => {
    if (event.key >= "1" && event.key <= "5") {
      currentBirdType = parseInt(event.key, 10);
      updateBirdIndicator();
      updateHudBird();
    }
    if (event.code === "Space") {
      activateBirdAbility();
    }
  });
}

function onWindowResize() {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
}

function triggerImpactFX(magnitude) {
  shakeTime = maxShakeTime;
  shakeIntensity = magnitude;
  timeScale = 0.4;
  shakeBasePosition = camera.position.clone();
}

function animationLoop() {
  requestAnimationFrame(animationLoop);
  const rawDelta = clock.getDelta();
  const dt = rawDelta * timeScale;
  TWEEN.update();

  if (isCharging) {
    const params = getBirdParams(currentBirdType);
    const factor = getChargeFactor();
    const speed = params.speed * factor;
    const origin = slingshotAnchor
      .clone()
      .sub(aimDirection.clone().multiplyScalar(1.5));
    updateAimPreview(origin, aimDirection, speed);
    updateSlingshotBand(true);
  } else {
    hideAimPreview();
    updateSlingshotBand(false);
  }

  if (!isReplaying) {
    updatePhysics(dt);
  }

  if (followBird && followBird.userData && followBird.userData.physicsBody) {
    const targetPos = followBird.position.clone();
    const desiredPos = targetPos.clone().add(new THREE.Vector3(-8, 6, 0));
    camera.position.lerp(desiredPos, 0.08);
    controls.target.lerp(targetPos, 0.08);
    controls.update();
    if (followBird.position.y < -2 || followBird.position.lengthSq() > 20000) {
      followBird = null;
    }
  } else if (!isReplaying && shakeTime <= 0) {
    resetCameraToBase(false);
  }

  if (shakeTime > 0 && shakeBasePosition) {
    shakeTime -= rawDelta;
    const t = Math.max(shakeTime / maxShakeTime, 0);
    const strength = shakeIntensity * t;
    const base = shakeBasePosition;
    const offsetX = (Math.random() - 0.5) * strength;
    const offsetY = (Math.random() - 0.5) * strength;
    const offsetZ = (Math.random() - 0.5) * strength;
    camera.position.set(base.x + offsetX, base.y + offsetY, base.z + offsetZ);
    timeScale = 0.4 + 0.6 * (1 - t);
    if (shakeTime <= 0) {
      shakeTime = 0;
      timeScale = 1;
      camera.position.copy(base);
      shakeBasePosition = null;
    }
  }

  renderer.render(scene, camera);
}

function explodeBird(body) {
  const obj = body.threeObject;
  if (!obj) return;

  const ms = body.getMotionState();
  if (!ms) return;

  ms.getWorldTransform(transformAux1);
  const center = transformAux1.getOrigin();
  const cx = center.x();
  const cy = center.y();
  const cz = center.z();

  const explosionRadius = 6;
  const explosionStrength = 60;

  for (let i = 0; i < rigidBodies.length; i++) {
    const target = rigidBodies[i];
    if (target === obj) continue;
    const targetBody = target.userData.physicsBody;
    if (!targetBody) continue;
    const tms = targetBody.getMotionState();
    if (!tms) continue;
    tms.getWorldTransform(transformAux1);
    const p = transformAux1.getOrigin();
    const dx = p.x() - cx;
    const dy = p.y() - cy;
    const dz = p.z() - cz;
    const dist = Math.sqrt(dx * dx + dy * dy + dz * dz);
    if (dist > explosionRadius || dist < 0.001) continue;
    const strength = (1 - dist / explosionRadius) * explosionStrength;
    const nx = dx / dist;
    const ny = dy / dist;
    const nz = dz / dist;
    const impulse = new Ammo.btVector3(
      nx * strength,
      ny * strength,
      nz * strength
    );
    targetBody.applyCentralImpulse(impulse);
    Ammo.destroy(impulse);
  }

  createExplosionEffect(new THREE.Vector3(cx, cy, cz), 0x66cc33);
  triggerImpactFX(0.7);

  if (activeBird === obj) {
    activeBird = null;
    activeBirdBody = null;
    updateAbilityHUD("Sin habilidad activa");
  }
  if (followBird === obj) {
    followBird = null;
  }

  scene.remove(obj);
  physicsWorld.removeRigidBody(body);
  for (let i = rigidBodies.length - 1; i >= 0; i--) {
    if (rigidBodies[i] === obj) {
      rigidBodies.splice(i, 1);
    }
  }
}

function duplicateBird(body) {
  const original = body.threeObject;
  if (!original) return;
  if (original.userData.hasDuplicated) return;
  original.userData.hasDuplicated = true;

  const type = original.userData.birdType || 4;
  const params = getBirdParams(type);

  const newBird = createBirdMesh(params.radius, params.color, params.type);
  newBird.position.copy(original.position).add(new THREE.Vector3(0.5, 0.5, 0));
  newBird.quaternion.copy(original.quaternion);

  const shape = new Ammo.btSphereShape(params.radius);
  shape.setMargin(margin);
  const newBody = createRigidBody(
    newBird,
    shape,
    params.mass,
    newBird.position,
    newBird.quaternion
  );

  const vel = body.getLinearVelocity();
  const dir = new THREE.Vector3(vel.x(), vel.y(), vel.z());
  if (dir.lengthSq() === 0) {
    dir.set(1, 0, 0);
  }
  dir.normalize();
  const dir2 = dir.clone().applyAxisAngle(new THREE.Vector3(0, 1, 0), 0.4);

  launchBird(newBird, newBody, dir2, params);
}

function activateBirdAbility() {
  if (!activeBird || !activeBirdBody) return;
  if (activeBird.userData.abilityUsed) return;
  const type = activeBird.userData.birdType || 1;
  if (type === 2) {
    const v = activeBirdBody.getLinearVelocity();
    const vx = v.x();
    const vy = v.y();
    const vz = v.z();
    const speed = Math.sqrt(vx * vx + vy * vy + vz * vz) || 1;
    const factor = 1.8;
    const newV = new Ammo.btVector3(
      (vx / speed) * speed * factor,
      (vy / speed) * speed * factor,
      (vz / speed) * speed * factor
    );
    activeBirdBody.setLinearVelocity(newV);
    Ammo.destroy(newV);
    triggerImpactFX(0.3);
  } else if (type === 3) {
    explodeBird(activeBirdBody);
  } else if (type === 4) {
    duplicateBird(activeBirdBody);
  } else if (type === 5) {
    const v = activeBirdBody.getLinearVelocity();
    const inv = new Ammo.btVector3(-v.x(), -v.y(), -v.z());
    activeBirdBody.setLinearVelocity(inv);
    Ammo.destroy(inv);
    triggerImpactFX(0.3);
  }
  activeBird.userData.abilityUsed = true;
  updateAbilityHUD("Habilidad usada");
}

function registerPigKill() {
  const now = worldTime;
  if (now - lastPigKillTime < 1.0) {
    comboMultiplier = Math.min(comboMultiplier + 1, 5);
  } else {
    comboMultiplier = 1;
  }
  lastPigKillTime = now;
  const gain = basePigScore * comboMultiplier;
  score += gain;
  updateScore();
  updateComboHUD();
}

function killPigObject(obj, withFX) {
  if (!obj || obj.userData.removedPig) return;
  obj.userData.removedPig = true;
  if (withFX) {
    createExplosionEffect(obj.position.clone(), 0x66cc33);
    triggerImpactFX(0.45);
  }
  const body = obj.userData.physicsBody;
  if (body) {
    physicsWorld.removeRigidBody(body);
  }
  scene.remove(obj);
  for (let i = rigidBodies.length - 1; i >= 0; i--) {
    if (rigidBodies[i] === obj) {
      rigidBodies.splice(i, 1);
    }
  }
  pigsAlive -= 1;
  if (pigsAlive < 0) pigsAlive = 0;
  updatePigsUI();
  registerPigKill();
  if (pigsAlive === 0 && gameState.state === "playing") {
    const bonus = gameState.birdsLeft * birdBonusPerRemaining;
    if (bonus > 0) {
      score += bonus;
      updateScore();
    }
    gameState.state = "win";
    pendingLoseCheck = false;
    showEndOverlay("win", bonus);
  }
}

function handleCollisions() {
  const dispatcherLocal = physicsWorld.getDispatcher();
  const numManifolds = dispatcherLocal.getNumManifolds();
  const pigsToRemove = new Set();
  const birdsToExplode = new Set();
  const birdsToDuplicate = new Set();

  if (worldTime < 0.8) {
    return;
  }

  for (let i = 0; i < numManifolds; i++) {
    const manifold = dispatcherLocal.getManifoldByIndexInternal(i);
    const body0 = Ammo.castObject(manifold.getBody0(), Ammo.btRigidBody);
    const body1 = Ammo.castObject(manifold.getBody1(), Ammo.btRigidBody);
    const obj0 = body0 && body0.threeObject;
    const obj1 = body1 && body1.threeObject;
    if (!obj0 && !obj1) continue;

    const numContacts = manifold.getNumContacts();
    for (let j = 0; j < numContacts; j++) {
      const pt = manifold.getContactPoint(j);
      if (pt.getAppliedImpulse() > 1) {
        if (obj0 && obj0.userData && obj0.userData.isPig)
          pigsToRemove.add(obj0);
        if (obj1 && obj1.userData && obj1.userData.isPig)
          pigsToRemove.add(obj1);

        if (obj0 && obj0.userData && obj0.userData.isBird) {
          const t0 = obj0.userData.birdType || 1;
          if (t0 === 3) birdsToExplode.add(body0);
          if (t0 === 4) birdsToDuplicate.add(body0);
        }
        if (obj1 && obj1.userData && obj1.userData.isBird) {
          const t1 = obj1.userData.birdType || 1;
          if (t1 === 3) birdsToExplode.add(body1);
          if (t1 === 4) birdsToDuplicate.add(body1);
        }
        break;
      }
    }
  }

  pigsToRemove.forEach((obj) => {
    killPigObject(obj, true);
  });

  birdsToExplode.forEach((body) => {
    explodeBird(body);
  });

  birdsToDuplicate.forEach((body) => {
    duplicateBird(body);
  });
}

function updatePhysics(deltaTime) {
  worldTime += deltaTime;
  physicsWorld.stepSimulation(deltaTime, 10);
  handleCollisions();

  for (let i = 0, il = rigidBodies.length; i < il; i++) {
    const objThree = rigidBodies[i];
    const objPhys = objThree.userData.physicsBody;
    const ms = objPhys.getMotionState();
    if (ms) {
      ms.getWorldTransform(transformAux1);
      const p = transformAux1.getOrigin();
      const q = transformAux1.getRotation();
      objThree.position.set(p.x(), p.y(), p.z());
      objThree.quaternion.set(q.x(), q.y(), q.z(), q.w());
      objThree.userData.collided = false;
    }
  }

  for (let i = 0; i < levelObjects.length; i++) {
    const obj = levelObjects[i];
    if (!obj.userData.isPig || obj.userData.removedPig) continue;
    if (obj.position.y < -2 || obj.position.lengthSq() > 20000) {
      killPigObject(obj, true);
    }
  }

  for (let i = 0; i < levelObjects.length; i++) {
    const obj = levelObjects[i];
    if (!obj.userData.isPig || obj.userData.removedPig) continue;
    const body = obj.userData.physicsBody;
    if (!body) continue;
    const v = body.getLinearVelocity();
    const speed2 = v.x() * v.x() + v.y() * v.y() + v.z() * v.z();
    if (speed2 < 0.05) {
      const phase = obj.userData.pigPhase || 0;
      const wobble = Math.sin(worldTime * 2 + phase) * 0.1;
      obj.rotation.y = wobble;
      obj.scale.setScalar(1 + Math.sin(worldTime * 4 + phase) * 0.03);
    }
  }

  if (replayData && activeBird && activeBird.userData.isReplayBird) {
    replayData.positions.push(activeBird.position.clone());
    replayData.duration += deltaTime;
  }

  updateBirdTrails();
  updateTrailParticles(deltaTime);
  updateExplosionParticles(deltaTime);

  if (pendingLoseCheck && gameState.state === "playing" && pigsAlive > 0) {
    let anyFastBird = false;
    for (let i = 0; i < rigidBodies.length; i++) {
      const obj = rigidBodies[i];
      if (!obj.userData.isBird) continue;
      const body = obj.userData.physicsBody;
      if (!body) continue;
      const v = body.getLinearVelocity();
      const vx = v.x();
      const vy = v.y();
      const vz = v.z();
      const speed2 = vx * vx + vy * vy + vz * vz;
      if (speed2 > 0.5) {
        anyFastBird = true;
        break;
      }
    }
    if (!anyFastBird) {
      pendingLoseCheck = false;
      gameState.state = "lose";
      showEndOverlay("lose", 0);
    }
  }
}

function playReplay() {
  if (!replayData || !replayData.positions || replayData.positions.length < 2) {
    return;
  }
  if (replayTween) {
    replayTween.stop();
  }
  const curve = new THREE.CatmullRomCurve3(replayData.positions);
  const totalTime = Math.max(replayData.duration, 0.5) * 1000 * 1.2;
  const data = { t: 0 };
  isReplaying = true;
  replayTween = new TWEEN.Tween(data)
    .to({ t: 1 }, totalTime)
    .easing(TWEEN.Easing.Quadratic.InOut)
    .onUpdate(() => {
      const p = curve.getPointAt(data.t);
      const desiredPos = p.clone().add(new THREE.Vector3(-6, 4, 0));
      camera.position.lerp(desiredPos, 0.2);
      controls.target.lerp(p, 0.2);
      controls.update();
    })
    .onComplete(() => {
      isReplaying = false;
      resetCameraToBase(false);
    });
  replayTween.start();
}
```


El código desarrollado se encuentra en *script_45_ammo.js*.

### Añadidos gráficos

Configuré un entorno más *cartoon* con niebla y mapas de tonos.

```js
scene = new THREE.Scene();
scene.background = new THREE.Color(0x87b5ff);
scene.fog = new THREE.Fog(0x87b5ff, 40, 140);

renderer = new THREE.WebGLRenderer();
renderer.setPixelRatio(window.devicePixelRatio);
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.shadowMap.enabled = true;
renderer.outputEncoding = THREE.sRGBEncoding;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.0;
```

A su vez añadí una cámara con un movimiento suavizado y con desplazamiento con reseteado.

```js
controls = new OrbitControls(camera, renderer.domElement);
controls.enablePan = false;
controls.enableDamping = true;
controls.dampingFactor = 0.08;
controls.rotateSpeed = 0.9;
controls.zoomSpeed = 1.0;
controls.minDistance = 10;
controls.maxDistance = 60;
```
Esta es la función que reseteará la cámara cuando siga al pájaro:

```js
const cameraBasePosition = new THREE.Vector3();
const cameraBaseTarget = new THREE.Vector3();

cameraBasePosition.set(-14, 7, 14);
cameraBaseTarget.set(12, 5, 0);
camera.position.copy(cameraBasePosition);
controls.target.copy(cameraBaseTarget);

function resetCameraToBase(immediate) {
  if (!camera || !controls) return;
  if (immediate) {
    camera.position.copy(cameraBasePosition);
    controls.target.copy(cameraBaseTarget);
    controls.update();
  } else {
    camera.position.lerp(cameraBasePosition, 0.2);
    controls.target.lerp(cameraBaseTarget, 0.2);
    controls.update();
  }
}
```

### Tirachinas y apuntado

Para ello puse dos elementos gráficos que sirven como forma visual del tirachinas y se simula una goma que se estira y carga un disparo.

```js
const slingshotAnchor = new THREE.Vector3(-8, 3, 0);
let slingshotGroup = null;
let slingshotBand = null;
let slingshotForkLeft = null;
let slingshotForkRight = null;
```
Con esto incializamos las variables necesarias para crear el  tirachinas.
```js
function createSlingshot() {
  slingshotGroup = new THREE.Group();
  slingshotGroup.position.copy(slingshotAnchor);

  const baseGeom = new THREE.BoxGeometry(1.2, 0.3, 1.2);
  const baseMat = new THREE.MeshStandardMaterial({
    color: 0x8b5a2b,
    roughness: 0.8,
    metalness: 0.05,
  });
  const base = new THREE.Mesh(baseGeom, baseMat);
  base.position.set(0, -0.15, 0);
  slingshotGroup.add(base);

  const pillarGeom = new THREE.CylinderGeometry(0.2, 0.25, 2, 12);
  const pillarMat = new THREE.MeshStandardMaterial({
    color: 0x8b5a2b,
    roughness: 0.8,
    metalness: 0.05,
  });

  const left = new THREE.Mesh(pillarGeom, pillarMat);
  left.position.set(-0.4, 1, 0);
  const right = new THREE.Mesh(pillarGeom, pillarMat);
  right.position.set(0.4, 1, 0);
  slingshotGroup.add(left);
  slingshotGroup.add(right);

  slingshotForkLeft = new THREE.Vector3(-0.4, 2, 0);
  slingshotForkRight = new THREE.Vector3(0.4, 2, 0);

  const bandGeom = new THREE.BufferGeometry();
  const bandPositions = new Float32Array(3 * 3);
  bandGeom.setAttribute("position", new THREE.BufferAttribute(bandPositions, 3));
  const bandMat = new THREE.LineBasicMaterial({ color: 0x442200 });
  slingshotBand = new THREE.Line(bandGeom, bandMat);
  slingshotGroup.add(slingshotBand);

  scene.add(slingshotGroup);
  updateSlingshotBand(false);
}
```
Con esto tenemos inicializado y creado el tirachinas

```js
let isCharging = false;
let chargeStart = 0;
const aimOrigin = new THREE.Vector3();
const aimDirection = new THREE.Vector3();

function getChargeFactor() {
  if (!isCharging) return 1;
  const now = performance.now();
  const t = (now - chargeStart) / 1000;
  const maxExtra = 1.6;
  return 0.4 + Math.min(t, maxExtra);
}

function updateSlingshotBand(stretched) {
  if (!slingshotBand) return;
  const positions = slingshotBand.geometry.attributes.position.array;
  const leftWorld = slingshotForkLeft.clone().applyMatrix4(slingshotGroup.matrixWorld);
  const rightWorld = slingshotForkRight.clone().applyMatrix4(slingshotGroup.matrixWorld);
  let pocket;
  if (stretched && isCharging) {
    const charge = Math.min(getChargeFactor(), 2.0);
    const stretchLen = 1 + charge * 1.5;
    pocket = slingshotAnchor.clone().sub(aimDirection.clone().multiplyScalar(stretchLen));
  } else {
    pocket = leftWorld.clone().add(rightWorld).multiplyScalar(0.5);
  }
  positions[0] = leftWorld.x;  positions[1] = leftWorld.y;  positions[2] = leftWorld.z;
  positions[3] = pocket.x;     positions[4] = pocket.y;     positions[5] = pocket.z;
  positions[6] = rightWorld.x; positions[7] = rightWorld.y; positions[8] = rightWorld.z;
  slingshotBand.geometry.attributes.position.needsUpdate = true;
}
```
Con esto se carga el tirachinas.

```js
const aimPlane = new THREE.Plane(new THREE.Vector3(0, 1, 0), -slingshotAnchor.y);
const mouseCoords = new THREE.Vector2();
const raycaster = new THREE.Raycaster();

function updateAimFromMouse() {
  raycaster.setFromCamera(mouseCoords, camera);
  const intersection = new THREE.Vector3();
  raycaster.ray.intersectPlane(aimPlane, intersection);
  if (intersection) {
    aimOrigin.copy(slingshotAnchor);
    aimDirection.copy(intersection.sub(slingshotAnchor).normalize());
  }
}
```
Y con este fragmento de código, se hace el apuntado con el ratón.

### HUD e Interfaz
Esta es la inicialización del HUD:
```js
scoreElement = document.createElement("div");
  scoreElement.style.position = "fixed";
  scoreElement.style.top = "10px";
  scoreElement.style.left = "10px";
  scoreElement.style.padding = "8px 12px";
  scoreElement.style.background = "rgba(0,0,0,0.5)";
  scoreElement.style.color = "#fff";
  scoreElement.style.fontFamily = "sans-serif";
  scoreElement.style.fontSize = "14px";
  scoreElement.style.zIndex = "100";
  document.body.appendChild(scoreElement);
  updateScore();

  birdIndicatorElement = document.createElement("div");
  birdIndicatorElement.style.position = "fixed";
  birdIndicatorElement.style.top = "40px";
  birdIndicatorElement.style.left = "10px";
  birdIndicatorElement.style.padding = "6px 10px";
  birdIndicatorElement.style.background = "rgba(0,0,0,0.5)";
  birdIndicatorElement.style.color = "#fff";
  birdIndicatorElement.style.fontFamily = "sans-serif";
  birdIndicatorElement.style.fontSize = "13px";
  birdIndicatorElement.style.zIndex = "100";
  document.body.appendChild(birdIndicatorElement);
  updateBirdIndicator();

  birdsLeftElement = document.createElement("div");
  birdsLeftElement.style.position = "fixed";
  birdsLeftElement.style.top = "70px";
  birdsLeftElement.style.left = "10px";
  birdsLeftElement.style.padding = "6px 10px";
  birdsLeftElement.style.background = "rgba(0,0,0,0.5)";
  birdsLeftElement.style.color = "#fff";
  birdsLeftElement.style.fontFamily = "sans-serif";
  birdsLeftElement.style.fontSize = "13px";
  birdsLeftElement.style.zIndex = "100";
  document.body.appendChild(birdsLeftElement);

  levelElement = document.createElement("div");
  levelElement.style.position = "fixed";
  levelElement.style.top = "100px";
  levelElement.style.left = "10px";
  levelElement.style.padding = "6px 10px";
  levelElement.style.background = "rgba(0,0,0,0.5)";
  levelElement.style.color = "#fff";
  levelElement.style.fontFamily = "sans-serif";
  levelElement.style.fontSize = "13px";
  levelElement.style.zIndex = "100";
  document.body.appendChild(levelElement);

  pigsElement = document.createElement("div");
  pigsElement.style.position = "fixed";
  pigsElement.style.top = "130px";
  pigsElement.style.left = "10px";
  pigsElement.style.padding = "6px 10px";
  pigsElement.style.background = "rgba(0,0,0,0.5)";
  pigsElement.style.color = "#fff";
  pigsElement.style.fontFamily = "sans-serif";
  pigsElement.style.fontSize = "13px";
  pigsElement.style.zIndex = "100";
  document.body.appendChild(pigsElement);
  updatePigsUI();

  comboElement = document.createElement("div");
  comboElement.style.position = "fixed";
  comboElement.style.top = "160px";
  comboElement.style.left = "10px";
  comboElement.style.padding = "6px 10px";
  comboElement.style.background = "rgba(0,0,0,0.5)";
  comboElement.style.color = "#ffdd33";
  comboElement.style.fontFamily = "sans-serif";
  comboElement.style.fontSize = "13px";
  comboElement.style.zIndex = "100";
  comboElement.style.display = "none";
  document.body.appendChild(comboElement);

  abilityElement = document.createElement("div");
  abilityElement.style.position = "fixed";
  abilityElement.style.top = "190px";
  abilityElement.style.left = "10px";
  abilityElement.style.padding = "6px 10px";
  abilityElement.style.background = "rgba(0,0,0,0.5)";
  abilityElement.style.color = "#66ffcc";
  abilityElement.style.fontFamily = "sans-serif";
  abilityElement.style.fontSize = "13px";
  abilityElement.style.zIndex = "100";
  document.body.appendChild(abilityElement);
  updateAbilityHUD("Sin habilidad activa");

  endOverlay = document.createElement("div");
  endOverlay.style.position = "fixed";
  endOverlay.style.top = "0";
  endOverlay.style.left = "0";
  endOverlay.style.width = "100%";
  endOverlay.style.height = "100%";
  endOverlay.style.display = "none";
  endOverlay.style.alignItems = "center";
  endOverlay.style.justifyContent = "center";
  endOverlay.style.background = "rgba(0,0,0,0.5)";
  endOverlay.style.zIndex = "200";

  const panel = document.createElement("div");
  panel.style.background = "#222";
  panel.style.color = "#fff";
  panel.style.padding = "20px 30px";
  panel.style.borderRadius = "8px";
  panel.style.fontFamily = "sans-serif";
  panel.style.textAlign = "center";
  panel.style.minWidth = "260px";

  overlayText = document.createElement("div");
  overlayText.style.marginBottom = "15px";
  overlayText.style.fontSize = "18px";
  panel.appendChild(overlayText);

  const buttonsRow = document.createElement("div");
  buttonsRow.style.display = "flex";
  buttonsRow.style.justifyContent = "center";
  buttonsRow.style.gap = "10px";

  const retryButton = document.createElement("button");
  retryButton.textContent = "Reintentar nivel";
  retryButton.style.padding = "6px 10px";
  retryButton.style.cursor = "pointer";
  retryButton.onclick = () => {
    resetLevel();
  };

  overlayNextButton = document.createElement("button");
  overlayNextButton.textContent = "Siguiente nivel";
  overlayNextButton.style.padding = "6px 10px";
  overlayNextButton.style.cursor = "pointer";
  overlayNextButton.onclick = () => {
    nextLevel();
  };

  overlayReplayButton = document.createElement("button");
  overlayReplayButton.textContent = "Replay disparo";
  overlayReplayButton.style.padding = "6px 10px";
  overlayReplayButton.style.cursor = "pointer";
  overlayReplayButton.onclick = () => {
    playReplay();
  };

  buttonsRow.appendChild(retryButton);
  buttonsRow.appendChild(overlayNextButton);
  buttonsRow.appendChild(overlayReplayButton);
  panel.appendChild(buttonsRow);
  endOverlay.appendChild(panel);
  document.body.appendChild(endOverlay);

  updateHudBird();
  updateBirdsLeftUI();
  updateLevelUI();
```

Si nos fijamos la final llamamos a 4 funciones que actualizan el HUD.


```js
function updateScore() {
  if (scoreElement) {
    scoreElement.textContent = "Puntos: " + score;
  }
}

function getBirdLabel(type) {
  if (type === 2) return "2: Rápido rebotador";
  if (type === 3) return "3: Explosivo";
  if (type === 4) return "4: Doble";
  if (type === 5) return "5: Boomerang";
  return "1: Básico";
}

function updateBirdIndicator() {
  if (!birdIndicatorElement) return;
  const label = getBirdLabel(currentBirdType);
  birdIndicatorElement.textContent = "Pájaro actual: " + label;
}

function updateComboHUD() {
  if (!comboElement) return;
  if (comboMultiplier > 1) {
    comboElement.style.display = "block";
    comboElement.textContent = "Combo x" + comboMultiplier;
  } else {
    comboElement.style.display = "none";
  }
}

function updateAbilityHUD(text) {
  if (!abilityElement) return;
  abilityElement.textContent = text;
}
```
- `updateScore`: Actualiza la puntuación del jugador.
- `getBirdLabel`:Mediante el número que se le pasa detecta que pájaro es el cargado en escena.
- `updateBirdIndicatorr`: Indica el pájaro actual.
- `updateComboHUD`: Actualiza la puntuación con el comobo actual.
- `updateAbilityHUD`: Indica si la habilidad esta disponible


```js
let hudBird = null;

function updateHudBird() {
  if (hudBird) {
    camera.remove(hudBird);
    hudBird = null;
  }
  const params = getBirdParams(currentBirdType);
  hudBird = createBirdMesh(params.radius * 0.6, params.color, params.type);
  hudBird.traverse((o) => {
    if (o.isMesh) {
      o.castShadow = false;
      o.receiveShadow = false;
    }
  });
  hudBird.position.set(-3, -2, -8);
  camera.add(hudBird);
}
```

Coloca un pájaro indicativo de que pájaro es el actual de forma visual.


## Resultado

El video del juego  se puede ver aquí:

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
Para estos ejercicios he usado de referencia los apuntes del profesor. Y además los siguientes materiales complementarios:
- [*The Book of Shaders*](https://thebookofshaders.com/?lan=es): Para entender mejor el funcionamiento del lenguaje `GLSL`, las funciones de ruido, la interpolación de Hermite, domain warping, las paletas de colores, y para referencias de patrones generativos.

- [iquilez](https://www.shadertoy.com/view/MtcSz4) para referencias de mandala, uso de Brownian motion, colores.

- [paletton](https://paletton.com/): Como guía para la selección de colores.
- Chatgpt como guía tutor que me recomendaba técnicas y ajustaba al contenido de acuerdo a mis necesidades. Además de corrección de errores.




