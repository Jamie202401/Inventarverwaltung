# LogManager — Verschlüsseltes Logging

**Datei:** `Logmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class LogManager`

---

## Zweck

`LogManager` protokolliert alle relevanten Systemereignisse in einer verschlüsselten Log-Datei (`System_Log.enc`). Jeder Eintrag enthält Zeitstempel, Benutzer, Computername, IP-Adresse, Aktion und optionale Details. Die Verschlüsselung erfolgt über `EncryptionManager` mit AES-256.

---

## Konfiguration

| Feld | Wert | Beschreibung |
|---|---|---|
| `logFilePath` | `System_Log.enc` | Pfad zur verschlüsselten Log-Datei |
| `aktuellerBenutzer` | `"System"` (Standard) | Wird nach Anmeldung durch `SetAktuellerBenutzer()` gesetzt |

---

## Methoden

### `SetAktuellerBenutzer(benutzername)`

Setzt den aktiven Benutzernamen für alle nachfolgenden Log-Einträge. Wird nach erfolgreicher Anmeldung durch `AuthManager` aufgerufen.

---

### `InitializeLog()`

Initialisiert die Log-Datei beim Programmstart. Wenn `System_Log.enc` noch nicht existiert, wird ein Header mit Erstellungsdatum und Verschlüsselungshinweis angelegt und verschlüsselt gespeichert.

---

### `SchreibeLog(kategorie, aktion, details)` *(private)*

Interne Methode, die einen strukturierten Log-Eintrag erstellt und verschlüsselt an die Log-Datei anhängt. Jeder Eintrag enthält:

```
[dd.MM.yyyy HH:mm:ss] [KATEGORIE]
  ├─ Benutzer:    <name>
  ├─ Computer:    <hostname>
  ├─ IP-Adresse:  <ip>
  ├─ Aktion:      <aktion>
  └─ Details:     <details>  (optional)
```

---

### Öffentliche Log-Methoden

| Methode | Kategorie | Beschreibung |
|---|---|---|
| `LogAnmeldungErfolgreich(benutzer)` | ANMELDUNG | Erfolgreiche Benutzeranmeldung |
| `LogDatenGeladen(typ, anzahl)` | DATEN | Daten wurden aus Datei geladen |
| `LogDatenGespeichert(typ, details)` | DATEN | Daten wurden gespeichert |
| `LogWarnung(bereich, details)` | WARNUNG | Allgemeine Warnung |
| `LogMitarbeiterHinzugefuegt(vName, nName, abt)` | MITARBEITER | Neuer Mitarbeiter angelegt |
| `LogMitarbeiterAngezeigt(anzahl)` | MITARBEITER | Mitarbeiterliste angezeigt |
| `LogMitarbeiterDuplikat(vName, nName)` | DUPLIKAT | Doppelter Mitarbeiter erkannt |
| `LogInventarHinzugefuegt(invNr, name)` | INVENTAR | Neuer Artikel angelegt |
| `LogInventarAngezeigt(anzahl)` | INVENTAR | Inventarliste angezeigt |
| `LogBestandsaenderung(invNr, alt, neu)` | BESTAND | Bestandsänderung protokolliert |
| `LogBenutzerAngelegt(name, rolle)` | BENUTZER | Neuer Account erstellt |
| `LogExport(typ, pfad)` | EXPORT | Export-Vorgang protokolliert |
| `LogImport(typ, anzahl)` | IMPORT | Import-Vorgang protokolliert |

---

### `GetLocalIPAddress() → string` *(private)*

Ermittelt die lokale IP-Adresse des Computers für den Log-Eintrag. Gibt `"N/A"` zurück bei Fehler.

---

## Dateiformat

Die Datei `System_Log.enc` ist ein binär verschlüsseltes Byte-Array (AES-256-CBC). Sie kann nur über `EncryptionManager.ReadEncryptedFile()` oder `DecryptFileInPlace()` gelesen werden.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `EncryptionManager` | `AppendEncrypted()`, `EncryptString()` |
| `ConsoleHelper` | Fehlerausgaben |
