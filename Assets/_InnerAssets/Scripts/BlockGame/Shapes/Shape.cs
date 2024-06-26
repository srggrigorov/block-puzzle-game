using UnityEngine;

public struct Shape
{
    public readonly Vector2Int[] cellsLocalCoordinates;

    public Shape(Vector2Int[] cellsLocalCoordinates)
    {
        this.cellsLocalCoordinates = cellsLocalCoordinates;
    }
}
