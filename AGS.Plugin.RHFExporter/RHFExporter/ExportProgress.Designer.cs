namespace RedHerringFarm
{
    partial class ExportProgress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.MainTaskProgress = new System.Windows.Forms.ProgressBar();
            this.SubTaskProgress = new System.Windows.Forms.ProgressBar();
            this.MainTaskName = new System.Windows.Forms.Label();
            this.SubTaskName = new System.Windows.Forms.Label();
            this.InfoBox = new System.Windows.Forms.TextBox();
            this.ExportBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuccessTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // MainTaskProgress
            // 
            this.MainTaskProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.MainTaskProgress.Location = new System.Drawing.Point(12, 25);
            this.MainTaskProgress.Name = "MainTaskProgress";
            this.MainTaskProgress.Size = new System.Drawing.Size(354, 14);
            this.MainTaskProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.MainTaskProgress.TabIndex = 0;
            this.MainTaskProgress.UseWaitCursor = true;
            // 
            // SubTaskProgress
            // 
            this.SubTaskProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SubTaskProgress.Location = new System.Drawing.Point(12, 58);
            this.SubTaskProgress.Name = "SubTaskProgress";
            this.SubTaskProgress.Size = new System.Drawing.Size(354, 15);
            this.SubTaskProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.SubTaskProgress.TabIndex = 1;
            this.SubTaskProgress.UseWaitCursor = true;
            // 
            // MainTaskName
            // 
            this.MainTaskName.AutoSize = true;
            this.MainTaskName.Location = new System.Drawing.Point(9, 9);
            this.MainTaskName.Name = "MainTaskName";
            this.MainTaskName.Size = new System.Drawing.Size(56, 13);
            this.MainTaskName.TabIndex = 2;
            this.MainTaskName.Text = "Working...";
            this.MainTaskName.UseWaitCursor = true;
            // 
            // SubTaskName
            // 
            this.SubTaskName.AutoSize = true;
            this.SubTaskName.Location = new System.Drawing.Point(9, 42);
            this.SubTaskName.Name = "SubTaskName";
            this.SubTaskName.Size = new System.Drawing.Size(56, 13);
            this.SubTaskName.TabIndex = 3;
            this.SubTaskName.Text = "Working...";
            this.SubTaskName.UseWaitCursor = true;
            // 
            // InfoBox
            // 
            this.InfoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoBox.BackColor = System.Drawing.SystemColors.Window;
            this.InfoBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.InfoBox.Location = new System.Drawing.Point(12, 79);
            this.InfoBox.Multiline = true;
            this.InfoBox.Name = "InfoBox";
            this.InfoBox.ReadOnly = true;
            this.InfoBox.Size = new System.Drawing.Size(353, 111);
            this.InfoBox.TabIndex = 4;
            this.InfoBox.TabStop = false;
            this.InfoBox.UseWaitCursor = true;
            // 
            // ExportBackgroundWorker
            // 
            this.ExportBackgroundWorker.WorkerReportsProgress = true;
            this.ExportBackgroundWorker.WorkerSupportsCancellation = true;
            this.ExportBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ExportBackgroundWorker_RunWorkerCompleted);
            this.ExportBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.ExportBackgroundWorker_ProgressChanged);
            // 
            // SuccessTimer
            // 
            this.SuccessTimer.Interval = 750;
            this.SuccessTimer.Tick += new System.EventHandler(this.SuccessTimer_Tick);
            // 
            // ExportProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 202);
            this.Controls.Add(this.InfoBox);
            this.Controls.Add(this.SubTaskName);
            this.Controls.Add(this.MainTaskName);
            this.Controls.Add(this.SubTaskProgress);
            this.Controls.Add(this.MainTaskProgress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportProgress";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Exporting...";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.ExportProgress_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar MainTaskProgress;
        private System.Windows.Forms.ProgressBar SubTaskProgress;
        private System.Windows.Forms.Label SubTaskName;
        private System.Windows.Forms.TextBox InfoBox;
        private System.Windows.Forms.Timer SuccessTimer;
        public System.ComponentModel.BackgroundWorker ExportBackgroundWorker;
        public System.Windows.Forms.Label MainTaskName;
    }
}