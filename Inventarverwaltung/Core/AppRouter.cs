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

                if (eingabe == "0") break;

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