using UnityEngine;
using UnityEngine.UI;

namespace PolarBond.Views
{
    public class LevelButtonUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image backgroundImage;
        public Text levelText;
        public GameObject lockIcon;

        [Header("Sprites")]
        public Sprite bgUnlockedSprite;
        public Sprite bgLockedSprite;

        private int assignedLevelIndex;

        public void Setup(int levelIndex, bool isUnlocked)
        {
            assignedLevelIndex = levelIndex;

            // Dọn dẹp cục UI ngôi sao cũ nếu chưa chạy lại Builder
            Transform oldStars = transform.Find("StarsContainer");
            if (oldStars != null) Destroy(oldStars.gameObject);

            if (!isUnlocked)
            {
                backgroundImage.sprite = bgLockedSprite;
                levelText.gameObject.SetActive(false);
                lockIcon.SetActive(true);
                GetComponent<Button>().interactable = false;
            }
            else
            {
                backgroundImage.sprite = bgUnlockedSprite;
                levelText.gameObject.SetActive(true);
                levelText.text = (levelIndex + 1).ToString();
                
                lockIcon.SetActive(false);
                GetComponent<Button>().interactable = true;
            }
        }

        public void OnClickButton()
        {
            LevelSelectManager.Instance.SelectLevel(assignedLevelIndex);
        }
    }
}
