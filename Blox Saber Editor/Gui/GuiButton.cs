namespace Blox_Saber_Editor
{
	class GuiButton : Gui
	{
		public bool IsMouseOver { get; protected set; }
		public int ID;

		protected int Texture;

		protected GuiButton(int id, float x, float y, float sx, float sy) : base(x, y, sx, sy)
		{
			ID = id;
		}

		protected GuiButton(int id, float x, float y, float sx, float sy, int texture) : this(id, x, y, sx, sy)
		{
			Texture = texture;
		}

		public override void Render(float mouseX, float mouseY)
		{
			IsMouseOver = ClientRectangle.Contains(mouseX, mouseY);
		}
	}
}