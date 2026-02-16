using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventarverwaltung.Core
{
  public class MenuGroup
    {
        public string Nr { get; }
        public string Icon { get; }
        public string Titel { get; }
        public string SubLabel { get; }
        public ConsoleColor Farbe {  get; }

        public List<ICommand> Commands { get; } = new List<ICommand>();


        public MenuGroup(string nr, string icon, string titel, string subLabel, ConsoleColor farbe)
        {
            Nr = nr;
            Icon = icon;
            Titel = titel;
            SubLabel = subLabel;
            Farbe = farbe;
        }
        public MenuGroup Add(ICommand cmd)
        {
            Commands.Add(cmd);
            return this;   
        }
        
           
        }
    }

