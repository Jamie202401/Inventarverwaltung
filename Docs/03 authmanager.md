# AuthManager — Benutzeranmeldung

**Datei:** `Authmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class AuthManager`

---

## Zweck

`AuthManager` verwaltet die Benutzeranmeldung. Er liest verfügbare Accounts aus der Datei `Accounts.txt` (via `DataManager`) und prüft die Eingabe des Benutzers. Bei fehlendem Admin-Account wird automatisch ein Standard-Admin erstellt.

---

## Eigenschaften

| Eigenschaft | Typ | Zugriff | Beschreibung |
|---|---|---|---|
| `AktuellerBenutzer` | `string` | `public get / private set` | Benutzername des aktuell angemeldeten Nutzers |

---

## Methoden

### `Anmeldung()`

Führt den kompletten Anmelde-Workflow durch:

1. Lädt alle Benutzer aus `Accounts.txt` via `DataManager.LoadBenutzer()`
2. Falls keine Benutzer vorhanden: erstellt automatisch einen `admin`-Account mit `Berechtigungen.Admin` und speichert ihn
3. Zeigt die Anmeldemaske in einer Schleife
4. Prüft die Eingabe case-insensitive gegen alle bekannten Accounts
5. Bei Erfolg: setzt `AktuellerBenutzer`, loggt die Anmeldung via `LogManager.LogAnmeldungErfolgreich()` und zeigt die Rolle an
6. Bei Fehler: zeigt eine Fehlermeldung mit Hinweis auf den Admin

---

## Verhalten

- Leere Benutzernamen werden abgewiesen
- Nicht existierende Benutzer lösen eine Fehlermeldung aus (kein Passwort erforderlich — Authentifizierung rein über den Account-Eintrag)
- Die Anmeldeschleife läuft bis zu einer erfolgreichen Anmeldung
- Nach erfolgreicher Anmeldung wird 1,5 Sekunden pausiert, bevor das Hauptmenü erscheint

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `LoadBenutzer()`, `SaveBenutzerToFile()`, `Benutzer`-Liste |
| `LogManager` | `LogAnmeldungErfolgreich()` |
| `Accounts` | Datenmodell für Benutzer |
| `Berechtigungen` | Enum für Rollen (User / Admin) |