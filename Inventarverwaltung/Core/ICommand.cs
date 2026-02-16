namespace Inventarverwaltung.Core
{
    /// <summary>
    /// Jede Menü-Aktion implementiert dieses Interface.
    /// 
    /// Neuen Menüpunkt hinzufügen:
    ///   1. Neue Klasse in Commands/[Kategorie]Commands.cs anlegen
    ///   2. Dieses Interface implementieren (Key, Label, Icon, Execute)
    ///   3. In AppSetup.cs der gewünschten Gruppe per .Add() hinzufügen
    ///   → Fertig. Mehr braucht der Entwickler nicht zu tun.
    /// </summary>
    public interface ICommand
    {
        /// <summary>Interner Schlüssel, z.B. "INV_NEU"</summary>
        string Key { get; }

        /// <summary>Anzeigename im Untermenü, z.B. "Neuen Artikel hinzufügen"</summary>
        string Label { get; }

        /// <summary>Emoji-Icon für die Anzeige, z.B. "📦"</summary>
        string Icon { get; }

        /// <summary>Führt die Aktion aus</summary>
        void Execute();
    }
}