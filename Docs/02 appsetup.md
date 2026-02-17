# AppSetup — Menükonfiguration

**Datei:** `AppSetup.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class AppSetup`

---

## Zweck

`AppSetup` ist die zentrale **Entwickler-Konfigurationsdatei** für den Menüaufbau. Hier werden alle Menügruppen und die dazugehörigen Commands registriert. Jede Änderung am Menü — neuer Menüpunkt, neue Gruppe — findet ausschließlich hier statt.

---

## Methoden

### `Build() → AppRouter`

Baut den kompletten `AppRouter` auf und gibt ihn zurück. Erstellt alle Menügruppen mit ihren jeweiligen Commands und registriert sie im Router.

**Rückgabewert:** Fertig konfigurierter `AppRouter`

---

## Menüstruktur

| Nr. | Icon | Gruppe | Farbe | Enthaltene Commands |
|---|---|---|---|---|
| 1 | 📊 | Dashboard & KI | Cyan | SystemDashboard, KIDashboard, KIEngineInsights, KIEngineInit |
| 2 | 📦 | Inventar | White | InventarNeu, InventarZeige, InventarDetail |
| 3 | ⚡ | Schnellerfassung | Yellow | Schnellerfassung |
| 4 | 👥 | Mitarbeiter | Cyan | MitarbeiterNeu, MitarbeiterZeige, BenutzerNeu, BenutzerZeige |
| 5 | 📋 | Bestandspflege | Green | BestandErhoehen, BestandVerringern, Mindestbestand, BestandWarnung |
| 6 | 🖨️ | Hardware-Druck | Magenta | DruckNeu, DruckHistorie, DruckSuche, DruckEdit, DruckKonfig |
| 7 | 🔧 | Werkzeuge | DarkYellow | Import, Bearbeitung, Löschung |
| 8 | ⚙️ | System | DarkGray | SystemLog, Tagesreport, Verschlüsselung |

---

## Neuen Menüpunkt hinzufügen (2 Schritte)

**Schritt 1** — Neue Command-Klasse in `Commands/[X]Commands.cs`:

```csharp
public class MeinCommand : ICommand
{
    public string Key   => "MEIN_KEY";
    public string Label => "Meine Aktion";
    public string Icon  => "🆕";
    public void Execute() => MeinManager.MeineMethode();
}
```

**Schritt 2** — In `AppSetup.cs` in der passenden Gruppe eintragen:

```csharp
.Add(new MeinCommand())
```

→ Kein weiterer Code notwendig.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `AppRouter` | Wird aufgebaut und zurückgegeben |
| `MenuGroup` | Datenstruktur für Gruppeneinträge |
| Alle `*Commands.cs` | Commands werden hier instanziiert |