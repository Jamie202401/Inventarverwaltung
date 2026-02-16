using System;
using System.Collections.Generic;
using System.Threading;

namespace Inventarverwaltung.Core
{
    /// <summary>
    /// Alle Konsolenausgaben für Menüs an einem einzigen Ort.
    /// 
    /// Look &amp; Feel ändern → nur diese Datei anfassen.
    /// Kein Rendering-Code mehr verstreut über Manager-Klassen.
    /// </summary>
    public static class UI
    {
        private static readonly string[] Spinner =
            { "▁", "▂", "▃", "▄", "▅", "▆", "▇", "█", "▇", "▆", "▅", "▄", "▃", "▂" };
        private const int SpinMs = 28;

        // ══════════════════════════════════════════════════════════════
        // HAUPTMENÜ
        // ══════════════════════════════════════════════════════════════

        public static void ZeigeHauptmenu(IReadOnlyList<MenuGroup> gruppen)
        {
            Console.Clear();
            SystemHeader();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║              HAUPTMENÜ  ─  Bereich wählen                        ║");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            // Kacheln paarweise nebeneinander
            for (int i = 0; i < gruppen.Count; i += 2)
            {
                MenuGroup links = gruppen[i];
                MenuGroup rechts = (i + 1 < gruppen.Count) ? gruppen[i + 1] : null;
                KachelZeile(links, rechts);
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 69));
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [0]  ✕  Programm beenden");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ ");
            Spin();
            Console.Write("Bereich [1–" + gruppen.Count + "]: ");
            Console.ResetColor();
        }

        // ══════════════════════════════════════════════════════════════
        // UNTERMENÜ
        // ══════════════════════════════════════════════════════════════

        public static void ZeigeUntermenu(MenuGroup gruppe)
        {
            Console.Clear();
            SystemHeader();
            UnterHeader(gruppe.Icon + "  " + gruppe.Titel.ToUpper(), gruppe.Farbe);

            for (int i = 0; i < gruppe.Commands.Count; i++)
            {
                ICommand cmd = gruppe.Commands[i];
                MenuItem((i + 1).ToString(), cmd.Icon, cmd.Label, gruppe.Farbe);
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string('─', 44));
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [0]  ←  Zurück zum Hauptmenü");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Auswahl: ");
            Console.ResetColor();
        }

        // ══════════════════════════════════════════════════════════════
        // FEHLER
        // ══════════════════════════════════════════════════════════════

        public static void ZeigeUngueltigeEingabe()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  Ungültige Eingabe — bitte eine Zahl aus der Liste eingeben.");
            Console.ResetColor();
            Thread.Sleep(900);
        }

        // ══════════════════════════════════════════════════════════════
        // PRIVATE HELFER
        // ══════════════════════════════════════════════════════════════

        private static void SystemHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔════════════════════════════════════════════╗");
            Console.WriteLine("  ║      INVENTARVERWALTUNG SYSTEM             ║");
            Console.WriteLine("  ╚════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void UnterHeader(string titel, ConsoleColor farbe)
        {
            const int BoxBreite = 67;
            int innenLen = titel.Length + 7;
            int pad = Math.Max(0, BoxBreite - innenLen);

            Console.WriteLine();
            Console.ForegroundColor = farbe;
            Console.WriteLine("  ╔═══╡ " + titel + " ╞" + new string('═', pad) + "╗");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ║  Wählen Sie eine Aktion  ·  [0] = Zurück zum Hauptmenü           ║");
            Console.ForegroundColor = farbe;
            Console.WriteLine("  ╚" + new string('═', BoxBreite) + "╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void MenuItem(string nr, string icon, string text, ConsoleColor farbe)
        {
            Console.ForegroundColor = farbe;
            Console.Write("  [" + nr + "]");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  " + icon + "  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private static void KachelZeile(MenuGroup links, MenuGroup rechts)
        {
            const int W = 33; // Innere Breite der Kachel

            // ── Zeile 1: Oberer Rahmen ──────────────────────────────────
            Console.ForegroundColor = links.Farbe;
            Console.Write("  ┌─[" + links.Nr + "]" + new string('─', W - 4) + "┐");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   ");
            if (rechts != null)
            {
                Console.ForegroundColor = rechts.Farbe;
                Console.Write("┌─[" + rechts.Nr + "]" + new string('─', W - 4) + "┐");
            }
            Console.ResetColor();
            Console.WriteLine();

            // ── Zeile 2: Icon + Titel ───────────────────────────────────
            Console.ForegroundColor = links.Farbe;
            Console.Write("  │ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Pad(links.Icon + "  " + links.Titel, W));
            Console.ForegroundColor = links.Farbe;
            Console.Write(" │");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   ");
            if (rechts != null)
            {
                Console.ForegroundColor = rechts.Farbe;
                Console.Write("│ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Pad(rechts.Icon + "  " + rechts.Titel, W));
                Console.ForegroundColor = rechts.Farbe;
                Console.Write(" │");
            }
            Console.ResetColor();
            Console.WriteLine();

            // ── Zeile 3: Beschreibung ───────────────────────────────────
            Console.ForegroundColor = links.Farbe;
            Console.Write("  │ ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(Pad(links.SubLabel, W));
            Console.ForegroundColor = links.Farbe;
            Console.Write(" │");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   ");
            if (rechts != null)
            {
                Console.Write("│ ");
                Console.Write(Pad(rechts.SubLabel, W));
                Console.ForegroundColor = rechts.Farbe;
                Console.Write(" │");
            }
            Console.ResetColor();
            Console.WriteLine();

            // ── Zeile 4: Unterer Rahmen ─────────────────────────────────
            Console.ForegroundColor = links.Farbe;
            Console.Write("  └" + new string('─', W + 2) + "┘");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   ");
            if (rechts != null)
            {
                Console.ForegroundColor = rechts.Farbe;
                Console.Write("└" + new string('─', W + 2) + "┘");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        private static string Pad(string s, int breite)
        {
            if (string.IsNullOrEmpty(s)) return new string(' ', breite);
            if (s.Length >= breite) return s.Substring(0, breite);
            return s + new string(' ', breite - s.Length);
        }

        private static void Spin()
        {
            foreach (string s in Spinner)
            {
                Console.Write(s);
                Thread.Sleep(SpinMs);
                Console.Write("\b");
            }
        }
    }
}