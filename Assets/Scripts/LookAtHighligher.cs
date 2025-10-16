// LookAtHighlighter.cs
using UnityEngine;

public class LookAtHighlighter : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField, Range(0.5f, 50f)]
    float maxDistance = 12f;                         // <— backing field used everywhere internally
    [SerializeField] LayerMask interactableMask = ~0;

    [Header("Crosshair (optional)")]
    [SerializeField] MonoBehaviour crosshair;        // must implement ICrosshairHighlight

    [Header("Visual Line (Option B)")]
    [SerializeField] Transform lineStart;            // optional muzzle/offset transform
    [SerializeField] Vector3 visualStartOffset = new Vector3(0.06f, -0.02f, 0f);

    [Header("Debug")]
    [SerializeField] bool debug = true;
    [SerializeField] Color hitColor = Color.green;
    [SerializeField] Color missColor = Color.red;

    Camera cam;
    HighlightOnLook current;
    ICrosshairHighlight crosshairCtl;
    
    [Header("Selection")]
    [SerializeField] bool singleSelect = true;   // only one outlined at a time
    HighlightOnLook lastSelected;

    // ── Public API used by HUD/other scripts ────────────────────────────────────────
    public float MaxDistance
    {
        get => maxDistance;
        set => maxDistance = Mathf.Clamp(value, 0.5f, 50f);
    }
    public string LastHitName { get; private set; }
    public float LastHitDistance { get; private set; } = -1f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        crosshairCtl = crosshair as ICrosshairHighlight;
        if (crosshair && crosshairCtl == null)
            Debug.LogWarning($"{crosshair.name} does not implement ICrosshairHighlight.");
    }

    void Update()
    {
        if (!cam) return;

        // Accurate center ray for detection
        var centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool hit = Physics.Raycast(centerRay, out var hitInfo, maxDistance, interactableMask, QueryTriggerInteraction.Ignore);

        // Visible line (offset so you can see it)
        Vector3 start = lineStart
            ? lineStart.position
            : cam.transform.position
              + cam.transform.right * visualStartOffset.x
              + cam.transform.up * visualStartOffset.y
              + cam.transform.forward * visualStartOffset.z;

        Vector3 end = hit ? hitInfo.point : centerRay.origin + centerRay.direction * maxDistance;

        if (debug)
        {
            Debug.DrawRay(centerRay.origin, centerRay.direction * maxDistance, hit ? hitColor : missColor);
            Debug.DrawLine(start, end, hit ? hitColor : missColor);
        }

        // Resolve highlight target
        var next = hit ? hitInfo.collider.GetComponentInParent<HighlightOnLook>() : null;
        if (next != current)
        {
            if (current) current.SetHighlighted(false);
            current = next;
            if (current) current.SetHighlighted(true);


        }

        crosshairCtl?.SetHighlighted(current != null);

        // --- CLICK TO TOGGLE OUTLINE ---
        if (Input.GetMouseButtonDown(0) && current != null)
        {
            if (singleSelect)
            {
                if (lastSelected && lastSelected != current)
                    lastSelected.SetSelected(false);
                current.SetSelected(!(lastSelected == current && lastSelected.IsSelected));
                lastSelected = current.IsSelected ? current : null;
            }
            else
            {
                current.ToggleSelected();
                if (current.IsSelected) lastSelected = current; // optional: remember last
            }
        }

        // HUD info
        if (hit)
        {
            LastHitName = hitInfo.collider.name;
            LastHitDistance = hitInfo.distance;
        }
        else
        {
            LastHitName = null;
            LastHitDistance = -1f;
        }
    }
}
