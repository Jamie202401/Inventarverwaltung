using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using Inventarverwaltung;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet den INFOS-Bereich:
    ///   1. Notizen schreiben (mit Benutzer + Zeitstempel)
    ///   2. Bug / Problem melden  → AES-256 verschlüsselt
    ///   3. Neue Vorschläge       → AES-256 verschlüsselt
    ///   4. Versionsinfo
    ///   5. [DEV] Bugs & Vorschläge lesen  → nur Benutzer "jah"
    /// </summary>
    public static class InfosManager
    {
        // ── Konstanten ────────────────────────────────────────────────────────
        private const string VERSION = "2.0.0";
        private const string BUILD_DATUM = "Februar 2026";
        private const string ENTWICKLER = "jh";
        private const string DEV_USER = "jah";          // einziger Benutzer mit Dev-Zugriff

        // ── Dateipfade ────────────────────────────────────────────────────────
        private static string InfoOrdner => "Infos";
        private static string NotizenDatei => Path.Combine(InfoOrdner, "Notizen.txt");
        private static string BugsDatei => Path.Combine(InfoOrdner, "Bugs.enc");       // verschlüsselt
        private static string VorschlaegeD => Path.Combine(InfoOrdner, "Vorschlaege.enc"); // verschlüsselt

        // ═════════════════════════════════════════════════════════════════════
        // HAUPTMENÜ
        // ═════════════════════════════════════════════════════════════════════
        public static void ZeigeInfosMenu()
        {
            EnsureInfoOrdner();
            string benutzer = AuthManager.AktuellerBenutzer ?? "";
            bool istEntwickler = benutzer.Equals(DEV_USER, StringComparison.OrdinalIgnoreCase);

            while (true)
            {
                Console.Clear();
                DruckeHeader();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1]  📝  Notizen schreiben / anzeigen");
                Console.WriteLine("  [2]  🐛  Bug / Problem melden");
                Console.WriteLine("  [3]  💡  Neuen Vorschlag einreichen");
                Console.WriteLine("  [4]  ℹ️   Versionsinfo anzeigen");

                // Dev-Punkt nur anzeigen wenn eingeloggt als "jah"
                if (istEntwickler)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("  [5]  🔐  [DEV] Bugs & Vorschläge einsehen");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine("  [0]  ↩️   Zurück");
                Console.ResetColor();
                Console.WriteLine();

                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");

                switch (auswahl)
                {
                    case "1": NotizenMenu(); break;
                    case "2": BugMelden(); break;
                    case "3": VorschlagMelden(); break;
                    case "4": VersionsinfoAnzeigen(); break;
                    case "5":
                        if (istEntwickler)
                            DevBereichAnzeigen();
                        else
                            ZeigeZugriffVerweigert();
                        break;
                    case "0": return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        Thread.Sleep(600);
                        break;
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // ZUGRIFF VERWEIGERT (für unbefugte die "5" tippen)
        // ═════════════════════════════════════════════════════════════════════
        private static void ZeigeZugriffVerweigert()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ║     🔒  ZUGRIFF VERWEIGERT                                     ║");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ║     Dieser Bereich ist ausschließlich für den                  ║");
            Console.WriteLine("  ║     Entwickler zugänglich.                                     ║");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            LogManager.LogWarnung("Dev-Zugriff", $"Unberechtigter Zugriffsversuch von '{AuthManager.AktuellerBenutzer}'");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═════════════════════════════════════════════════════════════════════
        // [1]  NOTIZEN  ──  Schreiben & Anzeigen
        // ═════════════════════════════════════════════════════════════════════
        private static void NotizenMenu()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("📝 Notizblock", ConsoleColor.Yellow);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1]  ✏️   Neue Notiz schreiben");
                Console.WriteLine("  [2]  📋  Alle Notizen anzeigen");
                Console.WriteLine("  [3]  🗑️   Notiz löschen");
                Console.WriteLine();
                Console.WriteLine("  [0]  ↩️   Zurück");
                Console.ResetColor();
                Console.WriteLine();

                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");
                switch (auswahl)
                {
                    case "1": NeueNotizSchreiben(); break;
                    case "2": AlleNotizenAnzeigen(); break;
                    case "3": NotizLoeschen(); break;
                    case "0": return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        Thread.Sleep(600);
                        break;
                }
            }
        }

        private static void NeueNotizSchreiben()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("✏️  Neue Notiz schreiben", ConsoleColor.Yellow);

            string benutzer = AuthManager.AktuellerBenutzer ?? "Unbekannt";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  👤 Benutzer: {benutzer}   📅 {DateTime.Now:dd.MM.yyyy HH:mm}");
            Console.ResetColor();
            Console.WriteLine();

            string titel = ConsoleHelper.GetInput("Titel der Notiz");
            if (string.IsNullOrWhiteSpace(titel))
            {
                ConsoleHelper.PrintError("Titel darf nicht leer sein!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Notizinhalt (leere Zeile zum Beenden):");
            Console.ResetColor();
            Console.WriteLine();

            var zeilen = new List<string>();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  > ");
                Console.ResetColor();
                string zeile = Console.ReadLine();
                if (string.IsNullOrEmpty(zeile)) break;
                zeilen.Add(zeile);
            }

            if (zeilen.Count == 0)
            {
                ConsoleHelper.PrintError("Notizinhalt darf nicht leer sein!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Priorität:  [1] 🟢 Normal    [2] 🟡 Wichtig    [3] 🔴 Dringend");
            Console.ResetColor();
            Console.WriteLine();
            string priEingabe = ConsoleHelper.GetInput("Priorität (1-3, Standard: 1)");
            string prioritaet = priEingabe switch { "2" => "🟡 Wichtig", "3" => "🔴 Dringend", _ => "🟢 Normal" };

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            sb.AppendLine($"  📝 NOTIZ  |  #{GetNaechsteId(NotizenDatei, "📝 NOTIZ")}");
            sb.AppendLine($"  Titel:    {titel}");
            sb.AppendLine($"  Autor:    {benutzer}");
            sb.AppendLine($"  Datum:    {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"  Priorität:{prioritaet}");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            foreach (var z in zeilen)
                sb.AppendLine($"  {z}");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");

            File.AppendAllText(NotizenDatei, sb.ToString(), Encoding.UTF8);

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Notiz '{titel}' gespeichert!");
            LogManager.LogDatenGespeichert("Notiz", $"'{titel}' von {benutzer}");
            ConsoleHelper.PressKeyToContinue();
        }

        private static void AlleNotizenAnzeigen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📋 Alle Notizen", ConsoleColor.Yellow);

            if (!File.Exists(NotizenDatei) || new FileInfo(NotizenDatei).Length == 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Noch keine Notizen vorhanden.");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            foreach (var zeile in File.ReadAllLines(NotizenDatei, Encoding.UTF8))
            {
                if (zeile.Contains("🔴")) Console.ForegroundColor = ConsoleColor.Red;
                else if (zeile.Contains("🟡")) Console.ForegroundColor = ConsoleColor.Yellow;
                else if (zeile.Contains("🟢")) Console.ForegroundColor = ConsoleColor.Green;
                else if (zeile.Contains("📝 NOTIZ")) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (zeile.StartsWith("  ─") || zeile.StartsWith("─")) Console.ForegroundColor = ConsoleColor.DarkGray;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(zeile);
            }
            Console.ResetColor();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void NotizLoeschen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🗑️  Notiz löschen", ConsoleColor.Yellow);

            if (!File.Exists(NotizenDatei) || new FileInfo(NotizenDatei).Length == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Notizen vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            var bloecke = ParseNotizbloecke(NotizenDatei);
            if (bloecke.Count == 0) { ConsoleHelper.PrintWarning("Keine Notizen gefunden."); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            for (int i = 0; i < bloecke.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  [{i + 1}] {bloecke[i].Titel}  —  {bloecke[i].Autor}  —  {bloecke[i].Datum}  {bloecke[i].Prioritaet}");
                Console.ResetColor();
            }

            Console.WriteLine();
            string eingabe = ConsoleHelper.GetInput("Nummer der Notiz zum Löschen (oder 'X' zum Abbrechen)");
            if (eingabe.ToLower() == "x") return;

            if (!int.TryParse(eingabe, out int nr) || nr < 1 || nr > bloecke.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            bloecke.RemoveAt(nr - 1);
            SchreibeNotizbloeckeZurueck(NotizenDatei, bloecke);

            ConsoleHelper.PrintSuccess("✓ Notiz erfolgreich gelöscht!");
            LogManager.LogDatenGespeichert("Notiz-Gelöscht", $"Notiz #{nr} gelöscht");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═════════════════════════════════════════════════════════════════════
        // [2]  BUG MELDEN  →  AES-256 verschlüsselt
        // ═════════════════════════════════════════════════════════════════════
        private static void BugMelden()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🐛 Bug / Problem melden", ConsoleColor.Red);

            string benutzer = AuthManager.AktuellerBenutzer ?? "Unbekannt";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  👤 Gemeldet von: {benutzer}   📅 {DateTime.Now:dd.MM.yyyy HH:mm}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  🔐 Dieser Bericht wird verschlüsselt gespeichert.");
            Console.ResetColor();
            Console.WriteLine();

            string betreff = ConsoleHelper.GetInput("Betreff / Kurzbezeichnung des Problems");
            if (string.IsNullOrWhiteSpace(betreff)) { ConsoleHelper.PrintError("Betreff darf nicht leer sein!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Schweregrad:");
            Console.ResetColor();
            Console.WriteLine("  [1] 🟢 Gering  – Kleinigkeit, keine Beeinträchtigung");
            Console.WriteLine("  [2] 🟡 Mittel  – Einschränkung, Workaround möglich");
            Console.WriteLine("  [3] 🔴 Kritisch – Blockiert Arbeit, sofortige Lösung nötig");
            Console.WriteLine();
            string schwEingabe = ConsoleHelper.GetInput("Schweregrad (1-3)");
            string schweregrad = schwEingabe switch { "2" => "🟡 Mittel", "3" => "🔴 Kritisch", _ => "🟢 Gering" };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Kategorie:");
            Console.ResetColor();
            Console.WriteLine("  [1] 💾 Datenspeicherung   [2] 🖥️  Oberfläche");
            Console.WriteLine("  [3] 🔐 Sicherheit         [4] 🤖 KI-Engine");
            Console.WriteLine("  [5] 📦 Inventar           [6] 🔧 Sonstiges");
            Console.WriteLine();
            string katEingabe = ConsoleHelper.GetInput("Kategorie (1-6)");
            string kategorie = katEingabe switch
            {
                "1" => "💾 Datenspeicherung",
                "2" => "🖥️  Oberfläche",
                "3" => "🔐 Sicherheit",
                "4" => "🤖 KI-Engine",
                "5" => "📦 Inventar",
                _ => "🔧 Sonstiges"
            };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Problembeschreibung (leere Zeile zum Beenden):");
            Console.ResetColor();
            Console.WriteLine();
            var beschreibung = new List<string>();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White; Console.Write("  > "); Console.ResetColor();
                string z = Console.ReadLine();
                if (string.IsNullOrEmpty(z)) break;
                beschreibung.Add(z);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Schritte zur Reproduktion (optional, leere Zeile zum Beenden):");
            Console.ResetColor();
            Console.WriteLine();
            var schritte = new List<string>();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White; Console.Write("  > "); Console.ResetColor();
                string z = Console.ReadLine();
                if (string.IsNullOrEmpty(z)) break;
                schritte.Add(z);
            }

            string ticketId = $"BUG-{DateTime.Now:yyyyMMdd-HHmm}-{GetNaechsteIdEncrypted(BugsDatei, "BUG-REPORT"):D3}";

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"  🐛 BUG-REPORT  |  {ticketId}");
            sb.AppendLine($"  Betreff:     {betreff}");
            sb.AppendLine($"  Gemeldet:    {benutzer}  –  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"  Schweregrad: {schweregrad}");
            sb.AppendLine($"  Kategorie:   {kategorie}");
            sb.AppendLine($"  Status:      🔵 Offen");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            sb.AppendLine("  BESCHREIBUNG:");
            foreach (var z in beschreibung)
                sb.AppendLine($"    {z}");
            if (schritte.Count > 0)
            {
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                sb.AppendLine("  REPRODUKTION:");
                for (int i = 0; i < schritte.Count; i++)
                    sb.AppendLine($"    {i + 1}. {schritte[i]}");
            }
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // 🔐 Verschlüsselt speichern
            EncryptionManager.AppendEncrypted(BugsDatei, sb.ToString());

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║   ✓ BUG-REPORT VERSCHLÜSSELT GESPEICHERT  🔐              ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  🎫 Ticket-ID:  {ticketId}");

            LogManager.LogDatenGespeichert("Bug-Report", $"{ticketId}: {betreff}");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═════════════════════════════════════════════════════════════════════
        // [3]  VORSCHLAG EINREICHEN  →  AES-256 verschlüsselt
        // ═════════════════════════════════════════════════════════════════════
        private static void VorschlagMelden()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("💡 Neuen Vorschlag einreichen", ConsoleColor.Green);

            string benutzer = AuthManager.AktuellerBenutzer ?? "Unbekannt";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  👤 Eingereicht von: {benutzer}   📅 {DateTime.Now:dd.MM.yyyy HH:mm}");
            Console.WriteLine("  🔐 Dieser Vorschlag wird verschlüsselt gespeichert.");
            Console.ResetColor();
            Console.WriteLine();

            string titel = ConsoleHelper.GetInput("Titel des Vorschlags");
            if (string.IsNullOrWhiteSpace(titel)) { ConsoleHelper.PrintError("Titel darf nicht leer sein!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Bereich:");
            Console.ResetColor();
            Console.WriteLine("  [1] 📦 Inventar          [2] 👥 Mitarbeiter / Benutzer");
            Console.WriteLine("  [3] 🤖 KI-Engine         [4] 📊 Dashboard / Berichte");
            Console.WriteLine("  [5] 🖨️  Hardware-Druck    [6] 🔐 Sicherheit");
            Console.WriteLine("  [7] 🎨 Oberfläche        [8] 🔧 Sonstiges");
            Console.WriteLine();
            string bereichEingabe = ConsoleHelper.GetInput("Bereich (1-8)");
            string bereich = bereichEingabe switch
            {
                "1" => "📦 Inventar",
                "2" => "👥 Mitarbeiter / Benutzer",
                "3" => "🤖 KI-Engine",
                "4" => "📊 Dashboard / Berichte",
                "5" => "🖨️  Hardware-Druck",
                "6" => "🔐 Sicherheit",
                "7" => "🎨 Oberfläche",
                _ => "🔧 Sonstiges"
            };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Priorität:  [1] 🔵 Nice-to-have    [2] 🟡 Wichtig    [3] 🔴 Dringend");
            Console.ResetColor();
            Console.WriteLine();
            string priEingabe = ConsoleHelper.GetInput("Priorität (1-3)");
            string prioritaet = priEingabe switch { "2" => "🟡 Wichtig", "3" => "🔴 Dringend", _ => "🔵 Nice-to-have" };

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Beschreibung (leere Zeile zum Beenden):");
            Console.ResetColor();
            Console.WriteLine();
            var beschreibung = new List<string>();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White; Console.Write("  > "); Console.ResetColor();
                string z = Console.ReadLine();
                if (string.IsNullOrEmpty(z)) break;
                beschreibung.Add(z);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Nutzen / Begründung (optional, leere Zeile zum Beenden):");
            Console.ResetColor();
            Console.WriteLine();
            var nutzen = new List<string>();
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White; Console.Write("  > "); Console.ResetColor();
                string z = Console.ReadLine();
                if (string.IsNullOrEmpty(z)) break;
                nutzen.Add(z);
            }

            string vorschlagId = $"VOR-{DateTime.Now:yyyyMMdd-HHmm}-{GetNaechsteIdEncrypted(VorschlaegeD, "VORSCHLAG"):D3}";

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"  💡 VORSCHLAG  |  {vorschlagId}");
            sb.AppendLine($"  Titel:       {titel}");
            sb.AppendLine($"  Eingereicht: {benutzer}  –  {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"  Bereich:     {bereich}");
            sb.AppendLine($"  Priorität:   {prioritaet}");
            sb.AppendLine($"  Status:      🔵 Eingereicht");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            sb.AppendLine("  BESCHREIBUNG:");
            foreach (var z in beschreibung)
                sb.AppendLine($"    {z}");
            if (nutzen.Count > 0)
            {
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                sb.AppendLine("  NUTZEN / BEGRÜNDUNG:");
                foreach (var z in nutzen)
                    sb.AppendLine($"    {z}");
            }
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // 🔐 Verschlüsselt speichern
            EncryptionManager.AppendEncrypted(VorschlaegeD, sb.ToString());

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║   ✓ VORSCHLAG VERSCHLÜSSELT GESPEICHERT  🔐               ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  🎫 Vorschlags-ID:  {vorschlagId}");

            LogManager.LogDatenGespeichert("Vorschlag", $"{vorschlagId}: {titel}");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═════════════════════════════════════════════════════════════════════
        // [4]  VERSIONSINFO
        // ═════════════════════════════════════════════════════════════════════
        private static void VersionsinfoAnzeigen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ║           ℹ️   VERSIONSINFO – INVENTARVERWALTUNG                ║");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ─── PRODUKT ─────────────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine($"  📦 Produkt:         Inventarverwaltung");
            Console.WriteLine($"  🔢 Version:         {VERSION}");
            Console.WriteLine($"  📅 Build-Datum:     {BUILD_DATUM}");
            Console.WriteLine($"  👨‍💼 Entwickler:      {ENTWICKLER}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ─── FUNKTIONEN ──────────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine("  📦 Inventarverwaltung      mit Bestandspflege & Mindestbeständen");
            Console.WriteLine("  👥 Mitarbeiter- & Benutzerverwaltung");
            Console.WriteLine("  🤖 KI-Engine 2.0           für intelligente Vorschläge & Analyse");
            Console.WriteLine("  🔐 AES-256-Verschlüsselung für Logs, Bugs & Vorschläge");
            Console.WriteLine("  🖨️  Hardware-Druckverwaltung");
            Console.WriteLine("  🔄 Zuweisungshistorien     mit vollständiger Protokollierung");
            Console.WriteLine("  📊 Dashboard & Berichte");
            Console.WriteLine("  ⚡ Schnellerfassung        via CSV & Templates");
            Console.WriteLine("  🧾 Rechnungsdatum & Garantie-Tracking");
            Console.WriteLine("  📝 Notizen, Bug-Reports & Vorschläge (INFOS)");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ─── CHANGELOG ───────────────────────────────────────────────────");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  v2.0.0   🆕  KI-Engine 2.0, Rechnungsdatum, Garantie");
            Console.WriteLine("           🆕  Zuweisungshistorien mit automatischer IT-Zuweisung");
            Console.WriteLine("           🆕  INFOS: Notizen, Bugs (🔐), Vorschläge (🔐)");
            Console.WriteLine("           🆕  Dev-Bereich (nur Entwickler-Zugriff)");
            Console.WriteLine("           🔧  Bestandspflege & Mindestbestand-Warnungen");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  v1.x     Grundlegende Inventar- & Mitarbeiterverwaltung");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ─── SYSTEM ──────────────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine($"  💾 Inventar:        {DataManager.Inventar.Count} Artikel");
            Console.WriteLine($"  👥 Mitarbeiter:     {DataManager.Mitarbeiter.Count} Personen");
            Console.WriteLine($"  👤 Benutzer:        {DataManager.Benutzer.Count} Accounts");
            Console.WriteLine($"  🕐 Systemzeit:      {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            Console.WriteLine($"  📝 Notizen:         {ZaehleEintraege(NotizenDatei, "📝 NOTIZ")}");
            Console.WriteLine($"  🐛 Bugs (🔐):       {GetNaechsteIdEncrypted(BugsDatei, "BUG-REPORT") - 1}");
            Console.WriteLine($"  💡 Vorschläge (🔐): {GetNaechsteIdEncrypted(VorschlaegeD, "VORSCHLAG") - 1}");
            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        // ═════════════════════════════════════════════════════════════════════
        // [5]  DEV-BEREICH  –  nur für Benutzer "jah"
        // ═════════════════════════════════════════════════════════════════════
        private static void DevBereichAnzeigen()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine();
                Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                                                                 ║");
                Console.WriteLine("  ║     🔐  ENTWICKLER-BEREICH  –  nur für jah                     ║");
                Console.WriteLine("  ║                                                                 ║");
                Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1]  🐛  Alle Bug-Reports einsehen");
                Console.WriteLine("  [2]  💡  Alle Vorschläge einsehen");
                Console.WriteLine();
                Console.WriteLine("  [0]  ↩️   Zurück");
                Console.ResetColor();
                Console.WriteLine();

                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");
                switch (auswahl)
                {
                    case "1": DevZeigeBugs(); break;
                    case "2": DevZeigeVorschlaege(); break;
                    case "0": return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        Thread.Sleep(600);
                        break;
                }
            }
        }

        private static void DevZeigeBugs()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🐛 Alle Bug-Reports [DEV]", ConsoleColor.Red);

            string inhalt = EncryptionManager.ReadEncryptedFile(BugsDatei);

            if (string.IsNullOrWhiteSpace(inhalt))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Noch keine Bug-Reports vorhanden.");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            foreach (var zeile in inhalt.Split('\n'))
            {
                string z = zeile.TrimEnd('\r');
                if (z.Contains("🔴")) Console.ForegroundColor = ConsoleColor.Red;
                else if (z.Contains("🟡")) Console.ForegroundColor = ConsoleColor.Yellow;
                else if (z.Contains("🟢")) Console.ForegroundColor = ConsoleColor.Green;
                else if (z.Contains("🐛 BUG-REPORT")) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (z.StartsWith("  ━") || z.StartsWith("━") || z.StartsWith("  ─") || z.StartsWith("─"))
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(z);
            }
            Console.ResetColor();
            // LogManager.LogDatenGeladen("Dev-BugReport", "Bugs eingesehen von jah");
            ConsoleHelper.PressKeyToContinue();
        }

        private static void DevZeigeVorschlaege()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("💡 Alle Vorschläge [DEV]", ConsoleColor.Green);

            string inhalt = EncryptionManager.ReadEncryptedFile(VorschlaegeD);

            if (string.IsNullOrWhiteSpace(inhalt))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Noch keine Vorschläge vorhanden.");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            foreach (var zeile in inhalt.Split('\n'))
            {
                string z = zeile.TrimEnd('\r');
                if (z.Contains("🔴")) Console.ForegroundColor = ConsoleColor.Red;
                else if (z.Contains("🟡")) Console.ForegroundColor = ConsoleColor.Yellow;
                else if (z.Contains("🔵")) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (z.Contains("💡 VORSCHLAG")) Console.ForegroundColor = ConsoleColor.Green;
                else if (z.StartsWith("  ━") || z.StartsWith("━") || z.StartsWith("  ─") || z.StartsWith("─"))
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(z);
            }
            Console.ResetColor();
            //LogManager.LogDatenGeladen("Dev-Vorschlaege", "Vorschläge eingesehen von jah");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═════════════════════════════════════════════════════════════════════
        // HILFSMETHODEN
        // ═════════════════════════════════════════════════════════════════════

        private static void EnsureInfoOrdner()
        {
            if (!Directory.Exists(InfoOrdner))
                Directory.CreateDirectory(InfoOrdner);
        }

        private static int GetNaechsteId(string datei, string marker)
        {
            if (!File.Exists(datei)) return 1;
            int count = 0;
            foreach (var line in File.ReadAllLines(datei, Encoding.UTF8))
                if (line.Contains(marker)) count++;
            return count + 1;
        }

        private static int GetNaechsteIdEncrypted(string datei, string marker)
        {
            string inhalt = EncryptionManager.ReadEncryptedFile(datei);
            if (string.IsNullOrEmpty(inhalt)) return 1;
            int count = 0;
            foreach (var line in inhalt.Split('\n'))
                if (line.Contains(marker)) count++;
            return count + 1;
        }

        private static int ZaehleEintraege(string datei, string marker)
        {
            if (!File.Exists(datei)) return 0;
            int count = 0;
            foreach (var line in File.ReadAllLines(datei, Encoding.UTF8))
                if (line.Contains(marker)) count++;
            return count;
        }

        // ── Notiz-Parsing ─────────────────────────────────────────────────────
        private class NotizInfo
        {
            public string Titel { get; set; }
            public string Autor { get; set; }
            public string Datum { get; set; }
            public string Prioritaet { get; set; }
            public List<string> Zeilen { get; set; } = new List<string>();
        }

        private static List<NotizInfo> ParseNotizbloecke(string datei)
        {
            var liste = new List<NotizInfo>();
            NotizInfo aktuell = null;
            bool inNotiz = false;

            foreach (var line in File.ReadAllLines(datei, Encoding.UTF8))
            {
                if (line.Contains("📝 NOTIZ"))
                {
                    aktuell = new NotizInfo();
                    inNotiz = true;
                    aktuell.Zeilen.Add(line);
                    continue;
                }
                if (inNotiz && aktuell != null)
                {
                    aktuell.Zeilen.Add(line);
                    if (line.Contains("Titel:")) aktuell.Titel = line.Replace("Titel:", "").Trim().TrimStart();
                    if (line.Contains("Autor:")) aktuell.Autor = line.Replace("Autor:", "").Trim().TrimStart();
                    if (line.Contains("Datum:")) aktuell.Datum = line.Replace("Datum:", "").Trim().TrimStart();
                    if (line.Contains("Priorität:")) aktuell.Prioritaet = line.Replace("Priorität:", "").Trim().TrimStart();

                    if (line.StartsWith("─────────────") && aktuell.Titel != null && aktuell.Zeilen.Count > 5)
                    {
                        liste.Add(aktuell);
                        aktuell = null;
                        inNotiz = false;
                    }
                }
            }
            return liste;
        }

        private static void SchreibeNotizbloeckeZurueck(string datei, List<NotizInfo> bloecke)
        {
            var sb = new StringBuilder();
            foreach (var block in bloecke)
                foreach (var zeile in block.Zeilen)
                    sb.AppendLine(zeile);
            File.WriteAllText(datei, sb.ToString(), Encoding.UTF8);
        }

        private static void DruckeHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ║                    ℹ️   INFOS & FEEDBACK                        ║");
            Console.WriteLine("  ║         Notizen · Bugs · Vorschläge · Versionsinfo              ║");
            Console.WriteLine("  ║                                                                 ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
    }
}