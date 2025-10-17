using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LEDLight : MonoBehaviour
{
    [Header("Assign the lens renderer(s) that should glow")]
    public Renderer[] lensRenderers;

    [Header("Emission colors")]
    public Color onEmission = Color.red;    // pick your LED color
    public Color offEmission = Color.black;  // keep black for "off"

    [Header("Start state")]
    public bool startOn = false;             // false = dark on play

    Material[] _mats;           
    // per-instance mats
    public Light pointLight;
    void Awake()
    {
        // Fallback: if you forgot to assign, try to find a renderer on this object
        if (lensRenderers == null || lensRenderers.Length == 0)
        {
            var r = GetComponentInChildren<Renderer>();
            if (r) lensRenderers = new[] { r };
        }

        // Create unique material instances so each LED can be controlled independently
        var list = new System.Collections.Generic.List<Material>();
        foreach (var r in lensRenderers)
        {
            if (!r) continue;
            // .materials returns instances for all sub-mats
            var mats = r.materials;
            foreach (var m in mats) list.Add(m);
        }
        _mats = list.ToArray();
    }

    void Start()
    {
        SetOn(startOn); // ensure correct state on play
    }

    public void SetOn(bool on)
    {
        if (_mats == null) return;

        for (int i = 0; i < _mats.Length; i++)
        {
            var m = _mats[i];
            if (!m) continue;

            if (m.HasProperty("_EmissionColor"))
            {
                if (on)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", onEmission);
                }
                else
                {
                    // turn emission fully off
                    m.SetColor("_EmissionColor", offEmission);
                    m.DisableKeyword("_EMISSION");
                }
            }
        }
        if (pointLight) pointLight.enabled = on;
    }

    // Handy manual toggle (e.g., call from a button or key)
    public void Toggle() => SetOn(!IsOn());
    public bool IsOn()
    {
        if (_mats == null || _mats.Length == 0) return false;
        // If any mat has emission keyword enabled, consider it "on"
        for (int i = 0; i < _mats.Length; i++)
        {
            var m = _mats[i];
            if (m && m.IsKeywordEnabled("_EMISSION")) return true;
        }
        return false;
    }
}
