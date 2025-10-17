// PlacementController.cs
// Click-to-place system with center-screen ray, grid snap, ghost preview, and unified lift
// Works in Built-in 3D. Attach to Main Camera (or any object with a Camera).

using System.Collections.Generic;
using UnityEngine;

public class PlacementController : MonoBehaviour
{
    // Data model: each placeable prefab can define how it should sit on surfaces
    [System.Serializable]
    public class Placeable
    {
        public GameObject prefab;
        [Tooltip("If the prefab pivot is at its center, enable this so it lifts by half its height.")]
        public bool pivotIsCenter = true;
        [Tooltip("Extra per-prefab lift in meters (small tweaks like 0.005–0.02).")]
        public float extraLift = 0f;
    }

    [Header("Placeables (hotkeys: 1..n)")]
    public Placeable[] placeables;

    [Header("Raycast")]
    public LayerMask surfaceMask = ~0;          // surfaces you can place on
    public float maxDistance = 30f;

    [Header("Grid Snap")]
    public float gridSize = 0.5f;
    public bool snapYToSurface = true;          // usually true (snap height too)

    [Header("Preview (Ghost)")]
    [Tooltip("Optional: transparent material (Standard: Fade) used for the ghost.")]
    public Material previewMat;
    [Range(0f, 1f)] public float previewAlpha = 0.35f;
    public Color previewColor = new Color(1f, 1f, 1f, 0.6f);
    public Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.6f);

    [Header("Placement Rules")]
    [Tooltip("Layers considered 'occupied' and block placement.")]
    public LayerMask overlapMask = ~0;
    public Vector3 extraPadding = new Vector3(0.02f, 0.02f, 0.02f);
    [Tooltip("If true, lift along the surface normal (useful for slopes). If false, lift straight up.")]
    public bool useSurfaceNormalForLift = false;

    [Header("Rotation & Controls")]
    public float rotateStep = 90f;
    public KeyCode rotateKey = KeyCode.R;
    public KeyCode placeKey = KeyCode.Mouse0;
    public KeyCode cancelKey = KeyCode.Mouse1;

    [Header("Debug")]
    public bool drawRay = true;

    // internals 
    Camera cam;
    int currentIndex = -1;

    GameObject ghost;            // preview instance
    Quaternion ghostRotation = Quaternion.identity;
    Bounds ghostBoundsLocal;     // bounds of ghost in local space
    float currentLift = 0f;      // unified lift used by BOTH ghost and placed instance

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        HandleSelectionHotkeys();
        HandleRotationInput();

        if (currentIndex < 0 || !EnsureGhost()) return;

        // Center-screen ray
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        bool hit = Physics.Raycast(ray, out var hitInfo, maxDistance, surfaceMask, QueryTriggerInteraction.Ignore);

        if (drawRay)
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, hit ? Color.green : Color.red);

        if (!hit)
        {
            // park ghost near camera so user sees *something* while not over a surface
            ghost.transform.position = cam.transform.position + cam.transform.forward * 2f;
            return;
        }

        // Snap to grid
        Vector3 snapped = SnapToGrid(hitInfo.point, gridSize, snapYToSurface);

        // Place ghost
        PositionGhost(snapped, hitInfo.normal);

        // Validate overlap
        bool valid = !IsOverlappingAt(snapped, ghost.transform.rotation);

        // Tint preview
        SetGhostTint(valid ? previewColor : invalidColor);

        // Place / Cancel
        if (Input.GetKeyDown(placeKey) && valid)
            PlaceAt(snapped, ghost.transform.rotation, hitInfo.normal);

        if (Input.GetKeyDown(cancelKey))
            CancelPlacement();
    }

    //  Selection & Rotation 
    void HandleSelectionHotkeys()
    {
        int max = Mathf.Min(placeables != null ? placeables.Length : 0, 9);
        for (int i = 0; i < max; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectIndex(i);
        }
    }

    void HandleRotationInput()
    {
        if (currentIndex < 0) return;
        if (Input.GetKeyDown(rotateKey))
            ghostRotation = Quaternion.Euler(0f, ghostRotation.eulerAngles.y + rotateStep, 0f);
    }

    void SelectIndex(int idx)
    {
        if (placeables == null || idx < 0 || idx >= placeables.Length) return;
        if (placeables[idx] == null || placeables[idx].prefab == null) return;

        currentIndex = idx;
        ghostRotation = Quaternion.identity;
        BuildGhost();
    }

    //  Ghost lifecycle
    bool EnsureGhost()
    {
        if (ghost) return true;
        if (currentIndex < 0) return false;
        BuildGhost();
        return ghost != null;
    }

    void BuildGhost()
    {
        // Destroy old
        if (ghost) Destroy(ghost);

        var p = placeables[currentIndex];
        if (p == null || !p.prefab) return;

        // Create new ghost
        ghost = Instantiate(p.prefab);
        ghost.name = $"GHOST_{p.prefab.name}";

        // Ensure it's only visual (no physics)
        foreach (var c in ghost.GetComponentsInChildren<Collider>(true)) Destroy(c);
        foreach (var c in ghost.GetComponentsInChildren<Rigidbody>(true)) Destroy(c);

        // If a transparent preview material is provided, swap all materials to it
        if (previewMat)
        {
            var rends = ghost.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = previewMat;
                r.sharedMaterials = mats;
            }
        }

        // Cache bounds (local)
        ghostBoundsLocal = CalculateRendererBounds(ghost);

        // ?? ONE SOURCE OF TRUTH: compute the lift we will use everywhere ??
        float halfHeightWorld = ghostBoundsLocal.extents.y * ghost.transform.lossyScale.y;
        currentLift = (p.pivotIsCenter ? halfHeightWorld : 0f) + p.extraLift;

        // Initial tint
        SetGhostTint(previewColor);
    }

    void PositionGhost(Vector3 snappedPos, Vector3 surfaceNormal)
    {
        ghost.transform.SetPositionAndRotation(snappedPos, ghostRotation);
        Vector3 liftDir = useSurfaceNormalForLift ? surfaceNormal.normalized : Vector3.up;
        ghost.transform.position += liftDir * currentLift;
    }

    void CancelPlacement()
    {
        currentIndex = -1;
        if (ghost) Destroy(ghost);
    }

    //  Placement 
    void PlaceAt(Vector3 pos, Quaternion rot, Vector3 surfaceNormal)
    {
        var p = placeables[currentIndex];
        var instance = Instantiate(p.prefab, pos, rot);

        // Apply the SAME lift as the ghost
        Vector3 liftDir = useSurfaceNormalForLift ? surfaceNormal.normalized : Vector3.up;
        instance.transform.position += liftDir * currentLift;

        instance.name = $"{p.prefab.name}_{Time.frameCount}";
        // Keep ghost so user can place multiple
    }

    //  Validation: overlap test using ghost bounds 
    bool IsOverlappingAt(Vector3 worldPos, Quaternion worldRot)
    {
        // Approx world-space AABB from cached local bounds
        var b = TransformBounds(ghostBoundsLocal, worldPos, worldRot);
        b.Expand(extraPadding);

        // OverlapBox in that space (blocking layers only)
        var hits = Physics.OverlapBox(b.center, b.extents, worldRot, overlapMask, QueryTriggerInteraction.Ignore);
        return hits != null && hits.Length > 0;
    }

    //  Visuals: tint ghost via MaterialPropertyBlock (works with Standard/BaseColor) ?
    void SetGhostTint(Color tint)
    {
        tint.a = previewAlpha;
        var rends = ghost.GetComponentsInChildren<Renderer>(true);
        var mpb = new MaterialPropertyBlock();
        foreach (var r in rends)
        {
            r.GetPropertyBlock(mpb);
            if (r.sharedMaterial && r.sharedMaterial.HasProperty("_Color"))
                mpb.SetColor("_Color", tint);
            if (r.sharedMaterial && r.sharedMaterial.HasProperty("_BaseColor"))
                mpb.SetColor("_BaseColor", tint);
            r.SetPropertyBlock(mpb);
        }
    }

    //  Utilities 
    static Vector3 SnapToGrid(Vector3 p, float size, bool snapY)
    {
        float x = Mathf.Round(p.x / size) * size;
        float y = snapY ? Mathf.Round(p.y / size) * size : p.y;
        float z = Mathf.Round(p.z / size) * size;
        return new Vector3(x, y, z);
    }

    static Bounds CalculateRendererBounds(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.zero);

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        // Convert to local-space bounds relative to 'go'
        var centerLocal = go.transform.InverseTransformPoint(b.center);
        var invScale = new Vector3(
            1f / go.transform.lossyScale.x,
            1f / go.transform.lossyScale.y,
            1f / go.transform.lossyScale.z);
        var sizeLocal = Vector3.Scale(b.size, invScale);

        return new Bounds(centerLocal, sizeLocal);
    }

    static Bounds TransformBounds(Bounds local, Vector3 pos, Quaternion rot)
    {
        // Rotate the 8 corners and compute world AABB
        Vector3 c = local.center;
        Vector3 e = local.extents;

        Vector3[] corners =
        {
            c + new Vector3(-e.x, -e.y, -e.z),
            c + new Vector3(-e.x, -e.y,  e.z),
            c + new Vector3(-e.x,  e.y, -e.z),
            c + new Vector3(-e.x,  e.y,  e.z),
            c + new Vector3( e.x, -e.y, -e.z),
            c + new Vector3( e.x, -e.y,  e.z),
            c + new Vector3( e.x,  e.y, -e.z),
            c + new Vector3( e.x,  e.y,  e.z),
        };

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < 8; i++)
        {
            Vector3 w = pos + rot * corners[i];
            min = Vector3.Min(min, w);
            max = Vector3.Max(max, w);
        }

        var b = new Bounds();
        b.SetMinMax(min, max);
        return b;
    }
}
