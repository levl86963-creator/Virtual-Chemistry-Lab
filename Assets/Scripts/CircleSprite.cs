using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generates a circular sprite at runtime and assigns it to the Image component.
/// Works in both edit mode and play mode via ExecuteAlways.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class CircleSprite : MonoBehaviour
{
    private static Sprite _sharedSprite;

    private const int SPRITE_SIZE = 128;
    private const float EDGE_SOFTNESS = 2f;

    private void OnEnable()
    {
        AssignSprite();
    }

    private void OnValidate()
    {
        AssignSprite();
    }

    /// <summary>
    /// Assigns the generated circle sprite to the Image component if it has no sprite.
    /// </summary>
    private void AssignSprite()
    {
        if (_sharedSprite == null)
            _sharedSprite = CreateCircleSprite(SPRITE_SIZE);

        var img = GetComponent<Image>();
        if (img != null && img.sprite == null)
            img.sprite = _sharedSprite;
    }

    /// <summary>
    /// Creates a white circle sprite with anti-aliased edges.
    /// </summary>
    private static Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color[size * size];
        float center = size * 0.5f;
        float radius = size * 0.5f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center;
                float dy = y + 0.5f - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.SmoothStep(radius, radius - EDGE_SOFTNESS, dist);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}
