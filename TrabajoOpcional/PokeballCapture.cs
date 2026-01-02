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
