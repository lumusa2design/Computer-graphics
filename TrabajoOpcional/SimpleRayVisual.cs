using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleRayVisual : MonoBehaviour
{
    public float length = 5f;
    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
    }

    void Update()
    {
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * length;

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
