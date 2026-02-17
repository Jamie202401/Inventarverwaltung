# KIEngine — Künstliche Intelligenz Engine

**Datei:** `KIengine.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class KIEngine`

---

## Zweck

`KIEngine` ist eine vollständig lokal laufende KI-Engine, die Machine Learning, Predictive Analytics, Natural Language Processing und Anomalie-Erkennung für die Inventarverwaltung bereitstellt — ohne Internetverbindung.

---

## Feature-Übersicht

| Bereich | Features |
|---|---|
| 🧠 Machine Learning | Pattern Recognition, Behavioral Learning, Predictive Analytics, Anomaly Detection, Adaptive Kalibrierung |
| 🎯 Intelligente Vorschläge | Smart Auto-Complete, Context-Aware Recommendations, Multi-Factor Decision Making, Risk Assessment, Confidence Levels |
| 📊 Analytics & Insights | Trend-Analyse, Bestandsoptimierung, Kosten-Nutzen-Analyse, Wartungsvorhersagen, Statistische Auswertungen |
| 💬 NLP | Fuzzy String Matching, Semantic Analysis, Intent Recognition, Smart Search, Auto-Korrektur, Synonym-Datenbank |
| 🔬 Algorithmen | Levenshtein-Distanz, Running Average, Standard Deviation, Confidence Scoring, Multi-Factor Weighting |

---

## Interne Datenstrukturen

| Feld | Typ | Beschreibung |
|---|---|---|
| `geraeteZuAbteilung` | `Dict<string, List<string>>` | Lernbasis: Welche Geräte werden von welchen Abteilungen genutzt |
| `mitarbeiterZuGeraete` | `Dict<string, List<string>>` | Lernbasis: Mitarbeiter → Geräte-Zuordnungen |
| `kategorieHaeufigkeit` | `Dict<string, int>` | Wie häufig wurde jede Kategorie verwendet |
| `durchschnittPreise` | `Dict<string, decimal>` | Durchschnittspreise je Kategorie |
| `nutzungsMuster` | `Dict<string, int>` | Nutzungsmuster für Predictive Analytics |
| `aktionsHistorie` | `List<BenutzerAktion>` | Vollständige Benutzeraktions-Historie |
| `haeufigsteAktionen` | `Dict<string, int>` | Häufigkeitsranking der Aktionen |
| `erkannteAnomalien` | `List<Anomalie>` | Erkannte Datanomalie-Objekte |
| `tagesAktivitaet` | `Dict<DateTime, int>` | Aktivität pro Tag für Trend-Analyse |
| `kategorieTrends` | `Dict<string, TrendDaten>` | Trend-Entwicklung je Kategorie |
| `synonyme` | `Dict<string, List<string>>` | Synonym-Datenbank für NLP |
| `worthaeufigkeit` | `Dict<string, float>` | Worthäufigkeit für Relevanz-Scoring |

---

## Kern-Methoden

### `Initialisieren()`

Lädt und analysiert alle vorhandenen Inventar- und Mitarbeiterdaten. Befüllt alle internen Lern-Dictionaries. Muss vor der Verwendung anderer KI-Methoden aufgerufen werden.

---

### `SmartSuche(suchbegriff) → List<InvId>`

Führt eine intelligente Suche mit Fuzzy Matching (Levenshtein-Distanz), Synonym-Erkennung und Relevanz-Scoring durch. Gibt Ergebnisse nach Relevanz sortiert zurück.

---

### `SchlageKategorieVor(geraeteName) → string`

Schlägt eine Kategorie für ein Gerät vor, basierend auf Mustererkennung aus bisherigen Einträgen.

---

### `SchlagePreisVor(kategorie, hersteller) → decimal`

Berechnet einen Preisvorschlag auf Basis historischer Durchschnittswerte der Kategorie.

---

### `SchlageAbteilungVor(geraeteName) → string`

Schlägt eine Abteilung für ein Gerät vor, basierend auf Lernmustern aus bestehenden Zuordnungen.

---

### `ErkenneAnomalien() → List<Anomalie>`

Analysiert das Inventar auf Anomalien: ungewöhnliche Preise (> 2 Standardabweichungen), sehr hohe oder sehr niedrige Bestände, verdächtige Einträge.

---

### `AnalysiereTrends() → List<TrendInfo>`

Berechnet Wachstums- und Abnahmemuster je Kategorie basierend auf zeitlichen Mustern in der Aktionshistorie.

---

### `BerechneLevenshtein(s, t) → int` *(private)*

Berechnet die Levenshtein-Distanz zwischen zwei Strings für das Fuzzy Matching.

---

### `ZeigeKIInsights()`

Zeigt eine vollständige KI-Analyse auf der Konsole: Top-Kategorien, Preistrends, Anomalie-Bericht, Verhaltensanalyse und Vorhersagen.

---

## Interne Typen

### `BenutzerAktion`

| Eigenschaft | Beschreibung |
|---|---|
| `Zeitstempel` | Wann wurde die Aktion ausgeführt |
| `AktionsTyp` | Art der Aktion (z.B. "InventarHinzugefuegt") |
| `Details` | Optionale Zusatzinformationen |

### `Anomalie`

| Eigenschaft | Beschreibung |
|---|---|
| `Artikel` | Betroffener InvId |
| `Typ` | Art der Anomalie |
| `Schwere` | Schweregrad (Low / Medium / High) |
| `Beschreibung` | Erklärung der Anomalie |

### `TrendDaten`

| Eigenschaft | Beschreibung |
|---|---|
| `AnzahlEintraege` | Gesamteinträge in dieser Kategorie |
| `WachstumsRate` | Prozentuale Wachstumsrate |
| `LetzteAktualisierung` | Zeitpunkt der letzten Änderung |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Zugriff auf Inventar und Mitarbeiter |
| `ConsoleHelper` | Ausgabe der KI-Insights |
