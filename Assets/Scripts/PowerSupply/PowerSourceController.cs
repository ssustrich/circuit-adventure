// PowerSourceController.cs (patch)
using UnityEngine;

[DisallowMultipleComponent]
public class PowerSourceController : MonoBehaviour
{
    [Header("Startup")]
    public bool startPowered = true;
    public LEDLight powerLED;   // assign the LED button LEDLight
                                // In PowerSourceController.cs
    [ContextMenu("DEBUG/Toggle Power")]
    void DebugTogglePower() => TogglePower();

    public bool IsPowered { get; private set; }

    void Awake()
    {
        if (powerLED) powerLED.startOn = false; // visuals follow controller only
    }

    void OnEnable()
    {
        Apply(startPowered, propagate: false);   // early sync (may hit before materials)
    }

    void Start()
    {
        Apply(startPowered, propagate: true);    // guaranteed after materials exist
    }

    public void TogglePower() => Apply(!IsPowered, propagate: true);

    void Apply(bool on, bool propagate)
    {
        IsPowered = on;
        if (powerLED) powerLED.SetOn(on);

        if (propagate)
        {
            var leds = FindObjectsOfType<LEDDevice>();
            foreach (var led in leds) led.Recompute();
        }
    }
}
