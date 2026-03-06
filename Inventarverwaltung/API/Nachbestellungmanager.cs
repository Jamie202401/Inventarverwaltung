using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Manager.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════════╗
    /// ║               🛒  NACHBESTELLUNGS-MANAGER                        ║
    /// ╠══════════════════════════════════════════════════════════════════╣
    /// ║  Generiert direkte Kauf-Links für Artikel mit niedrigem Bestand. ║
    /// ║  Unterstützte Shops:                                             ║
    /// ║    • Amazon.de          – Breites Sortiment, schnelle Lieferung  ║
    /// ║    • Google Shopping    – Preisvergleich aller Händler            ║
    /// ║    • Alternate.de       – IT & Hardware Spezialist               ║
    /// ║    • Conrad.de          – Elektronik & Technik                   ║
    /// ║    • Cyberport.de       – IT & Consumer Electronics              ║
    /// ╚══════════════════════════════════════════════════════════════════╝
    /// </summary>
    public static class NachbestellungManager
    {
        // ══════════════════════════════════════════════════════════════
        //  SHOP-DEFINITIONEN
        // ══════════════════════════════════════════════════════════════
        private static readonly List<Shop> Shops = new List<Shop>
        {
            new Shop("Amazon.de",       "🛒", ConsoleColor.Yellow,
                     "https://www.amazon.de/s?k={QUERY}&ref=nb_sb_noss",
                     "Breites Sortiment · Prime-Versand · Schnellste Lieferung"),

            new Shop("Google Shopping", "🔍", ConsoleColor.Cyan,
                     "https://www.google.de/search?q={QUERY}&tbm=shop",
                     "Preisvergleich · Alle Händler · Günstigster Preis"),

            new Shop("Alternate.de",    "💻", ConsoleColor.Green,
                     "https://www.alternate.de/search?query={QUERY}",
                     "IT & Hardware Spezialist · Große Auswahl"),

            new Shop("Conrad.de",       "⚡", ConsoleColor.Magenta,
                     "https://www.conrad.de/search.html?search={QUERY}",
                     "Elektronik & Technik · Fachhandel"),

            new Shop("Cyberport.de",    "🖥️ ", ConsoleColor.Blue,
                     "https://www.cyberport.de/search/?q={QUERY}",
                     "IT & Consumer Electronics · Top-Marken"),

             new Shop("Böttcher.de",    "🖥️ ", ConsoleColor.Blue,
                     "https://www.bueromarkt-ag.de/{QUERY}",
                     "IT & Consumer Electronics · Top-Marken"),

        };

        // ══════════════════════════════════════════════════════════════
        //  HAUPT-EINSTIEG
        // ══════════════════════════════════════════════════════════════
        public static void ZeigeNachbestellungMenu()
        {
            while (true)
            {
                Console.Clear();
                ZeigeNachbestellHeader();

                var kritischeArtikel = HoleKritischeArtikel();

                if (kritischeArtikel.Count == 0)
                {
                    ZeigeAlleBestaendeOK();
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }

                ZeigeKritischeArtikelListe(kritischeArtikel);
                ZeigeAuswahlHinweis(kritischeArtikel.Count);

                string eingabe = ConsoleHelper.GetInput("  Artikel-Nummer (oder 0 = Zurück)");

                if (eingabe == "0") return;

                if (int.TryParse(eingabe, out int nr) && nr >= 1 && nr <= kritischeArtikel.Count)
                {
                    ZeigeShopAuswahl(kritischeArtikel[nr - 1]);
                }
                else
                {
                    ZeigeFehler("Ungültige Auswahl — bitte eine Nummer aus der Liste eingeben.");
                    System.Threading.Thread.Sleep(1200);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  SHOP-AUSWAHL FÜR EINEN ARTIKEL
        // ══════════════════════════════════════════════════════════════
        private static void ZeigeShopAuswahl(InvId artikel)
        {
            while (true)
            {
                Console.Clear();
                ZeigeNachbestellHeader();
                ZeigeArtikelDetailKarte(artikel);

                Console.WriteLine();
                ZeigeShopListe();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  ┌─ OPTIONEN ───────────────────────────────────────────────────────┐");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  │  ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("[1-5]");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" Shop öffnen     ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("[A]");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" Alle Links öffnen     ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("[0]");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" Zurück");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("          │");
                Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
                Console.ResetColor();
                Console.WriteLine();

                string eingabe = ConsoleHelper.GetInput("  Shop wählen");

                if (eingabe == "0") return;

                if (eingabe.ToLower() == "a")
                {
                    OeffneAlleLinks(artikel);
                    break;
                }

                if (int.TryParse(eingabe, out int shopNr) && shopNr >= 1 && shopNr <= Shops.Count)
                {
                    OeffneShopLink(artikel, Shops[shopNr - 1]);
                    // Zurück zur Shop-Auswahl damit man weiteren Shop öffnen kann
                }
                else
                {
                    ZeigeFehler("Ungültige Auswahl.");
                    System.Threading.Thread.Sleep(900);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  LINK ÖFFNEN
        // ══════════════════════════════════════════════════════════════
        private static void OeffneShopLink(InvId artikel, Shop shop)
        {
            string suchbegriff = BaueOptimalenSuchbegriff(artikel);
            string url = shop.UrlTemplate.Replace("{QUERY}", Uri.EscapeDataString(suchbegriff));

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = shop.Farbe;
            Console.Write($"{shop.Icon}  Öffne {shop.Name}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            int pad = Math.Max(0, 60 - shop.Name.Length);
            Console.WriteLine(new string(' ', pad) + "│");

            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("🔎  Suche: ");
            Console.ForegroundColor = ConsoleColor.White;
            string anzeigeSuche = suchbegriff.Length > 50 ? suchbegriff.Substring(0, 47) + "..." : suchbegriff;
            Console.Write(anzeigeSuche);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string(' ', Math.Max(0, 53 - anzeigeSuche.Length)) + "│");

            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();

            // Animation
            ZeigeLadeAnimation($"Browser wird geöffnet");

            bool erfolg = OeffneBrowser(url);

            if (erfolg)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  ✔  ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Browser geöffnet — {shop.Name} mit exakter Suchanfrage.");
                Console.ResetColor();

                LogManager.LogDatenGespeichert("Nachbestellung",
                    $"Shop-Link geöffnet: {shop.Name} für '{artikel.GeraeteName}' (Bestand: {artikel.Anzahl}/{artikel.Mindestbestand})");
            }
            else
            {
                ZeigeFehler("Browser konnte nicht geöffnet werden. Bitte URL manuell kopieren:");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  {url}");
                Console.ResetColor();
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void OeffneAlleLinks(InvId artikel)
        {
            string suchbegriff = BaueOptimalenSuchbegriff(artikel);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║  🌐  Öffne alle Shops gleichzeitig                               ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            int erfolgreich = 0;
            foreach (var shop in Shops)
            {
                string url = shop.UrlTemplate.Replace("{QUERY}", Uri.EscapeDataString(suchbegriff));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"  {shop.Icon}  {shop.Name,-18}");

                System.Threading.Thread.Sleep(400); // Damit Browser nicht blockiert

                bool ok = OeffneBrowser(url);
                if (ok)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✔ geöffnet");
                    erfolgreich++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("✖ fehlgeschlagen");
                }
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✔  {erfolgreich}/{Shops.Count} Shops geöffnet.");
            Console.ResetColor();

            LogManager.LogDatenGespeichert("Nachbestellung",
                $"Alle Shop-Links geöffnet für '{artikel.GeraeteName}' (Bestand: {artikel.Anzahl}/{artikel.Mindestbestand})");

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        // ══════════════════════════════════════════════════════════════
        //  INTELLIGENTE SUCHBEGRIFF-ERSTELLUNG
        // ══════════════════════════════════════════════════════════════
        private static string BaueOptimalenSuchbegriff(InvId artikel)
        {
            var teile = new List<string>();

            // Hersteller hinzufügen (wenn nicht "Unbekannt" oder leer)
            if (!string.IsNullOrWhiteSpace(artikel.Hersteller) &&
                !artikel.Hersteller.Equals("Unbekannt", StringComparison.OrdinalIgnoreCase) &&
                !artikel.Hersteller.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                teile.Add(artikel.Hersteller.Trim());
            }

            // Gerätename (Duplikate mit Hersteller vermeiden)
            string gerät = artikel.GeraeteName.Trim();
            if (!gerät.StartsWith(artikel.Hersteller, StringComparison.OrdinalIgnoreCase))
            {
                teile.Add(gerät);
            }
            else
            {
                teile.Add(gerät);
            }

            // Seriennummer / Modellnummer hinzufügen wenn vorhanden und sinnvoll
            if (!string.IsNullOrWhiteSpace(artikel.SerienNummer) &&
                !artikel.SerienNummer.Equals("N/A", StringComparison.OrdinalIgnoreCase) &&
                artikel.SerienNummer.Length <= 20) // Lange SNRs weglassen
            {
                teile.Add(artikel.SerienNummer.Trim());
            }

            return string.Join(" ", teile.Distinct());
        }

        // ══════════════════════════════════════════════════════════════
        //  BROWSER ÖFFNEN (Cross-Platform)
        // ══════════════════════════════════════════════════════════════
        private static bool OeffneBrowser(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  DATEN-HELFER
        // ══════════════════════════════════════════════════════════════
        private static List<InvId> HoleKritischeArtikel()
        {
            return DataManager.Inventar
                .Where(a => a.Anzahl <= a.Mindestbestand)
                .OrderBy(a => a.Anzahl)          // Leerste zuerst
                .ThenBy(a => a.GeraeteName)
                .ToList();
        }

        // ══════════════════════════════════════════════════════════════
        //  DESIGN-KOMPONENTEN
        // ══════════════════════════════════════════════════════════════
        private static void ZeigeNachbestellHeader()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("🛒  N A C H B E S T E L L U N G S - A S S I S T E N T");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("          ║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════════════════╣");
            Console.Write("  ║  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Direkte Shop-Links · Exakte Modellsuche · 5 Händler");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("              ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeKritischeArtikelListe(List<InvId> artikel)
        {
            // Statistik-Zeile
            int leer = artikel.Count(a => a.Anzahl == 0);
            int niedrig = artikel.Count(a => a.Anzahl > 0);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ ARTIKEL MIT KRITISCHEM BESTAND ────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"🔴 Leer: {leer}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"🟡 Niedrig: {niedrig}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Gesamt: {artikel.Count} Artikel");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            int statPad = Math.Max(0, 65 - 8 - leer.ToString().Length - 11 - niedrig.ToString().Length - 10 - artikel.Count.ToString().Length);
            Console.WriteLine(new string(' ', statPad) + "│");
            Console.WriteLine("  ├──────────────────────────────────────────────────────────────────┤");
            Console.ResetColor();

            // Spaltenheader
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"{"#",-4} {"Artikel",-27} {"Bestand",-9} {"Mindest",-9} {"Status",-12}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("     │");
            Console.WriteLine("  ├──────────────────────────────────────────────────────────────────┤");
            Console.ResetColor();

            for (int i = 0; i < artikel.Count; i++)
            {
                var a = artikel[i];
                bool istLeer = a.Anzahl == 0;

                ConsoleColor zeilenFarbe = istLeer ? ConsoleColor.Red : ConsoleColor.Yellow;

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  │  ");

                // Nummer
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[{i + 1:D2}] ");

                // Artikelname
                string name = a.GeraeteName.Length > 26 ? a.GeraeteName.Substring(0, 23) + "..." : a.GeraeteName;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{name,-27}");

                // Bestand
                Console.ForegroundColor = zeilenFarbe;
                Console.Write($"{a.Anzahl,-9}");

                // Mindestbestand
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{a.Mindestbestand,-9}");

                // Status-Badge
                Console.ForegroundColor = zeilenFarbe;
                string statusText = istLeer ? "🔴 LEER" : "🟡 NIEDRIG";
                Console.Write($"{statusText,-12}");

                // Hersteller-Tag
                if (!string.IsNullOrWhiteSpace(a.Hersteller) &&
                    !a.Hersteller.Equals("Unbekannt", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"· {a.Hersteller}");
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" │");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeArtikelDetailKarte(InvId artikel)
        {
            bool istLeer = artikel.Anzahl == 0;
            ConsoleColor fc = istLeer ? ConsoleColor.Red : ConsoleColor.Yellow;
            string suchbegriff = BaueOptimalenSuchbegriff(artikel);

            Console.ForegroundColor = fc;
            Console.WriteLine("  ┌─ AUSGEWÄHLTER ARTIKEL ───────────────────────────────────────────┐");

            // Zeile 1: Name
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("📦  ");
            Console.ForegroundColor = ConsoleColor.White;
            string name = artikel.GeraeteName.Length > 55 ? artikel.GeraeteName.Substring(0, 52) + "..." : artikel.GeraeteName;
            Console.Write($"{name}");
            Console.ForegroundColor = fc;
            Console.WriteLine(new string(' ', Math.Max(0, 63 - name.Length)) + "│");

            // Zeile 2: Hersteller + Inv-Nr
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("🏭  Hersteller: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            string hersteller = artikel.Hersteller ?? "—";
            Console.Write($"{hersteller,-20}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Inv-Nr: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{artikel.InvNmr}");
            Console.ForegroundColor = fc;
            Console.WriteLine(new string(' ', Math.Max(0, 65 - hersteller.Length - artikel.InvNmr.Length - 28)) + "│");

            // Zeile 3: Bestand
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("📊  Bestand: ");
            Console.ForegroundColor = fc;
            Console.Write($"{artikel.Anzahl}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" / Mindestbestand: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{artikel.Mindestbestand}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   Fehlend: ");
            Console.ForegroundColor = ConsoleColor.Red;
            int fehlend = Math.Max(0, artikel.Mindestbestand - artikel.Anzahl);
            Console.Write($"{fehlend} Stück");
            Console.ForegroundColor = fc;
            Console.WriteLine(new string(' ', Math.Max(0, 65 - fehlend.ToString().Length - artikel.Anzahl.ToString().Length - artikel.Mindestbestand.ToString().Length - 40)) + "│");

            // Zeile 4: Suchbegriff
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("🔎  Suchbegriff: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            string sb = suchbegriff.Length > 47 ? suchbegriff.Substring(0, 44) + "..." : suchbegriff;
            Console.Write(sb);
            Console.ForegroundColor = fc;
            Console.WriteLine(new string(' ', Math.Max(0, 65 - sb.Length - 17)) + "│");

            Console.ForegroundColor = fc;
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeShopListe()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌─ SHOPS ──────────────────────────────────────────────────────────┐");
            Console.ResetColor();

            for (int i = 0; i < Shops.Count; i++)
            {
                var shop = Shops[i];
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  │  ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[{i + 1}]  ");
                Console.ForegroundColor = shop.Farbe;
                Console.Write($"{shop.Icon}  {shop.Name,-18}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"· {shop.Beschreibung}");
                int shopPad = Math.Max(0, 65 - shop.Name.Length - shop.Beschreibung.Length - 10);
                Console.WriteLine(new string(' ', shopPad) + "│");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ├──────────────────────────────────────────────────────────────────┤");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[A]  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("🌐  Alle Shops gleichzeitig öffnen");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("                             │");
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        private static void ZeigeAlleBestaendeOK()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                  ║");
            Console.WriteLine("  ║   ✅  ALLE BESTÄNDE IM NORMALEN BEREICH                          ║");
            Console.WriteLine("  ║                                                                  ║");
            Console.WriteLine("  ║   Kein Artikel hat den Mindestbestand erreicht.                  ║");
            Console.WriteLine("  ║   Eine Nachbestellung ist aktuell nicht erforderlich.             ║");
            Console.WriteLine("  ║                                                                  ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void ZeigeAuswahlHinweis(int anzahl)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────┐");
            Console.Write("  │  ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"Artikel-Nummer [1–{anzahl}] wählen um Shop-Links zu generieren.");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            int p = Math.Max(0, 65 - anzahl.ToString().Length - 46);
            Console.WriteLine(new string(' ', p) + "│");
            Console.WriteLine("  └──────────────────────────────────────────────────────────────────┘");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeLadeAnimation(string text)
        {
            string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            Console.ForegroundColor = ConsoleColor.Yellow;
            for (int i = 0; i < 12; i++)
            {
                Console.Write($"\r  {frames[i % frames.Length]}  {text}...");
                System.Threading.Thread.Sleep(55);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        private static void ZeigeFehler(string nachricht)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ✖  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(nachricht);
            Console.ResetColor();
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  SHOP-MODELL
    // ══════════════════════════════════════════════════════════════════
    internal class Shop
    {
        public string Name { get; }
        public string Icon { get; }
        public ConsoleColor Farbe { get; }
        public string UrlTemplate { get; }
        public string Beschreibung { get; }

        public Shop(string name, string icon, ConsoleColor farbe, string urlTemplate, string beschreibung)
        {
            Name = name;
            Icon = icon;
            Farbe = farbe;
            UrlTemplate = urlTemplate;
            Beschreibung = beschreibung;
        }
    }
}