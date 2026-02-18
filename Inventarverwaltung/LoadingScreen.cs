using System;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Ladebildschirm — sauber, groß, zentriert.
    /// Kein SetCursorPosition im Hauptlayout — alles läuft von oben nach unten.
    /// Spinner-Updates laufen in-place nur innerhalb der Task-Zeile.
    /// </summary>
    public static class LoadingScreen
    {
        private static readonly ConsoleColor C_CYAN = ConsoleColor.Cyan;
        private static readonly ConsoleColor C_BLUE = ConsoleColor.Blue;
        private static readonly ConsoleColor C_DBLUE = ConsoleColor.DarkBlue;
        private static readonly ConsoleColor C_DCYAN = ConsoleColor.DarkCyan;
        private static readonly ConsoleColor C_WHITE = ConsoleColor.White;
        private static readonly ConsoleColor C_YELLOW = ConsoleColor.Yellow;
        private static readonly ConsoleColor C_GREEN = ConsoleColor.Green;
        private static readonly ConsoleColor C_DGREEN = ConsoleColor.DarkGreen;
        private static readonly ConsoleColor C_GRAY = ConsoleColor.DarkGray;
        private static readonly ConsoleColor C_RED = ConsoleColor.Red;

        private static readonly Random _rnd = new Random();

        private static int _W = 120;   // Terminal-Breite
        private static int _H = 40;    // Terminal-Höhe
        private static int _LP = 0;     // Linker Rand für Zentrierung (auf BLOCK_W)

        // Feste Inhaltsbreite — alle Boxen, Balken, Logos richten sich danach
        private const int BLOCK_W = 100;

        // ─────────────────────────────────────────────────────────
        public static void Show()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Clear();

            _W = Math.Max(Console.WindowWidth - 1, BLOCK_W + 4);
            _H = Math.Max(Console.WindowHeight - 1, 40);
            _LP = (_W - BLOCK_W) / 2;   // zentrierender linker Rand

            PhaseGlitch();
            PhaseLogo();
            PhaseLaden();
            PhaseFinale();

            Console.CursorVisible = true;
        }

        // ═════════════════════════════════════════════════════════
        // PHASE 1 — GLITCH  (kurzes Rauschen in der Bildschirmmitte)
        // ═════════════════════════════════════════════════════════
        private static void PhaseGlitch()
        {
            string[] g = { "█", "▓", "▒", "░", "▄", "▀", "■", "◆", "●", "◉" };
            // Rauschen auf ca. 8 Zeilen vertikal zentriert
            int glitchH = 8;
            int glitchTop = (_H - glitchH) / 2;

            for (int frame = 0; frame < 8; frame++)
            {
                for (int y = 0; y < glitchH; y++)
                {
                    Console.SetCursorPosition(0, glitchTop + y);
                    Console.ForegroundColor = (frame + y) % 3 == 0 ? C_DBLUE
                                            : (frame + y) % 3 == 1 ? C_BLUE : C_DCYAN;
                    var sb = new System.Text.StringBuilder(_W);
                    // Leerzeichen links + Rauschen mittig
                    for (int x = 0; x < _LP; x++) sb.Append(' ');
                    for (int x = 0; x < BLOCK_W; x++)
                        sb.Append(_rnd.Next(6) < 2 ? g[_rnd.Next(g.Length)] : " ");
                    Console.Write(sb.ToString().Substring(0, Math.Min(sb.Length, _W)));
                }
                Thread.Sleep(50);
            }
            Console.Clear();
            Thread.Sleep(60);
        }

        // ═════════════════════════════════════════════════════════
        // PHASE 2 — LOGO  (groß, zentriert, von oben nach unten)
        // ═════════════════════════════════════════════════════════
        private static void PhaseLogo()
        {
            Console.Clear();

            // Leere Zeilen oben damit Logo vertikal etwas eingerückt ist
            int topPad = Math.Max(1, (_H - 28) / 4);
            for (int i = 0; i < topPad; i++) Console.WriteLine();

            // ── obere Trennlinie ─────────────────────────────────
            Ln(new string('═', BLOCK_W), C_DBLUE);
            Blank();

            // ── "INVENTAR"  ASCII-Art (7 Zeilen, ~70 Zeichen breit) ──
            ConsoleColor[] grad1 = { C_DBLUE, C_BLUE, C_DCYAN, C_CYAN, C_WHITE, C_CYAN, C_DCYAN };
            string[] inv = {
                @"██╗███╗   ██╗██╗   ██╗███████╗███╗   ██╗████████╗ █████╗ ██████╗ ",
                @"██║████╗  ██║██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔══██╗██╔══██╗",
                @"██║██╔██╗ ██║██║   ██║█████╗  ██╔██╗ ██║   ██║   ███████║██████╔╝",
                @"██║██║╚██╗██║╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ██╔══██║██╔══██╗",
                @"██║██║ ╚████║ ╚████╔╝ ███████╗██║ ╚████║   ██║   ██║  ██║██║  ██║",
                @"╚═╝╚═╝  ╚═══╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝",
            };
            for (int i = 0; i < inv.Length; i++)
            {
                Ln(inv[i], grad1[i % grad1.Length]);
                Thread.Sleep(32);
            }

            Blank();

            // ── "VERWALTUNG"  ASCII-Art (6 Zeilen, ~88 Zeichen breit) ──
            ConsoleColor[] grad2 = { C_DCYAN, C_CYAN, C_WHITE, C_CYAN, C_DCYAN, C_BLUE };
            string[] verw = {
                @"██╗   ██╗███████╗██████╗ ██╗    ██╗ █████╗ ██╗  ████████╗██╗   ██╗███╗   ██╗ ██████╗",
                @"██║   ██║██╔════╝██╔══██╗██║    ██║██╔══██╗██║  ╚══██╔══╝██║   ██║████╗  ██║██╔════╝",
                @"██║   ██║█████╗  ██████╔╝██║ █╗ ██║███████║██║     ██║   ██║   ██║██╔██╗ ██║██║  ███╗",
                @"╚██╗ ██╔╝██╔══╝  ██╔══██╗██║███╗██║██╔══██║██║     ██║   ██║   ██║██║╚██╗██║██║   ██║",
                @" ╚████╔╝ ███████╗██║  ██║╚███╔███╔╝██║  ██║███████╗██║   ╚██████╔╝██║ ╚████║╚██████╔╝",
                @"  ╚═══╝  ╚══════╝╚═╝  ╚═╝ ╚══╝╚══╝ ╚═╝  ╚═╝╚══════╝╚═╝    ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝",
            };
            for (int i = 0; i < verw.Length; i++)
            {
                Ln(verw[i], grad2[i % grad2.Length]);
                Thread.Sleep(32);
            }

            Blank();

            // ── mittlere Trennlinie ──────────────────────────────
            Ln(new string('═', BLOCK_W), C_BLUE);
            Blank();

            // ── Badge-Zeilen (Typewriter) ────────────────────────
            LnType("🤖  KI ENGINE 2.0  —  PREMIUM EDITION", C_YELLOW, 11);
            LnType("🔐  AES-256 VERSCHLÜSSELUNG  ·  VOLLSTÄNDIG OFFLINE", C_CYAN, 11);
            LnType("📊  VERSION 2.2.1  ·  PRODUCTION BUILD  ·  © 2026  jh", C_GRAY, 11);

            Blank();
            Ln(new string('═', BLOCK_W), C_BLUE);

            Console.ResetColor();
            Thread.Sleep(180);
        }

        // ═════════════════════════════════════════════════════════
        // PHASE 3 — LADEN  (Task-Liste + Fortschrittsbalken)
        // ═════════════════════════════════════════════════════════
        private static void PhaseLaden()
        {
            Blank();

            // Box-Header
            int bw = BLOCK_W - 2; // innere Box-Breite
            Console.ForegroundColor = C_DCYAN;
            Ln("╔" + new string('═', bw) + "╗", C_DCYAN);
            Ln("║" + Center("  ⚙   SYSTEM INITIALISIERUNG", bw) + "║", C_DCYAN);
            Ln("╚" + new string('═', bw) + "╝", C_DCYAN);

            Blank();

            // Task-Liste
            var tasks = new (string icon, string label, Action action)[]
            {
                ("⚙ ", "Systemkern initialisieren   ", () => InitialisiereSystem()),
                ("👥", "Benutzerdaten laden         ", () => DataManager.LoadBenutzer()),
                ("🧑", "Mitarbeiterdaten laden       ", () => DataManager.LoadMitarbeiter()),
                ("📦", "Inventar laden              ", () => DataManager.LoadInventar()),
                ("🤖", "KI-Engine 2.0 starten       ", () => KIEngine.Initialisiere()),
                ("🔐", "Verschlüsselung aktivieren  ", () => LogManager.InitializeLog()),
                ("🛡 ", "Systemintegrität prüfen     ", () => PruefeSystem()),
            };

            // Merke die Startzeile der Tasks
            int taskStartRow = Console.CursorTop;

            // Alle Tasks grau vorab ausgeben
            foreach (var t in tasks)
            {
                Console.ForegroundColor = C_GRAY;
                Console.SetCursorPosition(_LP, Console.CursorTop);
                Console.WriteLine($"  {t.icon}  {t.label}  ·····");
            }

            Blank();

            // Fortschrittsbalken-Rahmen ausgeben
            int barInner = BLOCK_W - 6;
            int barStartRow = Console.CursorTop;

            Console.ForegroundColor = C_DCYAN;
            Console.SetCursorPosition(_LP, Console.CursorTop);
            Console.WriteLine("  ┌" + new string('─', barInner) + "┐");
            Console.SetCursorPosition(_LP, Console.CursorTop);
            Console.WriteLine("  │" + new string(' ', barInner) + "│");
            Console.SetCursorPosition(_LP, Console.CursorTop);
            Console.WriteLine("  └" + new string('─', barInner) + "┘");
            Console.SetCursorPosition(_LP, Console.CursorTop);
            Console.ForegroundColor = C_GRAY;
            Console.WriteLine("  [ 0% ]  Bereit...");

            // Tasks ausführen — in-place Updates auf die reservierten Zeilen
            for (int i = 0; i < tasks.Length; i++)
            {
                int taskRow = taskStartRow + i;

                // Aktiv-Status: gelb + Spinner
                Exception err = null;
                RunWithSpinner(tasks[i].action, taskRow, tasks[i].icon, tasks[i].label,
                               ref err);

                // Ergebnis in die gleiche Zeile schreiben
                Console.SetCursorPosition(_LP, taskRow);
                if (err == null)
                {
                    Console.ForegroundColor = C_GREEN;
                    Console.Write($"  {tasks[i].icon}  {tasks[i].label}  ");
                    Console.ForegroundColor = C_DGREEN;
                    Console.Write("✔ OK          ");
                }
                else
                {
                    Console.ForegroundColor = C_RED;
                    Console.Write($"  {tasks[i].icon}  {tasks[i].label}  ✘ FEHLER      ");
                }

                // Balken aktualisieren
                float pct = (float)(i + 1) / tasks.Length;
                UpdateBar(barStartRow, barInner, pct, tasks[i].label.Trim());

                Thread.Sleep(100);
            }

            UpdateBar(barStartRow, barInner, 1.0f, "Abgeschlossen");
            Thread.Sleep(350);
        }

        private static void RunWithSpinner(Action work, int taskRow,
                                            string icon, string label,
                                            ref Exception caught)
        {
            var spin = new[] { "⣾", "⣽", "⣻", "⢿", "⡿", "⣟", "⣯", "⣷" };
            bool done = false;
            Exception localErr = null;

            var t = new Thread(() => {
                try { work(); }
                catch (Exception ex) { localErr = ex; }
                finally { done = true; }
            });
            t.Start();

            int f = 0;
            while (!done)
            {
                Console.SetCursorPosition(_LP, taskRow);
                Console.ForegroundColor = C_CYAN;
                Console.Write($"  {icon}  {label}  ");
                Console.ForegroundColor = C_YELLOW;
                Console.Write(spin[f++ % spin.Length] + "     ");
                Thread.Sleep(70);
            }
            t.Join();
            caught = localErr;
        }

        private static void UpdateBar(int barRow, int inner, float pct, string task)
        {
            int filled = (int)(inner * pct);
            int p = (int)(pct * 100);

            // Balken-Inhalt
            string bar = new string('█', Math.Max(0, filled - 1))
                       + (filled > 0 ? (pct >= 1f ? "█" : "▓") : "")
                       + new string('░', inner - filled);

            // Balken-Zeile (barRow + 1 = innere Zeile)
            Console.SetCursorPosition(_LP, barRow + 1);
            Console.ForegroundColor = pct >= 1f ? C_GREEN : pct >= .6f ? C_DCYAN : C_CYAN;
            Console.Write("  │" + bar.Substring(0, Math.Min(bar.Length, inner)) + "│");

            // Status-Zeile (barRow + 3)
            Console.SetCursorPosition(_LP, barRow + 3);
            Console.ForegroundColor = pct >= 1f ? C_GREEN : C_YELLOW;
            string status = $"  [ {p,3}% ]  {(pct >= 1f ? "✔  Alle Systeme bereit!" : task + "...")}";
            Console.Write(status.PadRight(inner + 6));

            Console.ResetColor();
        }

        // ═════════════════════════════════════════════════════════
        // PHASE 4 — FINALE BOX
        // ═════════════════════════════════════════════════════════
        private static void PhaseFinale()
        {
            Thread.Sleep(200);
            Blank();
            Blank();

            int bw = BLOCK_W - 2;

            string[] box = {
                "╔" + new string('═', bw) + "╗",
                "║" + new string(' ', bw) + "║",
                "║" + Center("✅   ALLE SYSTEME BEREIT  —  ANMELDUNG WIRD GESTARTET", bw) + "║",
                "║" + new string(' ', bw) + "║",
                "║" + Center(
                    $"📦  {DataManager.Inventar.Count,4} Artikel" +
                    $"    ·    👥  {DataManager.Mitarbeiter.Count,3} Mitarbeiter" +
                    $"    ·    👤  {DataManager.Benutzer.Count,3} Benutzer",
                    bw) + "║",
                "║" + new string(' ', bw) + "║",
                "║" + Center("🤖  KI-Engine 2.0    ·    🔐  AES-256    ·    📊  v2.0 Production", bw) + "║",
                "║" + new string(' ', bw) + "║",
                "╚" + new string('═', bw) + "╝",
            };

            foreach (var line in box)
            {
                Ln(line, C_GREEN);
                Thread.Sleep(52);
            }

            Console.ResetColor();

            // Pulsierender Hinweis
            Blank();
            string hint = "──  Anmeldung wird vorbereitet  ──";
            int hintRow = Console.CursorTop;

            var pulse = new[] { C_GRAY, C_DCYAN, C_CYAN, C_WHITE, C_CYAN, C_DCYAN, C_GRAY };
            foreach (var col in pulse)
            {
                Console.SetCursorPosition(0, hintRow);
                Console.ForegroundColor = col;
                // zentriert ausgeben
                string line = hint.PadLeft(_LP + hint.Length / 2 + hint.Length / 2);
                Console.Write(line.PadRight(_W));
                Thread.Sleep(85);
            }

            Console.ResetColor();
            Thread.Sleep(230);
        }

        // ═════════════════════════════════════════════════════════
        // QUICK RELOAD
        // ═════════════════════════════════════════════════════════
        public static void QuickReload()
        {
            Console.Clear();
            Console.CursorVisible = false;

            _W = Math.Max(Console.WindowWidth - 1, 80);
            _H = Math.Max(Console.WindowHeight - 1, 20);
            _LP = (_W - BLOCK_W) / 2;

            int bw = BLOCK_W - 2;
            Blank(); Blank();
            Ln("╔" + new string('═', bw) + "╗", C_CYAN);
            Ln("║" + Center("  🔄  DATEN WERDEN AKTUALISIERT", bw) + "║", C_CYAN);
            Ln("╚" + new string('═', bw) + "╝", C_CYAN);
            Blank();

            var spin = new[] { "⣾", "⣽", "⣻", "⢿", "⡿", "⣟", "⣯", "⣷" };
            int spinRow = Console.CursorTop;
            bool done = false; int f = 0;

            var t = new Thread(() => {
                DataManager.LoadBenutzer();
                DataManager.LoadMitarbeiter();
                DataManager.LoadInventar();
                KIEngine.Initialisiere();
                done = true;
            });
            t.Start();

            while (!done)
            {
                Console.SetCursorPosition(_LP, spinRow);
                Console.ForegroundColor = C_CYAN;
                Console.Write($"  {spin[f++ % spin.Length]}  Lade Daten...        ");
                Thread.Sleep(80);
            }
            t.Join();

            Console.SetCursorPosition(_LP, spinRow);
            Console.ForegroundColor = C_GREEN;
            Console.WriteLine("  ✅  Daten erfolgreich aktualisiert!       ");
            Console.ResetColor();
            Console.CursorVisible = true;
            Thread.Sleep(600);
        }

        // ═════════════════════════════════════════════════════════
        // INTERNE HELFER
        // ═════════════════════════════════════════════════════════

        /// Schreibt eine zentrierte Zeile via WriteLine (kein SetCursorPosition im Layout)
        private static void Ln(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            // Linker Rand für Zentrierung
            string line = new string(' ', _LP) + text;
            if (line.Length > _W) line = line.Substring(0, _W);
            Console.WriteLine(line);
        }

        /// Schreibt eine zentrierte Zeile mit Typewriter-Effekt
        private static void LnType(string text, ConsoleColor color, int delay = 12)
        {
            Console.ForegroundColor = color;
            Console.Write(new string(' ', _LP));
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
            Console.WriteLine();
        }

        /// Leerzeile
        private static void Blank()
        {
            Console.WriteLine();
        }

        /// Text linksbündig in Feld der Breite w einpassen (Leerzeichen-Padding rechts)
        private static string Center(string text, int w)
        {
            // Berechne sichtbare Länge (Emojis ~doppelt)
            int vl = 0;
            foreach (char c in text) vl += c > 0xFF ? 2 : 1;
            if (vl >= w) return text.Length > w ? text.Substring(0, w) : text;
            int lp = (w - vl) / 2;
            int rp = w - vl - lp;
            return new string(' ', lp) + text + new string(' ', rp);
        }

        private static void InitialisiereSystem()
        {
            FileManager.HideAllFiles();
            ConsoleHelper.SetupConsole();
        }

        private static void PruefeSystem() => Thread.Sleep(150);
    }
}