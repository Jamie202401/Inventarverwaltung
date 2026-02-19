using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Inventarverwaltung
{
    // ═══════════════════════════════════════════════════════════════════
    // BARCODE SERVER — WLAN (lokal & sicher)
    //
    // Handy und PC müssen im gleichen WLAN sein.
    // Alle Daten bleiben im lokalen Netzwerk — nichts geht ins Internet.
    //
    // Ablauf:
    //   1. Server starten → PC-IP wird angezeigt
    //   2. IP in die Handy-App eintragen
    //   3. Barcode scannen → Artikel erscheint sofort
    //
    // Kein NuGet nötig — System.Net.HttpListener ist in .NET eingebaut
    // ═══════════════════════════════════════════════════════════════════

    public static class BarcodeServer
    {
        private static HttpListener _listener;
        private static CancellationTokenSource _cts;
        private static Thread _serverThread;

        public static bool Laeuft { get; private set; } = false;
        public static string LocalIP => HoleLocalIP();
        public const int PORT = 8080;

        // ═══════════════════════════════════════════════════════════════
        // SERVER STARTEN
        // ═══════════════════════════════════════════════════════════════

        public static void Starten()
        {
            if (Laeuft)
            {
                ConsoleHelper.PrintWarning("Server läuft bereits!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{PORT}/");
                _listener.Start();

                Laeuft = true;
                _cts = new CancellationTokenSource();

                // Hintergrund-Thread — blockiert nicht die Konsole
                _serverThread = new Thread(() => ServerSchleife(_cts.Token))
                {
                    IsBackground = true,
                    Name = "BarcodeServer"
                };
                _serverThread.Start();

                ZeigeStartmeldung();
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 5)
            {
                // Zugriff verweigert — einmalige Admin-Einrichtung nötig
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⚠️  Einmalige Admin-Einrichtung nötig!");
                Console.WriteLine("  Öffne CMD als Administrator und führe aus:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  netsh http add urlacl url=http://+:{PORT}/ user=Jeder");
                Console.ResetColor();
                Console.WriteLine("  Danach Programm neu starten.");
                ConsoleHelper.PressKeyToContinue();
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Server-Start fehlgeschlagen: {ex.Message}");
                Laeuft = false;
                ConsoleHelper.PressKeyToContinue();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // SERVER STOPPEN
        // ═══════════════════════════════════════════════════════════════

        public static void Stoppen()
        {
            if (!Laeuft)
            {
                ConsoleHelper.PrintWarning("Server läuft nicht.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            _cts?.Cancel();
            _listener?.Stop();
            _listener?.Close();
            Laeuft = false;

            ConsoleHelper.PrintSuccess("✔ Barcode-Server gestoppt.");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // SERVER-SCHLEIFE — läuft im Hintergrund
        // ═══════════════════════════════════════════════════════════════

        private static void ServerSchleife(CancellationToken token)
        {
            while (!token.IsCancellationRequested && Laeuft)
            {
                try
                {
                    var context = _listener.GetContext();
                    Task.Run(() => VerarbeiteAnfrage(context));
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (Laeuft)
                        LogManager.LogFehler("BarcodeServer", ex.Message);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ANFRAGE VERARBEITEN
        // ═══════════════════════════════════════════════════════════════

        private static void VerarbeiteAnfrage(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;

            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Methods", "GET, OPTIONS");
            resp.ContentType = "application/json; charset=utf-8";

            if (req.HttpMethod == "OPTIONS")
            {
                resp.StatusCode = 200;
                resp.Close();
                return;
            }

            string pfad = req.Url.AbsolutePath.ToLower();

            try
            {
                string antwortJson;

                if (pfad == "/ping")
                {
                    antwortJson = JsonSerializer.Serialize(new
                    {
                        status = "OK",
                        version = "1.0",
                        artikel = DataManager.Inventar.Count
                    });
                }
                else if (pfad == "/scan")
                {
                    string barcode = req.QueryString["barcode"] ?? "";
                    antwortJson = SucheArtikel(barcode);
                }
                else
                {
                    antwortJson = JsonSerializer.Serialize(new { fehler = "Unbekannter Pfad" });
                    resp.StatusCode = 404;
                }

                byte[] daten = Encoding.UTF8.GetBytes(antwortJson);
                resp.ContentLength64 = daten.Length;
                resp.OutputStream.Write(daten, 0, daten.Length);
            }
            catch (Exception ex)
            {
                string fehlerJson = JsonSerializer.Serialize(new { fehler = ex.Message });
                byte[] daten = Encoding.UTF8.GetBytes(fehlerJson);
                resp.StatusCode = 500;
                resp.OutputStream.Write(daten, 0, daten.Length);
            }
            finally
            {
                resp.Close();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // ARTIKEL SUCHEN
        // ═══════════════════════════════════════════════════════════════

        private static string SucheArtikel(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return JsonSerializer.Serialize(new
                {
                    gefunden = false,
                    hinweis = "Kein Barcode übergeben"
                });

            var artikel = DataManager.Inventar.FirstOrDefault(a =>
                a.SerienNummer.Equals(barcode, StringComparison.OrdinalIgnoreCase) ||
                a.InvNmr.Equals(barcode, StringComparison.OrdinalIgnoreCase) ||
                a.GeraeteName.Contains(barcode, StringComparison.OrdinalIgnoreCase));

            if (artikel == null)
                return JsonSerializer.Serialize(new
                {
                    gefunden = false,
                    hinweis = $"Kein Artikel mit Barcode '{barcode}' gefunden"
                });

            string bestandStatus = artikel.Anzahl == 0 ? "Leer" :
                                   artikel.Anzahl <= artikel.Mindestbestand ? "Niedrig" : "OK";

            return JsonSerializer.Serialize(new
            {
                gefunden = true,
                invNummer = artikel.InvNmr,
                geraeteName = artikel.GeraeteName,
                hersteller = artikel.Hersteller,
                kategorie = artikel.Kategorie,
                serienNummer = artikel.SerienNummer,
                mitarbeiter = artikel.MitarbeiterBezeichnung,
                anzahl = artikel.Anzahl,
                mindestbestand = artikel.Mindestbestand,
                preis = artikel.Preis.ToString("F2"),
                datum = artikel.Anschaffungsdatum.ToString("dd.MM.yyyy"),
                erstelltVon = artikel.ErstelltVon,
                bestandStatus = bestandStatus
            });
        }

        // ═══════════════════════════════════════════════════════════════
        // HILFSMETHODEN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ermittelt die lokale WLAN-IP des PCs automatisch
        /// </summary>
        private static string HoleLocalIP()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork,
                                              SocketType.Dgram, ProtocolType.Udp);
                socket.Connect("8.8.8.8", 80);
                return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString()
                       ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }

        private static void ZeigeStartmeldung()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║         📡 BARCODE-SERVER AKTIV  ·  🔒 NUR WLAN                  ║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ║  PC-IP:      {LocalIP,-52}║");
            Console.WriteLine($"  ║  Port:       {PORT,-52}║");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ║  Handy-URL:  http://{LocalIP}:{PORT}/scan{new string(' ', Math.Max(0, 28 - LocalIP.Length))}║");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╠══════════════════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ║  ➡️  Diese URL in die App eintragen                              ║");
            Console.WriteLine("  ║  ➡️  Handy + PC müssen im gleichen WLAN sein                    ║");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ║  ✔  Alle Daten bleiben im lokalen Netzwerk                      ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
    }
}