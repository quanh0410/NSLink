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
                CanvasGroup cg = winScreenCanvas.GetComponent<CanvasGroup>();
                if (cg == null) cg = winScreenCanvas.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                UITweener.FadeTo(cg, 1f, 0.5f);
                
                if (winScreenCanvas.transform.childCount > 0)
                {
                    Transform panel = winScreenCanvas.transform.GetChild(0);
                    panel.localScale = Vector3.zero;
                    UITweener.ScaleTo(panel, Vector3.one, 0.5f);
                }
            }
        }

        public void HideWinScreen()
        {
            if (winScreenCanvas != null)
            {
                CanvasGroup cg = winScreenCanvas.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    UITweener.FadeTo(cg, 0f, 0.3f, () => winScreenCanvas.SetActive(false));
                }
                else
                {
                    winScreenCanvas.SetActive(false);
                }
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
