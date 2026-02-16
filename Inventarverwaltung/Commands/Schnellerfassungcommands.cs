namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // SCHNELLERFASSUNG-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Öffnet das Schnellerfassungs-Center (aus Schnellerfassungsmanager.cs)</summary>
    public class SchnellerfassungCommand : Core.ICommand
    {
        public string Key => "SCHNELL";
        public string Label => "Schnellerfassungs-Center";
        public string Icon => "⚡";
        public void Execute() => SchnellerfassungsManager.ZeigeSchnellerfassungsMenu();
    }
}