namespace Inventarverwaltung
{
    /// <summary>
    /// Berechtigungsstufen für Benutzer
    /// </summary>
    public enum Berechtigungen
    {
        Unbekannt = 0,  // Keine/unbekannte Berechtigung
        User = 1,       // Normaler Benutzer (nur lesen und hinzufügen)
        Admin = 2,      // Administrator (alle Rechte)
    }

    /// <summary>
    /// Mitarbeiter-Datenmodell
    /// Speichert Informationen über einen Mitarbeiter
    /// </summary>
    public class MID
    {
        public string NName { get; set; }      // Nachname
        public string VName { get; set; }      // Vorname
        public string Abteilung { get; set; }  // Abteilung (z.B. IT, Vertrieb)

        public MID(string vName, string nName, string abteilung)
        {
            this.VName = vName;
            this.NName = nName;
            this.Abteilung = abteilung;
        }
    }

    /// <summary>
    /// Inventar-Datenmodell
    /// Speichert Informationen über ein Inventarobjekt
    /// </summary>
    public class InvId
    {
        public string InvNmr { get; set; }                  // Inventarnummer (z.B. INV001)
        public string GeraeteName { get; set; }             // Name des Geräts
        public string MitarbeiterBezeichnung { get; set; }  // Zugewiesener Mitarbeiter

        public InvId(string invNmr, string geraeteName, string mitarbeiterBezeichnung)
        {
            this.InvNmr = invNmr;
            this.GeraeteName = geraeteName;
            this.MitarbeiterBezeichnung = mitarbeiterBezeichnung;
        }
    }

    /// <summary>
    /// Benutzer-Account-Datenmodell
    /// Speichert Benutzername und Berechtigungen
    /// </summary>
    public class Accounts
    {
        public string Benutzername { get; set; }
        public Berechtigungen Berechtigung { get; set; }
        public string V1 { get; }  // Legacy-Feld (wird nicht mehr verwendet)
        public string V2 { get; }  // Legacy-Feld (wird nicht mehr verwendet)

        public Accounts(string benutzername, Berechtigungen berechtigung)
        {
            Benutzername = benutzername;
            Berechtigung = berechtigung;
        }

        // Legacy-Konstruktor (für Abwärtskompatibilität)
        public Accounts(string v1, string v2)
        {
            V1 = v1;
            V2 = v2;
        }
    }

    /// <summary>
    /// Anmelde-Datenmodell
    /// Speichert den Benutzernamen für die Anmeldung
    /// </summary>
    public class Anmelder
    {
        public string Anmeldename { get; set; }

        public Anmelder(string anmeldename)
        {
            this.Anmeldename = anmeldename;
        }
    }
}