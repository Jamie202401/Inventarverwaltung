using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Ultra-modernes Premium-Dashboard mit Animationen und fortgeschrittenem Design
    /// Inspiriert von modernen Web-Dashboards wie Vercel, Tailwind UI, etc.
    /// </summary>
    public static class DashboardManager
    {
        private static string suchbegriff = "";
        private static string aktuellerKategorieFilter = "Alle";
        private static string aktuelleSortierung = "invNmr";
        private static bool sortierungAufsteigend = true;

        // Farbpalette für Premium-Design
        private static readonly ConsoleColor[] GradientColors =
        {
            ConsoleColor.DarkBlue, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.White
        };

        /// <summary>
        /// Haupteinstiegspunkt für das Dashboard
        /// </summary>
        public static void ZeigeDashboard()
        {
            Console.CursorVisible = false;
            bool dashboardAktiv = true;

            // Initialer Ladeeffekt
            ZeigeLadeAnimation();

            while (dashboardAktiv)
            {
                Console.Clear();
                ZeigePremiumHeader();
                ZeigeStatistikKachelnPremium();
                ZeigeSuchUndFilterLeistePremium();
                ZeigeArtikelKartenPremium();
                ZeigePremiumFusszeile();

                Console.WriteLine();
                Console.CursorVisible = true;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  ▶ ");
                Console.ResetColor();

                string eingabe = Console.ReadLine()?.Trim().ToLower();
                Console.CursorVisible = false;

                if (eingabe == "0" || eingabe == "exit" || eingabe == "q")
                {
                    ZeigeExitAnimation();
                    dashboardAktiv = false;
                }
                else if (eingabe == "suche" || eingabe == "s")
                {
                    Suchen();
                }
                else if (eingabe == "filter" || eingabe == "f")
                {
                    FilterMenu();
                }
                else if (eingabe == "sort" || eingabe == "sortieren")
                {
                    SortierMenu();
                }
                else if (eingabe == "details" || eingabe == "d")
                {
                    ArtikelDetailsAnzeigen();
                }
                else if (eingabe == "reset" || eingabe == "r")
                {
                    AllesZuruecksetzen();
                }
                else if (eingabe?.StartsWith("+") == true)
                {
                    SchnellBestandErhoehen(eingabe);
                }
                else if (eingabe?.StartsWith("-") == true)
                {
                    SchnellBestandVerringern(eingabe);
                }
                else if (!string.IsNullOrWhiteSpace(eingabe))
                {
                    suchbegriff = eingabe;
                }
            }

            Console.CursorVisible = true;
        }

        /// <summary>
        /// Zeigt eine animierte Ladesequenz beim Start
        /// </summary>
        private static void ZeigeLadeAnimation()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();

            string[] frames = { "◐", "◓", "◑", "◒" };
            string ladeText = "Dashboard wird geladen";

            for (int i = 0; i < 8; i++)
            {
                Console.SetCursorPosition(0, 12);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition((Console.WindowWidth - ladeText.Length - 3) / 2, 12);
                Console.Write($"{frames[i % frames.Length]} {ladeText}");
                Console.ResetColor();
                Thread.Sleep(150);
            }

            Thread.Sleep(200);
        }

        /// <summary>
        /// Zeigt Exit-Animation
        /// </summary>
        private static void ZeigeExitAnimation()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                         ✓ Dashboard geschlossen");
            Console.ResetColor();
            Thread.Sleep(500);
        }

        /// <summary>
        /// Premium-Header mit Gradient-Effekt und Animation
        /// </summary>
        private static void ZeigePremiumHeader()
        {
            Console.WriteLine();
            Console.WriteLine();

            // Obere Linie mit Gradient
            ZeigeGradientLinie("═");

            Console.WriteLine();

            // Titel mit Zentriertem Text und Gradient
            string titel = "📊 INVENTAR DASHBOARD";
            string subtitel = "Bestandsverwaltung & Übersicht";

            int breite = Console.WindowWidth;
            int titelPos = (breite - titel.Length) / 2;
            int subtitelPos = (breite - subtitel.Length) / 2;

            Console.Write(new string(' ', titelPos));
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (char c in titel)
            {
                Console.Write(c);
                Thread.Sleep(10);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.Write(new string(' ', subtitelPos));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(subtitel);
            Console.ResetColor();

            Console.WriteLine();

            // Untere Linie mit Gradient
            ZeigeGradientLinie("═");

            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt eine Linie mit Gradient-Effekt
        /// </summary>
        private static void ZeigeGradientLinie(string zeichen)
        {
            Console.Write("  ");
            int breite = Console.WindowWidth - 4;
            int segmentGroesse = breite / 4;

            for (int i = 0; i < breite; i++)
            {
                ConsoleColor farbe = i switch
                {
                    < 20 => ConsoleColor.DarkCyan,
                    < 40 => ConsoleColor.Cyan,
                    _ when i < breite - 40 => ConsoleColor.White,
                    _ when i < breite - 20 => ConsoleColor.Cyan,
                    _ => ConsoleColor.DarkCyan
                };

                Console.ForegroundColor = farbe;
                Console.Write(zeichen);
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Premium Statistik-Kacheln mit 3D-Effekt und Schatten
        /// </summary>
        private static void ZeigeStatistikKachelnPremium()
        {
            var stats = DataManager.GetBestandsStatistik();
            decimal gesamtwert = DataManager.Inventar.Sum(a => a.Anzahl * a.Preis);
            int kategorienAnzahl = DataManager.Inventar.Select(a => a.Kategorie).Distinct().Count();

            Console.WriteLine();

            // Sektions-Header
            ZeigeSektionsHeader("📈", "STATISTIK-ÜBERSICHT");

            Console.WriteLine();

            // Kacheln mit 3D-Effekt
            ZeigePremiumKachelReihe(new[]
            {
                (new KachelData { Icon = "📦", Titel = "GESAMT", Wert = stats.gesamt.ToString(), HauptFarbe = ConsoleColor.Blue, IstHervorgehoben = true }),
                (new KachelData { Icon = "🟢", Titel = "OK", Wert = stats.ok.ToString(), HauptFarbe = ConsoleColor.Green, Prozent = stats.gesamt > 0 ? (stats.ok * 100 / stats.gesamt) : 0 }),
                (new KachelData { Icon = "🟡", Titel = "NIEDRIG", Wert = stats.niedrig.ToString(), HauptFarbe = ConsoleColor.Yellow, Prozent = stats.gesamt > 0 ? (stats.niedrig * 100 / stats.gesamt) : 0 }),
                (new KachelData { Icon = "🔴", Titel = "LEER", Wert = stats.leer.ToString(), HauptFarbe = ConsoleColor.Red, IstWarnung = stats.leer > 0 }),
                (new KachelData { Icon = "💰", Titel = "WERT", Wert = FormatGeld(gesamtwert), HauptFarbe = ConsoleColor.Magenta, IstHervorgehoben = true })
            });

            Console.WriteLine();
        }

        private class KachelData
        {
            public string Icon { get; set; }
            public string Titel { get; set; }
            public string Wert { get; set; }
            public ConsoleColor HauptFarbe { get; set; }
            public int Prozent { get; set; }
            public bool IstHervorgehoben { get; set; }
            public bool IstWarnung { get; set; }
        }

        /// <summary>
        /// Zeigt eine Reihe von Premium-Kacheln mit 3D-Effekt
        /// </summary>
        private static void ZeigePremiumKachelReihe(KachelData[] kacheln)
        {
            int kachelBreite = 19;
            int abstand = 2;

            // Obere Schatten-Linie
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  " + new string('▀', kachelBreite));
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Obere Linie
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("╔" + new string('═', kachelBreite) + "╗");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Icon-Zeile mit Animation-Effekt
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("║ ");
                Console.ForegroundColor = kachel.HauptFarbe;
                Console.Write($"{kachel.Icon}");
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write(new string(' ', kachelBreite - 3) + " ║");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Titel-Zeile
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{kachel.Titel,-17}");
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write(" ║");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Trennlinie
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("║" + new string('─', kachelBreite) + "║");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Wert-Zeile (groß und fett)
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("║ ");
                Console.ForegroundColor = kachel.HauptFarbe;
                string wert = kachel.Wert.Length > 17 ? kachel.Wert.Substring(0, 14) + "..." : kachel.Wert;
                Console.Write($"{wert,-17}");
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write(" ║");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Prozent-Balken (wenn vorhanden)
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("║ ");

                if (kachel.Prozent > 0)
                {
                    ZeigeMiniProgressBar(kachel.Prozent, kachel.HauptFarbe, 17);
                }
                else
                {
                    Console.Write(new string(' ', 17));
                }

                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write(" ║");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Untere Linie
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = kachel.IstHervorgehoben ? kachel.HauptFarbe : ConsoleColor.DarkGray;
                Console.Write("╚" + new string('═', kachelBreite) + "╝");
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();

            // Untere Schatten
            Console.Write("  ");
            foreach (var kachel in kacheln)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  " + new string('▄', kachelBreite));
                Console.Write(new string(' ', abstand));
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt einen Mini-Fortschrittsbalken
        /// </summary>
        private static void ZeigeMiniProgressBar(int prozent, ConsoleColor farbe, int breite)
        {
            int gefuellt = (prozent * breite) / 100;

            Console.ForegroundColor = farbe;
            Console.Write(new string('█', gefuellt));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', breite - gefuellt));
            Console.ResetColor();
        }

        /// <summary>
        /// Premium Such- und Filterleiste
        /// </summary>
        private static void ZeigeSuchUndFilterLeistePremium()
        {
            Console.WriteLine();
            ZeigeSektionsHeader("🔍", "SUCHE & FILTER");
            Console.WriteLine();

            // Suchfeld mit Rahmen
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("┌─ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("🔍 Suche");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" " + new string('─', 62) + "┐");
            Console.ResetColor();

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("│ ");
            Console.ResetColor();

            if (string.IsNullOrEmpty(suchbegriff))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Tippe Suchbegriff... (Inv-Nr, Gerät, Mitarbeiter, Kategorie)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("► ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(suchbegriff);
            }

            Console.SetCursorPosition(73, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("│");
            Console.ResetColor();

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("└" + new string('─', 71) + "┘");
            Console.ResetColor();

            // Filter & Sortierung
            Console.WriteLine();
            Console.Write("  ");
            ZeigeFilterChip("📂 Filter", aktuellerKategorieFilter, aktuellerKategorieFilter != "Alle" ? ConsoleColor.Cyan : ConsoleColor.DarkGray);
            Console.Write("   ");
            ZeigeFilterChip("⇅ Sort", $"{GetSortierungName()} {(sortierungAufsteigend ? "↑" : "↓")}", ConsoleColor.Magenta);
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Zeigt einen Filter-Chip
        /// </summary>
        private static void ZeigeFilterChip(string label, string wert, ConsoleColor farbe)
        {
            Console.ForegroundColor = farbe;
            Console.Write("╭─ ");
            Console.Write(label);
            Console.Write(" ─╮  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(wert);
        }

        /// <summary>
        /// Premium Artikel-Karten mit Hover-Effekt-Style
        /// </summary>
        private static void ZeigeArtikelKartenPremium()
        {
            var artikel = GetGefilterteUndSortierteArtikel();

            if (artikel.Count == 0)
            {
                ZeigePremiumLeereAnsicht();
                return;
            }

            ZeigeSektionsHeader("📦", $"ARTIKEL ({artikel.Count})");
            Console.WriteLine();

            // Premium-Tabellenkopf
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╔════╦══════════════╦══════════════════════════════╦══════════════════╦═══════════════╦══════════════════╗");
            Console.ResetColor();

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            ZeigeTabellenHeader("Nr", 4);
            Console.Write("║");
            ZeigeTabellenHeader("Inv-Nr", 14);
            Console.Write("║");
            ZeigeTabellenHeader("Gerät", 30);
            Console.Write("║");
            ZeigeTabellenHeader("Kategorie", 18);
            Console.Write("║");
            ZeigeTabellenHeader("Bestand", 15);
            Console.Write("║");
            ZeigeTabellenHeader("Status", 18);
            Console.WriteLine("║");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠════╬══════════════╬══════════════════════════════╬══════════════════╬═══════════════╬══════════════════╣");
            Console.ResetColor();

            // Artikel-Zeilen (max 12 für Premium-Look)
            int maxAnzeige = Math.Min(artikel.Count, 12);
            for (int i = 0; i < maxAnzeige; i++)
            {
                ZeigeArtikelZeilePremium(artikel[i], i + 1);
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╚════╩══════════════╩══════════════════════════════╩══════════════════╩═══════════════╩══════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"► Angezeigt: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{maxAnzeige}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" von ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{artikel.Count}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" Artikeln {(artikel.Count > 12 ? "(Top 12)" : "")}");
            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt eine Premium-Artikel-Zeile mit Balken
        /// </summary>
        private static void ZeigeArtikelZeilePremium(InvId artikel, int nummer)
        {
            var status = artikel.GetBestandsStatus();
            ConsoleColor statusFarbe = GetStatusFarbe(status);

            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            Console.ResetColor();

            // Nummer
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" {nummer,2} ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            Console.ResetColor();

            // Inv-Nr
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {TruncateString(artikel.InvNmr, 12),-12} ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            Console.ResetColor();

            // Gerät
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {TruncateString(artikel.GeraeteName, 28),-28} ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            Console.ResetColor();

            // Kategorie
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($" {TruncateString(artikel.Kategorie, 16),-16} ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            Console.ResetColor();

            // Bestand mit Progress-Bar
            Console.Write(" ");
            ZeigeBestandBalken(artikel.Anzahl, artikel.Mindestbestand, 13);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("║");
            Console.ResetColor();

            // Status Badge
            Console.Write(" ");
            ZeigePremiumStatusBadge(status);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("║");
            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt einen Bestandsbalken mit visueller Progress-Bar
        /// </summary>
        private static void ZeigeBestandBalken(int anzahl, int mindestbestand, int breite)
        {
            string text = $"{anzahl}/{mindestbestand}";
            Console.Write(text);

            int restBreite = breite - text.Length;
            if (restBreite > 0)
            {
                Console.Write(new string(' ', restBreite));
            }
        }

        /// <summary>
        /// Premium Status Badge mit Glow-Effekt
        /// </summary>
        private static void ZeigePremiumStatusBadge(BestandsStatus status)
        {
            string icon = status switch
            {
                BestandsStatus.Leer => "🔴",
                BestandsStatus.Niedrig => "🟡",
                BestandsStatus.Mittel => "🟢",
                BestandsStatus.Gut => "🟢",
                _ => "⚪"
            };

            string text = status switch
            {
                BestandsStatus.Leer => "LEER",
                BestandsStatus.Niedrig => "NIEDRIG",
                BestandsStatus.Mittel => "OK",
                BestandsStatus.Gut => "GUT",
                _ => "?"
            };

            ConsoleColor farbe = GetStatusFarbe(status);

            Console.Write(icon);
            Console.Write(" ");
            Console.ForegroundColor = farbe;
            Console.Write($"[{text}]");
            Console.ResetColor();

            // Auffüllen auf 16 Zeichen
            int laenge = text.Length + 4;
            if (laenge < 16)
            {
                Console.Write(new string(' ', 16 - laenge));
            }
        }

        /// <summary>
        /// Zeigt Premium leere Ansicht
        /// </summary>
        private static void ZeigePremiumLeereAnsicht()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            string[] art = {
                "        ╔═══════════════════════════════════╗",
                "        ║                                   ║",
                "        ║              📦                   ║",
                "        ║                                   ║",
                "        ║      Keine Artikel gefunden       ║",
                "        ║                                   ║",
                "        ║   Versuche andere Suchkriterien   ║",
                "        ║                                   ║",
                "        ╚═══════════════════════════════════╝"
            };

            foreach (string line in art)
            {
                Console.WriteLine("  " + line);
                Thread.Sleep(50);
            }

            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Premium Fußzeile mit Befehls-Grid
        /// </summary>
        private static void ZeigePremiumFusszeile()
        {
            Console.WriteLine();
            ZeigeSektionsHeader("🎛️", "BEFEHLE");
            Console.WriteLine();

            // Befehls-Grid 2x4
            string[][] befehle = new string[][]
            {
                new[] { "suche", "🔍 Suchen", "filter", "🎯 Filtern", "sort", "⇅ Sortieren", "details", "📋 Details" },
                new[] { "+INV 5", "➕ Erhöhen", "-INV 2", "➖ Verringern", "reset", "↺ Reset", "0", "❌ Exit" }
            };

            foreach (var zeile in befehle)
            {
                Console.Write("  ");
                for (int i = 0; i < zeile.Length; i += 2)
                {
                    ZeigeBefehlButton(zeile[i], zeile[i + 1]);
                    if (i < zeile.Length - 2)
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Zeigt einen Befehl als Button
        /// </summary>
        private static void ZeigeBefehlButton(string befehl, string beschreibung)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("┌─");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {befehl,-10} ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("─┐");
            Console.ResetColor();

            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
            int startPos = Console.CursorLeft - 15;

            Console.WriteLine();
            Console.SetCursorPosition(startPos, Console.CursorTop);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{beschreibung,-12}");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(" │");
            Console.ResetColor();

            Console.WriteLine();
            Console.SetCursorPosition(startPos, Console.CursorTop);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("└" + new string('─', 14) + "┘");
            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt einen Sektions-Header
        /// </summary>
        private static void ZeigeSektionsHeader(string icon, string titel)
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("╔═");
            Console.Write($" {icon} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(titel);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" ");
            Console.Write(new string('═', 60 - titel.Length));
            Console.WriteLine("═╗");
            Console.ResetColor();
        }

        /// <summary>
        /// Zeigt einen Tabellen-Header
        /// </summary>
        private static void ZeigeTabellenHeader(string text, int breite)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
          //  Console.Write(value: $" {text.ToUpper(),-breite} ");
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════════════════
        // FUNKTIONEN (Identisch mit vorheriger Version)
        // ═══════════════════════════════════════════════════════════════

        private static void Suchen()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  🔍 Suchbegriff: ");
            Console.ResetColor();
            Console.CursorVisible = true;
            string eingabe = Console.ReadLine()?.Trim();
            Console.CursorVisible = false;
            suchbegriff = eingabe ?? "";

            ZeigeErfolgsMeldung(string.IsNullOrEmpty(suchbegriff) ? "Suche gelöscht" : $"Suche nach: '{suchbegriff}'");
        }

        private static void FilterMenu()
        {
            var kategorien = DataManager.Inventar.Select(a => a.Kategorie).Distinct().OrderBy(k => k).ToList();

            Console.Clear();
            Console.WriteLine();
            ZeigeSektionsHeader("🎯", "KATEGORIE-FILTER");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  [0] 📂 Alle Kategorien");
            Console.ResetColor();

            for (int i = 0; i < kategorien.Count; i++)
            {
                int anzahl = DataManager.Inventar.Count(a => a.Kategorie == kategorien[i]);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  [{i + 1}] 📁 {kategorien[i]} ({anzahl})");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ► Auswahl: ");
            Console.ResetColor();
            Console.CursorVisible = true;
            string eingabe = Console.ReadLine()?.Trim();
            Console.CursorVisible = false;

            if (eingabe == "0")
            {
                aktuellerKategorieFilter = "Alle";
                ZeigeErfolgsMeldung("Filter entfernt");
            }
            else if (int.TryParse(eingabe, out int auswahl) && auswahl > 0 && auswahl <= kategorien.Count)
            {
                aktuellerKategorieFilter = kategorien[auswahl - 1];
                ZeigeErfolgsMeldung($"Filter: {aktuellerKategorieFilter}");
            }
        }

        private static void SortierMenu()
        {
            Console.Clear();
            Console.WriteLine();
            ZeigeSektionsHeader("⇅", "SORTIERUNG");
            Console.WriteLine();

            var optionen = new[]
            {
                ("1", "📋 Inventar-Nummer", "invNmr"),
                ("2", "📦 Gerätename", "geraeteName"),
                ("3", "📂 Kategorie", "kategorie"),
                ("4", "📊 Bestand", "anzahl"),
                ("5", "🚦 Status", "status"),
                ("6", "💰 Preis", "preis"),
                ("7", "↕️  Reihenfolge umkehren", "toggle")
            };

            foreach (var opt in optionen)
            {
                bool istAktiv = aktuelleSortierung == opt.Item3;
                Console.ForegroundColor = istAktiv ? ConsoleColor.Green : ConsoleColor.White;
                Console.Write($"  [{opt.Item1}] {opt.Item2}");
                if (istAktiv && opt.Item3 != "toggle")
                {
                    Console.Write(sortierungAufsteigend ? " ↑" : " ↓");
                }
                Console.WriteLine();
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  ► Auswahl: ");
            Console.ResetColor();
            Console.CursorVisible = true;
            string eingabe = Console.ReadLine()?.Trim();
            Console.CursorVisible = false;

            if (eingabe == "7")
            {
                sortierungAufsteigend = !sortierungAufsteigend;
                ZeigeErfolgsMeldung(sortierungAufsteigend ? "Aufsteigend ↑" : "Absteigend ↓");
            }
            else
            {
                var gewaehlte = optionen.FirstOrDefault(o => o.Item1 == eingabe);
                if (gewaehlte.Item3 != null && gewaehlte.Item3 != "toggle")
                {
                    aktuelleSortierung = gewaehlte.Item3;
                    ZeigeErfolgsMeldung($"Sortiert: {gewaehlte.Item2}");
                }
            }
        }

        private static void ArtikelDetailsAnzeigen()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  📋 Inv-Nr: ");
            Console.ResetColor();
            Console.CursorVisible = true;
            string invNr = Console.ReadLine()?.Trim();
            Console.CursorVisible = false;

            var artikel = DataManager.Inventar.FirstOrDefault(a =>
                a.InvNmr.Equals(invNr, StringComparison.OrdinalIgnoreCase));

            if (artikel != null)
            {
                InventoryManager.ZeigeArtikelDetails();
            }
            else
            {
                ZeigeFehlerMeldung($"Artikel '{invNr}' nicht gefunden!");
            }
        }

        private static void SchnellBestandErhoehen(string befehl)
        {
            var teile = befehl.Split(' ');
            if (teile.Length == 3 && int.TryParse(teile[2], out int menge))
            {
                DataManager.BestandErhoehen(teile[1], menge);
                ZeigeErfolgsMeldung($"Bestand +{menge}");
            }
        }

        private static void SchnellBestandVerringern(string befehl)
        {
            var teile = befehl.Split(' ');
            if (teile.Length == 3 && int.TryParse(teile[2], out int menge))
            {
                bool erfolg = DataManager.BestandVerringern(teile[1], menge);
                if (erfolg)
                    ZeigeErfolgsMeldung($"Bestand -{menge}");
                else
                    ZeigeFehlerMeldung("Nicht genug Bestand!");
            }
        }

        private static void AllesZuruecksetzen()
        {
            suchbegriff = "";
            aktuellerKategorieFilter = "Alle";
            aktuelleSortierung = "invNmr";
            sortierungAufsteigend = true;
            ZeigeErfolgsMeldung("Alles zurückgesetzt!");
        }

        private static void ZeigeErfolgsMeldung(string text)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ {text}");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        private static void ZeigeFehlerMeldung(string text)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ {text}");
            Console.ResetColor();
            Thread.Sleep(1500);
        }

        private static List<InvId> GetGefilterteUndSortierteArtikel()
        {
            IEnumerable<InvId> query = DataManager.Inventar;

            if (!string.IsNullOrEmpty(suchbegriff))
            {
                query = query.Where(a =>
                    a.InvNmr.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                    a.GeraeteName.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                    a.MitarbeiterBezeichnung.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase) ||
                    a.Kategorie.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase));
            }

            if (aktuellerKategorieFilter != "Alle")
            {
                query = query.Where(a => a.Kategorie == aktuellerKategorieFilter);
            }

            query = aktuelleSortierung switch
            {
                "invNmr" => sortierungAufsteigend ? query.OrderBy(a => a.InvNmr) : query.OrderByDescending(a => a.InvNmr),
                "geraeteName" => sortierungAufsteigend ? query.OrderBy(a => a.GeraeteName) : query.OrderByDescending(a => a.GeraeteName),
                "kategorie" => sortierungAufsteigend ? query.OrderBy(a => a.Kategorie) : query.OrderByDescending(a => a.Kategorie),
                "anzahl" => sortierungAufsteigend ? query.OrderBy(a => a.Anzahl) : query.OrderByDescending(a => a.Anzahl),
                "status" => sortierungAufsteigend ? query.OrderBy(a => a.GetBestandsStatus()) : query.OrderByDescending(a => a.GetBestandsStatus()),
                "preis" => sortierungAufsteigend ? query.OrderBy(a => a.Preis) : query.OrderByDescending(a => a.Preis),
                _ => query.OrderBy(a => a.InvNmr)
            };

            return query.ToList();
        }

        private static ConsoleColor GetStatusFarbe(BestandsStatus status)
        {
            return status switch
            {
                BestandsStatus.Leer => ConsoleColor.Red,
                BestandsStatus.Niedrig => ConsoleColor.Yellow,
                BestandsStatus.Mittel => ConsoleColor.Green,
                BestandsStatus.Gut => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };
        }

        private static string GetSortierungName()
        {
            return aktuelleSortierung switch
            {
                "invNmr" => "Inv-Nr",
                "geraeteName" => "Gerät",
                "kategorie" => "Kategorie",
                "anzahl" => "Bestand",
                "status" => "Status",
                "preis" => "Preis",
                _ => "?"
            };
        }

        private static string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
        }

        private static string FormatGeld(decimal betrag)
        {
            if (betrag >= 1000000)
                return $"{betrag / 1000000:F1}M€";
            if (betrag >= 1000)
                return $"{betrag / 1000:F1}K€";
            return $"{betrag:F0}€";
        }
    }
}