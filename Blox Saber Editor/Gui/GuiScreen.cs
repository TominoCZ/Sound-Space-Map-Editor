using System.Collections.Generic;

namespace Blox_Saber_Editor
{
	class GuiScreen : Gui
	{
		protected List<GuiButton> Buttons = new List<GuiButton>();

		public bool Pauses { get; }

		protected GuiScreen(float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
			Pauses = true;
		}

		public override void Render(float mouseX, float mouseY)
		{
			foreach (var button in Buttons)
			{
				button.Render(mouseX, mouseY);
			}
		}

		public virtual void OnMouseMove(float x, float y)
		{

		}

		public virtual void OnMouseClick(float x, float y)
		{
			foreach (var button in Buttons)
			{
				if (button.IsMouseOver)
				{
					OnButtonClicked(button.ID);
					break;
				}
			}
		}

		protected virtual void OnButtonClicked(int id)
		{

		}

		public virtual void OnClosing()
		{

		}
	}
}