# EmployeeManager — Mitarbeiterverwaltung

**Datei:** `Employeemanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class EmployeeManager`

---

## Zweck

`EmployeeManager` stellt alle Operationen rund um die Mitarbeiterverwaltung bereit. Die Eingaben werden durch KI-Unterstützung (`IntelligentAssistant`) validiert und mit Vorschlägen angereichert. Duplikaterkennung verhindert doppelte Einträge.

---

## Methoden

### `NeuenMitarbeiterHinzufuegen()`

Geführter Workflow zum Anlegen eines neuen Mitarbeiters:

1. **KI-Analyse** der aktuellen Abteilungsverteilung wird angezeigt
2. **Vorname** — Pflichtfeld, mindestens 2 Zeichen
3. **Nachname** — Pflichtfeld, mindestens 2 Zeichen
   - KI-Plausibilitätsprüfung auf ähnliche Namen
   - Duplikatprüfung (Vor- + Nachname kombiniert, case-insensitiv)
   - Bei Duplikat: Fehlermeldung + Log-Eintrag + erneute Eingabe
4. **Abteilung** — KI schlägt bis zu 5 häufig verwendete Abteilungen vor
   - Auswahl per Nummer möglich
   - Oder freie Texteingabe
5. Mitarbeiter wird in `DataManager.Mitarbeiter` eingetragen
6. `DataManager.SaveMitarbeiterToFile()` wird aufgerufen
7. `IntelligentAssistant.IniializeAI()` wird aufgerufen (KI aktualisiert)
8. Log-Eintrag via `LogManager.LogMitarbeiterHinzugefuegt()`

---

### `ZeigeMitarbeiter()`

Zeigt alle Mitarbeiter in einer ausgerichteten Tabelle mit Spaltenüberschriften:

```
Nr   Vorname              Nachname             Abteilung
──── ──────────────────── ──────────────────── ────────────────────
1    Max                  Mustermann           IT
2    Anna                 Beispiel             Buchhaltung
```

Bei leerer Liste: Warnmeldung und Rückkehr.  
Am Ende: Gesamtanzahl der Mitarbeiter und Log-Eintrag.

---

## Validierungsregeln

| Feld | Regel |
|---|---|
| Vorname | Pflichtfeld, min. 2 Zeichen |
| Nachname | Pflichtfeld, min. 2 Zeichen, kein Duplikat |
| Abteilung | Pflichtfeld, beliebige Länge |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Mitarbeiter`-Liste, `SaveMitarbeiterToFile()` |
| `IntelligentAssistant` | Abteilungsanalyse, Namensplausibilität, Vorschläge |
| `LogManager` | Logging von Hinzufügen, Anzeigen, Duplikaten |
| `ConsoleHelper` | Eingabemasken, Erfolgs-/Fehlermeldungen |
| `MID` | Datenmodell Mitarbeiter |
