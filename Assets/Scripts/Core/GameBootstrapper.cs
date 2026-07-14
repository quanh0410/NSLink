using UnityEngine;

namespace PolarBond.Core
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeGame()
        {
            // 1. Tắt VSync. Trên nền tảng di động, nếu VSync bật, hệ điều hành có thể ép khoá FPS ở 30.
            QualitySettings.vSyncCount = 0;

            // 2. Ép mức khung hình mục tiêu là 60 FPS.
            Application.targetFrameRate = 60;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            // 3. Tối ưu hoá độ phân giải trên Mobile để tránh nghẽn cổ chai GPU (Fill-rate bottleneck)
            // Màn hình điện thoại ngày nay thường là 1.5K hoặc 2K, ép render ở 1080p sẽ giúp máy mát và mượt mà hơn.
            int maxResolutionHeight = 1080; // Bạn có thể giảm xuống 720 nếu muốn tối ưu sâu hơn
            
            // Ở chế độ màn hình dọc (Portrait), chiều cao là cạnh dài.
            if (Screen.height > maxResolutionHeight)
            {
                float aspect = (float)Screen.width / Screen.height;
                int targetHeight = maxResolutionHeight;
                int targetWidth = Mathf.RoundToInt(targetHeight * aspect);
                
                Screen.SetResolution(targetWidth, targetHeight, true);
                Debug.Log($"[GameBootstrapper] Mobile Build: Downscaling resolution to {targetWidth}x{targetHeight} @ 60 FPS");
            }
            else
            {
                Debug.Log($"[GameBootstrapper] Mobile Build: Running at native resolution @ 60 FPS");
            }
#else
            Debug.Log("[GameBootstrapper] Editor/PC Build: VSync OFF, Target 60 FPS");
#endif
        }
    }
}
