using UnityEngine;

namespace PolarBond.Views
{
    public class CameraFitter : MonoBehaviour
    {
        [Header("UI Viewport Padding (0.0 to 1.0)")]
        [Tooltip("Phần trăm màn hình phía trên dành cho TopPanel (VD: 0.15 = 15%)")]
        public float topViewportPadding = 0.15f;
        
        [Tooltip("Phần trăm màn hình phía dưới dành cho DPadPanel (VD: 0.40 = 40%)")]
        public float bottomViewportPadding = 0.45f;
        
        [Tooltip("Phần trăm màn hình 2 bên lề")]
        public float horizontalViewportPadding = 0.05f;
        
        public void FitCameraToGrid(UnityEngine.Tilemaps.Tilemap[] tilemaps)
        {
            if (tilemaps == null || tilemaps.Length == 0) return;

            Bounds totalBounds = new Bounds();
            bool hasBounds = false;

            foreach (var tm in tilemaps)
            {
                tm.CompressBounds();
                if (tm.cellBounds.size.x > 0 && tm.cellBounds.size.y > 0)
                {
                    Bounds localBounds = tm.localBounds;
                    Bounds worldBounds = new Bounds(tm.transform.TransformPoint(localBounds.center), localBounds.size);
                    
                    if (!hasBounds)
                    {
                        totalBounds = worldBounds;
                        hasBounds = true;
                    }
                    else
                    {
                        totalBounds.Encapsulate(worldBounds);
                    }
                }
            }

            if (!hasBounds) return;

            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic) return;

            float screenAspect = (float)Screen.width / (float)Screen.height;
            
            // Calculate available space ratios
            float availableHeightRatio = Mathf.Clamp(1.0f - topViewportPadding - bottomViewportPadding, 0.1f, 1.0f);
            float availableWidthRatio = Mathf.Clamp(1.0f - horizontalViewportPadding * 2f, 0.1f, 1.0f);

            // Calculate required orthographic sizes to fit the bounds into the available viewport space
            float orthoSizeForHeight = (totalBounds.size.y / 2f) / availableHeightRatio;
            float orthoSizeForWidth = (totalBounds.size.x / 2f) / (screenAspect * availableWidthRatio);

            // Apply the larger size to ensure everything fits
            cam.orthographicSize = Mathf.Max(orthoSizeForHeight, orthoSizeForWidth);

            // Calculate where the center of the available space is on the screen (0.0 to 1.0)
            float viewportCenterY = bottomViewportPadding + (availableHeightRatio / 2f);
            
            // Offset from the true center of the screen (0.5)
            float offsetRatio = viewportCenterY - 0.5f;

            // Shift the camera in world space to align the grid's center with the viewport's safe center
            // (If viewport center is higher (offset > 0), camera must move lower to push objects up)
            float yOffsetWorld = offsetRatio * 2f * cam.orthographicSize;
            
            Vector3 camPos = new Vector3(totalBounds.center.x, totalBounds.center.y - yOffsetWorld, cam.transform.position.z);
            cam.transform.position = camPos;
        }
    }
}
