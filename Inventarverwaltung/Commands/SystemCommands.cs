using Inventarverwaltung.Manager.UI;
using Inventarverwaltung.Security;

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

    // FIX CS0117: GuardZuruecksetzen existiert nicht mehr.
    // Korrekte Methode heißt jetzt ReferenzHashAktualisieren()
    public class ScannerGuardResetCommand : Core.ICommand
    {
        public string Key => "SYS_GUARD";
        public string Label => "Scanner-Guard aktualisieren (Admin)";
        public string Icon => "🔑";
        public void Execute() => ScannerSchutz.ReferenzHashAktualisieren();
    }

    public class ScannerAuditLogCommand : Core.ICommand
    {
        public string Key => "SYS_AUDIT";
        public string Label => "Scanner-Audit-Log anzeigen";
        public string Icon => "🔒";
        public void Execute() => ScannerSchutz.ZeigeAuditLog();
    }
    /*public class NotfallPasswortAendernCommand : Core.ICommand
    {
        public string Key => "SYS_NOTFALL_PW";
        public string Label => "Notfall-Passwort ändern";
        public string Icon => "🆘";
        public void Execute() => NotfallZugang.PasswortAendern();
    }*/
}