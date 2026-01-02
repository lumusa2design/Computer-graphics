using UnityEngine;

public class MonsterRespawn : MonoBehaviour
{
    Vector3 originalPos;
    Quaternion originalRot;

    void Awake()
    {
        CaptureOriginNow();
    }

    public void CaptureOriginNow()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    public void HideForCapture()
    {
        gameObject.SetActive(false);
    }

    public void RespawnAtOrigin()
    {
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(originalPos, originalRot);
    }
}
