using System;
using System.ComponentModel.DataAnnotations;

namespace SecureVault.Linux.Models;

public class PasswordEntry
{
    [Key]
    public int Id { get;  set; }

    public string ServiceName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    
    public byte[] EncryptedPayload { get; set; } = Array.Empty<byte>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
}