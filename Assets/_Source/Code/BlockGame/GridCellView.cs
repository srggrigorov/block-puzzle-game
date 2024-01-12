using UnityEngine;
using UnityEngine.UI;

namespace Dreamloft.Game
{
	public class GridCellView : MonoBehaviour
	{
		[SerializeField]
		private Image background;
		[SerializeField]
		private Color emptyColor1;
		[SerializeField]
		private Color emptyColor2;
		[SerializeField]
		private Color canBeFilledColor;
		[SerializeField]
		private Color filledColor;
		private Color emptyColor;
		public Vector2Int CellIndex;

		public void SetBackgroundColor(bool first) => emptyColor = background.color = first ? emptyColor1 : emptyColor2;
		public void SetEmptyColor() => background.color = emptyColor;
		public void SetCanBeFilledColor() => background.color = canBeFilledColor;
		public void SetFilledColor() => background.color = filledColor;
	}
}