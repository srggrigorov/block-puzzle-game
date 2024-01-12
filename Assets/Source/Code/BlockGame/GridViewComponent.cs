using UnityEngine;

namespace Dreamloft.Game.Minigames.BlockGame
{
	public class GridViewComponent : MonoBehaviour
	{
		[SerializeField]
		private GridCellView gridCellPrefab;
		[SerializeField]
		private Transform gridContainerTransform;

		public GridCellView[,] GridCellViews { get; private set; }

		public void CreateGridView()
		{
			GridCellViews = new GridCellView[9, 9];
			for (int i = 0; i < GridCellViews.GetLength(0); i++)
			{
				for (int j = 0; j < GridCellViews.GetLength(1); j++)
				{
					GridCellView cellView = Instantiate(gridCellPrefab, gridContainerTransform);
					cellView.CellIndex = new Vector2Int(j, i);
					cellView.SetBackgroundColor(Mathf.Abs(i / 3 - j / 3) % 2 == 0);
					GridCellViews[j, i] = cellView;
				}
			}
		}
	}
}