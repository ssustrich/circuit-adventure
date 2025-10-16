using UnityEngine;

public class SimpleCrosshair : MonoBehaviour, ICrosshairHighlight
{
    public int size = 6;
    public Color color = Color.white;

    Texture2D _tex;

    void Awake()
    {
        _tex = new Texture2D(1, 1);
        _tex.SetPixel(0, 0, Color.white);
        _tex.Apply();
    }

    void OnGUI()
    {
        var prev = GUI.color;
        GUI.color = color;
        float x = (Screen.width - size) * 0.5f;
        float y = (Screen.height - size) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, size, size), _tex);
        GUI.color = prev;
    }

    public void SetHighlighted(bool on) => color = on ? new Color(1f, 0.9f, 0.2f) : Color.white;
}
