[README.md](https://github.com/user-attachments/files/24965418/README.md)
# Inventarverwaltung - Projektstruktur

## 🎯 Übersicht
Das Projekt wurde in mehrere übersichtliche Dateien aufgeteilt, um die Wartbarkeit und Lesbarkeit zu verbessern.

## 📁 Dateistruktur

### Program.cs
- Enthält die Main-Methode (Einstiegspunkt der Anwendung)
- Koordiniert den Programmablauf
- Verwaltet die Hauptschleife und Menüauswahl

### Models.cs
- Enthält alle Datenmodelle:
  - `Berechtigungen` (Enum für User/Admin)
  - `MID` (Mitarbeiter-Klasse)
  - `InvId` (Inventar-Klasse)
  - `Accounts` (Benutzer-Klasse mit Berechtigungen)
  - `Anmelder` (Anmelde-Klasse)

### FileManager.cs
- Verwaltet Dateipfade
- Erstellt und versteckt Dateien
- Zentrale Datei-Konfiguration

### ConsoleHelper.cs
- Hilfsfunktionen für die Konsole
- Konsolen-Setup (Größe, Titel, etc.)
- Formatierte Ausgaben (Erfolg, Fehler, Warnung, Info)
- Header- und Menü-Formatierung

### MenuManager.cs
- Zeigt das Hauptmenü mit Icons an
- Formatiert Menüeinträge übersichtlich

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
- Neuen Artikel erstellen (mit Fehlerbehandlung)
- Inventar anzeigen (übersichtliche Tabelle)

### EmployeeManager.cs
- Verwaltet Mitarbeiter-Operationen
- Neuen Mitarbeiter hinzufügen (mit Validierung)
- Mitarbeiter anzeigen

### UserManager.cs
- Verwaltet Benutzer-Operationen
- Benutzer mit Berechtigungen anlegen
- Benutzer anzeigen

## ✨ Neue Verbesserungen

### 1. **Intelligente Fehlerbehandlung**
- Bei falschen Eingaben wird nur die fehlerhafte Eingabe wiederholt
- Keine komplette Neueingabe mehr nötig
- Farbcodierte Fehlermeldungen (rot) und Erfolgsmeldungen (grün)

### 2. **Schöneres Design**
- Unicode-Rahmen (╔═══╗) statt einfacher Linien
- Icons für bessere Übersichtlichkeit (📦, 👤, 👥, 📊, 🔐, etc.)
- Farbcodierte Ausgaben:
  - ✓ Grün für Erfolg
  - ✗ Rot für Fehler
  - ⚠ Gelb für Warnungen
  - ℹ Cyan für Informationen

### 3. **Bessere Benutzerführung**
- Hilfreiche Beispiele bei Eingaben
- Verfügbare Optionen werden angezeigt
- Klare Anweisungen bei jeder Aktion

### 4. **Validierung**
- Prüfung auf leere Eingaben
- Mindestlänge für Benutzernamen (3 Zeichen)
- Duplikatsprüfung vor dem Speichern
- Existenzprüfung bei Zuweisungen

### 5. **Übersichtliche Tabellen**
- Einheitliche Tabellenformatierung
- Nummerierung der Einträge
- Zusammenfassung am Ende (z.B. "Gesamt: 5 Mitarbeiter")

## 📋 Beispiel-Ablauf

### Neuen Artikel erstellen:
1. Inventarnummer eingeben
   - Falls ungültig → nur Inventarnummer erneut eingeben
2. Gerätename eingeben
   - Falls ungültig → nur Gerätename erneut eingeben
3. Mitarbeiter auswählen
   - Zeigt alle verfügbaren Mitarbeiter an
   - Falls nicht vorhanden → nur Mitarbeitername erneut eingeben
4. ✓ Erfolg! Artikel wurde gespeichert

## 🎨 Farbschema

- **Cyan**: Header, Titel, wichtige Informationen
- **Grün**: Erfolgsmeldungen
- **Rot**: Fehlermeldungen
- **Gelb**: Warnungen
- **Blau**: Überschriften von Abschnitten
- **Grau**: Tabellenkopfzeilen

## 💡 Vorteile der Aufteilung

1. **Bessere Übersichtlichkeit**: Jede Datei hat eine klare Verantwortung
2. **Einfachere Wartung**: Änderungen können gezielt vorgenommen werden
3. **Wiederverwendbarkeit**: Klassen können unabhängig verwendet werden
4. **Teamarbeit**: Mehrere Entwickler können parallel arbeiten
5. **Testbarkeit**: Einzelne Komponenten können isoliert getestet werden
6. **Benutzerfreundlich**: Keine frustrierenden Komplett-Neueingaben mehr
7. **Professionell**: Modernes Design mit Icons und Farben

## 🔧 Kompilierung

Alle Dateien müssen zusammen kompiliert werden:

```bash
csc /out:Inventarverwaltung.exe *.cs
```

Oder in Visual Studio: Alle Dateien in ein Projekt einbinden und kompilieren.

## 📝 Hinweise

- Alle Kommentare sind auf Deutsch
- Code-Konventionen werden eingehalten
- Aussagekräftige Variablennamen
- Strukturierte Fehlerbehandlung
- Benutzerfreundliche Eingabe-Masken
