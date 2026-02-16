using System;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet die Menüanzeige mit modernem Design und Animationen
    /// ERWEITERT: Dashboard, Schnellerfassung, Export + KI Engine 2.0
    /// </summary>
    public static class MenuManager
    {
        private static readonly string[] LoadingSymbols = { "▁", "▂", "▃", "▄", "▅", "▆", "▇", "█", "▇", "▆", "▅", "▄", "▃", "▂" };
        private const int AnimationSpeed = 30;

        public static void ShowMenu()
        {
            ConsoleHelper.PrintHeader();

            Console.WriteLine();

            // Menü-Kategorien
            DrawDashboardSection();
            DrawKISection();  // NEU: KI-Bereich
            DrawSchnellerfassungSection();
            DrawInventorySection();
            DrawStockManagementSection();
            DrawHardwareDruckSection();
            DrawEmployeeSection();
            DrawSystemSection();
            DrawExtraFunctions();
            DrawExitSection();

            Console.WriteLine();
            DrawUserInput();
        }

        private static void DrawDashboardSection()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ┌─ 📊 DASHBOARD " + new string('─', 52) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("99", "📊 Inventar Dashboard (Umfassende Übersicht)")
            }, ConsoleColor.Magenta);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawKISection()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ┌─ 🤖 KI ENGINE 2.0 (NEU!) " + new string('─', 39) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("98", "🤖 KI-Insights & Analysen (Machine Learning)"),
                ("97", "KI Dashbaord")
            }, ConsoleColor.Green);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawSchnellerfassungSection()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ┌─ ⚡ SCHNELLERFASSUNG " + new string('─', 44) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("20", "⚡ Ultra-Schnell-Modus, CSV-Import & Templates")
            }, ConsoleColor.Yellow);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawInventorySection()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ 📦 INVENTARVERWALTUNG " + new string('─', 43) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("1", "📦 Neuen Artikel hinzufügen (Erweitert)"),
                ("4", "📊 Inventar anzeigen (mit Bestandsstatus)"),
                ("14", "🔍 Artikel-Details anzeigen")
            }, ConsoleColor.White);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawStockManagementSection()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ 📊 BESTANDSVERWALTUNG " + new string('─', 42) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("11", "➕ Bestand erhöhen"),
                ("12", "➖ Bestand verringern"),
                ("13", "⚙️  Mindestbestand konfigurieren"),
                ("15", "🔴 Artikel unter Mindestbestand anzeigen")
            }, ConsoleColor.Cyan);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawEmployeeSection()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ┌─ 👥 MITARBEITERVERWALTUNG " + new string('─', 39) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("2", "👤 Neuen Mitarbeiter hinzufügen"),
                ("3", "👥 Mitarbeiter anzeigen"),
                ("6", "👨‍💼 Benutzer anzeigen")
            }, ConsoleColor.DarkCyan);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawExtraFunctions()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("  ┌─ 🔧 EXTRAFUNKTIONEN " + new string('─', 45) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("16", "📥 Import"),
                ("17", "✏️  Bearbeitung"),
                ("18", "🗑️  Löschung"),
                ("19", "📤 Export (Excel/PDF)")
            }, ConsoleColor.Blue);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawSystemSection()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ┌─ 🔧 SYSTEMFUNKTIONEN " + new string('─', 43) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("5", "🔐 Benutzer anlegen"),
                ("7", "📝 System-Log anzeigen (verschlüsselt)"),
                ("8", "📄 Tagesreport erstellen (verschlüsselt)"),
                ("9", "🔐 Verschlüsselungs-Info anzeigen")
            }, ConsoleColor.DarkYellow);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }
        private static void DrawHardwareDruckSection()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ┌─ 🖨️  HARDWARE AUSGABE / DRUCK " + new string('─', 35) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("21", "🖨️  Hardware-Ausgabe drucken & verwalten")
            }, ConsoleColor.Magenta);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawExitSection()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ┌─ " + new string('─', 64) + "┐");
            ConsoleHelper.PrintMenuItem("0", "❌ Programm beenden");
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
        }

        private static void DrawMenuItems((string Key, string Text)[] items, ConsoleColor color)
        {
            foreach (var item in items)
            {
                Console.ForegroundColor = color;
                Console.Write("  │ ");
                Console.WriteLine($"[{item.Key}] {item.Text} │");
                Console.ResetColor();
            }
        }

        private static void DrawUserInput()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ ");
            AnimatePulseSpinner();
            Console.Write("Ihre Auswahl: ");
            Console.ResetColor();
        }

        private static void AnimatePulseSpinner()
        {
            for (int i = 0; i < LoadingSymbols.Length; i++)
            {
                Console.Write(LoadingSymbols[i]);
                Thread.Sleep(AnimationSpeed);
                Console.Write("\b");
            }
        }
    }
}