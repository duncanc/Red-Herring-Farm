namespace RedHerringFarm
{
    partial class SettingsPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.PNGToolTextBox = new System.Windows.Forms.TextBox();
            this.PNGToolButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(266, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "PNG Post-Processing Tool (e.g. PNGOut, OptiPNG, ...)";
            // 
            // PNGToolTextBox
            // 
            this.PNGToolTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PNGToolTextBox.Location = new System.Drawing.Point(6, 19);
            this.PNGToolTextBox.Name = "PNGToolTextBox";
            this.PNGToolTextBox.Size = new System.Drawing.Size(513, 20);
            this.PNGToolTextBox.TabIndex = 1;
            this.PNGToolTextBox.TextChanged += new System.EventHandler(this.PNGToolTextBox_TextChanged);
            // 
            // PNGToolButton
            // 
            this.PNGToolButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PNGToolButton.Location = new System.Drawing.Point(525, 19);
            this.PNGToolButton.Name = "PNGToolButton";
            this.PNGToolButton.Size = new System.Drawing.Size(27, 19);
            this.PNGToolButton.TabIndex = 2;
            this.PNGToolButton.Text = "...";
            this.PNGToolButton.UseVisualStyleBackColor = true;
            this.PNGToolButton.Click += new System.EventHandler(this.PNGToolButton_Click);
            // 
            // SettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PNGToolButton);
            this.Controls.Add(this.PNGToolTextBox);
            this.Controls.Add(this.label1);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(562, 356);
            this.Load += new System.EventHandler(this.SettingsPanel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox PNGToolTextBox;
        private System.Windows.Forms.Button PNGToolButton;

    }
}
