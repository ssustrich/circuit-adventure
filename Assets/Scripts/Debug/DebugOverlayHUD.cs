using UnityEngine;

public class DebugOverlayHUD : MonoBehaviour
{
    public bool show = true;

    void OnGUI()
    {
        if (!show) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 300), GUI.skin.box);
        GUILayout.Label("<b>DEBUG</b>", new GUIStyle(GUI.skin.label) { richText = true });

        if (GUILayout.Button("Toggle ALL Power (P)"))
            foreach (var ps in FindObjectsOfType<PowerSourceController>()) ps.TogglePower();

        if (GUILayout.Button("Recompute ALL LEDs (L)"))
            foreach (var led in FindObjectsOfType<LEDDevice>()) led.Recompute();

        GUILayout.Space(8);
        foreach (var ps in FindObjectsOfType<PowerSourceController>())
            GUILayout.Label($"{ps.name}: {(ps.IsPowered ? "ON" : "OFF")}");

        GUILayout.EndArea();
    }
}
