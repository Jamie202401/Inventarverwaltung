namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // MITARBEITER & BENUTZER-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    public class MitarbeiterNeuCommand : Core.ICommand
    {
        public string Key => "MA_NEU";
        public string Label => "Neuen Mitarbeiter anlegen";
        public string Icon => "👤";
        public void Execute() => EmployeeManager.NeuenMitarbeiterHinzufuegen();
    }

    public class MitarbeiterZeigeCommand : Core.ICommand
    {
        public string Key => "MA_ZEIGE";
        public string Label => "Mitarbeiter anzeigen";
        public string Icon => "👥";
        public void Execute() => EmployeeManager.ZeigeMitarbeiter();
    }

    public class BenutzerNeuCommand : Core.ICommand
    {
        public string Key => "USER_NEU";
        public string Label => "Neuen Benutzer-Account anlegen";
        public string Icon => "🔐";
        public void Execute() => UserManager.NeuerBenutzer();
    }

    public class BenutzerZeigeCommand : Core.ICommand
    {
        public string Key => "USER_ZEIGE";
        public string Label => "Benutzer-Accounts anzeigen";
        public string Icon => "📋";
        public void Execute() => UserManager.ZeigeBenutzer();
    }
}