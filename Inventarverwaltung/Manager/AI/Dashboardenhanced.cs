using Inventarverwaltung;
using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Inventarverwaltung.Manager.AI
{
    public static class DashboardEnhanced
    {
        private static readonly ConsoleColor C_PRIMARY = ConsoleColor.Cyan;
        private static readonly ConsoleColor C_SUCCESS = ConsoleColor.Green;
        private static readonly ConsoleColor C_WARNING = ConsoleColor.Yellow;
        private static readonly ConsoleColor C_DANGER = ConsoleColor.Red;
        private static readonly ConsoleColor C_INFO = ConsoleColor.Blue;
        private static readonly ConsoleColor C_MUTED = ConsoleColor.DarkGray;
        private static readonly ConsoleColor C_WHITE = ConsoleColor.White;

        public static void ZeigeDashboardMenu()
        {
            bool menuAktiv = true;
            while (menuAktiv)
            {
                Console.Clear();
                ZeigeMenuHeader();
                Console.WriteLine();
                Console.ForegroundColor = C_PRIMARY;
                Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                    📊  DASHBOARD STATISTIKEN - MENÜ                           ║");
                Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = C_WHITE;
                Console.WriteLine("  ┌─────────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine("  │  KENNZAHLEN & ÜBERSICHTEN                                                   │");
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.ResetColor();
                Console.WriteLine("  │  [1]  📊  Kennzahlen-Kacheln (Gesamt, OK, Niedrig, Leer)                   │");
                Console.WriteLine("  │  [2]  📈  Kennzahlen Kompakt (einzeilig)                                    │");
                Console.WriteLine("  │  [3]  🔄  Trend-Indikatoren (steigend/stabil/fallend)                       │");
                Console.ForegroundColor = C_WHITE;
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.WriteLine("  │  GRAFIKEN & CHARTS                                                          │");
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.ResetColor();
                Console.WriteLine("  │  [4]  📂  Kategorie-Chart (Top 8 Kategorien)                                │");
                Console.WriteLine("  │  [5]  📊  Bestandsstatus-Chart (OK/Niedrig/Leer Verteilung)                 │");
                Console.WriteLine("  │  [6]  📉  Trend-Chart (Artikel-Verlauf letzte 20 Einträge)                  │");
                Console.ForegroundColor = C_WHITE;
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.WriteLine("  │  AKTIVITÄT & SYSTEM                                                         │");
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.ResetColor();
                Console.WriteLine("  │  [7]  📜  Aktivitäts-Feed (letzte 5 Log-Einträge)                           │");
                Console.ForegroundColor = C_WHITE;
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.WriteLine("  │  KOMPLETTANSICHTEN                                                          │");
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.ResetColor();
                Console.WriteLine("  │  [8]  🎯  Alle Kennzahlen & Trends (kombiniert)                             │");
                Console.WriteLine("  │  [9]  📊  Alle Charts (kombiniert)                                          │");
                Console.WriteLine("  │  [A]  ⭐  Komplette Dashboard-Ansicht (ALLES)                               │");
                Console.ForegroundColor = C_WHITE;
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.WriteLine("  │  NAVIGATION                                                                 │");
                Console.WriteLine("  ├─────────────────────────────────────────────────────────────────────────────┤");
                Console.ResetColor();
                Console.WriteLine("  │  [0]  🚪  Zurück zum Hauptmenü                                              │");
                Console.WriteLine("  └─────────────────────────────────────────────────────────────────────────────┘");
                Console.WriteLine();
                Console.ForegroundColor = C_PRIMARY;
                Console.Write("  ▶ Ihre Auswahl: ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim().ToUpper();
                switch (eingabe)
                {
                    case "1": ZeigeEinzelWidget("Kennzahlen-Kacheln", () => ZeigeKennzahlenKacheln()); break;
                    case "2": ZeigeEinzelWidget("Kennzahlen Kompakt", () => { Console.WriteLine(); ZeigeKennzahlenKompakt(); Console.WriteLine(); }); break;
                    case "3": ZeigeEinzelWidget("Trend-Indikatoren", () => ZeigeTrendIndikatoren()); break;
                    case "4": ZeigeEinzelWidget("Kategorie-Chart", () => ZeigeKategorieChart()); break;
                    case "5": ZeigeEinzelWidget("Bestandsstatus-Chart", () => ZeigeBestandsstatusChart()); break;
                    case "6": ZeigeEinzelWidget("Trend-Chart", () => ZeigeTrendChart()); break;
                    case "7": ZeigeEinzelWidget("Aktivitäts-Feed", () => ZeigeAktivitaetsFeed()); break;
                    case "8": ZeigeKombiniertKennzahlen(); break;
                    case "9": ZeigeKombiniertCharts(); break;
                    case "A": ZeigeErweiterteDashboardAnsicht(); break;
                    case "0": menuAktiv = false; break;
                    default:
                        Console.ForegroundColor = C_DANGER;
                        Console.WriteLine("\n  ✗ Ungültige Auswahl!");
                        Console.ResetColor();
                        global::System.Threading.Thread.Sleep(1200);
                        break;
                }
            }
        }

        private static void ZeigeMenuHeader()
        {
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              🎯  ERWEITERTE DASHBOARD-STATISTIKEN                             ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void ZeigeEinzelWidget(string titel, Action widget)
        {
            Console.Clear();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine();
            Console.WriteLine($"  ╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║  {titel.PadRight(77)} ║");
            Console.WriteLine($"  ╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            widget();
            Console.WriteLine();
            Console.ForegroundColor = C_MUTED;
            Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────");
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  Drücken Sie eine beliebige Taste zum Fortfahren...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        private static void ZeigeKombiniertKennzahlen()
        {
            Console.Clear();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              📊  KENNZAHLEN & TRENDS (Kombiniert)                             ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            ZeigeKennzahlenKacheln();
            ZeigeTrendIndikatoren();
            Console.WriteLine();
            Console.ForegroundColor = C_MUTED;
            Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────");
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  Drücken Sie eine beliebige Taste zum Fortfahren...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        private static void ZeigeKombiniertCharts()
        {
            Console.Clear();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                   📊  ALLE CHARTS (Kombiniert)                                ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            ZeigeKategorieChart();
            ZeigeBestandsstatusChart();
            ZeigeTrendChart();
            Console.WriteLine();
            Console.ForegroundColor = C_MUTED;
            Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────");
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  Drücken Sie eine beliebige Taste zum Fortfahren...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        public static void ZeigeKennzahlenKacheln()
        {
            var stats = DataManager.GetBestandsStatistik();
            int gesamt = stats.gesamt;
            int gut = gesamt - stats.niedrig - stats.leer;
            int niedrig = stats.niedrig;
            int leer = stats.leer;

            Console.WriteLine();
            Console.ForegroundColor = C_MUTED;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                          📊  KENNZAHLEN ÜBERSICHT                             ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  ┌─────────────────┬─────────────────┬─────────────────┬─────────────────┐");
            Console.WriteLine("  │  📦 GESAMT      │  🟢 LAGER OK    │  🟡 NIEDRIG     │  🔴 LEER        │");
            Console.WriteLine("  ├─────────────────┼─────────────────┼─────────────────┼─────────────────┤");
            Console.WriteLine("  │                 │                 │                 │                 │");
            Console.WriteLine($"  │   {gesamt,6}        │   {gut,6}        │   {niedrig,6}        │   {leer,6}        │");
            Console.WriteLine("  │                 │                 │                 │                 │");
            Console.WriteLine("  └─────────────────┴─────────────────┴─────────────────┴─────────────────┘");
            Console.ResetColor();
        }

        public static void ZeigeKennzahlenKompakt()
        {
            var stats = DataManager.GetBestandsStatistik();
            Console.ForegroundColor = C_MUTED; Console.Write("  📊 ");
            Console.ForegroundColor = C_WHITE; Console.Write($"Gesamt: {stats.gesamt,4}   ");
            Console.ForegroundColor = C_SUCCESS; Console.Write($"OK: {stats.gesamt - stats.niedrig - stats.leer,4}   ");
            Console.ForegroundColor = C_WARNING; Console.Write($"Niedrig: {stats.niedrig,4}   ");
            Console.ForegroundColor = C_DANGER; Console.Write($"Leer: {stats.leer,4}");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void ZeigeKategorieChart()
        {
            var kategorien = DataManager.Inventar
                .GroupBy(a => a.Kategorie ?? "Unbekannt")
                .Select(g => new { Kat = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(8)
                .ToList();

            if (kategorien.Count == 0)
            {
                Console.ForegroundColor = C_MUTED;
                Console.WriteLine("\n  (Keine Daten für Kategorien-Chart)");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║               📂  ARTIKEL PRO KATEGORIE (Top 8)                     ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            int max = kategorien.Max(x => x.Count);
            int barMaxWidth = 40;
            foreach (var k in kategorien)
            {
                int barWidth = max > 0 ? k.Count * barMaxWidth / max : 0;
                Console.ForegroundColor = C_MUTED; Console.Write($"  {k.Kat,-15} ");
                Console.ForegroundColor = C_PRIMARY; Console.Write(new string('█', barWidth));
                Console.ForegroundColor = C_MUTED; Console.Write(new string('░', barMaxWidth - barWidth));
                Console.ForegroundColor = C_WHITE; Console.WriteLine($"  {k.Count,4}");
            }
            Console.ResetColor();
        }

        public static void ZeigeBestandsstatusChart()
        {
            var stats = DataManager.GetBestandsStatistik();
            int gesamt = stats.gesamt;
            int gut = gesamt - stats.niedrig - stats.leer;
            int niedrig = stats.niedrig;
            int leer = stats.leer;
            if (gesamt == 0) return;

            Console.WriteLine();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                  📈  BESTANDSSTATUS-VERTEILUNG                      ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            ZeigeBalken("Lager OK ", gut, gesamt, 50, C_SUCCESS);
            ZeigeBalken("Niedrig  ", niedrig, gesamt, 50, C_WARNING);
            ZeigeBalken("Leer     ", leer, gesamt, 50, C_DANGER);
            Console.ResetColor();
        }

        private static void ZeigeBalken(string label, int wert, int max, int barW, ConsoleColor farbe)
        {
            int filled = max > 0 ? wert * barW / max : 0;
            float pct = max > 0 ? wert * 100f / max : 0f;
            Console.ForegroundColor = C_MUTED; Console.Write($"  {label} ");
            Console.ForegroundColor = farbe; Console.Write(new string('█', filled));
            Console.ForegroundColor = C_MUTED; Console.Write(new string('░', barW - filled));
            Console.ForegroundColor = C_WHITE; Console.WriteLine($"  {wert,4} ({pct,5:F1}%)");
        }

        public static void ZeigeTrendChart()
        {
            var letzten = DataManager.Inventar
                .Where(a => a.ErstelltAm != default)
                .OrderBy(a => a.ErstelltAm)
                .TakeLast(20)
                .ToList();

            if (letzten.Count < 2)
            {
                Console.ForegroundColor = C_MUTED;
                Console.WriteLine("\n  (Nicht genug Daten für Trend-Chart)");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║              📉  ARTIKEL-TREND (letzte 20 Einträge)                 ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            var grouped = letzten
                .GroupBy(a => a.ErstelltAm.Date.Subtract(letzten.First().ErstelltAm.Date).Days / 7)
                .Select(g => new { Woche = g.Key, Count = g.Count() })
                .OrderBy(x => x.Woche)
                .ToList();

            int maxC = grouped.Max(x => x.Count);
            for (int row = 8; row >= 1; row--)
            {
                Console.Write("  ");
                foreach (var w in grouped)
                {
                    int height = maxC > 0 ? w.Count * 8 / maxC : 0;
                    Console.ForegroundColor = height >= row ? C_PRIMARY : C_MUTED;
                    Console.Write("█");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
            Console.ForegroundColor = C_MUTED;
            Console.Write("  "); Console.WriteLine(new string('─', grouped.Count));
            Console.Write("  ");
            foreach (var w in grouped) Console.Write(w.Woche % 10);
            Console.WriteLine("  (Wochen-Index)");
            Console.ResetColor();
        }

        public static void ZeigeTrendIndikatoren()
        {
            Console.WriteLine();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                     📊  TREND-INDIKATOREN                           ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            var letzte30Tage = DateTime.Now.AddDays(-30);
            var artikelNeu = DataManager.Inventar.Count(a => a.ErstelltAm >= letzte30Tage);

            string trendIcon;
            ConsoleColor trendCol;
            if (artikelNeu > 10) { trendIcon = "📈 STEIGEND"; trendCol = C_SUCCESS; }
            else if (artikelNeu > 3) { trendIcon = "➡️  STABIL"; trendCol = C_WARNING; }
            else { trendIcon = "📉 FALLEND"; trendCol = C_DANGER; }

            Console.ForegroundColor = C_MUTED; Console.Write("  Artikel-Wachstum (30 Tage):  ");
            Console.ForegroundColor = trendCol; Console.WriteLine($"{trendIcon}  (+{artikelNeu} neue)");
            Console.ResetColor();

            var stats = DataManager.GetBestandsStatistik();
            float leerRate = stats.gesamt > 0 ? stats.leer * 100f / stats.gesamt : 0f;
            Console.ForegroundColor = C_MUTED;
            Console.Write("  Leer-Rate:                   ");
            Console.ForegroundColor = leerRate > 10 ? C_DANGER : leerRate > 5 ? C_WARNING : C_SUCCESS;
            Console.WriteLine($"{leerRate:F1}%  {(leerRate > 10 ? "⚠️ KRITISCH" : "✅ OK")}");
            Console.ResetColor();
        }

        public static void ZeigeAktivitaetsFeed()
        {
            Console.WriteLine();
            Console.ForegroundColor = C_INFO;
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    📜  AKTIVITÄTS-FEED (letzte 5)                   ║");
            Console.WriteLine("  ╚═════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            string logPath = global::System.IO.Path.Combine(
                global::System.Environment.CurrentDirectory, "System_Log.enc");

            if (!File.Exists(logPath))
            {
                Console.ForegroundColor = C_MUTED;
                Console.WriteLine("  (Keine Log-Datei vorhanden)");
                Console.ResetColor();
                return;
            }

            try
            {
                byte[] encData = File.ReadAllBytes(logPath);
                string decrypted = EncryptionManager.DecryptBytes(encData);

                if (string.IsNullOrWhiteSpace(decrypted))
                {
                    Console.ForegroundColor = C_MUTED;
                    Console.WriteLine("  (Log-Datei ist leer)");
                    Console.ResetColor();
                    return;
                }

                var zeilen = decrypted.Split(new[] { '\r', '\n' },
                    global::System.StringSplitOptions.RemoveEmptyEntries);

                foreach (var z in zeilen.TakeLast(5))
                {
                    string icon = "•"; ConsoleColor col = C_MUTED;
                    if (z.Contains("Anmeldung")) { icon = "🔐"; col = C_SUCCESS; }
                    else if (z.Contains("Hinzugefügt")) { icon = "✅"; col = C_PRIMARY; }
                    else if (z.Contains("Gelöscht")) { icon = "🗑️"; col = C_DANGER; }
                    else if (z.Contains("Bearbeitet")) { icon = "✏️"; col = C_WARNING; }
                    else if (z.Contains("Export")) { icon = "📤"; col = C_INFO; }
                    else if (z.Contains("Import")) { icon = "📥"; col = C_INFO; }

                    string kurz = z.Length > 70 ? z.Substring(0, 67) + "..." : z;
                    Console.ForegroundColor = col; Console.Write($"  {icon} ");
                    Console.ForegroundColor = C_MUTED; Console.WriteLine(kurz);
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = C_DANGER;
                Console.WriteLine($"  ✗ Fehler beim Lesen des Logs: {ex.Message}");
                Console.ResetColor();
            }
        }

        public static void ZeigeErweiterteDashboardAnsicht()
        {
            Console.Clear();
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                  📊  ERWEITERTE DASHBOARD-ANSICHT                             ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            ZeigeKennzahlenKacheln();
            ZeigeTrendIndikatoren();
            ZeigeKategorieChart();
            ZeigeBestandsstatusChart();
            ZeigeTrendChart();
            ZeigeAktivitaetsFeed();
            Console.WriteLine();
            Console.ForegroundColor = C_MUTED;
            Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────");
            Console.ForegroundColor = C_PRIMARY;
            Console.WriteLine("  Drücken Sie eine beliebige Taste zum Fortfahren...");
            Console.ResetColor();
            Console.ReadKey(true);
        }
    }
}