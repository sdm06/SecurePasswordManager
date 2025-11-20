using System.Collections.Generic;
using System.Linq;
using SecureVault.Linux.Data;
using SecureVault.Linux.Models;

namespace SecureVault.Linux.Services;

public class PasswordService
{
    private readonly EncryptionService _encryption;

    public PasswordService()
    {
        _encryption = new EncryptionService();
    }

    public List<PasswordEntry> GetAll()
    {
        using var db = new AppDbContext();
        return db.Passwords.OrderBy(x => x.ServiceName).ToList();
    }

    public void AddPassword(string service, string username, string rawPassword, byte[] key)
    {
        byte[] encryptedBlob = _encryption.Encrypt(rawPassword, key);

        var entry = new PasswordEntry()
        {
            ServiceName = service,
            Username = username,
            EncryptedPayload = encryptedBlob
        };

        using var db = new AppDbContext();
        db.Passwords.Add(entry);
        db.SaveChanges();
    }

    public string DecryptPassword(int id, byte[] key)
    {
        using var db = new AppDbContext();
        var entry = db.Passwords.Find(id);

        if (entry == null) return string.Empty;
        return _encryption.Decrypt(entry.EncryptedPayload, key);
    }

    public void DeletePassword(int id)
    {
        using var db = new AppDbContext();
        var entry = db.Passwords.Find(id);
        if (entry != null)
        {
            db.Passwords.Remove(entry);
            db.SaveChanges();
        }
    }
}