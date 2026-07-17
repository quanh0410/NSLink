using UnityEngine;
using UnityEngine.UI;

namespace PolarBond.Views
{
    public class OptionsManager : MonoBehaviour
    {
        public Slider bgmSlider;
        public Slider sfxSlider;

        private void Start()
        {
            // Load saved volumes or default to 1
            float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (bgmSlider != null)
            {
                bgmSlider.value = bgmVol;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = sfxVol;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }

        public void OnBGMVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("BGMVolume", value);
            PlayerPrefs.Save();
            
            if (Managers.AudioManager.Instance != null)
            {
                Managers.AudioManager.Instance.SetBGMVolume(value);
            }
        }

        public void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
            // SFX volume will be applied automatically the next time a sound plays in AudioManager
        }

        public void OnClickClose()
        {
            UITweener.ScaleTo(transform, Vector3.zero, 0.2f, () => gameObject.SetActive(false));
        }
    }
}
