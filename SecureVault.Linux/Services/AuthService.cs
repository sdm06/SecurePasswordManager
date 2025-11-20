using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Linux.Services;

public class AuthService
{
    private readonly EncryptionService _encryptionService;
    private const string SaltFileName = "vault.salt";
    private const string CheckFileName = "vault.chack";

    public AuthService()
    {
        _encryptionService = new EncryptionService();
    }

    private string GetConfigPath(string fileName)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Path.Join(Environment.GetFolderPath(folder), "SecureVault");
        Directory.CreateDirectory(path);
        return Path.Join(path, fileName);
    }
    
    public bool IsVaultInitialized()
    {
        return File.Exists(GetConfigPath(SaltFileName));
    }

    public void Register(string password)
    {
        byte[] salt = EncryptionService.GenerateSalt();
        File.WriteAllBytes(GetConfigPath(SaltFileName), salt);
        byte[] key = _encryptionService.DeriveKeyFromPassword(password, salt);
        byte[] validationBlob = _encryptionService.Encrypt("VALID", key);
        File.WriteAllBytes(GetConfigPath(CheckFileName), validationBlob);
    }

    public byte[]? Login(string password)
    {
        if (!IsVaultInitialized()) return null;

        try
        {
            byte[] salt = File.ReadAllBytes(GetConfigPath(SaltFileName));
            byte[] key = _encryptionService.DeriveKeyFromPassword(password, salt);

            byte[] checkBlob = File.ReadAllBytes(GetConfigPath(CheckFileName));
            string result = _encryptionService.Decrypt(checkBlob, key);

            if (result == "VALID")
            {
                return key;
            }
        }
        catch (CryptographicException)
        {

        }
        catch (Exception)
        {
            
        }
        return null;
    }
}