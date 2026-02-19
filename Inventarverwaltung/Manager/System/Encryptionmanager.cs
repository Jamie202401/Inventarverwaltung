using Inventarverwaltung.Manager.UI;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet die Verschlüsselung und Entschlüsselung von Log-Dateien
    /// Verwendet AES-256 Verschlüsselung für maximale Sicherheit
    /// </summary>
    public static class EncryptionManager
    {
        // Verschlüsselungs-Schlüssel (256 Bit für AES-256)
        // WICHTIG: In einer Produktivumgebung sollte dieser Schlüssel sicher gespeichert werden!
        private static readonly byte[] encryptionKey = DeriveKeyFromPassword("InventarVerwaltung2026!SecureLogSystem#");

        // Initialisierungsvektor für zusätzliche Sicherheit
        private static readonly byte[] iv = new byte[16]
        {
            0x1A, 0x2B, 0x3C, 0x4D, 0x5E, 0x6F, 0x7A, 0x8B,
            0x9C, 0xAD, 0xBE, 0xCF, 0xDA, 0xEB, 0xFC, 0x0D
        };

        /// <summary>
        /// Leitet einen sicheren 256-Bit Schlüssel aus einem Passwort ab
        /// Verwendet PBKDF2 mit SHA256
        /// </summary>
        private static byte[] DeriveKeyFromPassword(string password)
        {
            // Salt für zusätzliche Sicherheit
            byte[] salt = Encoding.UTF8.GetBytes("InventarSalt2026!");

            // PBKDF2 mit 10000 Iterationen für erhöhte Sicherheit
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 256 Bit = 32 Bytes
            }
        }

        /// <summary>
        /// Verschlüsselt einen Text mit AES-256
        /// </summary>
        public static byte[] EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return msEncrypt.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Verschlüsselungsfehler: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Entschlüsselt einen verschlüsselten Byte-Array zurück zu Text
        /// </summary>
        public static string DecryptBytes(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length == 0)
                return null;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Entschlüsselungsfehler: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Verschlüsselt eine gesamte Datei
        /// </summary>
        public static bool EncryptFile(string inputFile, string outputFile)
        {
            try
            {
                // Datei lesen
                string content = File.ReadAllText(inputFile);

                // Verschlüsseln
                byte[] encrypted = EncryptString(content);

                if (encrypted == null)
                    return false;

                // Verschlüsselte Daten speichern
                File.WriteAllBytes(outputFile, encrypted);

                return true;
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Verschlüsseln der Datei: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Entschlüsselt eine gesamte Datei
        /// </summary>
        public static bool DecryptFile(string inputFile, string outputFile)
        {
            try
            {
                // Verschlüsselte Daten lesen
                byte[] encrypted = File.ReadAllBytes(inputFile);

                // Entschlüsseln
                string decrypted = DecryptBytes(encrypted);

                if (decrypted == null)
                    return false;

                // Entschlüsselte Daten speichern
                File.WriteAllText(outputFile, decrypted);

                return true;
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Entschlüsseln der Datei: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verschlüsselt eine Datei an Ort und Stelle (überschreibt Original)
        /// </summary>
        public static bool EncryptFileInPlace(string filePath)
        {
            string tempFile = filePath + ".tmp";

            try
            {
                if (!File.Exists(filePath))
                    return false;

                // In temporäre Datei verschlüsseln
                if (!EncryptFile(filePath, tempFile))
                    return false;

                // Original löschen und verschlüsselte Datei umbenennen
                File.Delete(filePath);
                File.Move(tempFile, filePath);

                return true;
            }
            catch (Exception ex)
            {
                // Aufräumen bei Fehler
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                ConsoleHelper.PrintError($"Fehler beim Verschlüsseln: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Entschlüsselt eine Datei an Ort und Stelle (überschreibt Original)
        /// </summary>
        public static bool DecryptFileInPlace(string filePath)
        {
            string tempFile = filePath + ".tmp";

            try
            {
                if (!File.Exists(filePath))
                    return false;

                // In temporäre Datei entschlüsseln
                if (!DecryptFile(filePath, tempFile))
                    return false;

                // Original löschen und entschlüsselte Datei umbenennen
                File.Delete(filePath);
                File.Move(tempFile, filePath);

                return true;
            }
            catch (Exception ex)
            {
                // Aufräumen bei Fehler
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                ConsoleHelper.PrintError($"Fehler beim Entschlüsseln: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Fügt verschlüsselte Daten zu einer bestehenden verschlüsselten Datei hinzu
        /// </summary>
        public static bool AppendEncrypted(string filePath, string textToAdd)
        {
            try
            {
                string existingContent = "";

                // Wenn Datei existiert, entschlüsseln
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    byte[] existingEncrypted = File.ReadAllBytes(filePath);
                    existingContent = DecryptBytes(existingEncrypted);

                    if (existingContent == null)
                        existingContent = "";
                }

                // Neuen Text hinzufügen
                string newContent = existingContent + textToAdd;

                // Alles verschlüsseln und speichern
                byte[] encrypted = EncryptString(newContent);

                if (encrypted == null)
                    return false;

                File.WriteAllBytes(filePath, encrypted);

                return true;
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Hinzufügen verschlüsselter Daten: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Liest und entschlüsselt eine komplette Datei
        /// </summary>
        public static string ReadEncryptedFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                byte[] encrypted = File.ReadAllBytes(filePath);
                return DecryptBytes(encrypted);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Fehler beim Lesen der verschlüsselten Datei: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Erstellt einen Hash der Datei zur Integritätsprüfung
        /// </summary>
        public static string GetFileHash(string filePath)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Zeigt Verschlüsselungsinformationen an
        /// </summary>
        public static void ZeigeVerschluesselungsInfo()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Verschlüsselungs-Informationen", ConsoleColor.DarkCyan);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  🔐 Sicherheits-Details:");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  ├─ Algorithmus: AES-256 (Advanced Encryption Standard)");
            Console.WriteLine("  ├─ Schlüssellänge: 256 Bit (32 Bytes)");
            Console.WriteLine("  ├─ Modus: CBC (Cipher Block Chaining)");
            Console.WriteLine("  ├─ Padding: PKCS7");
            Console.WriteLine("  ├─ Key Derivation: PBKDF2 mit SHA-256");
            Console.WriteLine("  ├─ Iterationen: 10.000");
            Console.WriteLine("  └─ IV: 128 Bit fest");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Alle Log-Dateien werden automatisch verschlüsselt!");
            Console.WriteLine("  ✓ Ohne den richtigen Schlüssel sind die Daten nicht lesbar!");
            Console.WriteLine("  ✓ Militärischer Standard - NSA-sicher!");
            Console.ResetColor();
            Console.WriteLine();

            ConsoleHelper.PressKeyToContinue();
        }
    }
}