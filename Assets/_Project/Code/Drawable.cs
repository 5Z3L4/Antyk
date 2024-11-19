using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace FreeDraw
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Drawable : MonoBehaviour
    {
        public static Color PenColor = Color.black;
        public static int PenWidth = 3;

        public delegate void BrushFunction(Vector2 worldPosition);
        public BrushFunction CurrentBrush;

        public LayerMask DrawingLayers;
        public bool ResetCanvasOnPlay = true;
        public Color ResetColor = new Color(0, 0, 0, 0);

        public static Drawable Instance;
        Sprite drawableSprite;
        Texture2D drawableTexture;

        Vector2 previousDragPosition;
        Color[] cleanColorsArray;
        Color transparent;
        Color32[] currentColors;
        bool mousePreviouslyHeldDown = false;
        bool noDrawingOnCurrentDrag = false;
        private Camera camera;

        public void PenBrush(Vector3 worldPoint)
        {
            Vector3 pixelPosition = WorldToPixelCoordinates(worldPoint);
            currentColors = drawableTexture.GetPixels32();

            if (previousDragPosition == Vector2.zero)
            {
                ColorPixels(pixelPosition, PenWidth, PenColor);
            }
            else
            {
                ColorBetween(previousDragPosition, pixelPosition, PenWidth, PenColor);
            }

            previousDragPosition = pixelPosition;
        }

        void Update()
        {
            bool mouseHeldDown = Input.GetMouseButton(0);
            if (mouseHeldDown && !noDrawingOnCurrentDrag)
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, DrawingLayers.value))
                {
                    PenBrush(hit.point);
                }
                else
                {
                    previousDragPosition = Vector2.zero;
                    if (!mousePreviouslyHeldDown)
                    {
                        noDrawingOnCurrentDrag = true;
                    }
                }
            }
            else if (!mouseHeldDown)
            {
                previousDragPosition = Vector2.zero;
                noDrawingOnCurrentDrag = false;
            }
            mousePreviouslyHeldDown = mouseHeldDown;
        }

        public void ColorBetween(Vector2 startPoint, Vector2 endPoint, int width, Color color)
        {
            float distance = Vector2.Distance(startPoint, endPoint);
            Vector2 direction = (startPoint - endPoint).normalized;

            Vector2 currentPosition = startPoint;
            float lerpSteps = 1 / distance;

            for (float lerp = 0; lerp <= 1; lerp += lerpSteps)
            {
                currentPosition = Vector2.Lerp(startPoint, endPoint, lerp);
                ColorPixels(currentPosition, width, color);
            }
        }

        public void MarkPixelsToColor(Vector2 centerPixel, int penThickness, Color penColor)
        {
            int centerX = (int)centerPixel.x;
            int centerY = (int)centerPixel.y;

            for (int x = centerX - penThickness; x <= centerX + penThickness; x++)
            {
                if (x >= (int)drawableSprite.rect.width || x < 0)
                    continue;

                for (int y = centerY - penThickness; y <= centerY + penThickness; y++)
                {
                    MarkPixelToChange(x, y, penColor);
                }
            }
        }

        public void MarkPixelToChange(int x, int y, Color color)
        {
            int arrayPos = y * (int)drawableSprite.rect.width + x;

            if (arrayPos > currentColors.Length || arrayPos < 0)
                return;

            currentColors[arrayPos] = color;
        }

        public void ApplyMarkedPixelChanges()
        {
            drawableTexture.SetPixels32(currentColors);
            drawableTexture.Apply();
        }

        public void ColorPixels(Vector2 centerPixel, int penThickness, Color penColor)
        {
            int centerX = (int)centerPixel.x;
            int centerY = (int)centerPixel.y;

            for (int x = centerX - penThickness; x <= centerX + penThickness; x++)
            {
                for (int y = centerY - penThickness; y <= centerY + penThickness; y++)
                {
                    drawableTexture.SetPixel(x, y, penColor);
                }
            }
            drawableTexture.Apply();
        }

        public Vector3 WorldToPixelCoordinates(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            float pixelWidth = drawableSprite.rect.width;
            float pixelHeight = drawableSprite.rect.height;
            float unitsToPixels = pixelWidth / drawableSprite.bounds.size.x * transform.localScale.x;

            float centeredX = localPos.x * unitsToPixels + pixelWidth / 2;
            float centeredY = localPos.y * unitsToPixels + pixelHeight / 2;

            Vector2 pixelPos = new Vector2(Mathf.RoundToInt(centeredX), Mathf.RoundToInt(centeredY));

            return pixelPos;
        }

        public void ResetCanvas()
        {
            drawableTexture.SetPixels(cleanColorsArray);
            drawableTexture.Apply();
        }

        void Awake()
        {
            Instance = this;

            drawableSprite = this.GetComponent<SpriteRenderer>().sprite;
            drawableTexture = drawableSprite.texture;

            cleanColorsArray = new Color[(int)drawableSprite.rect.width * (int)drawableSprite.rect.height];
            for (int x = 0; x < cleanColorsArray.Length; x++)
                cleanColorsArray[x] = ResetColor;

            if (ResetCanvasOnPlay)
                ResetCanvas();

            camera = FindObjectOfType<Camera>();
        }
    }
}
