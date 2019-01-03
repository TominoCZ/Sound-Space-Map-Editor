using System.Drawing;

namespace Blox_Saber_Editor
{
	class ColorSequence
	{
		private readonly Color[] _colors;
		private int _index;

		public ColorSequence()
		{
			_colors = new[] { Color.Red, Color.Cyan };
		}

		public Color Next()
		{
			var color = _colors[_index];

			_index = (_index + 1) % _colors.Length;

			return color;
		}

		public void Reset()
		{
			_index = 0;
		}
	}
}