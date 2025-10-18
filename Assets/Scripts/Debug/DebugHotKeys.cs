using UnityEngine;

public class DebugHotkeys : MonoBehaviour
{
    public KeyCode toggleAllPowerKey = KeyCode.P;
    public KeyCode recomputeAllLEDKey = KeyCode.L;

    void Update()
    {
        if (Input.GetKeyDown(toggleAllPowerKey))
        {
            foreach (var ps in FindObjectsOfType<PowerSourceController>())
                ps.TogglePower();
        }
        if (Input.GetKeyDown(recomputeAllLEDKey))
        {
            foreach (var led in FindObjectsOfType<LEDDevice>())
                led.Recompute();
        }
    }
}
