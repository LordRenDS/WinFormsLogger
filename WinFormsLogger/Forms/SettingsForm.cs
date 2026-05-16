using WinFormsLogger.Services;

namespace WinFormsLogger.Forms;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;

    public SettingsForm(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        
        txtServerUrl.Text = _settings.ServerUrl;
        numSyncInterval.Value = _settings.SyncIntervalMinutes;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        _settings.ServerUrl = txtServerUrl.Text;
        _settings.SyncIntervalMinutes = (int)numSyncInterval.Value;
        _settings.Save();
        
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
