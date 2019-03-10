using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Blox_Saber_Editor
{
	class Program
	{
		//TODO - add arrow controls to navigate though ticks, add an option to disable snapping to ticks

		[STAThread]
		static void Main()
		{
			Application.SetCompatibleTextRenderingDefault(false);

			EditorWindow w;

			try
			{
				w = new EditorWindow();
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