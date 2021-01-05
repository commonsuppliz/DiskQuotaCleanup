using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Resources;
using System.Threading;
using System.Runtime.InteropServices;
using System.Data;
using ClosedXML;
using ClosedXML.Excel;

namespace DiskQuotaCleanup
{
	public partial class MainForm : Form
	{
		private TableLayout _tableLayout = null;
		private TreeGridView _folderView = null;
		private Splitter _splitter = null;
		private GridView _grid = null;
		private ContextMenu _folderViewContextMenu = null;
		private ContextMenu _gridContextMenu = null;
		private Panel _rightPanel = null;
		private DiskUsageControl _rightTopPanel = null;
		private Splitter _spRight = null;
		private UITimer _timerUI = null;
		private frmProgress _frmProgress = null;
		internal static  List<Color> GlobalColorList = new List<Color>();
		private Label _statusBarLabel1 = null;
		private Label _statusBarLabel2 = null;
		//
		public static List<FolderNode> _dirList = new List<FolderNode>();
		private static string _currentFolder = null;
		private static List<FolderNode> _treeFolderNodeList = new List<FolderNode>();
		private Task<long> _lookupTask = null;
		private long _rootFolderSize = 0;
		private Command menuExportExcel = null;
		/// <summary>
		/// Files count has been checked size
		/// </summary>
		public static long LookedFileCount;
        /// <summary>
        /// List of System Directories you dont want remove. Designed for StartsWith()
        /// </summary>
        private Dictionary<string, string> _Sy_Dir_List_StartsWith = new Dictionary<string, string>();

        public MainForm()
		{
			Title = "DiskQuotaCleanUp (Eto.Forms)";
			
			ClientSize = new Size(1000, 600);
			_folderView = new TreeGridView();


			_folderView.AllowMultipleSelection = true;
			_folderView.Columns.Add(new GridColumn
			{
				HeaderText = "Path",
				DataCell = new TextBoxCell(0),
				Width = 350
			});
			_folderView.Columns.Add(new GridColumn
			{
				HeaderText = "Size",
				DataCell = new TextBoxCell(1),
				Width = 100
			});
			_folderView.Columns.Add(new GridColumn
			{
				HeaderText = "LastModifed",
				DataCell = new TextBoxCell(2),
				Width = 70
			});
			_folderView.Columns.Add(new GridColumn
			{
				HeaderText = "Depth",
				DataCell = new TextBoxCell(5),
				Width=30
			});

            _folderView.MouseDown += _folderView_MouseDown;
			_folderViewContextMenu = new ContextMenu();
			ButtonMenuItem _buttonViewSubFoldersPropertiesForFoldeList = new ButtonMenuItem();
			_buttonViewSubFoldersPropertiesForFoldeList.Text = "View SubFolders";
            _buttonViewSubFoldersPropertiesForFoldeList.Click += _buttonViewSubFoldersPropertiesForFoldeList_Click;
			_folderViewContextMenu.Items.Add(_buttonViewSubFoldersPropertiesForFoldeList);
			_folderView.ContextMenu = _folderViewContextMenu;
			_splitter = new Splitter();
			_splitter.Width = 5;
			_splitter.Orientation = Orientation.Horizontal;
			_splitter.BackgroundColor = Colors.Yellow ;
			_grid = new GridView();


			_grid.Columns.Add(new GridColumn
			{
				HeaderText = "Path\u2193",
				DataCell = new TextBoxCell(0),
				Width = 400,
				Sortable = true

			}); ;
			_grid.Columns.Add(new GridColumn
			{
				HeaderText = "Size\u2193",
				DataCell = new TextBoxCell(1),
				Width = 50,
				Sortable = true
			});


			_grid.Columns.Add(new GridColumn
			{
				HeaderText = "LastModified",
				DataCell = new TextBoxCell(2),
				Width = 70,
				Sortable = false
			});
			_grid.Columns.Add(new GridColumn
			{
				HeaderText = "CreationTime",
				DataCell = new TextBoxCell(3),
				Width = 70,
				Sortable = false
			});
			_grid.Columns.Add(new GridColumn
			{
				HeaderText = "LastAccess",
				DataCell = new TextBoxCell(4),
				Width = 70,
				Sortable = false
			});
			_grid.Columns.Add(new GridColumn
			{
				HeaderText = "FolderDepth",
				DataCell = new TextBoxCell(5),
				Width = 10,
				Sortable = false
			});
			this._gridContextMenu = new ContextMenu();
			ButtonMenuItem _buttonViewNode = new ButtonMenuItem();
			_buttonViewNode.Text = "View in Node";
            _buttonViewNode.Click += _buttonViewNode_Click;
			ButtonMenuItem _buttonOpenShell = new ButtonMenuItem();
			_buttonOpenShell.Text = "Open Folder";
            _buttonOpenShell.Click += _buttonOpenShell_Click;
			ButtonMenuItem _buttonDeleteFolder = new ButtonMenuItem();
			_buttonDeleteFolder.Text = "Delete this Folder";
            _buttonDeleteFolder.Click += _buttonDeleteFolder_Click;
			
			this._gridContextMenu.Items.Add(_buttonViewNode);
			this._gridContextMenu.Items.Add(_buttonOpenShell);
			this._gridContextMenu.Items.Add(_buttonDeleteFolder);
			this._grid.ContextMenu = this._gridContextMenu;
			_grid.Border = BorderType.Bezel;
			_grid.AllowMultipleSelection = true;
			_gridContextMenu = new ContextMenu();
			//_grid.AllowColumnReordering = true;
            _grid.ColumnHeaderClick += _grid_ColumnHeaderClick;
            _grid.SelectedItemsChanged += _grid_SelectedItemsChanged;
			_grid.GridLines = GridLines.Both;
			
			_grid.Size = new Size(500, this.Height);
			_tableLayout = new TableLayout();
			_rightPanel = new Panel();
			_rightPanel.BackgroundColor = Colors.Silver;
			this._rightTopPanel = new DiskUsageControl();
			this._rightTopPanel.BackgroundColor = Colors.White;
			_rightTopPanel.Size = new Size(400, this.ClientSize.Height /2);

			_spRight = new Splitter();
			_spRight.Orientation = Orientation.Vertical;
			_spRight.Width = 5;
			_spRight.BackgroundColor = Colors.Blue;
			_spRight.Position = 200;
			_spRight.Panel1 = _rightTopPanel;
			_spRight.Panel2 = _grid;

			_tableLayout.ClientSize = this.ClientSize;
			_tableLayout.Size = this.ClientSize;
			_tableLayout.Padding = 10;

			_splitter.Position = 650;
			_splitter.Panel1 = _folderView;
			_splitter.Panel2 = _spRight;
			_splitter.ClientSize = new Size(this.ClientSize.Width, 5);

			_tableLayout.Padding = 3;
			_tableLayout.Size = this.ClientSize;


			_tableLayout.Rows.Add(new TableRow(_splitter) { ScaleHeight = true });
			TableLayout _panelStatusBar = new TableLayout();
			_panelStatusBar.BackgroundColor = Colors.Azure;
			_panelStatusBar.Size = new Size(this.Size.Width, 20);
			this._statusBarLabel1 = new Label();
			this._statusBarLabel1.Text = "Ready";
			this._statusBarLabel2 = new Label();
			this._statusBarLabel2.Text = DateTime.Now.ToShortTimeString();
			this._statusBarLabel1.Size = new Size(250, 20);
			this._statusBarLabel2.Size = new Size(250, 20);
			TableRow _row = new TableRow() { Cells ={ this._statusBarLabel1, this._statusBarLabel2 } };
			_panelStatusBar.Rows.Add(_row);
			_tableLayout.Rows.Add(new TableRow(_panelStatusBar));
			
		

			this.Content = _tableLayout;
			

            this.SizeChanged += MainForm_SizeChanged;
		
			

			// create a few commands that can be used for the menu and toolbar
			string strOpenFolder = MainForm_Resource.ResourceManager.GetString("imgOpenFolder");
			Bitmap bmpOpenFolder = ConvertBase64ToBitmap(strOpenFolder);

			var cmdSelectFolder = new Command { MenuText = "Select Folder", ToolBarText = "Select Folder", Image=bmpOpenFolder };

			cmdSelectFolder.Executed += CmdSelectFolder_Executed;
			string strImageOptions = MainForm_Resource.ResourceManager.GetString("imgOptions");
			Bitmap bmpOptions = ConvertBase64ToBitmap(strImageOptions);
			var cmdOpenOptionDialog = new Command { MenuText = "Option", ToolBarText = "Option", Image = bmpOptions };
            cmdOpenOptionDialog.Executed += CmdOpenOptionDialog_Executed;

			string strExcelImage = MainForm_Resource.ResourceManager.GetString("imgExcel");
			Bitmap bmpExcel = ConvertBase64ToBitmap(strExcelImage);

			menuExportExcel = new Command { MenuText = "Export Grid To Excel", ToolBarText = "Export Grid To Excel", Image=bmpExcel };
            menuExportExcel.Executed += CmdExportExcel_Executed;
			menuExportExcel.Enabled = false;



			var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
			quitCommand.Executed += (sender, e) => Application.Instance.Quit();

			var aboutCommand = new Command { MenuText = "About..." };
			aboutCommand.Executed += (sender, e) => new AboutDialog().ShowDialog(this);

			// create menu
			Menu = new MenuBar
			{
				Items =
				{
					// File submenu
					new ButtonMenuItem { Text = "&File", Items ={ cmdSelectFolder } },
					new ButtonMenuItem{Text = "&Option", Items={cmdOpenOptionDialog }}

				},

				QuitItem = quitCommand,
				AboutItem = aboutCommand
			};

			// create toolbar			
#if DEBUG
			var cmdTest = new Command { MenuText = "Test", ToolBarText = "Testl" };
            cmdTest.Executed += CmdTest_Executed;
			ToolBar = new ToolBar { Items = { cmdSelectFolder, new SeparatorToolItem(), cmdOpenOptionDialog, new SeparatorToolItem(), menuExportExcel, new SeparatorToolItem(), cmdTest, new SeparatorToolItem() } };
#else
			ToolBar = new ToolBar { Items = { cmdSelectFolder, new SeparatorToolItem(), cmdOpenOptionDialog, new SeparatorToolItem(), menuExportExcel, new SeparatorToolItem() } };
#endif
			this._frmProgress = new frmProgress();
			this._frmProgress.Visible = false;
			this._frmProgress.ShowInTaskbar = false;
			this._frmProgress.Location = new Point(500, 500);
			//this._frmProgress.
			this._timerUI = new UITimer();
			this._timerUI.Interval = 1;
            this._timerUI.Elapsed += _timerUI_Elapsed;
			this._timerUI.Stop();
			foreach (var methodInfo in typeof(Colors).GetMethods())
			{
				switch(methodInfo.Name )
                {
					case "GetType":
					case "ToString":
					case "Equals":
					case "GetHashCode":
						//this.GetType().e
						continue;
					default:
						break;
                }
				try
				{
					var result = methodInfo.Invoke(null, null);
					if (result is Color)
					{
						GlobalColorList.Add((Color)result);
					}
				}catch
				{
					System.Diagnostics.Debug.WriteLine(methodInfo.Name + " has Exception");
					continue;
                }

			}
			try
			{
				var platformInfo = Eto.Platform.Detect;
				// Create System Directory List
				if (platformInfo.IsWinForms || platformInfo.IsWpf)
				{
					var _windows_system_drive = System.Environment.GetEnvironmentVariable("SystemDrive");
					if (string.IsNullOrEmpty(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) == false)
					{
						// "Program Files"
						_Sy_Dir_List_StartsWith[System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)] = "";
					}
					if (string.IsNullOrEmpty(System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)) == false)
					{
						// "Program Files(X86)"
						_Sy_Dir_List_StartsWith[System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)] = "";
					}
					if (string.IsNullOrEmpty(System.Environment.GetFolderPath(Environment.SpecialFolder.Windows)) == false)
					{
						// "Windows"
						_Sy_Dir_List_StartsWith[System.Environment.GetFolderPath(Environment.SpecialFolder.Windows)] = "";
					}
					if (string.IsNullOrEmpty(System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) == false)
					{
						// "Windows"
						_Sy_Dir_List_StartsWith[System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)] = "";
					}
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "ProgamData")] = "";
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "$Recycle.Bin")] = "";
					// _Sy_Dir_List_StartsWith["Config.Msi"] = ""; This directory is creating during install Windows App. Looks ok to remove after install
					// _Sy_Dir_List_StartsWith["ESD"] = ""; Media Creation Working Folder it seems ok to remove
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "MSOCache")] = ""; // MSOCache should be removed by disk cleanup
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "pagefile.sys")] = "";
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "hiblid.sys")] = "";
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "pagefile.sys")] = "";
					_Sy_Dir_List_StartsWith[Path.Combine(_windows_system_drive, "System Volume Information")] = "";
				}
				else
				{
					// Unix System Directories List
					_Sy_Dir_List_StartsWith["/bin"] = "";
					_Sy_Dir_List_StartsWith["/boot"] = ""; // boot loader
					_Sy_Dir_List_StartsWith["/etc"] = "";  // app boot related files
					_Sy_Dir_List_StartsWith["/lib"] = ""; // library files
					_Sy_Dir_List_StartsWith["/lib32"] = ""; // library files 32-bit
					_Sy_Dir_List_StartsWith["/lib64"] = ""; // library files 64-bit
					_Sy_Dir_List_StartsWith["/opt"] = ""; // big application is installed here.
					_Sy_Dir_List_StartsWith["/proc"] = ""; // system info
					_Sy_Dir_List_StartsWith["/sbin"] = ""; // root users command folder
					_Sy_Dir_List_StartsWith["/sys"] = ""; // drivers etc.
					_Sy_Dir_List_StartsWith["/tmp"] = ""; // temporary app dir
					_Sy_Dir_List_StartsWith["/user"] = ""; // system info
				}

            }
            catch
			{
				System.Diagnostics.Debug.WriteLine("Sys dir list Error");
			}

		}
		/// <summary>
		/// Test Functions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void CmdTest_Executed(object sender, EventArgs e)
        {
			var mouseevent = MouseEventArgs.Empty;

			var node = this._folderView.SelectedItem;
			if(node != null)
            {
				node.Expanded = true;
            }
			
        }

        private void _buttonDeleteFolder_Click(object sender, EventArgs e)
        {
			try
			{
				string strDir = null;
				string strDirInfo = null;
				if (this._grid.SelectedItem != null)
				{
					if (this._grid.SelectedItem is FolderNode)
					{
						FolderNode fNode = this._grid.SelectedItem as FolderNode;
						if (fNode != null)
						{
							strDir = fNode.FullPath;
							strDirInfo = string.Format("\tPath:\t{0}\r\n\tLastModified:\t{1}\r\n\tSize:\t{2}", fNode.FullPath, fNode.LastModified, fNode.Size);

						}
					}
				}
                if (string.IsNullOrEmpty(strDir) == false)
                {
                    // Confirm Deleting Dir is not in _SysDirList
                    foreach (var s in _Sy_Dir_List_StartsWith.Keys)
                    {
                        if (strDir.ToLower().StartsWith(s.ToLower()))
                        {
                            MessageBox.Show(string.Format("You can not remove System Directory\r\n\r\nPlease use System Uninstall Option if possible.", strDir), MessageBoxType.Warning);
                            return;

                        }
                    }
                

                var result = MessageBox.Show(string.Format("Remove {0}? \r\n\r\n{1}",  strDir, strDirInfo) ,  strDir, MessageBoxButtons.OKCancel, MessageBoxType.Warning, MessageBoxDefaultButton.OK);
					if(result== DialogResult.Ok)
                    {
						System.IO.Directory.Delete(strDir, true);
						//if no exceptions, display dialog to lookup the Analyze the same folder again or not
						if(MessageBox.Show(String.Format("Analyze the folder again?\r\n\r\n\t{0}", _currentFolder),"Analyze the folder again", MessageBoxButtons.OKCancel,MessageBoxType.Question, MessageBoxDefaultButton.OK) == DialogResult.Ok)
                        {
							StartAnalyzeFolder(_currentFolder);
                        }
                    }
                }

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace.ToString());
			}
		}

        private void _buttonOpenShell_Click(object sender, EventArgs e)
		{
			try
			{
				string strDir = null;

				if (this._grid.SelectedItem != null)
				{
					if(this._grid.SelectedItem is FolderNode)
                    {
						FolderNode fNode = this._grid.SelectedItem as FolderNode;
						if(fNode != null)
                        {
							strDir = fNode.FullPath;
                        }
                    }
				}
                
				if (this.Platform.IsWinForms == true || this.Platform.IsWpf)
				{
					System.Diagnostics.Process.Start("explorer.exe", strDir);
				}else if(this.Platform.IsMac)
                {
					System.Diagnostics.Process.Start("/usr/bin/open", strDir);
				}else if(this.Platform.IsGtk)
                {
					System.Diagnostics.Process.Start("/usr/bin/open", strDir);
				}
			}catch
            {
				MessageBox.Show("Could not open shell...");
            }
        }


		public static Bitmap ConvertBase64ToBitmap(string strBase64)
		{
			strBase64 = strBase64.Substring(strBase64.IndexOf(',') + 1);
			try
			{
				if (strBase64.Length > 0)
				{

					Byte[] bitmapData = new Byte[strBase64.Length];
					bitmapData = Convert.FromBase64String(FixBase64ForImage(strBase64));

					System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);

					return new Bitmap(streamBitmap);
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("Eror: Generating Bitmap from Resource String");
				System.Console.WriteLine(ex.ToString());
				System.Console.WriteLine(ex.StackTrace.ToString());
			}
			System.Console.WriteLine("Generationg adhoc images");
			Bitmap bmpAdhoc = new Bitmap(16, 16, PixelFormat.Format32bppRgba);
			Graphics grc = new Graphics(bmpAdhoc);
			grc.Clear(Colors.White);
			grc.Dispose();
			return bmpAdhoc;

		}
		public static string FixBase64ForImage(string Image)
		{
			System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);

			sbText.Replace("\r\n", String.Empty);

			sbText.Replace(" ", String.Empty);

			return sbText.ToString();
		}



		//ブック作成



		private bool _isFormProgressVisibile = false;

        private void _timerUI_Elapsed(object sender, EventArgs e)
        {
            if(this._isFormProgressVisibile == true)
            {
				System.Diagnostics.Debug.WriteLine("Tick...");
				return;
            }
			{
				System.Diagnostics.Debug.WriteLine("Done...");
				//this._frmProgress.Enabled = false;
				this._frmProgress.StopTimer();
				this._frmProgress.Visible = false;
				
				this._timerUI.Stop();


            }
        }
        protected override void Dispose(bool disposing)
        {
			if(disposing )
            {
				if(this._frmProgress != null)
                {
					this._frmProgress.Dispose();
                }
				if(this._timerUI != null)
                {
					if(this._timerUI.Started == true)
                    {
						this._timerUI.Stop();
                    }
					this._timerUI.Dispose();
					this._timerUI = null;
                }
            }
            base.Dispose(disposing);
        }
        private void _buttonViewSubFoldersPropertiesForFoldeList_Click(object sender, EventArgs e)
        {
            if(this._folderView.SelectedItem != null)
            {
				FolderNode _fNode = this._folderView.SelectedItem as FolderNode;
				
				if(_fNode != null)
                {
					//int MachCount = 0;
					System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
					_stopwatch.Start();
					List<FolderNode> _newList = new List<FolderNode>();
					foreach(var f in _dirList)
                    {
						if(f.FullPath.StartsWith(_fNode.FullPath)== true)
                        {
							_newList.Add(f);
                        }
                    }
					this._grid.UnselectAll();

					this._grid.DataStore = _newList;
                }
            }
        }



        private void _buttonViewNode_Click(object sender, EventArgs e)
        {
			if(this._grid.SelectedItem != null)
            {
				FolderNode nNode = this._grid.SelectedItem as FolderNode;
				if(nNode != null)
                {
					this._folderView.SelectedItem = nNode;
                }
            }
        }

        private void _grid_SelectedItemsChanged(object sender, EventArgs e)
        {
           if(this._grid.SelectedItem != null)
            {
#if DEBUG
				Log(this._grid.SelectedItem.ToString());
#endif
			}
        }
		private void Log(string s)
        {
			System.Diagnostics.Debug.WriteLine(s);
        }

        private void _grid_ColumnHeaderClick(object sender, GridColumnEventArgs e)
		{
			// throw new NotImplementedException();
			System.Diagnostics.Debug.WriteLine(e.Column.HeaderText.ToString());
			switch (e.Column.HeaderText)
			{
				case "Path\u2193":
					
					this._grid.SuspendLayout();
					_dirList.Sort(new MySortByNameSorter());
					this._grid.DataStore = _dirList;
					e.Column.HeaderText = "Path\u2191";

					this._grid.ResumeLayout();
					break;
				case "Path\u2191":

					this._grid.SuspendLayout();
					_dirList.Sort(new MySortByNameSorter(true));
					this._grid.DataStore = _dirList;
					e.Column.HeaderText = "Path\u2193";
					this._grid.ResumeLayout();
					break;
				case "Size\u2193":
	
					this._grid.SuspendLayout();
					_dirList.Sort(new MySortBySizeSorter());
					e.Column.HeaderText = "Size\u2191";
					this._grid.DataStore = _dirList;
					e.Column.HeaderText = "Size\u2191";
					this._grid.ResumeLayout();
					break;
				case "Size\u2191":

					this._grid.SuspendLayout();
					_dirList.Sort(new MySortBySizeSorter(true));
					this._grid.DataStore = _dirList;
					e.Column.HeaderText = "Size\u2193";
					this._grid.ResumeLayout();
					break;
				default:
					System.Diagnostics.Debug.WriteLine("Not Implemented");
					break;


			}
		}

        private void CmdOpenOptionDialog_Executed(object sender, EventArgs e)
        {
			frmOptionsDialog _frmOption = new frmOptionsDialog();
			_frmOption.DisplayMode = DialogDisplayMode.Separate;
			//_frmOption.
			_frmOption.ShowModal(this);
			_frmOption.Dispose();
			
        }

        private void _folderView_MouseDown(object sender, MouseEventArgs e)
        {
             if(this._folderView.SelectedItem != null)
            {
				System.Diagnostics.Debug.WriteLine($"Screeen Mouse Position : {Eto.Forms.Mouse.Position}");
				//MessageBox.Show(this._folderView.SelectedItem.ToString());
				System.Diagnostics.Debug.WriteLine($"Screeen Mouse Position : {this._folderView.SelectedItem}");
			}
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void CmdSelectFolder_Executed(object sender, EventArgs e)
		{
#if DEBUG
			if (sender is Command)
			{
				var cmd = sender as Command;

			}
#endif
			SelectFolderDialog fDalog = new SelectFolderDialog();
			fDalog.Title = "Select Folder to Ananayze...";
			if (fDalog.ShowDialog(this) == DialogResult.Ok)
			{
                if (System.IO.Directory.Exists(fDalog.Directory) == true)
                {

					StartAnalyzeFolder(fDalog.Directory);
				}
                else
				{
					MessageBox.Show("Folder does not exists!");
				}

			}
			if (fDalog  != null)
			{
				fDalog.Dispose();
				fDalog = null;
			}
		
        }
		/// <summary>
		/// Staart Point Function to lookup folder
		/// </summary>
		/// <param name="_strFolderPath"></param>
		private async void StartAnalyzeFolder(string _strFolderPath)
        {
			_currentFolder = _strFolderPath;
			
			_dirList.Clear();
			this.Invalidate();


			int depth = 0;
			_treeFolderNodeList.Clear();
			if (this._timerUI.Started == true)
			{
				this._timerUI.Stop();
			}
			this._frmProgress.Topmost = true;
			this._frmProgress.Enabled = true;
			this._frmProgress.Show();
			this._frmProgress.StartTimer();
			this._isFormProgressVisibile = true;
			this._timerUI.Start();
			// Reset Counter
			LookedFileCount = 0;
			DirectoryInfo rootDir = new DirectoryInfo(_currentFolder);
			FolderNode rootDirInfo = new FolderNode(_currentFolder, 0, rootDir.LastWriteTime, rootDir.CreationTime, rootDir.LastAccessTime, 0);
			// Expand Only First Node
			rootDirInfo.Expanded = true;
			_treeFolderNodeList.Add(rootDirInfo);
			System.Diagnostics.Debug.WriteLine($"Task Started :{System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()}");
			long result = await Task<long>.Run(() => AnalayzeDirectory(_currentFolder, rootDir, rootDirInfo, depth));
			System.Diagnostics.Debug.WriteLine($"Task Completed :{System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()}");
			this._rootFolderSize = result;

			performDisplayDiskUsage();
			this._isFormProgressVisibile = false;
			this._folderView.SuspendLayout();
			this._folderView.DataStore = new TreeGridItemCollection(_treeFolderNodeList);
			this._folderView.ResumeLayout();
			this._grid.SuspendLayout();
			// Sort By Folder Size at Begining
			_dirList.Sort(new MySortBySizeSorter());

			this._grid.DataStore = _dirList;
			this._grid.ResumeLayout();
			this.menuExportExcel.Enabled = true;

		}
		private void LookupDone()
        {
			System.Diagnostics.Debug.WriteLine($"Task Completed :{System.Threading.Thread.CurrentThread.ToString()}");
			long rootSize = this._lookupTask.Result;

			this._folderView.SuspendLayout();
			this._folderView.DataStore = new TreeGridItemCollection(_treeFolderNodeList);
			this._folderView.ResumeLayout();
		}

		
	
		private  long AnalayzeDirectory(string path,  DirectoryInfo parentFolderDirInfo, FolderNode parentFolderNode, int depth)
        {
			depth++;
			long dirSize = 0;
			System.IO.DirectoryInfo dirInfo = parentFolderDirInfo;
;
			try
			{
				foreach (var f in dirInfo.GetFiles())
				{
					try
					{
						
						System.IO.FileInfo newFileInfo = new FileInfo(f.FullName);
						newFileInfo.Refresh();
						dirSize += newFileInfo.Length;
						LookedFileCount++;
					}
					catch 
					{

					}
				}
			}catch(Exception ex)
            {
				System.Console.WriteLine(ex.Message.ToString());
            }

			try
			{
				foreach (var d in dirInfo.GetDirectories())
				{

					FolderNode _childFolder = new FolderNode(d.FullName,  0 , d.LastWriteTime, d.CreationTime, d.LastAccessTime  , depth);
					long childFolderSize = AnalayzeDirectory(d.FullName, new DirectoryInfo(d.FullName), _childFolder , depth);
					_childFolder.Size = childFolderSize;
					dirSize += childFolderSize;
					parentFolderNode.Size = dirSize;
		
					parentFolderNode.Children.Add(_childFolder);
					_dirList.Add(_childFolder);

				}
			} catch  { };


			return dirSize;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
			this.SuspendLayout();
			this._tableLayout.Size = new Size(this.Size.Width , this.Size.Height - 25);
			//throw new NotImplementedException();
			this.ResumeLayout();
        }
		private void performDisplayDiskUsage()
		{
			System.Diagnostics.Debug.WriteLine("Invoked :" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
			List<FolderNode> _cloneList = new List<FolderNode>();
			foreach (var n in _dirList)
			{
				if (n.Depth == 1)
				{
					_cloneList.Add(n);
				}
			}
			this._rightTopPanel.TotalSize = this._rootFolderSize;

			Eto.Forms.Application.Instance.AsyncInvoke(() => { this._rightTopPanel.DisplayBigFolders(_cloneList); });
		}
		/// <summary>
		/// Export to *.xlsx file using OpenXML
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CmdExportExcel_Executed(object sender, EventArgs e)
		{
			int rowPos = 2;
			try
			{
				var saveDialog = new SaveFileDialog { Directory = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) };
				saveDialog.Filters.Add(new FileFilter("Microsoftot Excel workbook (*.xlsx)", "*.xlsx"));
				saveDialog.Title = "Select file name to save *.xlsx";
				var lastPos = _currentFolder.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
				string strSheetName = _currentFolder.Substring(lastPos);
				saveDialog.FileName = strSheetName + ".xlsx";
				if (saveDialog.ShowDialog(this) != DialogResult.Ok)
                {
					return;
                }
				

				string filePath = saveDialog.FileName;

	
				// ============================================================
				// Export Section
				// ============================================================

				using (var workbook = new XLWorkbook())

				{
					var worksheet = workbook.Worksheets.Add(strSheetName);
					string strFolderName = null;
					worksheet.Cell(1,1).Value = "Folder Name";
					worksheet.Cell(1,2).Value = "Path";
					worksheet.Cell(1,3).Value = "Size";
					worksheet.Cell(1,4).Value = "LastModified";
					foreach(var dirInfo in _dirList)
                    {
						strFolderName = dirInfo.FullPath.Substring(dirInfo.FullPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
						worksheet.Cell(rowPos, 1).Value = strFolderName;
						worksheet.Cell(rowPos, 2).Value = dirInfo.FullPath;
						worksheet.Cell(rowPos, 3).Value = dirInfo.Size;
						worksheet.Cell(rowPos, 4).Value = dirInfo.LastModified;
						rowPos++;

					}

					workbook.SaveAs(filePath);
				}

				// ====================================================================
				System.Diagnostics.Debug.WriteLine("Data is saved to  " + filePath);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + "\r\nRow: " + rowPos.ToString());
			}
		}
	}


	
}
