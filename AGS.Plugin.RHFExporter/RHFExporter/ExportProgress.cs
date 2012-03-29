
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using RedHerringFarm.TaskManaging;
using System.IO;

namespace RedHerringFarm
{
    public partial class ExportProgress : Form
    {
        public ExportProgress()
        {
            InitializeComponent();
        }

        private TaskManager taskManager;

        public void SetTaskManager(TaskManager taskManager)
        {
            this.taskManager = taskManager;
            taskManager.Started += new EventHandler(taskManager_Started);
            taskManager.StatusUpdated += new StatusUpdateEventHandler(taskManager_StatusUpdated);
            taskManager.Finished += new EventHandler(taskManager_Finished);
        }

        void taskManager_Finished_Inner(object sender, EventArgs e)
        {
            TaskManager tm = (TaskManager)sender;
            if (tm.Parent == taskManager)
            {
                MainTaskName.Text = "";
            }
            else
            {
                SubTaskName.Text = "";
                SubTaskProgress.Value = 100;
            }
            MainTaskProgress.Value = taskManager.PercentComplete;
        }

        void taskManager_Finished(object sender, EventArgs e)
        {
            Invoke(new EventHandler(taskManager_Finished_Inner), sender, e);
        }

        void taskManager_StatusUpdated_Inner(object sender, StatusUpdateEventArgs e)
        {
            TaskManager tm = (TaskManager)sender;
            if (tm == taskManager)
            {
                MainTaskName.Text = e.Text;
            }
            else
            {
                SubTaskName.Text = e.Text;
            }
            AddLineToInfoBox(e.Text);
        }

        void taskManager_StatusUpdated(object sender, StatusUpdateEventArgs e)
        {
            Invoke(new StatusUpdateEventHandler(taskManager_StatusUpdated_Inner), sender, e);
        }

        void taskManager_Started_Inner(object sender, EventArgs e)
        {
            TaskManager tm = (TaskManager)sender;
            if (tm.Parent == taskManager)
            {
                MainTaskName.Text = tm.Name;
            }
            else
            {
                SubTaskName.Text = tm.Name;
            }
            SubTaskProgress.Value = 0;
            if (tm.Unpredictable)
            {
                if (SubTaskProgress.Style != ProgressBarStyle.Marquee)
                {
                    SubTaskProgress.Style = ProgressBarStyle.Marquee;
                }
            }
            else
            {
                if (SubTaskProgress.Style != ProgressBarStyle.Continuous)
                {
                    SubTaskProgress.Style = ProgressBarStyle.Continuous;
                }
            }
            AddLineToInfoBox(tm.Name);
        }

        void taskManager_Started(object sender, EventArgs e)
        {
            Invoke(new EventHandler(taskManager_Started_Inner), sender, e);
        }

        private void ExportProgress_Load(object sender, EventArgs e)
        {
            MainTaskName.Text = "";
            SubTaskName.Text = "";
            ExportBackgroundWorker.RunWorkerAsync();
        }

        private void ExportBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            File.WriteAllText("export.log", InfoBox.Text);
            if (e.Error != null)
            {
                MessageBox.Show(
                    HacksAndKludges.GetMainWindow(),
                    "Export failed!\r\n\r\n" + e.Error.ToString(),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
            else
            {
                this.MainTaskName.Text = "";
                this.MainTaskProgress.Value = 100;
                this.SubTaskName.Text = "";
                this.SubTaskProgress.Value = 0;
                SuccessTimer.Enabled = true;
            }
        }

        public void SetMainTask(string name)
        {
            MainTaskName.Text = name;
            AddLineToInfoBox(name);
        }

        public void AddLineToInfoBox(string line)
        {
            InfoBox.Text += line + "\r\n";
            InfoBox.SelectionStart = InfoBox.Text.Length;
            InfoBox.ScrollToCaret();
        }

        private void ExportBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.MainTaskProgress.Value = e.ProgressPercentage;
        }

        private void SuccessTimer_Tick(object sender, EventArgs e)
        {
            SuccessTimer.Enabled = false;
            MessageBox.Show(
                this,
                "Export completed successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            this.Close();
        }
    }
}
