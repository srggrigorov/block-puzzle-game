using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class ShapeGeneratorService
{
    private readonly ShapeKind[] _shapeKinds =
    {
        ShapeKind.Line,
        ShapeKind.Line,
        ShapeKind.Line | ShapeKind.Corner,
        ShapeKind.Line | ShapeKind.Corner | ShapeKind.Shape | ShapeKind.ZShape | ShapeKind.Square,
        ShapeKind.Line | ShapeKind.Corner | ShapeKind.Shape | ShapeKind.Arc | ShapeKind.Cross,
    };

    private readonly Random _random = new();

    private void RandomizeShape(ref Vector2Int[] localCellsCoordinates)
    {
        //Random inversion around Y-axis
        if (_random.Next(0, 2) == 0)
        {
            localCellsCoordinates = localCellsCoordinates.Select(coordinates =>
                new Vector2Int(coordinates.x * -1, coordinates.y)).ToArray();
        }

        //Random inversion around X-axis
        if (_random.Next(0, 2) == 0)
        {
            localCellsCoordinates = localCellsCoordinates.Select(coordinates =>
                new Vector2Int(coordinates.x, coordinates.y * -1)).ToArray();
        }

        //Random 90-degrees turn
        if (_random.Next(0, 2) == 0)
        {
            for (int i = 0; i < localCellsCoordinates.Length; i++)
            {
                int coordinateXValue = localCellsCoordinates[i].x;
                localCellsCoordinates[i] = new Vector2Int(localCellsCoordinates[i].y, coordinateXValue);
            }
        }
    }

    public Shape CreateRandomShape(int cellsCount)
    {
        if (cellsCount is < 0 or > 5)
        {
            throw new ArgumentException("Shape cells count must be more than 0 and less than 5!");
        }

        // Get random shape kind for given cells count
        ShapeKind[] matchingShapeKinds = Enum.GetValues(typeof(ShapeKind)).Cast<ShapeKind>().Where(c =>
            (_shapeKinds[cellsCount - 1] &
                c) == c).ToArray();

        return matchingShapeKinds[UnityEngine.Random.Range(0, matchingShapeKinds.Length)] switch
        {
            ShapeKind.Line => CreateLine(cellsCount, _random.Next(0, 2) == 0),
            ShapeKind.Arc => CreateArc(),
            ShapeKind.Corner => CreateCorner(cellsCount),
            ShapeKind.Square => CreateSquare(),
            ShapeKind.Shape => CreateTShape(cellsCount),
            ShapeKind.ZShape => CreateZShape(),
            ShapeKind.Cross => CreateCross(),
            _ => throw new ArgumentException("No matching shape kind found!"),
        };
    }

    private Shape CreateLine(int cellsCount, bool diagonal = false)
    {
        Vector2Int[] localCellsCoordinates = new Vector2Int[cellsCount];
        for (int i = 0; i < cellsCount; i++)
        {
            localCellsCoordinates[i] = new Vector2Int(i, diagonal ? i : 0);
        }

        RandomizeShape(ref localCellsCoordinates);
        return new Shape(localCellsCoordinates);
    }

    private Shape CreateSquare()
    {
        Vector2Int[] localCellsCoordinates = { Vector2Int.zero, Vector2Int.right, Vector2Int.up, Vector2Int.one };
        return new Shape(localCellsCoordinates);
    }

    private Shape CreateCorner(int cellsCount)
    {
        Vector2Int[] localCellsCoordinates = new Vector2Int[cellsCount];
        for (int i = 0; i < cellsCount; i++)
        {
            localCellsCoordinates[i] = i % 2 == 0 ? new Vector2Int(i / 2, 0) : new Vector2Int(0, i / 2 + 1);
        }

        RandomizeShape(ref localCellsCoordinates);
        return new Shape(localCellsCoordinates);
    }

    private Shape CreateTShape(int cellsCount)
    {
        Vector2Int[] localCellsCoordinates = new Vector2Int[cellsCount];
        localCellsCoordinates[0] = Vector2Int.zero;
        for (int i = 1; i < cellsCount; i++)
        {
            if (i == 1)
            {
                localCellsCoordinates[i] = Vector2Int.right;
                continue;
            }

            if (i == 2)
            {
                localCellsCoordinates[i] = Vector2Int.left;
                continue;
            }

            if (i > 2)
            {
                localCellsCoordinates[i] = new Vector2Int(0, i / 4 + 1);
            }
        }

        RandomizeShape(ref localCellsCoordinates);
        return new Shape(localCellsCoordinates);
    }

    private Shape CreateCross()
    {
        Vector2Int[] localCellsCoordinates = { Vector2Int.zero, Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };
        return new Shape(localCellsCoordinates);
    }

    private Shape CreateArc()
    {
        Vector2Int[] localCellsCoordinates = { Vector2Int.zero, Vector2Int.right, Vector2Int.left, Vector2Int.one, new(-1, 1) };
        RandomizeShape(ref localCellsCoordinates);
        return new Shape(localCellsCoordinates);
    }

    private Shape CreateZShape()
    {
        Vector2Int[] localCellsCoordinates = { Vector2Int.zero, Vector2Int.right, Vector2Int.one, new(2, 1) };
        RandomizeShape(ref localCellsCoordinates);
        return new Shape(localCellsCoordinates);
    }
}
