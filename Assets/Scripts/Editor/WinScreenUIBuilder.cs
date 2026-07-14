using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PolarBond.Views;

namespace PolarBond.Editor
{
    public class WinScreenUIBuilder
    {
        [MenuItem("PolarBond/Build Win Screen UI")]
        public static void BuildUI()
        {
            // Destroy existing canvas if any
            var existing = GameObject.Find("WinScreenCanvas");
            if (existing != null) Object.DestroyImmediate(existing);

            // Load sprites
            Sprite bannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/BoxesBanners/Banner_Orange.png");
            Sprite btnBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsIcons/IconButton_Large_Blue_Rounded.png");

            if (bannerSprite == null || btnBg == null)
            {
                Debug.LogError("Failed to load sprites for Win Screen. Check paths!");
                return;
            }

            // 1. Create Canvas
            GameObject canvasGO = new GameObject("WinScreenCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20; // High order to appear above everything else
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f; // Portrait
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Add Dark Overlay Background
            GameObject bgPanel = new GameObject("DarkOverlay");
            bgPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgPanel.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.8f); // 80% opacity black

            // Add Banner
            GameObject bannerObj = new GameObject("Banner");
            bannerObj.transform.SetParent(canvasGO.transform, false);
            RectTransform bannerRect = bannerObj.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0, 200); // Shifted up slightly
            bannerRect.sizeDelta = new Vector2(800, 300);
            Image bannerImg = bannerObj.AddComponent<Image>();
            bannerImg.sprite = bannerSprite;
            bannerImg.type = Image.Type.Sliced; // In case it's a 9-slice sprite

            // Banner Text
            GameObject titleObj = new GameObject("Text");
            titleObj.transform.SetParent(bannerObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.sizeDelta = Vector2.zero;
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "CONGRATULATIONS";
            titleTxt.fontSize = 70;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.fontStyle = FontStyle.Bold;

            // Next Level Button
            GameObject nextBtnObj = new GameObject("Btn_NextLevel");
            nextBtnObj.transform.SetParent(canvasGO.transform, false);
            RectTransform nextRect = nextBtnObj.AddComponent<RectTransform>();
            nextRect.anchorMin = new Vector2(0.5f, 0.5f);
            nextRect.anchorMax = new Vector2(0.5f, 0.5f);
            nextRect.pivot = new Vector2(0.5f, 0.5f);
            nextRect.anchoredPosition = new Vector2(0, -200); // Below banner
            nextRect.sizeDelta = new Vector2(500, 150);
            Image nextImg = nextBtnObj.AddComponent<Image>();
            nextImg.sprite = btnBg;
            nextImg.type = Image.Type.Sliced;
            Button nextBtn = nextBtnObj.AddComponent<Button>();

            // Next Level Text
            GameObject nextTxtObj = new GameObject("Text");
            nextTxtObj.transform.SetParent(nextBtnObj.transform, false);
            RectTransform nextTxtRect = nextTxtObj.AddComponent<RectTransform>();
            nextTxtRect.anchorMin = Vector2.zero;
            nextTxtRect.anchorMax = Vector2.one;
            nextTxtRect.sizeDelta = Vector2.zero;
            Text nextTxt = nextTxtObj.AddComponent<Text>();
            nextTxt.text = "NEXT LEVEL";
            nextTxt.fontSize = 60;
            nextTxt.alignment = TextAnchor.MiddleCenter;
            nextTxt.color = Color.white;
            nextTxt.fontStyle = FontStyle.Bold;

            // 3. Attach Manager
            WinScreenManager manager = canvasGO.AddComponent<WinScreenManager>();
            manager.winScreenCanvas = canvasGO;

            // Hook up onClick
            UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.onClick, manager.OnClickNextLevel);

            // Add UIButtonEffect for juice
            nextBtnObj.AddComponent<UIButtonEffect>();

            canvasGO.SetActive(false); // Hide by default

            Debug.Log("Win Screen UI Canvas built successfully!");
            Selection.activeGameObject = canvasGO;
        }
    }
}
