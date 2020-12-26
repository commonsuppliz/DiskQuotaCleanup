using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;
namespace DiskQuotaCleanup
{
    class frmProgress : Form
    {
        private UITimer _uiTimer = null;
        private ProgressBar _progressBar = null;
        public frmProgress()
        {
            this.ClientSize = new Eto.Drawing.Size(300, 30);
            this.Title = "Wait...";
            
            this._progressBar = new ProgressBar();
            this._progressBar.Size = new Eto.Drawing.Size(150, 40);
            this._progressBar.MinValue = 0;
            this._progressBar.MaxValue = 10;
            this.Maximizable = false;
            this.Minimizable = false;
            this.Content = this._progressBar;
            this._uiTimer = new UITimer();
            this._uiTimer.Interval = 0.1;
            this._uiTimer.Elapsed += _uiTimer_Elapsed;
            this._uiTimer.Stop();
            this.EnabledChanged += FrmProgress_EnabledChanged;
           
        }

        private void FrmProgress_EnabledChanged(object sender, EventArgs e)
        {
            if (this.Enabled == false)
            {
                this._uiTimer.Stop();
            }
            else
            {
                this._uiTimer.Start();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(this._uiTimer != null)
                {
                    this._uiTimer.Dispose();
                    this._uiTimer = null;
                }
                if(this._progressBar != null)
                {
                    this._progressBar.Dispose();
                    this._progressBar = null;
                }
            }
            base.Dispose(disposing);
        }
        public void StartTimer()
        {
            this._uiTimer.Start();
        }
        public void StopTimer()
        {
            this._uiTimer.Stop();
        }
        private void _uiTimer_Elapsed(object sender, EventArgs e)
        {

            if(this._progressBar.Value >= this._progressBar.MaxValue)
            {
                this._progressBar.Value = 0;
            }
            this._progressBar.Value++;
            this.Title = string.Format("Count: {0} ...", MainForm.LookedFileCount);
        }

    }
}
