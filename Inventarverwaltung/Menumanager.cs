using System;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet die Menüanzeige mit modernem Design und Animationen
    /// ERWEITERT: Neue Menüpunkte für Bestandsverwaltung
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
            DrawInventorySection();
            DrawStockManagementSection();  // NEU!
            DrawEmployeeSection();
            DrawSystemSection();
            DrawExitSection();

            Console.WriteLine();
            DrawUserInput();
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ┌─ 📊 BESTANDSVERWALTUNG (NEU!) " + new string('─', 35) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("11", "➕ Bestand erhöhen"),
                ("12", "➖ Bestand verringern"),
                ("13", "⚙️  Mindestbestand konfigurieren"),
                ("15", "🔴 Artikel unter Mindestbestand anzeigen")
            }, ConsoleColor.Green);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawEmployeeSection()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ 👥 MITARBEITERVERWALTUNG " + new string('─', 39) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("2", "👤 Neuen Mitarbeiter hinzufügen"),
                ("3", "👥 Mitarbeiter anzeigen"),
                ("6", "👨‍💼 Benutzer anzeigen")
            }, ConsoleColor.Cyan);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  └" + new string('─', 68) + "┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void DrawSystemSection()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ┌─ 🔧 SYSTEMFUNKTIONEN " + new string('─', 43) + "┐");
            Console.ResetColor();

            DrawMenuItems(new[]
            {
                ("5", "🔐 Benutzer anlegen"),
                ("7", "📝 System-Log anzeigen (verschlüsselt)"),
                ("8", "📄 Tagesreport erstellen (verschlüsselt)"),
                ("9", "🔐 Verschlüsselungs-Info anzeigen")
            }, ConsoleColor.Yellow);

            Console.ForegroundColor = ConsoleColor.Yellow;
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