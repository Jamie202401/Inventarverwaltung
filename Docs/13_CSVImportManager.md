# CSVImportManager — CSV-Import

**Datei:** `Import.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class CSVImportManager`

---

## Zweck

`CSVImportManager` ermöglicht den Massenimport von Daten aus CSV-Dateien in die Anwendung. Unterstützt werden Inventarartikel, Mitarbeiter und Benutzer. Fehlerbehandlung und Zugriffskontrolle sind verbessert, um robuste Importe auch bei ungültigen oder gesperrten Dateien zu gewährleisten.

---

## Methoden

### `ZeigeImportMenu()`

Zeigt das interaktive Import-Hauptmenü:

| Auswahl | Aktion |
|---|---|
| `[1]` | Inventar-Artikel importieren |
| `[2]` | Mitarbeiter importieren |
| `[3]` | Benutzer importieren |
| `[0]` | Zurück zum Hauptmenü |

---

### `ImportiereInventar()`

Importiert Inventarartikel aus einer CSV-Datei:

1. Benutzer gibt den Dateipfad ein
2. Datei wird zeilenweise gelesen
3. Kommentarzeilen (`#`) und Leerzeilen werden übersprungen
4. Jede Datenzeile wird nach `;` gesplittet
5. Unterstützte Formate: 3 Felder (minimal) oder 12 Felder (vollständig)
6. Gültige Artikel werden in `DataManager.Inventar` eingefügt
7. Nach dem Import wird `DataManager.SaveKomplettesInventar()` aufgerufen
8. Zusammenfassung: Anzahl importierter und übersprungener Zeilen
9. Log-Eintrag via `LogManager.LogImport()`

---

### `ImportiereMitarbeiter()`

Importiert Mitarbeiter aus einer CSV-Datei:

- Erwartetes Format: `Vorname;Nachname;Abteilung`
- Fehlende oder leere Felder werden übersprungen
- Duplikatprüfung (Vor- + Nachname kombiniert)
- Speichert nach erfolgreichem Import

---

### `ImportiereBenutzer()`

Importiert Benutzer-Accounts aus einer CSV-Datei:

- Erwartetes Format: `Benutzername;Berechtigung`
- Ungültige Rollen werden als `User` behandelt
- Bestehende Benutzernamen werden übersprungen (kein Überschreiben)
- Speichert nach erfolgreichem Import

---

### `LeseCSVDatei(pfad) → string[]` *(private)*

Liest eine CSV-Datei mit korrekter Fehlerbehandlung. Unterstützt UTF-8 und verschiedene Zeilenendungen. Gibt `null` zurück wenn die Datei nicht existiert oder nicht gelesen werden kann.

---

### `FrageDateipfad(hinweis) → string` *(private)*

Fordert den Benutzer zur Eingabe eines Dateipfads auf. Zeigt einen Kontexthint an. Prüft ob die Datei existiert und gibt eine Fehlermeldung wenn nicht.

---

## CSV-Formate

### Inventar (vollständig, 12 Felder):

```
INV001;Laptop Dell XPS;Max Mustermann;SN-123;1299.99;01.01.2024;Dell;IT;5;2;admin;17.02.2026 10:00:00
```

### Mitarbeiter:

```
Max;Mustermann;IT
```

### Benutzer:

```
max.mustermann;User
```

---

## Fehlerbehandlung

- Dateien mit falscher Anzahl an Feldern werden pro Zeile übersprungen
- `UnauthorizedAccessException` und `IOException` werden abgefangen und angezeigt
- Ungültige Datumswerte oder Zahlenformate führen zum Überspringen der Zeile (kein Programmabbruch)

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Einfügen und Speichern aller Daten |
| `LogManager` | `LogImport()` |
| `ConsoleHelper` | Eingaben, Fehler- und Erfolgsmeldungen |
