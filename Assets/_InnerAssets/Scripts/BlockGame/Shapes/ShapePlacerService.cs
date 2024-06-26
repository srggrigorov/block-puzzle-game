using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class ShapePlacerService : IDisposable, IInitializable
{
    public delegate void ShapePlacedDelegate(int cellsToEmptyCount);
    public event ShapePlacedDelegate OnShapePlaced;
    public event Action OnPlaceForShapesDepleted;

    #region Injections

    private readonly GridService _gridService;
    private readonly ObjectPooler _objectPooler;
    private readonly ScoreService _scoreService;

    #endregion

    private readonly GraphicRaycaster _raycaster;
    private readonly PointerEventData _pointerEventData;
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
    private readonly ReactiveCollection<ShapeView> _unplacedShapeViews = new ReactiveCollection<ShapeView>();
    private readonly ShapeViewGeneratorService _shapeViewGeneratorService;

    public ShapePlacerService(GraphicRaycaster raycaster, GridService gridService,
        ObjectPooler objectPooler, ShapeViewGeneratorService shapeViewGeneratorService, ScoreService scoreService)
    {
        (_raycaster, _gridService, _objectPooler, _scoreService, _shapeViewGeneratorService) =
            (raycaster, gridService, objectPooler, scoreService, shapeViewGeneratorService);

        _pointerEventData = new PointerEventData(EventSystem.current);
        OnShapePlaced += _scoreService.AddScore;


    }

    private void ShapeViewDragged(ShapeView shapeView)
    {
        _pointerEventData.position = shapeView.StartCell.RectTransform.position;
        _raycastResults.Clear();
        _raycaster.Raycast(_pointerEventData, _raycastResults);
        GridCellView gridCellView = _raycastResults?.Select(x => x.gameObject.GetComponent<GridCellView>()).FirstOrDefault();

        if (gridCellView != null)
        {
            if (_gridService.FilledCellsIndexes?.Count > 0 &&
                _gridService.FilledCellsIndexes[0] != gridCellView.cellIndex)
            {
                foreach (Vector2Int cell in _gridService.FilledCellsIndexes)
                {
                    _gridService.GridCellViews[cell.x, cell.y].SetEmptyColor();
                }
            }

            if (_gridService.ShapeCanBePlaced(gridCellView.cellIndex, shapeView.Shape))
            {
                foreach (Vector2Int cell in _gridService.FilledCellsIndexes)
                {
                    _gridService.GridCellViews[cell.x, cell.y].SetCanBeFilledColor();
                }
            }

            return;
        }

        if (_gridService.FilledCellsIndexes != null)
        {
            foreach (Vector2Int cell in _gridService.FilledCellsIndexes)
            {
                _gridService.GridCellViews[cell.x, cell.y].SetEmptyColor();
            }

            _gridService.FilledCellsIndexes.Clear();
        }
    }

    private void ShapeViewReleased(ShapeView shapeView)
    {
        if (_gridService.FilledCellsIndexes.Count <= 0)
        {
            return;
        }

        HashSet<Vector2Int> allCellsToEmpty = new HashSet<Vector2Int>();
        foreach (var cell in _gridService.FilledCellsIndexes)
        {
            _gridService.Grid.Cells[cell.x, cell.y].isFilled = true;
            _gridService.GridCellViews[cell.x, cell.y].SetFilledColor();
        }

        Vector2Int[] cellsToEmpty;
        HashSet<int> checkedColumns = new HashSet<int>(), checkedRows = new HashSet<int>();
        HashSet<(Vector2Int Min, Vector2Int Max)> checkedSquares = new HashSet<(Vector2Int Min, Vector2Int Max)>();
        foreach (var cell in _gridService.FilledCellsIndexes)
        {
            if (!checkedColumns.Contains(cell.x))
            {
                checkedColumns.Add(cell.x);
                if (_gridService.Grid.IsColumnFilled(cell, out cellsToEmpty))
                {
                    allCellsToEmpty.UnionWith(cellsToEmpty);
                }
            }

            if (!checkedRows.Contains(cell.y))
            {
                checkedRows.Add(cell.y);
                if (_gridService.Grid.IsRowFilled(cell, out cellsToEmpty))
                {
                    allCellsToEmpty.UnionWith(cellsToEmpty);
                }
            }

            (Vector2Int Min, Vector2Int Max) squareBorders =
                (new Vector2Int(cell.x / 3 * 3, cell.y / 3 * 3),
                    new Vector2Int(cell.x / 3 * 3 + 3, cell.y / 3 * 3 + 3));
            if (!checkedSquares.Contains(squareBorders))
            {
                checkedSquares.Add(squareBorders);
                if (_gridService.Grid.IsSquareFilled(cell, out cellsToEmpty))
                {
                    allCellsToEmpty.UnionWith(cellsToEmpty);
                }
            }
        }

        foreach (var cell in allCellsToEmpty)
        {
            _gridService.Grid.Cells[cell.x, cell.y].isFilled = false;
            _gridService.GridCellViews[cell.x, cell.y].SetEmptyColor();
        }

        OnShapePlaced?.Invoke(allCellsToEmpty.Count > 0 ? allCellsToEmpty.Count : _gridService.FilledCellsIndexes.Count);
        shapeView.ClearShapeView();
        _objectPooler.Despawn(shapeView);
        _unplacedShapeViews.Remove(shapeView);


        _gridService.FilledCellsIndexes.Clear();
    }

    private bool ShapeCanBePlaced(ShapeView shapeView)
    {
        bool canBePlaced = _gridService.Grid.ShapeCanBePlacedAnywhere(shapeView.Shape);
        shapeView.SetInteractable(canBePlaced);
        return canBePlaced;
    }
    public void Dispose()
    {
        OnShapePlaced -= _scoreService.AddScore;
        foreach (var shapeView in _unplacedShapeViews)
        {
            shapeView.OnPointerDownEvent -= ShapeViewDragged;
            shapeView.OnPointerUpEvent -= ShapeViewReleased;
        }
        _unplacedShapeViews?.Dispose();
    }

    public void Initialize()
    {
        foreach (var shapeView in _shapeViewGeneratorService.ShapeViews)
        {
            shapeView.OnPointerDownEvent += ShapeViewDragged;
            shapeView.onDragObservable.Subscribe(_ => { ShapeViewDragged(shapeView); }).AddTo(shapeView);
            shapeView.OnPointerUpEvent += ShapeViewReleased;
            _unplacedShapeViews.Add(shapeView);
        }

        _unplacedShapeViews.ObserveCountChanged()
            .Subscribe(count =>
                {
                    switch (count)
                    {
                        case > 0 when _unplacedShapeViews.Count(ShapeCanBePlaced) < 1:
                            OnPlaceForShapesDepleted?.Invoke();
                            return;
                        case > 0:
                            return;
                        default:
                        {
                            _shapeViewGeneratorService.CreateRandomShapes();
                            foreach (var shapeView in _shapeViewGeneratorService.ShapeViews)
                            {
                                _unplacedShapeViews.Add(shapeView);
                            }
                            break;
                        }
                    }
                });
    }
}
