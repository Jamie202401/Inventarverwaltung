# IntelligentAssistant — Intelligenter Assistent

**Datei:** `Intelligentassistant.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class IntelligentAssistant`

---

## Zweck

`IntelligentAssistant` ist der direkt in die Eingabe-Workflows integrierte KI-Helfer. Er analysiert vorhandene Daten und gibt kontextbezogene Vorschläge während der Dateneingabe aus. Er läuft vollständig lokal und benötigt keine Internetverbindung.

---

## Interne Lernstrukturen (private)

| Feld | Typ | Beschreibung |
|---|---|---|
| `geraeteZuAbteilung` | `Dict<string, List<string>>` | Gerätename → Abteilungen die dieses Gerät nutzen |
| `mitarbeiterZuGeraete` | `Dict<string, List<string>>` | Mitarbeiter → zugeordnete Geräte |
| `haeufigsteAbteilungen` | `Dict<string, int>` | Häufigkeit jeder Abteilung |

---

## Methoden

### `IniializeAI()`

Initialisiert den Assistenten und ruft `LerneAusVorhandenenDaten()` auf. Gibt nach Abschluss eine Erfolgsbestätigung auf der Konsole aus. Muss nach jeder Datenänderung erneut aufgerufen werden, damit die Lernbasis aktuell bleibt.

---

### `LerneAusVorhandenenDaten()` *(private)*

Iteriert über alle Einträge in `DataManager.Inventar` und `DataManager.Mitarbeiter` und befüllt die internen Dictionaries:
- Welche Geräte werden in welchen Abteilungen genutzt
- Welche Geräte hat ein bestimmter Mitarbeiter bereits
- Wie häufig kommt jede Abteilung vor

---

### `SchlageInventarnummernVor() → string`

Generiert einen Vorschlag für die nächste Inventarnummer. Analysiert bestehende Nummern, erkennt Muster (z.B. `INV001`, `INV002`) und schlägt die nächste logische Nummer vor.

---

### `SchlageAbteilungenVor() → List<string>`

Gibt eine nach Häufigkeit sortierte Liste der bisher verwendeten Abteilungen zurück. Maximal 5 Einträge. Wird in `EmployeeManager` für die Abteilungsauswahl verwendet.

---

### `AnalysiereAbteilungsverteilung() → string`

Gibt eine formatierte Textausgabe der aktuellen Abteilungsverteilung zurück (für die Anzeige bei `NeuenMitarbeiterHinzufuegen()`).

---

### `PruefeNamePlausibilitaet(vName, nName) → string`

Prüft ob ein ähnlicher Name bereits im System existiert (Fuzzy Matching). Gibt ein Feedback-Texte zurück wenn verdächtige Ähnlichkeiten gefunden werden, oder `null` wenn alles in Ordnung ist.

---

### `ZeigeSystemInsights()`

Zeigt beim Anlegen eines neuen Artikels eine kompakte KI-Auswertung:
- Häufigste Kategorien
- Durchschnittliche Preise je Kategorie
- Ungewöhnliche Geräte-Mitarbeiter-Kombinationen

---

### `SchlageKategorieVor(geraeteName) → string`

Gibt einen Kategorie-Vorschlag basierend auf dem Gerätenamen zurück. Nutzt Keyword-Matching und gelernte Muster.

---

### `SchlageHerstellerVor(geraeteName) → string`

Gibt einen Hersteller-Vorschlag basierend auf dem Gerätenamen zurück (z.B. "Dell" für "Latitude", "HP" für "ProBook").

---

### `SchlagePreisVor(kategorie) → decimal`

Berechnet einen Durchschnittspreis aus allen bestehenden Artikeln derselben Kategorie als Preisvorschlag.

---

## Integration in Eingabe-Workflows

Der Assistent ist direkt in folgende Manager integriert:

| Manager | Verwendete Methoden |
|---|---|
| `InventoryManager` | Inventarnummer, Kategorie, Hersteller, Preis, Mindestbestand |
| `EmployeeManager` | Abteilungsvorschläge, Namensplausibilität, Abteilungsverteilung |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Inventar`- und `Mitarbeiter`-Listen als Lernbasis |
| `ConsoleHelper` | Ausgabe der KI-Feedback-Texte |
