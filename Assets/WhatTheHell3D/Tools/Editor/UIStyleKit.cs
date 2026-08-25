using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Kit de estilos UGUI compartido (Fase 10/11): esquinas redondeadas con borde
/// al estilo StyleBoxFlat de Godot, botones con relleno+marco y textos con sombra.
/// </summary>
public static class UIStyleKit
{
    private const string UiRoot = "Assets/WhatTheHell3D/UI";
    private static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    // Paleta del menú original de Godot (main_menu.gd).
    public static readonly Color BtnNormalBg = new Color(0.09f, 0.10f, 0.14f, 0.88f);
    public static readonly Color BtnNormalBorder = new Color(0.32f, 0.38f, 0.47f, 0.9f);
    public static readonly Color BtnHoverBg = new Color(0.85f, 0.43f, 0.07f, 0.94f);
    public static readonly Color BtnHoverBorder = new Color(1f, 0.75f, 0.29f, 1f);
    public static readonly Color BtnPressedBg = new Color(0.55f, 0.22f, 0.03f, 1f);
    public static readonly Color BtnText = new Color(0.93f, 0.95f, 1f);
    public static readonly Color BtnTextHover = new Color(1f, 0.75f, 0.29f);
    public static readonly Color PopupBg = new Color(0.035f, 0.028f, 0.055f, 1f);
    public static readonly Color PopupBorder = new Color(0.85f, 0.56f, 0.13f, 1f);
    public static readonly Color Gold = new Color(1f, 0.76f, 0.36f);

    /// <summary>Sprite blanco redondeado (9-slice) generado por código; se tiñe via Image.color.</summary>
    public static Sprite RoundedSprite()
    {
        if (!AssetDatabase.IsValidFolder(UiRoot))
        {
            Directory.CreateDirectory(UiRoot);
            AssetDatabase.Refresh();
        }

        string path = $"{UiRoot}/RoundedRect.png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        const int size = 72;
        const int radius = 20;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
        float half = size * 0.5f;
        Vector2 center = new Vector2(half, half);
        Vector2 halfSize = new Vector2(half - 0.5f, half - 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // SDF de rectángulo redondeado → alpha con anti-aliasing de 1 px.
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - center;
                Vector2 q = new Vector2(Mathf.Abs(p.x) - halfSize.x + radius, Mathf.Abs(p.y) - halfSize.y + radius);
                float dist = Mathf.Min(Mathf.Max(q.x, q.y), 0f) + new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude - radius;
                float alpha = Mathf.Clamp01(0.5f - dist);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return sprite;
    }

    public static Text CreateStyledText(Transform parent, string name, string content, int size, Color color,
        FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        RemoveExistingChild(parent, name);
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        ((RectTransform)go.transform).SetParent(parent, false);
        Text text = go.GetComponent<Text>();
        text.font = DefaultFont;
        text.text = content;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.raycastTarget = false;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        AddShadow(text, new Vector2(2f, -2f));
        return text;
    }

    public static void AddShadow(Graphic target, Vector2 distance)
    {
        if (target.gameObject.GetComponent<Shadow>() == null)
        {
            Shadow shadow = target.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = distance;
        }
    }

    /// <summary>Panel redondeado con marco: Image exterior (borde) + hijo interior (relleno).</summary>
    public static GameObject CreateBorderedPanel(Transform parent, string name, Color fill, Color border,
        float borderThickness = 3f)
    {
        RemoveExistingChild(parent, name);
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = (RectTransform)panel.transform;
        rect.SetParent(parent, false);
        Image borderImage = panel.GetComponent<Image>();
        borderImage.sprite = RoundedSprite();
        borderImage.type = Image.Type.Sliced;
        borderImage.color = border;
        borderImage.raycastTarget = false;

        GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = (RectTransform)fillGo.transform;
        fillRect.SetParent(rect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(borderThickness, borderThickness);
        fillRect.offsetMax = new Vector2(-borderThickness, -borderThickness);
        Image fillImage = fillGo.GetComponent<Image>();
        fillImage.sprite = RoundedSprite();
        fillImage.type = Image.Type.Sliced;
        fillImage.color = fill;
        fillImage.raycastTarget = false;
        return panel;
    }

    /// <summary>Botón estilo Godot: marco + relleno + etiqueta (alineación izquierda opcional) + UIButtonHover.</summary>
    public static Button CreateStyledButton(Transform parent, string name, string label, Vector2 anchoredPosition,
        Vector2 size, int fontSize, Color fill, Color border, Color textColor, bool alignLeft = false,
        bool topAnchor = false)
    {
        RemoveExistingChild(parent, name);
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonHover));
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        if (topAnchor)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
        }

        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image borderImage = go.GetComponent<Image>();
        borderImage.sprite = RoundedSprite();
        borderImage.type = Image.Type.Sliced;
        borderImage.color = border;

        GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform fillRect = (RectTransform)fillGo.transform;
        fillRect.SetParent(rect, false);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);
        Image fillImage = fillGo.GetComponent<Image>();
        fillImage.sprite = RoundedSprite();
        fillImage.type = Image.Type.Sliced;
        fillImage.color = fill;

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        RectTransform labelRect = (RectTransform)labelGo.transform;
        labelRect.SetParent(fillRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(alignLeft ? 24f : 0f, 0f);
        labelRect.offsetMax = new Vector2(alignLeft ? -12f : 0f, 0f);
        Text labelText = labelGo.GetComponent<Text>();
        labelText.font = DefaultFont;
        labelText.text = label;
        labelText.fontSize = fontSize;
        labelText.color = textColor;
        labelText.alignment = alignLeft ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        labelText.raycastTarget = false;
        AddShadow(labelText, new Vector2(2f, -2f));

        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.fadeDuration = 0.08f;
        colors.highlightedColor = new Color(1.05f, 1.03f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        colors.selectedColor = Color.white;
        button.colors = colors;

        UIButtonHover hover = go.GetComponent<UIButtonHover>();
        hover.borderImage = borderImage;
        hover.fillImage = fillImage;
        hover.labelText = labelText;
        hover.normalBackground = fill;
        hover.hoverBackground = fill == BtnNormalBg ? BtnHoverBg : Color.Lerp(fill, BtnHoverBg, 0.75f);
        hover.pressedBackground = BtnPressedBg;
        hover.normalBorder = border;
        hover.hoverBorder = BtnHoverBorder;
        hover.normalText = textColor;
        hover.hoverText = BtnTextHover;
        return button;
    }

    public static void RemoveExistingChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }
}
