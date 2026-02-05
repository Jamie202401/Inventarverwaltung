using System;

namespace Inventarverwaltung
{
    /// <summary>
    /// Repräsentiert einen Inventar-Artikel mit vollständiger Bestandsführung
    /// ERWEITERT: Tracking von Erstellungsbenutzer und Zeitpunkt
    /// </summary>
    public class InvId
    {
        // Basis-Informationen
        public string InvNmr { get; set; }                    // Inventar-Nummer (z.B. INV001)
        public string GeraeteName { get; set; }               // Gerätename (z.B. Laptop Dell XPS)
        public string MitarbeiterBezeichnung { get; set; }    // Zugewiesener Mitarbeiter

        // Erweiterte Informationen
        public string SerienNummer { get; set; }              // Seriennummer (SNR)
        public decimal Preis { get; set; }                    // Anschaffungspreis
        public DateTime Anschaffungsdatum { get; set; }       // Kaufdatum
        public string Hersteller { get; set; }                // Hersteller (z.B. Dell, HP)
        public string Kategorie { get; set; }                 // Kategorie (z.B. IT, Büro)

        // Bestandsführung
        public int Anzahl { get; set; }                       // Aktuelle Anzahl/Menge
        public int Mindestbestand { get; set; }               // Mindestbestand (Warnschwelle)

        // Tracking-Informationen (NEU!)
        public string ErstelltVon { get; set; }               // Welcher Benutzer hat den Artikel angelegt
        public DateTime ErstelltAm { get; set; }              // Wann wurde der Artikel angelegt

        // Konstruktor für NEUE Artikel (mit allen Feldern inkl. Tracking)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung,
                     string serienNummer, decimal preis, DateTime anschaffungsdatum,
                     string hersteller, string kategorie, int anzahl, int mindestbestand,
                     string erstelltVon, DateTime erstelltAm)
        {
            InvNmr = invNmr;
            GeraeteName = geraeteName;
            MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = serienNummer;
            Preis = preis;
            Anschaffungsdatum = anschaffungsdatum;
            Hersteller = hersteller;
            Kategorie = kategorie;
            Anzahl = anzahl;
            Mindestbestand = mindestbestand;
            ErstelltVon = erstelltVon;
            ErstelltAm = erstelltAm;
        }

        // Konstruktor OHNE Tracking (für einfache Erstellung)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung,
                     string serienNummer, decimal preis, DateTime anschaffungsdatum,
                     string hersteller, string kategorie, int anzahl, int mindestbestand)
        {
            InvNmr = invNmr;
            GeraeteName = geraeteName;
            MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = serienNummer;
            Preis = preis;
            Anschaffungsdatum = anschaffungsdatum;
            Hersteller = hersteller;
            Kategorie = kategorie;
            Anzahl = anzahl;
            Mindestbestand = mindestbestand;
            ErstelltVon = "System";
            ErstelltAm = DateTime.Now;
        }

        // Konstruktor für ALTE Artikel (Rückwärtskompatibilität)
        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung)
        {
            InvNmr = invNmr;
            GeraeteName = geraeteName;
            MitarbeiterBezeichnung = mitarbeiterBezeichnung;
            SerienNummer = "N/A";
            Preis = 0.00m;
            Anschaffungsdatum = DateTime.Now;
            Hersteller = "Unbekannt";
            Kategorie = "Sonstiges";
            Anzahl = 1;
            Mindestbestand = 1;
            ErstelltVon = "System (Migriert)";
            ErstelltAm = DateTime.Now;
        }

        /// <summary>
        /// Berechnet den Bestandsstatus mit Farbcode
        /// </summary>
        public BestandsStatus GetBestandsStatus()
        {
            if (Anzahl == 0)
                return BestandsStatus.Leer;
            else if (Anzahl <= Mindestbestand)
                return BestandsStatus.Niedrig;
            else if (Anzahl <= Mindestbestand * 2)
                return BestandsStatus.Mittel;
            else
                return BestandsStatus.Gut;
        }

        /// <summary>
        /// Gibt den Bestandsstatus als farbigen Text zurück
        /// </summary>
        public string GetBestandsStatusText()
        {
            var status = GetBestandsStatus();
            switch (status)
            {
                case BestandsStatus.Leer:
                    return "🔴 LEER";
                case BestandsStatus.Niedrig:
                    return "🟡 NIEDRIG";
                case BestandsStatus.Mittel:
                    return "🟢 OK";
                case BestandsStatus.Gut:
                    return "🟢 GUT";
                default:
                    return "⚪ UNBEKANNT";
            }
        }
    }

    /// <summary>
    /// Bestandsstatus-Enum für Farbcodierung
    /// </summary>
    public enum BestandsStatus
    {
        Leer,       // Rot - 0 Stück
        Niedrig,    // Gelb - <= Mindestbestand
        Mittel,     // Grün - <= Mindestbestand * 2
        Gut         // Grün - > Mindestbestand * 2
    }

    /// <summary>
    /// Repräsentiert einen Mitarbeiter
    /// </summary>
    public class MID
    {
        public string VName { get; set; }      // Vorname
        public string NName { get; set; }      // Nachname
        public string Abteilung { get; set; }  // Abteilung

        public MID(string vName, string nName, string abteilung)
        {
            VName = vName;
            NName = nName;
            Abteilung = abteilung;
        }
    }

    /// <summary>
    /// Berechtigungsstufen
    /// </summary>
    public enum Berechtigungen
    {
        User,   // Normale Benutzer
        Admin   // Administratoren
    }

    /// <summary>
    /// Repräsentiert einen Benutzer-Account (in Accounts.txt)
    /// </summary>
    public class Accounts
    {
        public string Benutzername { get; set; }
        public Berechtigungen Berechtigung { get; set; }

        public Accounts(string benutzername, Berechtigungen berechtigung)
        {
            Benutzername = benutzername;
            Berechtigung = berechtigung;
        }
    }

    /// <summary>
    /// Repräsentiert eine Anmeldung (für Anmelder.txt - Legacy)
    /// </summary>
    public class Anmelder
    {
        public string Anmeldename { get; set; }

        public Anmelder(string anmeldename)
        {
            Anmeldename = anmeldename;
        }
    }
}