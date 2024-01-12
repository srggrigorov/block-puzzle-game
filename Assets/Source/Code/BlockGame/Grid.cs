//Logic of the grid and cells will be separated from graphics

using System.Collections.Generic;
using UnityEngine;

namespace Dreamloft.Game.Minigames.BlockGame
{
	public class Grid
	{
		public GridCell[,] Cells { get; }

		public Grid()
		{
			Cells = new GridCell[9, 9];
			for (int i = 0; i < Cells.GetLength(0); i++)
			{
				for (int j = 0; j < Cells.GetLength(1); j++)
				{
					Cells[j, i] = default;
				}
			}
		}

		public bool IsRowFilled(Vector2Int startCellIndex, out Vector2Int[] filledCellsIndexes)
		{
			filledCellsIndexes = new Vector2Int[9];
			for (int i = 0; i < filledCellsIndexes.Length; i++)
			{
				if (!Cells[i, startCellIndex.y].isFilled)
				{
					filledCellsIndexes = null;
					return false;
				}

				filledCellsIndexes[i] = new Vector2Int(i, startCellIndex.y);
			}

			return true;
		}

		public bool IsColumnFilled(Vector2Int startCellIndex, out Vector2Int[] filledCellsIndexes)
		{
			filledCellsIndexes = new Vector2Int[9];
			for (int j = 0; j < filledCellsIndexes.Length; j++)
			{
				if (!Cells[startCellIndex.x, j].isFilled)
				{
					filledCellsIndexes = null;
					return false;
				}

				filledCellsIndexes[j] = new Vector2Int(startCellIndex.x, j);
			}

			return true;
		}

		public bool IsSquareFilled(Vector2Int startCellIndex, out Vector2Int[] filledCellsIndexes)
		{
			filledCellsIndexes = new Vector2Int[9];
			HashSet<Vector2Int> filledCellsIndexesHashSet = new();
			int squareStartX = startCellIndex.x / 3 * 3, squareStartY = startCellIndex.y / 3 * 3;
			for (int i = squareStartX; i < squareStartX + 3; i++)
			{
				for (int j = squareStartY; j < squareStartY + 3; j++)
				{
					if (!Cells[i, j].isFilled)
					{
						filledCellsIndexes = null;
						return false;
					}

					filledCellsIndexesHashSet.Add(new Vector2Int(i, j));
				}
			}

			filledCellsIndexesHashSet.CopyTo(filledCellsIndexes);
			return true;
		}

		private bool ShapeCanBePlaced(Vector2Int startCellIndex, Shape shape)
		{
			return ShapeCanBePlaced(startCellIndex, shape, out _);
		}

		public bool ShapeCanBePlacedAnywhere(Shape shape)
		{
			for (int i = 0; i < Cells.GetLength(0); i++)
			{
				for (int j = 0; j < Cells.GetLength(1); j++)
				{
					if (ShapeCanBePlaced(new Vector2Int(i, j), shape))
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool ShapeCanBePlaced(
			Vector2Int startCellIndex,
			Shape shape,
			out Vector2Int[] filledCellsIndexes)
		{
			filledCellsIndexes = new Vector2Int[shape.CellsLocalCoordinates.Length];
			for (int i = 0; i < shape.CellsLocalCoordinates.Length; i++)
			{
				Vector2Int filledCellIndex = startCellIndex + shape.CellsLocalCoordinates[i];
				if (filledCellIndex.x is < 0 or > 8
				    || filledCellIndex.y is < 0 or > 8
				    || Cells[filledCellIndex.x, filledCellIndex.y].isFilled)
				{
					filledCellsIndexes = null;
					return false;
				}

				filledCellsIndexes[i] = filledCellIndex;
			}

			return true;
		}
	}
}