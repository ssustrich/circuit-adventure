using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour, ICrosshairHighlight
{
    public Image img;
    public Color normal = Color.white;
    public Color highlight = new Color(1f, 0.9f, 0.2f);

    void Awake() { if (!img) img = GetComponent<Image>(); if (img) img.color = normal; }

    public void SetHighlighted(bool on) { if (img) img.color = on ? highlight : normal; }
}
