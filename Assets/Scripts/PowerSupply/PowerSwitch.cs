using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PowerSwitch : MonoBehaviour
{
    PowerSourceController ctrl;

    void Awake() { ctrl = GetComponentInParent<PowerSourceController>(); }

    void OnMouseDown()
    {
        var ctrl = GetComponentInParent<PowerSourceController>();
        if (ctrl) ctrl.TogglePower();
    }
}
