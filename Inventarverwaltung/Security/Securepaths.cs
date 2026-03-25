using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung
{
    // ══════════════════════════════════════════════════════════════════════════
    //  SECURE-PATHS  —  Punkt 7: Versteckte Speicherorte für ALLE Dateien
    //
    //  Alle sensitiven Dateien (Log, Notfall-PW, Inventar, Mitarbeiter,
    //  Benutzer, Assembly-Hash, Audit-Log) liegen in:
    //    Windows: %LOCALAPPDATA%\Microsoft\Windows\Themes\<hash8>\
    //    macOS:   ~/Library/Caches/<hash8>/
    //    Linux:   ~/.cache/<hash8>/
    //
    //  Dateinamen sind echte Windows-Systemdateinamen (XOR-verschleiert
    //  im Code — kein Hex-Editor kann sie als Klartext lesen).
    //
    //  Dateiname-Mapping (nur zur Dokumentation, nie im Code lesbar):
    //    thumbcache_96.db   → System-Log
    //    IconCache.db       → Notfall-Passwort
    //    SettingSyncCache.db→ Assembly-Hash
    //    acmru.dat          → Scanner-Audit-Log
    //    thumbcache_32.db   → Regelbank-Hash
    //    msctf.dat          → Inventar-Datenbank
    //    UserDataCache.db   → Mitarbeiter-Datenbank
    //    indexedDB.db       → Benutzer-Datenbank
    //    RecentActivity.db  → Export-Log
    // ══════════════════════════════════════════════════════════════════════════

    public static class SecurePaths
    {
        // ══════════════════════════════════════════════════════════════════════
        //  XOR-VERSCHLEIERTE DATEINAMEN — Sicherheitssystem-Dateien
        // ══════════════════════════════════════════════════════════════════════

        private static readonly byte[] _dateiLog_M = { 0x00, 0x6B, 0x91, 0xE9, 0x6F, 0xB2, 0xC5, 0xD8, 0xA9, 0x07, 0x97, 0x07, 0x09, 0xF7, 0x6D, 0x96 };
        private static readonly byte[] _dateiLog_D = { 0x74, 0x03, 0xE4, 0x84, 0x0D, 0xD1, 0xA4, 0xBB, 0xC1, 0x62, 0xC8, 0x3E, 0x3F, 0xD9, 0x09, 0xF4 };

        private static readonly byte[] _dateiNotfall_M = { 0x6E, 0xF7, 0xF9, 0xD9, 0xB1, 0x4C, 0xF9, 0x33, 0xC9, 0xF4, 0xC6, 0x13 };
        private static readonly byte[] _dateiNotfall_D = { 0x27, 0x94, 0x96, 0xB7, 0xF2, 0x2D, 0x9A, 0x5B, 0xAC, 0xDA, 0xA2, 0x71 };

        private static readonly byte[] _dateiAssemblyHash_M = { 0x72, 0x11, 0x18, 0x18, 0xA8, 0x36, 0xFF, 0xDC, 0x10, 0x56, 0xA4, 0x39, 0x68, 0x92, 0x32, 0x66, 0x65, 0x5D, 0xDA };
        private static readonly byte[] _dateiAssemblyHash_D = { 0x21, 0x74, 0x6C, 0x6C, 0xC1, 0x58, 0x98, 0x8F, 0x69, 0x38, 0xC7, 0x7A, 0x09, 0xF1, 0x5A, 0x03, 0x4B, 0x39, 0xB8 };

        private static readonly byte[] _dateiAuditLog_M = { 0xB1, 0xD2, 0xB3, 0xCA, 0x82, 0x16, 0x7D, 0x21, 0x2D };
        private static readonly byte[] _dateiAuditLog_D = { 0xD0, 0xB1, 0xDE, 0xB8, 0xF7, 0x38, 0x19, 0x40, 0x59 };

        private static readonly byte[] _dateiRegelbank_M = { 0x9D, 0x7B, 0x4B, 0x33, 0xFE, 0x00, 0xFA, 0x1E, 0xCE, 0x9D, 0xD6, 0x93, 0x48, 0xF3, 0x7D, 0x39 };
        private static readonly byte[] _dateiRegelbank_D = { 0xE9, 0x13, 0x3E, 0x5E, 0x9C, 0x63, 0x9B, 0x7D, 0xA6, 0xF8, 0x89, 0xA0, 0x7A, 0xDD, 0x19, 0x5B };

        // ══════════════════════════════════════════════════════════════════════
        //  XOR-VERSCHLEIERTE DATEINAMEN — Datendateien (Punkte 2, 3)
        // ══════════════════════════════════════════════════════════════════════

        private static readonly byte[] _dateiInventar_M = { 0xD4, 0x64, 0x5E, 0xF3, 0x32, 0x2F, 0x34, 0x95, 0x8B };
        private static readonly byte[] _dateiInventar_D = { 0xB9, 0x17, 0x3D, 0x87, 0x54, 0x01, 0x50, 0xF4, 0xFF };

        private static readonly byte[] _dateiMitarbeiter_M = { 0x7E, 0x35, 0x7A, 0xA3, 0xC4, 0x85, 0x86, 0x74, 0x0C, 0xC6, 0x45, 0xD2, 0x45, 0x82, 0x69, 0x3A };
        private static readonly byte[] _dateiMitarbeiter_D = { 0x2B, 0x46, 0x1F, 0xD1, 0x80, 0xE4, 0xF2, 0x15, 0x4F, 0xA7, 0x26, 0xBA, 0x20, 0xAC, 0x0D, 0x58 };

        private static readonly byte[] _dateiBenutzer_M = { 0xE7, 0x9D, 0x71, 0x9A, 0x80, 0x5C, 0x84, 0x51, 0x97, 0x79, 0xC5, 0xA9 };
        private static readonly byte[] _dateiBenutzer_D = { 0x8E, 0xF3, 0x15, 0xFF, 0xF8, 0x39, 0xE0, 0x15, 0xD5, 0x57, 0xA1, 0xCB };

        private static readonly byte[] _dateiExportLog_M = { 0x95, 0xC5, 0xDE, 0xC5, 0xA3, 0xF8, 0x79, 0x52, 0xC2, 0x9C, 0xA0, 0xF6, 0x90, 0x7E, 0xD2, 0x72, 0x32 };
        private static readonly byte[] _dateiExportLog_D = { 0xC7, 0xA0, 0xBD, 0xA0, 0xCD, 0x8C, 0x38, 0x31, 0xB6, 0xF5, 0xD6, 0x9F, 0xE4, 0x07, 0xFC, 0x16, 0x50 };

        // ══════════════════════════════════════════════════════════════════════
        //  XOR-VERSCHLEIERTE PFADTEILE
        // ══════════════════════════════════════════════════════════════════════

        private static readonly byte[] _pMicrosoft_M = { 0xAF, 0x28, 0x1B, 0xF6, 0x1E, 0x61, 0x69, 0xA3, 0x64 };
        private static readonly byte[] _pMicrosoft_D = { 0xE2, 0x41, 0x78, 0x84, 0x71, 0x12, 0x06, 0xC5, 0x10 };

        private static readonly byte[] _pWindows_M = { 0xB9, 0x94, 0xB9, 0x5C, 0xD1, 0x1E, 0x1A };
        private static readonly byte[] _pWindows_D = { 0xEE, 0xFD, 0xD7, 0x38, 0xBE, 0x69, 0x69 };

        private static readonly byte[] _pThemes_M = { 0x85, 0xE2, 0xAF, 0xCE, 0x2F, 0x0D };
        private static readonly byte[] _pThemes_D = { 0xD1, 0x8A, 0xCA, 0xA3, 0x4A, 0x7E };

        private static readonly byte[] _pLibrary_M = { 0xFC, 0x90, 0x0A, 0xE3, 0x09, 0x12, 0xFE };
        private static readonly byte[] _pLibrary_D = { 0xB0, 0xF9, 0x68, 0x91, 0x68, 0x60, 0x87 };

        private static readonly byte[] _pCaches_M = { 0x6F, 0xFC, 0x4A, 0xD3, 0xC1, 0x6B };
        private static readonly byte[] _pCaches_D = { 0x2C, 0x9D, 0x29, 0xBB, 0xA4, 0x18 };

        private static readonly byte[] _pCache_M = { 0x5F, 0x84, 0xB2, 0xCD, 0xD3, 0x25 };
        private static readonly byte[] _pCache_D = { 0x71, 0xE7, 0xD3, 0xAE, 0xBB, 0x40 };

        // Ordner-Seed für Hash-Berechnung
        private static readonly byte[] _ordnerSeed = {
            0xC4, 0x7E, 0x12, 0xA9, 0x5B, 0xD3, 0x88, 0x2F,
            0x46, 0xE1, 0x73, 0x0C, 0x9A, 0xF5, 0x3D, 0x61
        };

        // ══════════════════════════════════════════════════════════════════════
        //  GECACHTE PFADE
        // ══════════════════════════════════════════════════════════════════════

        private static string _basis;
        private static readonly object _lock = new object();

        private static string _cLog, _cNotfall, _cAssembly, _cAudit,
                              _cRegelbank, _cInventar, _cMitarbeiter,
                              _cBenutzer, _cExportLog;

        // ══════════════════════════════════════════════════════════════════════
        //  ÖFFENTLICHE PROPERTIES — Sicherheitssystem
        // ══════════════════════════════════════════════════════════════════════

        public static string SystemLog
            => _cLog ??= Pfad(Dec(_dateiLog_M, _dateiLog_D));
        public static string NotfallDatei
            => _cNotfall ??= Pfad(Dec(_dateiNotfall_M, _dateiNotfall_D));
        public static string AssemblyHashDatei
            => _cAssembly ??= Pfad(Dec(_dateiAssemblyHash_M, _dateiAssemblyHash_D));
        public static string AuditLogDatei
            => _cAudit ??= Pfad(Dec(_dateiAuditLog_M, _dateiAuditLog_D));
        public static string RegelbankDatei
            => _cRegelbank ??= Pfad(Dec(_dateiRegelbank_M, _dateiRegelbank_D));

        // ══════════════════════════════════════════════════════════════════════
        //  ÖFFENTLICHE PROPERTIES — Datendateien (Punkte 2, 3, 7)
        // ══════════════════════════════════════════════════════════════════════

        public static string InventarDatei
            => _cInventar ??= Pfad(Dec(_dateiInventar_M, _dateiInventar_D));
        public static string MitarbeiterDatei
            => _cMitarbeiter ??= Pfad(Dec(_dateiMitarbeiter_M, _dateiMitarbeiter_D));
        public static string BenutzerDatei
            => _cBenutzer ??= Pfad(Dec(_dateiBenutzer_M, _dateiBenutzer_D));
        public static string ExportLogDatei
            => _cExportLog ??= Pfad(Dec(_dateiExportLog_M, _dateiExportLog_D));

        // ══════════════════════════════════════════════════════════════════════
        //  INITIALISIERUNG
        // ══════════════════════════════════════════════════════════════════════

        public static void Initialisieren()
        {
            try
            {
                string p = Basis();
                if (Directory.Exists(p)) return;
                Directory.CreateDirectory(p);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    File.SetAttributes(p, FileAttributes.Hidden | FileAttributes.System);
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DATEI-SCHUTZ: Hidden + System + ReadOnly + NotContentIndexed
        // ══════════════════════════════════════════════════════════════════════

        public static void DateiSchuetzen(string pfad)
        {
            try
            {
                if (!File.Exists(pfad)) return;
                File.SetAttributes(pfad,
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? FileAttributes.Hidden | FileAttributes.System |
                          FileAttributes.ReadOnly | FileAttributes.NotContentIndexed
                        : FileAttributes.ReadOnly);
            }
            catch { }
        }

        public static void MitSchreibzugriff(string pfad, Action aktion)
        {
            try
            {
                if (File.Exists(pfad))
                    File.SetAttributes(pfad, FileAttributes.Normal);
                aktion();
            }
            finally { DateiSchuetzen(pfad); }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRIVATE
        // ══════════════════════════════════════════════════════════════════════

        private static string Pfad(string dateiname)
        {
            Initialisieren();
            return Path.Combine(Basis(), dateiname);
        }

        private static string Basis()
        {
            if (_basis != null) return _basis;
            lock (_lock)
            {
                if (_basis != null) return _basis;
                _basis = Path.Combine(Elternordner(), OrdnerHash());
            }
            return _basis;
        }

        private static string OrdnerHash()
        {
            var sb = new StringBuilder();
            try { sb.Append(System.Net.Dns.GetHostName()); } catch { }
            try { sb.Append(Environment.UserName); } catch { }

            byte[] inp = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] data = new byte[inp.Length + _ordnerSeed.Length];
            Buffer.BlockCopy(inp, 0, data, 0, inp.Length);
            Buffer.BlockCopy(_ordnerSeed, 0, data, inp.Length, _ordnerSeed.Length);

            using var sha = SHA256.Create();
            byte[] h = sha.ComputeHash(data);
            return BitConverter.ToString(h, 0, 4).Replace("-", "").ToLowerInvariant();
        }

        private static string Elternordner()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string lad = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);
                string ms = Dec(_pMicrosoft_M, _pMicrosoft_D);
                string win = Dec(_pWindows_M, _pWindows_D);
                string them = Dec(_pThemes_M, _pThemes_D);
                string full = Path.Combine(lad, ms, win, them);
                return Directory.Exists(full)
                    ? full
                    : Path.Combine(lad, ms, win);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string home = Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile);
                return Path.Combine(home,
                    Dec(_pLibrary_M, _pLibrary_D),
                    Dec(_pCaches_M, _pCaches_D));
            }
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Dec(_pCache_M, _pCache_D));
        }

        private static string Dec(byte[] m, byte[] d)
        {
            if (m.Length != d.Length)
                throw new InvalidOperationException("XOR-Paar ungültig.");
            byte[] r = new byte[m.Length];
            for (int i = 0; i < m.Length; i++) r[i] = (byte)(m[i] ^ d[i]);
            return Encoding.UTF8.GetString(r);
        }
    }
}