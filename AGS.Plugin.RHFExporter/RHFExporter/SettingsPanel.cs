using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace RedHerringFarm
{
    public partial class SettingsPanel : AGS.Types.EditorContentPanel
    {
        public AGS.Types.ContentDocument contentDocument;
        private ExporterPlugin parentPlugin;

        public SettingsPanel(ExporterPlugin parentPlugin)
        {
            this.parentPlugin = parentPlugin;
            InitializeComponent();
        }

        private void PNGToolButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Please select an .EXE file to use...";
            openDialog.Filter = "Executable tools (*.exe)|*.exe";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                PNGToolTextBox.Text = openDialog.FileName;
            }
        }

        private void PNGToolTextBox_TextChanged(object sender, EventArgs e)
        {
            parentPlugin.Settings.PngTool = PNGToolTextBox.Text;
        }

        private void SettingsPanel_Load(object sender, EventArgs e)
        {
            PNGToolTextBox.Text = parentPlugin.Settings.PngTool;
        }
    }
}
