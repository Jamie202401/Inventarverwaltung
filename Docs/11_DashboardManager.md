# DashboardManager — System-Dashboard

**Datei:** `Dashboardmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class DashboardManager`

---

## Zweck

`DashboardManager` rendert ein interaktives, animiertes System-Dashboard direkt in der Konsole. Es zeigt Statistikkacheln, Bestandsübersichten, Such- und Filteroptionen sowie eine Artikelliste im Karten-Layout an. Das Design ist an moderne Web-Dashboards (Vercel, Tailwind UI) angelehnt.

---

## Zustandsfelder (private)

| Feld | Standardwert | Beschreibung |
|---|---|---|
| `suchbegriff` | `""` | Aktueller Suchtext |
| `aktuellerKategorieFilter` | `"Alle"` | Aktiv gesetzter Kategoriefilter |
| `aktuelleSortierung` | `"invNmr"` | Aktuelles Sortierfeld |
| `sortierungAufsteigend` | `true` | Sortierrichtung |

---

## Methoden

### `ZeigeDashboard()`

Haupteinstiegspunkt. Zeigt eine Ladeanimation und startet die interaktive Dashboard-Schleife. Verarbeitet folgende Eingaben:

| Eingabe | Aktion |
|---|---|
| `0`, `exit`, `q` | Dashboard schließen |
| `suche`, `s` | Suchbegriff eingeben |
| `filter`, `f` | Kategorie-Filter wählen |
| `sort` | Sortierung ändern |
| `r`, `reload` | Dashboard neu laden |
| `export`, `e` | Export-Menü öffnen |
| `hilfe`, `h` | Hilfe anzeigen |

---

### `ZeigeLadeAnimation()` *(private)*

Zeigt eine kurze animierte Ladeanzeige mit Fortschrittsbalken beim ersten Öffnen des Dashboards.

---

### `ZeigeExitAnimation()` *(private)*

Zeigt eine kurze Abschlussanimation beim Verlassen des Dashboards.

---

### `ZeigePremiumHeader()` *(private)*

Rendert den Dashboard-Kopfbereich mit aktuellem Benutzernamen, Datum, Uhrzeit und animierten Designelementen.

---

### `ZeigeStatistikKachelnPremium()` *(private)*

Zeigt vier Statistikkacheln nebeneinander:

- **Gesamt Artikel** — Gesamtanzahl aller Inventareinträge
- **Lager-OK** — Artikel mit ausreichendem Bestand
- **Niedrig** — Artikel unter dem Mindestbestand
- **Leer** — Artikel mit Bestand = 0

---

### `ZeigeSuchUndFilterLeistePremium()` *(private)*

Rendert die Such- und Filterleiste mit aktivem Suchbegriff und Kategoriefilter.

---

### `ZeigeArtikelKartenPremium()` *(private)*

Zeigt die gefilterte und sortierte Artikelliste als Karten mit Bestandsstatus-Farbcodierung.

---

### `ZeigePremiumFusszeile()` *(private)*

Rendert die Fußzeile mit den verfügbaren Tastaturkürzeln.

---

## Farbpalette

| Farbe | Verwendung |
|---|---|
| `DarkBlue` → `Blue` → `Cyan` → `White` | Gradient-Effekte im Header |
| `Green` | Bestand OK |
| `Yellow` | Niedriger Bestand |
| `Red` | Leerer Bestand |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Inventar`-Liste, `GetBestandsStatistik()` |
| `ExportManager` | Export-Menü |
| `ConsoleHelper` | Eingaben und Ausgaben |
| `InvId` | Artikeldaten mit `GetBestandsStatus()` |
