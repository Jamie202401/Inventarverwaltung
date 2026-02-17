# EditManager — Bearbeitung

**Datei:** `Editmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class Editmanager`

---

## Zweck

`Editmanager` ermöglicht das Bearbeiten bestehender Datensätze: Inventarartikel, Mitarbeiter und Benutzer. Alle Änderungen werden nach Bestätigung gespeichert und geloggt.

---

## Methoden

### `ZeigeBearbeitungsMenu()`

Zeigt das interaktive Bearbeitungs-Hauptmenü:

| Auswahl | Aktion |
|---|---|
| `[1]` | Artikel bearbeiten |
| `[2]` | Mitarbeiter bearbeiten |
| `[3]` | Benutzer bearbeiten |
| `[0]` | Zurück zum Hauptmenü |

---

### `BearbeiteArtikel()`

Bearbeitungs-Workflow für Inventarartikel:

1. Benutzer sucht nach Inventarnummer oder Gerätename
2. Gefundener Artikel wird als Detailkarte angezeigt
3. Auswahl welches Feld bearbeitet werden soll
4. Neue Eingabe mit Anzeige des alten Wertes
5. Bestätigung der Änderung
6. `DataManager.SaveKomplettesInventar()` wird aufgerufen
7. Log-Eintrag

**Editierbare Felder:**

| Feld | Validierung |
|---|---|
| Gerätename | Pflichtfeld, min. 2 Zeichen |
| Mitarbeiter | Muss aus bestehender Liste gewählt werden |
| Seriennummer | Freitext |
| Preis | Dezimalzahl ≥ 0 |
| Anschaffungsdatum | Format `dd.MM.yyyy` |
| Hersteller | Freitext |
| Kategorie | Freitext |
| Anzahl | Ganzzahl ≥ 0 |
| Mindestbestand | Ganzzahl ≥ 0 |

> Die Inventarnummer und Tracking-Felder (`ErstelltVon`, `ErstelltAm`) sind nicht editierbar.

---

### `BearbeiteMitarbeiter()`

Bearbeitungs-Workflow für Mitarbeiter:

1. Suche nach Name oder Listenauswahl
2. Anzeige der aktuellen Daten
3. Wahl des zu ändernden Feldes (Vorname, Nachname, Abteilung)
4. Neue Eingabe und Bestätigung
5. Prüfung auf Duplikate bei Namensänderung
6. `DataManager.SaveMitarbeiterToFile()` wird aufgerufen

---

### `BearbeiteBenutzer()`

Bearbeitungs-Workflow für Benutzer-Accounts:

1. Suche nach Benutzername
2. Anzeige der aktuellen Daten
3. Mögliche Änderung: Berechtigung (User / Admin)
4. Bestätigung und Speicherung
5. `DataManager.SaveBenutzerToFile()` wird aufgerufen

> Benutzernamen können nicht geändert werden. Für Namensänderungen muss der Account neu angelegt werden.

---

## Sicherheitshinweis

Vor jeder destruktiven Änderung (Überschreiben von Feldern) zeigt der Manager eine Bestätigungsabfrage mit dem alten und neuen Wert an. Nur nach expliziter Bestätigung wird gespeichert.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Alle Datenlisten und Speichermethoden |
| `LogManager` | Logging aller Änderungen |
| `ConsoleHelper` | Eingaben, Meldungen |
