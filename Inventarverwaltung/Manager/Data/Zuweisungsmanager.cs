using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Inventarverwaltung.Manager.Data
{
    public static class ZuweisungsManager
    {
        private static string HistorieOrdner => "ZuweisungsHistorien";
        private static string GetHistorieDatei(string invNmr) => Path.Combine(HistorieOrdner, $"Historie_{invNmr.Trim()}.txt");

        public static void ZeigeZuweisungsMenu()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("🔄 Zuweisungsverwaltung", ConsoleColor.Cyan);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1] 🔄  Mitarbeiter von Artikel entfernen  (→ IT)");
                Console.WriteLine("  [2] 👤  Artikel neu zuweisen");
                Console.WriteLine("  [3] 📋  Zuweisungshistorie anzeigen");
                Console.WriteLine();
                Console.WriteLine("  [0] ↩️   Zurück");
                Console.ResetColor();
                Console.WriteLine();

                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");
                switch (auswahl)
                {
                    case "1": MitarbeiterEntfernen(); break;
                    case "2": ArtikelNeuZuweisen(); break;
                    case "3": ZuweisungsHistorieAnzeigen(); break;
                    case "0": return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        Thread.Sleep(700);
                        break;
                }
            }
        }

        public static void MitarbeiterEntfernen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("🔄 Mitarbeiter von Artikel entfernen", ConsoleColor.Yellow);
            if (DataManager.Inventar.Count == 0) { ConsoleHelper.PrintWarning("Noch keine Artikel vorhanden!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                var a = DataManager.Inventar[i];
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  [{i + 1,-3}] {a.InvNmr,-10} {a.GeraeteName,-28} → {a.MitarbeiterBezeichnung}");
                Console.ResetColor();
            }

            Console.WriteLine();
            string eingabe = ConsoleHelper.GetInput("Inventar-Nr oder Nummer (oder 'X' zum Abbrechen)");
            if (eingabe.ToLower() == "x") return;

            InvId artikel = WaehleArtikel(eingabe);
            if (artikel == null) { ConsoleHelper.PrintError("Artikel nicht gefunden!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    AKTUELLE ZUWEISUNG                         ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  📦 Artikel:         {artikel.GeraeteName} ({artikel.InvNmr})");
            Console.WriteLine($"  👤 Aktuell bei:     {artikel.MitarbeiterBezeichnung}");
            Console.WriteLine($"  📅 Angelegt am:     {artikel.ErstelltAm:dd.MM.yyyy HH:mm}");

            string abteilungAlt = GetAbteilung(artikel.MitarbeiterBezeichnung);
            Console.WriteLine($"  🏢 Abteilung:       {abteilungAlt}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠️  Der Artikel wird nach dem Entfernen automatisch der IT zugeordnet.");
            Console.ResetColor();
            Console.WriteLine();

            DateTime zugewiesenBis = DateTime.Now;
            string bisDatumEingabe = ConsoleHelper.GetInput("Zugewiesen-bis-Datum (TT.MM.JJJJ oder Enter für heute)");
            if (!string.IsNullOrWhiteSpace(bisDatumEingabe))
            {
                if (!DateTime.TryParseExact(bisDatumEingabe, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out zugewiesenBis))
                    zugewiesenBis = DateTime.Now;
            }

            string notizen = ConsoleHelper.GetInput("Notizen / Grund für Entfernung (optional)");
            if (string.IsNullOrWhiteSpace(notizen)) notizen = "Kein Grund angegeben";

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ⚠️  Möchten Sie '{artikel.MitarbeiterBezeichnung}' wirklich entfernen?");
            Console.ResetColor();
            string bestaetigung = ConsoleHelper.GetInput("Bestätigen mit 'ja'");
            if (bestaetigung.ToLower() != "ja" && bestaetigung.ToLower() != "j")
            { ConsoleHelper.PrintInfo("Vorgang abgebrochen."); ConsoleHelper.PressKeyToContinue(); return; }

            string mitarbeiterAlt = artikel.MitarbeiterBezeichnung;
            string erfasstVon = AuthManager.AktuellerBenutzer ?? "System";

            var eintrag = new ZuweisungsEintrag("Entfernt → IT", mitarbeiterAlt, "IT",
                artikel.ErstelltAm, zugewiesenBis, abteilungAlt, "IT", notizen, erfasstVon);

            artikel.MitarbeiterBezeichnung = "IT";
            artikel.ZuweisungsHistorie.Add(eintrag);
            DataManager.SaveKomplettesInventar();
            SpeichereHistorieEintrag(artikel.InvNmr, eintrag, artikel);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║     ✓ ZUWEISUNG ERFOLGREICH ENTFERNT                          ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  👤 War zugewiesen an:  {mitarbeiterAlt} ({abteilungAlt})");
            Console.WriteLine($"  📅 Zugewiesen bis:     {zugewiesenBis:dd.MM.yyyy}");
            Console.WriteLine($"  🏢 Neue Zuweisung:     IT");
            Console.WriteLine($"  📝 Notizen:            {notizen}");
            Console.WriteLine($"  👨‍💼 Erfasst von:        {erfasstVon}");
            LogManager.LogDatenGespeichert("Zuweisung-Entfernt", $"{artikel.InvNmr}: {mitarbeiterAlt} → IT");
            ConsoleHelper.PressKeyToContinue();
        }

        public static void ArtikelNeuZuweisen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("👤 Artikel neu zuweisen", ConsoleColor.Cyan);
            if (DataManager.Inventar.Count == 0) { ConsoleHelper.PrintWarning("Noch keine Artikel vorhanden!"); ConsoleHelper.PressKeyToContinue(); return; }
            if (DataManager.Mitarbeiter.Count == 0) { ConsoleHelper.PrintWarning("Noch keine Mitarbeiter vorhanden!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                var a = DataManager.Inventar[i];
                Console.WriteLine($"  [{i + 1,-3}] {a.InvNmr,-10} {a.GeraeteName,-28} → {a.MitarbeiterBezeichnung}");
            }

            Console.WriteLine();
            string eingabe = ConsoleHelper.GetInput("Inventar-Nr oder Nummer (oder 'X' zum Abbrechen)");
            if (eingabe.ToLower() == "x") return;

            InvId artikel = WaehleArtikel(eingabe);
            if (artikel == null) { ConsoleHelper.PrintError("Artikel nicht gefunden!"); ConsoleHelper.PressKeyToContinue(); return; }

            string mitarbeiterAlt = artikel.MitarbeiterBezeichnung;
            string abteilungAlt = GetAbteilung(mitarbeiterAlt);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Mitarbeiter:");
            Console.ResetColor();
            Console.WriteLine();
            for (int i = 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                var m = DataManager.Mitarbeiter[i];
                Console.WriteLine($"  [{i + 1}] {m.VName} {m.NName} ({m.Abteilung})");
            }

            Console.WriteLine();
            string mitEingabe = ConsoleHelper.GetInput("Neuer Mitarbeiter (Nummer oder Name)");
            string neuerMitarbeiter = WaehleMitarbeiter(mitEingabe);
            if (string.IsNullOrWhiteSpace(neuerMitarbeiter)) { ConsoleHelper.PrintError("Mitarbeiter nicht gefunden!"); ConsoleHelper.PressKeyToContinue(); return; }

            string abteilungNeu = GetAbteilung(neuerMitarbeiter);

            DateTime zugewiesenAm = DateTime.Now;
            string zugewiesenAmEingabe = ConsoleHelper.GetInput("Zugewiesen ab (TT.MM.JJJJ oder Enter für heute)");
            if (!string.IsNullOrWhiteSpace(zugewiesenAmEingabe))
            {
                if (!DateTime.TryParseExact(zugewiesenAmEingabe, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out zugewiesenAm))
                    zugewiesenAm = DateTime.Now;
            }

            string notizen = ConsoleHelper.GetInput("Notizen zur Zuweisung (optional)");
            if (string.IsNullOrWhiteSpace(notizen)) notizen = "Keine Notizen";
            string erfasstVon = AuthManager.AktuellerBenutzer ?? "System";

            var eintrag = new ZuweisungsEintrag("Neuzuweisung", mitarbeiterAlt, neuerMitarbeiter,
                zugewiesenAm, null, abteilungAlt, abteilungNeu, notizen, erfasstVon);

            artikel.MitarbeiterBezeichnung = neuerMitarbeiter;
            artikel.ZuweisungsHistorie.Add(eintrag);
            DataManager.SaveKomplettesInventar();
            SpeichereHistorieEintrag(artikel.InvNmr, eintrag, artikel);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║     ✓ ARTIKEL ERFOLGREICH NEU ZUGEWIESEN                      ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  📦 Artikel:          {artikel.GeraeteName} ({artikel.InvNmr})");
            Console.WriteLine($"  👤 Vorher:           {mitarbeiterAlt} ({abteilungAlt})");
            Console.WriteLine($"  👤 Jetzt:            {neuerMitarbeiter} ({abteilungNeu})");
            Console.WriteLine($"  📅 Zugewiesen ab:    {zugewiesenAm:dd.MM.yyyy}");
            Console.WriteLine($"  👨‍💼 Erfasst von:      {erfasstVon}");
            LogManager.LogDatenGespeichert("Neuzuweisung", $"{artikel.InvNmr}: {mitarbeiterAlt} → {neuerMitarbeiter}");
            ConsoleHelper.PressKeyToContinue();
        }

        public static void ZuweisungsHistorieAnzeigen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📋 Zuweisungshistorie", ConsoleColor.Magenta);
            if (DataManager.Inventar.Count == 0) { ConsoleHelper.PrintWarning("Noch keine Artikel vorhanden!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.WriteLine();
            for (int i = 0; i < DataManager.Inventar.Count; i++)
            {
                var a = DataManager.Inventar[i];
                Console.WriteLine($"  [{i + 1,-3}] {a.InvNmr,-10} {a.GeraeteName,-28} → {a.MitarbeiterBezeichnung}");
            }

            Console.WriteLine();
            string eingabe = ConsoleHelper.GetInput("Inventar-Nr oder Nummer (oder 'X' zum Abbrechen)");
            if (eingabe.ToLower() == "x") return;

            InvId artikel = WaehleArtikel(eingabe);
            if (artikel == null) { ConsoleHelper.PrintError("Artikel nicht gefunden!"); ConsoleHelper.PressKeyToContinue(); return; }

            Console.Clear();
            ConsoleHelper.PrintSectionHeader($"📋 Historie: {artikel.GeraeteName} ({artikel.InvNmr})", ConsoleColor.Magenta);
            Console.WriteLine();
            Console.WriteLine($"  📦 Inventar-Nr:      {artikel.InvNmr}");
            Console.WriteLine($"  🖥️  Gerät:            {artikel.GeraeteName}");
            Console.WriteLine($"  👤 Aktuell bei:      {artikel.MitarbeiterBezeichnung}");
            Console.WriteLine($"  📅 Angelegt am:      {artikel.ErstelltAm:dd.MM.yyyy HH:mm}");

            List<ZuweisungsEintrag> eintraege = LadeHistorieEintraege(artikel.InvNmr);
            foreach (var e in artikel.ZuweisungsHistorie)
                if (!eintraege.Any(x => x.ZugewisenAm == e.ZugewisenAm && x.Aktion == e.Aktion))
                    eintraege.Add(e);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ═══ ZUWEISUNGSHISTORIE ({eintraege.Count} Einträge) ═══");
            Console.ResetColor();

            if (eintraege.Count == 0)
            {
                Console.WriteLine(); Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Noch keine Zuweisungsänderungen vorhanden."); Console.ResetColor();
            }
            else
            {
                int nr = 1;
                foreach (var e in eintraege.OrderBy(x => x.ZugewisenAm))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"  ─── Eintrag #{nr++} ───────────────────────────────────");
                    Console.ResetColor();
                    ConsoleColor aktionFarbe = e.Aktion.Contains("Entfernt") ? ConsoleColor.Red :
                                              e.Aktion.Contains("Neu") ? ConsoleColor.Green : ConsoleColor.Cyan;
                    Console.ForegroundColor = aktionFarbe;
                    Console.WriteLine($"  🔖 Aktion:     {e.Aktion}");
                    Console.ResetColor();
                    Console.WriteLine($"  👤 Von:        {e.MitarbeiterAlt}  ({e.AbteilungAlt})");
                    Console.WriteLine($"  👤 Nach:       {e.MitarbeiterNeu}  ({e.AbteilungNeu})");
                    Console.WriteLine($"  📅 Ab:         {e.ZugewisenAm:dd.MM.yyyy HH:mm}");
                    string bisText = e.ZugewiesenBis.HasValue ? e.ZugewiesenBis.Value.ToString("dd.MM.yyyy") : "noch aktiv";
                    Console.WriteLine($"  📅 Bis:        {bisText}");
                    Console.WriteLine($"  📝 Notizen:    {e.Notizen}");
                    Console.WriteLine($"  👨‍💼 Erfasst von:{e.ErfasstVon}");
                }
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        private static InvId WaehleArtikel(string eingabe)
        {
            if (int.TryParse(eingabe, out int nr) && nr > 0 && nr <= DataManager.Inventar.Count)
                return DataManager.Inventar[nr - 1];
            return DataManager.Inventar.FirstOrDefault(a =>
                a.InvNmr.Equals(eingabe.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string WaehleMitarbeiter(string eingabe)
        {
            if (int.TryParse(eingabe, out int nr) && nr > 0 && nr <= DataManager.Mitarbeiter.Count)
            {
                var m = DataManager.Mitarbeiter[nr - 1];
                return $"{m.VName} {m.NName}";
            }
            var gefunden = DataManager.Mitarbeiter.FirstOrDefault(m =>
                $"{m.VName} {m.NName}".Equals(eingabe.Trim(), StringComparison.OrdinalIgnoreCase));
            return gefunden != null ? $"{gefunden.VName} {gefunden.NName}" : null;
        }

        private static string GetAbteilung(string mitarbeiterName)
        {
            if (mitarbeiterName == "IT") return "IT";
            var m = DataManager.Mitarbeiter.FirstOrDefault(x =>
                $"{x.VName} {x.NName}".Equals(mitarbeiterName.Trim(), StringComparison.OrdinalIgnoreCase));
            return m?.Abteilung ?? "Unbekannt";
        }

        private static void SpeichereHistorieEintrag(string invNmr, ZuweisungsEintrag eintrag, InvId artikel)
        {
            try
            {
                if (!Directory.Exists(HistorieOrdner)) Directory.CreateDirectory(HistorieOrdner);
                string datei = GetHistorieDatei(invNmr);
                bool istNeu = !File.Exists(datei) || new FileInfo(datei).Length == 0;
                var sb = new StringBuilder();
                if (istNeu)
                {
                    sb.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");
                    sb.AppendLine("║              ZUWEISUNGSHISTORIE – INVENTARVERWALTUNG                  ║");
                    sb.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");
                    sb.AppendLine();
                    sb.AppendLine($"# Inventar-Nr:   {artikel.InvNmr}");
                    sb.AppendLine($"# Gerätename:    {artikel.GeraeteName}");
                    sb.AppendLine($"# Angelegt am:   {artikel.ErstelltAm:dd.MM.yyyy HH:mm:ss}");
                    sb.AppendLine();
                    sb.AppendLine("[HISTORIE]");
                    sb.AppendLine();
                }
                sb.AppendLine(eintrag.ToFileString());
                File.AppendAllText(datei, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { LogManager.LogWarnung("ZuweisungsHistorie", $"Fehler beim Speichern: {ex.Message}"); }
        }

        private static List<ZuweisungsEintrag> LadeHistorieEintraege(string invNmr)
        {
            var liste = new List<ZuweisungsEintrag>();
            string datei = GetHistorieDatei(invNmr);
            if (!File.Exists(datei)) return liste;
            bool inHistorie = false;
            foreach (var line in File.ReadAllLines(datei, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#") || line.StartsWith("=") || line.StartsWith("╔") ||
                    line.StartsWith("║") || line.StartsWith("╚") || line.StartsWith("─")) continue;
                if (line.Contains("[HISTORIE]")) { inHistorie = true; continue; }
                if (!inHistorie || !line.Contains("|")) continue;
                var e = ZuweisungsEintrag.FromFileString(line);
                if (e != null) liste.Add(e);
            }
            return liste;
        }
    }
}