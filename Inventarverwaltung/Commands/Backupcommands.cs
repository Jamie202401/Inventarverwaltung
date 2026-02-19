using Inventarverwaltung.Core;
using Inventarverwaltung.Manager.UI;

namespace Inventarverwaltung.Commands
{
    public class BackupErstellenCommand : ICommand
    {
        public string Key => "BACKUP_ERSTELLEN";
        public string Label => "Backup jetzt erstellen";
        public string Icon => "💾";
        public void Execute() => BackupManager.ManuellesBackup();
    }

    public class BackupUebersichtCommand : ICommand
    {
        public string Key => "BACKUP_UEBERSICHT";
        public string Label => "Backup-Übersicht & Wiederherstellen";
        public string Icon => "📋";
        public void Execute() => BackupManager.ZeigeBackupUebersicht();
    }

    public class BackupMasterPasswortCommand : ICommand
    {
        public string Key => "BACKUP_MASTER_PW";
        public string Label => "Master-Passwort ändern";
        public string Icon => "🔐";
        public void Execute() => BackupManager.MasterPasswortAendern();
    }
}