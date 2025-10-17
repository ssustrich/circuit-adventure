using UnityEngine;

public class CenterClickToggleLED : MonoBehaviour
{
    [SerializeField] LayerMask mask = ~0;   // set to your Interactable/Default layers
    [SerializeField] float maxDistance = 20f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out var hit, maxDistance, mask, QueryTriggerInteraction.Ignore))
        {
            var led = hit.collider.GetComponentInParent<LEDLight>();
            if (led)
            {
                led.Toggle();
                Debug.Log("[CenterClickToggleLED] Toggled LED via " + hit.collider.name);
            }
        }
    }
}
