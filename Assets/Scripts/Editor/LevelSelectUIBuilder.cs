using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PolarBond.Views;

namespace PolarBond.Editor
{
    public class LevelSelectUIBuilder
    {
        [MenuItem("PolarBond/Build Level Select UI")]
        public static void BuildUI()
        {
            // Destroy existing canvas if any
            var existing = GameObject.Find("LevelSelectCanvas");
            if (existing != null) Object.DestroyImmediate(existing);

            // Load sprites
            Sprite bgUnlocked = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsIcons/IconButton_Large_Blue_Rounded.png");
            Sprite bgLocked = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsIcons/IconButton_Large_GreyOutline_Rounded.png");
            Sprite lockIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/Icons/Icon_Small_Lock.png");
            Sprite scrollBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/Sliders/SlimSlider_Background.png");
            Sprite scrollHandle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/Sliders/SlimSlider_Button.png");
            
            if (bgUnlocked == null || lockIcon == null)
            {
                Debug.LogError("Failed to load sprites. Check paths!");
                return;
            }

            // 1. Create Canvas
            GameObject canvasGO = new GameObject("LevelSelectCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // Overlay above main game canvas
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f; // Portrait
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Add Background (Fully Opaque)
            GameObject bgPanel = new GameObject("Background");
            bgPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgPanel.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 1.0f); // Fully Opaque

            // Add Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasGO.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -150);
            titleRect.sizeDelta = new Vector2(800, 200);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "SELECT LEVEL";
            titleTxt.fontSize = 100;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.fontStyle = FontStyle.Bold;

            // Close Button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(canvasGO.transform, false);
            RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.anchoredPosition = new Vector2(-50, -50);
            closeRect.sizeDelta = new Vector2(150, 150);
            Image closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = Color.red;
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            GameObject closeTxtObj = new GameObject("Text");
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            Text closeTxt = closeTxtObj.AddComponent<Text>();
            closeTxt.text = "X";
            closeTxt.fontSize = 80;
            closeTxt.alignment = TextAnchor.MiddleCenter;
            closeTxt.color = Color.white;
            closeTxtObj.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);

            // Add Scroll View
            GameObject scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(canvasGO.transform, false);
            RectTransform scrollRectTransform = scrollViewObj.AddComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0, 0);
            scrollRectTransform.anchorMax = new Vector2(1, 1);
            scrollRectTransform.offsetMin = new Vector2(50, 100);
            scrollRectTransform.offsetMax = new Vector2(-50, -400);

            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 50f;

            // Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = new Vector2(0, 1);
            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.01f); // Almost invisible
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0); // Height driven by fitter

            GridLayoutGroup grid = contentObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(250, 250);
            grid.spacing = new Vector2(50, 50);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;

            // Scrollbar Vertical
            GameObject scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1, 0);
            scrollbarRect.anchorMax = new Vector2(1, 1);
            scrollbarRect.pivot = new Vector2(1, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(40, 0);
            scrollbarRect.anchoredPosition = new Vector2(20, 0);
            Image scrollbarBg = scrollbarObj.AddComponent<Image>();
            if (scrollBg != null) { scrollbarBg.sprite = scrollBg; scrollbarBg.type = Image.Type.Sliced; }
            Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingAreaObj = new GameObject("Sliding Area");
            slidingAreaObj.transform.SetParent(scrollbarObj.transform, false);
            RectTransform slidingAreaRect = slidingAreaObj.AddComponent<RectTransform>();
            slidingAreaRect.anchorMin = Vector2.zero;
            slidingAreaRect.anchorMax = Vector2.one;
            slidingAreaRect.sizeDelta = new Vector2(-20, -20);

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(slidingAreaObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 0);
            handleRect.anchorMax = new Vector2(1, 0.2f);
            Image handleImage = handleObj.AddComponent<Image>();
            if (scrollHandle != null) { handleImage.sprite = scrollHandle; handleImage.type = Image.Type.Sliced; }
            
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            // 2. Create LevelButton Prefab Template
            GameObject btnTemplate = new GameObject("LevelButtonPrefab");
            RectTransform btnRect = btnTemplate.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(250, 250);
            Image btnBg = btnTemplate.AddComponent<Image>();
            btnBg.sprite = bgUnlocked;
            Button btnComp = btnTemplate.AddComponent<Button>();

            // Text
            GameObject numObj = new GameObject("NumberText");
            numObj.transform.SetParent(btnTemplate.transform, false);
            RectTransform numRect = numObj.AddComponent<RectTransform>();
            numRect.anchorMin = Vector2.zero;
            numRect.anchorMax = Vector2.one;
            numRect.sizeDelta = Vector2.zero;
            Text numTxt = numObj.AddComponent<Text>();
            numTxt.text = "1";
            numTxt.fontSize = 120;
            numTxt.alignment = TextAnchor.MiddleCenter;
            numTxt.color = Color.white;
            numTxt.fontStyle = FontStyle.Bold;


            // Lock Icon
            GameObject lockObj = new GameObject("LockIcon");
            lockObj.transform.SetParent(btnTemplate.transform, false);
            RectTransform lockRect = lockObj.AddComponent<RectTransform>();
            lockRect.anchorMin = new Vector2(0.5f, 0.5f);
            lockRect.anchorMax = new Vector2(0.5f, 0.5f);
            lockRect.pivot = new Vector2(0.5f, 0.5f);
            lockRect.anchoredPosition = Vector2.zero;
            lockRect.sizeDelta = new Vector2(120, 120);
            Image lockImage = lockObj.AddComponent<Image>();
            lockImage.sprite = lockIcon;
            lockObj.SetActive(false);

            // Add LevelButtonUI component
            LevelButtonUI btnUI = btnTemplate.AddComponent<LevelButtonUI>();
            btnUI.backgroundImage = btnBg;
            btnUI.levelText = numTxt;
            btnUI.lockIcon = lockObj;
            btnUI.bgUnlockedSprite = bgUnlocked;
            btnUI.bgLockedSprite = bgLocked;

            // Hook up onClick
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnComp.onClick, btnUI.OnClickButton);

            // Add UIButtonEffect for juice!
            btnTemplate.AddComponent<UIButtonEffect>();

            // Create Prefab
            if (!System.IO.Directory.Exists("Assets/Prefabs/UI"))
            {
                System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            }
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(btnTemplate, "Assets/Prefabs/UI/LevelButtonPrefab.prefab");
            Object.DestroyImmediate(btnTemplate);

            // 3. Attach LevelSelectManager
            LevelSelectManager manager = canvasGO.AddComponent<LevelSelectManager>();
            manager.levelSelectCanvas = canvasGO;
            manager.buttonsContainer = contentObj.transform;
            manager.levelButtonPrefab = savedPrefab;
            
            // Try to find MobileGameCanvas to hide it automatically
            var uis = Object.FindObjectsOfType<MobileUIManager>(true);
            if (uis.Length > 0)
            {
                manager.mobileGameCanvas = uis[0].gameObject;
            }

            // Also add juice effect to close button
            closeBtnObj.AddComponent<UIButtonEffect>();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, manager.CloseToMainMenu);

            canvasGO.SetActive(false); // Hide by default

            Debug.Log("Level Select UI Canvas built successfully with ScrollView!");
            Selection.activeGameObject = canvasGO;
        }
    }
}
