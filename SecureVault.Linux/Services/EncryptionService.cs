using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureVault.Linux.Services;

public class EncryptionService
{
    // CONSTANTS
    private const int KeySize = 32; 
    private const int NonceSize = 12; 
    private const int TagSize = 16; 
    
    // KEY DERIVATION (PBKDF2)
    public byte[] DeriveKeyFromPassword(string password, byte[] salt)
    {
        using var kdf = new Rfc2898DeriveBytes(password, salt, 600000, HashAlgorithmName.SHA256);
        
        return kdf.GetBytes(KeySize); // Returns exactly 32 bytes
    }
    
    //ENCRYPTION (AES-GCM)
    public byte[] Encrypt(string plainText, byte[] key)
    {
        //Convert the string to raw bytes
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        //Create containers for our crypto parts
        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] cipherText = new byte[plainBytes.Length];

        //Generate a BRAND NEW random Nonce. 
        RandomNumberGenerator.Fill(nonce);

        //Perform the Encryption
        using (var aes = new AesGcm(key, TagSize))
        {
            // This function fills 'cipherText' with encrypted data 
            // and fills 'tag' with the security signature.
            aes.Encrypt(nonce, plainBytes, cipherText, tag);
        }
        
        var combined = new byte[NonceSize + TagSize + cipherText.Length];
        
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
        Buffer.BlockCopy(cipherText, 0, combined, NonceSize + TagSize, cipherText.Length);

        return combined;
    }
    
    // DECRYPTION
    public string Decrypt(byte[] encryptedData, byte[] key)
    {
        // Basic validation
        if (encryptedData.Length < NonceSize + TagSize)
            throw new ArgumentException("Invalid encrypted data format");
        
        // 1. Get Nonce
        byte[] nonce = new byte[NonceSize];
        Buffer.BlockCopy(encryptedData, 0, nonce, 0, NonceSize);

        // 2. Get Tag
        byte[] tag = new byte[TagSize];
        Buffer.BlockCopy(encryptedData, NonceSize, tag, 0, TagSize);

        // 3. Get CipherText
        int cipherTextLength = encryptedData.Length - NonceSize - TagSize;
        byte[] cipherText = new byte[cipherTextLength];
        Buffer.BlockCopy(encryptedData, NonceSize + TagSize, cipherText, 0, cipherTextLength);

        //Prepare output container
        byte[] plainBytes = new byte[cipherTextLength];

        //Decrypt and Verify
        using (var aes = new AesGcm(key, TagSize))
        {
            // If the password is wrong OR if the data is corrupted/hacked,
            // this line will crash with a CryptographicException.
            aes.Decrypt(nonce, cipherText, tag, plainBytes);
        }

        //Convert back to string
        return Encoding.UTF8.GetString(plainBytes);
    }
    
    public static byte[] GenerateSalt()
    {
        byte[] salt = new byte[32];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}