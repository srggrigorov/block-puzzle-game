using DG.Tweening;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;


public class ShapeView : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Shape Shape { get; protected set; }
    public CellView StartCell { get; protected set; }

    public event Action<ShapeView> OnPointerDownEvent;
    public event Action<ShapeView> OnDragEvent;
    public event Action<ShapeView> OnPointerUpEvent;

    public IObservable<ShapeView> onDragObservable;

    [SerializeField]
    private CellView _cellViewPrefab;
    [SerializeField]
    private CanvasGroup _canvasGroup;
    [SerializeField]
    private RectTransform _rectTransform;

    private ObjectPooler _objectPooler;
    private readonly HashSet<CellView> _cellsViews = new HashSet<CellView>();

    [Inject]
    private void Construct(ObjectPooler objectPooler)
    {
        _objectPooler = objectPooler;
        onDragObservable = Observable.FromEvent<ShapeView>(
                handler => OnDragEvent += handler,
                handler => OnDragEvent -= handler)
            .ThrottleFirst(TimeSpan.FromSeconds(0.03f)).TakeUntilDestroy(this);
    }

    public void CreateShape(Shape shape)
    {
        Shape = shape;
        int cellsCount = Shape.cellsLocalCoordinates.Length;
        Vector3 cellsLocalPositionAverage = Vector3.zero;

        for (int i = 0; i < cellsCount; i++)
        {
            CellView cell = (CellView)_objectPooler.Spawn(
                _cellViewPrefab, new Vector3(
                    Shape.cellsLocalCoordinates[i].x * _cellViewPrefab.RectTransform.rect.width,
                    Shape.cellsLocalCoordinates[i].y * _cellViewPrefab.RectTransform.rect.height),
                Quaternion.identity, gameObject.transform);

            cellsLocalPositionAverage += cell.RectTransform.localPosition / cellsCount;
            _cellsViews.Add(cell);

            if (i == 0)
            {
                StartCell = cell;
            }
        }

        foreach (CellView cell in _cellsViews)
        {
            cell.RectTransform.localPosition -= cellsLocalPositionAverage;
        }
    }

    public void SetInteractable(bool value)
    {
        _canvasGroup.interactable = value;
        _canvasGroup.alpha = value ? 1 : 0.5f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.position = eventData.position + Vector2.up * 200;
        OnDragEvent?.Invoke(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _rectTransform.position = eventData.position + Vector2.up * 200;
        _rectTransform.DOScale(Vector3.one, 0.2f);
        OnPointerDownEvent?.Invoke(this);
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _rectTransform.DOScale(Vector3.one * 0.5f, 0.2f);
        _rectTransform.DOLocalMove(Vector3.zero, 0.2f);
        OnPointerUpEvent?.Invoke(this);
        _canvasGroup.blocksRaycasts = true;
    }

    private void OnDisable()
    {
        _rectTransform.DOKill();
    }

    public void ClearShapeView()
    {
        foreach (var cell in _cellsViews)
        {
            _objectPooler.Despawn(cell);
        }
        _cellsViews.Clear();
    }
}
