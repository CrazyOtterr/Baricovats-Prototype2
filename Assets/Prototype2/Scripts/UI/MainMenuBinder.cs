using UnityEngine;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class MainMenuBinder : MonoBehaviour
    {
        [Header("Existing scene objects")]
        public GameObject menuPanel;
        public GameObject settingsPanel;
        public Button startButton;
        public Button settingsButton;
        public Button quitButton;
        public Button closeSettingsButton;

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartGame);
                startButton.onClick.AddListener(StartGame);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(OpenSettings);
                settingsButton.onClick.AddListener(OpenSettings);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
                quitButton.onClick.AddListener(QuitGame);
            }

            if (closeSettingsButton != null)
            {
                closeSettingsButton.onClick.RemoveListener(CloseSettings);
                closeSettingsButton.onClick.AddListener(CloseSettings);
            }

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void OnDisable()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(StartGame);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OpenSettings);
            if (quitButton != null)
                quitButton.onClick.RemoveListener(QuitGame);
            if (closeSettingsButton != null)
                closeSettingsButton.onClick.RemoveListener(CloseSettings);
        }

        private void StartGame()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.RestartCampaign();

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            if (menuPanel != null)
                menuPanel.SetActive(false);
        }

        private void OpenSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
