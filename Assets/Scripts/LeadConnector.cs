using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LeadConnector : MonoBehaviour
{
    public enum LeadType { Positive, Negative }
    public LeadType type;

    public bool isConnected => connectedTo != null;
    public LeadConnector connectedTo;

    public void ConnectTo(LeadConnector other)
    {
        connectedTo = other;
        other.connectedTo = this;
        Debug.Log($"{name} connected to {other.name}");
    }

    [Header("Gizmo Debug")]
    public bool drawGizmos = true;
    public float gizmoRadius = 1.0f;   // try 0.05–0.1 if you scaled x10
    public Color posColor = new Color(1, 0.2f, 0.2f, 1);
    public Color negColor = new Color(0.2f, 0.4f, 1, 1);
    void OnDrawGizmos()
    {
        if (!drawGizmos || connectedTo == null) return;

        // Pick color by polarity
        var c = (type == LeadType.Positive) ? posColor : negColor;
        Gizmos.color = c;

        // Scale the spheres sensibly in world space
        float r = gizmoRadius;// * Mathf.Max(
            //transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        // Line + endpoint spheres
        Gizmos.DrawLine(transform.position, connectedTo.transform.position);
        Gizmos.DrawSphere(transform.position, r);
        Gizmos.DrawSphere(connectedTo.transform.position, r);
    }
        void LateUpdate()
    {
        if (!drawGizmos || connectedTo == null) return;
        var c = (type == LeadType.Positive) ? posColor : negColor;
        Debug.DrawLine(transform.position, connectedTo.transform.position, c);
    }

    public void Disconnect()
    {
        if (connectedTo)
        {
            var other = connectedTo;
            connectedTo = null;
            other.connectedTo = null;
        }
    }

    // small helper for visuals later
    public Vector3 WorldPosition => transform.position;
}
