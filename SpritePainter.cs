using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class SpritePainter : MonoBehaviour
{
    public Image image;                  // The image component in the Canvas
    public Color brushColor = Color.red; // The color of the brush used for painting
    public int brushSize = 5;            // The size of the brush
    public Texture2D paintMask;          // The mask texture where white areas are paintable and black outlines are non-paintable

    private Texture2D textureToPaintOn;
    private string texturePath;          // Path to the original texture file
    private bool isPainting = false;     // Indicates if painting is allowed
    private HashSet<Vector2Int> squarePixels;  // Set of pixels within the current square

    void Start()
    {
        // Get the texture from the sprite and create a new Texture2D to paint on
        Sprite sprite = image.sprite;
        Texture2D originalTexture = sprite.texture;

        // Get the path of the original texture
        texturePath = UnityEditor.AssetDatabase.GetAssetPath(originalTexture);

        // Create a new texture with a supported format
        textureToPaintOn = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);

        // Copy pixels from the original texture to the new one
        Color[] pixels = originalTexture.GetPixels();
        textureToPaintOn.SetPixels(pixels);
        textureToPaintOn.Apply();

        // Apply the texture to the image
        image.sprite = Sprite.Create(textureToPaintOn, sprite.rect, new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartPainting();
        }

        if (Input.GetMouseButton(0) && isPainting)
        {
            Paint();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopPainting();
        }
    }

    void StartPainting()
    {
        // Get the mouse position in screen space
        Vector2 mousePos = Input.mousePosition;

        // Convert the mouse position to the local space of the image
        RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform, mousePos, null, out Vector2 localPoint);

        // Convert local point to texture coordinates
        Vector2 pivot = image.rectTransform.pivot;
        Vector2 normalizedPoint = new Vector2(localPoint.x / image.rectTransform.rect.width + pivot.x,
                                              localPoint.y / image.rectTransform.rect.height + pivot.y);
        int x = Mathf.RoundToInt(normalizedPoint.x * textureToPaintOn.width);
        int y = Mathf.RoundToInt(normalizedPoint.y * textureToPaintOn.height);

        // Check if the initial pixel under the mouse is in a white area of the mask
        Color maskColor = paintMask.GetPixel(x, y);
        if (IsWhite(maskColor))
        {
            // Use flood fill to determine the bounds of the square
            squarePixels = FloodFill(x, y);

            // Start painting
            isPainting = true;
        }
    }

    void Paint()
    {
        // Get the mouse position in screen space
        Vector2 mousePos = Input.mousePosition;

        // Convert the mouse position to the local space of the image
        RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform, mousePos, null, out Vector2 localPoint);

        // Convert local point to texture coordinates
        Vector2 pivot = image.rectTransform.pivot;
        Vector2 normalizedPoint = new Vector2(localPoint.x / image.rectTransform.rect.width + pivot.x,
                                              localPoint.y / image.rectTransform.rect.height + pivot.y);
        int x = Mathf.RoundToInt(normalizedPoint.x * textureToPaintOn.width);
        int y = Mathf.RoundToInt(normalizedPoint.y * textureToPaintOn.height);

        // Paint on the texture within the bounds of the current square
        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                int px = x + i;
                int py = y + j;

                Vector2Int pixelPos = new Vector2Int(px, py);

                // Check if the pixel is within the bounds of the texture and the current square
                if (px >= 0 && px < textureToPaintOn.width && py >= 0 && py < textureToPaintOn.height && squarePixels.Contains(pixelPos))
                {
                    // Paint on the texture
                    textureToPaintOn.SetPixel(px, py, brushColor);
                }
            }
        }
        textureToPaintOn.Apply();
    }

    void StopPainting()
    {
        // Stop painting when the mouse button is released
        isPainting = false;
    }

    HashSet<Vector2Int> FloodFill(int startX, int startY)
    {
        // Initialize the flood fill algorithm to determine the bounds of the square
        HashSet<Vector2Int> result = new HashSet<Vector2Int>();
        Queue<Vector2Int> pixelsToCheck = new Queue<Vector2Int>();
        pixelsToCheck.Enqueue(new Vector2Int(startX, startY));

        while (pixelsToCheck.Count > 0)
        {
            Vector2Int current = pixelsToCheck.Dequeue();
            if (result.Contains(current))
                continue;

            Color maskColor = paintMask.GetPixel(current.x, current.y);
            if (IsWhite(maskColor))
            {
                result.Add(current);

                // Add the neighboring pixels to check
                if (current.x > 0) pixelsToCheck.Enqueue(new Vector2Int(current.x - 1, current.y));
                if (current.x < paintMask.width - 1) pixelsToCheck.Enqueue(new Vector2Int(current.x + 1, current.y));
                if (current.y > 0) pixelsToCheck.Enqueue(new Vector2Int(current.x, current.y - 1));
                if (current.y < paintMask.height - 1) pixelsToCheck.Enqueue(new Vector2Int(current.x, current.y + 1));
            }
        }

        return result;
    }

    bool IsWhite(Color color)
    {
        // Check if the color is close to white
        return color.r > 0.9f && color.g > 0.9f && color.b > 0.9f;
    }

    public void SaveTexture()
    {
        // Encode the painted texture to PNG format
        byte[] bytes = textureToPaintOn.EncodeToPNG();

        // Overwrite the original texture file
        File.WriteAllBytes(texturePath, bytes);

        // Refresh the AssetDatabase to reflect the changes in the editor
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log("Texture overwritten at: " + texturePath);
    }
}
