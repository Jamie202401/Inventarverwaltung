namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // BESTANDSPFLEGE-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    public class BestandErhoehenCommand : Core.ICommand
    {
        public string Key => "BEST_ERHÖH";
        public string Label => "Bestand erhöhen";
        public string Icon => "➕";
        public void Execute() => InventoryManager.BestandErhoehen();
    }

    public class BestandVerringernCommand : Core.ICommand
    {
        public string Key => "BEST_VERR";
        public string Label => "Bestand verringern";
        public string Icon => "➖";
        public void Execute() => InventoryManager.BestandVerringern();
    }

    public class MindestbestandCommand : Core.ICommand
    {
        public string Key => "BEST_MIN";
        public string Label => "Mindestbestand konfigurieren";
        public string Icon => "⚙️ ";
        public void Execute() => InventoryManager.MindestbestandAendern();
    }

    public class BestandWarnungCommand : Core.ICommand
    {
        public string Key => "BEST_WARN";
        public string Label => "Artikel unter Mindestbestand anzeigen";
        public string Icon => "🔴";
        public void Execute() => InventoryManager.ZeigeArtikelUnterMindestbestand();
    }
}