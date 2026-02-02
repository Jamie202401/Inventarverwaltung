using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet die Menüanzeige
    /// </summary>
    public static class MenuManager
    {
        public static void ShowMenu()
        {
            ConsoleHelper.PrintHeader();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📋 HAUPTMENÜ - Bitte wählen Sie eine Aktion:");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            ConsoleHelper.PrintMenuItem("1", "📦 Neuen Artikel hinzufügen");
            ConsoleHelper.PrintMenuItem("2", "👤 Neuen Mitarbeiter hinzufügen");
            ConsoleHelper.PrintMenuItem("3", "👥 Mitarbeiter anzeigen");
            ConsoleHelper.PrintMenuItem("4", "📊 Inventar anzeigen");
            ConsoleHelper.PrintMenuItem("5", "🔐 Benutzer anlegen");
            ConsoleHelper.PrintMenuItem("6", "👨‍💼 Benutzer anzeigen");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            ConsoleHelper.PrintMenuItem("7", "📝 System-Log anzeigen");
            ConsoleHelper.PrintMenuItem("8", "📄 Tagesreport erstellen");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleHelper.PrintMenuItem("0", "❌ Programm beenden");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Ihre Auswahl: ");
            Console.ResetColor();
        }
    }
}