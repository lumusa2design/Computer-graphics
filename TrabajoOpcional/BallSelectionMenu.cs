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
    [Tooltip("Multiplicador para la opción seleccionada (1.15 recomendado)")]
    public float selectedScale = 1.15f;

    [Tooltip("Multiplicador para opciones no seleccionadas (1.0 recomendado)")]
    public float normalScale = 1.0f;

    [Header("Input")]
    public float deadzone = 0.6f;
    public float repeatDelay = 0.25f;

    [Header("Behavior")]
    [Tooltip("Si está activo, no hace falta pulsar X: se aplica al mover la palanca")]
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
