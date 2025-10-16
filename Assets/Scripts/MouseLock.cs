using UnityEngine;

public class MouseLock : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.Escape; // unlock with Esc

    void OnEnable() => Lock(true);

    void Update()
    {
        // Press Esc to unlock; click to re-lock
        if (Input.GetKeyDown(toggleKey)) Lock(false);
        if (!Cursor.visible && Cursor.lockState == CursorLockMode.Locked) return;
        if (Input.GetMouseButtonDown(0)) Lock(true);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Re-lock when focus returns
        if (hasFocus && !Cursor.visible) Lock(true);
    }

    void Lock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
