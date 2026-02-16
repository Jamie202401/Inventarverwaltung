using Inventarverwaltung;
using Inventarverwaltung.Core;

namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // LIEFERANTEN-COMMANDS
    // Eingebunden in AppSetup.cs unter MenuGroup [9]
    // ══════════════════════════════════════════════════════════════════

    public class LieferantMenuCommand : ICommand
    {
        public string Key => "LF_MENU";
        public string Label => "Lieferanten verwalten";
        public string Icon => "🚚";
        public void Execute() => LieferantManager.ZeigeLieferantMenu();
    }
}