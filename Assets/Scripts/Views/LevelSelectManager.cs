using UnityEngine;
using PolarBond.Managers;

namespace PolarBond.Views
{
    public class LevelSelectManager : MonoBehaviour
    {
        public static LevelSelectManager Instance;

        public GameObject levelSelectCanvas;
        public GameObject mobileGameCanvas; // The main game UI canvas

        public Transform buttonsContainer; // Layout Group container
        public GameObject levelButtonPrefab;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            PopulateLevels();
        }

        public void PopulateLevels()
        {
            // Clear existing buttons
            foreach (Transform child in buttonsContainer)
            {
                Destroy(child.gameObject);
            }

            int totalLevels = LevelManager.Instance.levelPrefabs.Count;
            int highestUnlocked = LevelManager.Instance.HighestUnlockedLevel;

            for (int i = 0; i < totalLevels; i++)
            {
                GameObject btnObj = Instantiate(levelButtonPrefab, buttonsContainer);
                LevelButtonUI btnUI = btnObj.GetComponent<LevelButtonUI>();

                bool isUnlocked = (i <= highestUnlocked);
                btnUI.Setup(i, isUnlocked);
            }
        }

        public void ShowLevelSelect()
        {
            levelSelectCanvas.SetActive(true);
            if (mobileGameCanvas != null) mobileGameCanvas.SetActive(false);
            
            PopulateLevels();
        }

        public void CloseToMainMenu()
        {
            levelSelectCanvas.SetActive(false);
            if (MainMenuManager.Instance != null)
            {
                MainMenuManager.Instance.ShowMainMenu();
            }
            else if (mobileGameCanvas != null)
            {
                mobileGameCanvas.SetActive(true);
            }
        }

        public void HideLevelSelect()
        {
            levelSelectCanvas.SetActive(false);
            if (mobileGameCanvas != null) mobileGameCanvas.SetActive(true);
        }

        public void SelectLevel(int index)
        {
            LevelManager.Instance.LoadLevel(index);
            HideLevelSelect();
        }
    }
}
