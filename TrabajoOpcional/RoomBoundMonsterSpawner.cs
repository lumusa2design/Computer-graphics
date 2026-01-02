using UnityEngine;
using Meta.XR.MRUtilityKit;

public class RoomBoundMonsterSpawner : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Monsters")]
    public GameObject[] monsterPrefabs;
    public int maxAlive = 3;
    public string monsterTag = "Monster";

    [Header("Spawn rules")]
    public float minDistanceFromPlayer = 1f;
    public float spawnEverySeconds = 2f;
    public int maxAttemptsPerSpawn = 40;

    [Header("Grounding")]
    public float rayStartHeight = 2.0f;
    public float rayDistance = 6.0f;
    public LayerMask groundLayers; 

    float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + 0.5f;
    }

    void Update()
    {
        if (!player || monsterPrefabs == null || monsterPrefabs.Length == 0) return;
        if (Time.time < nextSpawnTime) return;

        int alive = CountAliveByTag(monsterTag);
        if (alive < maxAlive)
            TrySpawnOneInRoom();

        nextSpawnTime = Time.time + spawnEverySeconds;
    }

    int CountAliveByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return 0;
        try { return GameObject.FindGameObjectsWithTag(tag).Length; }
        catch { return 0; }
    }

    void TrySpawnOneInRoom()
    {
        if (MRUK.Instance == null || MRUK.Instance.GetCurrentRoom() == null) return;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();

        Bounds b = room.GetRoomBounds();

        for (int attempt = 0; attempt < maxAttemptsPerSpawn; attempt++)
        {
            Vector3 p = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y),
                Random.Range(b.min.z, b.max.z)
            );

            if (!room.IsPositionInRoom(p, true)) continue;

            float dist = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(player.position.x, player.position.z));
            if (dist < minDistanceFromPlayer) continue;

            Vector3 rayStart = p + Vector3.up * rayStartHeight;
            int mask = (groundLayers.value != 0) ? groundLayers.value : Physics.DefaultRaycastLayers;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, mask, QueryTriggerInteraction.Ignore))
            {
                Vector3 spawnPos = hit.point;

                Vector3 dir = player.position - spawnPos; dir.y = 0f;
                Quaternion rot = (dir.sqrMagnitude > 0.0001f) ? Quaternion.LookRotation(dir) : Quaternion.identity;

                GameObject prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
                GameObject m = Instantiate(prefab, spawnPos, rot);

                if (!string.IsNullOrEmpty(monsterTag))
                {
                    try { m.tag = monsterTag; } catch { }
                }
                return;
            }
        }
    }
}
