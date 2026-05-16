namespace WinFormsLogger.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblServerUrl = new Label();
            txtServerUrl = new TextBox();
            lblSyncInterval = new Label();
            numSyncInterval = new NumericUpDown();
            btnSave = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numSyncInterval).BeginInit();
            SuspendLayout();
            // 
            // lblServerUrl
            // 
            lblServerUrl.AutoSize = true;
            lblServerUrl.Location = new Point(12, 15);
            lblServerUrl.Name = "lblServerUrl";
            lblServerUrl.Size = new Size(63, 15);
            lblServerUrl.TabIndex = 0;
            lblServerUrl.Text = "Server URL:";
            // 
            // txtServerUrl
            // 
            txtServerUrl.Location = new Point(130, 12);
            txtServerUrl.Name = "txtServerUrl";
            txtServerUrl.Size = new Size(242, 23);
            txtServerUrl.TabIndex = 1;
            // 
            // lblSyncInterval
            // 
            lblSyncInterval.AutoSize = true;
            lblSyncInterval.Location = new Point(12, 43);
            lblSyncInterval.Name = "lblSyncInterval";
            lblSyncInterval.Size = new Size(112, 15);
            lblSyncInterval.TabIndex = 2;
            lblSyncInterval.Text = "Sync Interval (min):";
            // 
            // numSyncInterval
            // 
            numSyncInterval.Location = new Point(130, 41);
            numSyncInterval.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            numSyncInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSyncInterval.Name = "numSyncInterval";
            numSyncInterval.Size = new Size(60, 23);
            numSyncInterval.TabIndex = 3;
            numSyncInterval.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnSave
            // 
            btnSave.Location = new Point(216, 80);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 4;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(297, 80);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // SettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(384, 115);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(numSyncInterval);
            Controls.Add(lblSyncInterval);
            Controls.Add(txtServerUrl);
            Controls.Add(lblServerUrl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)numSyncInterval).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblServerUrl;
        private TextBox txtServerUrl;
        private Label lblSyncInterval;
        private NumericUpDown numSyncInterval;
        private Button btnSave;
        private Button btnCancel;
    }
}
