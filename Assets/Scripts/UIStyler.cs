using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 样式工具：生成渐变背景、圆角按钮等纹理
/// </summary>
public static class UIStyler
{
    /// <summary>
    /// 为 Image 设置垂直渐变纹理（自上而下）
    /// </summary>
    public static void ApplyVerticalGradient(Image image, Color topColor, Color bottomColor, int width = 4, int height = 256)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color color = Color.Lerp(topColor, bottomColor, t);
            for (int x = 0; x < width; x++)
                tex.SetPixel(x, y, color);
        }
        tex.Apply();

        // 创建精灵并应用到图片
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
        sprite.name = "GradientBG";
        image.sprite = sprite;
        image.color = Color.white; // 使用精灵颜色而非纯色
        image.type = Image.Type.Simple;
        image.raycastTarget = false;
    }

    /// <summary>
    /// 为按钮生成带圆角的背景纹理
    /// </summary>
    public static void StyleButton(Image buttonImage, Color color, float roundness = 0.2f)
    {
        int size = 64;
        int radius = Mathf.RoundToInt(size * roundness);

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        // 绘制圆角矩形
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inCircleTL = (x < radius && y < radius) && (x - radius) * (x - radius) + (y - radius) * (y - radius) > radius * radius;
                bool inCircleTR = (x >= size - radius && y < radius) && (x - (size - radius)) * (x - (size - radius)) + (y - radius) * (y - radius) > radius * radius;
                bool inCircleBL = (x < radius && y >= size - radius) && (x - radius) * (x - radius) + (y - (size - radius)) * (y - (size - radius)) > radius * radius;
                bool inCircleBR = (x >= size - radius && y >= size - radius) && (x - (size - radius)) * (x - (size - radius)) + (y - (size - radius)) * (y - (size - radius)) > radius * radius;

                if (inCircleTL || inCircleTR || inCircleBL || inCircleBR)
                    tex.SetPixel(x, y, Color.clear);
                else
                    tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        sprite.name = "RoundedButton";

        buttonImage.sprite = sprite;
        buttonImage.color = Color.white;
        buttonImage.type = Image.Type.Sliced;
        buttonImage.pixelsPerUnitMultiplier = 0.5f;
    }
}
