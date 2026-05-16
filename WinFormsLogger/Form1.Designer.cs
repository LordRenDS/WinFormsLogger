namespace WinFormsLogger
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            bindingSource1 = new BindingSource(components);
            activeProcessTimer = new System.Windows.Forms.Timer(components);
            splitContainer1 = new SplitContainer();
            dataGridView1 = new DataGridView();
            lstServerLogs = new ListBox();
            Id = new DataGridViewTextBoxColumn();
            ProcessName = new DataGridViewTextBoxColumn();
            WindowsName = new DataGridViewTextBoxColumn();
            ProcessStart = new DataGridViewTextBoxColumn();
            Duration = new DataGridViewTextBoxColumn();
            IsSynced = new DataGridViewTextBoxColumn();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem1 = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            showLogsToolStripMenuItem = new ToolStripMenuItem();
            accountToolStripMenuItem = new ToolStripMenuItem();
            loginToolStripMenuItem = new ToolStripMenuItem();
            logoutToolStripMenuItem = new ToolStripMenuItem();
            syncToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            syncNowToolStripMenuItem = new ToolStripMenuItem();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            openToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            menuStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // activeProcessTimer
            // 
            activeProcessTimer.Interval = 1000;
            activeProcessTimer.Tick += ActiveProcessTimer_Tick;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(6, 30);
            splitContainer1.Margin = new Padding(0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lstServerLogs);
            splitContainer1.Size = new Size(688, 302);
            splitContainer1.SplitterDistance = 183;
            splitContainer1.SplitterWidth = 6;
            splitContainer1.TabIndex = 0;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Id, ProcessName, WindowsName, ProcessStart, Duration, IsSynced });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.Size = new Size(688, 183);
            dataGridView1.TabIndex = 0;
            // 
            // lstServerLogs
            // 
            lstServerLogs.Dock = DockStyle.Fill;
            lstServerLogs.IntegralHeight = false;
            lstServerLogs.Location = new Point(0, 0);
            lstServerLogs.Name = "lstServerLogs";
            lstServerLogs.Size = new Size(688, 113);
            lstServerLogs.TabIndex = 1;
            // 
            // Id
            // 
            Id.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            Id.DataPropertyName = "Id";
            Id.HeaderText = "ID";
            Id.Name = "Id";
            Id.ReadOnly = true;
            // 
            // ProcessName
            // 
            ProcessName.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            ProcessName.DataPropertyName = "ProcessName";
            ProcessName.HeaderText = "Процес";
            ProcessName.Name = "ProcessName";
            ProcessName.ReadOnly = true;
            // 
            // WindowsName
            // 
            WindowsName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            WindowsName.DataPropertyName = "WindowsName";
            WindowsName.HeaderText = "Заголовок вікна";
            WindowsName.Name = "WindowsName";
            WindowsName.ReadOnly = true;
            // 
            // ProcessStart
            // 
            ProcessStart.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            ProcessStart.DataPropertyName = "ProcessStart";
            dataGridViewCellStyle1.Format = "G";
            dataGridViewCellStyle1.NullValue = null;
            ProcessStart.DefaultCellStyle = dataGridViewCellStyle1;
            ProcessStart.HeaderText = "Початок";
            ProcessStart.Name = "ProcessStart";
            ProcessStart.ReadOnly = true;
            // 
            // Duration
            // 
            Duration.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            Duration.DataPropertyName = "Duration";
            Duration.HeaderText = "Тривалість (с)";
            Duration.Name = "Duration";
            Duration.ReadOnly = true;
            // 
            // IsSynced
            // 
            IsSynced.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            IsSynced.DataPropertyName = "IsSynced";
            IsSynced.HeaderText = "Синхр.";
            IsSynced.Name = "IsSynced";
            IsSynced.ReadOnly = true;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, accountToolStripMenuItem, syncToolStripMenuItem });
            menuStrip1.Location = new Point(6, 6);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(688, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem1 });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem1
            // 
            exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            exitToolStripMenuItem1.Size = new Size(180, 22);
            exitToolStripMenuItem1.Text = "E&xit";
            exitToolStripMenuItem1.Click += exitToolStripMenuItem1_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showLogsToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "&View";
            // 
            // showLogsToolStripMenuItem
            // 
            showLogsToolStripMenuItem.CheckOnClick = true;
            showLogsToolStripMenuItem.Name = "showLogsToolStripMenuItem";
            showLogsToolStripMenuItem.Size = new Size(180, 22);
            showLogsToolStripMenuItem.Text = "Server &Logs";
            showLogsToolStripMenuItem.Click += showLogsToolStripMenuItem_Click;
            // 
            // accountToolStripMenuItem
            // 
            accountToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loginToolStripMenuItem, logoutToolStripMenuItem });
            accountToolStripMenuItem.Name = "accountToolStripMenuItem";
            accountToolStripMenuItem.Size = new Size(64, 20);
            accountToolStripMenuItem.Text = "Account";
            // 
            // loginToolStripMenuItem
            // 
            loginToolStripMenuItem.Name = "loginToolStripMenuItem";
            loginToolStripMenuItem.Size = new Size(112, 22);
            loginToolStripMenuItem.Text = "Login";
            loginToolStripMenuItem.Click += loginToolStripMenuItem_Click;
            // 
            // logoutToolStripMenuItem
            // 
            logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            logoutToolStripMenuItem.Size = new Size(112, 22);
            logoutToolStripMenuItem.Text = "Logout";
            logoutToolStripMenuItem.Click += logoutToolStripMenuItem_Click;
            // 
            // syncToolStripMenuItem
            // 
            syncToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, syncNowToolStripMenuItem });
            syncToolStripMenuItem.Name = "syncToolStripMenuItem";
            syncToolStripMenuItem.Size = new Size(44, 20);
            syncToolStripMenuItem.Text = "Sync";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(127, 22);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // syncNowToolStripMenuItem
            // 
            syncNowToolStripMenuItem.Name = "syncNowToolStripMenuItem";
            syncNowToolStripMenuItem.Size = new Size(127, 22);
            syncNowToolStripMenuItem.Text = "Sync Now";
            syncNowToolStripMenuItem.Click += syncNowToolStripMenuItem_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("$this.Icon");
            notifyIcon1.Text = "WinFormsLogger";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openToolStripMenuItem, exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(104, 48);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(103, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(103, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 338);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form1";
            Padding = new Padding(6);
            Text = "WinFormsLogger";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private DataGridView dataGridView1;
        private ListBox lstServerLogs;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewTextBoxColumn ProcessName;
        private DataGridViewTextBoxColumn WindowsName;
        private DataGridViewTextBoxColumn ProcessStart;
        private DataGridViewTextBoxColumn Duration;
        private DataGridViewTextBoxColumn IsSynced;
        private BindingSource bindingSource1;
        private System.Windows.Forms.Timer activeProcessTimer;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem1;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem showLogsToolStripMenuItem;
        private ToolStripMenuItem accountToolStripMenuItem;
        private ToolStripMenuItem loginToolStripMenuItem;
        private ToolStripMenuItem logoutToolStripMenuItem;
        private ToolStripMenuItem syncToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem syncNowToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;    }
}
