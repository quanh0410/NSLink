using UnityEngine;
using UnityEngine.UI;
using PolarBond.Logic;
using PolarBond.Core;
using PolarBond.Managers;

namespace PolarBond.Views
{
    public class MobileUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameLoopController gameController;
        private Button btnUndo;
        private bool lastUndoState = false;

        private void Start()
        {
            if (gameController == null)
            {
                gameController = FindObjectOfType<GameLoopController>();
            }

            // 1. Auto-fix EventSystem if using wrong Input Module (New Input System mismatch)
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                var oldModule = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                if (oldModule != null)
                {
                    Destroy(oldModule);
                    eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    Debug.Log("[MobileUIManager] Replaced StandaloneInputModule with InputSystemUIInputModule.");
                }
            }

            // 2. Add Button Juice Effect to all buttons in Canvas
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.GetComponent<UIButtonEffect>() == null)
                {
                    btn.gameObject.AddComponent<UIButtonEffect>();
                }
                
                if (btn.gameObject.name == "Btn_Undo")
                {
                    btnUndo = btn;
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickUndo);
                }
                if (btn.gameObject.name == "Btn_Settings")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickSettings);
                }
                if (btn.gameObject.name == "Btn_Home")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickHome);
                }
                if (btn.gameObject.name == "Btn_Restart")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickRestart);
                }
                if (btn.gameObject.name == "Btn_Up")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickUp);
                }
                if (btn.gameObject.name == "Btn_Down")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickDown);
                }
                if (btn.gameObject.name == "Btn_Left")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickLeft);
                }
                if (btn.gameObject.name == "Btn_Right")
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(OnClickRight);
                }
            }

            if (gameController != null)
            {
                gameController.OnUndoStateChanged += UpdateUndoButtonState;
                UpdateUndoButtonState();
            }
        }

        private void OnDestroy()
        {
            if (gameController != null)
            {
                gameController.OnUndoStateChanged -= UpdateUndoButtonState;
            }
        }

        private void UpdateUndoButtonState()
        {
            if (btnUndo != null && gameController != null)
            {
                bool canUndo = gameController.CanUndo();
                if (btnUndo.interactable != canUndo)
                {
                    btnUndo.interactable = canUndo;
                    lastUndoState = canUndo;
                }
            }
        }

        public void AssignGameController(GameLoopController controller)
        {
            if (gameController != null)
            {
                gameController.OnUndoStateChanged -= UpdateUndoButtonState;
            }
            gameController = controller;
            if (gameController != null)
            {
                gameController.OnUndoStateChanged += UpdateUndoButtonState;
                UpdateUndoButtonState();
            }
        }

        private bool hasSearchedController = false;

        private GameLoopController GetController()
        {
            if (gameController == null && !hasSearchedController)
            {
                gameController = FindObjectOfType<GameLoopController>();
                if (gameController != null)
                {
                    gameController.OnUndoStateChanged += UpdateUndoButtonState;
                    UpdateUndoButtonState();
                }
                hasSearchedController = true;
            }
            return gameController;
        }

        public void OnClickUp()
        {
            if (GetController() != null) GetController().ProcessTurn(Direction.Up);
        }

        public void OnClickDown()
        {
            if (GetController() != null) GetController().ProcessTurn(Direction.Down);
        }

        public void OnClickLeft()
        {
            if (GetController() != null) GetController().ProcessTurn(Direction.Left);
        }

        public void OnClickRight()
        {
            if (GetController() != null) GetController().ProcessTurn(Direction.Right);
        }

        public void OnClickUndo()
        {
            Debug.Log("[MobileUIManager] OnClickUndo clicked!");
            if (GetController() != null) 
            {
                if (GetController().CanUndo())
                {
                    GetController().UndoTurn();
                }
                else
                {
                    Debug.Log("[MobileUIManager] No states to undo!");
                }
            }
            else
            {
                Debug.LogError("[MobileUIManager] GameLoopController is NULL when clicking Undo!");
            }
        }

        public void OnClickSettings()
        {
            if (MainMenuManager.Instance != null && MainMenuManager.Instance.optionsPopup != null)
            {
                MainMenuManager.Instance.OnClickOptions();
            }
            else
            {
                Debug.LogWarning("[MobileUIManager] MainMenuManager or OptionsPopup is not available.");
            }
        }

        public void OnClickHome()
        {
            if (MainMenuManager.Instance != null)
            {
                MainMenuManager.Instance.ShowMainMenu();
            }
            else
            {
                Debug.LogWarning("[MobileUIManager] MainMenuManager not found.");
            }
        }

        public void OnClickRestart()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }
            else
            {
                Debug.LogWarning("[MobileUIManager] LevelManager not found.");
            }
        }
    }
}
