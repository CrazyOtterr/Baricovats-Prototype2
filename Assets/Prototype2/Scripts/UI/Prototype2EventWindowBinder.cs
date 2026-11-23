using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Baricovats.Prototype2
{
    public sealed class Prototype2EventWindowBinder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TMP_Text title;
        public TMP_Text body;
        public RectTransform card;
        public float swipeDistance = 180f;
        public Button continueButton;
        public Button restartButton;
        private Vector2 startPosition;

        private void OnEnable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ResolveFinalEvent);
                continueButton.onClick.AddListener(ResolveFinalEvent);
            }
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartCampaign);
                restartButton.onClick.AddListener(RestartCampaign);
            }
        }

        private void OnDisable()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(ResolveFinalEvent);
            if (restartButton != null)
                restartButton.onClick.RemoveListener(RestartCampaign);
        }

        private void ResolveFinalEvent()
        {
            if (Prototype2EventManager.Instance != null)
                Prototype2EventManager.Instance.ResolveFinalEvent();
        }

        private void RestartCampaign()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.RestartCampaign();
        }

        public void Show(Prototype2EventData data)
        {
            if (title != null) title.text = data.title;
            if (body != null)
                body.text = string.IsNullOrEmpty(data.source)
                    ? data.body
                    : $"Источник: {data.source}\n\n{data.body}";
            if (card != null) card.anchoredPosition = Vector2.zero;

            bool isEndingCard = data.id.StartsWith("ending_", System.StringComparison.Ordinal);
            bool isFinalEvent = data.isMandatory && !data.hasChoices;
            if (continueButton != null)
                continueButton.gameObject.SetActive(isFinalEvent && !isEndingCard);
            if (restartButton != null)
                restartButton.gameObject.SetActive(isEndingCard);

            gameObject.SetActive(true);
        }

        public void Hide() { gameObject.SetActive(false); }
        public void OnBeginDrag(PointerEventData eventData) { startPosition = eventData.position; }
        public void OnDrag(PointerEventData eventData) { if (card != null) card.anchoredPosition = eventData.position - startPosition; }
        public void OnEndDrag(PointerEventData eventData)
        {
            var activeEvent = Prototype2EventManager.Instance != null
                ? Prototype2EventManager.Instance.activeEvent
                : null;
            if (activeEvent != null && !activeEvent.hasChoices)
            {
                if (card != null) card.anchoredPosition = Vector2.zero;
                return;
            }
            var delta = eventData.position - startPosition;
            if (delta.magnitude < swipeDistance) { if (card != null) card.anchoredPosition = Vector2.zero; return; }
            var direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x < 0 ? EventSwipeDirection.Left : EventSwipeDirection.Right)
                : EventSwipeDirection.Down;
            if (Prototype2EventManager.Instance != null) Prototype2EventManager.Instance.Resolve(direction);
        }
    }
}
