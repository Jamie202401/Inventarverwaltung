using Inventarverwaltung.Manager.Inventory;
namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // WERKZEUGE-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    public class ImportCommand : Core.ICommand
    {
        public string Key => "TOOL_IMPORT";
        public string Label => "Daten importieren (CSV)";
        public string Icon => "📥";
        public void Execute() => CSVImportManager.ZeigeImportMenu();
    }

    public class BearbeitungCommand : Core.ICommand
    {
        public string Key => "TOOL_EDIT";
        public string Label => "Artikel / Daten bearbeiten";
        public string Icon => "✏️ ";
        public void Execute() => Editmanager.ZeigeBearbeitungsMenu();
    }

    public class LoeschungCommand : Core.ICommand
    {
        public string Key => "TOOL_DEL";
        public string Label => "Artikel / Daten löschen";
        public string Icon => "🗑️ ";
        public void Execute() => DeleteManager.ZeigeLöschMenu();
    }
}