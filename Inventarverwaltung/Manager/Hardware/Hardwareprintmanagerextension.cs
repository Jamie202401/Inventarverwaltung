namespace Inventarverwaltung
{
    /// <summary>
    /// Ergänzt HardwarePrintManager um 5 öffentliche Einstiegspunkte
    /// die von DruckCommands.cs aufgerufen werden.
    ///
    /// WICHTIG: Diese Datei einfach zum Projekt hinzufügen.
    ///          Die originale HardwarePrintManager.cs bleibt unverändert.
    ///
    /// Da C# keine partiellen statischen Klassen über Dateien hinweg
    /// unterstützt (partial static), ist diese Klasse ein schlanker
    /// Wrapper der die privaten Methoden über ZeigeDruckMenu() erreicht —
    /// oder du fügst "partial" zur HardwarePrintManager-Klasse hinzu
    /// (siehe Kommentar unten).
    ///
    /// EINFACHSTE LÖSUNG (1 Zeile in HardwarePrintManager.cs ändern):
    ///   public static class HardwarePrintManager
    ///   →
    ///   public static partial class HardwarePrintManager
    ///
    /// Dann kompiliert diese Datei direkt als Erweiterung.
    /// </summary>
    public static partial class HardwarePrintManager
    {
        /// <summary>Startet den Ablauf für ein neues Druckdokument.</summary>
        public static void DruckNeuStart()
            => NeuesDokumentErstellen();

        /// <summary>Öffnet die Druckhistorie-Ansicht.</summary>
        public static void DruckHistorieStart()
            => ZeigeDruckHistorie();

        /// <summary>Öffnet die Suche in der Druckhistorie.</summary>
        public static void DruckSucheStart()
            => SucheDruckHistorie();

        /// <summary>Öffnet die Bearbeitung eines Historien-Eintrags.</summary>
        public static void DruckEditStart()
            => BearbeiteHistorienEintrag();

        /// <summary>Öffnet die Druckprogramm-Konfiguration.</summary>
        public static void DruckKonfigStart()
            => DruckerAuswaehlen();
    }
}