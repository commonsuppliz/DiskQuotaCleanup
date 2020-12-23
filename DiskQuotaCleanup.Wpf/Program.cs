using System;
using Eto.Forms;

namespace DiskQuotaCleanup.Wpf
{
	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			EnableStartupProfile();
			new Application(Eto.Platforms.Wpf).Run(new MainForm());
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
