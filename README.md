# 📦 Inventarverwaltung

Eine moderne, konsolenbasierte Inventarverwaltung für Windows — mit KI-Unterstützung, Hardware-Druck und AES-256-Verschlüsselung. Komplett offline nutzbar.

---

## ✨ Features

- 🤖 **KI Engine 2.0** — lokale KI mit Pattern Recognition, NLP & Predictive Analytics
- ⚡ **Schnellerfassung** — Ultra-Schnell-Modus, CSV-Import & Template-System
- 🖨️ **Hardware-Druck** — Druckvorschau, Druckerauswahl & Druckhistorie
- ⚠️ **Bestandswarnungen** — automatisches Pop-up & E-Mail-Benachrichtigung bei kritischem Bestand
- 🔐 **Verschlüsselung** — AES-256 für alle sensiblen Daten
- 👥 **Benutzerverwaltung** — Rollen (Admin / User) mit Login-System
- 📊 **Dashboard** — Systemkennzahlen & KI-Insights auf einen Blick

---

## 🚀 Installation

### Voraussetzungen
- [.NET 8 SDK](https://dotnet.microsoft.com/download) oder höher
- Windows (für Druckfunktion empfohlen)

### Schritte

```bash
# Repository klonen
git clone https://github.com/dein-name/inventarverwaltung.git
cd inventarverwaltung

# Projekt bauen
dotnet build

# Programm starten
dotnet run
```

Beim ersten Start werden alle benötigten Datendateien automatisch erstellt.

---

## 📁 Projektstruktur

```
inventarverwaltung/
├── Program.cs                  # Einstiegspunkt
├── AppSetup.cs                 # Menükonfiguration
├── AppRouter.cs                # Routing-Logik
├── Commands/                   # Einzelne Menüaktionen (Command-Pattern)
├── KIengine.cs                 # KI Engine 2.0
├── Kidashboard.cs              # KI Control Center
├── Schnellerfassungsmanager.cs # Ultra-Schnell & CSV-Import
├── Hardwareprintermanager.cs   # Druckmodul
├── Exportmanager.cs            # Excel / PDF Export
├── Datamanager.cs              # Datenhaltung
└── Models.cs                   # Datenmodelle
```

---

## 📂 Datendateien

Die App legt folgende Dateien im Programmverzeichnis an:

| Datei | Inhalt |
|---|---|
| `Inventar.txt` | Alle Inventarartikel |
| `Mitarbeiter.txt` | Mitarbeiterliste |
| `Accounts.txt` | Benutzer & Rollen |
| `Log.txt` | Systemprotokoll |
| `KI_Config.dat` | KI-Konfiguration |
| `Templates/` | Schnellerfassungs-Vorlagen |
| `Exports/` | Exportierte Dateien |

> ⚠️ Vor Updates bitte `Inventar.txt`, `Mitarbeiter.txt` und `Accounts.txt` sichern.

---

## 🔧 Konfiguration

Der **E-Mail-Versand** für Bestandswarnungen muss in `Emailmanager.cs` einmalig konfiguriert werden:

```csharp
private static readonly string SMTP_SERVER   = "smtp.deinserver.de";
private static readonly int    SMTP_PORT     = 587;
private static readonly string SENDER_EMAIL  = "absender@firma.de";
private static readonly string SENDER_PASSWORD = "passwort";
private static readonly string RECIPIENT_EMAIL = "empfaenger@firma.de";
```

---

## 📄 Lizenz

MIT License — siehe [LICENSE](LICENSE)
