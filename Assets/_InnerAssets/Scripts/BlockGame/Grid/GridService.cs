using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridService : IInitializable
{
    public Grid Grid { get; private set; }
    public GridCellView[,] GridCellViews { get; private set; }
    public List<Vector2Int> FilledCellsIndexes => _filledCellsIndexes;

    private readonly GridCellView.Factory _factory;
    private readonly Transform _gridContainer;
    private readonly List<Vector2Int> _filledCellsIndexes = new List<Vector2Int>();

    public GridService(GridCellView.Factory factory, Transform gridContainer)
    {
        (_factory, _gridContainer) = (factory, gridContainer);
    }

    private void CreateGrid()
    {
        Grid = new Grid();

        GridCellViews = new GridCellView[9, 9];
        for (int i = 0; i < GridCellViews.GetLength(0); i++)
        {
            for (int j = 0; j < GridCellViews.GetLength(1); j++)
            {
                GridCellView cellView = _factory.Create();
                cellView.transform.SetParent(_gridContainer);
                cellView.cellIndex = new Vector2Int(j, i);
                cellView.SetBackgroundColor(Mathf.Abs(i / 3 - j / 3) % 2 == 0);
                GridCellViews[j, i] = cellView;
            }
        }
    }

    public bool ShapeCanBePlaced(Vector2Int startCellIndex, Shape shape)
    {
        return Grid.ShapeCanBePlaced(startCellIndex, shape, in _filledCellsIndexes);
    }

    public void Initialize()
    {
        CreateGrid();
    }
}
