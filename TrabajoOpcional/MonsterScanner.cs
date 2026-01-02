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

    [Tooltip("Image donde se mostrará el icono del tipo")]
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
                Debug.LogWarning("[Scanner] Detecto Monster por tag pero NO encuentro PokemonIdentity. Añádelo al prefab del monstruo (en root o hijo).");
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
