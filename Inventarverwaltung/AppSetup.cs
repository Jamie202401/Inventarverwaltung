using System;
using Inventarverwaltung.Commands;
using Inventarverwaltung.Core;

namespace Inventarverwaltung
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════════╗
    /// ║            ENTWICKLER-KONFIGURATION — MENÜAUFBAU                ║
    /// ╠══════════════════════════════════════════════════════════════════╣
    /// ║                                                                  ║
    /// ║  NEUEN MENÜPUNKT HINZUFÜGEN (2 Schritte):                       ║
    /// ║                                                                  ║
    /// ║  Schritt 1 — Neue Klasse in Commands/[X]Commands.cs:            ║
    /// ║    public class MeinCommand : ICommand {                        ║
    /// ║        public string Key   => "MEIN_KEY";                       ║
    /// ║        public string Label => "Meine Aktion";                   ║
    /// ║        public string Icon  => "🆕";                              ║
    /// ║        public void Execute() => MeinManager.MeineMethode();     ║
    /// ║    }                                                             ║
    /// ║                                                                  ║
    /// ║  Schritt 2 — Hier in der passenden Gruppe eintragen:            ║
    /// ║    .Add(new MeinCommand())                                       ║
    /// ║                                                                  ║
    /// ║  → Fertig. Kein weiterer Code nötig.                            ║
    /// ║                                                                  ║
    /// ╚══════════════════════════════════════════════════════════════════╝
    /// </summary>
    public static class AppSetup
    {
        public static AppRouter Build()
        {
            var router = new AppRouter();

            // ── [1] DASHBOARD & KI ──────────────────────────────────────────
            router.Register(
                new MenuGroup("1", "📊", "Dashboard & KI",
                              "Übersicht · KI-Engine · Insights",
                              ConsoleColor.Cyan)
                    .Add(new SystemDashboardCommand())       // System-Kennzahlen
                    .Add(new KIDashboardCommand())           // KI Control-Center
                    .Add(new KIEngineInsightsCommand())      // KI-Engine Analyse
                    .Add(new KIEngineInitCommand())          // KI neu laden
            );

            // ── [2] INVENTAR ────────────────────────────────────────────────
            router.Register(
                new MenuGroup("2", "📦", "Inventar",
                              "Artikel · Anzeigen · Details",
                              ConsoleColor.White)
                    .Add(new InventarNeuCommand())
                    .Add(new InventarZeigeCommand())
                    .Add(new InventarDetailCommand())
            );

            // ── [3] SCHNELLERFASSUNG ────────────────────────────────────────
            router.Register(
                new MenuGroup("3", "⚡", "Schnellerfassung",
                              "Ultra-Schnell · CSV · Templates",
                              ConsoleColor.Yellow)
                    .Add(new SchnellerfassungCommand())
            );

            // ── [4] MITARBEITER & BENUTZER ──────────────────────────────────
            router.Register(
                new MenuGroup("4", "👥", "Mitarbeiter",
                              "Anlegen · Anzeigen · Benutzer",
                              ConsoleColor.Cyan)
                    .Add(new MitarbeiterNeuCommand())
                    .Add(new MitarbeiterZeigeCommand())
                    .Add(new BenutzerNeuCommand())
                    .Add(new BenutzerZeigeCommand())
            );

            // ── [5] BESTANDSPFLEGE ──────────────────────────────────────────
            router.Register(
                new MenuGroup("5", "📋", "Bestandspflege",
                              "Erhöhen · Verringern · Limits",
                              ConsoleColor.Green)
                    .Add(new BestandErhoehenCommand())
                    .Add(new BestandVerringernCommand())
                    .Add(new MindestbestandCommand())
                    .Add(new BestandWarnungCommand())
            );

            // ── [6] HARDWARE-DRUCK ──────────────────────────────────────────
            router.Register(
                new MenuGroup("6", "🖨️ ", "Hardware-Druck",
                              "Drucken · Historie · Suche",
                              ConsoleColor.Magenta)
                    .Add(new DruckNeuCommand())
                    .Add(new DruckHistorieCommand())
                    .Add(new DruckSucheCommand())
                    .Add(new DruckEditCommand())
                    .Add(new DruckKonfigCommand())
            );

            // ── [7] WERKZEUGE ───────────────────────────────────────────────
            router.Register(
                new MenuGroup("7", "🔧", "Werkzeuge",
                              "Import · Bearbeitung · Löschung",
                              ConsoleColor.DarkYellow)
                    .Add(new ImportCommand())
                    .Add(new BearbeitungCommand())
                    .Add(new LoeschungCommand())
            );

            // ── [8] SYSTEM ──────────────────────────────────────────────────
            router.Register(
                new MenuGroup("8", "⚙️ ", "System",
                              "Log · Report · Verschlüsselung",
                              ConsoleColor.DarkGray)
                    .Add(new SystemLogCommand())
                    .Add(new TagesreportCommand())
                    .Add(new VerschluesselungCommand())
            );

            // ── [9] LIEFERANTEN ─────────────────────────────────────────────
            router.Register(
                new MenuGroup("9", "🚚", "Lieferanten",
                              "Anlegen · Suchen · Adresse validieren",
                              ConsoleColor.DarkGreen)
                    .Add(new LieferantMenuCommand())
            );

            // ── [10] Handy Scanner ─────────────────────────────────────────────
            router.Register(
                new MenuGroup("10", "🚚", "Handy Scanner",
                              "Server starten . Barcode per App Scannen",
                              ConsoleColor.Cyan)
                    .Add(new BarcodeServerStartenCommand())
                    .Add(new BarcodeServerStoppenCommand())
                    .Add(new BarcodeServerStatusCommand())
                    .Add(new BarcodeServerIPCommand())
            );
            // ── [11] Rollen Details ─────────────────────────────────────────────

            router.Register(
                new MenuGroup("11", "[]", "Rollen Details",
                               "Rollen . Rechte . Benutzerzuweisung",
                               ConsoleColor.Magenta)
                .Add(new RollenUebersichtCommand())
                .Add(new NeueRolleCommand())
                .Add(new RolleBearbeitenCommand())
                .Add(new BenutzerRolleZuweisenCommand())
                .Add(new RolleLoeschenCommand())
                );
            return router;
        }
    }
}