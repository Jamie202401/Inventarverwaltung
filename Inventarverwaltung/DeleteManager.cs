using System;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet das Löschen von Artikeln, Mitarbeitern und Benutzern
    /// </summary>
    public static class DeleteManager
    {
        /// <summary>
        /// Hauptmenü für Löschen
        /// </summary>
        public static void ZeigeLöschMenu()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("Lösch-Menü", ConsoleColor.DarkRed);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ⚠️  ACHTUNG: Gelöschte Daten können NICHT wiederhergestellt werden!");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1] 📦 Artikel löschen");
                Console.WriteLine("  [2] 👥 Mitarbeiter löschen");
                Console.WriteLine("  [3] 👨‍💼 Benutzer löschen");
                Console.WriteLine();
                Console.WriteLine("  [0] ↩️  Zurück zum Hauptmenü");
                Console.ResetColor();

                Console.WriteLine();
                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");

                switch (auswahl)
                {
                    case "1":
                        LöscheArtikel();
                        break;
                    case "2":
                        LöscheMitarbeiter();
                        break;
                    case "3":
                        LöscheBenutzer();
                        break;
                    case "0":
                        return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }
        }

        #region Artikel löschen

        /// <summary>
        /// Löscht einen Artikel aus dem Inventar
        /// </summary>
        public static void LöscheArtikel()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Artikel löschen", ConsoleColor.Red);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige alle Artikel
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Artikel:");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                var artikel = DataManager.Inventar[i];
                var status = artikel.GetBestandsStatus();
                ConsoleColor farbe = status switch
                {
                    BestandsStatus.Leer => ConsoleColor.Red,
                    BestandsStatus.Niedrig => ConsoleColor.Yellow,
                    _ => ConsoleColor.White
                };

                Console.ForegroundColor = farbe;
                Console.WriteLine($"  [{i + 1}] {artikel.InvNmr} - {artikel.GeraeteName} ({artikel.Anzahl} Stk.)");
                Console.ResetColor();
            }

            Console.WriteLine();
            string auswahl = ConsoleHelper.GetInput("Artikel-Nummer oder Inventar-Nr (oder 'x' zum Abbrechen)");

            if (auswahl.ToLower() == "x") return;

            InvId zuLöschenderArtikel = null;

            // Prüfe ob Nummer oder Inventar-Nr eingegeben wurde
            if (int.TryParse(auswahl, out int nummer) && nummer > 0 && nummer <= DataManager.Inventar.Count)
            {
                zuLöschenderArtikel = DataManager.Inventar[nummer - 1];
            }
            else
            {
                zuLöschenderArtikel = DataManager.Inventar.FirstOrDefault(a =>
                    a.InvNmr.Equals(auswahl, StringComparison.OrdinalIgnoreCase));
            }

            if (zuLöschenderArtikel == null)
            {
                ConsoleHelper.PrintError("Artikel nicht gefunden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige Artikel-Details
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              ZU LÖSCHENDER ARTIKEL                                ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  Inventar-Nr:  {zuLöschenderArtikel.InvNmr}");
            Console.WriteLine($"  Gerät:        {zuLöschenderArtikel.GeraeteName}");
            Console.WriteLine($"  Mitarbeiter:  {zuLöschenderArtikel.MitarbeiterBezeichnung}");
            Console.WriteLine($"  Anzahl:       {zuLöschenderArtikel.Anzahl} Stück");
            Console.WriteLine($"  Preis:        {zuLöschenderArtikel.Preis:F2}€");
            Console.WriteLine();

            // Sicherheitsabfrage
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ⚠️  WARNUNG: Dieser Artikel wird unwiderruflich gelöscht!");
            Console.ResetColor();
            Console.WriteLine();

            string bestaetigung = ConsoleHelper.GetInput("Wirklich löschen? (ja/nein)");

            if (bestaetigung.ToLower() != "ja")
            {
                ConsoleHelper.PrintInfo("Löschen abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Löschen
            string geloeschterArtikel = $"{zuLöschenderArtikel.InvNmr} - {zuLöschenderArtikel.GeraeteName}";
            DataManager.Inventar.Remove(zuLöschenderArtikel);
            DataManager.SaveKomplettesInventar();

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Artikel '{geloeschterArtikel}' wurde gelöscht!");

            // Logging
            LogManager.LogDatenGespeichert("Artikel-Löschung",
                $"Gelöscht: {geloeschterArtikel} (Mitarbeiter: {zuLöschenderArtikel.MitarbeiterBezeichnung})");

            // KI neu initialisieren
            IntelligentAssistant.IniializeAI();

            ConsoleHelper.PressKeyToContinue();
        }

        #endregion

        #region Mitarbeiter löschen

        /// <summary>
        /// Löscht einen Mitarbeiter
        /// </summary>
        public static void LöscheMitarbeiter()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Mitarbeiter löschen", ConsoleColor.Red);

            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Mitarbeiter vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige alle Mitarbeiter
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Mitarbeiter:");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                var m = DataManager.Mitarbeiter[i];

                // Prüfe ob Mitarbeiter Artikel hat
                int anzahlArtikel = DataManager.Inventar.Count(a =>
                    a.MitarbeiterBezeichnung.Equals($"{m.VName} {m.NName}", StringComparison.OrdinalIgnoreCase));

                if (anzahlArtikel > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  [{i + 1}] {m.VName} {m.NName} - {m.Abteilung} ⚠️  ({anzahlArtikel} Artikel zugewiesen)");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  [{i + 1}] {m.VName} {m.NName} - {m.Abteilung}");
                }
            }

            Console.WriteLine();
            string auswahl = ConsoleHelper.GetInput("Mitarbeiter-Nummer (oder 'x' zum Abbrechen)");

            if (auswahl.ToLower() == "x") return;

            if (!int.TryParse(auswahl, out int nummer) || nummer < 1 || nummer > DataManager.Mitarbeiter.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            MID mitarbeiter = DataManager.Mitarbeiter[nummer - 1];
            string mitarbeiterName = $"{mitarbeiter.VName} {mitarbeiter.NName}";

            // Prüfe ob Mitarbeiter Artikel hat
            var zugewieseneArtikel = DataManager.Inventar.Where(a =>
                a.MitarbeiterBezeichnung.Equals(mitarbeiterName, StringComparison.OrdinalIgnoreCase)).ToList();

            // Zeige Mitarbeiter-Details
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              ZU LÖSCHENDER MITARBEITER                            ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  Name:      {mitarbeiterName}");
            Console.WriteLine($"  Abteilung: {mitarbeiter.Abteilung}");
            Console.WriteLine($"  Artikel:   {zugewieseneArtikel.Count} zugewiesen");
            Console.WriteLine();

            if (zugewieseneArtikel.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ⚠️  WARNUNG: Diesem Mitarbeiter sind noch Artikel zugewiesen!");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Zugewiesene Artikel:");
                Console.ResetColor();
                foreach (var artikel in zugewieseneArtikel.Take(5))
                {
                    Console.WriteLine($"     • {artikel.InvNmr} - {artikel.GeraeteName}");
                }
                if (zugewieseneArtikel.Count > 5)
                {
                    Console.WriteLine($"     ... und {zugewieseneArtikel.Count - 5} weitere");
                }
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  💡 Tipp: Weisen Sie die Artikel zuerst einem anderen Mitarbeiter zu!");
                Console.ResetColor();
                Console.WriteLine();

                string trotzdem = ConsoleHelper.GetInput("Trotzdem löschen? (ja/nein)");
                if (trotzdem.ToLower() != "ja")
                {
                    ConsoleHelper.PrintInfo("Löschen abgebrochen.");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }
            }

            // Sicherheitsabfrage
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ⚠️  WARNUNG: Dieser Mitarbeiter wird unwiderruflich gelöscht!");
            Console.ResetColor();
            Console.WriteLine();

            string bestaetigung = ConsoleHelper.GetInput("Wirklich löschen? (ja/nein)");

            if (bestaetigung.ToLower() != "ja")
            {
                ConsoleHelper.PrintInfo("Löschen abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Löschen
            DataManager.Mitarbeiter.Remove(mitarbeiter);
            DataManager.SaveKompletteMitarbeiter();

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Mitarbeiter '{mitarbeiterName}' wurde gelöscht!");

            if (zugewieseneArtikel.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠️  Hinweis: {zugewieseneArtikel.Count} Artikel sind weiterhin '{mitarbeiterName}' zugewiesen");
                Console.WriteLine("  → Bitte weisen Sie diese Artikel neu zu!");
                Console.ResetColor();
            }

            // Logging
            LogManager.LogDatenGespeichert("Mitarbeiter-Löschung",
                $"Gelöscht: {mitarbeiterName} (Abteilung: {mitarbeiter.Abteilung}, {zugewieseneArtikel.Count} Artikel zugewiesen)");

            // KI neu initialisieren
            IntelligentAssistant.IniializeAI();

            ConsoleHelper.PressKeyToContinue();
        }

        #endregion

        #region Benutzer löschen

        /// <summary>
        /// Löscht einen Benutzer
        /// </summary>
        public static void LöscheBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Benutzer löschen", ConsoleColor.Red);

            if (DataManager.Benutzer.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Benutzer vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            if (DataManager.Benutzer.Count == 1)
            {
                ConsoleHelper.PrintError("Der letzte Benutzer kann nicht gelöscht werden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Zeige alle Benutzer
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Benutzer:");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < DataManager.Benutzer.Count; i++)
            {
                var b = DataManager.Benutzer[i];
                string icon = b.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";

                // Zeige aktuell angemeldeten Benutzer
                if (b.Benutzername.Equals(AuthManager.AktuellerBenutzer, StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"  [{i + 1}] {icon} {b.Benutzername} - {b.Berechtigung} ⭐ (angemeldet)");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  [{i + 1}] {icon} {b.Benutzername} - {b.Berechtigung}");
                }
            }

            Console.WriteLine();
            string auswahl = ConsoleHelper.GetInput("Benutzer-Nummer (oder 'x' zum Abbrechen)");

            if (auswahl.ToLower() == "x") return;

            if (!int.TryParse(auswahl, out int nummer) || nummer < 1 || nummer > DataManager.Benutzer.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Accounts benutzer = DataManager.Benutzer[nummer - 1];

            // Prüfe ob aktuell angemeldeter Benutzer
            bool istAktuellerBenutzer = benutzer.Benutzername.Equals(AuthManager.AktuellerBenutzer, StringComparison.OrdinalIgnoreCase);

            // Zeige Benutzer-Details
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              ZU LÖSCHENDER BENUTZER                               ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            string icon2 = benutzer.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
            Console.WriteLine($"  {icon2} Benutzername: {benutzer.Benutzername}");
            Console.WriteLine($"  Berechtigung:  {benutzer.Berechtigung}");
            Console.WriteLine();

            if (istAktuellerBenutzer)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ⚠️  WARNUNG: Sie sind aktuell mit diesem Benutzer angemeldet!");
                Console.WriteLine("  → Sie können sich selbst nicht löschen!");
                Console.ResetColor();
                Console.WriteLine();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Prüfe ob letzter Admin
            int anzahlAdmins = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin);
            if (benutzer.Berechtigung == Berechtigungen.Admin && anzahlAdmins == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ⚠️  WARNUNG: Dies ist der letzte Administrator!");
                Console.WriteLine("  → Es muss mindestens ein Admin im System bleiben!");
                Console.ResetColor();
                Console.WriteLine();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Sicherheitsabfrage
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ⚠️  WARNUNG: Dieser Benutzer wird unwiderruflich gelöscht!");
            Console.ResetColor();
            Console.WriteLine();

            string bestaetigung = ConsoleHelper.GetInput("Wirklich löschen? (ja/nein)");

            if (bestaetigung.ToLower() != "ja")
            {
                ConsoleHelper.PrintInfo("Löschen abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Löschen
            string geloeschterBenutzer = benutzer.Benutzername;
            DataManager.Benutzer.Remove(benutzer);
            DataManager.SaveBenutzerToFile();

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✓ Benutzer '{geloeschterBenutzer}' wurde gelöscht!");

            // Logging
            LogManager.LogDatenGespeichert("Benutzer-Löschung",
                $"Gelöscht: {geloeschterBenutzer} (Berechtigung: {benutzer.Berechtigung})");

            ConsoleHelper.PressKeyToContinue();
        }

        #endregion
    }
}