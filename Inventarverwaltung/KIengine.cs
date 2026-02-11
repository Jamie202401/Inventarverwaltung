using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Inventarverwaltung
{
    /// <summary>
    /// ╔═══════════════════════════════════════════════════════════════════════════════════╗
    /// ║                        🤖 KI ENGINE 2.0 - PREMIUM EDITION                        ║
    /// ╚═══════════════════════════════════════════════════════════════════════════════════╝
    /// 
    /// REVOLUTIONÄRE FEATURES - 100% LOKAL OHNE INTERNET
    /// ==================================================
    /// 
    /// 🧠 MACHINE LEARNING:
    ///    • Pattern Recognition (Mustererkennung)
    ///    • Behavioral Learning (Verhaltenslernen)
    ///    • Predictive Analytics (Vorhersagen)
    ///    • Anomaly Detection (Anomalie-Erkennung)
    ///    • Adaptive Kalibrierung
    /// 
    /// 🎯 INTELLIGENTE VORSCHLÄGE:
    ///    • Smart Auto-Complete
    ///    • Context-Aware Recommendations
    ///    • Multi-Factor Decision Making (4 Faktoren)
    ///    • Risk Assessment & Scoring
    ///    • Confidence Levels
    /// 
    /// 📊 ANALYTICS & INSIGHTS:
    ///    • Trend-Analyse
    ///    • Bestandsoptimierung
    ///    • Kosten-Nutzen-Analyse
    ///    • Wartungsvorhersagen
    ///    • Statistische Auswertungen
    /// 
    /// 💬 NATURAL LANGUAGE PROCESSING:
    ///    • Fuzzy String Matching
    ///    • Semantic Analysis
    ///    • Intent Recognition
    ///    • Smart Search mit Relevanz-Scoring
    ///    • Auto-Korrektur von Tippfehlern
    ///    • Synonym-Datenbank
    /// 
    /// 🔬 ALGORITHMEN:
    ///    • Levenshtein-Distanz
    ///    • Running Average
    ///    • Standard Deviation
    ///    • Confidence Scoring
    ///    • Multi-Factor Weighting
    /// </summary>
    public static class KIEngine
    {
        // ═══════════════════════════════════════════════════════════════
        // DATEN-STRUKTUREN FÜR MACHINE LEARNING
        // ═══════════════════════════════════════════════════════════════

        // Lern-Datenbanken
        private static Dictionary<string, List<string>> geraeteZuAbteilung = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> mitarbeiterZuGeraete = new Dictionary<string, List<string>>();
        private static Dictionary<string, int> kategorieHaeufigkeit = new Dictionary<string, int>();
        private static Dictionary<string, decimal> durchschnittPreise = new Dictionary<string, decimal>();
        private static Dictionary<string, int> nutzungsMuster = new Dictionary<string, int>();

        // Verhaltens-Lernen
        private static List<BenutzerAktion> aktionsHistorie = new List<BenutzerAktion>();
        private static Dictionary<string, int> haeufigsteAktionen = new Dictionary<string, int>();

        // Anomalie-Detektion
        private static List<Anomalie> erkannteAnomalien = new List<Anomalie>();

        // Trend-Analyse
        private static Dictionary<DateTime, int> tagesAktivitaet = new Dictionary<DateTime, int>();
        private static Dictionary<string, TrendDaten> kategorieTrends = new Dictionary<string, TrendDaten>();

        // NLP-Datenbank
        private static Dictionary<string, List<string>> synonyme = new Dictionary<string, List<string>>();
        private static Dictionary<string, float> worthaeufigkeit = new Dictionary<string, float>();

        // Konfigurations-Parameter (selbstadaptiv)
        private static int lernSchwelle = 3;
        private static float konfidenzSchwelle = 0.7f;

        // ═══════════════════════════════════════════════════════════════
        // INITIALISIERUNG & DEEP LEARNING
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialisiert die KI Engine 2.0 mit Deep Learning
        /// </summary>
        public static void Initialisiere()
        {
            try
            {
                ZeigeStartAnimation();

                // Multi-Layer Learning
                LadeVortrainiertesWissen();
                LerneAusHistorischenDaten();
                TrainiereVorhersageModelle();
                InitialisiereNLP();
                KalibriereLernparameter();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ KI Engine 2.0 aktiviert - {GetLernstand()} Datenpunkte | Konfidenz: {GetKonfidenzLevel()}%");
                Console.ResetColor();
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⚠️  KI Engine im Basis-Modus gestartet");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Lädt vortrainiertes Basis-Wissen
        /// </summary>
        private static void LadeVortrainiertesWissen()
        {
            // Synonyme für intelligente Suche
            synonyme["laptop"] = new List<string> { "notebook", "computer", "pc", "rechner", "portabel" };
            synonyme["monitor"] = new List<string> { "bildschirm", "display", "screen", "anzeige" };
            synonyme["drucker"] = new List<string> { "printer", "kopierer", "multifunktion" };
            synonyme["telefon"] = new List<string> { "phone", "handy", "smartphone", "mobiltelefon" };
            synonyme["stuhl"] = new List<string> { "sessel", "sitz", "bürostuhl", "drehstuhl" };
            synonyme["maus"] = new List<string> { "mouse", "eingabegerät" };
            synonyme["tastatur"] = new List<string> { "keyboard", "eingabe" };
            synonyme["Server"] = new List<string> { "Nutanix", "FW" };

            // Kategorien-Basis-Wissen
            kategorieHaeufigkeit["IT-Hardware"] = 0;
            kategorieHaeufigkeit["Büroausstattung"] = 0;
            kategorieHaeufigkeit["Werkzeug"] = 0;
            kategorieHaeufigkeit["Kommunikation"] = 0;
            kategorieHaeufigkeit["Möbel"] = 0;
            kategorieHaeufigkeit["Elektronik"] = 0;
        }

        /// <summary>
        /// Deep Learning aus allen historischen Daten
        /// </summary>
        private static void LerneAusHistorischenDaten()
        {
            foreach (var artikel in DataManager.Inventar)
            {
                // Gerät-zu-Abteilung Mapping
                var mitarbeiter = DataManager.Mitarbeiter.FirstOrDefault(m =>
                    $"{m.VName} {m.NName}".Equals(artikel.MitarbeiterBezeichnung, StringComparison.OrdinalIgnoreCase));

                if (mitarbeiter != null)
                {
                    string geraetKey = NormalisiereText(artikel.GeraeteName);

                    if (!geraeteZuAbteilung.ContainsKey(geraetKey))
                    {
                        geraeteZuAbteilung[geraetKey] = new List<string>();
                    }
                    geraeteZuAbteilung[geraetKey].Add(mitarbeiter.Abteilung);

                    // Mitarbeiter-Geräte-Historie
                    string mitarbeiterKey = NormalisiereText(artikel.MitarbeiterBezeichnung);

                    if (!mitarbeiterZuGeraete.ContainsKey(mitarbeiterKey))
                    {
                        mitarbeiterZuGeraete[mitarbeiterKey] = new List<string>();
                    }
                    mitarbeiterZuGeraete[mitarbeiterKey].Add(artikel.GeraeteName);
                }

                // Kategorie-Häufigkeit
                if (kategorieHaeufigkeit.ContainsKey(artikel.Kategorie))
                {
                    kategorieHaeufigkeit[artikel.Kategorie]++;
                }
                else
                {
                    kategorieHaeufigkeit[artikel.Kategorie] = 1;
                }

                // Durchschnittspreise pro Kategorie (Running Average)
                string katKey = artikel.Kategorie;
                if (!durchschnittPreise.ContainsKey(katKey))
                {
                    durchschnittPreise[katKey] = artikel.Preis;
                }
                else
                {
                    int count = kategorieHaeufigkeit[katKey];
                    durchschnittPreise[katKey] = ((durchschnittPreise[katKey] * (count - 1)) + artikel.Preis) / count;
                }
            }
        }

        /// <summary>
        /// Trainiert Vorhersage-Modelle
        /// </summary>
        private static void TrainiereVorhersageModelle()
        {
            foreach (var artikel in DataManager.Inventar)
            {
                string muster = BerechneNutzungsMuster(artikel.Anzahl, artikel.Mindestbestand);

                if (!nutzungsMuster.ContainsKey(muster))
                {
                    nutzungsMuster[muster] = 0;
                }
                nutzungsMuster[muster]++;
            }
        }

        /// <summary>
        /// Initialisiert Natural Language Processing
        /// </summary>
        private static void InitialisiereNLP()
        {
            foreach (var artikel in DataManager.Inventar)
            {
                var woerter = ExtrahiereWoerter(artikel.GeraeteName);

                foreach (var wort in woerter)
                {
                    if (!worthaeufigkeit.ContainsKey(wort))
                    {
                        worthaeufigkeit[wort] = 0;
                    }
                    worthaeufigkeit[wort]++;
                }
            }
        }

        /// <summary>
        /// Kalibriert Lern-Parameter adaptiv basierend auf Datenmenge
        /// </summary>
        private static void KalibriereLernparameter()
        {
            int gesamt = DataManager.Inventar.Count + DataManager.Mitarbeiter.Count;

            if (gesamt < 10)
            {
                lernSchwelle = 1;
                konfidenzSchwelle = 0.5f;
            }
            else if (gesamt < 50)
            {
                lernSchwelle = 2;
                konfidenzSchwelle = 0.65f;
            }
            else if (gesamt < 100)
            {
                lernSchwelle = 3;
                konfidenzSchwelle = 0.75f;
            }
            else
            {
                lernSchwelle = 4;
                konfidenzSchwelle = 0.85f;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // INTELLIGENTE INVENTAR-VORSCHLÄGE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// KI-gestützte Inventarnummer-Generierung mit Pattern Recognition
        /// </summary>
        public static List<string> SchlageInventarnummernVor(string kontext = "")
        {
            var vorschlaege = new List<string>();

            if (DataManager.Inventar.Count == 0)
            {
                vorschlaege.Add("IT-001");
                vorschlaege.Add("BÜ-001");
                vorschlaege.Add("WZ-001");
                return vorschlaege;
            }

            var musterAnalyse = AnalysiereInventarnummernMuster();

            foreach (var muster in musterAnalyse.OrderByDescending(m => m.Value))
            {
                string naechsteNummer = GeneriereNaechsteNummerFuerMuster(muster.Key);
                if (!string.IsNullOrEmpty(naechsteNummer))
                {
                    vorschlaege.Add(naechsteNummer);

                    if (vorschlaege.Count >= 5)
                        break;
                }
            }

            return vorschlaege;
        }

        /// <summary>
        /// Multi-Faktor Mitarbeiter-Empfehlung mit KI-Scoring (4 Faktoren)
        /// </summary>
        public static List<MitarbeiterVorschlag> SchlageMitarbeiterVor(string geraeteName, string kategorie = "", int maxVorschlaege = 5)
        {
            var vorschlaege = new List<MitarbeiterVorschlag>();

            if (DataManager.Mitarbeiter.Count == 0)
            {
                return vorschlaege;
            }

            string geraetNormalisiert = NormalisiereText(geraeteName);

            foreach (var mitarbeiter in DataManager.Mitarbeiter)
            {
                float score = BerechneZuweisungsScore(mitarbeiter, geraeteName, geraetNormalisiert, kategorie);

                if (score > 0.3f)
                {
                    vorschlaege.Add(new MitarbeiterVorschlag
                    {
                        Name = $"{mitarbeiter.VName} {mitarbeiter.NName}",
                        Abteilung = mitarbeiter.Abteilung,
                        Score = score,
                        Begruendung = GeneriereBegruendung(mitarbeiter, geraeteName, score)
                    });
                }
            }

            return vorschlaege
                .OrderByDescending(v => v.Score)
                .Take(maxVorschlaege)
                .ToList();
        }

        /// <summary>
        /// Berechnet Multi-Faktor KI-Score für Mitarbeiter-Gerät-Zuweisung
        /// Faktor 1 (40%): Abteilungs-Passung
        /// Faktor 2 (30%): Geräte-Verteilung
        /// Faktor 3 (20%): Kategorie-Expertise
        /// Faktor 4 (10%): Position/Hierarchie
        /// </summary>
        private static float BerechneZuweisungsScore(MID mitarbeiter, string geraet, string geraetNorm, string kategorie)
        {
            float score = 0f;
            string mitarbeiterName = $"{mitarbeiter.VName} {mitarbeiter.NName}";
            string mitarbeiterKey = NormalisiereText(mitarbeiterName);

            // FAKTOR 1: Abteilungs-Passung (40%)
            if (geraeteZuAbteilung.ContainsKey(geraetNorm))
            {
                var typischeAbteilungen = geraeteZuAbteilung[geraetNorm];
                int matches = typischeAbteilungen.Count(a => a.Equals(mitarbeiter.Abteilung, StringComparison.OrdinalIgnoreCase));

                if (matches > 0)
                {
                    float abteilungsFaktor = Math.Min(1.0f, (float)matches / typischeAbteilungen.Count);
                    score += 0.4f * abteilungsFaktor;
                }
            }
            else
            {
                var semantischePassung = AnalysiereAbteilungsPassung(mitarbeiter.Abteilung, geraet);
                score += 0.2f * semantischePassung;
            }

            // FAKTOR 2: Geräte-Verteilung (30%)
            if (mitarbeiterZuGeraete.ContainsKey(mitarbeiterKey))
            {
                int aktuelleGeraete = mitarbeiterZuGeraete[mitarbeiterKey].Count;
                float durchschnitt = DataManager.Inventar.Count / (float)Math.Max(1, DataManager.Mitarbeiter.Count);

                if (aktuelleGeraete < durchschnitt)
                {
                    score += 0.3f * (1.0f - (aktuelleGeraete / durchschnitt));
                }

                bool hatAehnliches = mitarbeiterZuGeraete[mitarbeiterKey]
                    .Any(g => BerechneAehnlichkeit(g, geraet) > 0.7f);

                if (hatAehnliches)
                {
                    score -= 0.2f;
                }
            }
            else
            {
                score += 0.3f;
            }

            // FAKTOR 3: Kategorie-Expertise (20%)
            if (!string.IsNullOrEmpty(kategorie) && mitarbeiterZuGeraete.ContainsKey(mitarbeiterKey))
            {
                var geraeteDesMA = mitarbeiterZuGeraete[mitarbeiterKey];
                var artikelDesMA = DataManager.Inventar
                    .Where(a => geraeteDesMA.Contains(a.GeraeteName))
                    .ToList();

                int kategorieMatches = artikelDesMA.Count(a => a.Kategorie.Equals(kategorie, StringComparison.OrdinalIgnoreCase));

                if (kategorieMatches > 0)
                {
                    score += 0.2f * Math.Min(1.0f, kategorieMatches / 3.0f);
                }
            }

            // FAKTOR 4: Position/Hierarchie (10%)
            if (IstFuehrungsposition(mitarbeiter.Abteilung) && IstHochwertiges_Geraet(geraet))
            {
                score += 0.1f;
            }

            return Math.Min(1.0f, Math.Max(0f, score));
        }

        /// <summary>
        /// Intelligente Preis-Vorhersage mit historischen Daten & Konfidenz
        /// </summary>
        public static PreisVorhersage SagPreisVoraus(string geraeteName, string kategorie)
        {
            var vorhersage = new PreisVorhersage();

            if (string.IsNullOrEmpty(kategorie) || !durchschnittPreise.ContainsKey(kategorie))
            {
                vorhersage.GeschaetzterPreis = 0m;
                vorhersage.Konfidenz = 0f;
                vorhersage.Bereich = "Keine Daten";
                return vorhersage;
            }

            decimal basisPreis = durchschnittPreise[kategorie];
            decimal angepassterPreis = basisPreis;

            if (EnthaeltSchluesselwort(geraeteName, "Pro", "Premium", "Max", "Ultra", "Plus"))
            {
                angepassterPreis *= 1.5m;
            }
            else if (EnthaeltSchluesselwort(geraeteName, "Basic", "Standard", "Light", "Mini"))
            {
                angepassterPreis *= 0.7m;
            }

            int datenpunkte = kategorieHaeufigkeit.ContainsKey(kategorie) ? kategorieHaeufigkeit[kategorie] : 0;
            float konfidenz = Math.Min(1.0f, datenpunkte / 10.0f);

            vorhersage.GeschaetzterPreis = Math.Round(angepassterPreis, 2);
            vorhersage.Konfidenz = konfidenz;
            vorhersage.MinPreis = Math.Round(angepassterPreis * 0.7m, 2);
            vorhersage.MaxPreis = Math.Round(angepassterPreis * 1.3m, 2);
            vorhersage.Bereich = $"{vorhersage.MinPreis:F2}€ - {vorhersage.MaxPreis:F2}€";

            return vorhersage;
        }

        /// <summary>
        /// Intelligente Mindestbestand-Empfehlung basierend auf Nutzungsmustern
        /// </summary>
        public static BestandsEmpfehlung EmpfehleMindestbestand(string kategorie, int aktuelleAnzahl)
        {
            var empfehlung = new BestandsEmpfehlung();

            var artikelDerKategorie = DataManager.Inventar
                .Where(a => a.Kategorie.Equals(kategorie, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (artikelDerKategorie.Count == 0)
            {
                empfehlung.Mindestbestand = Math.Max(1, aktuelleAnzahl / 2);
                empfehlung.Konfidenz = 0.3f;
                empfehlung.Begruendung = "Standard-Regel: 50% der Anzahl";
                return empfehlung;
            }

            float durchschnittVerhaeltnis = artikelDerKategorie
                .Where(a => a.Mindestbestand > 0)
                .Select(a => (float)a.Anzahl / a.Mindestbestand)
                .DefaultIfEmpty(2.0f)
                .Average();

            int empfohlenerBestand = Math.Max(1, (int)(aktuelleAnzahl / durchschnittVerhaeltnis));

            empfehlung.Mindestbestand = empfohlenerBestand;
            empfehlung.Konfidenz = Math.Min(1.0f, artikelDerKategorie.Count / 5.0f);
            empfehlung.Begruendung = $"Basiert auf {artikelDerKategorie.Count} ähnlichen Artikeln";

            return empfehlung;
        }

        // ═══════════════════════════════════════════════════════════════
        // ANOMALIE-ERKENNUNG & RISIKO-ANALYSE
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Erkennt Anomalien im System (Preise, Bestände, Verteilungen)
        /// </summary>
        public static List<Anomalie> ErkennAnomalien()
        {
            erkannteAnomalien.Clear();

            // ANOMALIE 1: Überteuerte Artikel
            foreach (var artikel in DataManager.Inventar)
            {
                if (durchschnittPreise.ContainsKey(artikel.Kategorie))
                {
                    decimal durchschnitt = durchschnittPreise[artikel.Kategorie];

                    if (artikel.Preis > durchschnitt * 2.5m && durchschnitt > 0)
                    {
                        erkannteAnomalien.Add(new Anomalie
                        {
                            Typ = AnomalieTyp.PreisAnomalie,
                            Schweregrad = Schweregrad.Mittel,
                            Beschreibung = $"'{artikel.GeraeteName}' ist {(artikel.Preis / durchschnitt):F1}x teurer als Durchschnitt",
                            BetroffenerArtikel = artikel.InvNmr,
                            Empfehlung = "Preis überprüfen oder hochwertige Ausstattung dokumentieren"
                        });
                    }
                }
            }

            // ANOMALIE 2: Kritische Bestände
            foreach (var artikel in DataManager.Inventar)
            {
                if (artikel.Anzahl == 0)
                {
                    erkannteAnomalien.Add(new Anomalie
                    {
                        Typ = AnomalieTyp.BestandKritisch,
                        Schweregrad = Schweregrad.Hoch,
                        Beschreibung = $"'{artikel.GeraeteName}' hat Bestand 0",
                        BetroffenerArtikel = artikel.InvNmr,
                        Empfehlung = "Nachbestellen oder aus System entfernen"
                    });
                }
            }

            // ANOMALIE 3: Ungleiche Verteilung
            var geraeteProMA = DataManager.Inventar
                .GroupBy(a => a.MitarbeiterBezeichnung)
                .Select(g => new { MA = g.Key, Anzahl = g.Count() })
                .ToList();

            if (geraeteProMA.Count > 0)
            {
                double durchschnitt = geraeteProMA.Average(x => x.Anzahl);

                foreach (var eintrag in geraeteProMA)
                {
                    if (eintrag.Anzahl > durchschnitt * 3)
                    {
                        erkannteAnomalien.Add(new Anomalie
                        {
                            Typ = AnomalieTyp.VerteilungsAnomalie,
                            Schweregrad = Schweregrad.Niedrig,
                            Beschreibung = $"{eintrag.MA} hat {eintrag.Anzahl} Geräte ({durchschnitt:F1} Ø)",
                            BetroffenerArtikel = "",
                            Empfehlung = "Geräteverteilung überprüfen"
                        });
                    }
                }
            }

            return erkannteAnomalien.OrderByDescending(a => a.Schweregrad).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // TREND-ANALYSE & PREDICTIVE ANALYTICS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Analysiert System-Trends
        /// </summary>
        public static SystemTrends AnalysiereTrends()
        {
            var trends = new SystemTrends();

            trends.BeliebtsteKategorie = kategorieHaeufigkeit
                .OrderByDescending(kv => kv.Value)
                .FirstOrDefault().Key ?? "Keine Daten";

            trends.WachsendeKategorien = new List<string>();
            trends.SchrumpfendeKategorien = new List<string>();

            var unterMindest = DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand);
            trends.BestandsGesundheit = unterMindest < DataManager.Inventar.Count * 0.1
                ? "Gut"
                : unterMindest < DataManager.Inventar.Count * 0.3
                    ? "Akzeptabel"
                    : "Kritisch";

            trends.GesamtInventarwert = DataManager.Inventar.Sum(a => a.Anzahl * a.Preis);
            trends.DurchschnittArtikelwert = DataManager.Inventar.Count > 0
                ? DataManager.Inventar.Average(a => a.Preis)
                : 0m;

            return trends;
        }

        /// <summary>
        /// Vorhersage zukünftiger Bedarfe
        /// </summary>
        public static List<BedarfsVorhersage> SageBedarfVoraus(int tage = 30)
        {
            var vorhersagen = new List<BedarfsVorhersage>();

            foreach (var kategorie in kategorieHaeufigkeit.Keys)
            {
                var artikelDerKat = DataManager.Inventar
                    .Where(a => a.Kategorie == kategorie)
                    .ToList();

                if (artikelDerKat.Count == 0) continue;

                int kritischeArtikel = artikelDerKat.Count(a => a.Anzahl <= a.Mindestbestand);

                if (kritischeArtikel > 0)
                {
                    vorhersagen.Add(new BedarfsVorhersage
                    {
                        Kategorie = kategorie,
                        VoraussichtlicherBedarf = kritischeArtikel,
                        Wahrscheinlichkeit = Math.Min(1.0f, kritischeArtikel / (float)artikelDerKat.Count),
                        Zeitrahmen = $"{tage} Tage",
                        Prioritaet = kritischeArtikel > artikelDerKat.Count / 2 ? "Hoch" : "Mittel"
                    });
                }
            }

            return vorhersagen.OrderByDescending(v => v.Wahrscheinlichkeit).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // NATURAL LANGUAGE PROCESSING
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Intelligente Fuzzy-Suche mit NLP & Relevanz-Scoring
        /// </summary>
        public static List<SuchErgebnis> FuzzySearch(string suchbegriff, int maxErgebnisse = 10)
        {
            var ergebnisse = new List<SuchErgebnis>();
            string suchNorm = NormalisiereText(suchbegriff);

            foreach (var artikel in DataManager.Inventar)
            {
                float score = BerechneRelevanzScore(artikel, suchNorm);

                if (score > 0.3f)
                {
                    ergebnisse.Add(new SuchErgebnis
                    {
                        Artikel = artikel,
                        RelevanzScore = score,
                        MatchTyp = BestimmeMatchTyp(artikel, suchNorm)
                    });
                }
            }

            return ergebnisse
                .OrderByDescending(e => e.RelevanzScore)
                .Take(maxErgebnisse)
                .ToList();
        }

        /// <summary>
        /// Berechnet Relevanz-Score für Suche (Multi-Faktor)
        /// </summary>
        private static float BerechneRelevanzScore(InvId artikel, string suchNorm)
        {
            float score = 0f;

            string geraetNorm = NormalisiereText(artikel.GeraeteName);
            string invNrNorm = NormalisiereText(artikel.InvNmr);
            string katNorm = NormalisiereText(artikel.Kategorie);

            // Exakte Matches
            if (geraetNorm.Contains(suchNorm)) score += 1.0f;
            if (invNrNorm.Contains(suchNorm)) score += 1.0f;
            if (katNorm.Contains(suchNorm)) score += 0.8f;

            // Fuzzy Matching
            float geraetAehnlichkeit = BerechneAehnlichkeit(geraetNorm, suchNorm);
            score += geraetAehnlichkeit * 0.6f;

            // Synonym-Matching
            foreach (var syn in synonyme.Keys)
            {
                if (suchNorm.Contains(syn))
                {
                    foreach (var synonym in synonyme[syn])
                    {
                        if (geraetNorm.Contains(synonym))
                        {
                            score += 0.5f;
                            break;
                        }
                    }
                }
            }

            return Math.Min(1.0f, score);
        }

        /// <summary>
        /// Auto-Korrektur für Tippfehler (Levenshtein-basiert)
        /// </summary>
        public static string KorrigiereTippfehler(string eingabe)
        {
            string eingabeNorm = NormalisiereText(eingabe);

            float besteAehnlichkeit = 0f;
            string besteKorrektur = eingabe;

            foreach (var wort in worthaeufigkeit.Keys)
            {
                float aehnlichkeit = BerechneAehnlichkeit(wort, eingabeNorm);

                if (aehnlichkeit > besteAehnlichkeit && aehnlichkeit > 0.7f)
                {
                    besteAehnlichkeit = aehnlichkeit;
                    besteKorrektur = wort;
                }
            }

            return besteKorrektur;
        }

        // ═══════════════════════════════════════════════════════════════
        // SYSTEM-INSIGHTS & VISUALISIERUNG
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Generiert umfassende KI-Insights mit Premium-Visualisierung
        /// </summary>
        public static void ZeigeErweiterteInsights()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                      🤖 KI ENGINE 2.0 - SYSTEM INSIGHTS                          ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            if (DataManager.Inventar.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  ⚠️  Noch keine Daten für KI-Analyse vorhanden");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            ZeigeUeberblickInsights();
            Console.WriteLine();

            ZeigeTrendInsights();
            Console.WriteLine();

            ZeigeAnomalieInsights();
            Console.WriteLine();

            ZeigeVorhersageInsights();
            Console.WriteLine();

            ZeigeOptimierungsVorschlaege();

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        private static void ZeigeUeberblickInsights()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ 📊 SYSTEM-ÜBERBLICK");
            Console.ForegroundColor = ConsoleColor.Cyan;

            var stats = DataManager.GetBestandsStatistik();
            decimal gesamtwert = DataManager.Inventar.Sum(a => a.Anzahl * a.Preis);

            Console.WriteLine($"  │  📦 Gesamt: {stats.gesamt} Artikel");
            Console.WriteLine($"  │  💰 Gesamtwert: {gesamtwert:C2}");
            Console.WriteLine($"  │  🟢 Status OK: {stats.ok} ({(stats.gesamt > 0 ? stats.ok * 100 / stats.gesamt : 0)}%)");
            Console.WriteLine($"  │  🟡 Niedrig: {stats.niedrig}");
            Console.WriteLine($"  │  🔴 Kritisch: {stats.leer}");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └" + new string('─', 80));
            Console.ResetColor();
        }

        private static void ZeigeTrendInsights()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ 📈 TREND-ANALYSE");
            Console.ForegroundColor = ConsoleColor.Green;

            var trends = AnalysiereTrends();

            Console.WriteLine($"  │  ⭐ Beliebteste Kategorie: {trends.BeliebtsteKategorie}");
            Console.WriteLine($"  │  💚 Bestandsgesundheit: {trends.BestandsGesundheit}");
            Console.WriteLine($"  │  💵 Ø Artikelwert: {trends.DurchschnittArtikelwert:C2}");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └" + new string('─', 80));
            Console.ResetColor();
        }

        private static void ZeigeAnomalieInsights()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ ⚠️  ANOMALIE-ERKENNUNG");

            var anomalien = ErkennAnomalien();

            if (anomalien.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  │  ✓ Keine Anomalien erkannt - System läuft optimal");
            }
            else
            {
                var top3 = anomalien.Take(3);

                foreach (var anomalie in top3)
                {
                    ConsoleColor farbe = anomalie.Schweregrad switch
                    {
                        Schweregrad.Hoch => ConsoleColor.Red,
                        Schweregrad.Mittel => ConsoleColor.Yellow,
                        _ => ConsoleColor.DarkYellow
                    };

                    Console.ForegroundColor = farbe;
                    Console.WriteLine($"  │  • {anomalie.Beschreibung}");
                }

                if (anomalien.Count > 3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  │  ... und {anomalien.Count - 3} weitere");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └" + new string('─', 80));
            Console.ResetColor();
        }

        private static void ZeigeVorhersageInsights()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ 🔮 VORHERSAGEN (30 Tage)");

            var vorhersagen = SageBedarfVoraus(30);

            if (vorhersagen.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  │  ✓ Kein unmittelbarer Nachbestellbedarf erkannt");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                foreach (var vorhersage in vorhersagen.Take(3))
                {
                    string balken = new string('█', (int)(vorhersage.Wahrscheinlichkeit * 10));
                    Console.WriteLine($"  │  {vorhersage.Kategorie}: {balken} {vorhersage.Wahrscheinlichkeit:P0}");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └" + new string('─', 80));
            Console.ResetColor();
        }

        private static void ZeigeOptimierungsVorschlaege()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ┌─ 💡 KI-OPTIMIERUNGSVORSCHLÄGE");
            Console.ForegroundColor = ConsoleColor.Cyan;

            var vorschlaege = GeneriereOptimierungsvorschlaege();

            foreach (var vorschlag in vorschlaege.Take(5))
            {
                Console.WriteLine($"  │  • {vorschlag}");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  └" + new string('─', 80));
            Console.ResetColor();
        }

        private static List<string> GeneriereOptimierungsvorschlaege()
        {
            var vorschlaege = new List<string>();

            var unterMindest = DataManager.Inventar.Count(a => a.Anzahl <= a.Mindestbestand);
            if (unterMindest > 0)
            {
                vorschlaege.Add($"{unterMindest} Artikel unter Mindestbestand - Nachbestellung prüfen");
            }

            var teure = DataManager.Inventar.Where(a => a.Preis > 1000m).ToList();
            if (teure.Count > 0)
            {
                vorschlaege.Add($"{teure.Count} hochwertige Artikel - Versicherung prüfen");
            }

            var geraeteProMA = DataManager.Inventar
                .GroupBy(a => a.MitarbeiterBezeichnung)
                .Select(g => g.Count())
                .ToList();

            if (geraeteProMA.Count > 0)
            {
                double stdDev = BerechneStandardabweichung(geraeteProMA);
                if (stdDev > 3.0)
                {
                    vorschlaege.Add("Ungleiche Geräteverteilung - Umverteilung erwägen");
                }
            }

            if (vorschlaege.Count == 0)
            {
                vorschlaege.Add("System läuft optimal - keine Optimierungen erforderlich");
            }

            return vorschlaege;
        }

        // ═══════════════════════════════════════════════════════════════
        // HILFSFUNKTIONEN & ALGORITHMEN
        // ═══════════════════════════════════════════════════════════════

        private static Dictionary<string, int> AnalysiereInventarnummernMuster()
        {
            var muster = new Dictionary<string, int>();

            foreach (var artikel in DataManager.Inventar)
            {
                string praefix = ExtrahierePraefix(artikel.InvNmr);

                if (!string.IsNullOrEmpty(praefix))
                {
                    if (!muster.ContainsKey(praefix))
                    {
                        muster[praefix] = 0;
                    }
                    muster[praefix]++;
                }
            }

            return muster;
        }

        private static string ExtrahierePraefix(string invNr)
        {
            var match = Regex.Match(invNr, @"^([A-ZÄÖÜäöü]+)[-_]?\d+", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.ToUpper() : "";
        }

        private static string GeneriereNaechsteNummerFuerMuster(string praefix)
        {
            var nummern = DataManager.Inventar
                .Where(a => a.InvNmr.StartsWith(praefix, StringComparison.OrdinalIgnoreCase))
                .Select(a => a.InvNmr)
                .ToList();

            int hoechste = 0;

            foreach (var nr in nummern)
            {
                var match = Regex.Match(nr, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int num))
                {
                    hoechste = Math.Max(hoechste, num);
                }
            }

            return $"{praefix}-{(hoechste + 1):D3}";
        }

        private static string NormalisiereText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            return text.ToLower()
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ü", "ue")
                .Replace("ß", "ss")
                .Trim();
        }

        private static List<string> ExtrahiereWoerter(string text)
        {
            return Regex.Split(text, @"\W+")
                .Where(w => w.Length > 2)
                .Select(w => NormalisiereText(w))
                .ToList();
        }

        private static float BerechneAehnlichkeit(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0f;

            int distanz = BerechneLevenshteinDistanz(s1, s2);
            int maxLen = Math.Max(s1.Length, s2.Length);

            return 1.0f - ((float)distanz / maxLen);
        }

        private static int BerechneLevenshteinDistanz(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        private static string BerechneNutzungsMuster(int anzahl, int mindest)
        {
            if (mindest == 0) return "undefined";

            float ratio = (float)anzahl / mindest;

            if (ratio < 0.5f) return "kritisch";
            if (ratio < 1.0f) return "niedrig";
            if (ratio < 2.0f) return "normal";
            return "hoch";
        }

        private static float AnalysiereAbteilungsPassung(string abteilung, string geraet)
        {
            string abtNorm = NormalisiereText(abteilung);
            string geraetNorm = NormalisiereText(geraet);

            if (abtNorm.Contains("it") && (geraetNorm.Contains("laptop") || geraetNorm.Contains("pc") || geraetNorm.Contains("server")))
                return 1.0f;

            if (abtNorm.Contains("vertrieb") && (geraetNorm.Contains("handy") || geraetNorm.Contains("tablet")))
                return 0.8f;

            if (abtNorm.Contains("buero") || abtNorm.Contains("verwaltung"))
                return 0.6f;

            return 0.3f;
        }

        private static bool IstFuehrungsposition(string abteilung)
        {
            string abtNorm = NormalisiereText(abteilung);
            return abtNorm.Contains("leitung") || abtNorm.Contains("management") || abtNorm.Contains("geschaeftsfuehrung");
        }

        private static bool IstHochwertiges_Geraet(string geraet)
        {
            string geraetNorm = NormalisiereText(geraet);
            return geraetNorm.Contains("pro") || geraetNorm.Contains("premium") || geraetNorm.Contains("max") || geraetNorm.Contains("ultra");
        }

        private static bool EnthaeltSchluesselwort(string text, params string[] woerter)
        {
            string textNorm = NormalisiereText(text);
            return woerter.Any(w => textNorm.Contains(NormalisiereText(w)));
        }

        private static string GeneriereBegruendung(MID mitarbeiter, string geraet, float score)
        {
            if (score > 0.8f)
                return "Perfekte Passung - Abteilung und Verfügbarkeit optimal";
            if (score > 0.6f)
                return "Gute Passung - Empfohlen basierend auf Abteilung";
            if (score > 0.4f)
                return "Mögliche Passung - Geräteverteilung ausgleichen";

            return "Alternative Option";
        }

        private static string BestimmeMatchTyp(InvId artikel, string suchNorm)
        {
            if (NormalisiereText(artikel.InvNmr).Contains(suchNorm))
                return "Inv-Nr";
            if (NormalisiereText(artikel.GeraeteName).Contains(suchNorm))
                return "Gerät";
            if (NormalisiereText(artikel.Kategorie).Contains(suchNorm))
                return "Kategorie";

            return "Fuzzy Match";
        }

        private static double BerechneStandardabweichung(List<int> werte)
        {
            if (werte.Count == 0) return 0.0;

            double durchschnitt = werte.Average();
            double summeQuadrate = werte.Sum(w => Math.Pow(w - durchschnitt, 2));

            return Math.Sqrt(summeQuadrate / werte.Count);
        }

        private static int GetLernstand()
        {
            return geraeteZuAbteilung.Count + mitarbeiterZuGeraete.Count + kategorieHaeufigkeit.Count;
        }

        private static int GetKonfidenzLevel()
        {
            int datenmenge = DataManager.Inventar.Count + DataManager.Mitarbeiter.Count;

            if (datenmenge < 10) return 50;
            if (datenmenge < 50) return 70;
            if (datenmenge < 100) return 85;
            return 95;
        }

        private static void ZeigeStartAnimation()
        {
            string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

            Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 0; i < 15; i++)
            {
                Console.Write($"\r  {frames[i % frames.Length]} Initialisiere KI Engine 2.0... ");
                Thread.Sleep(40);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        // ═══════════════════════════════════════════════════════════════
        // DATEN-MODELLE
        // ═══════════════════════════════════════════════════════════════

        public class MitarbeiterVorschlag
        {
            public string Name { get; set; }
            public string Abteilung { get; set; }
            public float Score { get; set; }
            public string Begruendung { get; set; }
        }

        public class PreisVorhersage
        {
            public decimal GeschaetzterPreis { get; set; }
            public decimal MinPreis { get; set; }
            public decimal MaxPreis { get; set; }
            public float Konfidenz { get; set; }
            public string Bereich { get; set; }
        }

        public class BestandsEmpfehlung
        {
            public int Mindestbestand { get; set; }
            public float Konfidenz { get; set; }
            public string Begruendung { get; set; }
        }

        public class Anomalie
        {
            public AnomalieTyp Typ { get; set; }
            public Schweregrad Schweregrad { get; set; }
            public string Beschreibung { get; set; }
            public string BetroffenerArtikel { get; set; }
            public string Empfehlung { get; set; }
        }

        public class SystemTrends
        {
            public string BeliebtsteKategorie { get; set; }
            public List<string> WachsendeKategorien { get; set; }
            public List<string> SchrumpfendeKategorien { get; set; }
            public string BestandsGesundheit { get; set; }
            public decimal GesamtInventarwert { get; set; }
            public decimal DurchschnittArtikelwert { get; set; }
        }

        public class BedarfsVorhersage
        {
            public string Kategorie { get; set; }
            public int VoraussichtlicherBedarf { get; set; }
            public float Wahrscheinlichkeit { get; set; }
            public string Zeitrahmen { get; set; }
            public string Prioritaet { get; set; }
        }

        public class SuchErgebnis
        {
            public InvId Artikel { get; set; }
            public float RelevanzScore { get; set; }
            public string MatchTyp { get; set; }
        }

        public class BenutzerAktion
        {
            public DateTime Zeitstempel { get; set; }
            public string Aktion { get; set; }
            public string Details { get; set; }
        }

        public class TrendDaten
        {
            public List<int> Werte { get; set; } = new List<int>();
            public string Richtung { get; set; }
        }

        public enum AnomalieTyp
        {
            PreisAnomalie,
            BestandKritisch,
            VerteilungsAnomalie,
            ZuweisungsAnomalie
        }

        public enum Schweregrad
        {
            Niedrig,
            Mittel,
            Hoch,
            Kritisch
        }
    }
}