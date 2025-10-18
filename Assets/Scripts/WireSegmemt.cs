using UnityEngine;

public class WireSegment : MonoBehaviour
{
    public LeadConnector A, B;
    public Vector3 aLocalOffset;   // local-space offset from A's transform to the hit point
    public Vector3 bLocalOffset;   // local-space offset from B's transform to the hit point

    LineRenderer lr;

    void Awake() { lr = GetComponent<LineRenderer>(); }

    void LateUpdate()
    {
        if (!lr) return;

        // Rebuild world positions from the saved local offsets
        Vector3 aPos = A ? A.transform.TransformPoint(aLocalOffset) : lr.GetPosition(0);
        Vector3 bPos = B ? B.transform.TransformPoint(bLocalOffset) : lr.GetPosition(1);

        lr.SetPosition(0, aPos);
        lr.SetPosition(1, bPos);
    }
}