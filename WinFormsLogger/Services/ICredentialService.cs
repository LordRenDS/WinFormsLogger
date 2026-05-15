namespace WinFormsLogger.Services;

public interface ICredentialService
{
    void SaveCredentials(string username, string password);
    (string username, string password)? GetCredentials();
    void DeleteCredentials();
}
