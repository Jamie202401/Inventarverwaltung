using Inventarverwaltung.Core;
using Inventarverwaltung.Manager.Auth;

namespace Inventarverwaltung.Commands
{
    // ── ROLLEN DETAILS ────────────────────────────────────────────────────────

    public class RollenUebersichtCommand : ICommand
    {
        public string Key => "ROLLEN_UEBERSICHT";
        public string Label => "Rollen & Berechtigungen anzeigen";
        public string Icon => "📋";
        public void Execute() => RollenManager.ZeigeRollenUebersicht();
    }

    public class NeueRolleCommand : ICommand
    {
        public string Key => "ROLLEN_NEU";
        public string Label => "Neue Rolle erstellen";
        public string Icon => "➕";
        public void Execute() => RollenManager.NeueRolleErstellen();
    }

    public class RolleBearbeitenCommand : ICommand
    {
        public string Key => "ROLLEN_BEARBEITEN";
        public string Label => "Rolle bearbeiten (Rechte anpassen)";
        public string Icon => "✏️ ";
        public void Execute() => RollenManager.RolleBearbeiten();
    }

    public class BenutzerRolleZuweisenCommand : ICommand
    {
        public string Key => "ROLLEN_ZUWEISEN";
        public string Label => "Benutzer-Berechtigungen verwalten";
        public string Icon => "👤";
        public void Execute() => RollenManager.BenutzerBerechtigungenVerwalten();
    }

    public class RolleLoeschenCommand : ICommand
    {
        public string Key => "ROLLEN_LOESCHEN";
        public string Label => "Rolle löschen";
        public string Icon => "🗑️ ";
        public void Execute() => RollenManager.RolleLoeschen();
    }
}