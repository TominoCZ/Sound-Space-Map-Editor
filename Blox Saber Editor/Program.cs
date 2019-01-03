using System;
using System.Windows.Forms;
using NAudio.Utils;

namespace Blox_Saber_Editor
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (var w = new EditorWindow())
			{
				w.Run();
			}
		}
	}
}
