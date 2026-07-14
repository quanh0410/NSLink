using UnityEngine;
using UnityEngine.UI;

namespace PolarBond.Views
{
    public class OptionsManager : MonoBehaviour
    {
        public Slider volumeSlider;

        private void Start()
        {
            // Load saved volume or default to 1
            float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (volumeSlider != null)
            {
                volumeSlider.value = savedVol;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
            
            AudioListener.volume = savedVol;
        }

        public void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
        }

        public void OnClickClose()
        {
            gameObject.SetActive(false);
        }
    }
}
