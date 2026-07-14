using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PolarBond.Views;

namespace PolarBond.Editor
{
    public class MobileUIBuilder
    {
        [MenuItem("PolarBond/Build Mobile UI")]
        public static void BuildUI()
        {
            // 1. Create Canvas
            GameObject canvasGO = new GameObject("MobileCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 1f; // Match width for portrait
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Add EventSystem if missing
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 2. Attach UIManager
            MobileUIManager uiManager = canvasGO.AddComponent<MobileUIManager>();

            // 3. Create Top Panel
            GameObject topPanel = new GameObject("TopPanel");
            topPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform topRect = topPanel.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.anchoredPosition = Vector2.zero;
            topRect.sizeDelta = new Vector2(0, 200);

            // Settings Button (Top Right)
            GameObject btnSettings = CreateButton("Btn_Settings", topPanel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-150, -100), new Vector2(150, 150), "Settings", Color.gray);
            // Undo Button (Next to Settings)
            GameObject btnUndo = CreateButton("Btn_Undo", topPanel.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-350, -100), new Vector2(150, 150), "Undo", Color.gray);

            // 4. Create Bottom D-Pad Panel
            GameObject bottomPanel = new GameObject("DPadPanel");
            bottomPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform bottomRect = bottomPanel.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0);
            bottomRect.pivot = new Vector2(0.5f, 0);
            bottomRect.anchoredPosition = new Vector2(0, 100);
            bottomRect.sizeDelta = new Vector2(1080, 600);

            // Create 4 Directional Buttons
            float btnSize = 200f;
            GameObject btnUp = CreateButton("Btn_Up", bottomPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, btnSize), new Vector2(btnSize, btnSize), "^", Color.white);
            GameObject btnDown = CreateButton("Btn_Down", bottomPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -btnSize), new Vector2(btnSize, btnSize), "v", Color.white);
            GameObject btnLeft = CreateButton("Btn_Left", bottomPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-btnSize, 0), new Vector2(btnSize, btnSize), "<", Color.white);
            GameObject btnRight = CreateButton("Btn_Right", bottomPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(btnSize, 0), new Vector2(btnSize, btnSize), ">", Color.white);

            // Hook up events via UnityEvents
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnUp.GetComponent<Button>().onClick, uiManager.OnClickUp);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnDown.GetComponent<Button>().onClick, uiManager.OnClickDown);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnLeft.GetComponent<Button>().onClick, uiManager.OnClickLeft);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnRight.GetComponent<Button>().onClick, uiManager.OnClickRight);
            
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettings.GetComponent<Button>().onClick, uiManager.OnClickSettings);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnUndo.GetComponent<Button>().onClick, uiManager.OnClickUndo);

            Debug.Log("Mobile UI Canvas built successfully!");
            Selection.activeGameObject = canvasGO;
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, string text, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            Image img = btnObj.AddComponent<Image>();
            img.color = color;

            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text txt = textObj.AddComponent<Text>();
            txt.text = text;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.black;
            txt.fontSize = 50;

            return btnObj;
        }
    }
}
