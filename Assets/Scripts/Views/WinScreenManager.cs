using UnityEngine;
using PolarBond.Managers;

namespace PolarBond.Views
{
    public class WinScreenManager : MonoBehaviour
    {
        public static WinScreenManager Instance;

        public GameObject winScreenCanvas;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            HideWinScreen();
        }

        public void ShowWinScreen()
        {
            if (winScreenCanvas != null)
            {
                winScreenCanvas.SetActive(true);
            }
        }

        public void HideWinScreen()
        {
            if (winScreenCanvas != null)
            {
                winScreenCanvas.SetActive(false);
            }
        }

        public void OnClickNextLevel()
        {
            HideWinScreen();
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteLevel();
            }
        }
    }
}
