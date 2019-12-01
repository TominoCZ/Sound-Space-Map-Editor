using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Sound_Space_Editor
{
	class Program
	{
		//TODO - add arrow controls to navigate through ticks, add an option to disable snapping to ticks

		[STAThread]
		static void Main(string[] args)
		{
			Application.SetCompatibleTextRenderingDefault(false);

			EditorWindow w;

			try
			{
				long offset = 0;

				if (args.Length >= 2 && args[0] == "-o")
				{
					long.TryParse(args[1], out offset);
				}

				w = new EditorWindow(offset);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			using (w)
			{
				w.Run();
			}
		}
	}
}