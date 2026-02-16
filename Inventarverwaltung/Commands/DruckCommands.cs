namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // HARDWARE-DRUCK-COMMANDS
    //
    // Jeder Command startet direkt die zugehörige Methode im
    // HardwarePrintManager — kein Umweg über Routing-Strings.
    // ══════════════════════════════════════════════════════════════════

    public class DruckNeuCommand : Core.ICommand
    {
        public string Key => "DRUCK_NEU";
        public string Label => "Neues Ausgabe-Dokument drucken";
        public string Icon => "🖨️ ";
        public void Execute() => HardwarePrintManager.DruckNeuStart();
    }

    public class DruckHistorieCommand : Core.ICommand
    {
        public string Key => "DRUCK_HIST";
        public string Label => "Druckhistorie anzeigen";
        public string Icon => "📚";
        public void Execute() => HardwarePrintManager.DruckHistorieStart();
    }

    public class DruckSucheCommand : Core.ICommand
    {
        public string Key => "DRUCK_SUCH";
        public string Label => "Druckhistorie durchsuchen";
        public string Icon => "🔍";
        public void Execute() => HardwarePrintManager.DruckSucheStart();
    }

    public class DruckEditCommand : Core.ICommand
    {
        public string Key => "DRUCK_EDIT";
        public string Label => "Historien-Eintrag bearbeiten";
        public string Icon => "✏️ ";
        public void Execute() => HardwarePrintManager.DruckEditStart();
    }

    public class DruckKonfigCommand : Core.ICommand
    {
        public string Key => "DRUCK_KONF";
        public string Label => "Druckprogramm konfigurieren";
        public string Icon => "⚙️ ";
        public void Execute() => HardwarePrintManager.DruckKonfigStart();
    }
}