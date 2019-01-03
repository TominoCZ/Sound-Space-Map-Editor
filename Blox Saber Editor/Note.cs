using System.Drawing;

namespace Blox_Saber_Editor
{
	class Note
	{
		public int X;
		public int Y;
		public int Ms;

		public Color Color;

		public Note(int x, int y, int ms)
		{
			X = x;
			Y = y;

			Ms = ms;
		}
	}
}