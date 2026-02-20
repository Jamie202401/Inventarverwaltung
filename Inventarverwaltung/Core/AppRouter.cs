using Inventarverwaltung.Manager.Auth;
using Inventarverwaltung;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventarverwaltung.Core
{
    /// <summary>
    /// Herzstück der Navigation.
    /// 
    /// Der Router:
    ///   - zeigt das Hauptmenü (Kacheln 1–n)
    ///   - zeigt das Untermenü der gewählten Gruppe
    ///   - ruft Execute() des gewählten Commands auf
    /// 
    /// Program.cs macht nur noch: AppSetup.Build().Run()
    /// 
    /// Neuen Menüpunkt hinzufügen → AppSetup.cs, nicht hier.
    /// </summary>
    public class AppRouter
    {
        private readonly List<MenuGroup> _groups = new List<MenuGroup>();

        /// <summary>
        /// Registriert eine Gruppe. Reihenfolge bestimmt die Anzeigereihenfolge.
        /// Gibt sich selbst zurück für Verkettung.
        /// </summary>
        public AppRouter Register(MenuGroup group)
        {
            _groups.Add(group);
            return this;
        }

        /// <summary>
        /// Startet die Hauptschleife.
        /// Kehrt erst zurück wenn der Benutzer [0] drückt.
        /// </summary>
        public void Run()
        {
            while (true)
            {
                UI.ZeigeHauptmenu(_groups);
                string eingabe = Console.ReadLine()?.Trim() ?? "";

                if (eingabe == "/console") { DevConsole.Open(); continue; }

                if (eingabe == "0")
                {
                    Console.Clear();
                    Console.WriteLine("\n  [1]  Programm beenden");
                    Console.WriteLine("  [2]  Programm neu starten");
                    Console.WriteLine("  [3]  Benutzer wechseln");
                    Console.WriteLine("  [0]  Abbrechen");
                    Console.Write("\n  Auswahl: ");
                    switch (Console.ReadLine()?.Trim())
                    {
                        case "1":
                            // Abschluss-Animation + sauberer Ausstieg
                            LogManager.LogProgrammEnde();
                            Program.Verabschiedung();
                            Environment.Exit(0);
                            return;   // wird nie erreicht, aber Compiler braucht es

                        case "2":
                            System.Diagnostics.Process.Start(
                                Environment.ProcessPath ?? Environment.GetCommandLineArgs()[0]);
                            Environment.Exit(0);
                            return;

                        case "3":
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write("  Bitte warten Sie einen Augenblick...");
                            Console.ResetColor();
                            Thread.Sleep(1000);
                            AuthManager.Anmeldung();
                            return;  // weiter im Router

                        default:           // "0" oder ungültig → Abbrechen
                            return;

                    }
                    continue;
                }

                MenuGroup gruppe = _groups.FirstOrDefault(g => g.Nr == eingabe);
                if (gruppe == null)
                {
                    UI.ZeigeUngueltigeEingabe();
                    continue;
                }
                RunGruppe(gruppe);
            }
        }

        /// <summary>
        /// Zeigt das Untermenü einer Gruppe und führt die gewählte Aktion aus.
        /// [0] kehrt zum Hauptmenü zurück.
        /// </summary>
        private void RunGruppe(MenuGroup gruppe)
        {
            while (true)
            {
                UI.ZeigeUntermenu(gruppe);
                string eingabe = Console.ReadLine()?.Trim() ?? "";

                if (eingabe == "0") return;

                if (int.TryParse(eingabe, out int index)
                    && index >= 1
                    && index <= gruppe.Commands.Count)
                {
                    gruppe.Commands[index - 1].Execute();
                }
                else
                {
                    UI.ZeigeUngueltigeEingabe();
                }
            }
        }
    }
}