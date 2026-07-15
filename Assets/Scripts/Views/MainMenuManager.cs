using UnityEngine;

namespace PolarBond.Views
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance;

        public GameObject mainMenuCanvas;
        public GameObject optionsPopup;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Ensure main menu is visible and options is hidden on start
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            mainMenuCanvas.SetActive(true);
            SetVisualsActive(true);
            optionsPopup.SetActive(false);
            
            CanvasGroup cg = mainMenuCanvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = mainMenuCanvas.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            UITweener.FadeTo(cg, 1f, 0.3f);
            
            // Hide other canvases if they exist
            if (LevelSelectManager.Instance != null)
                LevelSelectManager.Instance.levelSelectCanvas.SetActive(false);
                
            var uis = FindObjectsOfType<MobileUIManager>(true);
            foreach (var ui in uis)
            {
                ui.gameObject.SetActive(false);
            }
            
            if (PolarBond.Managers.LevelManager.Instance != null)
            {
                PolarBond.Managers.LevelManager.Instance.ClearLevel();
            }
        }

        public void HideMainMenu()
        {
            SetVisualsActive(false);
        }

        private void SetVisualsActive(bool active)
        {
            foreach (Transform child in mainMenuCanvas.transform)
            {
                if (child.gameObject != optionsPopup)
                {
                    child.gameObject.SetActive(active);
                }
            }
        }

        public void OnClickPlay()
        {
            if (LevelSelectManager.Instance != null)
            {
                LevelSelectManager.Instance.ShowLevelSelect();
            }
            HideMainMenu();
        }

        public void OnClickOptions()
        {
            optionsPopup.SetActive(true);
            optionsPopup.transform.localScale = Vector3.zero;
            UITweener.ScaleTo(optionsPopup.transform, Vector3.one, 0.3f);
        }

        public void OnClickExit()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
