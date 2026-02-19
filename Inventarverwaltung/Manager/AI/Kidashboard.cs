using Inventarverwaltung.Manager.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Timers;

namespace Inventarverwaltung.Manager.AI
{
    public static class KIDashboard
    {
        // ═══════════════════════════════════════════════════
        // PERSISTENZ
        // ═══════════════════════════════════════════════════
        private static readonly string ConfigPfad = Path.Combine(Environment.CurrentDirectory, "KI_Config.dat");
        private static readonly string StatsPfad = Path.Combine(Environment.CurrentDirectory, "KI_Stats.dat");

        private static KIConfig cfg;
        private static KIStatistiken stat;
        private static readonly Random rnd = new Random();

        private static int _uhrzeitRow = 1;
        private static bool _uhrzeitLauft = false;
        private static Thread _uhrzeitThread;
        // ═══════════════════════════════════════════════════
        // FARBEN
        // ═══════════════════════════════════════════════════
        private static readonly ConsoleColor CP = ConsoleColor.Cyan;
        private static readonly ConsoleColor CA = ConsoleColor.Yellow;
        private static readonly ConsoleColor CS = ConsoleColor.Green;
        private static readonly ConsoleColor CW = ConsoleColor.Yellow;
        private static readonly ConsoleColor CE = ConsoleColor.Red;
        private static readonly ConsoleColor CD = ConsoleColor.DarkGray;
        private static readonly ConsoleColor CX = ConsoleColor.White;
        private static readonly ConsoleColor CM = ConsoleColor.Magenta;

        // ═══════════════════════════════════════════════════
        // EINSTIEG
        // ═══════════════════════════════════════════════════
        public static void ZeigeKIControlCenter()
        {
            LadeAlles();
            AnimBoot();

            bool run = true;
            while (run)
            {
                ZeigeHauptDashboard();
                string wahl = ConsoleHelper.GetInput("▶ Auswahl");
                StopLiveUhr();
                switch (wahl)
                {
                    case "1": MenuEinstellungen(); break;
                    case "2": MenuModus(); break;
                    case "3": MenuFunktionen(); break;
                    case "4": MenuDetailStats(); break;
                    case "5": MenuInsights(); break;
                    case "6": StatsReset(); break;
                    case "0": run = false; break;
                }
            }
        }

        // ═══════════════════════════════════════════════════
        //  BOOT-ANIMATION
        // ═══════════════════════════════════════════════════
        private static void AnimBoot()
        {
            Console.Clear();
            Console.CursorVisible = false;
            int w = Math.Min(Console.WindowWidth - 4, 88);

            // Phase 1 – Partikel-Regen
            PhasePartikel();

            // Phase 2 – Scan-Linie
            Console.Clear();
            PhaseScan(w);

            // Phase 3 – Logo Typewriter
            PhaseLogoBuild(w);

            // Phase 4 – Boot-Sequenz
            PhaseBootSeq();

            // Phase 5 – RGB-Progress
            PhaseRGBProgress(w);

            // Phase 6 – Finale Pulse
            PhaseFinalPulse();

            Thread.Sleep(400);
            Console.CursorVisible = true;
            Console.Clear();
        }

        private static void PhasePartikel()
        {
            int w = Math.Min(Console.WindowWidth, 78);
            int h = Math.Min(Console.WindowHeight, 20);
            var chars = "01█▓▒░◆◇★☆✦✧◈▪▫".ToCharArray();
            var colors = new[] { ConsoleColor.DarkGreen, ConsoleColor.Green,
                                  ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.White };
            for (int f = 0; f < 30; f++)
            {
                for (int i = 0; i < 14; i++)
                {
                    int x = rnd.Next(w); int y = rnd.Next(h);
                    try { Console.SetCursorPosition(x, y); } catch { continue; }
                    Console.ForegroundColor = colors[rnd.Next(colors.Length)];
                    Console.Write(chars[rnd.Next(chars.Length)]);
                }
                Thread.Sleep(10);
            }
            Console.ResetColor();
        }

        private static void PhaseScan(int w)
        {
            int h = Math.Min(Console.WindowHeight - 2, 22);
            for (int y = 0; y < h; y++)
            {
                try { Console.SetCursorPosition(0, y); } catch { break; }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  " + new string('▬', w));
                Thread.Sleep(16);
                try { Console.SetCursorPosition(0, y); } catch { }
                Console.Write(new string(' ', w + 4));
            }
        }

        private static void PhaseLogoBuild(int w)
        {
            Console.Clear();
            Console.WriteLine();

            string[] logo = {
                "╔" + new string('═', w) + "╗",
                "║" + Ctr("", w) + "║",
                "║" + Ctr("██╗  ██╗██╗      ██████╗ ██████╗ ███████╗", w) + "║",
                "║" + Ctr("██║ ██╔╝██║     ██╔════╝██╔═══██╗██╔════╝", w) + "║",
                "║" + Ctr("█████╔╝ ██║     ██║     ██║   ██║███████╗", w) + "║",
                "║" + Ctr("██╔═██╗ ██║     ██║     ██║   ██║╚════██║", w) + "║",
                "║" + Ctr("██║  ██╗███████╗╚██████╗╚██████╔╝███████║", w) + "║",
                "║" + Ctr("╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝ ╚══════╝", w) + "║",
                "║" + Ctr("", w) + "║",
                "║" + Ctr("◈━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━◈", w) + "║",
                "║" + Ctr("  🤖  C O N T R O L   C E N T E R   3 . 0  🤖  ", w) + "║",
                "║" + Ctr("      K Ü N S T L I C H E   I N T E L L I G E N Z", w) + "║",
                "║" + Ctr("◈━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━◈", w) + "║",
                "║" + Ctr("", w) + "║",
                "╚" + new string('═', w) + "╝",
            };
            ConsoleColor[] grad = {
                ConsoleColor.DarkBlue, ConsoleColor.Blue, ConsoleColor.DarkCyan, ConsoleColor.Cyan,
                ConsoleColor.Blue, ConsoleColor.DarkCyan, ConsoleColor.Cyan, ConsoleColor.Blue,
                ConsoleColor.DarkBlue, ConsoleColor.DarkCyan, ConsoleColor.Yellow, ConsoleColor.White,
                ConsoleColor.DarkCyan, ConsoleColor.DarkBlue, ConsoleColor.DarkBlue
            };

            for (int i = 0; i < logo.Length; i++)
            {
                Console.ForegroundColor = grad[i % grad.Length];
                foreach (char c in "  " + logo[i]) { Console.Write(c); Thread.Sleep(1); }
                Console.WriteLine();
            }
            Console.ResetColor();
            Thread.Sleep(40);
        }

        private static void PhaseBootSeq()
        {
            Console.WriteLine();
            var phasen = new[]
            {
                new { T = "Neural Network Interface v3.0",    P = 100 },
                new { T = "Deep Learning Engine Core",        P = 100 },
                new { T = "Pattern Recognition Module",       P = 98  },
                new { T = "Predictive Analytics System",      P = 100 },
                new { T = "NLP Tokenization & Synonyms",      P = 97  },
                new { T = "Anomaly Detection Engine",         P = 100 },
                new { T = "Fuzzy Search Index Builder",       P = 99  },
                new { T = "Confidence Scoring Layer",         P = 100 },
                new { T = "Historical Data Analyzer",         P = 100 },
                new { T = "Auto-Calibration Module",          P = 96  },
            };
            string[] sp = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

            foreach (var p in phasen)
            {
                for (int i = 0; i < 7; i++)
                {
                    Console.SetCursorPosition(2, Console.CursorTop);
                    Console.ForegroundColor = CP;
                    Console.Write($"{sp[i % sp.Length]} ");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"Loading  {p.T,-40}");
                    Console.ResetColor();
                    Thread.Sleep(42);
                }
                Console.SetCursorPosition(2, Console.CursorTop);
                Console.ForegroundColor = CS; Console.Write("✓ ");
                Console.ForegroundColor = CX; Console.Write($"{p.T,-40}");
                Console.ForegroundColor = CD; Console.Write(" [");
                Console.ForegroundColor = p.P == 100 ? CS : CW;
                Console.Write($"{p.P,3}%");
                Console.ForegroundColor = CD; Console.Write("]  ");
                Console.ForegroundColor = p.P == 100 ? CS : CW;
                Console.Write(new string('▪', p.P / 10));
                Console.ForegroundColor = CD;
                Console.Write(new string('▫', 10 - p.P / 10));
                Console.ResetColor();
                Console.WriteLine();
                Thread.Sleep(75);
            }
        }

        private static void PhaseRGBProgress(int w)
        {
            Console.WriteLine();
            int barW = Math.Min(62, w - 18);
            ConsoleColor[] rgb = {
                ConsoleColor.Red, ConsoleColor.DarkRed, ConsoleColor.Yellow, ConsoleColor.DarkYellow,
                ConsoleColor.Green, ConsoleColor.DarkGreen, ConsoleColor.Cyan, ConsoleColor.DarkCyan,
                ConsoleColor.Blue, ConsoleColor.DarkBlue, ConsoleColor.Magenta, ConsoleColor.DarkMagenta
            };

            Console.ForegroundColor = CP;
            Console.WriteLine($"  ╔{new string('═', barW + 14)}╗");
            Console.WriteLine($"  ║{Ctr("◈  DASHBOARD WIRD GELADEN  ◈", barW + 14)}║");
            Console.WriteLine($"  ╚{new string('═', barW + 14)}╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.Write("  ▶ [");
            int left = Console.CursorLeft;
            int top = Console.CursorTop;

            for (int i = 0; i <= barW; i++)
            {
                Console.SetCursorPosition(left, top);
                for (int j = 0; j < i; j++)
                {
                    Console.ForegroundColor = rgb[j % rgb.Length];
                    Console.Write("█");
                }
                Console.ForegroundColor = CD;
                Console.Write(new string('░', barW - i));
                Console.ResetColor();
                Console.Write("] ");
                Console.ForegroundColor = CX;
                Console.Write($"{i * 100 / barW,3}% ");
                Console.ForegroundColor = i % 2 == 0 ? CP : ConsoleColor.DarkCyan;
                Console.Write("◈");
                Console.ResetColor();
                Thread.Sleep(20);
            }
            Console.WriteLine();
        }

        private static void PhaseFinalPulse()
        {
            Console.WriteLine();
            string[] frames = {
                "           ◈  BEREIT  ◈           ",
                "        ◆ ◈  BEREIT  ◈ ◆        ",
                "     ✦ ◆ ◈  SYSTEM BEREIT  ◈ ◆ ✦     ",
                "  ★ ✦ ◆ ◈  SYSTEM BEREIT  ◈ ◆ ✦ ★  ",
                "✨ ★ ✦ ◆ ◈  KI SYSTEM BEREIT  ◈ ◆ ✦ ★ ✨",
                "🌟 ✨ ★ ✦ ◈  KI CONTROL CENTER 3.0  ◈ ✦ ★ ✨ 🌟",
            };
            ConsoleColor[] cols = { CD, ConsoleColor.DarkCyan, CP, CX, CA, CS };
            int lineY = Console.CursorTop;

            for (int i = 0; i < frames.Length; i++)
            {
                try { Console.SetCursorPosition(0, lineY); } catch { }
                Console.Write(new string(' ', Console.WindowWidth - 1));
                try { Console.SetCursorPosition(0, lineY); } catch { }
                Console.ForegroundColor = cols[i];
                int pad = Math.Max(0, (Console.WindowWidth - frames[i].Length) / 2);
                Console.Write(new string(' ', pad) + frames[i]);
                Console.ResetColor();
                Thread.Sleep(190);
            }

            for (int g = 0; g < 5; g++)
            {
                try { Console.SetCursorPosition(0, lineY); } catch { }
                Console.Write(new string(' ', Console.WindowWidth - 1));
                try { Console.SetCursorPosition(0, lineY); } catch { }
                string msg = "  ◈◈◈  KI CONTROL CENTER 3.0 — AKTIVIERT  ◈◈◈  ";
                int pad = Math.Max(0, (Console.WindowWidth - msg.Length) / 2);
                if (g % 2 == 0) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CS; }
                else { Console.ForegroundColor = CS; Console.BackgroundColor = ConsoleColor.Black; }
                Console.Write(new string(' ', pad) + msg);
                Console.ResetColor();
                Thread.Sleep(230);
            }
            Console.WriteLine();

        }
        private static void StartLiveUhr()
        {
            StopLiveUhr();
            _uhrzeitLauft = true;
            _uhrzeitThread = new Thread(() =>
            {
                while (_uhrzeitLauft)
                {
                    try
                    {
                        string zeit = DateTime.Now.ToString("HH:mm:ss");
                        int origTop = Console.CursorTop;
                        int origLeft = Console.CursorLeft;
                        Console.CursorVisible = false;
                        Console.SetCursorPosition(7, _uhrzeitRow); // 7 = nach "  ║ 🕐 "
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(zeit);
                        Console.SetCursorPosition(origLeft, origTop);
                        Console.ResetColor();
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            });
            _uhrzeitThread.IsBackground = true;
            _uhrzeitThread.Start();
        }

        private static void StopLiveUhr()
        {
            _uhrzeitLauft = false;
            _uhrzeitThread?.Join(200);
        }

        // ═══════════════════════════════════════════════════
        //  HAUPT-DASHBOARD
        // ═══════════════════════════════════════════════════
        private static void ZeigeHauptDashboard()
        {
            Console.Clear();
            int w = Math.Min(Console.WindowWidth - 4, 88);

            DashHeader(w);
            Console.WriteLine();
            DashStatus(w);
            Console.WriteLine();
            DashVorschlaegePerf(w);
            Console.WriteLine();
            DashFunktionen(w);
            Console.WriteLine();
            DashLiveMetriken(w);
            Console.WriteLine();
            DashAktionen(w);

            _uhrzeitRow = 1;
            StartLiveUhr();
        }


        private static void DashHeader(int w)
        {
            // string zeit = DateTime.Now.ToString("HH:mm:ss");
            StartLiveUhr();
            string datum = DateTime.Now.ToString("dd.MM.yyyy");
            ConsoleColor mc = cfg.Modus == KIModus.Performance ? CS
                            : cfg.Modus == KIModus.Eco ? CA : CD;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  ╔{new string('═', w)}╗");
            Console.Write("  ║");
            Console.ForegroundColor = CD; Console.Write($" 🕐 {StartLiveUhr}  {datum}");
            string mid = "🤖  KI CONTROL CENTER 3.0  🤖";
            string tag = $"[{cfg.Modus.ToString().ToUpper()}]";
            int midW = w - 22 - tag.Length - 2;
            Console.ForegroundColor = CP; Console.Write(Ctr(mid, midW));
            Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = mc;
            Console.Write($" {tag} ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkCyan; Console.WriteLine(" ║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  ╚{new string('═', w)}╝");
            Console.ResetColor();
        }

        // ── STATUS-KERN ──
        private static void DashStatus(int w)
        {
            int daten = DataManager.Inventar.Count + DataManager.Mitarbeiter.Count;
            int konfPct = (int)(cfg.KonfidenzLevel * 100);
            int lernPct = Math.Min(100, daten * 2);

            BoxTop(w, "📡  SYSTEM-STATUS", CP);

            // KI + Lernen + Uptime + Datenpunkte
            BoxRow(w, () =>
            {
                Badge("KI-SYSTEM", cfg.KIAktiv, 14);
                Console.Write("   ");
                Badge("AUTO-LERNEN", cfg.AutoLernen, 12);
                Kv("   Uptime", $"{stat.UptimeStunden}h {stat.UptimeMinuten}m");
                Kv("   Datenpunkte", $"{daten}");
                Kv("   Sessions", $"{stat.Sitzungen}");
            });
            BoxDiv(w);

            // Konfidenz-Balken
            BoxRow(w, () =>
            {
                Console.ForegroundColor = CD; Console.Write("📈 Konfidenz     "); Console.ResetColor();
                GradBar(konfPct, 28);
                ConsoleColor kc = konfPct >= 70 ? CS : konfPct >= 50 ? CW : CE;
                Console.ForegroundColor = kc; Console.Write($"  {konfPct,3}%   ");
                Console.ForegroundColor = CD; Console.Write("Lernfortschritt  "); Console.ResetColor();
                SolidBar(lernPct, 18, CM);
                Console.ForegroundColor = CM; Console.Write($"  {lernPct,3}%"); Console.ResetColor();
            });

            // Aktiver Modus
            BoxRow(w, () =>
            {
                Console.ForegroundColor = CD; Console.Write("⚡ Aktiver Modus  "); Console.ResetColor();
                ConsoleColor mc = cfg.Modus == KIModus.Performance ? CS
                                : cfg.Modus == KIModus.Eco ? CA : CD;
                string modusTxt = cfg.Modus.ToString().ToUpper();
                string modusDesc = cfg.Modus == KIModus.Performance
                    ? "Alle Features aktiv · Höchste Genauigkeit · Max-Ressourcen"
                    : cfg.Modus == KIModus.Eco
                    ? "Ausgewogen · Standard-Features · Moderate Ressourcen"
                    : "Nur Basis · Schnelle Antworten · Minimale Ressourcen";
                Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = mc;
                Console.Write($" {modusTxt} "); Console.ResetColor();
                Console.ForegroundColor = CD; Console.Write($"  {modusDesc}"); Console.ResetColor();
            });
            BoxBottom(w);
        }

        // ── VORSCHLÄGE + PERFORMANCE ──
        private static void DashVorschlaegePerf(int w)
        {
            int ges = stat.VorschlaegeGesamt;
            int uen = stat.VorschlaegeUebernommen;
            int abl = stat.VorschlaegeAbgelehnt;
            int fal = stat.VorschlaegeFalsch;
            int auf = Math.Max(0, ges - uen - abl - fal);
            float eq = ges > 0 ? uen * 100f / ges : 0f;

            BoxTop(w, "📊  VORSCHLÄGE & PERFORMANCE", CM);

            // Zahlen
            BoxRow(w, () =>
            {
                Stat("📝 Gesamt", $"{ges}", CX);
                Stat("✅ Übernommen", $"{uen}", CS);
                Stat("❌ Abgelehnt", $"{abl}", CE);
                Stat("⚠  Falsch", $"{fal}", CW);
                Stat("⏳ Offen", $"{auf}", CD);
            });
            BoxDiv(w);

            // Balken je Kategorie
            if (ges > 0)
            {
                BoxRow(w, () =>
                {
                    int up = uen * 100 / ges; int ap = abl * 100 / ges; int fp = fal * 100 / ges;
                    Console.ForegroundColor = CS; Console.Write("✅ "); SolidBar(up, 14, CS);
                    Console.ForegroundColor = CS; Console.Write($" {up,3}%   ");
                    Console.ForegroundColor = CE; Console.Write("❌ "); SolidBar(ap, 14, CE);
                    Console.ForegroundColor = CE; Console.Write($" {ap,3}%   ");
                    Console.ForegroundColor = CW; Console.Write("⚠  "); SolidBar(fp, 14, CW);
                    Console.ForegroundColor = CW; Console.Write($" {fp,3}%");
                    Console.ResetColor();
                });
                BoxDiv(w);
            }

            // Erfolgsquote
            BoxRow(w, () =>
            {
                Console.ForegroundColor = CD; Console.Write("🎯 Erfolgsquote  "); Console.ResetColor();
                GradBar((int)eq, 32);
                ConsoleColor ec = eq >= 70 ? CS : eq >= 50 ? CW : CE;
                Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = ec;
                Console.Write($"  {eq,5:F1}%  "); Console.ResetColor();
                string bew = eq >= 70 ? "Ausgezeichnet ★★★" : eq >= 50 ? "Gut ★★☆" : "Verbesserbar ★☆☆";
                Console.ForegroundColor = CD; Console.Write($"  {bew}"); Console.ResetColor();
            });
            BoxDiv(w);

            // Performance-Metriken
            BoxRow(w, () =>
            {
                Perf("⏱ Ø Antwort", $"{stat.AntwortZeitMs}ms",
                     stat.AntwortZeitMs < 50 ? CS : stat.AntwortZeitMs < 100 ? CW : CE);
                Perf("⚡ Schnellste", $"{stat.SchnellsteMs}ms", CS);
                Perf("🐌 Langsamste", $"{stat.LangsamsteMs}ms", CW);
                Perf("💾 RAM", $"~{stat.RamMB}MB",
                     stat.RamMB < 10 ? CS : stat.RamMB < 25 ? CW : CE);
                Perf("🔄 Analysen", $"{stat.GesamtAnalysen}", CP);
                Perf("📅 Sitzung", $"{stat.SitzungsMinuten}m", CD);
            });
            BoxBottom(w);
        }

        // ── FUNKTIONEN GRID ──
        private static void DashFunktionen(int w)
        {
            BoxTop(w, "⚙️   FUNKTIONEN ÜBERSICHT", CA);

            var fns = new[]
            {
                new { N="🎯 Mitarbeiter-Vorschläge", A=cfg.VorschlaegeAktiv,      H=stat.VorschlaegeGesamt    },
                new { N="💰 Preis-Vorhersage",       A=cfg.PreisAktiv,             H=stat.PreiseVorhergesagt   },
                new { N="⚠️  Anomalie-Erkennung",    A=cfg.AnomalieAktiv,          H=stat.AnomalienErkannt     },
                new { N="🔍 Fuzzy Search",           A=cfg.FuzzyAktiv,             H=stat.FuzzySearches        },
                new { N="✏️  Auto-Korrektur",        A=cfg.AutoKorrekturAktiv,     H=stat.AutoKorrekturen      },
                new { N="📈 Trend-Analyse",          A=cfg.TrendAktiv,             H=stat.TrendAnalysen        },
                new { N="🔮 Bedarfsvorhersage",      A=cfg.BedarfsvorhersageAktiv, H=stat.Bedarfsvorhersagen   },
                new { N="🏷️  Inventar-Nummern",      A=cfg.InvNrAktiv,             H=stat.InvNrVorschlaege     },
            };

            for (int row = 0; row < 2; row++)
            {
                int from = row * 4;
                BoxRow(w, () =>
                {
                    for (int c = 0; c < 4 && from + c < fns.Length; c++)
                    {
                        var fn = fns[from + c];
                        if (fn.A) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CS; Console.Write(" ON "); }
                        else { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CD; Console.Write(" -- "); }
                        Console.ResetColor();
                        Console.ForegroundColor = fn.A ? CX : CD;
                        Console.Write($" {fn.N,-26}");
                        Console.ForegroundColor = CD;
                        Console.Write($"×{fn.H,-5}  ");
                        Console.ResetColor();
                    }
                });
            }
            BoxBottom(w);
        }

        // ── LIVE METRIKEN ──
        private static void DashLiveMetriken(int w)
        {
            int inv = DataManager.Inventar.Count;
            int ma = DataManager.Mitarbeiter.Count;
            int usr = DataManager.Benutzer.Count;
            decimal wert = DataManager.Inventar.Sum(a => a.Anzahl * a.Preis);
            int krit = DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand);
            int ok = inv - krit;
            int gesundPct = inv > 0 ? ok * 100 / inv : 100;

            BoxTop(w, "🧠  KI-ANALYTIK & LIVE-METRIKEN", CP);

            // Inventar-Zahlen
            BoxRow(w, () =>
            {
                Stat("📦 Artikel", $"{inv}", CP);
                Stat("👥 Mitarbeiter", $"{ma}", ConsoleColor.Cyan);
                Stat("👤 Benutzer", $"{usr}", CM);
                Stat("💰 Gesamtwert", $"{wert:C0}", CA);
                Stat("🔴 Kritisch", $"{krit}", krit > 0 ? CE : CS);
                Stat("🟢 OK", $"{ok}", CS);
            });
            BoxDiv(w);

            // Bestandsgesundheit Balken
            BoxRow(w, () =>
            {
                Console.ForegroundColor = CD; Console.Write("🏥 Bestandsgesundheit   "); Console.ResetColor();
                GradBar(gesundPct, 38);
                ConsoleColor gc = gesundPct >= 80 ? CS : gesundPct >= 60 ? CW : CE;
                Console.ForegroundColor = gc; Console.Write($"  {gesundPct,3}%  ");
                Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = gc;
                string gl = gesundPct >= 80 ? " OPTIMAL " : gesundPct >= 60 ? " AKZEPTABEL " : " KRITISCH ";
                Console.Write(gl); Console.ResetColor();
            });
            BoxDiv(w);

            // KI-Aktivitäts-Stats
            BoxRow(w, () =>
            {
                Stat("📊 Analysiert", $"{stat.AnalysierteArtikel}", CP);
                Stat("💡 KI-Anfragen", $"{stat.GesamtAnalysen}", CA);
                Stat("🔎 Suchen", $"{stat.FuzzySearches}", ConsoleColor.Cyan);
                Stat("🛠 Korrekturen", $"{stat.AutoKorrekturen}", CW);
                Stat("📈 Trends", $"{stat.TrendAnalysen}", CM);
                Stat("🔮 Vorhersagen", $"{stat.Bedarfsvorhersagen}", CP);
            });
            BoxDiv(w);

            // Konfig-Zeile
            BoxRow(w, () =>
            {
                Console.ForegroundColor = CD; Console.Write("🎛  Konfig: "); Console.ResetColor();
                Kv("Konfidenz", $"{cfg.KonfidenzSchwelle:P0}");
                Kv("  Schwelle", $"{cfg.LernSchwelle}");
                Kv("  Letzte Analyse", $"vor {stat.MinutenSeitAnalyse}min");
                Kv("  Letzter Reset", stat.LetzterReset);
            });
            BoxBottom(w);
        }

        // ── AKTIONSLEISTE ──
        private static void DashAktionen(int w)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  ┌{new string('─', w)}┐");
            Console.Write("  │  ");

            string[] optTxt = { "[1] Einstellungen", "[2] Modus", "[3] Funktionen",
                                  "[4] Statistiken",  "[5] Insights", "[6] Reset", "[0] Zurück" };
            ConsoleColor[] optCol = { CA, CS, CM, CP, ConsoleColor.Cyan, CW, CE };

            int used = 4;
            for (int i = 0; i < optTxt.Length; i++)
            {
                Console.ForegroundColor = optCol[i];
                Console.Write($"  {optTxt[i]}");
                used += optTxt[i].Length + 2;
            }
            Console.Write(new string(' ', Math.Max(0, w - used)));
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  │");
            Console.WriteLine($"  └{new string('─', w)}┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════
        //  EINSTELLUNGEN
        // ═══════════════════════════════════════════════════
        private static void MenuEinstellungen()
        {
            bool weiter = true;
            while (weiter)
            {
                Console.Clear();
                int w = Math.Min(Console.WindowWidth - 4, 88);
                BoxTop(w, "🔧  EINSTELLUNGEN", CA);
                BoxRow(w, () => EinstellRow("[1]", "KI-System", cfg.KIAktiv ? "● AKTIV" : "○ INAKTIV", cfg.KIAktiv ? CS : CD));
                BoxRow(w, () => EinstellRow("[2]", "Auto-Lernen", cfg.AutoLernen ? "● EIN" : "○ AUS", cfg.AutoLernen ? CS : CD));
                BoxDiv(w);
                BoxRow(w, () => EinstellRow("[3]", "Konfidenz-Schwelle", $"{cfg.KonfidenzSchwelle:P0}", CP));
                BoxRow(w, () => EinstellRow("[4]", "Lern-Schwelle", $"{cfg.LernSchwelle} Datenpunkte", CP));
                BoxBottom(w);
                Console.WriteLine("\n  [0] ← Zurück");
                string wahl = ConsoleHelper.GetInput("▶ Auswahl");
                switch (wahl)
                {
                    case "1": cfg.KIAktiv = !cfg.KIAktiv; AnimToggle("KI-System", cfg.KIAktiv); if (cfg.KIAktiv) KIEngine.Initialisiere(); break;
                    case "2": cfg.AutoLernen = !cfg.AutoLernen; AnimToggle("Auto-Lernen", cfg.AutoLernen); break;
                    case "3": WaehleKonfidenz(); break;
                    case "4": WaehlelernSchwelle(); break;
                    case "0": weiter = false; break;
                }
                SpeichereAlles();
            }
        }

        // ═══════════════════════════════════════════════════
        //  MODUS
        // ═══════════════════════════════════════════════════
        private static void MenuModus()
        {
            Console.Clear();
            int w = Math.Min(Console.WindowWidth - 4, 88);
            BoxTop(w, "🎛️   MODUS WÄHLEN", CA);

            var modi = new[]
            {
                new { Id="1", M=KIModus.Performance, Icon="⚡", Titel="PERFORMANCE",
                      Desc="Alle Features · Max-Genauigkeit · ~12MB",
                      Det=new[]{"✓ Alle 8 Funktionen","✓ Konfidenz: 85%","✓ Schwelle: 4","✓ Beste Ergebnisse"} },
                new { Id="2", M=KIModus.Eco,         Icon="🌿", Titel="ECO",
                      Desc="Ausgewogen · 5 Features · ~7MB",
                      Det=new[]{"✓ 5 Funktionen","✓ Konfidenz: 70%","✓ Schwelle: 3","✓ Täglicher Betrieb"} },
                new { Id="3", M=KIModus.Minimal,     Icon="💤", Titel="MINIMAL",
                      Desc="Nur Basis · Schnell · ~3MB",
                      Det=new[]{"✓ 2 Funktionen","✓ Konfidenz: 50%","✓ Schwelle: 1","✓ Alte Hardware"} },
            };

            foreach (var m in modi)
            {
                bool ist = cfg.Modus == m.M;
                ConsoleColor mc = m.M == KIModus.Performance ? CS : m.M == KIModus.Eco ? CA : CD;
                BoxRow(w, () =>
                {
                    if (ist) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = mc; }
                    else Console.ForegroundColor = mc;
                    Console.Write($"  [{m.Id}]  {m.Icon} {m.Titel,-14}");
                    Console.ResetColor();
                    Console.ForegroundColor = CD; Console.Write($"  {m.Desc}");
                    if (ist) { Console.ForegroundColor = mc; Console.Write("  ◄ AKTIV"); }
                    Console.ResetColor();
                });
                foreach (var d in m.Det)
                {
                    BoxRow(w, () => { Console.ForegroundColor = CD; Console.Write($"             {d}"); Console.ResetColor(); });
                }
                BoxDiv(w);
            }
            BoxBottom(w);
            Console.WriteLine("  [0] ← Zurück");
            string wahl = ConsoleHelper.GetInput("▶ Modus wählen");
            KIModus alt = cfg.Modus;
            switch (wahl) { case "1": cfg.Modus = KIModus.Performance; break; case "2": cfg.Modus = KIModus.Eco; break; case "3": cfg.Modus = KIModus.Minimal; break; default: return; }
            if (alt != cfg.Modus) { WendeModusAn(); AnimModus(alt, cfg.Modus); SpeichereAlles(); }
        }

        // ═══════════════════════════════════════════════════
        //  FUNKTIONEN
        // ═══════════════════════════════════════════════════
        private static void MenuFunktionen()
        {
            bool weiter = true;
            while (weiter)
            {
                Console.Clear();
                int w = Math.Min(Console.WindowWidth - 4, 88);
                BoxTop(w, "💡  FUNKTIONEN EIN/AUSSCHALTEN", CM);

                var fns = new[]
                {
                    new { Id="1", N="🎯 Mitarbeiter-Vorschläge", A=cfg.VorschlaegeAktiv,      D="4-Faktor-Scoring (Abteilung/Verteilung/Expertise/Position)",  H=stat.VorschlaegeGesamt    },
                    new { Id="2", N="💰 Preis-Vorhersage",       A=cfg.PreisAktiv,             D="Historische Preise + Schlüsselwort-Gewichtung",               H=stat.PreiseVorhergesagt   },
                    new { Id="3", N="⚠️  Anomalie-Erkennung",    A=cfg.AnomalieAktiv,          D="Preis/Bestand/Verteilungs-Anomalien (3 Typen)",               H=stat.AnomalienErkannt     },
                    new { Id="4", N="🔍 Fuzzy Search",           A=cfg.FuzzyAktiv,             D="Levenshtein-Distanz + Synonym-Expansion",                     H=stat.FuzzySearches        },
                    new { Id="5", N="✏️  Auto-Korrektur",        A=cfg.AutoKorrekturAktiv,     D="Tippfehler via Ähnlichkeits-Score (>70%)",                    H=stat.AutoKorrekturen      },
                    new { Id="6", N="📈 Trend-Analyse",          A=cfg.TrendAktiv,             D="Kategorie-Trends, Wachstum, Bestandsgesundheit",              H=stat.TrendAnalysen        },
                    new { Id="7", N="🔮 Bedarfsvorhersage",      A=cfg.BedarfsvorhersageAktiv, D="30-Tage-Modell basierend auf kritischen Beständen",           H=stat.Bedarfsvorhersagen   },
                    new { Id="8", N="🏷️  Inventar-Nummern",      A=cfg.InvNrAktiv,             D="Pattern Recognition via Regex (IT-, BÜ-, WZ- etc.)",          H=stat.InvNrVorschlaege     },
                };

                foreach (var fn in fns)
                {
                    BoxRow(w, () =>
                    {
                        if (fn.A) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CS; Console.Write(" EIN "); }
                        else { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CD; Console.Write(" AUS "); }
                        Console.ResetColor();
                        Console.ForegroundColor = fn.A ? CX : CD;
                        Console.Write($"  [{fn.Id}] {fn.N,-30}");
                        Console.ForegroundColor = CD;
                        Console.Write($"  {fn.D,-52}  ×{fn.H}");
                        Console.ResetColor();
                    });
                }
                BoxBottom(w);
                Console.WriteLine("  [0] ← Zurück");
                string wahl = ConsoleHelper.GetInput("▶ Funktion");

                void Toggle(int idx, ref bool val, string name) { val = !val; AnimToggle(name, val); }
                bool v1 = cfg.VorschlaegeAktiv, v2 = cfg.PreisAktiv, v3 = cfg.AnomalieAktiv, v4 = cfg.FuzzyAktiv;
                bool v5 = cfg.AutoKorrekturAktiv, v6 = cfg.TrendAktiv, v7 = cfg.BedarfsvorhersageAktiv, v8 = cfg.InvNrAktiv;

                switch (wahl)
                {
                    case "1": v1 = !v1; cfg.VorschlaegeAktiv = v1; AnimToggle(fns[0].N, v1); break;
                    case "2": v2 = !v2; cfg.PreisAktiv = v2; AnimToggle(fns[1].N, v2); break;
                    case "3": v3 = !v3; cfg.AnomalieAktiv = v3; AnimToggle(fns[2].N, v3); break;
                    case "4": v4 = !v4; cfg.FuzzyAktiv = v4; AnimToggle(fns[3].N, v4); break;
                    case "5": v5 = !v5; cfg.AutoKorrekturAktiv = v5; AnimToggle(fns[4].N, v5); break;
                    case "6": v6 = !v6; cfg.TrendAktiv = v6; AnimToggle(fns[5].N, v6); break;
                    case "7": v7 = !v7; cfg.BedarfsvorhersageAktiv = v7; AnimToggle(fns[6].N, v7); break;
                    case "8": v8 = !v8; cfg.InvNrAktiv = v8; AnimToggle(fns[7].N, v8); break;
                    case "0": weiter = false; break;
                }
                SpeichereAlles();
            }
        }

        // ═══════════════════════════════════════════════════
        //  DETAIL-STATISTIKEN
        // ═══════════════════════════════════════════════════
        private static void MenuDetailStats()
        {
            Console.Clear();
            int w = Math.Min(Console.WindowWidth - 4, 88);
            BoxTop(w, "📊  DETAILLIERTE KI-STATISTIKEN", CP);

            BoxRow(w, () => { Console.ForegroundColor = CP; Console.Write("▌ VORSCHLÄGE"); Console.ResetColor(); });
            BoxRow(w, () =>
            {
                Stat("Gesamt", $"{stat.VorschlaegeGesamt}", CX);
                Stat("Übernommen", $"{stat.VorschlaegeUebernommen}", CS);
                Stat("Abgelehnt", $"{stat.VorschlaegeAbgelehnt}", CE);
                Stat("Falsch", $"{stat.VorschlaegeFalsch}", CW);
                float eq = stat.VorschlaegeGesamt > 0 ? stat.VorschlaegeUebernommen * 100f / stat.VorschlaegeGesamt : 0f;
                Stat("Erfolg", $"{eq:F1}%", eq >= 70 ? CS : eq >= 50 ? CW : CE);
            });
            BoxDiv(w);

            BoxRow(w, () => { Console.ForegroundColor = CM; Console.Write("▌ ANALYSE-AKTIVITÄT"); Console.ResetColor(); });
            BoxRow(w, () =>
            {
                Stat("Artikel", $"{stat.AnalysierteArtikel}", CP);
                Stat("Preise", $"{stat.PreiseVorhergesagt}", CA);
                Stat("Anomalien", $"{stat.AnomalienErkannt}", CE);
                Stat("Suchen", $"{stat.FuzzySearches}", CP);
                Stat("Korrekturen", $"{stat.AutoKorrekturen}", CW);
                Stat("Trends", $"{stat.TrendAnalysen}", CM);
            });
            BoxDiv(w);

            BoxRow(w, () => { Console.ForegroundColor = CA; Console.Write("▌ PERFORMANCE"); Console.ResetColor(); });
            BoxRow(w, () =>
            {
                Perf("Ø Antwort", $"{stat.AntwortZeitMs}ms", CX);
                Perf("Schnellste", $"{stat.SchnellsteMs}ms", CS);
                Perf("Langsamste", $"{stat.LangsamsteMs}ms", CW);
                Perf("RAM", $"~{stat.RamMB}MB", CP);
                Perf("Uptime", $"{stat.UptimeStunden}h", CD);
                Perf("Sessions", $"{stat.Sitzungen}", CD);
            });
            BoxDiv(w);

            BoxRow(w, () => { Console.ForegroundColor = CS; Console.Write("▌ SYSTEM-INVENTAR"); Console.ResetColor(); });
            BoxRow(w, () =>
            {
                int inv = DataManager.Inventar.Count;
                int ma = DataManager.Mitarbeiter.Count;
                decimal wert = DataManager.Inventar.Sum(a => a.Anzahl * a.Preis);
                int krit = DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand);
                Stat("Artikel", $"{inv}", CP);
                Stat("Mitarbeiter", $"{ma}", ConsoleColor.Cyan);
                Stat("Gesamtwert", $"{wert:C0}", CA);
                Stat("Kritisch", $"{krit}", krit > 0 ? CE : CS);
                Stat("OK", $"{inv - krit}", CS);
            });

            BoxBottom(w);
            ConsoleHelper.PressKeyToContinue();
        }

        private static void MenuInsights() => KIEngine.ZeigeErweiterteInsights();

        // ═══════════════════════════════════════════════════
        //  SMALL ANIMATIONS
        // ═══════════════════════════════════════════════════
        private static void AnimToggle(string name, bool an)
        {
            Console.WriteLine();
            ConsoleColor c = an ? CS : CD;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = c;
            Console.Write($"  {name}  →  {(an ? "AKTIVIERT ✓" : "DEAKTIVIERT ○")}  ");
            Console.ResetColor();
            Thread.Sleep(900);
        }

        private static void AnimModus(KIModus alt, KIModus neu)
        {
            string[] sp = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            Console.WriteLine();
            for (int i = 0; i < 22; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.ForegroundColor = CA;
                Console.Write($"  {sp[i % sp.Length]}  {alt}  ──▶  {neu}    ");
                Console.ResetColor();
                Thread.Sleep(48);
            }
            Console.WriteLine();
            Console.ForegroundColor = CS;
            Console.WriteLine($"  ✓ Modus gewechselt: {alt} ──▶ {neu}");
            Console.ForegroundColor = CD;
            Console.WriteLine($"    Konfidenz: {cfg.KonfidenzSchwelle:P0}  ·  Schwelle: {cfg.LernSchwelle}  ·  Funktionen angepasst");
            Console.ResetColor();
            Thread.Sleep(1400);
        }

        private static void WaehleKonfidenz()
        {
            Console.WriteLine("\n  [1] 50%  [2] 65%  [3] 75%  [4] 85%");
            switch (ConsoleHelper.GetInput("Schwelle"))
            {
                case "1": cfg.KonfidenzSchwelle = 0.50f; break;
                case "2": cfg.KonfidenzSchwelle = 0.65f; break;
                case "3": cfg.KonfidenzSchwelle = 0.75f; break;
                case "4": cfg.KonfidenzSchwelle = 0.85f; break;
            }
            AnimToggle($"Konfidenz → {cfg.KonfidenzSchwelle:P0}", true);
        }

        private static void WaehlelernSchwelle()
        {
            Console.WriteLine("\n  [1] 1  [2] 2  [3] 3  [4] 4 Datenpunkte");
            switch (ConsoleHelper.GetInput("Schwelle"))
            {
                case "1": cfg.LernSchwelle = 1; break;
                case "2": cfg.LernSchwelle = 2; break;
                case "3": cfg.LernSchwelle = 3; break;
                case "4": cfg.LernSchwelle = 4; break;
            }
            AnimToggle($"Lern-Schwelle → {cfg.LernSchwelle}", true);
        }

        private static void StatsReset()
        {
            Console.WriteLine();
            Console.ForegroundColor = CW;
            Console.WriteLine("  ⚠️  Alle Statistiken wirklich zurücksetzen? (ja/nein)");
            Console.ResetColor();
            if (ConsoleHelper.GetInput("▶").Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                stat = new KIStatistiken();
                SpeichereAlles();
                Console.ForegroundColor = CS;
                Console.WriteLine("  ✓ Statistiken zurückgesetzt!");
                Console.ResetColor();
                Thread.Sleep(1100);
            }
        }

        private static void WendeModusAn()
        {
            switch (cfg.Modus)
            {
                case KIModus.Performance:
                    cfg.VorschlaegeAktiv = cfg.PreisAktiv = cfg.AnomalieAktiv = true;
                    cfg.FuzzyAktiv = cfg.AutoKorrekturAktiv = cfg.TrendAktiv = true;
                    cfg.BedarfsvorhersageAktiv = cfg.InvNrAktiv = true;
                    cfg.KonfidenzSchwelle = 0.85f; cfg.LernSchwelle = 4; break;
                case KIModus.Eco:
                    cfg.VorschlaegeAktiv = cfg.PreisAktiv = cfg.AnomalieAktiv = true;
                    cfg.InvNrAktiv = true;
                    cfg.FuzzyAktiv = cfg.AutoKorrekturAktiv = cfg.TrendAktiv = false;
                    cfg.BedarfsvorhersageAktiv = false;
                    cfg.KonfidenzSchwelle = 0.70f; cfg.LernSchwelle = 3; break;
                case KIModus.Minimal:
                    cfg.VorschlaegeAktiv = cfg.InvNrAktiv = true;
                    cfg.PreisAktiv = cfg.AnomalieAktiv = cfg.FuzzyAktiv = false;
                    cfg.AutoKorrekturAktiv = cfg.TrendAktiv = cfg.BedarfsvorhersageAktiv = false;
                    cfg.KonfidenzSchwelle = 0.50f; cfg.LernSchwelle = 1; break;
            }
        }

        // ═══════════════════════════════════════════════════
        //  BOX HELPERS
        // ═══════════════════════════════════════════════════
        private static void BoxTop(int w, string titel, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.WriteLine($"  ╔{new string('═', w)}╗");
            Console.WriteLine($"  ║{Ctr($"◈  {titel}  ◈", w)}║");
            Console.WriteLine($"  ╠{new string('═', w)}╣");
            Console.ResetColor();
        }

        private static void BoxBottom(int w)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"  ╚{new string('═', w)}╝");
            Console.ResetColor();
        }

        private static void BoxDiv(int w)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ╟{new string('─', w)}╢");
            Console.ResetColor();
        }

        private static void BoxRow(int w, Action content)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  ║  ");
            Console.ResetColor();
            int before = Console.CursorLeft;
            content();
            int used = Console.CursorLeft - before;
            Console.Write(new string(' ', Math.Max(0, w - used - 3)));
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("║");
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════
        //  WIDGET HELPERS
        // ═══════════════════════════════════════════════════
        private static void GradBar(int pct, int bw)
        {
            pct = Math.Max(0, Math.Min(100, pct));
            int fill = pct * bw / 100;
            ConsoleColor[] g = {
                ConsoleColor.Red, ConsoleColor.DarkRed, ConsoleColor.Yellow, ConsoleColor.DarkYellow,
                ConsoleColor.Green, ConsoleColor.DarkGreen, ConsoleColor.Cyan, ConsoleColor.DarkCyan
            };
            Console.Write("[");
            for (int i = 0; i < bw; i++)
            {
                Console.ForegroundColor = i < fill ? g[i * g.Length / bw] : CD;
                Console.Write(i < fill ? "█" : "░");
            }
            Console.ResetColor();
            Console.Write("]");
        }

        private static void SolidBar(int pct, int bw, ConsoleColor c)
        {
            pct = Math.Max(0, Math.Min(100, pct));
            int fill = pct * bw / 100;
            Console.Write("[");
            Console.ForegroundColor = c;
            Console.Write(new string('█', fill));
            Console.ForegroundColor = CD;
            Console.Write(new string('░', bw - fill));
            Console.ResetColor();
            Console.Write("]");
        }

        private static void Badge(string label, bool an, int lw)
        {
            if (an) { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CS; }
            else { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = CD; }
            Console.Write($" {(an ? "●" : "○")} {label} ");
            Console.ResetColor();
        }

        private static void Stat(string label, string val, ConsoleColor vc)
        {
            Console.ForegroundColor = CD; Console.Write($"{label}: ");
            Console.ForegroundColor = vc; Console.Write($"{val,-9}");
            Console.ResetColor(); Console.Write("  ");
        }

        private static void Perf(string label, string val, ConsoleColor vc)
        {
            Console.ForegroundColor = CD; Console.Write($"{label}: ");
            Console.ForegroundColor = vc; Console.Write($"{val,-9}");
            Console.ResetColor(); Console.Write("  ");
        }

        private static void Kv(string k, string v)
        {
            Console.ForegroundColor = CD; Console.Write($"{k}: ");
            Console.ForegroundColor = CX; Console.Write(v);
            Console.ResetColor();
        }

        private static void EinstellRow(string key, string name, string val, ConsoleColor vc)
        {
            Console.ForegroundColor = CA; Console.Write($"{key}  ");
            Console.ForegroundColor = CX; Console.Write($"{name,-28}");
            Console.ForegroundColor = vc; Console.Write(val);
            Console.ResetColor();
        }

        private static string Ctr(string text, int width)
        {
            if (text.Length >= width) return text.Length > width ? text.Substring(0, width) : text;
            int p = (width - text.Length) / 2;
            return new string(' ', p) + text + new string(' ', width - p - text.Length);
        }

        // ═══════════════════════════════════════════════════
        //  PERSISTENZ
        // ═══════════════════════════════════════════════════
        private static void LadeAlles()
        {
            if (File.Exists(ConfigPfad))
            {
                try
                {
                    var z = File.ReadAllLines(ConfigPfad);
                    cfg = new KIConfig
                    {
                        KIAktiv = bool.Parse(z[0]),
                        Modus = (KIModus)Enum.Parse(typeof(KIModus), z[1]),
                        AutoLernen = bool.Parse(z[2]),
                        VorschlaegeAktiv = bool.Parse(z[3]),
                        PreisAktiv = bool.Parse(z[4]),
                        AnomalieAktiv = bool.Parse(z[5]),
                        FuzzyAktiv = bool.Parse(z[6]),
                        AutoKorrekturAktiv = bool.Parse(z[7]),
                        TrendAktiv = bool.Parse(z[8]),
                        BedarfsvorhersageAktiv = bool.Parse(z[9]),
                        InvNrAktiv = bool.Parse(z[10]),
                        KonfidenzSchwelle = float.Parse(z[11]),
                        KonfidenzLevel = float.Parse(z[12]),
                        LernSchwelle = int.Parse(z[13])
                    };
                }
                catch { cfg = DefaultCfg(); }
            }
            else { cfg = DefaultCfg(); }

            if (File.Exists(StatsPfad))
            {
                try
                {
                    var z = File.ReadAllLines(StatsPfad);
                    stat = new KIStatistiken
                    {
                        VorschlaegeGesamt = int.Parse(z[0]),
                        VorschlaegeUebernommen = int.Parse(z[1]),
                        VorschlaegeAbgelehnt = int.Parse(z[2]),
                        VorschlaegeFalsch = int.Parse(z[3]),
                        AnalysierteArtikel = int.Parse(z[4]),
                        PreiseVorhergesagt = int.Parse(z[5]),
                        AnomalienErkannt = int.Parse(z[6]),
                        FuzzySearches = int.Parse(z[7]),
                        AutoKorrekturen = int.Parse(z[8]),
                        TrendAnalysen = int.Parse(z[9]),
                        Bedarfsvorhersagen = int.Parse(z[10]),
                        InvNrVorschlaege = int.Parse(z[11]),
                        GesamtAnalysen = int.Parse(z[12]),
                        AntwortZeitMs = int.Parse(z[13]),
                        SchnellsteMs = int.Parse(z[14]),
                        LangsamsteMs = int.Parse(z[15]),
                        RamMB = int.Parse(z[16]),
                        UptimeStunden = int.Parse(z[17]),
                        UptimeMinuten = int.Parse(z[18]),
                        Sitzungen = int.Parse(z[19]),
                        SitzungsMinuten = int.Parse(z[20]),
                        MinutenSeitAnalyse = int.Parse(z[21]),
                        LetzterReset = z[22]
                    };
                }
                catch { stat = new KIStatistiken(); }
            }
            else { stat = new KIStatistiken(); }

            stat.Sitzungen++;
            stat.LetzterReset = stat.LetzterReset.Length > 0 ? stat.LetzterReset : DateTime.Now.ToString("dd.MM.yyyy");
        }

        private static void SpeichereAlles()
        {
            File.WriteAllLines(ConfigPfad, new[] {
                cfg.KIAktiv.ToString(), cfg.Modus.ToString(), cfg.AutoLernen.ToString(),
                cfg.VorschlaegeAktiv.ToString(), cfg.PreisAktiv.ToString(), cfg.AnomalieAktiv.ToString(),
                cfg.FuzzyAktiv.ToString(), cfg.AutoKorrekturAktiv.ToString(), cfg.TrendAktiv.ToString(),
                cfg.BedarfsvorhersageAktiv.ToString(), cfg.InvNrAktiv.ToString(),
                cfg.KonfidenzSchwelle.ToString(), cfg.KonfidenzLevel.ToString(), cfg.LernSchwelle.ToString()
            });
            File.WriteAllLines(StatsPfad, new[] {
                stat.VorschlaegeGesamt.ToString(),      stat.VorschlaegeUebernommen.ToString(),
                stat.VorschlaegeAbgelehnt.ToString(),   stat.VorschlaegeFalsch.ToString(),
                stat.AnalysierteArtikel.ToString(),     stat.PreiseVorhergesagt.ToString(),
                stat.AnomalienErkannt.ToString(),       stat.FuzzySearches.ToString(),
                stat.AutoKorrekturen.ToString(),        stat.TrendAnalysen.ToString(),
                stat.Bedarfsvorhersagen.ToString(),     stat.InvNrVorschlaege.ToString(),
                stat.GesamtAnalysen.ToString(),         stat.AntwortZeitMs.ToString(),
                stat.SchnellsteMs.ToString(),           stat.LangsamsteMs.ToString(),
                stat.RamMB.ToString(),                  stat.UptimeStunden.ToString(),
                stat.UptimeMinuten.ToString(),          stat.Sitzungen.ToString(),
                stat.SitzungsMinuten.ToString(),        stat.MinutenSeitAnalyse.ToString(),
                stat.LetzterReset
            });
        }

        public static void ErhoehVorschlag(bool uebernommen, bool falsch = false)
        {
            if (stat == null) stat = new KIStatistiken();
            stat.VorschlaegeGesamt++;
            if (uebernommen) stat.VorschlaegeUebernommen++;
            else if (falsch) stat.VorschlaegeFalsch++;
            else stat.VorschlaegeAbgelehnt++;
            SpeichereAlles();
        }

        public static KIConfig GetConfig()
        {
            if (cfg == null) LadeAlles();
            return cfg;
        }

        private static KIConfig DefaultCfg() => new KIConfig
        {
            KIAktiv = true,
            Modus = KIModus.Performance,
            AutoLernen = true,
            VorschlaegeAktiv = true,
            PreisAktiv = true,
            AnomalieAktiv = true,
            FuzzyAktiv = true,
            AutoKorrekturAktiv = true,
            TrendAktiv = true,
            BedarfsvorhersageAktiv = true,
            InvNrAktiv = true,
            KonfidenzSchwelle = 0.75f,
            KonfidenzLevel = 0.85f,
            LernSchwelle = 3
        };

        // ═══════════════════════════════════════════════════
        //  DATEN-MODELLE
        // ═══════════════════════════════════════════════════
        public class KIConfig
        {
            public bool KIAktiv { get; set; }
            public KIModus Modus { get; set; }
            public bool AutoLernen { get; set; }
            public bool VorschlaegeAktiv { get; set; }
            public bool PreisAktiv { get; set; }
            public bool AnomalieAktiv { get; set; }
            public bool FuzzyAktiv { get; set; }
            public bool AutoKorrekturAktiv { get; set; }
            public bool TrendAktiv { get; set; }
            public bool BedarfsvorhersageAktiv { get; set; }
            public bool InvNrAktiv { get; set; }
            public float KonfidenzSchwelle { get; set; }
            public float KonfidenzLevel { get; set; }
            public int LernSchwelle { get; set; }
        }

        public class KIStatistiken
        {
            public int VorschlaegeGesamt { get; set; }
            public int VorschlaegeUebernommen { get; set; }
            public int VorschlaegeAbgelehnt { get; set; }
            public int VorschlaegeFalsch { get; set; }
            public int AnalysierteArtikel { get; set; }
            public int PreiseVorhergesagt { get; set; }
            public int AnomalienErkannt { get; set; }
            public int FuzzySearches { get; set; }
            public int AutoKorrekturen { get; set; }
            public int TrendAnalysen { get; set; }
            public int Bedarfsvorhersagen { get; set; }
            public int InvNrVorschlaege { get; set; }
            public int GesamtAnalysen { get; set; }
            public int AntwortZeitMs { get; set; } = 42;
            public int SchnellsteMs { get; set; } = 11;
            public int LangsamsteMs { get; set; } = 148;
            public int RamMB { get; set; } = 8;
            public int UptimeStunden { get; set; } = 0;
            public int UptimeMinuten { get; set; } = 0;
            public int Sitzungen { get; set; } = 0;
            public int SitzungsMinuten { get; set; } = 0;
            public int MinutenSeitAnalyse { get; set; } = 0;
            public string LetzterReset { get; set; } = "";
        }

        public enum KIModus { Performance, Eco, Minimal }
    }
}