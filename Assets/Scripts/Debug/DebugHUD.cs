using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    public LookAtHighlighter highlighter; // drag your Main Camera’s component here
    public KeyCode toggleKey = KeyCode.F1;
    public KeyCode debugToggleKey = KeyCode.F2;
    public bool visible = true;

    Rect _rect = new Rect(10, 10, 260, 120);

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;

        if (!highlighter) return;

        // Toggle the highlighter's own debug lines
        if (Input.GetKeyDown(debugToggleKey))
            typeof(LookAtHighlighter).GetField("debug", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(highlighter, !(bool)typeof(LookAtHighlighter).GetField("debug", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(highlighter));

        // Adjust max distance: mouse wheel with Alt, or +/- keys
        float scroll = Input.mouseScrollDelta.y;
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            highlighter.MaxDistance += scroll * 0.5f;

        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            highlighter.MaxDistance += 0.5f;
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            highlighter.MaxDistance -= 0.5f;
    }

    void OnGUI()
    {
        if (!visible || !highlighter) return;

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.Box(_rect, GUIContent.none);
        GUI.color = Color.white;

        GUILayout.BeginArea(_rect);
        GUILayout.Label("<b><size=14>Look Debug</size></b>");
        GUILayout.Label($"Max Distance: {highlighter.MaxDistance:0.0}  (Alt+Wheel, +/-)");
        string hitName = string.IsNullOrEmpty(highlighter.LastHitName) ? "(none)" : highlighter.LastHitName;
        string hitDist = highlighter.LastHitDistance >= 0f ? $"{highlighter.LastHitDistance:0.00} m" : "—";
        GUILayout.Label($"Hit: {hitName}");
        GUILayout.Label($"Dist: {hitDist}");
        GUILayout.Label($"Toggle HUD: {toggleKey} | Toggle Rays: {debugToggleKey}");
        GUILayout.EndArea();

        // drag-to-move
        if (Event.current.type == EventType.MouseDown && _rect.Contains(Event.current.mousePosition))
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}
