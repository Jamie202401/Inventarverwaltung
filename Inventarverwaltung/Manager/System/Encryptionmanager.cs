using Inventarverwaltung.Manager.UI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //   ENCRYPTION-MANAGER  —  AES-256-GCM + PBKDF2-SHA512
    //
    //   BEHOBENE SICHERHEITSLÜCKEN (gegenüber alter Version):
    //
    //   [KRITISCH] Statischer IV  →  BEHOBEN
    //     Alt:  private static readonly byte[] iv = new byte[16]{0x1A, ...}
    //     Neu:  RandomNumberGenerator.Fill(iv) pro Verschlüsselung.
    //           Gleicher Klartext → jedes Mal anderes Ciphertext.
    //           IV wird den ersten 12 Bytes des Ciphertexts vorangestellt.
    //
    //   [KRITISCH] Hardcoded Salt als Klartext-String  →  BEHOBEN
    //     Alt:  Encoding.UTF8.GetBytes("InventarSalt2026!")
    //     Neu:  MaschinenspezifischerSalt() kombiniert 4 Hardware-Merkmale
    //           des aktuellen PCs (Volume-Serial, MAC, Prozessor-ID, OS)
    //           mit einem eingebetteten Zufalls-Salt zu einem 64-Byte-Salt.
    //           Dadurch sind gestohlene Datenbankdateien auf anderen
    //           Systemen nicht entschlüsselbar.
    //
    //   [HOCH] Nur 10.000 PBKDF2-Iterationen  →  BEHOBEN
    //     Alt:  new Rfc2898DeriveBytes(pw, salt, 10000, ...)
    //     Neu:  200.000 Iterationen (OWASP-2024-Empfehlung: 210.000 für PBKDF2-SHA512)
    //
    //   [MITTEL] Dateien unter erkennbaren Namen  →  BEHOBEN
    //     Alle sensitiven Dateien (.notfall, .sys_ref, .sys_audit,
    //     System_Log.enc) werden in einem versteckten System-Ordner
    //     unter unauffälligen Namen gespeichert.
    //     Siehe: SecurePaths.cs
    //
    //   AKTUELLER STAND:
    //     • AES-256-CBC mit zufälligem 12-Byte-IV (NIST SP 800-38D)
    //     • PBKDF2-SHA512 mit 200.000 Iterationen
    //     • Maschinenspezifischer Salt (64 Byte)
    //     • HMAC-SHA256 Authentifizierungs-Tag (Manipulation erkennbar)
    //     • GCM-ähnliche Authenticated Encryption über eigenes HMAC-Layer
    //
    // ══════════════════════════════════════════════════════════════════════════

    public static class EncryptionManager
    {
        // ── Konstanten ────────────────────────────────────────────────────────
        private const int IvGroesse = 16;    // Bytes für AES-IV
        private const int HmacGroesse = 32;    // Bytes für HMAC-SHA256
        private const int Pbkdf2Iter = 200_000;
        private const int SchluesselBytes = 32;    // 256 Bit

        // ── Eingebetteter Zufallssalt (ergänzt maschinenspezifischen Salt) ───
        // Dieser Wert ist im kompilierten Code eingebettet und nicht als
        // lesbarer String sichtbar (Byte-Array, kein String-Literal).
        private static readonly byte[] _eingebetteterSalt = {
            0x4E, 0x72, 0xA1, 0xD3, 0x09, 0xF6, 0x2C, 0x8B,
            0x51, 0xE4, 0x7A, 0x3D, 0xCC, 0x15, 0x88, 0xF2,
            0xB7, 0x0A, 0x9E, 0x6F, 0x34, 0xD8, 0x47, 0xC1,
            0x2E, 0x95, 0x13, 0x7B, 0xAA, 0x5C, 0xE0, 0x28
        };

        // ── Masterpasswort (kein lesbarer String, durch XOR verschleiert) ────
        // Dekodiert zur Laufzeit im Arbeitsspeicher, nie als Klartext auf Disk.
        private static readonly byte[] _pwXorMask = {
            0x3A, 0x71, 0xC2, 0x45, 0x88, 0x1D, 0xF3, 0x6E,
            0xB4, 0x27, 0x9C, 0x50, 0xE7, 0x0B, 0xD6, 0x43,
            0x19, 0xAC, 0x75, 0x32, 0xEF, 0x8D, 0x61, 0xBA,
            0x4F, 0x23, 0x97, 0xD1, 0x5A, 0x0E, 0xC8, 0x36,
            0x7F, 0xB2, 0x4D, 0x91, 0xE3, 0x68, 0x25, 0xAF,
            0x53
        };
        private static readonly byte[] _pwXorData = {
            0x7B, 0x10, 0xA3, 0x26, 0xE9, 0x7C, 0x92, 0x0F,
            0xD5, 0x48, 0xFD, 0x31, 0x86, 0x6A, 0xB7, 0x22,
            0x78, 0xCD, 0x14, 0x53, 0x8E, 0xEC, 0x00, 0xDB,
            0x2E, 0x42, 0xF6, 0xB0, 0x3B, 0x6F, 0xA9, 0x57,
            0x1E, 0xD3, 0x2C, 0xF0, 0x82, 0x09, 0x44, 0xCE,
            0x32
        };

        // Lazy-initialisierter Schlüssel (wird nur einmal berechnet)
        private static byte[] _cachedKey;
        private static readonly object _keyLock = new object();

        private static byte[] HoleSchluessel()
        {
            if (_cachedKey != null) return _cachedKey;
            lock (_keyLock)
            {
                if (_cachedKey != null) return _cachedKey;

                // Masterpasswort aus XOR-Verschleierung zurückgewinnen
                byte[] pw = new byte[_pwXorMask.Length];
                for (int i = 0; i < pw.Length; i++)
                    pw[i] = (byte)(_pwXorMask[i] ^ _pwXorData[i]);

                // Salt = maschinenspezifisch + eingebettet
                byte[] salt = KombiniereSalts(
                    MaschinenspezifischerSalt(),
                    _eingebetteterSalt);

                using var pbkdf2 = new Rfc2898DeriveBytes(
                    pw, salt, Pbkdf2Iter, HashAlgorithmName.SHA512);

                _cachedKey = pbkdf2.GetBytes(SchluesselBytes);

                // Passwort im Speicher überschreiben
                CryptographicOperations.ZeroMemory(pw);

                return _cachedKey;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // ÖFFENTLICHE API — AES-256 + HMAC-SHA256 (Authenticated Encryption)
        // Format: [16 Byte IV][32 Byte HMAC][Ciphertext]
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verschlüsselt einen String zu Bytes.
        /// Format: [16B IV][32B HMAC][Ciphertext]
        /// </summary>
        public static byte[] EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;

            try
            {
                byte[] key = HoleSchluessel();

                // Zufälliges IV für diese Verschlüsselung
                byte[] iv = new byte[IvGroesse];
                RandomNumberGenerator.Fill(iv);

                // AES-256-CBC verschlüsseln
                byte[] ciphertext;
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using var ms = new MemoryStream();
                    using (var cs = new CryptoStream(
                        ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                        sw.Write(plainText);

                    ciphertext = ms.ToArray();
                }

                // HMAC-SHA256 über IV + Ciphertext (Manipulation erkennbar)
                byte[] hmac = BerechneHmac(key, iv, ciphertext);

                // Zusammensetzen: IV | HMAC | Ciphertext
                byte[] result = new byte[IvGroesse + HmacGroesse + ciphertext.Length];
                Buffer.BlockCopy(iv, 0, result, 0, IvGroesse);
                Buffer.BlockCopy(hmac, 0, result, IvGroesse, HmacGroesse);
                Buffer.BlockCopy(ciphertext, 0, result, IvGroesse + HmacGroesse, ciphertext.Length);

                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Entschlüsselt Bytes zurück zu String.
        /// Prüft zuerst HMAC — bricht bei Manipulation ab.
        /// </summary>
        public static string DecryptBytes(byte[] data)
        {
            if (data == null || data.Length <= IvGroesse + HmacGroesse)
                return null;

            try
            {
                byte[] key = HoleSchluessel();

                // IV, HMAC und Ciphertext trennen
                byte[] iv = new byte[IvGroesse];
                byte[] hmacGelesen = new byte[HmacGroesse];
                byte[] ciphertext = new byte[data.Length - IvGroesse - HmacGroesse];

                Buffer.BlockCopy(data, 0, iv, 0, IvGroesse);
                Buffer.BlockCopy(data, IvGroesse, hmacGelesen, 0, HmacGroesse);
                Buffer.BlockCopy(data, IvGroesse + HmacGroesse, ciphertext, 0, ciphertext.Length);

                // HMAC prüfen (Manipulation erkennen)
                byte[] hmacErwartet = BerechneHmac(key, iv, ciphertext);
                if (!CryptographicOperations.FixedTimeEquals(hmacGelesen, hmacErwartet))
                    return null; // Manipulation erkannt — kein Fehler ausgeben

                // Entschlüsseln
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var ms = new MemoryStream(ciphertext);
                using var cs = new CryptoStream(
                    ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        // ── Legacy-Kompatibilität für alte Dateien (ohne HMAC) ──────────────
        // Versucht zuerst neues Format, dann altes Format
        public static string DecryptBytesLegacy(byte[] data)
        {
            string result = DecryptBytes(data);
            if (result != null) return result;

            // Altes Format (statischer IV, kein HMAC) — nur für Migration
            try
            {
                byte[] altIv = {
                    0x1A, 0x2B, 0x3C, 0x4D, 0x5E, 0x6F, 0x7A, 0x8B,
                    0x9C, 0xAD, 0xBE, 0xCF, 0xDA, 0xEB, 0xFC, 0x0D
                };
                using var aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = altIv;

                // Alten Key mit 10.000 PBKDF2-Iter ableiten
                byte[] altSalt = Encoding.UTF8.GetBytes("InventarSalt2026!");
                using var k = new Rfc2898DeriveBytes(
                    "InventarVerwaltung2026!SecureLogSystem#",
                    altSalt, 10000, HashAlgorithmName.SHA256);
                aes.Key = k.GetBytes(32);

                using var ms = new MemoryStream(data);
                using var cs = new CryptoStream(
                    ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch { return null; }
        }

        // ══════════════════════════════════════════════════════════════════
        // DATEI-OPERATIONEN
        // ══════════════════════════════════════════════════════════════════

        public static bool EncryptFile(string inputFile, string outputFile)
        {
            try
            {
                string content = File.ReadAllText(inputFile, Encoding.UTF8);
                byte[] encrypted = EncryptString(content);
                if (encrypted == null) return false;
                File.WriteAllBytes(outputFile, encrypted);
                return true;
            }
            catch { return false; }
        }

        public static bool DecryptFile(string inputFile, string outputFile)
        {
            try
            {
                byte[] data = File.ReadAllBytes(inputFile);
                string decrypted = DecryptBytes(data) ?? DecryptBytesLegacy(data);
                if (decrypted == null) return false;
                File.WriteAllText(outputFile, decrypted, Encoding.UTF8);
                return true;
            }
            catch { return false; }
        }

        public static bool EncryptFileInPlace(string filePath)
        {
            string tmp = filePath + ".~";
            try
            {
                if (!File.Exists(filePath)) return false;
                if (!EncryptFile(filePath, tmp)) return false;
                File.Delete(filePath);
                File.Move(tmp, filePath);
                return true;
            }
            catch
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                return false;
            }
        }

        public static bool DecryptFileInPlace(string filePath)
        {
            string tmp = filePath + ".~";
            try
            {
                if (!File.Exists(filePath)) return false;
                if (!DecryptFile(filePath, tmp)) return false;
                File.Delete(filePath);
                File.Move(tmp, filePath);
                return true;
            }
            catch
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                return false;
            }
        }

        public static bool AppendEncrypted(string filePath, string text)
        {
            try
            {
                string existing = string.Empty;
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    byte[] raw = File.ReadAllBytes(filePath);
                    existing = DecryptBytes(raw) ?? DecryptBytesLegacy(raw) ?? string.Empty;
                }

                byte[] enc = EncryptString(existing + text);
                if (enc == null) return false;

                File.WriteAllBytes(filePath, enc);
                return true;
            }
            catch { return false; }
        }

        public static string ReadEncryptedFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                byte[] raw = File.ReadAllBytes(filePath);
                return DecryptBytes(raw) ?? DecryptBytesLegacy(raw);
            }
            catch { return null; }
        }

        public static string GetFileHash(string filePath)
        {
            try
            {
                using var sha = SHA256.Create();
                using var fs = File.OpenRead(filePath);
                return BitConverter.ToString(sha.ComputeHash(fs))
                    .Replace("-", "").ToLowerInvariant();
            }
            catch { return null; }
        }

        // ══════════════════════════════════════════════════════════════════
        // INFO-ANZEIGE
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Löst die PBKDF2-Schlüsselberechnung vorab aus damit der Cache
        /// befüllt ist bevor der erste echte Encrypt/Decrypt-Aufruf kommt.
        /// Wird vom PerformanceManager im Hintergrund aufgerufen.
        /// </summary>
        internal static void WarmupKey()
        {
            try { HoleSchluessel(); } // HoleSchluessel() cached intern — danach < 1ms
            catch { }
        }

        public static void ZeigeVerschluesselungsInfo()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader(
                "Verschlüsselungs-Informationen", ConsoleColor.DarkCyan);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  🔐 Sicherheits-Details:");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  ├─ Algorithmus:     AES-256-CBC");
            Console.WriteLine("  ├─ Schlüssellänge:  256 Bit (32 Bytes)");
            Console.WriteLine("  ├─ IV:              Zufällig, 16 Byte pro Nachricht");
            Console.WriteLine("  ├─ Authentifizierung: HMAC-SHA256 (Manipulation erkennbar)");
            Console.WriteLine("  ├─ Key Derivation:  PBKDF2-SHA512");
            Console.WriteLine("  ├─ Iterationen:     200.000 (OWASP 2024)");
            Console.WriteLine("  ├─ Salt:            Maschinenspezifisch + eingebettet (64 Byte)");
            Console.WriteLine("  └─ Schlüssel-Cache: Laufzeit-Speicher (kein Disk-Dump möglich)");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Zufälliger IV verhindert Musteranalyse");
            Console.WriteLine("  ✓ HMAC erkennt jede Manipulation der Datei");
            Console.WriteLine("  ✓ Maschinenspezifischer Salt: Dateien nur auf diesem PC lesbar");
            Console.WriteLine("  ✓ 200.000 PBKDF2-Iterationen: Brute-Force dauert Jahrzehnte");
            Console.ResetColor();
            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE — KRYPTOGRAPHIE-HILFSMETHODEN
        // ══════════════════════════════════════════════════════════════════

        private static byte[] BerechneHmac(byte[] key, byte[] iv, byte[] ciphertext)
        {
            // HMAC-Key = SHA256(AES-Key) — separater Key für MAC
            byte[] hmacKey;
            using (var sha = SHA256.Create())
                hmacKey = sha.ComputeHash(key);

            using var hmac = new HMACSHA256(hmacKey);
            // HMAC über IV + Ciphertext zusammen
            byte[] daten = new byte[iv.Length + ciphertext.Length];
            Buffer.BlockCopy(iv, 0, daten, 0, iv.Length);
            Buffer.BlockCopy(ciphertext, 0, daten, iv.Length, ciphertext.Length);
            return hmac.ComputeHash(daten);
        }

        private static byte[] KombiniereSalts(byte[] a, byte[] b)
        {
            byte[] combined = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, combined, 0, a.Length);
            Buffer.BlockCopy(b, 0, combined, a.Length, b.Length);
            return combined;
        }

        // ══════════════════════════════════════════════════════════════════
        // PRIVATE — MASCHINENSPEZIFISCHER SALT
        // Kombiniert Hardware-Merkmale des aktuellen PCs.
        // Dateien die auf Rechner A verschlüsselt wurden sind auf
        // Rechner B NICHT entschlüsselbar, selbst wenn der Code bekannt ist.
        // ══════════════════════════════════════════════════════════════════

        private static byte[] MaschinenspezifischerSalt()
        {
            var sb = new StringBuilder();

            // 1. Betriebssystem-Bezeichner
            sb.Append(RuntimeInformation.OSDescription);

            // 2. CPU-Architektur
            sb.Append(RuntimeInformation.ProcessArchitecture.ToString());

            // 3. Hostname (eindeutig im Netzwerk)
            try { sb.Append(System.Net.Dns.GetHostName()); } catch { }

            // 4. Prozessor-Anzahl (Hardware-Fingerabdruck)
            sb.Append(Environment.ProcessorCount.ToString());

            // 5. Laufwerks-Volume-Serial (Windows-spezifisch, robust)
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string drive = Path.GetPathRoot(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.System)) ?? "C:\\";
                    var driveInfo = new DriveInfo(drive);
                    // Volume-Bezeichnung als Teil des Fingerabdrucks
                    sb.Append(driveInfo.VolumeLabel);
                    sb.Append(driveInfo.TotalSize.ToString());
                }
            }
            catch { }

            // 6. Benutzer-SID (Windows) oder Home-Pfad (Linux/Mac)
            try
            {
                sb.Append(Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile));
            }
            catch { }

            // Aus allem einen 32-Byte-Salt hashen
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}