#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LeadConnector))]
public class LeadConnectorGizmoDrawer : Editor
{
    void OnSceneGUI()
    {
        var lead = (LeadConnector)target;
        if (!lead || !lead.drawGizmos || lead.connectedTo == null) return;

        var col = (lead.type == LeadConnector.LeadType.Positive) ? lead.posColor : lead.negColor;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always; // draw on top
        Handles.color = col;
        Handles.DrawAAPolyLine(4f, new Vector3[] { lead.transform.position, lead.connectedTo.transform.position });
        Handles.SphereHandleCap(0, lead.transform.position, Quaternion.identity, lead.gizmoRadius, EventType.Repaint);
        Handles.SphereHandleCap(0, lead.connectedTo.transform.position, Quaternion.identity, lead.gizmoRadius, EventType.Repaint);
    }
}
#endif
