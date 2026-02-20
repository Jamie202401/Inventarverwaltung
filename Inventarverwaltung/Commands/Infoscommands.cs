using System;
using Inventarverwaltung;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventarverwaltung.Commands
{
    public class InfosMenuCommand2 : Core.ICommand
    {
        public string Key => "INFOS_MENU";
        public string Label => "Infos & Feedback";
        public string Icon => "ℹ️";
        public void Execute() => InfosManager.ZeigeInfosMenu();
    }
}
