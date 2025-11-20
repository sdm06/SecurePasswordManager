# SecureVault
A secure, offline-first password manager built with .NET 8 and Avalonia UI. Features AES-GCM encryption, local SQLite storage, zero-knowledge architecture, and a monochrome terminal-style interface.
# SecureVault 🔒

**SecureVault** is a lightweight, offline-first password manager built with **.NET 8** and **Avalonia UI**. Designed for users who prioritize privacy and local control over cloud synchronization.

It features a high-contrast, monochrome "Terminal" aesthetic and military-grade encryption standards.
<img width="1907" height="1016" alt="image" src="https://github.com/user-attachments/assets/e75d59e9-775d-4d98-9bfb-325f4c2b79b8" />





## 🚀 Features

* **100% Offline:** Your passwords never leave your device. No cloud, no API, no tracking.
* **Zero-Knowledge:** We cannot recover your data. If you lose your Master Password, your data is cryptographically inaccessible.
* **Strong Encryption:** Uses **AES-256-GCM** (Galois/Counter Mode) for both encryption and data integrity verification.
* **Secure Storage:** Data is stored in a local SQLite database using Salted PBKDF2 key derivation.
* **Cross-Platform:** Runs natively on Linux (Arch/Debian/etc.), Windows, and macOS via Avalonia UI.
* **Built-in Generator:** Includes a Cryptographically Secure Pseudo-Random Number Generator (CSPRNG) for creating strong passwords.
* **Clipboard Safety:** One-click copy functionality.

## 🛠 Tech Stack

* **Framework:** .NET 8 (LTS)
* **UI Library:** Avalonia UI (MVVM Pattern)
* **Database:** SQLite (via Entity Framework Core)
* **Crypto:** `System.Security.Cryptography` (Native .NET libraries)

## 🔐 Security Architecture

SecureVault uses industry-standard cryptographic primitives:

1.  **Key Derivation:** The Master Password is never stored. It is hashed using **PBKDF2** (Rfc2898DeriveBytes) with a unique random Salt and 600,000 iterations (SHA-256) to derive the AES Key.
2.  **Encryption:** Data is encrypted using **AES-GCM**. This ensures that data cannot be read (Confidentiality) and has not been tampered with (Integrity).
3.  **Randomness:** All Salts, IVs (Nonces), and generated passwords use the OS-level CSPRNG (`RandomNumberGenerator`).

## 📦 Installation & Setup

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build from Source

```bash
# 1. Clone the repository
git clone https://github.com/sdm06/SecurePasswordManager.git
cd SecureVault

# 2. Restore dependencies
dotnet restore

# 3. Initialize the Database (First time only)
cd SecureVault.Linux
dotnet ef database update

# 4. Run the application
dotnet run
