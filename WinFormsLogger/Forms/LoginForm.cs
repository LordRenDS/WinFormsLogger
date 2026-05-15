using System.ComponentModel;

namespace WinFormsLogger.Forms;

public partial class LoginForm : Form
{
    private IContainer? components = null;
    private Label lblUsername = null!;
    private Label lblPassword = null!;
    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;
    private Button btnCancel = null!;

    public string Username => txtUsername.Text;
    public string Password => txtPassword.Text;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.lblUsername = new Label();
        this.lblPassword = new Label();
        this.txtUsername = new TextBox();
        this.txtPassword = new TextBox();
        this.btnLogin = new Button();
        this.btnCancel = new Button();
        this.SuspendLayout();

        // lblUsername
        this.lblUsername.AutoSize = true;
        this.lblUsername.Location = new Point(12, 15);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new Size(100, 15);
        this.lblUsername.Text = "Username:";

        // txtUsername
        this.txtUsername.Location = new Point(12, 33);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.Size = new Size(260, 23);

        // lblPassword
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new Point(12, 65);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new Size(100, 15);
        this.lblPassword.Text = "Password:";

        // txtPassword
        this.txtPassword.Location = new Point(12, 83);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '*';
        this.txtPassword.Size = new Size(260, 23);

        // btnLogin
        this.btnLogin.Location = new Point(116, 120);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new Size(75, 23);
        this.btnLogin.Text = "Login";
        this.btnLogin.UseVisualStyleBackColor = true;
        this.btnLogin.Click += btnLogin_Click;

        // btnCancel
        this.btnCancel.Location = new Point(197, 120);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new Size(75, 23);
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += btnCancel_Click;

        // LoginForm
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(284, 161);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnLogin);
        this.Controls.Add(this.txtPassword);
        this.Controls.Add(this.lblPassword);
        this.Controls.Add(this.txtUsername);
        this.Controls.Add(this.lblUsername);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoginForm";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Login";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private void btnLogin_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Please enter username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
}
