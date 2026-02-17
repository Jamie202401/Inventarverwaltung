# LowStockWarning — Bestandswarnungen

**Datei:** `LowStockWarning.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class LowStockWarning`

---

## Zweck

`LowStockWarning` prüft beim Programmstart alle Inventarartikel auf niedrige oder leere Bestände und zeigt ein formatiertes Pop-up-Fenster in der Konsole an. Artikel, deren aktueller Bestand den Mindestbestand erreicht oder unterschreitet, werden als Warnungen gemeldet.

---

## Methoden

### `ZeigeBestandswarnungen()`

Hauptmethode — muss nach `DataManager.LoadInventar()` aufgerufen werden.

**Ablauf:**

1. Filtert alle Artikel aus `DataManager.Inventar` wo `Anzahl <= Mindestbestand`
2. Sortiert nach Anzahl (aufsteigend) und dann alphabetisch nach Gerätename
3. Wenn keine Warnungen: `ZeigeKeineWarnungen()` wird aufgerufen
4. Wenn Warnungen vorhanden: `ZeigeWarnungsPopup(niedrigerBestand)` wird aufgerufen
5. Wartet auf Tastendruck via `ConsoleHelper.PressKeyToContinue()`

---

### `ZeigeKeineWarnungen()` *(private)*

Zeigt ein grünes Pop-up-Fenster mit dem Text:

```
✅ ALLE BESTÄNDE IN ORDNUNG
Kein Artikel hat den Mindestbestand erreicht.
```

---

### `ZeigeWarnungsPopup(niedrigerBestand)` *(private)*

Zeigt ein farbiges Warnungs-Pop-up mit einer Tabelle aller betroffenen Artikel:

| Spalte | Inhalt |
|---|---|
| Inv-Nr | Inventarnummer |
| Gerät | Gerätename |
| Bestand | Aktuelle Anzahl |
| Minimum | Mindestbestand |
| Status | 🔴 LEER oder 🟡 NIEDRIG |

Artikel mit `Anzahl == 0` werden in Rot hervorgehoben, Artikel mit niedrigem Bestand in Gelb.

Am Ende der Anzeige: Gesamtanzahl Warnungen und Empfehlung zum Nachbestellen.

---

## Warnungs-Kriterien

| Bedingung | Status | Farbe |
|---|---|---|
| `Anzahl == 0` | 🔴 LEER | Rot |
| `0 < Anzahl <= Mindestbestand` | 🟡 NIEDRIG | Gelb |

---

## Aufruf-Zeitpunkt

`ZeigeBestandswarnungen()` wird in `Program.cs` direkt nach dem Laden aller Daten aufgerufen, damit der Benutzer sofort beim Start über kritische Bestände informiert wird.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Inventar`-Liste |
| `ConsoleHelper` | `PressKeyToContinue()` |
| `InvId` | Artikeldaten und `GetBestandsStatus()` |
