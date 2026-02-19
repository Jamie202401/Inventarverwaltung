using System;
using Inventarverwaltung;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventarverwaltung.Commands
{
    public class InfosMenuCommand : Core.ICommand
    {
        public string Key => "INFOS";
        public string Label => "Infos zum System Sehen";
        public string Icon => "I ";
        public void Execute() => InfosManager.ZeigeInfosMenu();
    }
}
