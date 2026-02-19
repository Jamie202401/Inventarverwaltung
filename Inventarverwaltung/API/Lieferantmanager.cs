using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Inventarverwaltung.Manager.UI;

namespace Inventarverwaltung
{
    // ═══════════════════════════════════════════════════════════════════
    // LIEFERANT-MODELL
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Repräsentiert einen Lieferanten mit validierter Adresse.
    /// </summary>
    public class Lieferant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ansprechpartner { get; set; }
        public string Telefon { get; set; }
        public string Email { get; set; }

        // Adresse (vom Benutzer eingegeben)
        public string Strasse { get; set; }
        public string PLZ { get; set; }
        public string Stadt { get; set; }
        public string Land { get; set; }

        // Von Nominatim validiert & befüllt
        public string AdresseValidiert { get; set; }   // formatierte Adresse von Nominatim
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IstValidiert { get; set; }

        // Verknüpfte Artikel
        public List<string> GelieferteArtikelIds { get; set; } = new List<string>();

        public string VolleAdresse => $"{Strasse}, {PLZ} {Stadt}, {Land}";
    }

    // ═══════════════════════════════════════════════════════════════════
    // NOMINATIM API — Hilfsklassen für JSON-Deserialisierung
    // ═══════════════════════════════════════════════════════════════════

    internal class NominatimResult
    {
        public string display_name { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public NominatimAddress address { get; set; }
    }

    internal class NominatimAddress
    {
        public string road { get; set; }
        public string house_number { get; set; }
        public string postcode { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string village { get; set; }
        public string country { get; set; }
        public string state { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════
    // LIEFERANT MANAGER — HAUPTKLASSE
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Verwaltet Lieferanten und validiert Adressen über die
    /// kostenlose Nominatim API (OpenStreetMap) — kein API-Key nötig.
    ///
    /// NuGet:      Kein zusätzliches Paket nötig
    ///             System.Net.Http + System.Text.Json (in .NET 6+ enthalten)
    ///
    /// API-Docs:   https://nominatim.org/release-docs/develop/api/Search/
    /// Kosten:     Komplett kostenlos
    /// Rate-Limit: Max. 1 Anfrage/Sekunde (wird hier eingehalten)
    /// </summary>
    public static class LieferantManager
    {
        // Verweist direkt auf DataManager — Daten werden dauerhaft gespeichert
        private static List<Lieferant> _lieferanten => DataManager.Lieferanten;

        // HttpClient — einmal erstellen, immer wiederverwenden
        private static readonly HttpClient _httpClient = new HttpClient();

        // Nominatim erfordert einen User-Agent (Pflicht laut API-Richtlinien)
        private const string USER_AGENT    = "Inventarverwaltung/1.0 (lokale Anwendung)";
        private const string NOMINATIM_URL = "https://nominatim.openstreetmap.org/search";

        // ═══════════════════════════════════════════════════════════════
        // HAUPTMENÜ
        // ═══════════════════════════════════════════════════════════════

        public static void ZeigeLieferantMenu()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("Lieferanten-Verwaltung", ConsoleColor.DarkGreen);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  📍 Adressen werden automatisch via OpenStreetMap (Nominatim) validiert");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1] 📋 Alle Lieferanten anzeigen");
                Console.WriteLine("  [2] ➕ Neuen Lieferanten anlegen");
                Console.WriteLine("  [3] 🔍 Lieferant suchen");
                Console.WriteLine("  [4] 🗺️  Adresse auf Karte öffnen");
                Console.WriteLine("  [5] ✏️  Lieferant bearbeiten");
                Console.WriteLine("  [6] 🗑️  Lieferant löschen");
                Console.WriteLine("  [7] 🔄 Adresse neu validieren");
                Console.WriteLine();
                Console.WriteLine("  [0] ↩️  Zurück");
                Console.ResetColor();

                Console.WriteLine();
                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");

                switch (auswahl)
                {
                    case "1": ZeigeLieferanten(); break;
                    case "2": LieferantAnlegen().GetAwaiter().GetResult(); break;
                    case "3": LieferantSuchen(); break;
                    case "4": AdresseAufKarteOeffnen(); break;
                    case "5": LieferantBearbeiten().GetAwaiter().GetResult(); break;
                    case "6": LieferantLoeschen(); break;
                    case "7": AdresseNeuValidieren().GetAwaiter().GetResult(); break;
                    case "0": return;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // [1] ALLE LIEFERANTEN ANZEIGEN
        // ═══════════════════════════════════════════════════════════════

        private static void ZeigeLieferanten()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Alle Lieferanten", ConsoleColor.Cyan);
            Console.WriteLine();

            if (_lieferanten.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Lieferanten vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            foreach (var l in _lieferanten)
            {
                string validBadge  = l.IstValidiert ? "✔ Validiert" : "⚠ Nicht validiert";
                ConsoleColor color = l.IstValidiert ? ConsoleColor.Green : ConsoleColor.Yellow;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  ┌─ [{l.Id}] {l.Name}");
                Console.ResetColor();
                Console.WriteLine($"  │  Ansprechpartner:  {l.Ansprechpartner}");
                Console.WriteLine($"  │  Telefon:          {l.Telefon}");
                Console.WriteLine($"  │  E-Mail:           {l.Email}");
                Console.Write($"  │  Adresse:          ");

                if (l.IstValidiert)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(l.AdresseValidiert);
                    Console.ResetColor();
                    Console.WriteLine($"  │  Koordinaten:      {l.Latitude:F5}, {l.Longitude:F5}");
                }
                else
                {
                    Console.WriteLine(l.VolleAdresse);
                }

                Console.Write($"  │  Status:           ");
                Console.ForegroundColor = color;
                Console.WriteLine(validBadge);
                Console.ResetColor();
                Console.WriteLine($"  └{'─',60}");
                Console.WriteLine();
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // [2] NEUEN LIEFERANTEN ANLEGEN + ADRESSE VALIDIEREN
        // ═══════════════════════════════════════════════════════════════

        private static async Task LieferantAnlegen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Neuen Lieferanten anlegen", ConsoleColor.Green);
            Console.WriteLine();

            string name = ConsoleHelper.GetInput("Firmenname (oder 'x' zum Abbrechen)");
            if (name.ToLower() == "x") return;
            if (string.IsNullOrWhiteSpace(name))
            {
                ConsoleHelper.PrintError("Name darf nicht leer sein!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            string ansprechpartner = ConsoleHelper.GetInput("Ansprechpartner");
            string telefon         = ConsoleHelper.GetInput("Telefon");
            string email           = ConsoleHelper.GetInput("E-Mail");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📍 Adresse eingeben — wird automatisch via Nominatim validiert:");
            Console.ResetColor();
            Console.WriteLine();

            string strasse = ConsoleHelper.GetInput("Straße + Hausnummer");
            string plz     = ConsoleHelper.GetInput("PLZ");
            string stadt   = ConsoleHelper.GetInput("Stadt");
            string land    = ConsoleHelper.GetInput("Land (z.B. Deutschland)");

            var lieferant = new Lieferant
            {
                Id              = $"LF{_lieferanten.Count + 1:D3}",
                Name            = name,
                Ansprechpartner = ansprechpartner,
                Telefon         = telefon,
                Email           = email,
                Strasse         = strasse,
                PLZ             = plz,
                Stadt           = stadt,
                Land            = land,
                IstValidiert    = false
            };

            // ── Adresse via Nominatim API validieren ───────────────────
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  🔄 Sende Anfrage an Nominatim API...");
            Console.ResetColor();

            bool validiert = await ValidiereAdresse(lieferant);

            if (validiert)
            {
                Console.WriteLine();
                ConsoleHelper.PrintSuccess("✔ Adresse erfolgreich validiert!");
                Console.WriteLine();
                Console.WriteLine($"  Gefundene Adresse:  {lieferant.AdresseValidiert}");
                Console.WriteLine($"  Koordinaten:        {lieferant.Latitude:F5}, {lieferant.Longitude:F5}");
                Console.WriteLine();

                string bestaetigung = ConsoleHelper.GetInput("Ist diese Adresse korrekt? (ja/nein)");
                if (bestaetigung.ToLower() != "ja")
                {
                    lieferant.IstValidiert = false;
                    ConsoleHelper.PrintInfo("Adresse als nicht validiert gespeichert.");
                }
            }
            else
            {
                Console.WriteLine();
                ConsoleHelper.PrintWarning("Adresse konnte nicht validiert werden.");
                Console.WriteLine("  Lieferant wird trotzdem gespeichert — Adresse später manuell prüfen.");
            }

            // ── Hinzufügen UND dauerhaft speichern ────────────────────
            _lieferanten.Add(lieferant);
            DataManager.SaveLieferanten();

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"✔ Lieferant '{lieferant.Name}' [{lieferant.Id}] gespeichert!");

            try
            {
                LogManager.LogDatenGespeichert("Lieferant-Anlage",
                    $"Neu: {lieferant.Name} | Adresse validiert: {lieferant.IstValidiert}");
            }
            catch { /* LogManager optional */ }

            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // NOMINATIM API — KERN-METHODE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Ruft die Nominatim API auf und befüllt Latitude, Longitude
        /// und AdresseValidiert des Lieferanten.
        ///
        /// Beispiel-Request:
        /// GET https://nominatim.openstreetmap.org/search
        ///     ?q=Hauptstraße+5,+70173+Stuttgart,+Deutschland
        ///     &format=json
        ///     &addressdetails=1
        ///     &limit=1
        ///
        /// Beispiel-Response:
        /// [{
        ///   "display_name": "5, Hauptstraße, Stuttgart, Baden-Württemberg, Deutschland",
        ///   "lat": "48.77585",
        ///   "lon": "9.18304",
        ///   "address": { "road": "Hauptstraße", "city": "Stuttgart", ... }
        /// }]
        /// </summary>
        private static async Task<bool> ValidiereAdresse(Lieferant lieferant)
        {
            try
            {
                // Adresse als Suchstring zusammenbauen
                string suchanfrage  = $"{lieferant.Strasse}, {lieferant.PLZ} {lieferant.Stadt}, {lieferant.Land}";
                string encodedQuery = Uri.EscapeDataString(suchanfrage);
                string url          = $"{NOMINATIM_URL}?q={encodedQuery}&format=json&addressdetails=1&limit=1";

                // ── HTTP GET an Nominatim ──────────────────────────────
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ✗ API-Fehler: HTTP {(int)response.StatusCode}");
                    Console.ResetColor();
                    return false;
                }

                string json = await response.Content.ReadAsStringAsync();

                // ── JSON-Antwort parsen ────────────────────────────────
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<List<NominatimResult>>(json, options);

                if (results == null || results.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ⚠ Kein Ergebnis gefunden — Adresse prüfen.");
                    Console.ResetColor();
                    return false;
                }

                // Erstes Ergebnis übernehmen
                var ergebnis = results[0];

                lieferant.Latitude         = double.Parse(ergebnis.lat,
                                                System.Globalization.CultureInfo.InvariantCulture);
                lieferant.Longitude        = double.Parse(ergebnis.lon,
                                                System.Globalization.CultureInfo.InvariantCulture);
                lieferant.AdresseValidiert = ergebnis.display_name;
                lieferant.IstValidiert     = true;

                // Rate-Limit einhalten: Nominatim erlaubt max. 1 Anfrage/Sekunde
                await Task.Delay(1100);

                return true;
            }
            catch (HttpRequestException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⚠ Keine Internetverbindung — Adresse ohne Validierung gespeichert.");
                Console.ResetColor();
                return false;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Unerwarteter Fehler: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // [3] LIEFERANT SUCHEN
        // ═══════════════════════════════════════════════════════════════

        private static void LieferantSuchen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Lieferant suchen", ConsoleColor.Cyan);
            Console.WriteLine();

            string suchbegriff = ConsoleHelper.GetInput("Name, Stadt oder Ansprechpartner");
            if (string.IsNullOrWhiteSpace(suchbegriff)) return;

            var gefunden = _lieferanten.Where(l =>
                l.Name.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase)           ||
                l.Stadt.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase)          ||
                l.Ansprechpartner.Contains(suchbegriff, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine();

            if (gefunden.Count == 0)
            {
                ConsoleHelper.PrintWarning($"Kein Lieferant gefunden für '{suchbegriff}'.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  {gefunden.Count} Ergebnis(se) für '{suchbegriff}':");
                Console.ResetColor();
                Console.WriteLine();

                foreach (var l in gefunden)
                {
                    string status = l.IstValidiert ? "✔" : "⚠";
                    Console.ForegroundColor = l.IstValidiert ? ConsoleColor.Green : ConsoleColor.Yellow;
                    Console.Write($"  {status} ");
                    Console.ResetColor();
                    Console.WriteLine($"[{l.Id}] {l.Name} — {l.Stadt} — {l.Ansprechpartner}");
                }
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // [4] ADRESSE AUF KARTE ÖFFNEN (Google Maps im Browser)
        // ═══════════════════════════════════════════════════════════════

        private static void AdresseAufKarteOeffnen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Adresse auf Karte öffnen", ConsoleColor.Cyan);
            Console.WriteLine();

            Lieferant lieferant = WaehlLieferant();
            if (lieferant == null) return;

            string url;

            if (lieferant.IstValidiert && lieferant.Latitude != 0)
            {
                // Exakte GPS-Koordinaten von Nominatim verwenden (präziser)
                string lat = lieferant.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string lon = lieferant.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                url = $"https://www.google.com/maps?q={lat},{lon}";
            }
            else
            {
                // Adresse als Suchbegriff (Fallback ohne Koordinaten)
                string adresse = Uri.EscapeDataString(lieferant.VolleAdresse);
                url = $"https://www.google.com/maps/search/{adresse}";
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  🗺  Öffne Google Maps für: {lieferant.Name}");
            Console.WriteLine($"  📍 {(lieferant.IstValidiert ? lieferant.AdresseValidiert : lieferant.VolleAdresse)}");
            Console.ResetColor();

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                ConsoleHelper.PrintSuccess("✔ Browser geöffnet!");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Browser konnte nicht geöffnet werden: {ex.Message}");
                Console.WriteLine($"  Bitte manuell öffnen: {url}");
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // [5] LIEFERANT BEARBEITEN
        // ═══════════════════════════════════════════════════════════════

        private static async Task LieferantBearbeiten()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Lieferant bearbeiten", ConsoleColor.Yellow);
            Console.WriteLine();

            Lieferant lieferant = WaehlLieferant();
            if (lieferant == null) return;

            Console.WriteLine();
            Console.WriteLine($"  [1] Name:            {lieferant.Name}");
            Console.WriteLine($"  [2] Ansprechpartner: {lieferant.Ansprechpartner}");
            Console.WriteLine($"  [3] Telefon:         {lieferant.Telefon}");
            Console.WriteLine($"  [4] E-Mail:          {lieferant.Email}");
            Console.WriteLine($"  [5] Adresse:         {lieferant.VolleAdresse}");
            Console.WriteLine();

            string feld = ConsoleHelper.GetInput("Welches Feld? (1-5 oder 'x')");
            if (feld.ToLower() == "x") return;

            switch (feld)
            {
                case "1":
                    string neuerName = ConsoleHelper.GetInput($"Neuer Name (aktuell: {lieferant.Name})");
                    if (!string.IsNullOrWhiteSpace(neuerName)) lieferant.Name = neuerName;
                    break;
                case "2":
                    string neuerAP = ConsoleHelper.GetInput($"Neuer Ansprechpartner (aktuell: {lieferant.Ansprechpartner})");
                    if (!string.IsNullOrWhiteSpace(neuerAP)) lieferant.Ansprechpartner = neuerAP;
                    break;
                case "3":
                    string neuesTel = ConsoleHelper.GetInput($"Neue Telefonnummer (aktuell: {lieferant.Telefon})");
                    if (!string.IsNullOrWhiteSpace(neuesTel)) lieferant.Telefon = neuesTel;
                    break;
                case "4":
                    string neueEmail = ConsoleHelper.GetInput($"Neue E-Mail (aktuell: {lieferant.Email})");
                    if (!string.IsNullOrWhiteSpace(neueEmail)) lieferant.Email = neueEmail;
                    break;
                case "5":
                    lieferant.Strasse = ConsoleHelper.GetInput($"Straße (aktuell: {lieferant.Strasse})");
                    lieferant.PLZ     = ConsoleHelper.GetInput($"PLZ (aktuell: {lieferant.PLZ})");
                    lieferant.Stadt   = ConsoleHelper.GetInput($"Stadt (aktuell: {lieferant.Stadt})");
                    lieferant.Land    = ConsoleHelper.GetInput($"Land (aktuell: {lieferant.Land})");

                    Console.WriteLine();
                    ConsoleHelper.PrintInfo("Validiere neue Adresse via Nominatim...");
                    lieferant.IstValidiert = false;
                    bool ok = await ValidiereAdresse(lieferant);

                    if (ok)
                        ConsoleHelper.PrintSuccess($"✔ Validiert: {lieferant.AdresseValidiert}");
                    else
                        ConsoleHelper.PrintWarning("Adresse konnte nicht validiert werden.");
                    break;
                default:
                    ConsoleHelper.PrintError("Ungültige Auswahl!");
                    ConsoleHelper.PressKeyToContinue();
                    return;
            }

            // ── Dauerhaft speichern ────────────────────────────────────
            DataManager.SaveLieferanten();
            ConsoleHelper.PrintSuccess($"✔ Lieferant '{lieferant.Name}' gespeichert!");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // [6] LIEFERANT LÖSCHEN
        // ═══════════════════════════════════════════════════════════════

        private static void LieferantLoeschen()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Lieferant löschen", ConsoleColor.Red);
            Console.WriteLine();

            Lieferant lieferant = WaehlLieferant();
            if (lieferant == null) return;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ⚠️  '{lieferant.Name}' wird unwiderruflich gelöscht!");
            Console.ResetColor();
            Console.WriteLine();

            string bestaetigung = ConsoleHelper.GetInput("Wirklich löschen? (ja/nein)");
            if (bestaetigung.ToLower() != "ja")
            {
                ConsoleHelper.PrintInfo("Löschen abgebrochen.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            string geloeschterName = lieferant.Name;

            // ── Löschen UND dauerhaft speichern ───────────────────────
            _lieferanten.Remove(lieferant);
            DataManager.SaveLieferanten();

            ConsoleHelper.PrintSuccess($"✔ Lieferant '{geloeschterName}' gelöscht!");
            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // [7] ADRESSE NEU VALIDIEREN
        // ═══════════════════════════════════════════════════════════════

        private static async Task AdresseNeuValidieren()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Adresse neu validieren", ConsoleColor.Cyan);
            Console.WriteLine();

            Lieferant lieferant = WaehlLieferant();
            if (lieferant == null) return;

            Console.WriteLine();
            ConsoleHelper.PrintInfo($"Sende Anfrage für: {lieferant.VolleAdresse}");

            bool erfolg = await ValidiereAdresse(lieferant);

            Console.WriteLine();
            if (erfolg)
            {
                ConsoleHelper.PrintSuccess("✔ Adresse erfolgreich validiert!");
                Console.WriteLine($"  Adresse:     {lieferant.AdresseValidiert}");
                Console.WriteLine($"  Koordinaten: {lieferant.Latitude:F5}, {lieferant.Longitude:F5}");
            }
            else
            {
                ConsoleHelper.PrintError("Adresse konnte nicht validiert werden.");
                Console.WriteLine("  Tipp: Straße, PLZ und Stadt prüfen.");
            }

            ConsoleHelper.PressKeyToContinue();
        }

        // ═══════════════════════════════════════════════════════════════
        // HILFSMETHODE — LIEFERANT AUSWÄHLEN
        // ═══════════════════════════════════════════════════════════════

        private static Lieferant WaehlLieferant()
        {
            if (_lieferanten.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Lieferanten vorhanden.");
                ConsoleHelper.PressKeyToContinue();
                return null;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Lieferanten:");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < _lieferanten.Count; i++)
            {
                var l      = _lieferanten[i];
                string sym = l.IstValidiert ? "✔" : "⚠";
                Console.ForegroundColor = l.IstValidiert ? ConsoleColor.Green : ConsoleColor.Yellow;
                Console.Write($"  [{i + 1}] {sym} ");
                Console.ResetColor();
                Console.WriteLine($"{l.Name} — {l.Stadt}");
            }

            Console.WriteLine();
            string auswahl = ConsoleHelper.GetInput("Nummer (oder 'x' zum Abbrechen)");
            if (auswahl.ToLower() == "x") return null;

            if (!int.TryParse(auswahl, out int nr) || nr < 1 || nr > _lieferanten.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl!");
                ConsoleHelper.PressKeyToContinue();
                return null;
            }

            return _lieferanten[nr - 1];
        }
    }
}