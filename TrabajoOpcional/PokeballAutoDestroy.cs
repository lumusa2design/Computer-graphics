using UnityEngine;

public class PokeballAutoDestroy : MonoBehaviour
{
    [Header("Destroy conditions")]
    [Tooltip("Se destruye pase lo que pase tras X segundos desde que spawnea. 0 = desactivado")]
    public float destroyAfterSeconds = 0f; 

    [Tooltip("Se destruye al colisionar con objetos de estas layers")]
    public LayerMask groundLayers;

    [Tooltip("Alternativa a layers: si no está vacío, se considera suelo si toca un objeto con este tag")]
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
