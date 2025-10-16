using UnityEngine;

public class HighlightOnLook : MonoBehaviour
{
    [Header("Renderers to affect")]
    public Renderer[] targetRenderers;

    [Header("(Optional) Albedo Tint")]
    public bool tintAlbedoOnHighlight = false;        // keep OFF to preserve metal look
    public Color normalColor = Color.white;
    public Color highlightColor = new Color(1f, 0.9f, 0.2f);

    [Header("Emission Glow (on look)")]
    public bool enableEmissionOnHighlight = true;
    public Color emissionColor = new Color(1f, 0.9f, 0.3f);
    [Range(0f, 8f)] public float emissionIntensity = 1.8f;
    [Range(0f, 0.25f)] public float fadeSeconds = 0.08f;

    [Header("Outline (on click)")]
    public bool useOutline = true;                  // <- turn this on
    public GameObject outlineObject;                // <- assign your Outline child here

    // --- internals ---
    Material[] mats;
    Color[] originalAlbedo;

    float fadeVel;
    float lookTarget = 0f;      // 0=not looked at, 1=looked at (for emission fade)
    float lookCurrent = 0f;

    public bool IsSelected { get; private set; }    // <- persistent selection

    void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            var r = GetComponentInChildren<Renderer>();
            if (r) targetRenderers = new[] { r };
        }

        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            var list = new System.Collections.Generic.List<Material>();
            var cols = new System.Collections.Generic.List<Color>();
            foreach (var r in targetRenderers)
            {
                if (!r) continue;
                var m = r.material;
                list.Add(m);
                cols.Add(m.HasProperty("_Color") ? m.color : Color.white);
                if (m.HasProperty("_EmissionColor"))
                {
                    m.DisableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", Color.black);
                }
            }
            mats = list.ToArray();
            originalAlbedo = cols.ToArray();
        }

        if (outlineObject) outlineObject.SetActive(false);
    }

    // Called by the camera detector each frame when the object is/ isn't looked at
    public void SetHighlighted(bool on)
    {
        lookTarget = on ? 1f : 0f;
        UpdateOutlineVisibility();
    }

    // Called by the camera detector on click
    public void SetSelected(bool on)
    {
        IsSelected = on;
        UpdateOutlineVisibility();
    }

    public void ToggleSelected()
    {
        SetSelected(!IsSelected);
    }

    void Update()
    {
        if (mats == null) return;

        // Smooth fade for “look” emission
        if (Mathf.Approximately(fadeSeconds, 0f)) lookCurrent = lookTarget;
        else lookCurrent = Mathf.SmoothDamp(lookCurrent, lookTarget, ref fadeVel, fadeSeconds);

        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i];
            if (!m) continue;

            // Keep metal look unless you explicitly want an albedo tint
            if (tintAlbedoOnHighlight && m.HasProperty("_Color"))
                m.color = Color.Lerp(originalAlbedo[i], highlightColor, lookCurrent);
            else if (m.HasProperty("_Color"))
                m.color = originalAlbedo[i];

            // Emission only while looked at
            if (enableEmissionOnHighlight && m.HasProperty("_EmissionColor"))
            {
                if (lookCurrent > 0.001f)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", emissionColor * (emissionIntensity * lookCurrent));
                }
                else
                {
                    m.SetColor("_EmissionColor", Color.black);
                    m.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    void UpdateOutlineVisibility()
    {
        if (useOutline && outlineObject)
            outlineObject.SetActive(IsSelected);
    }
}
