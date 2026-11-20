using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Baricovats.Prototype2
{
    public sealed class Prototype2EventWindowBinder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TMP_Text title;
        public TMP_Text body;
        public RectTransform card;
        public float swipeDistance = 180f;
        private Vector2 startPosition;

        public void Show(Prototype2EventData data)
        {
            if (title != null) title.text = data.title;
            if (body != null) body.text = data.body;
            if (card != null) card.anchoredPosition = Vector2.zero;
            gameObject.SetActive(true);
        }

        public void Hide() { gameObject.SetActive(false); }
        public void OnBeginDrag(PointerEventData eventData) { startPosition = eventData.position; }
        public void OnDrag(PointerEventData eventData) { if (card != null) card.anchoredPosition = eventData.position - startPosition; }
        public void OnEndDrag(PointerEventData eventData)
        {
            var delta = eventData.position - startPosition;
            if (delta.magnitude < swipeDistance) { if (card != null) card.anchoredPosition = Vector2.zero; return; }
            var direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x < 0 ? EventSwipeDirection.Left : EventSwipeDirection.Right)
                : EventSwipeDirection.Down;
            if (Prototype2EventManager.Instance != null) Prototype2EventManager.Instance.Resolve(direction);
        }
    }
}
