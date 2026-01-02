using UnityEngine;
using UnityEngine.UI;

public class ReticlePulse : MonoBehaviour
{
    [Header("Assign")]
    public RectTransform pulseCircle;     
    public RectTransform outerCircle;

    [Header("Pulse")]
    public float minScale = 0.85f;
    public float maxScale = 1.15f;
    public float speed = 2.5f; 

    [Header("Optional alpha pulse")]
    public Graphic pulseGraphic;  
    public float minAlpha = 0.35f;
    public float maxAlpha = 1.0f;

    Vector3 baseScale;

    void Start()
    {
        if (pulseCircle) baseScale = pulseCircle.localScale;
    }

    void Update()
    {
        if (!pulseCircle) return;

        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; 
        float s = Mathf.Lerp(minScale, maxScale, t);

        pulseCircle.localScale = baseScale * s;

        if (pulseGraphic)
        {
            var c = pulseGraphic.color;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            pulseGraphic.color = c;
        }
    }
}
