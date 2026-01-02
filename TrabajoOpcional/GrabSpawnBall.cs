using UnityEngine;

public class GrabSpawnBall : MonoBehaviour
{
    [Header("Catalog")]
    public BallEntry[] balls;
    public BallType selectedType = BallType.Poke;

    [Header("Spawn")]
    public Transform spawnPoint;     
    public bool rightHand = true; 

    [Header("Hold behavior")]
    public bool stickToHandWhileHoldingGrip = true;
    public bool disablePhysicsWhileStuck = true;

    [Header("Throw tuning")]
    public float throwVelocityMultiplier = 1.25f;
    public float throwAngularMultiplier = 1.0f;
    public float minThrowSpeed = 0.15f;
    public float forwardBoost = 0.0f;

    [Header("Aim Assist (opcional)")]
    public AimAssistThrow aimAssist;

    [Header("Audio (Throw)")]
    public AudioSource handAudio;         
    public AudioClip throwClip;
    [Range(0f, 1f)] public float throwVolume = 1f;

    private GameObject currentBall;
    private Rigidbody currentRb;

    void Update()
    {
        if (spawnPoint == null) return;

        if (GripDown())
        {
            if (currentBall == null)
                SpawnBallAndStick();
        }

        if (currentBall != null && stickToHandWhileHoldingGrip && GripHeld())
        {
            currentBall.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        if (GripUp())
        {
            ReleaseAndThrow();
        }
    }

    public void SetBallType(int idx) => selectedType = (BallType)idx;
    public void SetBallType(BallType type) => selectedType = type;

    void SpawnBallAndStick()
    {
        var prefab = GetPrefab(selectedType);
        if (prefab == null)
        {
            Debug.LogWarning($"No hay prefab asignado para {selectedType}");
            return;
        }

        currentBall = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        currentBall.transform.SetParent(spawnPoint, true);

        var cap = currentBall.GetComponent<PokeballCapture>();
        if (cap) cap.ballType = selectedType;

        currentRb = currentBall.GetComponent<Rigidbody>();

        if (disablePhysicsWhileStuck && currentRb != null)
        {
            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
            currentRb.useGravity = false;
            currentRb.isKinematic = true;
        }
    }

    void ReleaseAndThrow()
    {
        if (currentBall == null) return;

        if (handAudio && throwClip)
            handAudio.PlayOneShot(throwClip, throwVolume);

        currentBall.transform.SetParent(null, true);

        if (currentRb != null)
        {
            if (disablePhysicsWhileStuck)
            {
                currentRb.isKinematic = false;
                currentRb.useGravity = true;
            }

            OVRInput.Controller c = rightHand ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;

            Vector3 v = OVRInput.GetLocalControllerVelocity(c) * throwVelocityMultiplier;
            Vector3 w = OVRInput.GetLocalControllerAngularVelocity(c) * throwAngularMultiplier;

            if (v.magnitude < minThrowSpeed)
                v = spawnPoint.forward * minThrowSpeed;

            if (forwardBoost != 0f)
                v += spawnPoint.forward * forwardBoost;

            if (aimAssist != null)
                v = aimAssist.ApplyAimAssist(v);

            currentRb.linearVelocity = v;
            currentRb.angularVelocity = w;
        }

        currentBall = null;
        currentRb = null;
    }

    GameObject GetPrefab(BallType type)
    {
        foreach (var b in balls)
            if (b.type == type) return b.prefab;
        return null;
    }

    bool GripHeld()
    {
        return rightHand
            ? OVRInput.Get(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
    }

    bool GripDown()
    {
        return rightHand
            ? OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger);
    }

    bool GripUp()
    {
        return rightHand
            ? OVRInput.GetUp(OVRInput.Button.SecondaryHandTrigger)
            : OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger);
    }
}
