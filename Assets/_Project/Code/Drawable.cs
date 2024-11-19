using UnityEngine;

public class ScribbleSurface : MonoBehaviour
{
    public LayerMask InteractionLayer;
    public Color BackgroundColor = new(0, 0, 0, 0);

    public bool ClearTextureOnStart = true;
    private Sprite surfaceSprite;
    private Texture2D surfaceTexture;

    private Vector2 lastDragPosition;
    private Color[] baseColors;
    private Color clearColor;
    private Color32[] currentPixelColors;
    private bool wasMouseDownLastFrame = false;
    private bool skipDrawingThisDrag = false;
    private Camera mainCamera;

    void Start()
    {
        surfaceSprite = GetComponent<SpriteRenderer>().sprite;
        surfaceTexture = surfaceSprite.texture;

        baseColors = new Color[(int)surfaceSprite.rect.width * (int)surfaceSprite.rect.height];
        for (int i = 0; i < baseColors.Length; i++)
            baseColors[i] = BackgroundColor;

        if (ClearTextureOnStart)
            ClearSurface();

        mainCamera = FindObjectOfType<Camera>();
    }

    void Update()
    {
        bool isMouseDown = Input.GetMouseButton(0);
        if (isMouseDown && !skipDrawingThisDrag)
        {
            Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo, Mathf.Infinity, InteractionLayer.value))
            {
                DrawLine(hitInfo.point);
            }
            else
            {
                lastDragPosition = Vector2.zero;
                if (!wasMouseDownLastFrame)
                {
                    skipDrawingThisDrag = true;
                }
            }
        }
        else if (!isMouseDown)
        {
            lastDragPosition = Vector2.zero;
            skipDrawingThisDrag = false;
        }

        wasMouseDownLastFrame = isMouseDown;
    }

    public void DrawLine(Vector3 worldPosition)
    {
        Vector3 pixelCoords = TransformToPixelCoordinates(worldPosition);
        currentPixelColors = surfaceTexture.GetPixels32();

        if (lastDragPosition == Vector2.zero)
            FillPixels(pixelCoords, 3, Color.black);
        else
            DrawLineSegment(lastDragPosition, pixelCoords, 3, Color.black);

        lastDragPosition = pixelCoords;
    }

    public void DrawLineSegment(Vector2 startPoint, Vector2 endPoint, int thickness, Color drawColor)
    {
        float segmentLength = Vector2.Distance(startPoint, endPoint);

        Vector2 interpolatedPosition = startPoint;
        float interpolationSteps = 1 / segmentLength;

        for (float t = 0; t <= 1; t += interpolationSteps)
        {
            interpolatedPosition = Vector2.Lerp(startPoint, endPoint, t);
            FillPixels(interpolatedPosition, thickness, drawColor);
        }
    }
    
    public void FillPixels(Vector2 center, int radius, Color color)
    {
        int centerX = (int)center.x;
        int centerY = (int)center.y;

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                surfaceTexture.SetPixel(x, y, color);
            }
        }

        surfaceTexture.Apply();
    }

    public Vector3 TransformToPixelCoordinates(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        float textureWidth = surfaceSprite.rect.width;
        float textureHeight = surfaceSprite.rect.height;
        float scaleFactor = textureWidth / surfaceSprite.bounds.size.x * transform.localScale.x;

        float adjustedX = localPosition.x * scaleFactor + textureWidth / 2;
        float adjustedY = localPosition.y * scaleFactor + textureHeight / 2;

        Vector2 pixelCoords = new(Mathf.RoundToInt(adjustedX), Mathf.RoundToInt(adjustedY));

        return pixelCoords;
    }

    public void ClearSurface()
    {
        surfaceTexture.SetPixels(baseColors);
        surfaceTexture.Apply();
    }
}
