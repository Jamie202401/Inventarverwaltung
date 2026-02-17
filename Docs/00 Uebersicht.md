# Inventarverwaltung — Dokumentationsübersicht

**Projekt:** KI-gestützte Inventarverwaltung  
**Technologie:** Aktuell -> C# / .NET (Konsolenanwendung)  
				 Geplant -> Datenbankanwendung / Umbau auf webanwendung
**Verschlüsselung:** AES-256  
**Stand:** Februar 2026

---

## Architektur-Überblick

```
Program.cs
  └─ LoadingScreen.Show()          → Daten laden, KI initialisieren
  └─ AuthManager.Anmeldung()       → Benutzer anmelden
  └─ AppSetup.Build().Run()        → Menü aufbauen und starten
        └─ AppRouter               → Navigation (Haupt- & Untermenü)
              └─ MenuGroup         → Menügruppen mit Commands
                    └─ ICommand    → Aktion ausführen (→ Manager)
```

---

## Dokumentations-Index

| Datei | Klasse | Beschreibung |
|---|---|---|
| `01_AppRouter.md` | `AppRouter` | Navigations-Kern, Hauptschleife |
| `02_AppSetup.md` | `AppSetup` | Menükonfiguration, Gruppen & Commands |
| `03_AuthManager.md` | `AuthManager` | Benutzeranmeldung |
| `04_DataManager.md` | `DataManager` | Zentrale Datenverwaltung (Laden/Speichern) |
| `05_Models.md` | `InvId`, `MID`, `Accounts`, Enums | Alle Datenmodelle |
| `06_EncryptionManager.md` | `EncryptionManager` | AES-256 Ver-/Entschlüsselung |
| `07_LogManager.md` | `LogManager` | Verschlüsseltes Logging |
| `08_FileManager.md` | `FileManager` | Dateipfade & Datei-Attribute |
| `09_EmployeeManager.md` | `EmployeeManager` | Mitarbeiterverwaltung |
| `10_UserManager.md` | `UserManager` | Benutzerverwaltung |
| `11_DashboardManager.md` | `DashboardManager` | Interaktives System-Dashboard |
| `12_ExportManager.md` | `ExportManager` | Datenexport |
| `13_CSVImportManager.md` | `CSVImportManager` | CSV-Import |
| `14_KIEngine.md` | `KIEngine` | KI-Engine 2.0 (ML, NLP, Analytics) |
| `15_LowStockWarning.md` | `LowStockWarning` | Bestandswarnungen beim Start |
| `16_SchnellerfassungsManager.md` | `SchnellerfassungsManager` | Schnellerfassung |
| `17_InventoryManager.md` | `InventoryManager` | Inventarverwaltung (CRUD) |
| `18_EditManager.md` | `Editmanager` | Bearbeitung von Datensätzen |
| `19_DeleteManager.md` | `DeleteManager` | Löschoperationen |
| `20_HardwarePrintManager.md` | `HardwarePrintManager` | Hardware-Ausgabe-Druck & Historie |
| `21_IntelligentAssistant.md` | `IntelligentAssistant` | Eingabe-Assistent mit KI-Vorschlägen |
| `22_KIDashboard.md` | `KIDashboard` | KI Control Center |
| `23_EmailManager.md` | `EmailManager` | E-Mail-Benachrichtigungen |
| `24_ConsoleHelper.md` | `ConsoleHelper` | Konsolen-Hilfsfunktionen |
| `25_LoadingScreen.md` | `LoadingScreen` | Animierter Ladebildschirm |
| `26_Program.md` | `Program` | Einstiegspunkt (Main) |

---

## Datendateien

| Datei | Format | Inhalt |
|---|---|---|
| `Inventar.txt` | Strukturiertes Textformat (`;` getrennt) | Inventarartikel |
| `Mitarbeiter.txt` | Strukturiertes Textformat | Mitarbeiter |
| `Accounts.txt` | Strukturiertes Textformat | Benutzer-Accounts |
| `System_Log.enc` | AES-256-verschlüsselt (Binär) | System-Log |
| `DruckHistorie.json` | JSON (UTF-8) | Druckhistorie |
| `KI_Config.dat` | Binär / JSON | KI-Konfiguration |
| `KI_Stats.dat` | Binär / JSON | KI-Nutzungsstatistiken |
| `Templates/artikel_templates.txt` | Textformat | Schnellerfassungs-Templates |

---

## Schichtenmodell

```
┌──────────────────────────────────────────────────────────┐
│  UI-Schicht                                               │
│  Program.cs · LoadingScreen · ConsoleHelper · UI.cs      │
├──────────────────────────────────────────────────────────┤
│  Navigations-Schicht                                      │
│  AppRouter · AppSetup · MenuGroup · ICommand             │
├──────────────────────────────────────────────────────────┤
│  Fachlogik-Schicht (Manager)                             │
│  InventoryManager · EmployeeManager · UserManager        │
│  DashboardManager · SchnellerfassungsManager             │
│  EditManager · DeleteManager · HardwarePrintManager      │
│  ExportManager · CSVImportManager                        │
├──────────────────────────────────────────────────────────┤
│  KI-Schicht                                              │
│  KIEngine · IntelligentAssistant · KIDashboard          │
├──────────────────────────────────────────────────────────┤
│  Infrastruktur-Schicht                                   │
│  DataManager · FileManager · AuthManager                 │
│  LogManager · EncryptionManager · EmailManager          │
│  LowStockWarning                                         │
└──────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────────────┐
│  Datei-System                                             │
│  Inventar.txt · Mitarbeiter.txt · Accounts.txt           │
│  System_Log.enc · DruckHistorie.json · KI_*.dat          │
└──────────────────────────────────────────────────────────┘
```

---

## Erweiterungshinweis

Um einen neuen Menüpunkt hinzuzufügen, sind genau **2 Schritte** nötig:

1. Neue Command-Klasse erstellen (in `*Commands.cs`)
2. In `AppSetup.cs` registrieren

Kein weiterer Code ist notwendig. Der `AppRouter` verarbeitet alles automatisch.