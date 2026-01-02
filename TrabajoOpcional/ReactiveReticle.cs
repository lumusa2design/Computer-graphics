using UnityEngine;
using UnityEngine.UI;

public class ReactiveReticle : MonoBehaviour
{
    [Header("UI")]
    public RectTransform pulseCircle;
    public Graphic pulseGraphic;
    public Graphic outerGraphic;

    [Header("Raycast")]
    public Transform rayOrigin;
    public float maxDistance = 30f;
    public LayerMask monsterLayers;
    public string monsterTag = "Monster";

    [Header("Ball source")]
    public GrabSpawnBall spawner;

    [Header("Pulse normal")]
    public float normalMinScale = 0.85f;
    public float normalMaxScale = 1.15f;
    public float normalSpeed = 2.2f;

    [Header("Pulse apuntando a monstruo")]
    public float targetMinScale = 0.95f;
    public float targetMaxScale = 1.35f;
    public float targetSpeed = 4.2f;

    [Header("Apariencia exterior")]
    public Color outerFixedColor = Color.black;
    public float outerAlpha = 1f;

    [Header("Smoothing")]
    public float smooth = 12f;


    public float AimAssist01 { get; private set; }

    Vector3 baseScale;
    float curMin, curMax, curSpeed;
    Color curPulseColor;

    void Start()
    {
        if (!rayOrigin)
            rayOrigin = Camera.main ? Camera.main.transform : null;

        if (pulseCircle)
            baseScale = pulseCircle.localScale;

        curMin = normalMinScale;
        curMax = normalMaxScale;
        curSpeed = normalSpeed;
        curPulseColor = Color.white;

        ApplyOuterBlack();
    }

    void Update()
    {
        if (!pulseCircle || !rayOrigin) return;

        bool aimingMonster = CheckMonsterAim();

        Color ballColor = GetColorFromSelectedBall();

        float tMin = aimingMonster ? targetMinScale : normalMinScale;
        float tMax = aimingMonster ? targetMaxScale : normalMaxScale;
        float tSpeed = aimingMonster ? targetSpeed : normalSpeed;

        Color tPulseColor = aimingMonster ? (ballColor * 1.15f) : ballColor;

        float k = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        curMin = Mathf.Lerp(curMin, tMin, k);
        curMax = Mathf.Lerp(curMax, tMax, k);
        curSpeed = Mathf.Lerp(curSpeed, tSpeed, k);
        curPulseColor = Color.Lerp(curPulseColor, tPulseColor, k);

        // pulso
        float s01 = (Mathf.Sin(Time.time * curSpeed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(curMin, curMax, s01);
        pulseCircle.localScale = baseScale * scale;


        AimAssist01 = Mathf.InverseLerp(curMax, curMin, scale); 

        // color pulse
        if (pulseGraphic)
        {
            var c = curPulseColor; c.a = 1f;
            pulseGraphic.color = c;
        }

        ApplyOuterBlack();
    }

    void ApplyOuterBlack()
    {
        if (!outerGraphic) return;
        var c = outerFixedColor; c.a = outerAlpha;
        outerGraphic.color = c;
    }

    Color GetColorFromSelectedBall()
    {
        BallType t = spawner ? spawner.selectedType : BallType.Poke;

        switch (t)
        {
            case BallType.Poke: return new Color(1f, 0.15f, 0.15f, 1f); 
            case BallType.Safari: return new Color(1f, 0.55f, 0.15f, 1f); 
            case BallType.Ultra: return new Color(1f, 0.92f, 0.20f, 1f); 
            case BallType.Master: return new Color(0.20f, 1f, 0.35f, 1f); 
            default: return Color.white;
        }
    }

    bool CheckMonsterAim()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (monsterLayers.value != 0)
            return Physics.Raycast(ray, maxDistance, monsterLayers, QueryTriggerInteraction.Ignore);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return !string.IsNullOrEmpty(monsterTag) && hit.collider.CompareTag(monsterTag);

        return false;
    }
}
