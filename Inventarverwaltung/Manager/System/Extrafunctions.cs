using Inventarverwaltung.Manager.UI;
using System;
using System.IO;
using System.Text;

namespace Inventarverwaltung
{


    /// <summary>
    /// Einfacher, thread‑sicherer Logger für das Programm.
    /// Verwenden: Extrafunctions.InitializeLog("logs\\app.log"); dann Extrafunctions.Log(...).
    /// Beinhaltet Convenience‑Methoden wie LogLogin, LogAction, LogException.
    /// </summary>
    public static class Extrafunctions
    {

    
    
       public static void Logwrite()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Logs");

        }
    }

    }
      
    
