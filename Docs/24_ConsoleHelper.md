# ConsoleHelper — Konsolen-Hilfsfunktionen

**Datei:** `Consolehelper.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class ConsoleHelper`

---

## Zweck

`ConsoleHelper` stellt zentrale Hilfsmethoden für alle Konsolen-Ausgaben und Benutzereingaben bereit. Alle anderen Manager verwenden diese Klasse für einheitliches Styling, Farben und Formatierung — dadurch ist das gesamte Erscheinungsbild konsistent.

---

## Methoden

### `SetupConsole()`

Initialisiert das Konsolenfenster beim Programmstart:
- Titel: `INVENTARVERWALTUNG`
- Fenstergröße: 150 × 60 Zeichen
- Puffergröße: 150 × 60 Zeichen
- Cursor sichtbar: `true`

---

### `PrintHeader()`

Zeigt den zentrierten Haupttitel der Anwendung in Cyan:

```
╔════════════════════════════════════════════╗
║      INVENTARVERWALTUNG SYSTEM             ║
╚════════════════════════════════════════════╝
```

---

### `PrintWelcome()`

Zeigt die Willkommensnachricht nach dem Start mit 1,5-Sekunden Pause.

---

### `PrintMenuItem(key, text)`

Gibt einen formatierten Menüpunkt aus:
```
  [key]  text
```

---

### `PrintSuccess(message)`

Erfolgstext in **Grün** mit `✓`-Prefix:
```
✓ Operation erfolgreich!
```

---

### `PrintError(message)`

Fehlertext in **Rot** mit `✗`-Prefix:
```
✗ Ein Fehler ist aufgetreten!
```

---

### `PrintWarning(message)`

Warntext in **Gelb** mit `⚠`-Prefix:
```
⚠ Achtung: Mindestbestand unterschritten!
```

---

### `PrintInfo(message)`

Infotext in **Cyan** mit `ℹ`-Prefix:
```
ℹ Gesamt: 42 Artikel
```

---

### `PrintSectionHeader(title, color)`

Zeigt einen dynamisch breiten Abschnitts-Header in der angegebenen Farbe:
```
╔══════════════════════╗
║  Neues Gerät anlegen  ║
╚══════════════════════╝
```

**Parameter:**
- `title`: Anzuzeigender Text
- `color`: Rahmenfarbe (Standard: `ConsoleColor.Blue`)

---

### `GetInput(prompt) → string`

Zeigt eine Eingabeaufforderung und liest die Eingabe des Benutzers:
```
▶ Inventarnummer: _
```

Gibt den eingegebenen Text getrimmt zurück. Gibt `null` zurück wenn keine Eingabe erfolgt.

---

### `PressKeyToContinue()`

Zeigt den Hinweis `[Drücken Sie eine beliebige Taste zum Fortfahren...]` in DarkGray an und wartet auf einen Tastendruck (ohne Anzeige der Taste).

---

### `PrintSeparator()`

Druckt eine 80-Zeichen breite horizontale Trennlinie in DarkGray:
```
────────────────────────────────────────────────────────────────────────────────
```

---

### `PrintTableHeader(params string[] headers)`

Zeigt Tabellenspaltenüberschriften in Gray an, je Spalte mit 20-Zeichen-Breite, gefolgt von einer Trennlinie.

---

## Verwendung

`ConsoleHelper` wird von **allen** Managern und Commands verwendet. Es gibt keine direkten Konsolen-Ausgaben außerhalb dieser Klasse (außer in Ausnahmefällen für sehr spezifisches Rendering wie im `DashboardManager`).

---

## Abhängigkeiten

Keine — verwendet nur `System` und `Console`.
