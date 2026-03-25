using Inventarverwaltung.Manager.Inventory;
using Inventarverwaltung.Manager.Data;
namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // INVENTAR-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    public class InventarNeuCommand : Core.ICommand
    {
        public string Key => "INV_NEU";
        public string Label => "Neuen Artikel hinzufügen";
        public string Icon => "📦";
        public void Execute() => InventoryManager.NeuenArtikelErstellen();
    }

    public class InventarZeigeCommand : Core.ICommand
    {
        public string Key => "INV_ZEIGE";
        public string Label => "Inventar anzeigen";
        public string Icon => "📋";
        public void Execute() => InventoryManager.ZeigeInventar();
    }

    public class InventarDetailCommand : Core.ICommand
    {
        public string Key => "INV_DETAIL";
        public string Label => "Artikel-Details anzeigen";
        public string Icon => "🔍";
        public void Execute() => InventoryManager.ZeigeArtikelDetails();
    }

    public class ZuweisungsVerwaltungCommand : Core.ICommand
    {
        public string Key => "INV_ZUWEISUNG";
        public string Label => "Zuweisungsverwaltung";
        public string Icon => "🔄";
        public void Execute() => ZuweisungsManager.ZeigeZuweisungsMenu();
    }
    public class TagVerwaltungCommand : Core.ICommand
    {
        public string Key => "TAG_VERWALTUNG";
        public string Label => "Tag-Verwaltung (Bearbeitungs-Tags)";
        public string Icon => "🏷️ ";
        public void Execute() => TagManager.ZeigeTagUebersicht();
    }
    public class AnhangVerwaltungCommand : Core.ICommand
    {
        public string Key => "ANHAENGE_HINZUFUEGEN";
        public string Label  => "Anhänge oder Dokus an den Artikel hängen";
        public string Icon => "[]";
        public void Execute() => AnhangManager.StarteAnhangMenu();
    }
}