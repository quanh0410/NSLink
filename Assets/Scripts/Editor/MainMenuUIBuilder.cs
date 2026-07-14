using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PolarBond.Views;

namespace PolarBond.Editor
{
    public class MainMenuUIBuilder
    {
        [MenuItem("PolarBond/Build Main Menu UI")]
        public static void BuildUI()
        {
            var existing = GameObject.Find("MainMenuCanvas");
            if (existing != null) Object.DestroyImmediate(existing);

            // Load sprites
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/BoxesBanners/Banner_Blank.png");
            Sprite playBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsText/ButtonText_Large_Blue_Round.png");
            Sprite optionsBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsText/PremadeButtons_Options.png");
            Sprite exitBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsText/PremadeButtons_ExitRed.png");
            
            Sprite popupBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/BoxesBanners/Box_Blank_Rounded.png");
            Sprite sliderBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/Sliders/WideSlider_Background.png");
            Sprite sliderHandleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/Sliders/WideSlider_Button.png");
            Sprite audioIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/Icons/Icon_Large_Audio_Blank.png");
            Sprite textboxBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/BoxesBanners/TextBox_Blank_Middle.png");
            Sprite closeBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Basic_GUI_Bundle/ButtonsText/PremadeButtons_XRed.png");

            // 1. Create Canvas
            GameObject canvasGO = new GameObject("MainMenuCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20; // Highest
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            MainMenuManager menuManager = canvasGO.AddComponent<MainMenuManager>();
            menuManager.mainMenuCanvas = canvasGO;

            // Background
            GameObject bgPanel = new GameObject("Background");
            bgPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgPanel.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.4f, 0.6f, 1f); // Nice blue

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasGO.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(900, 300);
            Text titleTxt = titleObj.AddComponent<Text>();
            titleTxt.text = "POLAR BOND";
            titleTxt.fontSize = 150;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.fontStyle = FontStyle.Bold;

            // Container for buttons
            GameObject btnsObj = new GameObject("ButtonsContainer");
            btnsObj.transform.SetParent(canvasGO.transform, false);
            RectTransform btnsRect = btnsObj.AddComponent<RectTransform>();
            btnsRect.anchorMin = new Vector2(0.5f, 0.4f);
            btnsRect.anchorMax = new Vector2(0.5f, 0.4f);
            btnsRect.pivot = new Vector2(0.5f, 0.5f);
            btnsRect.sizeDelta = new Vector2(600, 800);
            VerticalLayoutGroup vLayout = btnsObj.AddComponent<VerticalLayoutGroup>();
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.spacing = 80;
            vLayout.childControlHeight = false;
            vLayout.childControlWidth = false;

            // Play Button
            GameObject playBtnObj = new GameObject("PlayButton");
            playBtnObj.transform.SetParent(btnsObj.transform, false);
            RectTransform playRect = playBtnObj.AddComponent<RectTransform>();
            playRect.sizeDelta = new Vector2(500, 200);
            Image playImg = playBtnObj.AddComponent<Image>();
            if (playBtnSprite != null) playImg.sprite = playBtnSprite;
            Button playBtn = playBtnObj.AddComponent<Button>();
            GameObject playTxtObj = new GameObject("Text");
            playTxtObj.transform.SetParent(playBtnObj.transform, false);
            Text playTxt = playTxtObj.AddComponent<Text>();
            playTxt.text = "PLAY";
            playTxt.fontSize = 80;
            playTxt.fontStyle = FontStyle.Bold;
            playTxt.color = Color.white;
            playTxt.alignment = TextAnchor.MiddleCenter;
            playTxtObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            playTxtObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
            playTxtObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            // Options Button
            GameObject optBtnObj = new GameObject("OptionsButton");
            optBtnObj.transform.SetParent(btnsObj.transform, false);
            RectTransform optRect = optBtnObj.AddComponent<RectTransform>();
            optRect.sizeDelta = new Vector2(250, 250);
            Image optImg = optBtnObj.AddComponent<Image>();
            if (optionsBtnSprite != null) optImg.sprite = optionsBtnSprite;
            Button optBtn = optBtnObj.AddComponent<Button>();

            // Exit Button
            GameObject exitBtnObj = new GameObject("ExitButton");
            exitBtnObj.transform.SetParent(btnsObj.transform, false);
            RectTransform exitRect = exitBtnObj.AddComponent<RectTransform>();
            exitRect.sizeDelta = new Vector2(250, 250);
            Image exitImg = exitBtnObj.AddComponent<Image>();
            if (exitBtnSprite != null) exitImg.sprite = exitBtnSprite;
            Button exitBtn = exitBtnObj.AddComponent<Button>();

            // Add juice effect
            playBtnObj.AddComponent<UIButtonEffect>();
            optBtnObj.AddComponent<UIButtonEffect>();
            exitBtnObj.AddComponent<UIButtonEffect>();

            // ---------------- OPTIONS POPUP ----------------
            GameObject popupObj = new GameObject("OptionsPopup");
            popupObj.transform.SetParent(canvasGO.transform, false);
            RectTransform popupRect = popupObj.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.sizeDelta = Vector2.zero;
            
            // Dim background
            GameObject dimBg = new GameObject("DimBg");
            dimBg.transform.SetParent(popupObj.transform, false);
            RectTransform dimRect = dimBg.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;
            Image dimImg = dimBg.AddComponent<Image>();
            dimImg.color = new Color(0, 0, 0, 0.7f);

            // Window
            GameObject windowObj = new GameObject("Window");
            windowObj.transform.SetParent(popupObj.transform, false);
            RectTransform winRect = windowObj.AddComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(800, 1000);
            Image winImg = windowObj.AddComponent<Image>();
            if (popupBgSprite != null) { winImg.sprite = popupBgSprite; winImg.type = Image.Type.Sliced; }
            
            OptionsManager optManager = popupObj.AddComponent<OptionsManager>();

            // Title Options
            GameObject optTitleObj = new GameObject("Title");
            optTitleObj.transform.SetParent(windowObj.transform, false);
            RectTransform optTitleRect = optTitleObj.AddComponent<RectTransform>();
            optTitleRect.anchorMin = new Vector2(0.5f, 1);
            optTitleRect.anchorMax = new Vector2(0.5f, 1);
            optTitleRect.pivot = new Vector2(0.5f, 1);
            optTitleRect.anchoredPosition = new Vector2(0, -50);
            optTitleRect.sizeDelta = new Vector2(400, 150);
            Text optTitleTxt = optTitleObj.AddComponent<Text>();
            optTitleTxt.text = "OPTIONS";
            optTitleTxt.fontSize = 80;
            optTitleTxt.fontStyle = FontStyle.Bold;
            optTitleTxt.color = Color.black;
            optTitleTxt.alignment = TextAnchor.MiddleCenter;

            // Volume Section
            GameObject volIconObj = new GameObject("AudioIcon");
            volIconObj.transform.SetParent(windowObj.transform, false);
            RectTransform volIconRect = volIconObj.AddComponent<RectTransform>();
            volIconRect.anchorMin = new Vector2(0, 0.7f);
            volIconRect.anchorMax = new Vector2(0, 0.7f);
            volIconRect.pivot = new Vector2(0, 0.5f);
            volIconRect.anchoredPosition = new Vector2(50, 0);
            volIconRect.sizeDelta = new Vector2(100, 100);
            Image volImg = volIconObj.AddComponent<Image>();
            if (audioIconSprite != null) volImg.sprite = audioIconSprite;

            GameObject sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(windowObj.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.7f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.7f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(60, 0);
            sliderRect.sizeDelta = new Vector2(500, 60);
            Image sliderBgImg = sliderObj.AddComponent<Image>();
            if (sliderBgSprite != null) { sliderBgImg.sprite = sliderBgSprite; sliderBgImg.type = Image.Type.Sliced; }

            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            // No fill image for this style, we just use handle

            GameObject handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-40, 0);

            GameObject handleHandleObj = new GameObject("Handle");
            handleHandleObj.transform.SetParent(handleAreaObj.transform, false);
            RectTransform handleHandleRect = handleHandleObj.AddComponent<RectTransform>();
            handleHandleRect.sizeDelta = new Vector2(80, 80);
            Image handleImg = handleHandleObj.AddComponent<Image>();
            if (sliderHandleSprite != null) handleImg.sprite = sliderHandleSprite;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.handleRect = handleHandleRect;
            slider.value = 1f;
            optManager.volumeSlider = slider;

            // Instructions text
            GameObject instBgObj = new GameObject("InstructionsBg");
            instBgObj.transform.SetParent(windowObj.transform, false);
            RectTransform instBgRect = instBgObj.AddComponent<RectTransform>();
            instBgRect.anchorMin = new Vector2(0.5f, 0.3f);
            instBgRect.anchorMax = new Vector2(0.5f, 0.3f);
            instBgRect.pivot = new Vector2(0.5f, 0.5f);
            instBgRect.anchoredPosition = Vector2.zero;
            instBgRect.sizeDelta = new Vector2(700, 400);
            Image instBgImg = instBgObj.AddComponent<Image>();
            if (textboxBgSprite != null) { instBgImg.sprite = textboxBgSprite; instBgImg.type = Image.Type.Sliced; }

            GameObject instTxtObj = new GameObject("InstructionsText");
            instTxtObj.transform.SetParent(instBgObj.transform, false);
            RectTransform instTxtRect = instTxtObj.AddComponent<RectTransform>();
            instTxtRect.anchorMin = Vector2.zero;
            instTxtRect.anchorMax = Vector2.one;
            instTxtRect.sizeDelta = new Vector2(-40, -40);
            Text instTxt = instTxtObj.AddComponent<Text>();
            instTxt.text = "HƯỚNG DẪN CHƠI\n\n- Vuốt để đẩy các nam châm.\n- Cùng cực thì đẩy nhau, khác cực thì hút nhau.\n- Hãy đưa nam châm vào đúng mục tiêu!";
            instTxt.fontSize = 45;
            instTxt.color = Color.black;
            instTxt.alignment = TextAnchor.MiddleCenter;

            // Close Button
            GameObject closeOptBtnObj = new GameObject("CloseButton");
            closeOptBtnObj.transform.SetParent(windowObj.transform, false);
            RectTransform closeOptRect = closeOptBtnObj.AddComponent<RectTransform>();
            closeOptRect.anchorMin = new Vector2(1, 1);
            closeOptRect.anchorMax = new Vector2(1, 1);
            closeOptRect.pivot = new Vector2(1, 1);
            closeOptRect.anchoredPosition = new Vector2(30, 30);
            closeOptRect.sizeDelta = new Vector2(120, 120);
            Image closeOptImg = closeOptBtnObj.AddComponent<Image>();
            if (closeBtnSprite != null) closeOptImg.sprite = closeBtnSprite;
            Button closeOptBtn = closeOptBtnObj.AddComponent<Button>();
            closeOptBtnObj.AddComponent<UIButtonEffect>();

            // Hook up events
            UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, menuManager.OnClickPlay);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(optBtn.onClick, menuManager.OnClickOptions);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(exitBtn.onClick, menuManager.OnClickExit);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeOptBtn.onClick, optManager.OnClickClose);

            menuManager.optionsPopup = popupObj;
            popupObj.SetActive(false); // Hide options by default

            Debug.Log("Main Menu UI Canvas built successfully!");
            Selection.activeGameObject = canvasGO;
        }
    }
}
