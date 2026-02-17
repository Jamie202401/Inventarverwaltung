# DeleteManager — Löschoperationen

**Datei:** `DeleteManager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class DeleteManager`

---

## Zweck

`DeleteManager` verwaltet das permanente Löschen von Datensätzen aus dem System. Er stellt sicher, dass vor jeder Löschoperation eine explizite Bestätigung eingeholt wird, da gelöschte Daten nicht wiederhergestellt werden können.

---

## Methoden

### `ZeigeLöschMenu()`

Zeigt das interaktive Lösch-Hauptmenü mit Warnhinweis:

```
⚠️  ACHTUNG: Gelöschte Daten können NICHT wiederhergestellt werden!
```

| Auswahl | Aktion |
|---|---|
| `[1]` | Artikel löschen |
| `[2]` | Mitarbeiter löschen |
| `[3]` | Benutzer löschen |
| `[0]` | Zurück zum Hauptmenü |

---

### `LöscheArtikel()`

Löschworkflow für Inventarartikel:

1. Anzeige aller Artikel mit Nummern
2. Benutzer wählt Artikel per Inventarnummer oder Listenindex
3. Vollständige Detailanzeige des ausgewählten Artikels
4. Explizite Bestätigungsabfrage: `Wirklich löschen? (j/n)`
5. Nur bei `j`: Artikel wird aus `DataManager.Inventar` entfernt
6. `DataManager.SaveKomplettesInventar()` wird aufgerufen
7. Log-Eintrag

---

### `LöscheMitarbeiter()`

Löschworkflow für Mitarbeiter:

1. Anzeige aller Mitarbeiter
2. Auswahl per Name oder Listennummer
3. Warnung falls dem Mitarbeiter noch Inventarartikel zugeordnet sind
4. Explizite Bestätigung
5. Nur bei `j`: Mitarbeiter wird aus `DataManager.Mitarbeiter` entfernt
6. `DataManager.SaveMitarbeiterToFile()` wird aufgerufen

---

### `LöscheBenutzer()`

Löschworkflow für Benutzer-Accounts:

1. Anzeige aller Benutzer
2. Auswahl per Benutzername oder Listennummer
3. Schutz: Der aktuell eingeloggte Benutzer kann sich **nicht** selbst löschen
4. Schutz: Der letzte verbleibende Admin-Account kann **nicht** gelöscht werden
5. Explizite Bestätigung
6. Nur bei `j`: Account wird aus `DataManager.Benutzer` entfernt
7. `DataManager.SaveBenutzerToFile()` wird aufgerufen

---

## Sicherheitsmechanismen

| Schutz | Betrifft | Beschreibung |
|---|---|---|
| Bestätigungsabfrage | Alle | Muss `j` eingeben um zu löschen |
| Referenzwarnung | Mitarbeiter | Warnung bei zugeordneten Artikeln |
| Selbstschutz | Benutzer | Eingeloggter Benutzer kann sich nicht selbst löschen |
| Admin-Schutz | Benutzer | Letzter Admin kann nicht gelöscht werden |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Datenlisten und alle Save-Methoden |
| `AuthManager` | `AktuellerBenutzer` für Selbstschutz |
| `LogManager` | Logging aller Löschoperationen |
| `ConsoleHelper` | Eingaben, Warn- und Fehlermeldungen |
