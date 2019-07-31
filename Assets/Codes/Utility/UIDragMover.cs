using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YGOTzolkin.Utility
{
	class UIDragMover : MonoBehaviour
	{
		public Image Body;
		public RectTransform CanvasRectangle;
		private bool dragging;
		private Vector2 targetPosition;
		private Vector2 offset;
		public void Update()
		{
			if (dragging)
			{
				Body.rectTransform.anchoredPosition += (targetPosition - Body.rectTransform.anchoredPosition) * 0.5f;
			}
		}
		public void OnBeginDrag(BaseEventData baseEventData)
		{
			PointerEventData pointerEventData = baseEventData as PointerEventData;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				CanvasRectangle, pointerEventData.position, pointerEventData.pressEventCamera, out offset);
			offset = Body.rectTransform.anchoredPosition-offset;
			dragging = true;
		}
		public void OnDrag(BaseEventData baseEventData)
		{
			PointerEventData pointerEventData = baseEventData as PointerEventData;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				CanvasRectangle, pointerEventData.position, pointerEventData.pressEventCamera, out targetPosition);
			targetPosition = targetPosition + offset;
		}
		public void OnEndDrag(BaseEventData baseEventData)
		{
			dragging = false;
		}
	}
}
