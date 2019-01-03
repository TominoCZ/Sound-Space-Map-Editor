using System.Collections.Generic;

namespace Blox_Saber_Editor
{
	class GuiRenderer
	{
		public List<Gui> Guis = new List<Gui>();

		public void Render(float mouseX, float mouseY)
		{
			foreach (var gui in Guis)
			{
				gui.Render(mouseX, mouseY);
			}
		}
	}
}