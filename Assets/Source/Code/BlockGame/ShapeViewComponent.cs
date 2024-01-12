using DG.Tweening;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dreamloft.Game.Minigames.BlockGame
{
	public class ShapeViewComponent : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
	{
		public Shape Shape { get; protected set; }
		public RectTransform StartCell { get; protected set; }

		public event Action<ShapeViewComponent> OnPointerDownEvent;
		public event Action<ShapeViewComponent> OnDragEvent;
		public event Action<ShapeViewComponent> OnPointerUpEvent;
		public IObservable<ShapeViewComponent> OnDragObservable;
		[SerializeField]
		private RectTransform shapeCellPrefab;
		[SerializeField]
		private CanvasGroup canvasGroup;
		private RectTransform rectTransform;

		public void CreateShape(Shape shape)
		{
			OnDragObservable = Observable.FromEvent<ShapeViewComponent>(
					handler => OnDragEvent += handler,
					handler => OnDragEvent -= handler)
				.ThrottleFirst(TimeSpan.FromSeconds(0.03f)).TakeUntilDestroy(this);

			rectTransform = (RectTransform)transform;
			Shape = shape;
			int cellsCount = Shape.CellsLocalCoordinates.Length;
			Vector3 cellsLocalPositionAverage = Vector3.zero;
			RectTransform[] cellsTransforms = new RectTransform[cellsCount];
			for (int i = 0; i < cellsCount; i++)
			{
				RectTransform cell = Instantiate(
					shapeCellPrefab,
					new Vector3(
						Shape.CellsLocalCoordinates[i].x * shapeCellPrefab.rect.width,
						Shape.CellsLocalCoordinates[i].y * shapeCellPrefab.rect.height),
					Quaternion.identity,
					gameObject.transform);
				cellsLocalPositionAverage += cell.localPosition / cellsCount;
				cellsTransforms[i] = cell;

				if (i == 0)
				{
					StartCell = cell;
				}
			}

			foreach (RectTransform cell in cellsTransforms)
			{
				cell.localPosition -= cellsLocalPositionAverage;
			}
		}

		public void SetInteractable(bool value)
		{
			canvasGroup.interactable = value;
			canvasGroup.alpha = value ? 1 : 0.5f;
		}

		public void OnDrag(PointerEventData eventData)
		{
			rectTransform.position = eventData.position + Vector2.up * 200;
			OnDragEvent?.Invoke(this);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			rectTransform.position = eventData.position + Vector2.up * 200;
			rectTransform.DOScale(Vector3.one, 0.2f);
			OnPointerDownEvent?.Invoke(this);
			canvasGroup.blocksRaycasts = false;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			rectTransform.DOScale(Vector3.one * 0.5f, 0.2f);
			rectTransform.DOLocalMove(Vector3.zero, 0.2f);
			OnPointerUpEvent?.Invoke(this);
			canvasGroup.blocksRaycasts = true;
		}

		private void OnDestroy()
		{
			OnDragEvent = OnPointerUpEvent = OnPointerDownEvent = null;
			rectTransform.DOKill();
		}
	}
}