using System;

namespace Dreamloft.Game.Minigames.BlockGame
{
	[Flags]
	public enum ShapeKind
	{
		Line = 1,
		Square = 2,
		TShape = 4,
		Arc = 8,
		Corner = 16,
		ZShape = 32,
		Cross = 64,
	}
}