# EmailManager — E-Mail-Benachrichtigungen

**Datei:** `Emailmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class EmailManager` (o.ä.)*

---

## Zweck

`EmailManager` verwaltet den automatischen E-Mail-Versand für Benachrichtigungen aus dem Inventarsystem, insbesondere Bestandswarnungen, Tagesberichte und Systembenachrichtigungen.

---

## Konfiguration

E-Mail-Einstellungen werden in der Anwendungskonfiguration oder direkt im Manager hinterlegt:

| Parameter | Beschreibung |
|---|---|
| SMTP-Server | Adresse des Mailservers |
| Port | SMTP-Port (Standard: 587 für TLS) |
| Absender | Absender-E-Mail-Adresse |
| Empfänger | Empfänger-Liste |
| SSL/TLS | Verschlüsselung der Verbindung |

---

## Methoden

### `SendeBestandswarnung(artikel)`

Versendet eine E-Mail-Benachrichtigung wenn Artikel den Mindestbestand unterschreiten. Enthält eine Tabelle aller betroffenen Artikel mit aktuellem Bestand und Mindestbestand.

---

### `SendeTagesbericht()`

Erstellt und versendet einen täglichen Zusammenfassungsbericht mit:
- Anzahl Inventareinträge gesamt
- Artikel mit niedrigem Bestand
- Heute durchgeführte Aktionen (aus Log)
- Neue Mitarbeiter und Artikel des Tages

---

### `SendeSystembenachrichtigung(betreff, nachricht)`

Allgemeine Methode zum Versenden von Systembenachrichtigungen. Kann für individuelle Ereignisse aufgerufen werden.

---

### `TesteEmailVerbindung() → bool`

Testet die SMTP-Verbindung und gibt `true` zurück wenn erfolgreich. Zeigt Ergebnis auf der Konsole an.

---

### `ZeigeEmailKonfiguration()`

Zeigt die aktuell hinterlegten E-Mail-Einstellungen an (ohne Passwort) und ermöglicht deren Anpassung.

---

## E-Mail-Formate

Alle E-Mails werden als HTML und/oder Plaintext versendet. Der Betreff enthält den Anwendungsnamen und den Typ der Benachrichtigung:

```
Betreff: [Inventarverwaltung] ⚠️ Bestandswarnung - 3 Artikel unter Mindestbestand
```

---

## Fehlerbehandlung

- Netzwerkfehler und SMTP-Fehler werden abgefangen und geloggt
- Bei Fehler wird eine Konsolenausgabe erzeugt, aber kein Programmabbruch
- Optionale erneute Versuche bei temporären Verbindungsfehlern

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Bestandsdaten für Berichte |
| `LogManager` | Logging von gesendeten Mails |
| `LowStockWarning` | Bestandsdaten für Warnmails |
| `System.Net.Mail` | SMTP-Client |
