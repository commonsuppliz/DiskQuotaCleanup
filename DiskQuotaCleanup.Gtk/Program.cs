using System;
using Eto.Forms;
using Eto.GtkSharp;

namespace DiskQuotaCleanup.Gtk
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			EnableStartupProfile();
			//Eto.Style.Add<Eto.WinForms.Forms.Controls.GridViewHandler>(null, c => { c.Control.Columns.CollectionChanged += Columns_CollectionChanged; });
			//Eto.Style.Add<Eto.GtkSharp.Forms.FormHandler>(null, c => { c.Control.a; });
			new Application(Eto.Platforms.Gtk).Run(new MainForm());
		}
		private static string _localUserDataPath = null;
		public static void EnableStartupProfile()
		{

			_localUserDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DiskQuotaCleanup");
			System.Diagnostics.Debug.WriteLine("DataPath: " + _localUserDataPath);
			try
			{
				if (System.IO.Directory.Exists(_localUserDataPath) == false)
				{
					System.IO.Directory.CreateDirectory(_localUserDataPath);
				}
				System.Runtime.ProfileOptimization.SetProfileRoot(_localUserDataPath);
				System.Runtime.ProfileOptimization.StartProfile("startup.profile");
			}
			catch { }

		}
	}
}
