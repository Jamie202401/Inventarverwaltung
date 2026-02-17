# Program — Einstiegspunkt

**Datei:** `Program.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `class Program`

---

## Zweck

`Program.cs` ist der Einstiegspunkt der Anwendung. Die Datei enthält absichtlich so wenig Logik wie möglich — der gesamte Aufbau ist in `AppSetup.cs`, `AppRouter.cs` und den Commands-Klassen gekapselt.

---

## Methoden

### `Main(string[] args)`

Vollständige Startsequenz in 5 Schritten:

```csharp
Console.OutputEncoding = System.Text.Encoding.UTF8;

LoadingScreen.Show();          // Ladebildschirm + alle Daten laden
AuthManager.Anmeldung();       // Benutzeranmeldung
ConsoleHelper.PrintWelcome();  // Willkommensbildschirm

AppSetup.Build().Run();        // Router aufbauen und Hauptschleife starten

LogManager.LogProgrammEnde();  // Beendigungs-Log
Verabschiedung();              // Abschluss-Bildschirm
```

**Reihenfolge ist zwingend:**
1. UTF-8 Encoding muss vor allen Ausgaben gesetzt werden (für Umlaute und Emojis)
2. `LoadingScreen.Show()` lädt alle Daten — muss vor `AuthManager` sein
3. `AuthManager.Anmeldung()` setzt `AktuellerBenutzer` — muss vor `AppSetup.Build().Run()` sein
4. `AppSetup.Build().Run()` blockt bis der Benutzer `[0]` drückt

---

### `Verabschiedung()` *(private)*

Zeigt den Abschluss-Bildschirm nach Beendigung der Hauptschleife:

```
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║     ✓  VIELEN DANK FÜR DIE NUTZUNG!                              ║
║                                                                   ║
║     📦 Inventarverwaltung  ·  🖨️  Hardware-Ausgabe                ║
║     🤖 KI-gestützt  ·  🔐 AES-256 verschlüsselt                   ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
```

Bleibt 2 Sekunden sichtbar, dann endet das Programm.

---

## Design-Prinzip

`Program.cs` folgt dem Prinzip der minimalen Einstiegspunkte:

> "Nur 5 Zeilen Logik — der Rest steckt in AppSetup.cs, Core/ und Commands/"

Neue Funktionen werden **nie** in `Program.cs` hinzugefügt, sondern in den entsprechenden Managern und Commands.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `LoadingScreen` | Daten laden und Ladebildschirm |
| `AuthManager` | Benutzeranmeldung |
| `ConsoleHelper` | Willkommen-Bildschirm |
| `AppSetup` | Router aufbauen |
| `LogManager` | Programm-Ende loggen |
