using Inventarverwaltung.API;
using Inventarverwaltung.Core;
using Inventarverwaltung.Manager.UI;

namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // BARCODE SERVER COMMANDS
    // In AppSetup.cs unter MenuGroup [9] eingetragen
    // ══════════════════════════════════════════════════════════════════

    public class BarcodeServerStartenCommand : ICommand
    {
        public string Key => "BC_START";
        public string Label => "Barcode-Server starten";
        public string Icon => "📡";
        public void Execute() => BarcodeServer.Starten();
    }

    public class BarcodeServerStoppenCommand : ICommand
    {
        public string Key => "BC_STOP";
        public string Label => "Barcode-Server stoppen";
        public string Icon => "⏹️ ";
        public void Execute() => BarcodeServer.Stoppen();
    }

    public class BarcodeServerStatusCommand : ICommand
    {
        public string Key => "BC_STATUS";
        public string Label => "Server-Status anzeigen";
        public string Icon => "📊";

        public void Execute()
        {
            System.Console.WriteLine();

            if (BarcodeServer.Laeuft)
            {
                System.Console.ForegroundColor = System.ConsoleColor.Green;
                System.Console.WriteLine($"  ✔ Server läuft");
                System.Console.ForegroundColor = System.ConsoleColor.Cyan;
                System.Console.WriteLine($"  📡 Handy-URL: http://{BarcodeServer.LocalIP}:{BarcodeServer.PORT}/scan");
            }
            else
            {
                System.Console.ForegroundColor = System.ConsoleColor.Red;
                System.Console.WriteLine("  ✘ Server ist gestoppt");
            }

            System.Console.ResetColor();
            System.Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }
    }

    public class BarcodeServerIPCommand : ICommand
    {
        public string Key => "BC_IP";
        public string Label => "PC-IP Adresse anzeigen";
        public string Icon => "🌐";

        public void Execute()
        {
            System.Console.Clear();
            ConsoleHelper.PrintSectionHeader("PC-IP Adresse", System.ConsoleColor.Cyan);
            System.Console.WriteLine();

            System.Console.ForegroundColor = System.ConsoleColor.Cyan;
            System.Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("  ║              📱 IN DIE APP EINTRAGEN                             ║");
            System.Console.WriteLine("  ╠══════════════════════════════════════════════════════════════════╣");
            System.Console.ResetColor();

            // Alle Netzwerk-IPs auflisten
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            int index = 1;

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    bool istWLAN = ip.ToString().StartsWith("192.168") ||
                                   ip.ToString().StartsWith("10.") ||
                                   ip.ToString().StartsWith("172.");

                    System.Console.ForegroundColor = istWLAN
                        ? System.ConsoleColor.Green
                        : System.ConsoleColor.Gray;

                    System.Console.WriteLine($"  ║  [{index}] {ip,-60}║");

                    if (istWLAN)
                    {
                        System.Console.ForegroundColor = System.ConsoleColor.Yellow;
                        string handyUrl = $"http://{ip}:{BarcodeServer.PORT}/scan";
                        System.Console.WriteLine($"  ║      → {handyUrl,-58}║");
                    }

                    System.Console.ResetColor();
                    index++;
                }
            }

            System.Console.ForegroundColor = System.ConsoleColor.Cyan;
            System.Console.WriteLine("  ╠══════════════════════════════════════════════════════════════════╣");
            System.Console.ForegroundColor = System.ConsoleColor.White;
            System.Console.WriteLine("  ║  ➡️  Grüne IP in die App eingeben                                ║");
            System.Console.WriteLine("  ║  ➡️  Handy + PC müssen im gleichen WLAN sein                    ║");
            System.Console.WriteLine("  ║  ➡️  Server muss gestartet sein                                 ║");
            System.Console.WriteLine("  ║  🔒 Alle Daten bleiben im lokalen Netzwerk                      ║");
            System.Console.ForegroundColor = System.ConsoleColor.Cyan;
            System.Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();

            ConsoleHelper.PressKeyToContinue();
        }
    }
}