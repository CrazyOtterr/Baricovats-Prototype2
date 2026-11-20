using UnityEngine;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class FactionWindowBinder : MonoBehaviour
    {
        public GameObject window;
        public Button openButton;
        public Button closeButton;

        private void OnEnable()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(Open);
                openButton.onClick.AddListener(Open);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
        }

        private void OnDisable()
        {
            if (openButton != null) openButton.onClick.RemoveListener(Open);
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }

        public void Open()
        {
            var stateManager = GameStateManager.Instance;
            if (stateManager == null || stateManager.State == null)
                return;
            if (stateManager.State.campaignEnded || stateManager.State.mapActionsLocked)
                return;
            if (window != null)
                window.SetActive(true);
        }

        public void Close()
        {
            if (window != null)
                window.SetActive(false);
        }
    }
}
