using System.Collections.Generic;
using UnityEngine;
using PolarBond.Views;

namespace PolarBond.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public List<GameObject> levelPrefabs;
        
        [HideInInspector]
        public int currentLevelIndex = 0;
        private GameObject currentLevelInstance;

        public int HighestUnlockedLevel
        {
            get => PlayerPrefs.GetInt("HighestUnlockedLevel", 0);
            private set
            {
                PlayerPrefs.SetInt("HighestUnlockedLevel", value);
                PlayerPrefs.Save();
            }
        }


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            currentLevelIndex = PlayerPrefs.GetInt("CurrentLevel", 0);
            if (levelPrefabs != null && levelPrefabs.Count > 0)
            {
                // LoadLevel(currentLevelIndex); // Disabled so Main Menu controls flow
            }
        }

        public void LoadLevel(int index)
        {
            if (WinScreenManager.Instance != null)
            {
                WinScreenManager.Instance.HideWinScreen();
            }

            if (levelPrefabs == null || levelPrefabs.Count == 0) return;

            if (index >= levelPrefabs.Count)
            {
                Debug.Log("Game Finished! All levels completed.");
                index = 0; 
                PlayerPrefs.SetInt("CurrentLevel", 0);
            }
            
            currentLevelIndex = index;

            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
            }

            currentLevelInstance = Instantiate(levelPrefabs[index]);
            // Đưa root về 0,0,0 để đảm bảo lưới (Tilemap) khớp với tọa độ tính toán World Space của EntityView
            currentLevelInstance.transform.position = Vector3.zero;
            currentLevelInstance.transform.rotation = Quaternion.identity;
            
            TilemapLevelBuilder builder = GetComponent<TilemapLevelBuilder>();
            if (builder != null)
            {
                builder.BuildLevel(currentLevelInstance.transform);
            }
            else
            {
                Debug.LogError("[LevelManager] TilemapLevelBuilder not found on the same GameObject!");
            }
        }

        public void NextLevel()
        {
            currentLevelIndex++;
            PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex);
            PlayerPrefs.Save();
            LoadLevel(currentLevelIndex);
        }

        public void RestartLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        // To be called by GameLoopController when the level is won
        public void CompleteLevel()
        {
            // Unlock next level if this was the highest unlocked
            if (currentLevelIndex >= HighestUnlockedLevel && currentLevelIndex + 1 < levelPrefabs.Count)
            {
                HighestUnlockedLevel = currentLevelIndex + 1;
            }

            NextLevel();
        }

        public void ClearLevel()
        {
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
                currentLevelInstance = null;
            }
        }
    }
}
