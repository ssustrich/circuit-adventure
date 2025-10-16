using UnityEngine;

public class RaycastSanity : MonoBehaviour
{
    void Update()
    {
        var cam = GetComponent<Camera>();
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out var hit, 20f))
            Debug.Log("[Sanity] " + hit.collider.name);
    }
}