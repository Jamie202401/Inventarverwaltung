# Models — Datenmodelle

**Datei:** `Models.cs`  
**Namespace:** `Inventarverwaltung`

---

## Zweck

`Models.cs` enthält alle Datenklassen und Enumerationen der Anwendung. Diese Klassen bilden die Grundlage für alle Datenverwaltungs- und Anzeige-Operationen.

---

## Klasse: `InvId` — Inventarartikel

Repräsentiert einen einzelnen Inventarartikel mit vollständiger Bestandsführung und Tracking.

### Eigenschaften

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `InvNmr` | `string` | Inventar-Nummer (z.B. `INV001`) |
| `GeraeteName` | `string` | Gerätename (z.B. `Laptop Dell XPS`) |
| `MitarbeiterBezeichnung` | `string` | Zugewiesener Mitarbeiter |
| `SerienNummer` | `string` | Seriennummer (SNR) |
| `Preis` | `decimal` | Anschaffungspreis |
| `Anschaffungsdatum` | `DateTime` | Kaufdatum |
| `Hersteller` | `string` | Hersteller (z.B. Dell, HP) |
| `Kategorie` | `string` | Kategorie (z.B. IT, Büro) |
| `Anzahl` | `int` | Aktuelle Menge |
| `Mindestbestand` | `int` | Warnschwelle für Bestandsalarm |
| `ErstelltVon` | `string` | Benutzername der erstellenden Person |
| `ErstelltAm` | `DateTime` | Zeitstempel der Erstellung |

### Konstruktoren

| Konstruktor | Parameter | Verwendung |
|---|---|---|
| Vollständig (12 Parameter) | Alle Felder inkl. Tracking | Neue Artikel mit Benutzer-Tracking |
| Ohne Tracking (10 Parameter) | Alle Felder ohne `ErstelltVon`/`ErstelltAm` | Automatisch auf `"System"` / `DateTime.Now` gesetzt |
| Minimal (3 Parameter) | `invNmr`, `geraeteName`, `mitarbeiterBezeichnung` | Rückwärtskompatibilität mit alten Dateien |

### Methoden

#### `GetBestandsStatus() → BestandsStatus`

Berechnet den Bestandsstatus anhand von `Anzahl` und `Mindestbestand`:

| Bedingung | Status |
|---|---|
| `Anzahl == 0` | `Leer` |
| `Anzahl <= Mindestbestand` | `Niedrig` |
| `Anzahl <= Mindestbestand * 2` | `Mittel` |
| sonst | `Gut` |

#### `GetBestandsStatusText() → string`

Gibt den Status als formatierten Text mit Emoji zurück:

| Status | Ausgabe |
|---|---|
| Leer | `🔴 LEER` |
| Niedrig | `🟡 NIEDRIG` |
| Mittel | `🟢 OK` |
| Gut | `🟢 GUT` |

---

## Enum: `BestandsStatus`

| Wert | Bedeutung |
|---|---|
| `Leer` | 0 Stück vorhanden |
| `Niedrig` | Menge ≤ Mindestbestand |
| `Mittel` | Menge ≤ Mindestbestand × 2 |
| `Gut` | Ausreichend vorhanden |

---

## Klasse: `MID` — Mitarbeiter

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `VName` | `string` | Vorname |
| `NName` | `string` | Nachname |
| `Abteilung` | `string` | Abteilung |

---

## Enum: `Berechtigungen`

| Wert | Beschreibung |
|---|---|
| `User` | Normaler Benutzer |
| `Admin` | Administrator mit vollen Rechten |

---

## Klasse: `Accounts` — Benutzer-Account

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Benutzername` | `string` | Login-Name |
| `Berechtigung` | `Berechtigungen` | Rolle des Benutzers |

---

## Klasse: `Anmelder` — Legacy

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Anmeldename` | `string` | Name des Anmelders (Legacy-Unterstützung) |