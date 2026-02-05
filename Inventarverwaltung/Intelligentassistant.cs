using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;


namespace Inventarverwaltung
{
    /// <summary>
    ///    Intelligenter Assistent für das System
    ///    Arbeitet komplett Lokal - Keine Verbindung möglich heißt auch offline möglich
    ///    Analysiert die Eingaben Muster und gibt Verbesserungen ode Vorschläge
    /// </summary>

    public static class IntelligentAssistant
    {
        private static Dictionary<string, List<string>> geraeteZuAbteilung = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> mitarbeiterZuGeraete = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> haeufigsteAbteilungen = new Dictionary<string, int>();


        ///<summary>
        ///Initalisiert die KI und lernt aus vorhandenen eingaben und Listen
        /// </summary>
        public static void IniializeAI()
        {
            LerneAusVorhandenenDaten();
            ConsoleHelper.PrintSuccess("🤖 Intilligenter Assistent aktiviert ");
        }


        ///<summary>
        ///Lernt aus allen aktuell vorhandenen Dateien die zur aktuellen Zeit im System liegen
        /// </summary>

        private static void LerneAusVorhandenenDaten()
        {
            //Aus dem Inventar lernen

            foreach (var artikel in DataManager.Inventar)
            {
                //Merken welche Mitarbeiter welche Geräte haben
                var mitarbeiter = DataManager.Mitarbeiter.FirstOrDefault(m => $"{m.VName} {m.NName}".Equals(artikel.MitarbeiterBezeichnung, StringComparison.OrdinalIgnoreCase));

                if (mitarbeiter != null)
                {
                    //Merke: Welche Geräte welche Mitarbeiter aus den Abteilungen haben
                    if (!geraeteZuAbteilung.ContainsKey(artikel.GeraeteName.ToLower())) ;
                    {
                        geraeteZuAbteilung[artikel.GeraeteName.ToLower()] = new List<string>();
                    }
                    geraeteZuAbteilung[artikel.GeraeteName.ToLower()].Add(mitarbeiter.Abteilung);

                    //Merke: Welche Geräte hat dieser Mitarbeiter schon
                    string mitarbeiterKey = artikel.MitarbeiterBezeichnung.ToLower();

                    if (!mitarbeiterZuGeraete.ContainsKey(mitarbeiterKey))
                    {
                        mitarbeiterZuGeraete[mitarbeiterKey] = new List<string>();
                    }
                    mitarbeiterZuGeraete[mitarbeiterKey].Add(artikel.GeraeteName);
                }
            }
            foreach (var mitarbeiter in DataManager.Mitarbeiter)
            {
                string abteilung = mitarbeiter.Abteilung.ToLower();

                if (!haeufigsteAbteilungen.ContainsKey(abteilung))
                {
                    haeufigsteAbteilungen[abteilung] = 0;
                }
                haeufigsteAbteilungen[abteilung]++;
            }
        }
        #region Inventar-Intelligenz

        ///<summary>
        ///Schlägt passende Inventarnummern vor basierend auf dem Muster
        /// </summary>

        public static string SchlageInventarnummernVor()
        {
            if (DataManager.Inventar.Count == 0)
            {
                return "INV001";
            }

            //Analysiert vorhandene Nummern
            var nummern = DataManager.Inventar
                .Select(i => i.InvNmr)
                .Where(n => n.StartsWith("INV", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (nummern.Count == 0)
            {
                return "INV001";
            }

            int hoechsteNummer = 0;

            foreach (var nummer in nummern)
            {
                string zahlenTeil = new string(nummer.Where(Char.IsDigit).ToArray());
                if (int.TryParse(zahlenTeil, out int num))
                {
                    hoechsteNummer = Math.Max(hoechsteNummer, num);
                }
            }
            return $"INV {(hoechsteNummer + 1):D3}";
        }

        ///<summary>
        ///Schlägt passende Mitarbeiter vor basierend auf Gerät und Kontext
        /// </summary>

        public static List<string> SchlageMitarbeiterVor(string geraeteName, int maxVorschlaege = 3)
        {
            var vorschlaege = new List<string>();

            if (string.IsNullOrWhiteSpace(geraeteName) || DataManager.Mitarbeiter.Count == 0)
            {
                return vorschlaege;
            }

            // 1. Prüfen: Welche Abteilung nutzt normalerweiße dieses Gerät ?

            string geraeteLower = geraeteName.ToLower();
            List<string> typischeAbteilungen = new List<string>();

            //Intelligente Geräte Kategorisierung
            if (GeraetEnthaelt(geraeteName, "Laptop", "Notebook", "Computer", "PC", "VM"))
            {
                typischeAbteilungen = FindeAbteilungFuerGeraet(geraeteLower, new[] { "IT", "Auftragsbearbeitung", "Vertrieb", "Personal", "Buchhaltung" });
            }
            else if (GeraetEnthaelt(geraeteName, "Monitor", "Bildschirm"))
            {
                typischeAbteilungen = FindeAbteilungFuerGeraet(geraeteLower, new[] { "IT", "Auftragsbearbeitung", "Vertrieb", "Personal", "Buchhaltung" });
            }
            else if (GeraetEnthaelt(geraeteName, "Drucker", "Scanner", "Kopierer"))
            {
                typischeAbteilungen = FindeAbteilungFuerGeraet(geraeteLower, new[] { "Verwaltung", "IT" });
            }
            else if (GeraetEnthaelt(geraeteName, "Telefon", "Headset", "Handy"))
            {
                typischeAbteilungen = FindeAbteilungFuerGeraet(geraeteLower, new[] { "IT" });
            }
            else if (GeraetEnthaelt(geraeteName, "Werkzeug", "Akkuschrauber"))
            {
                typischeAbteilungen = FindeAbteilungFuerGeraet(geraeteLower, new[] { "IT", "Reperatur" });
            }

            //2. Finde Mitarbeiter aus diesen Abteilungen die noch kein solches Gerät haben

            foreach (var abteilung in typischeAbteilungen)
            {
                var mitarbeiterAusDieserAbteilung = DataManager.Mitarbeiter
                    .Where(m => m.Abteilung.Equals(abteilung, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var mitarbeiter in mitarbeiterAusDieserAbteilung)
                {
                    string mitarbeiterName = $"{mitarbeiter.VName} {mitarbeiter.NName}";
                    string mitarbeiterKey = mitarbeiterName.ToLower();

                    bool hatBereitsAehnlichesGeraet = false;

                    if (mitarbeiterZuGeraete.ContainsKey(mitarbeiterKey))
                    {
                        hatBereitsAehnlichesGeraet = mitarbeiterZuGeraete[mitarbeiterKey]
                            .Any(g => IstAehnlichesgeraet(g, geraeteName));
                    }
                    if (!hatBereitsAehnlichesGeraet && !vorschlaege.Contains(mitarbeiterName))
                    {
                        vorschlaege.Add(mitarbeiterName);
                        if (vorschlaege.Count >= maxVorschlaege)
                            return vorschlaege;
                    }
                }
            }
            if (vorschlaege.Count < maxVorschlaege)
            {
                var mitarbeiterMitWenigenGeraeten = DataManager.Mitarbeiter
                    .Select(m => new
                    {
                        Mitarbeiter = m,
                        AnzahlGeraete = DataManager.Inventar.Count(i => i.MitarbeiterBezeichnung.Equals($"{m.VName} {m.NName}", StringComparison.OrdinalIgnoreCase))
                    })
                    .OrderBy(x => x.AnzahlGeraete)
                    .Take(maxVorschlaege - vorschlaege.Count);

                foreach (var item in mitarbeiterMitWenigenGeraeten)
                {
                    string name = $"{item.Mitarbeiter.VName} {item.Mitarbeiter.NName}";

                    if (!vorschlaege.Contains(name))
                    {
                        vorschlaege.Add(name);
                    }
                }
            }
            return vorschlaege;
        }
        ///<summary>
        ///Analysiert die Gerätenamen und gibt zusätzliche Infos dazu
        /// </summary>

        public static string Analysieregeraete(string geraeteName)
        {
            StringBuilder analyse = new StringBuilder();

            //Kategoriesieren der einzelnen Gruppen

            string kategorie = "Sonstiges";
            if (GeraetEnthaelt(geraeteName, "Laptop", "Notebook"))
                kategorie = "Mobile Computer/Pcs";
            else if (GeraetEnthaelt(geraeteName, "Desktop", "pc", "Computer"))
                kategorie = "Feste Computer/Pcs";
            else if (GeraetEnthaelt(geraeteName, "Monitor", "Bildschirm"))
                kategorie = "Display-Geräte";
            else if (GeraetEnthaelt(geraeteName, "Drucker", "Scanner", "Kopierer"))
                kategorie = "Büro-Geräte";
            else if (GeraetEnthaelt(geraeteName, "Server"))
                kategorie = "Infrastruktur";
            else if (GeraetEnthaelt(geraeteName, "Telefon", "Handy", "Smartphone"))
                kategorie = "Kommunikation";
            else if (GeraetEnthaelt(geraeteName, "Tabelt", "Handscanner"))
                kategorie = " Mobile Devices für SPs";

            analyse.AppendLine($"  💡 Kategorie: {kategorie} ");

            //Häufiges vorkommen mehrerer Geräte

            int aehnlicheGeraete = DataManager.Inventar.Count(i => IstAehnlichesgeraet(i.GeraeteName, geraeteName));
            if (aehnlicheGeraete > 0)
            {
                analyse.AppendLine($"   📊 {aehnlicheGeraete} ähnliche(s) Gerät(e) bereits im System Vorhanden");
            }
            return analyse.ToString();
        }
        #endregion

        #region Mitarbeiter-Intelligenz

        ///<summary>
        /// Achlägt Abteilungen vor basierend auf Mustern
        /// </summary>
        /// 
        public static List<string> SchlageAbteilungenVor()
        {
            var vorschlaege = haeufigsteAbteilungen
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => CapitalizeFirst(kvp.Key))
                .Take(5)
                .ToList();

            //Ergänze Standard-Abetilungen, falls wenig Daten vorhanden Sind

            var standardAbteilungen = new[] { "IT", "Verwaltung", "Auftragsbearbeitung", "Buchhaltung" };

            foreach (var abteilung in standardAbteilungen)
            {
                if (!vorschlaege.Any(v => v.Equals(abteilung, StringComparison.OrdinalIgnoreCase)))
                {
                    vorschlaege.Add(abteilung);
                }
            }
            return vorschlaege.Take(5).ToList();
        }
        ///<summary>
        /// prüft, ob ein Mitarbeiter in Frage kommt
        /// </summary>
        public static string PruefeNamePlausibilitaet(string vorname, string nachname)
        {
            StringBuilder feedback = new StringBuilder();

            if (vorname.Length == 1)
                feedback.AppendLine("   ⚠️ Vorname sehr kurz - ist das korrekt?");
            if (nachname.Length == 1)
                feedback.AppendLine("   ⚠️ Nachname sehr kurz - ist das korrekt?");

            if (vorname.Any(c => Char.IsDigit(c)))
                feedback.AppendLine("   ⚠️ Vorname enthält Zahlen - ist das korrekt?");
            if (nachname.Any(c => Char.IsDigit(c)))
                feedback.AppendLine("⚠️ Vorname enthält Zahlen - ist das korrekt ?");


            //Warung ausgeben bei ähnlichen Namen
            foreach (var m in DataManager.Mitarbeiter)
            {
                int aehnlichkeitVorname = BerechneLevenshteinDistanz(vorname.ToLower(), m.VName.ToLower());
                int aehnlichkeitNachname = BerechneLevenshteinDistanz(nachname.ToLower(), m.NName.ToLower());

                if (aehnlichkeitNachname <= 2 && aehnlichkeitNachname <= 2)
                {
                    feedback.AppendLine($"    💡  Ähnlich zu: {m.VName} {m.VName} - Tippfehler ?");
                }
            }
            return feedback.ToString();
        }
        ///<summary>
        ///Analysiert Abteilungsverteilung und gibt Empfehlungen
        /// </summary>

        public static string AnalysiereAbteilungsverteilung()
        {
            if (DataManager.Mitarbeiter.Count == 0)
                return "";

            var verteilung = DataManager.Mitarbeiter
                .GroupBy(m => m.Abteilung.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(3);

            StringBuilder analyse = new StringBuilder();
            analyse.AppendLine("    📊 Abteilungsverteilung:");

            foreach (var gruppe in verteilung)
            {
                analyse.AppendLine($"       .{CapitalizeFirst(gruppe.Key)}: {gruppe.Count()} Mitarbeiter");
            }
            return analyse.ToString();
        }
        #endregion

        #region Intelligente Duplikat Erkennung

        ///<summary>
        ///Findet vermeintlich erstellte Duplikate und bearbeitet diese
        /// </summary>
        public static List<string> FindePotentielleDuplikate(string suchtext, List<string> vergleichsListe)
        {
            var potentielleDuplikate = new List<string>();

            foreach (var item in vergleichsListe)
            {
                int distanz = BerechneLevenshteinDistanz(suchtext.ToLower(), item.ToLower());

                //Wenn nur 1-3 Zeichen unterschiedlich sind - > Tippfehler ?

                if (distanz > 0 && distanz <= 3)
                {
                    potentielleDuplikate.Add(item);
                }
            }
            return potentielleDuplikate;
        }
        #endregion

        #region Benutzer-Intelligenz

        ///<summary>
        ///Schlägt Berechtigungen vor basierend auf Mustern
        /// </summary>
        public static string SchlageBerechtigungVor(string benutzername)
        {
            string lowerName = benutzername.ToLower();

            if (lowerName.Contains("admin") || lowerName.Contains("chef") || lowerName.Contains("leiter"))
            {
                return "💡 Empfehlung: Admin (basierend auf Benutzername)";
            }
            if (lowerName.Contains("praktikant") || lowerName.Contains("azubi") || lowerName.Contains("Mitarbeiter"))
            {
                return "💡 Empfehlung: User (basierend auf Benutzername)";
            }

            int anzahlAdmin = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.Admin);
            int anzahlUser = DataManager.Benutzer.Count(b => b.Berechtigung == Berechtigungen.User);

            if (anzahlAdmin == 0)
            {
                return "💡 Empfehlung: Admin (noch kein Admin im System)";
            }
            if (anzahlUser >= anzahlUser)
            {
                return "💡 Empfehlung: User (ausgewogenes Verhältnis)";
            }
            return "";
        }
        #endregion

        #region Hilfsfunktionen

        ///<summary>
        ///Prüft ob ein Gerätename bestimmte Schlusselwörter enthält
        /// </summary>

        private static bool GeraetEnthaelt(string geraeteName, params string[] schluesselwoerter)
        {
            string lower = geraeteName.ToLower();
            return schluesselwoerter.Any(s => lower.Contains(s.ToLower()));
        }

        ///<summary>
        ///Findet Abteilungen die typisch für ein Gerät sind
        /// </summary>
        private static List<string> FindeAbteilungFuerGeraet(string geraet, string[] typischeAbteilungen)
        {
            var gefunden = new List<string>();

            if (geraeteZuAbteilung.ContainsKey(geraet))
            {
                gefunden.AddRange(geraeteZuAbteilung[geraet].Distinct());
            }
            foreach (var abteilung in typischeAbteilungen)
            {
                if (DataManager.Mitarbeiter.Any(a => a.Abteilung.Equals(abteilung, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!gefunden.Any(a => a.Equals(abteilung, StringComparison.OrdinalIgnoreCase)))
                    {

                    }
                }
            }
            return gefunden;
        }
        ///<summary>
        ///prüft ob zwei Geräte ähnliche sind
        /// </summary>

        private static bool IstAehnlichesgeraet(string geraet1, string generaet2)
        {
            string g1 = geraet1.ToLower();
            string g2 = generaet2.ToLower();

            //Genaue Übereinstimmung
            if (g1 == g2) return true;

            //Gleiche kaegorie ?
            string[] kategorien = { "Laptop", "Notebook", "Desktop", "PC", "Monitor", "Drucker", "Scanner", "Kopierer", "Teblet", "Handy", "Smartphone" };
            foreach (var kat in kategorien)
            {
                if (g1.Contains(kat) && g2.Contains(kat))
                    return true;
            }
            return false;
        }

        ///<summary>
        /// Berechnet Levenshtein-Distanz (Edit-Distance) zwischen zwei Strings
        /// Misst wie viele Änderungen nötig sind um von String A zu String B zu kommen
        ///</summary>

        private static int BerechneLevenshteinDistanz(string s1, string s2)
        {
            // Erstelle eine 2D-Matrix (Tabelle) mit (Länge von s1 + 1) Zeilen und (Länge von s2 + 1) Spalten
            // Beispiel: "Müller" (6) und "Muller" (6) → Matrix ist 7x7 groß
            // Das +1 ist für das "leere Wort" am Anfang
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            // Fülle die erste Spalte mit 0, 1, 2, 3, 4, 5, 6...
            // Das bedeutet: Um von "" (leer) zu "M", "Mü", "Mül"... zu kommen, braucht man 1, 2, 3... Einfügungen
            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;  // d[0,0]=0, d[1,0]=1, d[2,0]=2 usw.

            // Fülle die erste Zeile mit 0, 1, 2, 3, 4, 5, 6...
            // Das bedeutet: Um von "" (leer) zu "M", "Mu", "Mul"... zu kommen, braucht man 1, 2, 3... Einfügungen
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;  // d[0,0]=0, d[0,1]=1, d[0,2]=2 usw.

            // Durchlaufe alle Spalten (alle Buchstaben von s2)
            for (int j = 1; j <= s2.Length; j++)
            {
                // Durchlaufe alle Zeilen (alle Buchstaben von s1)
                for (int i = 1; i <= s1.Length; i++)
                {
                    // Prüfe: Sind die aktuellen Buchstaben gleich?
                    // s1[i-1] ist der i-te Buchstabe von s1 (wegen 0-Indexierung)
                    // Wenn gleich: cost = 0 (keine Änderung nötig)
                    // Wenn unterschiedlich: cost = 1 (Ersetzen nötig)
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    // Berechne die minimalen Kosten für diese Position:
                    // Option 1: d[i-1, j] + 1   → Von links kommen (ein Zeichen aus s1 löschen)
                    // Option 2: d[i, j-1] + 1   → Von oben kommen (ein Zeichen in s2 einfügen)
                    // Option 3: d[i-1, j-1] + cost → Diagonal kommen (Zeichen ersetzen oder beibehalten)
                    // Nimm die günstigste Option (Math.Min findet das Minimum)
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1,      // Löschen
                                 d[i, j - 1] + 1),     // Einfügen
                                 d[i - 1, j - 1] + cost); // Ersetzen oder gleich lassen
                }
            }

            // Die Zahl unten rechts in der Matrix ist die finale Levenshtein-Distanz
            // Das ist die minimale Anzahl an Änderungen, um von s1 zu s2 zu kommen
            return d[s1.Length, s2.Length];
        }
        ///<summary>
        ///Ersten Buchstaben Groß
        /// </summary>
        private static string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1);
        }
        #endregion

        #region Statistiken und Insights

        ///<summary>
        ///Gibt neue System Insights
        /// </summary>
    public static void ZeigeSystemInsights()
        {
            if(DataManager.Inventar.Count == 0 && DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintInfo("Noch keine Daten zum Analysieren vorhanden");
                return;
            }
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("     🤖 KI-Analyse des Systems:");
            Console.ResetColor();
            Console.WriteLine("");


            //Inventar
        /*    if(DataManager.Mitarbeiter.Count > 0)
            {
                var geraeteProMitarbeiter = DataManager.Inventar
                    .GroupBy(i => i.MitarbeiterBezeichnung)
                    .Select(g => new { Mitarbeiter = g.Key, Anzahl = g.Count() })
                    .OrderByDescending(x => x.Anzahl)
                    .First();
                Console.WriteLine($"    {geraeteProMitarbeiter.Mitarbeiter} hat die meisten Geräte ({geraeteProMitarbeiter.Anzahl})");
            }
           */
            //Mitarbeiter
            if(DataManager.Mitarbeiter.Count > 0)
            {
                var groessteAbteilung = DataManager.Mitarbeiter
                    .GroupBy(m => m.Abteilung)
                    .Select(g => new { Abteilung = g.Key, Anzahl = g.Count() })
                    .OrderByDescending (x => x.Anzahl)
                    .First();

                Console.WriteLine($"     Größte Abteilung: {groessteAbteilung.Abteilung} ({groessteAbteilung.Anzahl})");
        }
            double durchschnittGeraeteProMitarbeiter = DataManager.Mitarbeiter.Count > 0
                ? (double)DataManager.Inventar.Count / DataManager.Mitarbeiter.Count
                : 0;

            Console.WriteLine($"    Durchschnitt: {durchschnittGeraeteProMitarbeiter:F1} Geräte pro Mitarbeiter");

            Console.WriteLine("");
            #endregion
        }
}
    }
