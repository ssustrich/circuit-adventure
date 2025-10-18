using UnityEngine;

public class WireConnectorManager : MonoBehaviour
{
    [SerializeField] LayerMask leadMask = ~0;
    [SerializeField] LineRenderer wirePrefab;
    [SerializeField] Material validMat;
    [SerializeField] Material invalidMat;
    [SerializeField] float maxDistance = 20f;
    [SerializeField] bool debugRay = true;

    Camera cam;
    LeadConnector first;
    LineRenderer preview;
    Vector3 firstPoint;



    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main; // works if your camera is tagged MainCamera
    }

    void Update()
    {
        if (!cam) cam = Camera.main;

        // DEBUG ray every frame
        if (debugRay)
        {
            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out var dbgHit, maxDistance, leadMask, QueryTriggerInteraction.Collide))
            {
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);
                // uncomment to log every frame:
                //Debug.Log($"[WireConnectorManager] Looking at: {dbgHit.collider.name} (layer {LayerMask.LayerToName(dbgHit.collider.gameObject.layer)})");
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
            }
        }

        if (Input.GetMouseButtonDown(0)) HandleClick();
        UpdatePreviewLine();
    }

    void HandleClick()
    {
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out var hit, maxDistance, leadMask, QueryTriggerInteraction.Collide))
            return;

        var lead = hit.collider.GetComponent<LeadConnector>();
        if (!lead) return;

        if (first == null)
        {
            first = lead;
            firstPoint = hit.point;         // store exact surface point
            StartPreviewLine(firstPoint);
        }
        else
        {
            TryConnect(first, firstPoint, lead, hit.point);
            ClearPreview();
        }
    }

    void TryConnect(LeadConnector a, Vector3 aPoint, LeadConnector b, Vector3 bPoint)
    {
        if (a == b) return;
        if (a.type == b.type) return;

        a.ConnectTo(b);
        SpawnWire(a, b, aPoint, bPoint);

        // 🔸 make LEDs update immediately (no need to cycle power)
        RecomputeAround(a, b);

        first = null;
    }


    void SpawnWire(LeadConnector a, LeadConnector b, Vector3 aPoint, Vector3 bPoint)
    {
        var wire = Instantiate(wirePrefab);
        var lr = wire.GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, aPoint);
        lr.SetPosition(1, bPoint);
        lr.material = validMat;

        var seg = wire.gameObject.AddComponent<WireSegment>();
        seg.A = a;
        seg.B = b;
        seg.aLocalOffset = a.transform.InverseTransformPoint(aPoint); // <— save as local
        seg.bLocalOffset = b.transform.InverseTransformPoint(bPoint); // <— save as local
    }

    void StartPreviewLine(Vector3 start)
    {
        preview = Instantiate(wirePrefab);
        preview.positionCount = 2;
        preview.SetPosition(0, start);
        preview.SetPosition(1, start);
        preview.material = validMat;
    }

    void UpdatePreviewLine()
    {
        if (!preview || first == null) return;

        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out var hit, maxDistance, leadMask, QueryTriggerInteraction.Collide))
            preview.SetPosition(1, hit.point);
        else
            preview.SetPosition(1, ray.origin + ray.direction * 2f);
    }

    void ClearPreview()
    {
        if (preview) Destroy(preview.gameObject);
        preview = null;
        first = null;
    }

    void SpawnWire(Vector3 a, Vector3 b)
    {
        var wire = Instantiate(wirePrefab);
        wire.positionCount = 2;
        wire.SetPosition(0, a);
        wire.SetPosition(1, b);
        wire.material = validMat;
    }

    void RecomputeAround(LeadConnector a, LeadConnector b)
    {
        var la = a.GetComponentInParent<LEDDevice>(); if (la) la.Recompute();
        var lb = b.GetComponentInParent<LEDDevice>(); if (lb && lb != la) lb.Recompute();
    }
}
