using UnityEngine;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class SoundSettingsBinder : MonoBehaviour
    {
        public const string MasterVolumeKey = "Prototype2.MasterVolume";

        [Header("Existing scene objects")]
        public Slider volumeSlider;

        private void OnEnable()
        {
            float volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            volume = Mathf.Clamp01(volume);

            if (volumeSlider != null)
            {
                volumeSlider.SetValueWithoutNotify(volume);
                volumeSlider.onValueChanged.RemoveListener(SetVolume);
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            SetVolume(volume);
        }

        private void OnDisable()
        {
            if (volumeSlider != null)
                volumeSlider.onValueChanged.RemoveListener(SetVolume);
        }

        private void OnApplicationQuit()
        {
            SaveVolume();
        }

        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        }

        public void SaveVolume()
        {
            PlayerPrefs.Save();
        }
    }
}
