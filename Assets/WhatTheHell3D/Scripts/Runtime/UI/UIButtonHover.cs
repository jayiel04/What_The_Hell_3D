using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Estilo de botón del menú de Godot: marco + relleno redondeados y texto que
/// cambian a naranja/dorado al pasar el ratón (y se oscurecen al pulsar).
/// </summary>
public sealed class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Colores normales")]
    public Color normalBackground = new Color(0.09f, 0.10f, 0.14f, 0.88f);
    public Color normalBorder = new Color(0.32f, 0.38f, 0.47f, 0.9f);
    public Color normalText = new Color(0.93f, 0.95f, 1f);

    [Header("Colores hover")]
    public Color hoverBackground = new Color(0.85f, 0.43f, 0.07f, 0.94f);
    public Color hoverBorder = new Color(1f, 0.75f, 0.29f, 1f);
    public Color hoverText = new Color(1f, 0.75f, 0.29f);

    [Header("Colores pressed")]
    public Color pressedBackground = new Color(0.55f, 0.22f, 0.03f, 1f);

    [Header("Referencias (se auto-detectan si están vacías)")]
    public Image borderImage;
    public Image fillImage;
    public Text labelText;

    private Button button;
    private bool pressed;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (borderImage == null)
        {
            borderImage = GetComponent<Image>();
        }

        if (fillImage == null)
        {
            Transform fill = transform.Find("Fill");
            if (fill != null)
            {
                fillImage = fill.GetComponent<Image>();
            }
        }

        if (labelText == null && button != null)
        {
            labelText = button.GetComponentInChildren<Text>();
        }

        ApplyNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pressed) return;
        SetColors(hoverBackground, hoverBorder, hoverText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pressed = false;
        ApplyNormal();
    }

    private void Update()
    {
        // Refleja el estado pressed del Button para el relleno oscuro.
        if (button != null && button.interactable)
        {
            bool isPressed = button.IsPressed();
            if (isPressed && !pressed)
            {
                pressed = true;
                SetColors(pressedBackground, hoverBorder, hoverText);
            }
            else if (!isPressed && pressed)
            {
                pressed = false;
                SetColors(hoverBackground, hoverBorder, hoverText);
            }
        }
    }

    private void ApplyNormal()
    {
        SetColors(normalBackground, normalBorder, normalText);
    }

    private void SetColors(Color background, Color border, Color text)
    {
        if (fillImage != null)
        {
            fillImage.color = background;
        }

        if (borderImage != null)
        {
            borderImage.color = border;
        }

        if (labelText != null)
        {
            labelText.color = text;
        }
    }
}
