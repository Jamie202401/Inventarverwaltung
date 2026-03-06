using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.Data;
using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet automatische Updates und Rollbacks über GitHub Releases.
    /// Repository : https://github.com/Jamie202401/Inventarverwaltung
    /// Nur Admins dürfen Update/Rollback ausführen.
    /// </summary>
    public static class UpdateManager
    {
        // ── Konfiguration ──────────────────────────────────────────────────
        private const string _owner = "Jamie202401";
        private const string _repo = "Inventarverwaltung";
        private const string _aktVersion = "v1.2.0";
        private const string _apiBase = "https://api.github.com";
        private const string _backupDir = "Backups";

        // ── Farben ─────────────────────────────────────────────────────────
        private static void _f(ConsoleColor c) => Console.ForegroundColor = c;
        private static void _r() => Console.ResetColor();

        // ══════════════════════════════════════════════════════════════════
        // ÖFFENTLICHE EINSTIEGSPUNKTE
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Prüft beim Programmstart still ob ein Update verfügbar ist.
        /// Zeigt nur einen kurzen Hinweis — kein Blocking.
        /// Nur für Admins sichtbar.
        /// </summary>
        public static void StartupCheckStill()
        {
            var akt = DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(AuthManager.AktuellerBenutzer,
                    StringComparison.OrdinalIgnoreCase));
            if (akt?.Berechtigung != Berechtigungen.Admin) return;

            try
            {
                var releases = _holeReleases();
                if (releases == null || releases.Count == 0) return;

                if (_istNeuer(releases[0].TagName, _aktVersion))
                {
                    Console.WriteLine();
                    _f(ConsoleColor.Yellow);
                    Console.WriteLine($"  💡 Update verfügbar: {releases[0].TagName}  " +
                                      $"(aktuell: {_aktVersion})  →  Menü [8] System");
                    _r();
                    Thread.Sleep(1800);
                }
            }
            catch { /* Kein Internet → still ignorieren */ }
        }

        /// <summary>
        /// Vollständiges Update / Rollback-Menü — nur für Admins.
        /// </summary>
        public static void ZeigeUpdateMenue()
        {
            if (!_istAdmin())
            {
                ConsoleHelper.PrintError("⛔  Nur Administratoren können Updates durchführen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            while (true)
            {
                Console.Clear();
                _kopf();

                _f(ConsoleColor.White);
                Console.WriteLine("  [1]  Auf Updates prüfen");
                Console.WriteLine("  [2]  Rollback — ältere Version wiederherstellen");
                Console.WriteLine("  [3]  Versionsverlauf anzeigen");
                Console.WriteLine("  [0]  Zurück");
                _r();
                Console.WriteLine();
                Console.Write("  Auswahl: ");

                switch (Console.ReadLine()?.Trim())
                {
                    case "1": _pruefeUpdate(); break;
                    case "2": _rollbackMenue(); break;
                    case "3": _zeigeVerlauf(); break;
                    case "0": return;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // UPDATE PRÜFEN & INSTALLIEREN
        // ══════════════════════════════════════════════════════════════════

        private static void _pruefeUpdate()
        {
            Console.Clear(); _kopf();
            _f(ConsoleColor.DarkGray);
            Console.Write("  GitHub wird abgefragt...");
            _r();

            List<GitHubRelease> releases;
            try { releases = _holeReleases(); }
            catch { _fehler("Keine Verbindung zu GitHub möglich."); return; }

            if (releases == null || releases.Count == 0)
            { _fehler("Keine Releases auf GitHub gefunden."); return; }

            var neueste = releases[0];
            Console.WriteLine();
            Console.WriteLine();
            _f(ConsoleColor.Cyan);
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine($"  │  Installiert  :  {_aktVersion,-38}│");
            Console.WriteLine($"  │  Aktuellste   :  {neueste.TagName,-38}│");
            Console.WriteLine($"  │  Veröffentlicht:  {neueste.PublishedAt:dd.MM.yyyy  HH:mm}                       │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────┘");
            _r();
            Console.WriteLine();

            if (!_istNeuer(neueste.TagName, _aktVersion))
            {
                _f(ConsoleColor.Green);
                Console.WriteLine("  ✓ Programm ist bereits auf dem neuesten Stand.");
                _r();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Release Notes
            if (!string.IsNullOrWhiteSpace(neueste.Body))
            {
                _f(ConsoleColor.DarkGray);
                Console.WriteLine("  Änderungen in dieser Version:");
                foreach (var z in neueste.Body.Split('\n').Take(8))
                {
                    string t = z.Trim();
                    if (!string.IsNullOrWhiteSpace(t))
                        Console.WriteLine($"    {t}");
                }
                _r();
                Console.WriteLine();
            }

            _f(ConsoleColor.Yellow);
            Console.Write($"  Update auf {neueste.TagName} jetzt installieren? [J/N]: ");
            _r();
            if ((Console.ReadLine()?.Trim().ToUpper() ?? "N") != "J") return;

            var asset = neueste.Assets?.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            { _fehler("Kein .exe-Asset im Release gefunden."); return; }

            LogManager.LogWarnung("UPDATE",
                $"Update gestartet | Von: {_aktVersion} → Ziel: {neueste.TagName} | " +
                $"Admin: {AuthManager.AktuellerBenutzer}");

            _downloadUndStarte(asset.BrowserDownloadUrl, neueste.TagName);
        }

        // ══════════════════════════════════════════════════════════════════
        // ROLLBACK
        // ══════════════════════════════════════════════════════════════════

        private static void _rollbackMenue()
        {
            Console.Clear(); _kopf();
            _f(ConsoleColor.DarkGray);
            Console.Write("  GitHub Releases werden geladen...");
            _r();

            List<GitHubRelease> releases;
            try { releases = _holeReleases(); }
            catch { _fehler("Keine Verbindung zu GitHub möglich."); return; }

            if (releases == null || releases.Count == 0)
            { _fehler("Keine Releases auf GitHub gefunden."); return; }

            Console.WriteLine();
            Console.WriteLine();
            _f(ConsoleColor.Cyan);
            Console.WriteLine($"  {"Nr",-4}  {"Version",-12}  {"Datum",-18}  Status");
            Console.WriteLine($"  {"────",-4}  {"────────────",-12}  {"──────────────────",-18}  ──────────────");
            _r();

            for (int i = 0; i < releases.Count; i++)
            {
                var rel = releases[i];
                bool istAktuell = rel.TagName == _aktVersion;
                _f(istAktuell ? ConsoleColor.Green : ConsoleColor.White);
                Console.WriteLine($"  [{i + 1,2}]  {rel.TagName,-12}  " +
                                  $"{rel.PublishedAt:dd.MM.yyyy  HH:mm}  " +
                                  $"{(istAktuell ? "◀ INSTALLIERT" : "")}");
                _r();
            }

            Console.WriteLine();
            _f(ConsoleColor.Yellow);
            Console.Write("  Nummer wählen (0 = Abbrechen): ");
            _r();

            string inp = Console.ReadLine()?.Trim() ?? "0";
            if (!int.TryParse(inp, out int wahl) || wahl < 1 || wahl > releases.Count) return;

            var ziel = releases[wahl - 1];

            if (ziel.TagName == _aktVersion)
            {
                _f(ConsoleColor.Yellow);
                Console.WriteLine("\n  ⚠ Das ist bereits die installierte Version.");
                _r();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            var asset = ziel.Assets?.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            { _fehler($"Kein .exe-Asset für {ziel.TagName} gefunden."); return; }

            Console.WriteLine();
            _f(ConsoleColor.Red);
            Console.Write($"  ⚠  Rollback auf {ziel.TagName} durchführen? [J/N]: ");
            _r();
            if ((Console.ReadLine()?.Trim().ToUpper() ?? "N") != "J") return;

            LogManager.LogWarnung("UPDATE",
                $"Rollback gestartet | Von: {_aktVersion} → Ziel: {ziel.TagName} | " +
                $"Admin: {AuthManager.AktuellerBenutzer}");

            _downloadUndStarte(asset.BrowserDownloadUrl, ziel.TagName);
        }

        // ══════════════════════════════════════════════════════════════════
        // DOWNLOAD & NEUSTART
        // ══════════════════════════════════════════════════════════════════

        private static void _downloadUndStarte(string url, string version)
        {
            Console.WriteLine();
            _f(ConsoleColor.DarkGray);

            string aktPfad = Environment.ProcessPath
                              ?? Environment.GetCommandLineArgs()[0];
            string tempPfad = Path.Combine(Path.GetTempPath(),
                $"Inventarverwaltung_{version}.exe");

            try
            {
                // Fortschrittsanzeige während Download
                Console.Write($"  ⬇  Lade {version} herunter  ");
                using var client = _neuerClient();

                var downloadTask = client.GetByteArrayAsync(url);
                while (!downloadTask.IsCompleted)
                {
                    Console.Write("█");
                    Thread.Sleep(300);
                }
                byte[] bytes = downloadTask.GetAwaiter().GetResult();
                File.WriteAllBytes(tempPfad, bytes);

                _r();
                _f(ConsoleColor.Green);
                Console.WriteLine($"\n  ✓ Download abgeschlossen ({bytes.Length / 1024:N0} KB)");
                _r();

                // Aktuelle Version als Backup sichern
                _sichereBackup(aktPfad, _aktVersion);

                // Batch-Datei die nach Programmende die neue .exe einspielt
                string batPfad = Path.Combine(Path.GetTempPath(), "inv_update.bat");
                File.WriteAllText(batPfad,
                    "@echo off\r\n" +
                    "timeout /t 2 /nobreak >nul\r\n" +
                    $"copy /Y \"{tempPfad}\" \"{aktPfad}\"\r\n" +
                    $"start \"\" \"{aktPfad}\"\r\n" +
                    "del \"%~f0\"\r\n");

                _f(ConsoleColor.Green);
                Console.WriteLine("  ✓ Programm wird neu gestartet...");
                _r();
                Thread.Sleep(1500);

                Process.Start(new ProcessStartInfo
                {
                    FileName = batPfad,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _r();
                _fehler($"Fehler: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // VERSIONSVERLAUF
        // ══════════════════════════════════════════════════════════════════

        private static void _zeigeVerlauf()
        {
            Console.Clear(); _kopf();
            _f(ConsoleColor.DarkGray);
            Console.Write("  Lade Versionsverlauf von GitHub...");
            _r();

            List<GitHubRelease> releases;
            try { releases = _holeReleases(); }
            catch { _fehler("Keine Verbindung zu GitHub möglich."); return; }

            Console.WriteLine();
            Console.WriteLine();

            foreach (var rel in releases)
            {
                bool istAktuell = rel.TagName == _aktVersion;
                _f(istAktuell ? ConsoleColor.Green : ConsoleColor.Cyan);
                Console.Write($"  {rel.TagName,-12}  {rel.PublishedAt:dd.MM.yyyy  HH:mm}");
                if (istAktuell) Console.Write("  ◀ INSTALLIERT");
                Console.WriteLine();
                _r();

                if (!string.IsNullOrWhiteSpace(rel.Body))
                {
                    _f(ConsoleColor.DarkGray);
                    foreach (var z in rel.Body.Split('\n').Take(4))
                    {
                        string t = z.Trim();
                        if (!string.IsNullOrWhiteSpace(t))
                            Console.WriteLine($"      {t}");
                    }
                    _r();
                }
                Console.WriteLine();
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════════
        // BACKUP
        // ══════════════════════════════════════════════════════════════════

        private static void _sichereBackup(string quellPfad, string version)
        {
            try
            {
                Directory.CreateDirectory(_backupDir);
                string ziel = Path.Combine(_backupDir,
                    $"Inventarverwaltung_{version}_{DateTime.Now:yyyyMMdd_HHmm}.exe");
                File.Copy(quellPfad, ziel, overwrite: true);
                _f(ConsoleColor.DarkGray);
                Console.WriteLine($"  💾 Backup gesichert: {Path.GetFileName(ziel)}");
                _r();
            }
            catch (Exception ex)
            {
                _f(ConsoleColor.Yellow);
                Console.WriteLine($"  ⚠ Backup fehlgeschlagen: {ex.Message}");
                _r();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // GITHUB API
        // ══════════════════════════════════════════════════════════════════

        private static List<GitHubRelease> _holeReleases()
        {
            string url = $"{_apiBase}/repos/{_owner}/{_repo}/releases";
            using var client = _neuerClient();
            string json = client.GetStringAsync(url).GetAwaiter().GetResult();

            var liste = new List<GitHubRelease>();
            using var doc = JsonDocument.Parse(json);

            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var rel = new GitHubRelease
                {
                    TagName = el.GetProperty("tag_name").GetString() ?? "",
                    Name = el.TryGetProperty("name", out var n) ? n.GetString() : "",
                    Body = el.TryGetProperty("body", out var b) ? b.GetString() : "",
                    PublishedAt = el.TryGetProperty("published_at", out var p)
                                  && DateTime.TryParse(p.GetString(), out var dt)
                                  ? dt : DateTime.MinValue,
                    Assets = new List<GitHubAsset>()
                };

                if (el.TryGetProperty("assets", out var assets))
                {
                    foreach (var a in assets.EnumerateArray())
                    {
                        rel.Assets.Add(new GitHubAsset
                        {
                            Name = a.GetProperty("name").GetString() ?? "",
                            BrowserDownloadUrl = a.GetProperty("browser_download_url").GetString() ?? ""
                        });
                    }
                }
                liste.Add(rel);
            }
            return liste;
        }

        private static HttpClient _neuerClient()
        {
            var c = new HttpClient();
            c.DefaultRequestHeaders.Add("User-Agent", $"{_repo}-Client/{_aktVersion}");
            c.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            c.Timeout = TimeSpan.FromSeconds(15);
            return c;
        }

        // ══════════════════════════════════════════════════════════════════
        // HELPER
        // ══════════════════════════════════════════════════════════════════

        private static bool _istAdmin()
        {
            var u = DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(AuthManager.AktuellerBenutzer,
                    StringComparison.OrdinalIgnoreCase));
            return u?.Berechtigung == Berechtigungen.Admin;
        }

        /// <summary>Vergleicht Semver-Tags — v1.2.1 > v1.2.0 → true</summary>
        private static bool _istNeuer(string neu, string alt)
        {
            static Version parse(string s)
            {
                s = s.TrimStart('v', 'V');
                return Version.TryParse(s, out var v) ? v : new Version(0, 0, 0);
            }
            return parse(neu) > parse(alt);
        }

        private static void _kopf()
        {
            _f(ConsoleColor.Cyan);
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║   🔄  UPDATE & ROLLBACK — Inventarverwaltung            ║");
            Console.WriteLine($"  ║   Version  :  {_aktVersion,-43}║");
            Console.WriteLine($"  ║   Quelle   :  github.com/{_owner}/{_repo,-20}║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════╝");
            _r();
            Console.WriteLine();
        }

        private static void _fehler(string msg)
        {
            Console.WriteLine();
            _f(ConsoleColor.Red);
            Console.WriteLine($"  ✗  {msg}");
            _r();
            ConsoleHelper.PressKeyToContinue();
        }

        // ── Datenmodelle ───────────────────────────────────────────────────
        private class GitHubRelease
        {
            public string TagName { get; set; }
            public string Name { get; set; }
            public string Body { get; set; }
            public DateTime PublishedAt { get; set; }
            public List<GitHubAsset> Assets { get; set; }
        }

        private class GitHubAsset
        {
            public string Name { get; set; }
            public string BrowserDownloadUrl { get; set; }
        }
    }
}