# FileManager — Dateipfade & Datei-Attribute

**Datei:** `Filemanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class FileManager`

---

## Zweck

`FileManager` verwaltet alle Dateipfade der Anwendung und stellt Hilfsmethoden bereit, um Datendateien korrekt anzulegen und störende Datei-Attribute (`Hidden`, `ReadOnly`, `System`) zu entfernen, die Schreiboperationen verhindern könnten.

---

## Dateipfade

| Feld | Wert | Inhalt |
|---|---|---|
| `FilePath` | `Inventar.txt` | Inventar-Daten |
| `FilePath2` | `Mitarbeiter.txt` | Mitarbeiter-Daten |
| `FilePath3` | `Accounts.txt` | Benutzer-Accounts |

Alle Pfade sind relativ zum aktuellen Arbeitsverzeichnis.

---

## Methoden

### `InitializeFiles()`

Erstellt alle drei Programmdateien, falls sie noch nicht existieren. Entfernt anschließend problematische Datei-Attribute (`Hidden`, `ReadOnly`). Muss beim Programmstart aufgerufen werden, bevor `DataManager.Load*()` verwendet wird.

---

### `HideAllFiles()` *(Legacy)*

Wird nicht mehr für das Verstecken von Dateien verwendet. Ruft intern nur noch `InitializeFiles()` auf. Die Methode bleibt aus Kompatibilitätsgründen erhalten.

---

### `FixFileAttributes()`

Repariert die Datei-Attribute aller drei Programmdateien auf einmal. Entfernt `Hidden`, `ReadOnly` und `System`-Attribute. Nützlich nach manuellen Dateioperationen oder bei Zugriffsfehlern.

---

### `CreateFile(path)` *(private)*

Erstellt eine einzelne Datei falls noch nicht vorhanden, und bereinigt danach deren Attribute.

---

### `FixFileAttribute(path)` *(private)*

Entfernt `Hidden`, `ReadOnly` und `System`-Attribute einer einzelnen Datei. Fehler werden ignoriert (z.B. wenn die Datei gerade geöffnet ist).

---

## Hinweis

Frühere Versionen versteckten Datendateien im Dateisystem (`FileAttributes.Hidden`), was auf manchen Systemen zu `Access Denied`-Fehlern beim Schreiben führte. Dieses Verhalten wurde entfernt. Dateien werden nun offen und ohne Sonderattribute gespeichert.

---

## Abhängigkeiten

Keine externen Abhängigkeiten. Verwendet nur `System.IO`.
