# KIDashboard — KI Control Center

**Datei:** `Kidashboard.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class KIDashboard`

---

## Zweck

`KIDashboard` ist das interaktive Steuerungszentrum für alle KI-Funktionen der Anwendung. Es ermöglicht die Konfiguration des KI-Verhaltens, die Anzeige von Statistiken, die Aktivierung von KI-Modi und das Zurücksetzen der Lernhistorie. Einstellungen und Statistiken werden persistent gespeichert.

---

## Persistenz

| Datei | Beschreibung |
|---|---|
| `KI_Config.dat` | KI-Konfigurationseinstellungen |
| `KI_Stats.dat` | Gesammelte KI-Nutzungsstatistiken |

---

## Farbschema

| Variable | Farbe | Verwendung |
|---|---|---|
| `CP` | Cyan | Primärfarbe, Überschriften |
| `CA` | Yellow | Aktive Elemente, Warnungen |
| `CS` | Green | Erfolgsmeldungen |
| `CW` | Yellow | Warnungen |
| `CE` | Red | Fehler |
| `CD` | DarkGray | Sekundärtext |
| `CX` | White | Standardtext |
| `CM` | Magenta | Hervorhebungen |

---

## Methoden

### `ZeigeKIControlCenter()`

Haupteinstiegspunkt. Lädt Konfiguration und Statistiken, zeigt die Boot-Animation und startet die interaktive Schleife:

| Auswahl | Menü |
|---|---|
| `[1]` | KI-Einstellungen |
| `[2]` | Betriebsmodus wählen |
| `[3]` | KI-Funktionen aktivieren/deaktivieren |
| `[4]` | Detaillierte Statistiken |
| `[5]` | KI-Insights anzeigen |
| `[6]` | Statistiken zurücksetzen |
| `[0]` | Zurück |

---

### `AnimBoot()` *(private)*

Zeigt eine animierte Boot-Sequenz beim Öffnen des KI Control Centers mit progressiver Textanzeige und Ladebalken.

---

### `MenuEinstellungen()` *(private)*

Konfigurationsmenü für KI-Parameter:
- Lernrate anpassen
- Konfidenz-Schwellenwert setzen
- Vorschlagsanzahl konfigurieren
- Anomalie-Empfindlichkeit einstellen

---

### `MenuModus()` *(private)*

Auswahl des KI-Betriebsmodus:

| Modus | Beschreibung |
|---|---|
| Konservativ | Nur sichere, häufig bestätigte Vorschläge |
| Standard | Ausgewogenes Verhältnis |
| Aggressiv | Viele Vorschläge, auch unsichere |
| Lernmodus | Aktiv lernen, aber keine Vorschläge ausgeben |

---

### `MenuFunktionen()` *(private)*

Einzelne KI-Funktionen ein- und ausschalten:
- Auto-Vervollständigung
- Preisvorschläge
- Kategorieerkennung
- Anomalie-Detektion
- Trend-Analyse

---

### `MenuDetailStats()` *(private)*

Zeigt detaillierte Nutzungsstatistiken:
- Anzahl gegebener Vorschläge gesamt
- Angenommene vs. abgelehnte Vorschläge
- Genauigkeit je Vorschlagstyp (in %)
- Aktivste KI-Nutzungszeiten

---

### `MenuInsights()` *(private)*

Ruft `KIEngine.ZeigeKIInsights()` auf und zeigt die vollständige KI-Analyse.

---

### `StatsReset()` *(private)*

Setzt alle gespeicherten Statistiken zurück nach einer Bestätigungsabfrage. Konfiguration bleibt erhalten.

---

### `LadeAlles()` *(private)*

Lädt `KIConfig` aus `KI_Config.dat` und `KIStatistiken` aus `KI_Stats.dat`. Erstellt Standardwerte wenn die Dateien nicht existieren.

---

### `SpeichereAlles()` *(private)*

Speichert aktuelle Konfiguration und Statistiken in die jeweiligen `.dat`-Dateien.

---

## Interne Typen

### `KIConfig`

Konfigurationsobjekt mit allen einstellbaren KI-Parametern.

### `KIStatistiken`

Statistikobjekt mit Zählern für Vorschläge, Annahmen, Ablehnungen und Genauigkeitswerten.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `KIEngine` | KI-Insights und Analysemethoden |
| `ConsoleHelper` | Alle Eingaben und Ausgaben |
