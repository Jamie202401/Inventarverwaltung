using Inventarverwaltung.Manager.AI;
using Inventarverwaltung.Manager.UI;
namespace Inventarverwaltung.Commands
{
    // ══════════════════════════════════════════════════════════════════
    // DASHBOARD & KI-COMMANDS
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Öffnet das allgemeine System-Dashboard (Kennzahlen, Status)</summary>
    public class SystemDashboardCommand : Core.ICommand
    {
        public string Key => "DASH_SYS";
        public string Label => "System-Dashboard";
        public string Icon => "📊";
        public void Execute() => DashboardManager.ZeigeDashboard();
    }

    public class SystemDashboardenhanced : Core.ICommand
    {
        public string Key => "DASH_ENC";
        public string Label => "Info-Dashboard";
        public string Icon => "📊";
        public void Execute() => DashboardEnhanced.ZeigeDashboardMenu();
    }

    /// <summary>Öffnet das KI-Control-Center (aus Kidashboard.cs)</summary>
    public class KIDashboardCommand : Core.ICommand
    {
        public string Key => "KI_DASH";
        public string Label => "KI-Dashboard & Control-Center";
        public string Icon => "🤖";
        public void Execute() => KIDashboard.ZeigeKIControlCenter();
    }

    /// <summary>Öffnet die erweiterten KI-Engine Insights (aus KIengine.cs)</summary>
    public class KIEngineInsightsCommand : Core.ICommand
    {
        public string Key => "KI_ENGINE";
        public string Label => "KI-Engine Insights & Analyse";
        public string Icon => "🧠";
        public void Execute() => KIEngine.ZeigeErweiterteInsights();
    }

    /// <summary>Initialisiert die KI-Engine neu</summary>
    public class KIEngineInitCommand : Core.ICommand
    {
        public string Key => "KI_INIT";
        public string Label => "KI-Engine neu initialisieren";
        public string Icon => "🔄";
        public void Execute()
        {
            KIEngine.Initialisiere();
            IntelligentAssistant.IniializeAI();
            ConsoleHelper.PressKeyToContinue();
        }
    }
}