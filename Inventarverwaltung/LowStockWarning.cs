using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet Warnungen für niedrige Bestände
    /// Zeigt beim Programmstart ein Pop-up mit allen Artikeln unter dem Mindestbestand
    /// </summary>
    public static class LowStockWarning
    {
        /// <summary>
        /// Prüft Bestände und zeigt Warnungen beim Programmstart
        /// Muss nach dem Laden der Daten aufgerufen werden
        /// </summary>
        public static void ZeigeBestandswarnungen()
        {
            // Finde alle Artikel mit niedrigem oder leerem Bestand
            var niedrigerBestand = DataManager.Inventar
                .Where(a => a.Anzahl <= a.Mindestbestand)
                .OrderBy(a => a.Anzahl)
                .ThenBy(a => a.GeraeteName)
                .ToList();

            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Wenn keine Warnungen vorhanden sind
            if (niedrigerBestand.Count == 0)
            {
                ZeigeKeineWarnungen();
            }
            else
            {
                ZeigeWarnungsPopup(niedrigerBestand);
            }

            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt das Pop-up wenn keine Warnungen vorhanden sind
        /// </summary>
        private static void ZeigeKeineWarnungen()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            // Großer Header
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                         ⚠️  BESTANDSWARNUNG - SYSTEMPRÜFUNG                          ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            // Großes OK-Symbol
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                                    ███████████████████");
            Console.WriteLine("                                ███████████████████████████");
            Console.WriteLine("                              ███████████████████████████████");
            Console.WriteLine("                            ███████████         ███████████████");
            Console.WriteLine("                          ███████████             ███████████████");
            Console.WriteLine("                         ███████████      ✓        ███████████████");
            Console.WriteLine("                        ███████████                ███████████████");
            Console.WriteLine("                        ███████████                 ██████████████");
            Console.WriteLine("                        ███████████                  █████████████");
            Console.WriteLine("                         ███████████                ██████████████");
            Console.WriteLine("                          ███████████             ███████████████");
            Console.WriteLine("                            ███████████         ███████████████");
            Console.WriteLine("                              ███████████████████████████████");
            Console.WriteLine("                                ███████████████████████████");
            Console.WriteLine("                                    ███████████████████");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            // Status Nachricht
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                           ✅  AKTUELL KEINE MELDUNGEN                                 ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                        Alle Bestände sind im normalen Bereich!                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            // Zeige Statistik
            int gesamtArtikel = DataManager.Inventar.Count;
            int artikelGut = DataManager.Inventar.Count(a => a.GetBestandsStatus() == BestandsStatus.Gut);
            int artikelMittel = DataManager.Inventar.Count(a => a.GetBestandsStatus() == BestandsStatus.Mittel);

            Console.WriteLine("  ┌───────────────────────────────────────────────────────────────────────────────────────┐");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  │                                  📊 BESTANDSÜBERSICHT                                 │");
            Console.WriteLine("  │                                                                                       │");
            Console.ResetColor();
            Console.WriteLine("  ├───────────────────────────────────────────────────────────────────────────────────────┤");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine($"  │            Gesamt Artikel:              {gesamtArtikel,-10}                                      │");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine($"  │            🟢  Guter Bestand:           {artikelGut,-10}  Artikel                              │");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine($"  │            🟢  Mittlerer Bestand:       {artikelMittel,-10}  Artikel                              │");
            Console.WriteLine("  │                                                                                       │");
            Console.ResetColor();
            Console.WriteLine("  └───────────────────────────────────────────────────────────────────────────────────────┘");
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt das Warnungs-Pop-up mit allen kritischen Artikeln
        /// </summary>
        private static void ZeigeWarnungsPopup(List<InvId> niedrigerBestand)
        {
            // Kategorisiere Warnungen
            var leereArtikel = niedrigerBestand.Where(a => a.Anzahl == 0).ToList();
            var niedrigeArtikel = niedrigerBestand.Where(a => a.Anzahl > 0 && a.Anzahl <= a.Mindestbestand).ToList();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            // Großer Warn-Header
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                          ⚠️  ⚠️  ⚠️   BESTANDSWARNUNG   ⚠️  ⚠️  ⚠️                     ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            // Warn-Symbol (großes Dreieck)
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("                                          ▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                                        ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                                      ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                                    ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                                  ▲▲▲▲▲▲▲▲▲  ⚠️  ▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                                ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                              ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                            ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.WriteLine("                          ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            // Hauptmeldung
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                                       ║");
            Console.Write("  ║                     ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Es gibt {niedrigerBestand.Count,3} Artikel mit kritischem Bestand!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            int padding = 73 - niedrigerBestand.Count.ToString().Length;
            Console.WriteLine(new string(' ', padding) + "║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            // Zeige LEERE Artikel (höchste Priorität)
            if (leereArtikel.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                                                                                       ║");
                Console.WriteLine("  ║                        🔴  KRITISCH - BESTAND AUFGEBRAUCHT (0 Stück)                  ║");
                Console.WriteLine("  ║                                                                                       ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ┌───────────────┬─────────────────────────────────┬──────────────────┬────────────────┐");
                Console.Write("  │");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Inv-Nummer  ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("         Gerätename             ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Aktuell/Min   ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("     Status     ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("│");
                Console.WriteLine("  ├───────────────┼─────────────────────────────────┼──────────────────┼────────────────┤");
                Console.ResetColor();

                foreach (var artikel in leereArtikel)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  │ ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{artikel.InvNmr,-13}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" │ ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{TrimString(artikel.GeraeteName, 31),-31}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" │ ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"   {artikel.Anzahl,3} / {artikel.Mindestbestand,3}    ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" │ ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("   ❌ LEER    ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" │");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  └───────────────┴─────────────────────────────────┴──────────────────┴────────────────┘");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
            }

            // Zeige NIEDRIGE Artikel
            if (niedrigeArtikel.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                                                                                       ║");
                Console.WriteLine("  ║                          🟡  WARNUNG - NIEDRIGER BESTAND                              ║");
                Console.WriteLine("  ║                                                                                       ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ┌───────────────┬─────────────────────────────────┬──────────────────┬────────────────┐");
                Console.Write("  │");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("   Inv-Nummer  ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("         Gerätename             ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Aktuell/Min   ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("│");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("     Status     ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("│");
                Console.WriteLine("  ├───────────────┼─────────────────────────────────┼──────────────────┼────────────────┤");
                Console.ResetColor();

                foreach (var artikel in niedrigeArtikel)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  │ ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{artikel.InvNmr,-13}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" │ ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{TrimString(artikel.GeraeteName, 31),-31}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" │ ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"   {artikel.Anzahl,3} / {artikel.Mindestbestand,3}    ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" │ ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("  ⚠️  NIEDRIG ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" │");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  └───────────────┴─────────────────────────────────┴──────────────────┴────────────────┘");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
            }

            // Zeige Zusammenfassung und Empfehlungen
            ZeigeWarnungsZusammenfassung(leereArtikel.Count, niedrigeArtikel.Count);
        }

        /// <summary>
        /// Zeigt eine Zusammenfassung und Empfehlungen
        /// </summary>
        private static void ZeigeWarnungsZusammenfassung(int leereAnzahl, int niedrigeAnzahl)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                                 📋 ZUSAMMENFASSUNG                                    ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌───────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │                                                                                       │");
            if (leereAnzahl > 0)
            {
                Console.Write("  │         🔴  ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{leereAnzahl,3} Artikel");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" sind komplett aufgebraucht (0 Stück)                          │");
            }
            if (niedrigeAnzahl > 0)
            {
                Console.Write("  │         🟡  ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{niedrigeAnzahl,3} Artikel");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" haben einen niedrigen Bestand                                 │");
            }
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  └───────────────────────────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ║                            💡 EMPFOHLENE MASSNAHMEN                                   ║");
            Console.WriteLine("  ║                                                                                       ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌───────────────────────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  │         1️⃣   Prüfen Sie die kritischen Artikel umgehend                               │");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  │         2️⃣   Bestellen Sie Nachschub für leere Positionen                             │");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  │         3️⃣   Nutzen Sie Option [15] für die vollständige Liste                        │");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  │         4️⃣   Passen Sie bei Bedarf den Mindestbestand an [13]                         │");
            Console.WriteLine("  │                                                                                       │");
            Console.WriteLine("  └───────────────────────────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ═════════════════════════════════════════════════════════════════════════════════════════");
            Console.Write("      ℹ️   Diese Warnung kann jederzeit mit ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Option [15]");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(" erneut angezeigt werden.");
            Console.WriteLine("  ═════════════════════════════════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Kürzt einen String auf die angegebene Länge
        /// </summary>
        private static string TrimString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Zählt Artikel nach Bestandsstatus
        /// </summary>
        public static Dictionary<BestandsStatus, int> GetBestandsStatistik()
        {
            var statistik = new Dictionary<BestandsStatus, int>();

            foreach (BestandsStatus status in Enum.GetValues(typeof(BestandsStatus)))
            {
                statistik[status] = DataManager.Inventar.Count(a => a.GetBestandsStatus() == status);
            }

            return statistik;
        }

        /// <summary>
        /// Gibt die Anzahl der Artikel mit kritischem Bestand zurück
        /// </summary>
        public static int GetAnzahlKritischeArtikel()
        {
            return DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand);
        }

        /// <summary>
        /// Prüft ob es kritische Warnungen gibt
        /// </summary>
        public static bool HatKritischeWarnungen()
        {
            return DataManager.Inventar.Any(a => a.Anzahl == 0);
        }
    }
}