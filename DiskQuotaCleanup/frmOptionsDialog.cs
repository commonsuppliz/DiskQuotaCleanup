using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;
using Eto.Drawing;

namespace DiskQuotaCleanup
{
    public class frmOptionsDialog : Dialog 
    {
        private Button _buttonOK = null;
        private Button _buttonCanel = null;
        private TabControl _tab = null;
        private PixelLayout _panel;
        /// <summary>
        /// LookUp File Options
        /// </summary>
        private TabPage _tabFileOptions = null;
        private PixelLayout _tabFileOptionsPanel = null;

        private CheckBox _chkReadHiddenFile = null;
        /// <summary>
        /// Remove Fiel Options
        /// </summary>
        private TabPage _tabRemoveOptions = null;
        private PixelLayout _tabRemoveOptionsPanel = null;
        public frmOptionsDialog()
        {
            this.ClientSize = new Size(300, 300);
            this.Title = "TODO:";
            _buttonOK = new Button();
            _buttonCanel = new Button();
            _buttonOK.Text = "OK";
            _buttonCanel.Text = "Cancel";
            _buttonOK.Size = new Size(100, 25);
            _buttonCanel.Size = new Size(100, 25);
            _buttonOK.Click += _buttonOK_Click;
            _buttonCanel.Click += _buttonCanel_Click;
            _tab = new TabControl();
            _tab.BackgroundColor = Colors.Gainsboro;
            _tab.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 30);
            _tabFileOptions = new TabPage();
            _tabRemoveOptions = new TabPage();
            _tabFileOptions.Text = "File Option";
            _chkReadHiddenFile = new CheckBox();
            _chkReadHiddenFile.Text = "Read Hidden Files";
            _tabFileOptionsPanel = new PixelLayout();
            _tabFileOptionsPanel.Add(_chkReadHiddenFile, 0, 0);
            _tabFileOptions.Content = _tabFileOptionsPanel;
            _tabRemoveOptions.Text = "Remove Option";

            _tab.Pages.Add(_tabFileOptions);
            _tab.Pages.Add(_tabRemoveOptions);
            _tab.SelectedIndexChanged += _tab_SelectedIndexChanged;
            _panel = new PixelLayout();
            _panel.Size = this.ClientSize;
            _panel.Add(_tab, 0, 0);
            _panel.Add(_buttonOK, 0, this.ClientSize.Height - 28);
            _panel.Add(_buttonCanel, 200, this.ClientSize.Height - 28);

            this.Content = _panel;
        }

        private void _tab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._tab.SelectedPage != null)
            {
                System.Diagnostics.Debug.WriteLine(this._tab.SelectedPage.Text.ToString() + " is selected");
            }
        }

        private void _buttonCanel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void _buttonOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
