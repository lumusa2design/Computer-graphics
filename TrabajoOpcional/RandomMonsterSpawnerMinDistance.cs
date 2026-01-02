using UnityEngine;

public class RandomMonsterSpawnerMinDistance : MonoBehaviour
{
    [Header("References")]
    public Transform player;                
    public Transform spawnCenter;           

    [Header("Monsters")]
    public GameObject[] monsterPrefabs;
    public int maxAlive = 3;
    public string monsterTag = "Monster";     

    [Header("Spawn Area")]
    public float areaRadius = 12f;
    public float minDistanceFromPlayer = 4f;

    [Header("Ground")]
    public float rayStartHeight = 8f;
    public float rayDistance = 30f;
    public LayerMask groundLayers;          

    [Header("Timing")]
    public float spawnEverySeconds = 3f;
    public float firstSpawnDelay = 0.5f;

    [Header("Attempts")]
    public int maxAttemptsPerSpawn = 30;

    [Header("Debug")]
    public bool debugLogs = true;

    float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + firstSpawnDelay;

        if (debugLogs)
        {
            Debug.Log($"[Spawner] Start. player={(player ? player.name : "NULL")} prefabs={(monsterPrefabs!=null ? monsterPrefabs.Length : 0)} groundMask={groundLayers.value}");
        }
    }

    void Update()
    {
        if (!player)
        {
            if (debugLogs) Debug.LogWarning("[Spawner] player is NULL. Asigna la cámara/CenterEyeAnchor en el inspector.");
            return;
        }

        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("[Spawner] monsterPrefabs vacío. Arrastra tus prefabs al array.");
            return;
        }

        if (Time.time < nextSpawnTime) return;

        int alive = CountAliveByTag(monsterTag);
        if (debugLogs) Debug.Log($"[Spawner] alive={alive}/{maxAlive}");

        if (alive < maxAlive)
        {
            bool spawned = TrySpawnOne();
            if (debugLogs) Debug.Log($"[Spawner] TrySpawnOne -> {spawned}");
        }

        nextSpawnTime = Time.time + spawnEverySeconds;
    }

    int CountAliveByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return 0;

        try { return GameObject.FindGameObjectsWithTag(tag).Length; }
        catch { return 0; }
    }

    bool TrySpawnOne()
    {
        Vector3 center = spawnCenter ? spawnCenter.position : transform.position;

        for (int attempt = 0; attempt < maxAttemptsPerSpawn; attempt++)
        {

            Vector2 r = Random.insideUnitCircle * areaRadius;
            Vector3 candidateXZ = new Vector3(center.x + r.x, center.y, center.z + r.y);

            Vector3 p = player.position;
            float dist = Vector2.Distance(new Vector2(candidateXZ.x, candidateXZ.z), new Vector2(p.x, p.z));
            if (dist < minDistanceFromPlayer) continue;

            Vector3 rayStart = candidateXZ + Vector3.up * rayStartHeight;

            bool useMask = groundLayers.value != 0;
            int mask = useMask ? groundLayers.value : Physics.DefaultRaycastLayers;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, mask, QueryTriggerInteraction.Ignore))
            {
                Vector3 spawnPos = hit.point;

                GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];

                Vector3 dir = player.position - spawnPos;
                dir.y = 0f;
                Quaternion rot = (dir.sqrMagnitude > 0.001f) ? Quaternion.LookRotation(dir) : Quaternion.identity;

                GameObject m = Instantiate(prefab, spawnPos, rot);

                if (!string.IsNullOrEmpty(monsterTag))
                {
                    try { m.tag = monsterTag; } catch {  }
                }

                if (debugLogs) Debug.Log($"[Spawner] Spawn OK: {m.name} at {spawnPos} (attempt {attempt}) dist={dist:F2}");
                return true;
            }
        }

        if (debugLogs) Debug.LogWarning("[Spawner] No encontró suelo válido. Revisa groundLayers o colliders del suelo.");
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = spawnCenter ? spawnCenter.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, areaRadius);

        if (player)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }
}
