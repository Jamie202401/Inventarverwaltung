# DataManager — Zentrale Datenverwaltung

**Datei:** `Datamanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class DataManager`

---

## Zweck

`DataManager` ist die zentrale Schnittstelle für das Laden und Speichern aller Anwendungsdaten. Er verwaltet die In-Memory-Listen für Inventar, Mitarbeiter und Benutzer und liest/schreibt die zugehörigen Textdateien in einem strukturierten, menschen-lesbaren Format mit optionalem Header.

---

## Globale Datenlisten

| Feld | Typ | Beschreibung |
|---|---|---|
| `Inventar` | `List<InvId>` | Alle geladenen Inventarartikel |
| `Mitarbeiter` | `List<MID>` | Alle geladenen Mitarbeiter |
| `Benutzer` | `List<Accounts>` | Alle geladenen Benutzer-Accounts |
| `Anmeldung` | `List<Anmelder>` | Legacy-Anmeldedaten |

---

## Inventar-Methoden

### `LoadInventar()`

Liest `Inventar.txt` und befüllt `DataManager.Inventar`. Unterstützt drei Formatversionen:

- **3 Felder:** Altes Minimal-Format (Rückwärtskompatibilität)
- **10 Felder:** Mittlere Version ohne Tracking
- **12+ Felder:** Aktuelle Version mit `ErstelltVon` und `ErstelltAm`

Header-Zeilen (`#`, `═`, `╔`, etc.) und der `[DATEN]`-Marker werden korrekt verarbeitet.

---

### `SaveInvToFile()`

Fügt den zuletzt hinzugefügten Artikel der Datei hinzu (Append-Modus). Wenn die Datei noch nicht existiert oder leer ist, wird `SaveKomplettesInventar()` aufgerufen.

---

### `SaveKomplettesInventar()`

Schreibt das gesamte Inventar neu in `Inventar.txt` mit vollständigem Header, Strukturbeschreibung und Footer. Entfernt zuvor ggf. `Hidden`- und `ReadOnly`-Attribute der Datei.

---

## Mitarbeiter-Methoden

### `LoadMitarbeiter()`

Liest `Mitarbeiter.txt` und befüllt `DataManager.Mitarbeiter`. Verarbeitet denselben Header-Skip-Mechanismus wie `LoadInventar()`.

### `SaveMitarbeiterToFile()`

Speichert alle Mitarbeiter in `Mitarbeiter.txt` mit schönem Format.

---

## Benutzer-Methoden

### `LoadBenutzer()`

Liest `Accounts.txt` und befüllt `DataManager.Benutzer`. Erwartet `Benutzername;Berechtigung` pro Zeile. Ungültige Rollen-Strings werden als `User` behandelt.

### `SaveBenutzerToFile()`

Speichert alle Benutzer mit Rollen-Icon (`👑` Admin, `👤` User) in `Accounts.txt`.

---

## Bestandsverwaltungs-Methoden

### `BestandErhoehen(invNr, menge) → bool`

Erhöht den Bestand eines Artikels um `menge`. Gibt `false` zurück wenn der Artikel nicht gefunden wird. Speichert danach das komplette Inventar.

### `BestandVerringern(invNr, menge) → bool`

Verringert den Bestand um `menge`. Gibt `false` zurück wenn der Artikel nicht gefunden wird oder nicht genug Bestand vorhanden ist.

### `MindestbestandAendern(invNr, neuerMindestbestand) → bool`

Ändert den Mindestbestand eines Artikels und speichert.

### `GetArtikelUnterMindestbestand() → List<InvId>`

Gibt alle Artikel zurück, deren aktueller Bestand kleiner oder gleich dem Mindestbestand ist.

### `GetBestandsStatistik() → (gesamt, leer, niedrig, ok)`

Gibt eine Statistik-Zusammenfassung als Tupel zurück:
- `gesamt`: Gesamtanzahl Artikel
- `leer`: Artikel mit Anzahl = 0
- `niedrig`: Artikel mit Anzahl > 0 und ≤ Mindestbestand
- `ok`: Artikel mit Anzahl > Mindestbestand

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `FileManager` | Liefert Dateipfade (`FilePath`, `FilePath2`, `FilePath3`) |
| `LogManager` | Logging aller Lade- und Speichervorgänge |
| `InvId`, `MID`, `Accounts` | Datenmodelle |