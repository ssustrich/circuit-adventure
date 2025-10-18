using System.Collections.Generic;
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

    bool _ready = false;


    Material[] _mats;           
    // per-instance mats
    public Light pointLight;

    void Awake()
    {
        if (lensRenderers == null || lensRenderers.Length == 0)
        {
            var r = GetComponentInChildren<Renderer>();
            if (r) lensRenderers = new[] { r };
        }
        // don’t build materials yet — do it lazily in SetOn
    }

    void Start()
    {
        SetOn(startOn);  // safe now because SetOn ensures init
    }

    void EnsureReady()
    {
        if (_ready) return;
        var list = new List<Material>();
        foreach (var r in lensRenderers)
        {
            if (!r) continue;
            // IMPORTANT: .materials (not sharedMaterials) -> per-instance copies
            var mats = r.materials;
            foreach (var m in mats) list.Add(m);
        }
        _mats = list.ToArray();
        _ready = true;
    }
    public void SetOn(bool on)
    {
        EnsureReady();                      // <-- lazy init here

        if (_mats == null) return;
        for (int i = 0; i < _mats.Length; i++)
        {
            var m = _mats[i];
            if (!m) continue;
            if (!m.HasProperty("_EmissionColor")) continue;

            if (on)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", onEmission);
            }
            else
            {
                m.SetColor("_EmissionColor", offEmission);
                m.DisableKeyword("_EMISSION");
            }
        }
    }

    public bool IsOn()
    {
        if (!_ready || _mats == null) return false;
        foreach (var m in _mats)
            if (m && m.IsKeywordEnabled("_EMISSION")) return true;
        return false;
    }
}
