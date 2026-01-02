using UnityEngine;

public class AimAssistThrow : MonoBehaviour
{
    [Header("References")]
    public ReactiveReticle reticle;    
    public Transform rayOrigin;      
    public GrabSpawnBall spawner;      
    public LayerMask monsterLayers;    

    [Header("Aim assist settings")]
    public float maxDistance = 25f;
    public float coneAngleDegrees = 18f;  
    public float baseAssist = 0.75f;       
    public float snapAssistMaster = 1.0f; 

    public Vector3 ApplyAimAssist(Vector3 velocity)
    {
        if (!rayOrigin) return velocity;

        float aim01 = reticle ? reticle.AimAssist01 : 0f;
        if (spawner && spawner.selectedType == BallType.Master)
            aim01 = 1f;

        float strength = aim01 * baseAssist;
        if (spawner && spawner.selectedType == BallType.Master)
            strength = snapAssistMaster;

        if (strength <= 0.001f) return velocity;

        Transform target = FindBestTarget();
        if (!target) return velocity;

        Vector3 dirTo = (target.position - rayOrigin.position);
        dirTo.y = 0f;

        if (dirTo.sqrMagnitude < 0.001f) return velocity;

        Vector3 desiredDir = dirTo.normalized;

        Vector3 v = velocity;
        float speed = v.magnitude;
        if (speed < 0.001f) return velocity;

        Vector3 curDir = v.normalized;

        Vector3 newDir = Vector3.Slerp(curDir, desiredDir, strength).normalized;
        return newDir * speed;
    }

    Transform FindBestTarget()
    {
        if (monsterLayers.value == 0) return null;

        Collider[] hits = Physics.OverlapSphere(rayOrigin.position, maxDistance, monsterLayers, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return null;

        Transform best = null;
        float bestScore = -999f;

        Vector3 forward = rayOrigin.forward;
        forward.y = 0f;
        forward.Normalize();

        float cosLimit = Mathf.Cos(coneAngleDegrees * Mathf.Deg2Rad);

        for (int i = 0; i < hits.Length; i++)
        {
            Transform t = hits[i].transform;
            Vector3 to = (t.position - rayOrigin.position);
            to.y = 0f;

            float dist = to.magnitude;
            if (dist < 0.01f) continue;

            Vector3 toDir = to / dist;

            float cos = Vector3.Dot(forward, toDir);
            if (cos < cosLimit) continue; 
            float score = (cos * 2.0f) - (dist / maxDistance);
            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }

    void OnDrawGizmosSelected()
    {
        if (!rayOrigin) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rayOrigin.position, maxDistance);
    }
}
