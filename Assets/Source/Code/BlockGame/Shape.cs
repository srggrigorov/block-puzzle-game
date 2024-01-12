using UnityEngine;

namespace Dreamloft.Game
{
	public struct Shape
	{
		public readonly Vector2Int[] CellsLocalCoordinates;

		public Shape(Vector2Int[] cellsLocalCoordinates)
		{
			CellsLocalCoordinates = cellsLocalCoordinates;
		}
	}
}