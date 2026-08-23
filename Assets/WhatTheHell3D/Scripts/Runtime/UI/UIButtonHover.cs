using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Estilo de botón del menú de Godot: color normal/hover/pressed sobre la Image
/// y color de texto que pasa a dorado al pasar el ratón.
/// </summary>
public sealed class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color normalBackground = new Color(0.09f, 0.10f, 0.14f, 0.88f);
    public Color hoverBackground = new Color(0.85f, 0.43f, 0.07f, 0.94f);
    public Color pressedBackground = new Color(0.55f, 0.22f, 0.03f, 1f);
    public Color normalText = new Color(0.93f, 0.95f, 1f);
    public Color hoverText = new Color(1f, 0.75f, 0.29f);

    private Image image;
    private Text label;
    private Button button;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        if (button != null)
        {
            label = button.GetComponentInChildren<Text>();
        }

        ApplyNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetBackground(hoverBackground);
        SetText(hoverText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormal();
    }

    private void ApplyNormal()
    {
        SetBackground(normalBackground);
        SetText(normalText);
    }

    private void SetBackground(Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }

    private void SetText(Color color)
    {
        if (label != null)
        {
            label.color = color;
        }
    }
}
