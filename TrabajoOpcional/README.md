<div style="center">

[![Texto en movimiento](https://readme-typing-svg.herokuapp.com?font=Fira+Code&size=25&duration=1500&pause=9000&color=8A36D2&center=true&vCenter=true&width=400&height=50&lines=Informática+gráfica)]()




---

![Unity](https://img.shields.io/badge/Unity-2022%2B-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-.NET%20%2F%20Unity-512BD4?logo=csharp)
![Meta Quest](https://img.shields.io/badge/Meta%20Quest-VR%20%26%20AR-1C1E20?logo=meta)
![XR Toolkit](https://img.shields.io/badge/XR%20Interaction%20Toolkit-Unity-0A84FF)
![AR Foundation](https://img.shields.io/badge/AR%20Foundation-Unity-00C7B7)
</div>

--- 
---
## Práctica opcional: Informática Gráfica
### Unity y Realidad Aumentada

En esta tarea se nos ha dado la libertad de explorar cualquiera de los contenidos vistos en la asignatura. Como en las últimas semanas solo hemos hecho una práctica de Realidad Virtual y ninguna basada en Unity (herramienta con la que ya había trabajado gracias a mi formación), he decidido desarrollar un pequeño programa de prueba que pueda seguir ampliando en adelante.

Actualmente, el proyecto consta de **18 scripts desarrollados en C#**.

### Funcionalidades implementadas

#### Apuntado automático
En función de lo cerrado que esté el círculo de la retícula, hará una corrección hacia el *Pokemón* más cercano. Siendo además un asistido que depende según el tipo de *Pokeball*. Este sistema analiza la dirección del jugador, la retícula y los *Pokemón* cercanos y corrige con esto la trayectoria.

El código de este apuntado sería:
`AimAssistThrow.cs`

```cs
using UnityEngine;

public class AimAssistThrow : MonoBehaviour
{
    [Header("References")]
    public ReactiveReticle reticle;    
    public Transform rayOrigin;      
    public GrabSpawnBall spawner;      
    public LayerMask monsterLayers;    

    [Header("Aim assist settings")]
    public float maxDistance = 25f;
    public float coneAngleDegrees = 18f;  
    public float baseAssist = 0.75f;       
    public float snapAssistMaster = 1.0f; 

    public Vector3 ApplyAimAssist(Vector3 velocity)
    {
        if (!rayOrigin) return velocity;

        float aim01 = reticle ? reticle.AimAssist01 : 0f;
        if (spawner && spawner.selectedType == BallType.Master)
            aim01 = 1f;

        float strength = aim01 * baseAssist;
        if (spawner && spawner.selectedType == BallType.Master)
            strength = snapAssistMaster;

        if (strength <= 0.001f) return velocity;

        Transform target = FindBestTarget();
        if (!target) return velocity;

        Vector3 dirTo = (target.position - rayOrigin.position);
        dirTo.y = 0f;

        if (dirTo.sqrMagnitude < 0.001f) return velocity;

        Vector3 desiredDir = dirTo.normalized;

        Vector3 v = velocity;
        float speed = v.magnitude;
        if (speed < 0.001f) return velocity;

        Vector3 curDir = v.normalized;

        Vector3 newDir = Vector3.Slerp(curDir, desiredDir, strength).normalized;
        return newDir * speed;
    }

    Transform FindBestTarget()
    {
        if (monsterLayers.value == 0) return null;

        Collider[] hits = Physics.OverlapSphere(rayOrigin.position, maxDistance, monsterLayers, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return null;

        Transform best = null;
        float bestScore = -999f;

        Vector3 forward = rayOrigin.forward;
        forward.y = 0f;
        forward.Normalize();

        float cosLimit = Mathf.Cos(coneAngleDegrees * Mathf.Deg2Rad);

        for (int i = 0; i < hits.Length; i++)
        {
            Transform t = hits[i].transform;
            Vector3 to = (t.position - rayOrigin.position);
            to.y = 0f;

            float dist = to.magnitude;
            if (dist < 0.01f) continue;

            Vector3 toDir = to / dist;

            float cos = Vector3.Dot(forward, toDir);
            if (cos < cosLimit) continue; 
            float score = (cos * 2.0f) - (dist / maxDistance);
            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }

    void OnDrawGizmosSelected()
    {
        if (!rayOrigin) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rayOrigin.position, maxDistance);
    }
}
``` 

#### Asignación de los *Headers* y variables:

En Unity, una buena práctica es declarar antes de cualquier función, las diferentes variables que vamos a utilizar. Y para poder verlo de una forma intuitiva en el inspector, se les pone, según las unidades lógicas a las que pertenecen un *Header* que nos permite rápidamente saber la utilidad de lo que estamos poniendo.

En nuestro claso las variabels declaradas y los headers son:

```cs
    [Header("References")]
    public ReactiveReticle reticle;    
    public Transform rayOrigin;      
    public GrabSpawnBall spawner;      
    public LayerMask monsterLayers;    

    [Header("Aim assist settings")]
    public float maxDistance = 25f;
    public float coneAngleDegrees = 18f;  
    public float baseAssist = 0.75f;       
    public float snapAssistMaster = 1.0f; 
```

Hay dependencias con otros códigos que explicaremos más adelante, como el ReactiveReticle, el GrabSpawnBall y luego a la Capa de monstruos y a nuestra cámara de las Gafas VR.

Luego están unas serie de deficiones a saber:

-  `maxDistance`: Distancia máxima a la que se le buscan Pokemón candidatos.
- `coneAngleDegrees`: Ángulo del cono frontal dentro del cual el Pokemón puede ser considerado objetivo.
- `baseAssist`: Factor de corrección aplicado a la retícula cerrada.
- `snapAssistManager`: Intensidad para la *Masterball*


### Aplicacioón del Asistente de apuntado:

Esta función es la central en el código. Recibe una velocidad original y devuelve una corregida, de forma suave hacia el *pokemón*.

```cs    
public Vector3 ApplyAimAssist(Vector3 velocity)
    {
        if (!rayOrigin) return velocity;

        float aim01 = reticle ? reticle.AimAssist01 : 0f;
        if (spawner && spawner.selectedType == BallType.Master)
            aim01 = 1f;

        float strength = aim01 * baseAssist;
        if (spawner && spawner.selectedType == BallType.Master)
            strength = snapAssistMaster;

        if (strength <= 0.001f) return velocity;

        Transform target = FindBestTarget();
        if (!target) return velocity;

        Vector3 dirTo = (target.position - rayOrigin.position);
        dirTo.y = 0f;

        if (dirTo.sqrMagnitude < 0.001f) return velocity;

        Vector3 desiredDir = dirTo.normalized;

        Vector3 v = velocity;
        float speed = v.magnitude;
        if (speed < 0.001f) return velocity;

        Vector3 curDir = v.normalized;

        Vector3 newDir = Vector3.Slerp(curDir, desiredDir, strength).normalized;
        return newDir * speed;
    }
```
Se comprueban que existen los componentes necesarios. Esto evita que se provoquen fallos en el asistido.

A continuación se obtiene el nivel de ayuda apartir de la retícula, una vez se tiene esto, se calcula la intensidad real. Esta intensidad, dependerá del porcentaje de acierto de la *Pokeball*. Con la fuerza calculada, el sistema busca un objetivo válido, con el más probable que sea el objetivo real.

Se escoge objetivo y se corrige la trayectoria suavemente, manteniendo la velocidad original   .


```cs
    Transform FindBestTarget()
    {
        if (monsterLayers.value == 0) return null;

        Collider[] hits = Physics.OverlapSphere(rayOrigin.position, maxDistance, monsterLayers, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return null;

        Transform best = null;
        float bestScore = -999f;

        Vector3 forward = rayOrigin.forward;
        forward.y = 0f;
        forward.Normalize();

        float cosLimit = Mathf.Cos(coneAngleDegrees * Mathf.Deg2Rad);

        for (int i = 0; i < hits.Length; i++)
        {
            Transform t = hits[i].transform;
            Vector3 to = (t.position - rayOrigin.position);
            to.y = 0f;

            float dist = to.magnitude;
            if (dist < 0.01f) continue;

            Vector3 toDir = to / dist;

            float cos = Vector3.Dot(forward, toDir);
            if (cos < cosLimit) continue; 
            float score = (cos * 2.0f) - (dist / maxDistance);
            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }
```

Esta función decide cuál es el *Pokemón* al que es más probale se apunte para el disparo. Usando un cono sobre donde mira el jugador. Se ignoran aquellos objetivos que estan fuera del cono.

```cs
    void OnDrawGizmosSelected()
    {
        if (!rayOrigin) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rayOrigin.position, maxDistance);
    }
```

Esto es para depuración permite ver el cono.

### Música de fondo

Existe un Script para gestionar la música que se escucha de fondo:

`BackgroundMusic.cs` 
```cs
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioSource musicSource;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (!musicSource)
            musicSource = GetComponent<AudioSource>();

        if (musicSource && !musicSource.isPlaying)
            musicSource.Play();
    }
}
```

Primero se declara el Audiosource que gestionará la música, se genera con *Awake* que se inicia antes de *Start* garantizando que funciona antes de que se inicialice la escena.

Lo ideal es que el *MusicManager* lo gestione también el *GameManager* pero, en este caso, no existe puesto que no tenemos varias escenas. En su lugar usamos *PlayerPrefs*, para guardar elementos que se guarden entre sesiones.

### Selección de bolas

Seleccionamos la *Pokeball* que queremos, con la palanca de la mano izquierda, navega entre las 4 opciones de las Pokeballs y actualiza el feedback del anillo y se aplica en el sistema de *Spawn*. E incluye *feedback* sonoro y vibración háptica.

```cs
using UnityEngine;
using UnityEngine.UI;

public class BallSelectionMenu : MonoBehaviour
{
    [Header("Spawner (mano derecha)")]
    public GrabSpawnBall spawner;

    [Header("UI (en orden: Poke, Ultra, Safari, Master)")]
    public RectTransform[] optionRects;  
    public Graphic[] optionGraphics;    
    public GameObject[] selectionRings;  

    [Header("Visual tuning (multiplicadores)")]
    [Tooltip("Multiplicador para la opcion seleccionada (1.15 recomendado)")]
    public float selectedScale = 1.15f;

    [Tooltip("Multiplicador para opciones no seleccionadas (1.0 recomendado)")]
    public float normalScale = 1.0f;

    [Header("Input")]
    public float deadzone = 0.6f;
    public float repeatDelay = 0.25f;

    [Header("Behavior")]
    [Tooltip("Si esta activo, no hace falta pulsar X: se aplica al mover la palanca")]
    public bool applyOnNavigate = true;

    [Header("Feedback")]
    public AudioSource audioSource;        
    public AudioClip moveClip;         
    public AudioClip confirmClip;           
    [Range(0f, 1f)] public float hapticStrength = 0.3f;
    public float hapticDuration = 0.03f;

    public int currentIndex = 0;

    float nextMoveTime = 0f;
    Vector3[] baseScales;

    void Start()
    {
        CacheBaseScales();
        ApplySelectionVisual();
        ApplySelectionToSpawner();
    }

    void Update()
    {
        HandleStickNavigation();
        if (!applyOnNavigate) HandleConfirm();
    }

    void CacheBaseScales()
    {
        if (optionRects == null || optionRects.Length < 4) return;

        baseScales = new Vector3[optionRects.Length];
        for (int i = 0; i < optionRects.Length; i++)
            baseScales[i] = optionRects[i] ? optionRects[i].localScale : Vector3.one;
    }

    void HandleStickNavigation()
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        bool wantsMove = Mathf.Abs(stick.x) > deadzone || Mathf.Abs(stick.y) > deadzone;
        if (!wantsMove) return;
        if (Time.time < nextMoveTime) return;

        nextMoveTime = Time.time + repeatDelay;

        int prev = currentIndex;

        if (Mathf.Abs(stick.x) > Mathf.Abs(stick.y))
            currentIndex += (stick.x > 0f) ? 1 : -1;
        else
            currentIndex += (stick.y > 0f) ? -1 : 1;

        currentIndex = Mod(currentIndex, 4);

        if (currentIndex != prev)
        {
            ApplySelectionVisual();

            if (applyOnNavigate)
                ApplySelectionToSpawner();

            PlayMoveFeedback();
        }
    }

    void HandleConfirm()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            ApplySelectionToSpawner();
            PlayConfirmFeedback();
        }
    }

    void ApplySelectionToSpawner()
    {
        if (spawner) spawner.SetBallType(currentIndex);
    }

    void ApplySelectionVisual()
    {
        if (optionRects != null && optionRects.Length >= 4 && baseScales != null)
        {
            for (int i = 0; i < optionRects.Length; i++)
            {
                if (!optionRects[i]) continue;
                float mult = (i == currentIndex) ? selectedScale : normalScale;
                optionRects[i].localScale = baseScales[i] * mult;
            }
        }
        if (selectionRings != null && selectionRings.Length >= 4)
        {
            for (int i = 0; i < selectionRings.Length; i++)
                if (selectionRings[i]) selectionRings[i].SetActive(i == currentIndex);
        }

        if (optionGraphics != null && optionGraphics.Length >= 4)
        {
            for (int i = 0; i < optionGraphics.Length; i++)
            {
                if (!optionGraphics[i]) continue;
                var c = optionGraphics[i].color;
                c.a = (i == currentIndex) ? 1f : 0.45f;
                optionGraphics[i].color = c;
            }
        }
    }

    void PlayMoveFeedback()
    {
        if (audioSource && moveClip) audioSource.PlayOneShot(moveClip);

        OVRInput.SetControllerVibration(0.0f, hapticStrength, OVRInput.Controller.LTouch);
        CancelInvoke(nameof(StopHaptics));
        Invoke(nameof(StopHaptics), hapticDuration);
    }

    void PlayConfirmFeedback()
    {
        if (audioSource && confirmClip) audioSource.PlayOneShot(confirmClip);

        OVRInput.SetControllerVibration(0.0f, Mathf.Min(1f, hapticStrength + 0.2f), OVRInput.Controller.LTouch);
        CancelInvoke(nameof(StopHaptics));
        Invoke(nameof(StopHaptics), hapticDuration * 1.5f);
    }

    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }
}
```

#### Inicialización de variables
```cs
    [Header("Spawner (mano derecha)")]
    public GrabSpawnBall spawner;

    [Header("UI (en orden: Poke, Ultra, Safari, Master)")]
    public RectTransform[] optionRects;  
    public Graphic[] optionGraphics;    
    public GameObject[] selectionRings;  

    [Header("Visual tuning (multiplicadores)")]
    [Tooltip("Multiplicador para la opcion seleccionada (1.15 recomendado)")]
    public float selectedScale = 1.15f;

    [Tooltip("Multiplicador para opciones no seleccionadas (1.0 recomendado)")]
    public float normalScale = 1.0f;

    [Header("Input")]
    public float deadzone = 0.6f;
    public float repeatDelay = 0.25f;

    [Header("Behavior")]
    [Tooltip("Si esta activo, no hace falta pulsar X: se aplica al mover la palanca")]
    public bool applyOnNavigate = true;

    [Header("Feedback")]
    public AudioSource audioSource;        
    public AudioClip moveClip;         
    public AudioClip confirmClip;           
    [Range(0f, 1f)] public float hapticStrength = 0.3f;
    public float hapticDuration = 0.03f;

    public int currentIndex = 0;

    float nextMoveTime = 0f;
    Vector3[] baseScales;
```

Se declara todo lo necesario para realizar la selección de la bola, el tamañom, el feedback, donde aparece...

```cs
    void CacheBaseScales()
    {
        if (optionRects == null || optionRects.Length < 4) return;

        baseScales = new Vector3[optionRects.Length];
        for (int i = 0; i < optionRects.Length; i++)
            baseScales[i] = optionRects[i] ? optionRects[i].localScale : Vector3.one;
    }
```

Guarda la escala original de cada opción y evita la acumulación.


```cs
    void HandleStickNavigation()
    {
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        bool wantsMove = Mathf.Abs(stick.x) > deadzone || Mathf.Abs(stick.y) > deadzone;
        if (!wantsMove) return;
        if (Time.time < nextMoveTime) return;

        nextMoveTime = Time.time + repeatDelay;

        int prev = currentIndex;

        if (Mathf.Abs(stick.x) > Mathf.Abs(stick.y))
            currentIndex += (stick.x > 0f) ? 1 : -1;
        else
            currentIndex += (stick.y > 0f) ? -1 : 1;

        currentIndex = Mod(currentIndex, 4);

        if (currentIndex != prev)
        {
            ApplySelectionVisual();

            if (applyOnNavigate)
                ApplySelectionToSpawner();

            PlayMoveFeedback();
        }
    }
```

Gestiona el movimiento de la palanca y selecciona de forma visual la pokeball con el movimiento del *Joystick*.

```cs
    void HandleConfirm()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            ApplySelectionToSpawner();
            PlayConfirmFeedback();
        }
    }
```

Se aplica el feedback, es un remanente, ahora se hace automáticamente sin necesidad de confirmar gracias a *ApplyOnNavigate*.

```cs
    void ApplySelectionToSpawner()
    {
        if (spawner) spawner.SetBallType(currentIndex);
    }
```

Aplica la selección a la mano o a donde se establezca el Spawn.

```cs
    void ApplySelectionVisual()
    {
        if (optionRects != null && optionRects.Length >= 4 && baseScales != null)
        {
            for (int i = 0; i < optionRects.Length; i++)
            {
                if (!optionRects[i]) continue;
                float mult = (i == currentIndex) ? selectedScale : normalScale;
                optionRects[i].localScale = baseScales[i] * mult;
            }
        }
        if (selectionRings != null && selectionRings.Length >= 4)
        {
            for (int i = 0; i < selectionRings.Length; i++)
                if (selectionRings[i]) selectionRings[i].SetActive(i == currentIndex);
        }

        if (optionGraphics != null && optionGraphics.Length >= 4)
        {
            for (int i = 0; i < optionGraphics.Length; i++)
            {
                if (!optionGraphics[i]) continue;
                var c = optionGraphics[i].color;
                c.a = (i == currentIndex) ? 1f : 0.45f;
                optionGraphics[i].color = c;
            }
        }
    }
```

Aplica la bola seleccionada a la mano, la que tenemos como *Prefab*.

```cs
    void PlayMoveFeedback()
    {
        if (audioSource && moveClip) audioSource.PlayOneShot(moveClip);

        OVRInput.SetControllerVibration(0.0f, hapticStrength, OVRInput.Controller.LTouch);
        CancelInvoke(nameof(StopHaptics));
        Invoke(nameof(StopHaptics), hapticDuration);
    }
```

Da un firma viusual y físico cuando se cambia de selección.

```cs
    void PlayConfirmFeedback()
    {
        if (audioSource && confirmClip) audioSource.PlayOneShot(confirmClip);

        OVRInput.SetControllerVibration(0.0f, Mathf.Min(1f, hapticStrength + 0.2f), OVRInput.Controller.LTouch);
        CancelInvoke(nameof(StopHaptics));
        Invoke(nameof(StopHaptics), hapticDuration * 1.5f);
    }
```

Si se mantiene la confirmación en las *Pokeball* se hace un *feedback*.

```cs
    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }
```

Para la vibración.

```cs
    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }
```
Evita que al decrementar desde 0 salga -1 y rompa el *array*.

### Ball Types

Define los tipos de *Pokeball* disponibles en el sistema y la estructura básica que los vincula con sus prefabs de Unity.

`BallTypes.cs`
```cs
using System;
using UnityEngine;

public enum BallType { Poke, Ultra, Safari, Master }

[Serializable]
public class BallEntry
{
    public BallType type;
    public GameObject prefab;
}
```

```cs
public enum BallType { Poke, Ultra, Safari, Master }
```
Enumera de forma explicita los tipos de Pokeballs que hay.
- *Pokeball*
- *Ultraball*
- *Safariball*
- *Master*

```cs
[Serializable]
public class BallEntry
{
    public BallType type;
    public GameObject prefab;
}
```
Esta clase actúa como una estructura de datos que relaciona:
- Tipo
- Prefab

### GrabSpawnBall

Gestiona el ciclo completo de la *Pokeball* en la mano: *spawn*, mantenearla pegada y lanzarla.

`GrabSpawnBall.cs`
```cs
using UnityEngine;

public class GrabSpawnBall : MonoBehaviour
{
    [Header("Catalog")]
    public BallEntry[] balls;
    public BallType selectedType = BallType.Poke;

    [Header("Spawn")]
    public Transform spawnPoint;     
    public bool rightHand = true; 

    [Header("Hold behavior")]
    public bool stickToHandWhileHoldingGrip = true;
    public bool disablePhysicsWhileStuck = true;

    [Header("Throw tuning")]
    public float throwVelocityMultiplier = 1.25f;
    public float throwAngularMultiplier = 1.0f;
    public float minThrowSpeed = 0.15f;
    public float forwardBoost = 0.0f;

    [Header("Aim Assist (opcional)")]
    public AimAssistThrow aimAssist;

    [Header("Audio (Throw)")]
    public AudioSource handAudio;         
    public AudioClip throwClip;
    [Range(0f, 1f)] public float throwVolume = 1f;

    private GameObject currentBall;
    private Rigidbody currentRb;

    void Update()
    {
        if (spawnPoint == null) return;

        if (GripDown())
        {
            if (currentBall == null)
                SpawnBallAndStick();
        }

        if (currentBall != null && stickToHandWhileHoldingGrip && GripHeld())
        {
            currentBall.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (GripUp())
        {
            ReleaseAndThrow();
        }
    }

    public void SetBallType(int idx) => selectedType = (BallType)idx;
    public void SetBallType(BallType type) => selectedType = type;

    void SpawnBallAndStick()
    {
        var prefab = GetPrefab(selectedType);
        if (prefab == null)
        {
            Debug.LogWarning($"No hay prefab asignado para {selectedType}");
            return;
        }

        currentBall = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        currentBall.transform.SetParent(spawnPoint, true);

        var cap = currentBall.GetComponent<PokeballCapture>();
        if (cap) cap.ballType = selectedType;

        currentRb = currentBall.GetComponent<Rigidbody>();

        if (disablePhysicsWhileStuck && currentRb != null)
        {
            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
            currentRb.useGravity = false;
            currentRb.isKinematic = true;
        }
    }

    void ReleaseAndThrow()
    {
        if (currentBall == null) return;

        if (handAudio && throwClip)
            handAudio.PlayOneShot(throwClip, throwVolume);

        currentBall.transform.SetParent(null, true);

        if (currentRb != null)
        {
            if (disablePhysicsWhileStuck)
            {
                currentRb.isKinematic = false;
                currentRb.useGravity = true;
            }

            OVRInput.Controller c = rightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

            Vector3 v = OVRInput.GetLocalControllerVelocity(c) * throwVelocityMultiplier;
            Vector3 w = OVRInput.GetLocalControllerAngularVelocity(c) * throwAngularMultiplier;

            if (v.magnitude < minThrowSpeed)
                v = spawnPoint.forward * minThrowSpeed;

            if (forwardBoost != 0f)
                v += spawnPoint.forward * forwardBoost;

            if (aimAssist != null)
                v = aimAssist.ApplyAimAssist(v);

            currentRb.linearVelocity = v;
            currentRb.angularVelocity = w;
        }

        currentBall = null;
        currentRb = null;
    }

    GameObject GetPrefab(BallType type)
    {
        foreach (var b in balls)
            if (b.type == type) return b.prefab;
        return null;
    }

    bool GripHeld()
    {
        return rightHand
            ? OVRInput.Get(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
    }

    bool GripDown()
    {
        return rightHand
            ? OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
    }

    bool GripUp()
    {
        return rightHand
            ? OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger);
    }
}
```

Inicialización de variables

```cs
    [Header("Catalog")]
    public BallEntry[] balls;
    public BallType selectedType = BallType.Poke;

    [Header("Spawn")]
    public Transform spawnPoint;     
    public bool rightHand = true; 

    [Header("Hold behavior")]
    public bool stickToHandWhileHoldingGrip = true;
    public bool disablePhysicsWhileStuck = true;

    [Header("Throw tuning")]
    public float throwVelocityMultiplier = 1.25f;
    public float throwAngularMultiplier = 1.0f;
    public float minThrowSpeed = 0.15f;
    public float forwardBoost = 0.0f;

    [Header("Aim Assist (opcional)")]
    public AimAssistThrow aimAssist;

    [Header("Audio (Throw)")]
    public AudioSource handAudio;         
    public AudioClip throwClip;
    [Range(0f, 1f)] public float throwVolume = 1f;

    private GameObject currentBall;
    private Rigidbody currentRb;
```

Inicializa todas las variables necesarias, y permite arrastrar en el inspector lo necesario para ellos.

```cs
    void Update()
    {
        if (spawnPoint == null) return;

        if (GripDown())
        {
            if (currentBall == null)
                SpawnBallAndStick();
        }

        if (currentBall != null && stickToHandWhileHoldingGrip && GripHeld())
        {
            currentBall.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (GripUp())
        {
            ReleaseAndThrow();
        }
    }
```

Actualiza en tiempo real para poder lanzar la *Pokeball*

```cs
    public void SetBallType(int idx) => selectedType = (BallType)idx;
    public void SetBallType(BallType type) => selectedType = type;
```

Coloca el tipo de bola en la mano.

```cs
    void SpawnBallAndStick()
    {
        var prefab = GetPrefab(selectedType);
        if (prefab == null)
        {
            Debug.LogWarning($"No hay prefab asignado para {selectedType}");
            return;
        }

        currentBall = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        currentBall.transform.SetParent(spawnPoint, true);

        var cap = currentBall.GetComponent<PokeballCapture>();
        if (cap) cap.ballType = selectedType;

        currentRb = currentBall.GetComponent<Rigidbody>();

        if (disablePhysicsWhileStuck && currentRb != null)
        {
            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
            currentRb.useGravity = false;
            currentRb.isKinematic = true;
        }
    }
```
Junto al `BallSelectionMenu.cs`gestiona la bola que aparece y asigna el *prefab*.

```    void ReleaseAndThrow()
    {
        if (currentBall == null) return;

        if (handAudio && throwClip)
            handAudio.PlayOneShot(throwClip, throwVolume);

        currentBall.transform.SetParent(null, true);

        if (currentRb != null)
        {
            if (disablePhysicsWhileStuck)
            {
                currentRb.isKinematic = false;
                currentRb.useGravity = true;
            }

            OVRInput.Controller c = rightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

            Vector3 v = OVRInput.GetLocalControllerVelocity(c) * throwVelocityMultiplier;
            Vector3 w = OVRInput.GetLocalControllerAngularVelocity(c) * throwAngularMultiplier;

            if (v.magnitude < minThrowSpeed)
                v = spawnPoint.forward * minThrowSpeed;

            if (forwardBoost != 0f)
                v += spawnPoint.forward * forwardBoost;

            if (aimAssist != null)
                v = aimAssist.ApplyAimAssist(v);

            currentRb.linearVelocity = v;
            currentRb.angularVelocity = w;
        }

        currentBall = null;
        currentRb = null;
    }
```
Permite lanzar la *Pokeball*.

```cs
    GameObject GetPrefab(BallType type)
    {
        foreach (var b in balls)
            if (b.type == type) return b.prefab;
        return null;
    }
```
Obtiene el *Prefab* de la *Pokeball* según el tipo.

```cs
    bool GripHeld()
    {
        return rightHand
            ? OVRInput.Get(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
    }
```
Mantiene la rotación fija con la rotación local de la mano para hacer el seguimiento.

```cs
    bool GripDown()
    {
        return rightHand
            ? OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
    }
```

Pega la Pokeball selecionada a la mano

```cs
    bool GripUp()
    {
        return rightHand
            ? OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger);
    }
```

Suelta y reactiva las físicas a la Pokeball.

## MonsterRespawn

Genera los monstruos al rededor del jugador de forma aleatoria.

```cs
using UnityEngine;

public class MonsterRespawn : MonoBehaviour
{
    Vector3 originalPos;
    Quaternion originalRot;

    void Awake()
    {
        CaptureOriginNow();
    }

    public void CaptureOriginNow()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    public void HideForCapture()
    {
        gameObject.SetActive(false);
    }

    public void RespawnAtOrigin()
    {
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(originalPos, originalRot);
    }
}
```

```cs
    void Awake()
    {
        CaptureOriginNow();
    }
```
Inicializa el *Spawner* antes del Start, estando al inicio de la escena.

```cs
    public void CaptureOriginNow()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }
```
Actualiza manualmente el punto de *spawn* 

```cs
public void HideForCapture()
{
    gameObject.SetActive(false);
}
```
Desactiva al *Pokemón* al ser capturado.

```cs
public void RespawnAtOrigin()
{
    gameObject.SetActive(true);
    transform.SetPositionAndRotation(originalPos, originalRot);
}
```
Si la Captura falla, vuelve a aparecer el *Pokemón*.

### MonsterScanner

Escanea los *Pokemon* con la Retícula y muestra de forma visual que criatura es y si ha sido o no capturada previamente.
```cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MonsterScanner : MonoBehaviour
{
    [Header("Ray origin")]
    public Camera cam;
    public Transform fallbackOrigin;

    [Header("Detection")]
    public float scanDistance = 20f;
    public float sphereRadius = 0.08f;
    public LayerMask monsterLayers;
    public string monsterTag = "Monster";
    public bool includeTriggers = true;

    [Header("Scan timing")]
    public float holdSeconds = 1.0f;

    [Header("UI")]
    public TMP_Text scanText;
    public CanvasGroup canvasGroup;

    [Tooltip("Image donde se mostrar� el icono del tipo")]
    public Image typeImage;

    [Tooltip("Opcional: para ocultar el contenedor del icono")]
    public GameObject typeIconRoot;

    [Header("Type icons (asigna en inspector)")]
    public TypeIconMap[] typeIcons;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool debugDraw = true;

    float timer;
    PokemonIdentity current;
    bool lockedResult;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!fallbackOrigin) fallbackOrigin = transform;
        SetPanel(false);
        SetTypeIcon(null);
    }

    void Update()
    {
        if (!cam && !fallbackOrigin) return;

        Ray ray = BuildCenterRay();
        var qti = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        if (debugDraw)
            Debug.DrawRay(ray.origin, ray.direction * scanDistance, Color.cyan);

        bool hitSomething = Physics.SphereCast(
            ray, sphereRadius, out RaycastHit hit, scanDistance,
            monsterLayers.value != 0 ? monsterLayers : Physics.DefaultRaycastLayers,
            qti
        );

        if (!hitSomething)
        {
            ResetScan();
            return;
        }

        var id = hit.collider.GetComponentInParent<PokemonIdentity>();
        if (!id) id = hit.collider.GetComponentInChildren<PokemonIdentity>(true);

        bool tagOk = !string.IsNullOrEmpty(monsterTag) &&
                     (hit.collider.CompareTag(monsterTag) || hit.collider.transform.root.CompareTag(monsterTag));

        if (!id && !tagOk)
        {
            if (debugLogs)
                Debug.Log($"[Scanner] Hit '{hit.collider.name}' layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} tag={hit.collider.tag} (NO monster)");
            ResetScan();
            return;
        }

        if (!id)
        {
            if (debugLogs)
                Debug.LogWarning("[Scanner] Detecto Monster por tag pero NO encuentro PokemonIdentity. A��delo al prefab del monstruo (en root o hijo).");
            ResetScan();
            return;
        }

        if (debugLogs)
            Debug.Log($"[Scanner] Aiming at #{id.pokemonId:00} {id.pokemonName} type={id.primaryType} hit={hit.collider.name}");

        if (id != current)
        {
            current = id;
            timer = 0f;
            lockedResult = false;
        }

        if (!lockedResult)
        {
            timer += Time.deltaTime;

            SetPanel(true);
            if (scanText) scanText.text = $"Escaneando... {(timer / holdSeconds) * 100f:0}%";
            SetTypeIcon(null);

            if (timer >= holdSeconds)
            {
                ShowInfo(id);
                lockedResult = true;
            }
        }
    }

    Ray BuildCenterRay()
    {
        if (cam) return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return new Ray(fallbackOrigin.position, fallbackOrigin.forward);
    }

    void ShowInfo(PokemonIdentity id)
    {
        bool captured = (PokedexData.I != null && PokedexData.I.IsCaptured(id.pokemonId));

        if (scanText)
        {
            scanText.text =
                $"#{id.pokemonId:00} {id.pokemonName}\n" +
                (captured ? "Registrado" : "No registrado");
        }

        Sprite icon = GetTypeIcon(id.primaryType);
        SetTypeIcon(icon);

        SetPanel(true);
    }

    Sprite GetTypeIcon(PokemonType type)
    {
        if (typeIcons == null) return null;

        for (int i = 0; i < typeIcons.Length; i++)
        {
            if (typeIcons[i] != null && typeIcons[i].pokemonType == type)
                return typeIcons[i].icon;
        }

        return null;
    }

    void SetTypeIcon(Sprite s)
    {
        if (typeImage)
        {
            typeImage.sprite = s;
            typeImage.enabled = (s != null);
        }

        if (typeIconRoot)
            typeIconRoot.SetActive(s != null);
    }

    void ResetScan()
    {
        timer = 0f;
        current = null;
        lockedResult = false;
        SetPanel(false);
        SetTypeIcon(null);
    }

    void SetPanel(bool on)
    {
        if (canvasGroup) canvasGroup.alpha = on ? 1f : 0f;
    }
}
```
#### Inicialización de variables:
```cs
    [Header("Ray origin")]
    public Camera cam;
    public Transform fallbackOrigin;

    [Header("Detection")]
    public float scanDistance = 20f;
    public float sphereRadius = 0.08f;
    public LayerMask monsterLayers;
    public string monsterTag = "Monster";
    public bool includeTriggers = true;

    [Header("Scan timing")]
    public float holdSeconds = 1.0f;

    [Header("UI")]
    public TMP_Text scanText;
    public CanvasGroup canvasGroup;

    [Tooltip("Image donde se mostrara el icono del tipo")]
    public Image typeImage;

    [Tooltip("Opcional: para ocultar el contenedor del icono")]
    public GameObject typeIconRoot;

    [Header("Type icons (asigna en inspector)")]
    public TypeIconMap[] typeIcons;

    [Header("Debug")]
    public bool debugLogs = true;
    public bool debugDraw = true;

    float timer;
    PokemonIdentity current;
    bool lockedResult;
```
Se configuran los iconos que se usarán, el canvas, entre otros elementos.

```cs
    Ray BuildCenterRay()
    {
        if (cam) return cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return new Ray(fallbackOrigin.position, fallbackOrigin.forward);
    }
```

Crea un "rayo" en la cámara que servirá para el Scanner.

```cs
   void ShowInfo(PokemonIdentity id)
    {
        bool captured = (PokedexData.I != null && PokedexData.I.IsCaptured(id.pokemonId));

        if (scanText)
        {
            scanText.text =
                $"#{id.pokemonId:00} {id.pokemonName}\n" +
                (captured ? "Registrado" : "No registrado");
        }

        Sprite icon = GetTypeIcon(id.primaryType);
        SetTypeIcon(icon);

        SetPanel(true);
    }
```

Muestra la Información del *Pokemon*

```cs
    Sprite GetTypeIcon(PokemonType type)
    {
        if (typeIcons == null) return null;

        for (int i = 0; i < typeIcons.Length; i++)
        {
            if (typeIcons[i] != null && typeIcons[i].pokemonType == type)
                return typeIcons[i].icon;
        }

        return null;
    }

    void SetTypeIcon(Sprite s)
    {
        if (typeImage)
        {
            typeImage.sprite = s;
            typeImage.enabled = (s != null);
        }

        if (typeIconRoot)
            typeIconRoot.SetActive(s != null);
    }

    void ResetScan()
    {
        timer = 0f;
        current = null;
        lockedResult = false;
        SetPanel(false);
        SetTypeIcon(null);
    }

    void SetPanel(bool on)
    {
        if (canvasGroup) canvasGroup.alpha = on ? 1f : 0f;
    }
```
Gestiona toda la información de la UI.

### OneShotAudio3D

Genera un audio corto en 3D, para los *SFX*.

`OneShotAudio3D`
```cs
using UnityEngine;

public static class OneShotAudio3D
{
    public static void Play(AudioClip clip, Vector3 pos, float volume = 1f, float minDist = 0.3f, float maxDist = 15f)
    {
        if (!clip) return;

        var go = new GameObject("OneShotAudio3D_" + clip.name);
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        src.playOnAwake = false;
        src.volume = volume;
        src.minDistance = minDist;
        src.maxDistance = maxDist;
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;

        src.clip = clip;
        src.Play();

        Object.Destroy(go, clip.length + 0.1f);
    }
}
```
Gestiona todo el sonido y reproduce un sonido según la distancia, y del efecto que se quiera transmitir.

### PokeballAutoDestroy

Genera la autodestrucción de la Pokeball para no sobrecargar la escena con Pokeballs.

`PokeballAutoDestroy.cs`
```cs
using UnityEngine;

public class PokeballAutoDestroy : MonoBehaviour
{
    [Header("Destroy conditions")]
    [Tooltip("Se destruye pase lo que pase tras X segundos desde que spawnea. 0 = desactivado")]
    public float destroyAfterSeconds = 0f; 

    [Tooltip("Se destruye al colisionar con objetos de estas layers")]
    public LayerMask groundLayers;

    [Tooltip("Alternativa a layers: si no est� vac�o, se considera suelo si toca un objeto con este tag")]
    public string groundTag = "Ground";

    [Header("On ground behavior")]
    [Tooltip("Tiempo tras tocar suelo antes de destruirse")]
    public float destroyDelayAfterGroundHit = 12.0f;

    [Tooltip("Evita dobles destrucciones")]
    public bool onlyFirstGroundHit = true;

    [Header("Debug")]
    public bool debugLogs = false;

    bool groundHitTriggered;
    float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        if (destroyAfterSeconds > 0f)
        {
            if (debugLogs) Debug.Log($"[AutoDestroy] Scheduled from spawn: {destroyAfterSeconds}s ({name})");
            Destroy(gameObject, destroyAfterSeconds);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if (groundHitTriggered && onlyFirstGroundHit) return;

        if (!IsGround(collision.gameObject)) return;

        groundHitTriggered = true;

 

        float desired = destroyDelayAfterGroundHit;

        if (desired <= 0f)
        {
            if (debugLogs) Debug.Log($"[AutoDestroy] Destroy NOW on ground hit ({name})");
            Destroy(gameObject);
            return;
        }

        if (debugLogs)
        {
            float timeAlive = Time.time - spawnTime;
            Debug.Log($"[AutoDestroy] Ground hit at t={timeAlive:F2}s -> destroy in {desired:F2}s ({name})");
        }

        Destroy(gameObject, desired);
    }

    bool IsGround(GameObject other)
    {
        if (!string.IsNullOrEmpty(groundTag) && other.CompareTag(groundTag))
            return true;

        if (groundLayers.value != 0)
        {
            int bit = 1 << other.layer;
            if ((groundLayers.value & bit) != 0) return true;
        }

        return false;
    }
}
```

#### Inicialización de las variables y *Headers*

```cs
    [Header("Destroy conditions")]
    [Tooltip("Se destruye pase lo que pase tras X segundos desde que spawnea. 0 = desactivado")]
    public float destroyAfterSeconds = 0f; 

    [Tooltip("Se destruye al colisionar con objetos de estas layers")]
    public LayerMask groundLayers;

    [Tooltip("Alternativa a layers: si no est� vac�o, se considera suelo si toca un objeto con este tag")]
    public string groundTag = "Ground";

    [Header("On ground behavior")]
    [Tooltip("Tiempo tras tocar suelo antes de destruirse")]
    public float destroyDelayAfterGroundHit = 12.0f;

    [Tooltip("Evita dobles destrucciones")]
    public bool onlyFirstGroundHit = true;

    [Header("Debug")]
    public bool debugLogs = false;

    bool groundHitTriggered;
    float spawnTime;
``` 

Despues de todas las configuraciones e inicializaciones alguna por inspector:

```cs
    void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if (groundHitTriggered && onlyFirstGroundHit) return;

        if (!IsGround(collision.gameObject)) return;

        groundHitTriggered = true;

 

        float desired = destroyDelayAfterGroundHit;

        if (desired <= 0f)
        {
            if (debugLogs) Debug.Log($"[AutoDestroy] Destroy NOW on ground hit ({name})");
            Destroy(gameObject);
            return;
        }

        if (debugLogs)
        {
            float timeAlive = Time.time - spawnTime;
            Debug.Log($"[AutoDestroy] Ground hit at t={timeAlive:F2}s -> destroy in {desired:F2}s ({name})");
        }

        Destroy(gameObject, desired);
    }
```
Gestiona la autodestrucción con colisión en el suelo. Lo desactive porque me daba problemas. También detecta el tiempo en el que ha estado.

```cs
    bool IsGround(GameObject other)
    {
        if (!string.IsNullOrEmpty(groundTag) && other.CompareTag(groundTag))
            return true;

        if (groundLayers.value != 0)
        {
            int bit = 1 << other.layer;
            if ((groundLayers.value & bit) != 0) return true;
        }

        return false;
    }
```

Detecta si el objeto con el que colisiona, es suelo o no.

### PokeballCapture

Gestiona como se captura la *Pokeball*.

`PokeballCapture.cs`
```cs
using System.Collections;
using UnityEngine;

public class PokeballCapture : MonoBehaviour
{
    [Header("Ball info (lo asigna el spawner al instanciar)")]
    public BallType ballType = BallType.Poke;

    [Header("Target ID")]
    public string monsterTag = "Monster";
    public LayerMask monsterLayers;

    [Header("Ground (para clavar en el suelo)")]
    public LayerMask groundLayers;
    public float groundRayUp = 2.0f;
    public float groundRayDown = 6.0f;
    public float groundOffset = 0.02f;

    [Header("Look at player")]
    public Transform player;
    public bool facePlayerOnStick = true;

    [Header("Chances")]
    [Range(0f, 1f)] public float pokeChance = 0.10f;
    [Range(0f, 1f)] public float safariChance = 0.25f;
    [Range(0f, 1f)] public float ultraChance = 0.60f;
    [Range(0f, 1f)] public float masterChance = 1.00f;

    [Header("Shake animation")]
    public float settleDelay = 0.05f;
    public float shakeDuration = 0.30f;
    public float shakeAngle = 40f;
    public float betweenShakes = 0.25f;
    public float finalDelay = 0.20f;

    [Header("Destroy timing")]
    public float destroyAfterSuccess = 3.2f;
    public float destroyAfterFail = 1.6f;

    [Header("Audio (3D) - en el prefab")]
    public AudioSource audioSource;
    public AudioClip hitMonsterClip;
    public AudioClip wobbleClip;
    public AudioClip captureClip;
    public AudioClip failClip;

    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 4f)] public float captureVolume = 1.0f;

    [Header("Pokédex popup (opcional)")]
    public PokedexToast pokedexToast;

    bool resolving;
    Rigidbody rb;
    Collider[] myColliders;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myColliders = GetComponentsInChildren<Collider>(true);

        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;    
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 0.25f;
            audioSource.maxDistance = 25f;
        }

        if (!player && Camera.main) player = Camera.main.transform;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (resolving) return;
        if (!IsMonster(collision.gameObject, collision.collider)) return;

        resolving = true;

        PlaySfx(hitMonsterClip, sfxVolume);

        GameObject monsterGO = collision.collider.transform.root.gameObject;
        Vector3 contactPoint = collision.GetContact(0).point;

        StartCoroutine(CaptureSequence(monsterGO, contactPoint));
    }

    IEnumerator CaptureSequence(GameObject monsterGO, Vector3 contactPoint)
    {
        var auto = GetComponent<PokeballAutoDestroy>();
        if (auto) auto.enabled = false;
        Vector3 stickPos = FindGroundPosition(contactPoint);
        Quaternion stickRot = facePlayerOnStick ? FacingPlayerRotation(stickPos) : transform.rotation;
        StickBallAt(stickPos, stickRot);
        MonsterRespawn resp = monsterGO.GetComponent<MonsterRespawn>();
        if (!resp) resp = monsterGO.AddComponent<MonsterRespawn>();
        resp.CaptureOriginNow();
        resp.HideForCapture();

        yield return new WaitForSeconds(settleDelay);

        float chance = GetChance(ballType);
        bool success = Random.value <= chance;

        int shakes = success ? 3 : 1;
        yield return StartCoroutine(DoShakes(shakes));

        yield return new WaitForSeconds(finalDelay);

        if (success)
        {
            PlaySfx(captureClip, sfxVolume * captureVolume);

            var identity = monsterGO.GetComponentInChildren<PokemonIdentity>(true);
            if (identity && PokedexData.I)
            {
                bool already = PokedexData.I.IsCaptured(identity.pokemonId);
                PokedexData.I.RegisterCapture(identity.pokemonId);

                if (!pokedexToast) pokedexToast = FindObjectOfType<PokedexToast>();
                if (pokedexToast)
                    pokedexToast.Show(already ? "Ya registrado en la Pokédex" : "Registrado en la Pokédex");
            }

            Destroy(monsterGO, destroyAfterSuccess);
            Destroy(gameObject, destroyAfterSuccess);
        }
        else
        {
            PlaySfx(failClip, sfxVolume);

            resp.RespawnAtOrigin();
            Destroy(gameObject, destroyAfterFail);
        }
    }

    void PlaySfx(AudioClip clip, float volume)
    {
        if (!clip || !audioSource) return;
        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    Vector3 FindGroundPosition(Vector3 nearPoint)
    {
        int mask = (groundLayers.value != 0) ? groundLayers.value : Physics.DefaultRaycastLayers;

        Vector3 rayStart = nearPoint + Vector3.up * groundRayUp;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit,
            groundRayUp + groundRayDown, mask, QueryTriggerInteraction.Ignore))
            return hit.point + Vector3.up * groundOffset;

        return nearPoint;
    }

    Quaternion FacingPlayerRotation(Vector3 fromPos)
    {
        if (!player) return transform.rotation;

        Vector3 dir = player.position - fromPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return transform.rotation;

        return Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    void StickBallAt(Vector3 worldPos, Quaternion worldRot)
    {
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;           
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (myColliders != null)
            foreach (var c in myColliders) if (c) c.enabled = false;

        transform.SetParent(null, true);
        transform.SetPositionAndRotation(worldPos, worldRot);
    }

    IEnumerator DoShakes(int count)
    {
        Quaternion baseRot = transform.rotation;

        for (int i = 0; i < count; i++)
        {
            PlaySfx(wobbleClip, sfxVolume);

            float t = 0f;
            while (t < shakeDuration)
            {
                t += Time.deltaTime;
                float s = Mathf.Sin((t / shakeDuration) * Mathf.PI * 2f);
                float angle = s * shakeAngle;

                transform.rotation = baseRot * Quaternion.Euler(0f, angle, 0f);
                yield return null;
            }

            transform.rotation = baseRot;

            if (i < count - 1)
                yield return new WaitForSeconds(betweenShakes);
        }
    }

    float GetChance(BallType type)
    {
        switch (type)
        {
            case BallType.Poke: return pokeChance;
            case BallType.Safari: return safariChance;
            case BallType.Ultra: return ultraChance;
            case BallType.Master: return masterChance;
            default: return pokeChance;
        }
    }

    bool IsMonster(GameObject other, Collider hitCollider)
    {
        if (monsterLayers.value != 0)
        {
            int bit = 1 << other.layer;
            if ((monsterLayers.value & bit) != 0) return true;

            GameObject root = hitCollider ? hitCollider.transform.root.gameObject : other.transform.root.gameObject;
            int rootBit = 1 << root.layer;
            if ((monsterLayers.value & rootBit) != 0) return true;
        }

        if (!string.IsNullOrEmpty(monsterTag))
        {
            if (other.CompareTag(monsterTag)) return true;
            if (hitCollider && hitCollider.transform.root.CompareTag(monsterTag)) return true;
        }

        return false;
    }
}
```

#### Inicialización

```cs
    [Header("Ball info (lo asigna el spawner al instanciar)")]
    public BallType ballType = BallType.Poke;

    [Header("Target ID")]
    public string monsterTag = "Monster";
    public LayerMask monsterLayers;

    [Header("Ground (para clavar en el suelo)")]
    public LayerMask groundLayers;
    public float groundRayUp = 2.0f;
    public float groundRayDown = 6.0f;
    public float groundOffset = 0.02f;

    [Header("Look at player")]
    public Transform player;
    public bool facePlayerOnStick = true;

    [Header("Chances")]
    [Range(0f, 1f)] public float pokeChance = 0.10f;
    [Range(0f, 1f)] public float safariChance = 0.25f;
    [Range(0f, 1f)] public float ultraChance = 0.60f;
    [Range(0f, 1f)] public float masterChance = 1.00f;

    [Header("Shake animation")]
    public float settleDelay = 0.05f;
    public float shakeDuration = 0.30f;
    public float shakeAngle = 40f;
    public float betweenShakes = 0.25f;
    public float finalDelay = 0.20f;

    [Header("Destroy timing")]
    public float destroyAfterSuccess = 3.2f;
    public float destroyAfterFail = 1.6f;

    [Header("Audio (3D) - en el prefab")]
    public AudioSource audioSource;
    public AudioClip hitMonsterClip;
    public AudioClip wobbleClip;
    public AudioClip captureClip;
    public AudioClip failClip;

    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 4f)] public float captureVolume = 1.0f;

    [Header("Pokédex popup (opcional)")]
    public PokedexToast pokedexToast;

    bool resolving;
    Rigidbody rb;
    Collider[] myColliders;
```

Después de la inicialización:
```cs
        IEnumerator CaptureSequence(GameObject monsterGO, Vector3 contactPoint)
        {
            var auto = GetComponent<PokeballAutoDestroy>();
            if (auto) auto.enabled = false;
            Vector3 stickPos = FindGroundPosition(contactPoint);
            Quaternion stickRot = facePlayerOnStick ? FacingPlayerRotation(stickPos) : transform.rotation;
            StickBallAt(stickPos, stickRot);
            MonsterRespawn resp = monsterGO.GetComponent<MonsterRespawn>();
            if (!resp) resp = monsterGO.AddComponent<MonsterRespawn>();
            resp.CaptureOriginNow();
            resp.HideForCapture();

            yield return new WaitForSeconds(settleDelay);

            float chance = GetChance(ballType);
            bool success = Random.value <= chance;

            int shakes = success ? 3 : 1;
            yield return StartCoroutine(DoShakes(shakes));

            yield return new WaitForSeconds(finalDelay);

            if (success)
            {
                PlaySfx(captureClip, sfxVolume * captureVolume);

                var identity = monsterGO.GetComponentInChildren<PokemonIdentity>(true);
                if (identity && PokedexData.I)
                {
                    bool already = PokedexData.I.IsCaptured(identity.pokemonId);
                    PokedexData.I.RegisterCapture(identity.pokemonId);

                    if (!pokedexToast) pokedexToast = FindObjectOfType<PokedexToast>();
                    if (pokedexToast)
                        pokedexToast.Show(already ? "Ya registrado en la Pokédex" : "Registrado en la Pokédex");
                }

                Destroy(monsterGO, destroyAfterSuccess);
                Destroy(gameObject, destroyAfterSuccess);
            }
            else
            {
                PlaySfx(failClip, sfxVolume);

                resp.RespawnAtOrigin();
                Destroy(gameObject, destroyAfterFail);
            }
        }
```

Hace una captura de secuencia según si se ha capturado o no el Pokemón.

```cs
        void PlaySfx(AudioClip clip, float volume)
        {
            if (!clip || !audioSource) return;
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
```

Reproduce los efectos especiales.

```cs
        Vector3 FindGroundPosition(Vector3 nearPoint)
        {
            int mask = (groundLayers.value != 0) ? groundLayers.value : Physics.DefaultRaycastLayers;

            Vector3 rayStart = nearPoint + Vector3.up * groundRayUp;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit,
                groundRayUp + groundRayDown, mask, QueryTriggerInteraction.Ignore))
                return hit.point + Vector3.up * groundOffset;

            return nearPoint;
        }
```

Encuentra la posición del suelo y coloca la *pokeball* así.

```cs
        Quaternion FacingPlayerRotation(Vector3 fromPos)
        {
            if (!player) return transform.rotation;

            Vector3 dir = player.position - fromPos;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return transform.rotation;

            return Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
```

Orienta la *Pokeball* hacia el jugador.

```cs
        void StickBallAt(Vector3 worldPos, Quaternion worldRot)
        {
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;           
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            if (myColliders != null)
                foreach (var c in myColliders) if (c) c.enabled = false;

            transform.SetParent(null, true);
            transform.SetPositionAndRotation(worldPos, worldRot);
        }
```

"Clava" la pokeball al suelo.

```cs
        IEnumerator DoShakes(int count)
        {
            Quaternion baseRot = transform.rotation;

            for (int i = 0; i < count; i++)
            {
                PlaySfx(wobbleClip, sfxVolume);

                float t = 0f;
                while (t < shakeDuration)
                {
                    t += Time.deltaTime;
                    float s = Mathf.Sin((t / shakeDuration) * Mathf.PI * 2f);
                    float angle = s * shakeAngle;

                    transform.rotation = baseRot * Quaternion.Euler(0f, angle, 0f);
                    yield return null;
                }

                transform.rotation = baseRot;

                if (i < count - 1)
                    yield return new WaitForSeconds(betweenShakes);
            }
        }
```
Hace la "animación" de agitarse.

```cs
        float GetChance(BallType type)
        {
            switch (type)
            {
                case BallType.Poke: return pokeChance;
                case BallType.Safari: return safariChance;
                case BallType.Ultra: return ultraChance;
                case BallType.Master: return masterChance;
                default: return pokeChance;
            }
        }
```
Obtiene la probabilidad de acierto a la hora de capturar.

```cs
        bool IsMonster(GameObject other, Collider hitCollider)
        {
            if (monsterLayers.value != 0)
            {
                int bit = 1 << other.layer;
                if ((monsterLayers.value & bit) != 0) return true;

                GameObject root = hitCollider ? hitCollider.transform.root.gameObject : other.transform.root.gameObject;
                int rootBit = 1 << root.layer;
                if ((monsterLayers.value & rootBit) != 0) return true;
            }

            if (!string.IsNullOrEmpty(monsterTag))
            {
                if (other.CompareTag(monsterTag)) return true;
                if (hitCollider && hitCollider.transform.root.CompareTag(monsterTag)) return true;
            }

            return false;
        }
```

Comprueba que la colisión es un *Pokemón*.

### Pokedex Data

Es el sistema uqe tiene los datos de forma persistente.
```cs
using UnityEngine;

public class PokedexData : MonoBehaviour
{
    public static PokedexData I;

    [Header("IDs 1..10. El �ndice 0 se ignora.")]
    public bool[] captured = new bool[11];

    const string PrefKey = "POKEDEX_CAPTURED_MASK"; 

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            ResetAll();
            Debug.Log("[PokedexData] PlayerPrefs borrado y Pok�dex reseteada.");
        }
    }
    public void RegisterCapture(int id)
    {
        if (id < 1 || id > 10) return;

        bool was = captured[id];
        captured[id] = true;

        if (!was) Save(); 
    }

    public bool IsCaptured(int id)
    {
        return id >= 1 && id <= 10 && captured[id];
    }

    public int CapturedCount()
    {
        int c = 0;
        for (int id = 1; id <= 10; id++)
            if (captured[id]) c++;
        return c;
    }

    public void Save()
    {
        int mask = 0;
        for (int id = 1; id <= 10; id++)
            if (captured[id]) mask |= (1 << id);

        PlayerPrefs.SetInt(PrefKey, mask);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        int mask = PlayerPrefs.GetInt(PrefKey, 0);

        for (int id = 1; id <= 10; id++)
            captured[id] = (mask & (1 << id)) != 0;
    }

    public void ResetAll()
    {
        for (int id = 1; id <= 10; id++) captured[id] = false;
        PlayerPrefs.DeleteKey("POKEDEX_CAPTURED_MASK");
        PlayerPrefs.Save();
    }
}
```

Acontinuación analizamos:
```cs
    public void RegisterCapture(int id)
    {
        if (id < 1 || id > 10) return;

        bool was = captured[id];
        captured[id] = true;

        if (!was) Save(); 
    }
```

Registra la captura si está en la lista de *Pokemon* y no estaba de antes.

```cs
    public bool IsCaptured(int id)
    {
        return id >= 1 && id <= 10 && captured[id];
    }
```
Devuelve si ha sido o no capturado.

```cs
    public int CapturedCount()
    {
        int c = 0;
        for (int id = 1; id <= 10; id++)
            if (captured[id]) c++;
        return c;
    }
```

Sigue la cantidad de *Pokemon* capturados (de la lista o sea 1/10, 6/10...)


```cs
    public void Save()
    {
        int mask = 0;
        for (int id = 1; id <= 10; id++)
            if (captured[id]) mask |= (1 << id);

        PlayerPrefs.SetInt(PrefKey, mask);
        PlayerPrefs.Save();
    }
```

Guarda los nuevos capturados.

```cs
    public void Load()
    {
        int mask = PlayerPrefs.GetInt(PrefKey, 0);

        for (int id = 1; id <= 10; id++)
            captured[id] = (mask & (1 << id)) != 0;
    }
```

carga la lista de Pokemons de PlayerPrefs.

```cs
    public void ResetAll()
    {
        for (int id = 1; id <= 10; id++) captured[id] = false;
        PlayerPrefs.DeleteKey("POKEDEX_CAPTURED_MASK");
        PlayerPrefs.Save();
    }
```

Borra todos los pokemons capturados.

### PokedexListItems

Representa un elemento individual de la lista de la *Pokedex*

```cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokedexListItem : MonoBehaviour
{
    public TMP_Text label;
    public Image lockIcon;      
    public Image typeIcon;      

    public void SetCaptured(int pokemonId, string name, bool captured, Sprite typeSprite = null)
    {
        if (label)
        {
            if (captured) label.text = $"{pokemonId:00}  {name}";
            else label.text = $"{pokemonId:00}  ???";
        }

        if (lockIcon) lockIcon.enabled = !captured;

        if (typeIcon)
        {
            typeIcon.sprite = (captured ? typeSprite : null);
            typeIcon.enabled = (captured && typeSprite != null);
        }
    }
}

```

SetCaptured lo que hace es cargar los *Pokemon* y actualiza la información de los que si han sido capturados.