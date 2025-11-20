using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SecureVault.Linux.Models;

namespace SecureVault.Linux.Data;

public class AppDbContext : DbContext
{
    public DbSet<PasswordEntry> Passwords { get; set; }

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);

        var dbPath = Path.Join(path, "SecureVault");
        Directory.CreateDirectory(dbPath);
        
        var dbFile = Path.Join(dbPath, "vault.db");
        
        optionsBuilder.UseSqlite($"Data Source= {dbFile}");
    }
}