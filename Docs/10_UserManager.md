# UserManager — Benutzerverwaltung

**Datei:** `Usermanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class UserManager`

---

## Zweck

`UserManager` verwaltet die Benutzer-Accounts der Anwendung. Er ermöglicht das Anlegen neuer Benutzer sowie das Aktualisieren von Berechtigungen bestehender Accounts. Die Benutzerverwaltung unterstützt zwei Rollen: `User` und `Admin`.

---

## Methoden

### `NeuerBenutzer()`

Erstellt einen neuen Benutzer oder aktualisiert die Berechtigung eines bereits vorhandenen Accounts:

**Workflow:**

1. **Benutzername** — Pflichtfeld, mindestens 3 Zeichen
2. Prüfung ob Benutzer bereits existiert (case-insensitiv):
   - **Bereits vorhanden:** Zeigt aktuellen Account an und fragt, ob die Berechtigung geändert werden soll
     - `j`: Neues Rollen-Menü wird angezeigt → Berechtigung wird aktualisiert und gespeichert
     - `n`: Abbruch
   - **Neu:** Weiter zu Schritt 3
3. **Berechtigung wählen:**
   - `[1]` 👤 User
   - `[2]` 👑 Admin
4. Neuen Account in `DataManager.Benutzer` eintragen
5. `DataManager.SaveBenutzerToFile()` aufrufen
6. Log-Eintrag via `LogManager.LogBenutzerAngelegt()`

---

### `ZeigeBenutzer()`

Zeigt alle vorhandenen Benutzer-Accounts in einer Tabelle an:

```
Nr   Benutzername         Berechtigung
──── ──────────────────── ──────────────
1    admin                👑 Admin
2    max.mustermann       👤 User
```

Bei leerer Liste: Warnmeldung und Rückkehr.  
Am Ende: Gesamtanzahl und Log-Eintrag.

---

## Validierungsregeln

| Feld | Regel |
|---|---|
| Benutzername | Pflichtfeld, min. 3 Zeichen |
| Berechtigung | Auswahl aus `[1] User` oder `[2] Admin` |

---

## Besonderheit: Bestehende Benutzer

Wenn ein bereits vorhandener Benutzername eingegeben wird, fragt das System explizit nach, ob die Berechtigung geändert werden soll. So können Rollen-Upgrades (z.B. User → Admin) direkt über diese Funktion durchgeführt werden, ohne einen separaten Bearbeitungsworkflow.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Benutzer`-Liste, `SaveBenutzerToFile()` |
| `LogManager` | Logging von Anlegen und Anzeigen |
| `ConsoleHelper` | Eingabemasken, Meldungen |
| `Accounts` | Datenmodell Benutzer |
| `Berechtigungen` | Enum für Rollen |
