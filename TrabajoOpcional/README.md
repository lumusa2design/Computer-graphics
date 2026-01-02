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