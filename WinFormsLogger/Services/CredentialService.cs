using CredentialManagement;

namespace WinFormsLogger.Services;

public class CredentialService : ICredentialService
{
    private const string TargetName = "WinFormsLogger_UserCredentials";

    public void SaveCredentials(string username, string password)
    {
        using var cred = new Credential
        {
            Target = TargetName,
            Username = username,
            Password = password,
            PersistanceType = PersistanceType.LocalComputer,
            Type = CredentialType.Generic
        };
        cred.Save();
    }

    public (string username, string password)? GetCredentials()
    {
        using var cred = new Credential { Target = TargetName };
        if (cred.Load())
        {
            return (cred.Username, cred.Password);
        }
        return null;
    }

    public void DeleteCredentials()
    {
        using var cred = new Credential { Target = TargetName };
        cred.Delete();
    }
}
