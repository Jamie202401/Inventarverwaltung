using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Data;
using Inventarverwaltung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════════╗
    /// ║                      TAG-MANAGER                                 ║
    /// ╠══════════════════════════════════════════════════════════════════╣
    /// ║  Verwaltet automatische Tags für Artikel und Mitarbeiter.        ║
    /// ║  Tags werden automatisch gesetzt bei:                            ║
    /// ║    - Artikel-Bearbeitung                                         ║
    /// ║    - Mitarbeiter-Zuweisung/Entfernung                            ║
    /// ║  NUR DEV darf Tags manuell bearbeiten oder entfernen.            ║
    /// ╚══════════════════════════════════════════════════════════════════╝
    /// </summary>
    public static class TagManager
    {
        // ══════════════════════════════════════════════════════════════
        //  SYSTEM-TAGS (automatisch vergeben)
        // ══════════════════════════════════════════════════════════════
        public const string TAG_BEARBEITET = "BEARBEITET";
        public const string TAG_MITARBEITER_GEAENDERT = "MITARBEITER_GEAENDERT";
        public const string TAG_MITARBEITER_ENTFERNT = "MITARBEITER_ENTFERNT";
        public const string TAG_MITARBEITER_BEARBEITET = "BEARBEITET";
        public const string TAG_ZUWEISUNG_GEAENDERT = "ZUWEISUNG_GEAENDERT";

        // Tag-Farben für visuelle Unterscheidung
        private static ConsoleColor GetTagFarbe(string tag)
        {
            if (tag.StartsWith("BEARBEITET")) return ConsoleColor.Cyan;
            if (tag.StartsWith("MITARBEITER_GEAENDERT")) return ConsoleColor.Yellow;
            if (tag.StartsWith("MITARBEITER_ENTFERNT")) return ConsoleColor.Red;
            if (tag.StartsWith("ZUWEISUNG_GEAENDERT")) return ConsoleColor.Green;
            return ConsoleColor.White;
        }

        private static string GetTagIcon(string tag)
        {
            if (tag.StartsWith("BEARBEITET")) return "✏️ ";
            if (tag.StartsWith("MITARBEITER_GEAENDERT")) return "🔄";
            if (tag.StartsWith("MITARBEITER_ENTFERNT")) return "❌";
            if (tag.StartsWith("ZUWEISUNG_GEAENDERT")) return "✅";
            return "🏷️ ";
        }

        // ══════════════════════════════════════════════════════════════
        //  DEV-PRÜFUNG
        // ══════════════════════════════════════════════════════════════
        public static bool IstDev()
        {
            return AuthManager.AktuellerBenutzer != null &&
                   AuthManager.AktuellerBenutzer.Equals("dev", StringComparison.OrdinalIgnoreCase);
        }

        // ══════════════════════════════════════════════════════════════
        //  ARTIKEL-TAGS
        // ══════════════════════════════════════════════════════════════
        public static void SetzeArtikelTag(InvId artikel, string tag)
        {
            string eintrag = FormatTag(tag);
            artikel.Tags.RemoveAll(t => t.StartsWith(tag + "|"));
            artikel.Tags.Add(eintrag);
        }

        public static bool EntferneArtikelTag(InvId artikel, string tag)
        {
            if (!IstDev())
            {
                ZeigeZugriffVerweigert();
                return false;
            }
            int anzahl = artikel.Tags.RemoveAll(t => t.StartsWith(tag + "|") || t == tag);
            return anzahl > 0;
        }

        public static bool EntferneAlleArtikelTags(InvId artikel)
        {
            if (!IstDev()) { ZeigeZugriffVerweigert(); return false; }
            artikel.Tags.Clear();
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        //  MITARBEITER-TAGS
        // ══════════════════════════════════════════════════════════════
        public static void SetzeMitarbeiterTag(MID mitarbeiter, string tag)
        {
            string eintrag = FormatTag(tag);
            mitarbeiter.Tags.RemoveAll(t => t.StartsWith(tag + "|"));
            mitarbeiter.Tags.Add(eintrag);
        }

        public static bool EntferneMitarbeiterTag(MID mitarbeiter, string tag)
        {
            if (!IstDev()) { ZeigeZugriffVerweigert(); return false; }
            int anzahl = mitarbeiter.Tags.RemoveAll(t => t.StartsWith(tag + "|") || t == tag);
            return anzahl > 0;
        }

        public static bool EntferneAlleMitarbeiterTags(MID mitarbeiter)
        {
            if (!IstDev()) { ZeigeZugriffVerweigert(); return false; }
            mitarbeiter.Tags.Clear();
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        //  HAUPT-ÜBERSICHT
        // ══════════════════════════════════════════════════════════════
        public static void ZeigeTagUebersicht()
        {
            while (true)
            {
                Console.Clear();
                ZeigeTagHeader();
                ZeigeStatistikPanel();
                ZeigeHauptMenu();

                string auswahl = ConsoleHelper.GetInput("  Auswahl");
                switch (auswahl)
                {
                    case "1": WaehleArtikelFuerTags(); break;
                    case "2": WaehleMitarbeiterFuerTags(); break;
                    case "0": ZeigeAbschied(); return;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  ARTIKEL-TAG ANZEIGE
        // ══════════════════════════════════════════════════════════════
        public static void ZeigeArtikelTags(InvId artikel)
        {
            Console.Clear();
            ZeigeTagHeader();

            // Artikel-Info-Karte
            ZeigeArtikelKarte(artikel);

            Console.WriteLine();

            if (artikel.Tags == null || artikel.Tags.Count == 0)
            {
                ZeigeLeereTagsPanel("Dieser Artikel hat noch keine Tags.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeTagListe(artikel.Tags, "ARTIKEL-TAGS");

            Console.WriteLine();

            if (IstDev())
            {
                ZeigeDevPanel();
                string devEingabe = ConsoleHelper.GetInput("  DEV-Aktion");

                if (devEingabe.ToLower() == "a")
                {
                    ZeigeAktionAnimation("Lösche alle Tags");
                    EntferneAlleArtikelTags(artikel);
                    DataManager.SaveKomplettesInventar();
                    ZeigeErfolgsMeldung("Alle Tags erfolgreich entfernt");
                    LogManager.LogDatenGespeichert("Tag-Manager", $"DEV: Alle Tags von Artikel {artikel.InvNmr} entfernt");
                }
                else if (devEingabe.ToLower() == "d")
                {
                    Console.WriteLine();
                    string tagNr = ConsoleHelper.GetInput("  Tag-Nummer zum Löschen");
                    if (int.TryParse(tagNr, out int nr) && nr >= 1 && nr <= artikel.Tags.Count)
                    {
                        string entfernterTag = artikel.Tags[nr - 1];
                        ZeigeAktionAnimation($"Lösche Tag #{nr}");
                        artikel.Tags.RemoveAt(nr - 1);
                        DataManager.SaveKomplettesInventar();
                        ZeigeErfolgsMeldung($"Tag erfolgreich entfernt");
                        LogManager.LogDatenGespeichert("Tag-Manager", $"DEV: Tag '{entfernterTag}' von Artikel {artikel.InvNmr} entfernt");
                    }
                    else
                    {
                        ZeigeFehlerMeldung("Ungültige Tag-Nummer");
                    }
                }
            }
            else
            {
                ZeigeInfoPanel("Tags können ausschließlich von DEV bearbeitet werden.");
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════
        //  MITARBEITER-TAG ANZEIGE
        // ══════════════════════════════════════════════════════════════
        public static void ZeigeMitarbeiterTags(MID mitarbeiter)
        {
            Console.Clear();
            ZeigeTagHeader();

            ZeigeMitarbeiterKarte(mitarbeiter);

            Console.WriteLine();

            if (mitarbeiter.Tags == null || mitarbeiter.Tags.Count == 0)
            {
                ZeigeLeereTagsPanel("Dieser Mitarbeiter hat noch keine Tags.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeTagListe(mitarbeiter.Tags, "MITARBEITER-TAGS");

            Console.WriteLine();

            if (IstDev())
            {
                ZeigeDevPanel();
                string devEingabe = ConsoleHelper.GetInput("  DEV-Aktion");

                if (devEingabe.ToLower() == "a")
                {
                    ZeigeAktionAnimation("Lösche alle Tags");
                    EntferneAlleMitarbeiterTags(mitarbeiter);
                    DataManager.SaveKompletteMitarbeiter();
                    ZeigeErfolgsMeldung("Alle Tags erfolgreich entfernt");
                    LogManager.LogDatenGespeichert("Tag-Manager", $"DEV: Alle Tags von {mitarbeiter.VName} {mitarbeiter.NName} entfernt");
                }
                else if (devEingabe.ToLower() == "d")
                {
                    Console.WriteLine();
                    string tagNr = ConsoleHelper.GetInput("  Tag-Nummer zum Löschen");
                    if (int.TryParse(tagNr, out int nr) && nr >= 1 && nr <= mitarbeiter.Tags.Count)
                    {
                        string entfernterTag = mitarbeiter.Tags[nr - 1];
                        ZeigeAktionAnimation($"Lösche Tag #{nr}");
                        mitarbeiter.Tags.RemoveAt(nr - 1);
                        DataManager.SaveKompletteMitarbeiter();
                        ZeigeErfolgsMeldung("Tag erfolgreich entfernt");
                        LogManager.LogDatenGespeichert("Tag-Manager", $"DEV: Tag '{entfernterTag}' von {mitarbeiter.VName} {mitarbeiter.NName} entfernt");
                    }
                    else
                    {
                        ZeigeFehlerMeldung("Ungültige Tag-Nummer");
                    }
                }
            }
            else
            {
                ZeigeInfoPanel("Tags können ausschließlich von DEV bearbeitet werden.");
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════
        //  AUSWAHL-SCREENS
        // ══════════════════════════════════════════════════════════════
        private static void WaehleArtikelFuerTags()
        {
            Console.Clear();
            ZeigeTagHeader();

            var artikelMitTags = DataManager.Inventar
                .Where(a => a.Tags != null && a.Tags.Count > 0)
                .OrderByDescending(a => a.Tags.Count)
                .ToList();

            if (artikelMitTags.Count == 0)
            {
                ZeigeLeereTagsPanel("Noch keine Artikel mit Tags vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            // Sektion-Header
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │  📦  ARTIKEL MIT TAGS                                            │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < artikelMitTags.Count; i++)
            {
                var a = artikelMitTags[i];
                ZeigeArtikelListenEintrag(i + 1, a);
            }

            Console.WriteLine();
            ZeigeTrennlinie();
            string auswahl = ConsoleHelper.GetInput("  Artikel-Nummer (X = Zurück)");
            if (auswahl.ToLower() == "x") return;

            if (int.TryParse(auswahl, out int nr) && nr >= 1 && nr <= artikelMitTags.Count)
            {
                ZeigeArtikelTags(artikelMitTags[nr - 1]);
            }
            else
            {
                ZeigeFehlerMeldung("Ungültige Auswahl");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        private static void WaehleMitarbeiterFuerTags()
        {
            Console.Clear();
            ZeigeTagHeader();

            var mitMitTags = DataManager.Mitarbeiter
                .Where(m => m.Tags != null && m.Tags.Count > 0)
                .OrderByDescending(m => m.Tags.Count)
                .ToList();

            if (mitMitTags.Count == 0)
            {
                ZeigeLeereTagsPanel("Noch keine Mitarbeiter mit Tags vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │  👥  MITARBEITER MIT TAGS                                        │");
            Console.WriteLine("  └─────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < mitMitTags.Count; i++)
            {
                var m = mitMitTags[i];
                ZeigeMitarbeiterListenEintrag(i + 1, m);
            }

            Console.WriteLine();
            ZeigeTrennlinie();
            string auswahl = ConsoleHelper.GetInput("  Mitarbeiter-Nummer (X = Zurück)");
            if (auswahl.ToLower() == "x") return;

            if (int.TryParse(auswahl, out int nr) && nr >= 1 && nr <= mitMitTags.Count)
            {
                ZeigeMitarbeiterTags(mitMitTags[nr - 1]);
            }
            else
            {
                ZeigeFehlerMeldung("Ungültige Auswahl");
                ConsoleHelper.PressKeyToContinue();
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  DESIGN-KOMPONENTEN
        // ══════════════════════════════════════════════════════════════

        private static void ZeigeTagHeader()
        {
            // Animierter Gradient-Effekt durch Farbwechsel
            ConsoleColor[] headerFarben = {
                ConsoleColor.DarkYellow, ConsoleColor.Yellow,
                ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.DarkYellow
            };

            Console.WriteLine();

            // Obere Doppelrahmenlinie mit Farbverlauf
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");

            // Logo-Zeile
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("🏷️  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("T A G - V E R W A L T U N G");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("                              ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ║");

            // Trennzeile
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("  ╠══════════════════════════════════════════════════════════════════╣");
            Console.WriteLine();

            // Untertitel-Zeile
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Automatisches Tagging  ·  Bearbeitungshistorie  ·  DEV-Kontrolle");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ║");

            // DEV-Badge wenn angemeldet
            if (IstDev())
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("  ║  ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("⚡ DEV-MODUS AKTIV");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" — voller Zugriff auf Tag-Bearbeitung & Löschung       ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("║");
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeStatistikPanel()
        {
            int totalArtikel = DataManager.Inventar.Count;
            int artikelMitTags = DataManager.Inventar.Count(a => a.Tags != null && a.Tags.Count > 0);
            int totalMitarbeiter = DataManager.Mitarbeiter.Count;
            int mitarbeiterMitTags = DataManager.Mitarbeiter.Count(m => m.Tags != null && m.Tags.Count > 0);
            int gesamtTags = DataManager.Inventar.Sum(a => a.Tags?.Count ?? 0)
                                   + DataManager.Mitarbeiter.Sum(m => m.Tags?.Count ?? 0);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ STATISTIK ──────────────────────────────────────────────────────┐");
            Console.ResetColor();

            // Artikel-Zeile
            ZeigeStatistikZeile("📦 Artikel getaggt", artikelMitTags, totalArtikel, ConsoleColor.Cyan);

            // Mitarbeiter-Zeile
            ZeigeStatistikZeile("👥 Mitarbeiter getaggt", mitarbeiterMitTags, totalMitarbeiter, ConsoleColor.DarkCyan);

            // Gesamt-Tags
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"🏷️  Gesamte Tags aktiv: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{gesamtTags}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', Math.Max(0, 41 - gesamtTags.ToString().Length)) + "│");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeStatistikZeile(string label, int wert, int gesamt, ConsoleColor farbe)
        {
            int balkenBreite = 30;
            int gefuellt = gesamt > 0 ? (int)((double)wert / gesamt * balkenBreite) : 0;
            double prozent = gesamt > 0 ? (double)wert / gesamt * 100 : 0;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = farbe;
            Console.Write($"{label,-25}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [");
            Console.ForegroundColor = farbe;
            Console.Write(new string('█', gefuellt));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', balkenBreite - gefuellt));
            Console.Write("] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{wert}/{gesamt}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" ({prozent:F0}%)");
            Console.WriteLine(" │");
            Console.ResetColor();
        }

        private static void ZeigeHauptMenu()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ AKTIONEN ───────────────────────────────────────────────────────┐");
            Console.ResetColor();

            ZeigeMenuPunkt("1", "📦", "Artikel-Tags anzeigen & verwalten", ConsoleColor.Cyan);
            ZeigeMenuPunkt("2", "👥", "Mitarbeiter-Tags anzeigen & verwalten", ConsoleColor.DarkCyan);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ├──────────────────────────────────────────────────────────────────┤");
            Console.ResetColor();

            ZeigeMenuPunkt("0", "↩️ ", "Zurück zum Hauptmenü", ConsoleColor.DarkGray);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeMenuPunkt(string taste, string icon, string text, ConsoleColor farbe)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"[{taste}]");
            Console.Write(" ");
            Console.ForegroundColor = farbe;
            Console.Write($"{icon}  {text}");
            int padding = 56 - text.Length - icon.Length;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', Math.Max(0, padding)) + "│");
            Console.ResetColor();
        }

        private static void ZeigeTagListe(List<string> tags, string titel)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ┌─ {titel} " + new string('─', Math.Max(0, 58 - titel.Length)) + "┐");
            Console.ResetColor();

            for (int i = 0; i < tags.Count; i++)
            {
                var teile = tags[i].Split('|');
                string tagName = teile.Length > 0 ? teile[0].Trim() : tags[i];
                string benutzer = teile.Length > 1 ? teile[1].Trim() : "System";
                string datum = teile.Length > 2 ? teile[2].Trim() : "";

                ConsoleColor tagFarbe = GetTagFarbe(tagName);
                string tagIcon = GetTagIcon(tagName);

                // Tag-Badge Zeile
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  │  ");

                // Nummer
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{i + 1:D2}] ");

                // Icon + Tag-Name als Badge
                Console.ForegroundColor = tagFarbe;
                Console.Write($"{tagIcon} ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = tagFarbe;
                Console.Write($"◈ {tagName,-28}");
                Console.ResetColor();

                // Metadaten
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"von: ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{benutzer,-10}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($" {datum}");

                // Zeilenende auffüllen
                int gesamtBreite = 5 + 6 + 2 + 30 + 5 + 10 + 1 + datum.Length;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                int rest = Math.Max(0, 65 - gesamtBreite);
                Console.WriteLine(new string(' ', rest) + "│");
                Console.ResetColor();

                // Trennlinie zwischen Tags
                if (i < tags.Count - 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("  │  " + new string('·', 60) + "│");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeArtikelKarte(InvId artikel)
        {
            int tagAnzahl = artikel.Tags?.Count ?? 0;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ ARTIKEL ────────────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"📦  {artikel.GeraeteName,-35}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"Nr: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{artikel.InvNmr,-10}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            int padding = Math.Max(0, 65 - artikel.GeraeteName.Length - artikel.InvNmr.Length - 8);
            Console.WriteLine(new string(' ', padding) + "│");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"👤  Mitarbeiter: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{artikel.MitarbeiterBezeichnung,-25}");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"🏷️  Tags: ");
            Console.ForegroundColor = tagAnzahl > 0 ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
            Console.Write($"{tagAnzahl}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            int pad2 = Math.Max(0, 65 - artikel.MitarbeiterBezeichnung.Length - tagAnzahl.ToString().Length - 24);
            Console.WriteLine(new string(' ', pad2) + "│");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeMitarbeiterKarte(MID mitarbeiter)
        {
            int tagAnzahl = mitarbeiter.Tags?.Count ?? 0;

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ┌─ MITARBEITER ────────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"👤  {mitarbeiter.VName} {mitarbeiter.NName,-30}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Abt: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            string abt = mitarbeiter.Abteilung ?? "";
            Console.Write($"{abt,-15}");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            int pad1 = Math.Max(0, 65 - mitarbeiter.VName.Length - mitarbeiter.NName.Length - abt.Length - 10);
            Console.WriteLine(new string(' ', pad1) + "│");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"🏷️  Aktive Tags: ");
            Console.ForegroundColor = tagAnzahl > 0 ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
            Console.Write($"{tagAnzahl}");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            int pad2 = Math.Max(0, 65 - tagAnzahl.ToString().Length - 16);
            Console.WriteLine(new string(' ', pad2) + "│");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeArtikelListenEintrag(int index, InvId artikel)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  [{index:D2}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"📦  {artikel.GeraeteName,-28}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" {artikel.InvNmr,-12}");

            // Tag-Badges inline
            int tagAnzahl = artikel.Tags?.Count ?? 0;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"  🏷️  ");
            Console.ForegroundColor = tagAnzahl >= 3 ? ConsoleColor.Red :
                                      tagAnzahl >= 2 ? ConsoleColor.Yellow : ConsoleColor.Green;
            Console.Write($"{tagAnzahl} Tag(s)");

            // Ersten Tag anzeigen als Vorschau
            if (tagAnzahl > 0)
            {
                string ersterTag = artikel.Tags[0].Split('|')[0].Trim();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  › ");
                ConsoleColor tagFarbe = GetTagFarbe(ersterTag);
                Console.ForegroundColor = tagFarbe;
                Console.Write(ersterTag);
                if (tagAnzahl > 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" +{tagAnzahl - 1}");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeMitarbeiterListenEintrag(int index, MID mitarbeiter)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  [{index:D2}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"👤  {mitarbeiter.VName} {mitarbeiter.NName,-22}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" {mitarbeiter.Abteilung,-15}");

            int tagAnzahl = mitarbeiter.Tags?.Count ?? 0;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"  🏷️  ");
            Console.ForegroundColor = tagAnzahl >= 3 ? ConsoleColor.Red :
                                      tagAnzahl >= 2 ? ConsoleColor.Yellow : ConsoleColor.Green;
            Console.Write($"{tagAnzahl} Tag(s)");

            if (tagAnzahl > 0)
            {
                string ersterTag = mitarbeiter.Tags[0].Split('|')[0].Trim();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  › ");
                Console.ForegroundColor = GetTagFarbe(ersterTag);
                Console.Write(ersterTag);
                if (tagAnzahl > 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" +{tagAnzahl - 1}");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeDevPanel()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ┌─ ⚡ DEV-KONSOLE ──────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("D");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]  Einzelnen Tag löschen     ");
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("A");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]  ALLE Tags löschen         ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("   │");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeLeereTagsPanel(string nachricht)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  ○  {nachricht}");
            int padding = Math.Max(0, 61 - nachricht.Length);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', padding) + "│");
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeInfoPanel(string nachricht)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ℹ️   ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(nachricht);
            Console.ResetColor();
        }

        private static void ZeigeErfolgsMeldung(string nachricht)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  ✔  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(nachricht);
            Console.ResetColor();
            Thread.Sleep(600);
        }

        private static void ZeigeFehlerMeldung(string nachricht)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ✖  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(nachricht);
            Console.ResetColor();
        }

        private static void ZeigeZugriffVerweigert()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔══════════════════════════════════════════════════╗");
            Console.WriteLine("  ║  🔒  ZUGRIFF VERWEIGERT                          ║");
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Nur DEV darf Tags bearbeiten oder löschen.");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════╝");
            Console.ResetColor();
            Thread.Sleep(1200);
        }

        private static void ZeigeAktionAnimation(string aktion)
        {
            Console.WriteLine();
            string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            Console.ForegroundColor = ConsoleColor.Yellow;
            for (int i = 0; i < 14; i++)
            {
                Console.Write($"\r  {frames[i % frames.Length]}  {aktion}...");
                Thread.Sleep(60);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void ZeigeAbschied()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Tag-Verwaltung geschlossen.");
            Console.ResetColor();
            Thread.Sleep(300);
        }

        private static void ZeigeTrennlinie()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 66));
            Console.ResetColor();
        }

        // ══════════════════════════════════════════════════════════════
        //  SERIALISIERUNG
        // ══════════════════════════════════════════════════════════════
        private static string FormatTag(string tag)
        {
            string benutzer = AuthManager.AktuellerBenutzer ?? "System";
            string zeitpunkt = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            return $"{tag}|{benutzer}|{zeitpunkt}";
        }

        public static string SerialisiereTagListe(List<string> tags)
        {
            if (tags == null || tags.Count == 0) return "";
            return string.Join("§", tags);
        }

        public static List<string> DeserialisiereTagListe(string rohdaten)
        {
            if (string.IsNullOrWhiteSpace(rohdaten))
                return new List<string>();
            return new List<string>(rohdaten.Split('§'));
        }
    }
}