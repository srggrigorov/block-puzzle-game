using UnityEngine;

namespace Dreamloft.Game.Minigames.BlockGame
{
	public class ShapesViewGeneratorComponent : MonoBehaviour
	{
		[SerializeField]
		private ShapeViewComponent shapeViewPrefab;

		public ShapeViewComponent CreateShapeView(Shape shape)
		{
			ShapeViewComponent shapeView = Instantiate(shapeViewPrefab);
			shapeView.CreateShape(shape);
			return shapeView;
		}
	}
}