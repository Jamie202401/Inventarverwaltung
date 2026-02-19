using Inventarverwaltung.Manager.UI;
namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // SYSTEM-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    public class SystemLogCommand : Core.ICommand
    {
        public string Key => "SYS_LOG";
        public string Label => "System-Log anzeigen";
        public string Icon => "📝";
        public void Execute() => LogManager.ZeigeLogDatei();
    }

    public class TagesreportCommand : Core.ICommand
    {
        public string Key => "SYS_REP";
        public string Label => "Tagesreport erstellen";
        public string Icon => "📄";
        public void Execute()
        {
            LogManager.ErstelleTagesReport();
            ConsoleHelper.PressKeyToContinue();
        }
    }

    public class VerschluesselungCommand : Core.ICommand
    {
        public string Key => "SYS_ENC";
        public string Label => "Verschlüsselungs-Info anzeigen";
        public string Icon => "🔐";
        public void Execute() => EncryptionManager.ZeigeVerschluesselungsInfo();
    }
}