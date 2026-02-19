using System;

namespace Inventarverwaltung.Manager.Models
{
    /// <summary>
    /// Repräsentiert eine konfigurierbare Rolle mit individuellem Rechte-Set
    /// </summary>
    public class Rolle
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public ConsoleColor Farbe { get; set; }
        public RollenRechte Rechte { get; set; } = new RollenRechte();

        public Rolle(string name, string icon, ConsoleColor farbe)
        {
            Name = name;
            Icon = icon;
            Farbe = farbe;
        }
    }

    /// <summary>
    /// Alle granularen Einzelrechte einer Rolle.
    /// Jedes Recht = ein bool-Flag.
    /// </summary>
    public class RollenRechte
    {
        // ── Benutzerverwaltung ────────────────────────────────────────────────
        /// <summary>Darf Rollen erstellen, bearbeiten, löschen</summary>
        public bool RollenVerwalten { get; set; }

        /// <summary>Darf Benutzer anlegen und bearbeiten</summary>
        public bool BenutzerVerwalten { get; set; }

        // ── Mitarbeiter ───────────────────────────────────────────────────────
        /// <summary>Darf Mitarbeiterdaten bearbeiten</summary>
        public bool MitarbeiterBearbeiten { get; set; }

        /// <summary>Darf Mitarbeiter löschen</summary>
        public bool MitarbeiterLoeschen { get; set; }

        // ── Inventar ──────────────────────────────────────────────────────────
        /// <summary>Darf Inventar-Artikel bearbeiten</summary>
        public bool InventarBearbeiten { get; set; }

        /// <summary>Darf Inventar-Artikel löschen</summary>
        public bool InventarLoeschen { get; set; }

        /// <summary>Darf Bestand erhöhen/verringern und Limits setzen</summary>
        public bool BestandAendern { get; set; }

        // ── Betrieb ───────────────────────────────────────────────────────────
        /// <summary>Darf Hardware-Druck-Aufträge verwalten</summary>
        public bool DruckVerwalten { get; set; }

        /// <summary>Darf Daten importieren und exportieren</summary>
        public bool ImportExport { get; set; }

        // ── System ────────────────────────────────────────────────────────────
        /// <summary>Darf System-Logs einsehen</summary>
        public bool SystemLog { get; set; }

        /// <summary>Darf Verschlüsselung verwalten</summary>
        public bool Verschluesselung { get; set; }

        /// <summary>Darf das KI-Dashboard nutzen</summary>
        public bool KIDashboard { get; set; }

        /// <summary>Darf die Schnellerfassung nutzen</summary>
        public bool Schnellerfassung { get; set; }
    }
}