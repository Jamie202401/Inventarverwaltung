[README.md](https://github.com/user-attachments/files/24965224/README.md)
# Inventarverwaltung - Projektstruktur

## Übersicht
Das Projekt wurde in mehrere übersichtliche Dateien aufgeteilt, um die Wartbarkeit und Lesbarkeit zu verbessern.

## Dateistruktur

### Program.cs
- Enthält die Main-Methode (Einstiegspunkt der Anwendung)
- Koordiniert den Programmablauf
- Verwaltet die Hauptschleife und Menüauswahl

### Models.cs
- Enthält alle Datenmodelle:
  - `Berechtigungen` (Enum)
  - `MID` (Mitarbeiter-Klasse)
  - `InvId` (Inventar-Klasse)
  - `Accounts` (Benutzer-Klasse)
  - `Anmelder` (Anmelde-Klasse)

### FileManager.cs
- Verwaltet Dateipfade
- Erstellt und versteckt Dateien
- Zentrale Datei-Konfiguration

### ConsoleHelper.cs
- Hilfsfunktionen für die Konsole
- Konsolen-Setup (Größe, Titel, etc.)
- Header- und Menü-Formatierung

### MenuManager.cs
- Zeigt das Hauptmenü an
- Formatiert Menüeinträge

### AuthManager.cs
- Verwaltet die Benutzeranmeldung
- Erstellt neue Anmeldekonten
- Prüft bestehende Benutzer

### DataManager.cs
- Zentrale Klasse für Datenverwaltung
- Laden und Speichern von:
  - Inventar
  - Mitarbeitern
  - Benutzern
  - Anmeldungen
- Verwaltet alle Listen (Inventar, Mitarbeiter, Benutzer, Anmeldung)

### InventoryManager.cs
- Verwaltet Inventar-Operationen
- Neuen Artikel erstellen
- Inventar anzeigen

### EmployeeManager.cs
- Verwaltet Mitarbeiter-Operationen
- Neuen Mitarbeiter hinzufügen
- Mitarbeiter anzeigen

### UserManager.cs
- Verwaltet Benutzer-Operationen
- Benutzer mit Berechtigungen anlegen
- Benutzer anzeigen

## Vorteile der Aufteilung

1. **Bessere Übersichtlichkeit**: Jede Datei hat eine klare Verantwortung
2. **Einfachere Wartung**: Änderungen können gezielt vorgenommen werden
3. **Wiederverwendbarkeit**: Klassen können unabhängig verwendet werden
4. **Teamarbeit**: Mehrere Entwickler können parallel arbeiten
5. **Testbarkeit**: Einzelne Komponenten können isoliert getestet werden

## Kompilierung

Alle Dateien müssen zusammen kompiliert werden:

```bash
csc /out:Inventarverwaltung.exe *.cs
```

Oder in Visual Studio: Alle Dateien in ein Projekt einbinden und kompilieren.
