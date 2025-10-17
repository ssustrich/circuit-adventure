using UnityEngine;

public class LEDClickTester : MonoBehaviour
{
    LEDLight led;
    void Awake() { led = GetComponent<LEDLight>(); }
    void OnMouseDown()
    {
        if (led) led.Toggle();   // click in Scene/Game to toggle on/off
    }
}