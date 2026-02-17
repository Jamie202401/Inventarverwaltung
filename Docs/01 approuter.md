# AppRouter — Navigations-Kern

**Datei:** `AppRouter.cs`  
**Namespace:** `Inventarverwaltung.Core`  
**Typ:** `public class AppRouter`

---

## Zweck

Der `AppRouter` ist das Herzstück der gesamten Menünavigation. Er verwaltet alle registrierten Menügruppen, zeigt das Hauptmenü sowie Untermenüs an und ruft bei einer Auswahl die entsprechende `Execute()`-Methode des zugehörigen Commands auf.

> `Program.cs` tut nur noch: `AppSetup.Build().Run()`

---

## Felder

| Feld | Typ | Beschreibung |
|---|---|---|
| `_groups` | `List<MenuGroup>` | Interne Liste aller registrierten Menügruppen (private) |

---

## Methoden

### `Register(MenuGroup group) → AppRouter`

Registriert eine neue Menügruppe im Router. Die Reihenfolge der Aufrufe bestimmt die spätere Anzeigereihenfolge im Hauptmenü. Gibt `this` zurück, sodass mehrere Aufrufe verkettet werden können.

```csharp
router.Register(new MenuGroup("1", "📊", "Dashboard & KI", ...))
      .Register(new MenuGroup("2", "📦", "Inventar", ...));
```

---

### `Run()`

Startet die Hauptschleife der Anwendung. Zeigt das Hauptmenü und wartet auf Benutzereingabe. Die Schleife läuft, bis der Benutzer `0` eingibt. Bei einer gültigen Gruppenauswahl wird `RunGruppe()` aufgerufen.

---

### `RunGruppe(MenuGroup gruppe)` *(private)*

Zeigt das Untermenü der gewählten Gruppe und führt bei einer gültigen Auswahl `Execute()` des entsprechenden Commands aus. `0` kehrt zum Hauptmenü zurück.

---

## Erweiterung

Neue Menüpunkte werden **nicht** hier eingetragen, sondern in `AppSetup.cs`. Der Router muss dafür nicht angefasst werden.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `MenuGroup` | Datenstruktur für eine Gruppe mit Commands |
| `UI` | Rendert Haupt- und Untermenüs |
| `ICommand` | Schnittstelle aller ausführbaren Aktionen |