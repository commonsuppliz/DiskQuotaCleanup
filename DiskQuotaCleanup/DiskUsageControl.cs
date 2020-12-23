using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Data.Common;
using System.Dynamic;

namespace DiskQuotaCleanup
{
    class DiskUsageControl : Panel
    {
        public static List<Color> ColorsList = new List<Color>();
        internal GroupBox _topGroupBox = null;
        internal Drawable  _drawablePanel = null;
        internal Eto.Forms.Splitter _splitter = null;
        internal GroupBox _bottomGrroupBox = null;

        internal List<FolderNodeEx> _topNodeList = new List<FolderNodeEx>();
        public DiskUsageControl()
        {
            System.Diagnostics.Debug.WriteLine("Conrol Creation Time :" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());

            // allColor = typeof(Colors).GetFields(System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic  );
            // this.Rows.Add(new TableRow())
            //this.Orientation = Orientation.Vertical;
            this._topGroupBox = new GroupBox();
            this._topGroupBox.Size = new Size(300, 100);
            this._topGroupBox.Text = "Data Size Summary Graph for Selected Folder";
            this._drawablePanel = new Drawable();
            ///this._drawablePanel.
            this._drawablePanel.BackgroundColor = Colors.White;
            this._drawablePanel.Paint += _drawablePanel_Paint;
            this._drawablePanel.SizeChanged += _drawablePanel_SizeChanged;
            this._drawablePanel.Size = this._topGroupBox.Size;
            
            
            this._topGroupBox.Content = this._drawablePanel;
            this._bottomGrroupBox = new GroupBox();
            this._bottomGrroupBox.Size = new Size(300, 100);
            this._bottomGrroupBox.Text = "Sub-Directory Size Summary Graph";
            this._splitter = new Splitter();
            this._splitter.Orientation = Orientation.Vertical;
            this._splitter.Position = 100;
            this._splitter.Panel1 = this._topGroupBox;
            this._splitter.Panel2 = this._bottomGrroupBox;
            this.Content = this._splitter;

            


        }

        private void _drawablePanel_MouseDown(object sender, MouseEventArgs e)
        {
            var x = e.Location.X;
            var y = e.Location.Y;
            MessageBox.Show("Sender" + sender.ToString() + "X: " + x.ToString() + " Y:" + y);

        }

        private void _drawablePanel_SizeChanged(object sender, EventArgs e)
        {
            if (_isChildContolSizeChanging == true || this.IsSuspended == true || this.IsSuspended == true)
                return;
            this._drawablePanel.SuspendLayout();
            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToShortTimeString()} : Drawable Sizechanged...");
            int CurrentButtonPos = 10;
            foreach (var eNode in this._topNodeList)
            {
                if (eNode.isOthers == false)
                {
                    eNode.size = GetAdjustedControlSize(eNode, this._drawablePanel.ClientSize);
                    eNode.Locatioon = new Point(CurrentButtonPos, 5);
                    CurrentButtonPos += eNode.size.Width;
                }
                else
                {
                    eNode.Locatioon = new Point(CurrentButtonPos, 5);
                    eNode.size = new Size(this._drawablePanel.Width - CurrentButtonPos - 20, this._drawablePanel.Height - 20);
                }
            

            }
            this._drawablePanel.ResumeLayout();
            this._drawablePanel.Invalidate();
        }

        private void _drawablePanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Font fnt = new Font(FontFamilies.Monospace, 9, FontStyle.Bold);
                foreach(var eNode in this._topNodeList)
                {
                    e.Graphics.FillRectangle(eNode.color, eNode.Locatioon.X, eNode.Locatioon.Y, eNode.size.Width, eNode.size.Height);
                    e.Graphics.DrawText(fnt, Brushes.White, new Point(eNode.Locatioon.X + 5, eNode.Locatioon.Y + 20), eNode.FullPath) ;
                }
                fnt.Dispose();
            }
            catch { }

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }
        private bool _isChildContolSizeChanging = false;


        public const int ReportMaxItems = 15;
        
        public long TotalSize { get; set; }

        [STAThread]
        public void DisplayBigFolders(List<FolderNode> _clonedList)
        {

            this._topNodeList.Clear();
            this._drawablePanel.SuspendLayout();
            //this._topGroupPanel.BeginHorizontal();
           // this.RemoveAll();
            System.Diagnostics.Debug.WriteLine("Invoked :" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
            _clonedList.Sort(new MySortBySizeSorter());
            System.Diagnostics.Debug.WriteLine("Sort Done...");
            long test = 0;
            if(_clonedList.Count > ReportMaxItems)
            {
                test = ReportMaxItems;
            }
            else
            {
                test = _clonedList.Count;
            }
            int CurrentButtonPos = 10;
            int colorCount = MainForm.GlobalColorList.Count;
            for (var i =0; i  < test; i ++)
            {
                FolderNode eNode = _clonedList[i];
           
                double _radtio = (double)eNode.Size / (double)TotalSize;
                
                Random randomColor = new Random();
          
                var enodeEx = new FolderNodeEx(eNode.FullPath, eNode.Size, eNode.LastModified, eNode.CreatedTime, eNode.LastAccessed , eNode.Depth);
                enodeEx.FullPath = GetName(eNode);
                enodeEx.size = GetAdjustedControlSize(eNode, this._drawablePanel.ClientSize);
                enodeEx.color = MainForm.GlobalColorList[randomColor.Next(0, colorCount - 1)];
                enodeEx.Locatioon = new Point(CurrentButtonPos, 5);
                enodeEx.rect = new Rectangle(enodeEx.Locatioon, enodeEx.size);
                int buttonWidthInt = enodeEx.size.Width;
                this._topNodeList.Add(enodeEx);
                
                CurrentButtonPos += buttonWidthInt;
            }
            if(CurrentButtonPos < this._drawablePanel.ClientSize.Width - 20)
            {
 
                var enodeEx = new FolderNodeEx("Others", 0, DateTime.Today, DateTime.Today, DateTime.Today, 0);
                enodeEx.size = new Size(this._drawablePanel.Width - CurrentButtonPos - 20, this._drawablePanel.Height - 20);
                enodeEx.color = Colors.DimGray;
                enodeEx.Locatioon = new Point(CurrentButtonPos, 5);
                enodeEx.rect = new Rectangle(enodeEx.Locatioon, enodeEx.size);
                enodeEx.isOthers = true;
                this._topNodeList.Add(enodeEx);
            }

            this._drawablePanel.ResumeLayout();
            this._drawablePanel.Invalidate();

            
        }
        public Size GetAdjustedControlSize(FolderNode eNode, Size parentSize)
        {
            double _radtio = (double)eNode.Size / (double)TotalSize;

            double buttonWidthDouble = _radtio * this._drawablePanel.Width;
            int buttonWidthInt = (int)buttonWidthDouble;
            if (buttonWidthInt < 1)
            {
                buttonWidthInt = 1;
            }
            return  new Size(buttonWidthInt, this._drawablePanel.Height - 20);
        }
        private string GetName(FolderNode fNode)
        {
            if(string.IsNullOrEmpty(fNode.FullPath) == false)
            {
                int lastPos = fNode.FullPath.LastIndexOfAny(new char[] { '\\', '/' });
                if(lastPos > 0)
                {
                    return fNode.FullPath.Substring(lastPos + 1);
                }
            }
            return "";
        }
        

    }
    public class FolderNodeEx : FolderNode
    {
        public Color color { get; set; }
        public Point Locatioon { get; set; }
        public Size size { get; set; }
        public Rectangle rect {get;set;}
        public bool isOthers { get; set; }
        public FolderNodeEx(string _strpath, long _size, DateTime dt, DateTime creationTime, DateTime lastAccess, int depth) : base(_strpath, _size, dt, creationTime , lastAccess , depth)
        {
        }
            
    }
}
