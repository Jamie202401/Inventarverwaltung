using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Inventarverwaltung
{
    /// <summary>
    /// Export-Manager für Datenexport als Excel oder PDF
    /// Unterstützt: Artikel, Mitarbeiter, Benutzer mit individueller Pfadwahl
    /// </summary>
    public static class ExportManager
    {
        private static readonly string StandardExportVerzeichnis = Path.Combine(Environment.CurrentDirectory, "Exports");

        /// <summary>
        /// Zeigt das Export-Hauptmenü
        /// </summary>
        public static void ZeigeExportMenu()
        {
            bool menuAktiv = true;

            while (menuAktiv)
            {
                Console.Clear();
                ZeigeExportHeader();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  ╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║                    📤 EXPORT-OPTIONEN                      ║");
                Console.WriteLine("  ╚════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  ┌─ Was möchten Sie exportieren? ──────────────────────────┐");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1] 📦 Artikel exportieren");
                Console.WriteLine("  [2] 👥 Mitarbeiter exportieren");
                Console.WriteLine("  [3] 👨‍💼 Benutzer exportieren");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [4] 📊 Vollständiger Export (Alle Daten)");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  [0] ← Zurück zum Hauptmenü");
                Console.ResetColor();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  └──────────────────────────────────────────────────────────┘");
                Console.ResetColor();

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  ▶ Ihre Auswahl: ");
                Console.ResetColor();

                string auswahl = Console.ReadLine()?.Trim();

                switch (auswahl)
                {
                    case "1":
                        ExportiereArtikel();
                        break;
                    case "2":
                        ExportiereMitarbeiter();
                        break;
                    case "3":
                        ExportiereBenutzer();
                        break;
                    case "4":
                        ExportiereAlles();
                        break;
                    case "0":
                        menuAktiv = false;
                        break;
                    default:
                        ConsoleHelper.PrintError("Ungültige Auswahl!");
                        ConsoleHelper.PressKeyToContinue();
                        break;
                }
            }
        }

        /// <summary>
        /// Zeigt den Export-Header
        /// </summary>
        private static void ZeigeExportHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("  ╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                    ║");
            Console.WriteLine("  ║                      📤 DATEN-EXPORT                               ║");
            Console.WriteLine("  ║                Excel (XLSX) & PDF Export                           ║");
            Console.WriteLine("  ║                                                                    ║");
            Console.WriteLine("  ╚════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Exportiert Artikel
        /// </summary>
        private static void ExportiereArtikel()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📦 Artikel exportieren", ConsoleColor.Cyan);

            if (DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Artikel zum Exportieren vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📊 Anzahl der Artikel: {DataManager.Inventar.Count}");
            Console.ResetColor();
            Console.WriteLine();

            string format = WaehleExportFormat();
            if (string.IsNullOrEmpty(format)) return;

            string dateiname = $"Artikel_{DateTime.Now:yyyyMMdd_HHmmss}";
            string exportPfad = WaehleExportPfad(dateiname, format == "excel" ? "csv" : "txt");

            if (string.IsNullOrEmpty(exportPfad)) return;

            if (format == "excel")
            {
                ExportiereArtikelAlsExcel(exportPfad);
            }
            else if (format == "pdf")
            {
                ExportiereArtikelAlsPDF(exportPfad);
            }
        }

        /// <summary>
        /// Exportiert Mitarbeiter
        /// </summary>
        private static void ExportiereMitarbeiter()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("👥 Mitarbeiter exportieren", ConsoleColor.Cyan);

            if (DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Mitarbeiter zum Exportieren vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📊 Anzahl der Mitarbeiter: {DataManager.Mitarbeiter.Count}");
            Console.ResetColor();
            Console.WriteLine();

            string format = WaehleExportFormat();
            if (string.IsNullOrEmpty(format)) return;

            string dateiname = $"Mitarbeiter_{DateTime.Now:yyyyMMdd_HHmmss}";
            string exportPfad = WaehleExportPfad(dateiname, format == "excel" ? "csv" : "txt");

            if (string.IsNullOrEmpty(exportPfad)) return;

            if (format == "excel")
            {
                ExportiereMitarbeiterAlsExcel(exportPfad);
            }
            else if (format == "pdf")
            {
                ExportiereMitarbeiterAlsPDF(exportPfad);
            }
        }

        /// <summary>
        /// Exportiert Benutzer
        /// </summary>
        private static void ExportiereBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("👨‍💼 Benutzer exportieren", ConsoleColor.Cyan);

            if (DataManager.Benutzer.Count == 0)
            {
                ConsoleHelper.PrintWarning("Keine Benutzer zum Exportieren vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📊 Anzahl der Benutzer: {DataManager.Benutzer.Count}");
            Console.ResetColor();
            Console.WriteLine();

            string format = WaehleExportFormat();
            if (string.IsNullOrEmpty(format)) return;

            string dateiname = $"Benutzer_{DateTime.Now:yyyyMMdd_HHmmss}";
            string exportPfad = WaehleExportPfad(dateiname, format == "excel" ? "csv" : "txt");

            if (string.IsNullOrEmpty(exportPfad)) return;

            if (format == "excel")
            {
                ExportiereBenutzerAlsExcel(exportPfad);
            }
            else if (format == "pdf")
            {
                ExportiereBenutzerAlsPDF(exportPfad);
            }
        }

        /// <summary>
        /// Exportiert alle Daten
        /// </summary>
        private static void ExportiereAlles()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("📊 Vollständiger Export", ConsoleColor.Yellow);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📦 Artikel: {DataManager.Inventar.Count}");
            Console.WriteLine($"  👥 Mitarbeiter: {DataManager.Mitarbeiter.Count}");
            Console.WriteLine($"  👨‍💼 Benutzer: {DataManager.Benutzer.Count}");
            Console.ResetColor();
            Console.WriteLine();

            string format = WaehleExportFormat();
            if (string.IsNullOrEmpty(format)) return;

            if (format == "excel")
            {
                // Für Excel: Drei separate Dateien
                string basisPfad = WaehleExportVerzeichnis();
                if (string.IsNullOrEmpty(basisPfad)) return;

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                ExportiereArtikelAlsExcel(Path.Combine(basisPfad, $"Artikel_{timestamp}.csv"));
                ExportiereMitarbeiterAlsExcel(Path.Combine(basisPfad, $"Mitarbeiter_{timestamp}.csv"));
                ExportiereBenutzerAlsExcel(Path.Combine(basisPfad, $"Benutzer_{timestamp}.csv"));

                Console.Clear();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("  ║          ✓ VOLLSTÄNDIGER EXPORT ERFOLGREICH               ║");
                Console.WriteLine("  ╚════════════════════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  📁 Verzeichnis: {basisPfad}");
                Console.WriteLine($"  📄 Artikel_{timestamp}.csv");
                Console.WriteLine($"  📄 Mitarbeiter_{timestamp}.csv");
                Console.WriteLine($"  📄 Benutzer_{timestamp}.csv");
                Console.ResetColor();
                ConsoleHelper.PressKeyToContinue();
            }
            else if (format == "pdf")
            {
                // Für PDF: Eine kombinierte Datei
                string dateiname = $"Komplett_Export_{DateTime.Now:yyyyMMdd_HHmmss}";
                string exportPfad = WaehleExportPfad(dateiname, "txt");

                if (string.IsNullOrEmpty(exportPfad)) return;

                ExportiereAllesAlsPDF(exportPfad);
            }
        }

        /// <summary>
        /// Lässt den Benutzer das Exportformat wählen
        /// </summary>
        private static string WaehleExportFormat()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ Exportformat wählen ────────────────────────────────┐");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  [1] 📊 Excel (CSV) - Tabellenformat");
            Console.WriteLine("  [2] 📄 PDF (TXT) - Druckfertig");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [0] ❌ Abbrechen");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  └──────────────────────────────────────────────────────┘");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Format: ");
            Console.ResetColor();

            string auswahl = Console.ReadLine()?.Trim();

            return auswahl switch
            {
                "1" => "excel",
                "2" => "pdf",
                "0" => null,
                _ => null
            };
        }

        /// <summary>
        /// Lässt den Benutzer den Export-Pfad wählen
        /// </summary>
        private static string WaehleExportPfad(string vorschlagDateiname, string endung)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ Speicherort wählen ─────────────────────────────────┐");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  [1] 📁 Standard-Export-Ordner (./Exports/)");
            Console.WriteLine("  [2] 🖥️  Desktop");
            Console.WriteLine("  [3] 📂 Eigener Pfad (manuell eingeben)");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [0] ❌ Abbrechen");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  └──────────────────────────────────────────────────────┘");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Auswahl: ");
            Console.ResetColor();

            string auswahl = Console.ReadLine()?.Trim();
            string basisPfad = null;

            switch (auswahl)
            {
                case "1":
                    basisPfad = StandardExportVerzeichnis;
                    StelleVerzeichnisSicher(basisPfad);
                    break;

                case "2":
                    basisPfad = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    break;

                case "3":
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  💡 Hinweise:");
                    Console.WriteLine("     • Geben Sie den vollständigen Pfad ein");
                    Console.WriteLine("     • Windows: C:\\Ordner\\Unterordner");
                    Console.WriteLine("     • Linux/Mac: /home/user/ordner");
                    Console.WriteLine("     • Der Ordner wird erstellt falls nicht vorhanden");
                    Console.ResetColor();
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("  📁 Pfad: ");
                    Console.ResetColor();

                    basisPfad = Console.ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(basisPfad))
                    {
                        ConsoleHelper.PrintError("Kein Pfad angegeben!");
                        ConsoleHelper.PressKeyToContinue();
                        return null;
                    }

                    try
                    {
                        StelleVerzeichnisSicher(basisPfad);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.PrintError($"Ungültiger Pfad: {ex.Message}");
                        ConsoleHelper.PressKeyToContinue();
                        return null;
                    }
                    break;

                case "0":
                    return null;

                default:
                    ConsoleHelper.PrintError("Ungültige Auswahl!");
                    ConsoleHelper.PressKeyToContinue();
                    return null;
            }

            // Dateinamen anpassen wenn gewünscht
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  📄 Vorgeschlagener Dateiname: {vorschlagDateiname}.{endung}");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Möchten Sie den Dateinamen ändern? [j/N]");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ ");
            Console.ResetColor();

            string aendernAntwort = Console.ReadLine()?.Trim().ToLower();

            if (aendernAntwort == "j" || aendernAntwort == "ja")
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  📝 Neuer Dateiname (ohne Endung): ");
                Console.ResetColor();

                string neuerName = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(neuerName))
                {
                    // Ungültige Zeichen entfernen
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        neuerName = neuerName.Replace(c.ToString(), "");
                    }

                    vorschlagDateiname = neuerName;
                }
            }

            string vollstaendigerPfad = Path.Combine(basisPfad, $"{vorschlagDateiname}.{endung}");

            // Bestätigung anzeigen
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Export-Pfad festgelegt:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📁 {vollstaendigerPfad}");
            Console.ResetColor();
            Console.WriteLine();

            return vollstaendigerPfad;
        }

        /// <summary>
        /// Lässt den Benutzer nur ein Verzeichnis wählen (für Vollexport mit mehreren Dateien)
        /// </summary>
        private static string WaehleExportVerzeichnis()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ┌─ Speicherort wählen ─────────────────────────────────┐");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  [1] 📁 Standard-Export-Ordner (./Exports/)");
            Console.WriteLine("  [2] 🖥️  Desktop");
            Console.WriteLine("  [3] 📂 Eigener Pfad (manuell eingeben)");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [0] ❌ Abbrechen");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  └──────────────────────────────────────────────────────┘");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  ▶ Auswahl: ");
            Console.ResetColor();

            string auswahl = Console.ReadLine()?.Trim();
            string pfad = null;

            switch (auswahl)
            {
                case "1":
                    pfad = StandardExportVerzeichnis;
                    StelleVerzeichnisSicher(pfad);
                    break;

                case "2":
                    pfad = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    break;

                case "3":
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("  📁 Pfad: ");
                    Console.ResetColor();

                    pfad = Console.ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(pfad))
                    {
                        ConsoleHelper.PrintError("Kein Pfad angegeben!");
                        ConsoleHelper.PressKeyToContinue();
                        return null;
                    }

                    try
                    {
                        StelleVerzeichnisSicher(pfad);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.PrintError($"Ungültiger Pfad: {ex.Message}");
                        ConsoleHelper.PressKeyToContinue();
                        return null;
                    }
                    break;

                case "0":
                    return null;

                default:
                    ConsoleHelper.PrintError("Ungültige Auswahl!");
                    ConsoleHelper.PressKeyToContinue();
                    return null;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Export-Verzeichnis festgelegt:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📁 {pfad}");
            Console.ResetColor();
            Console.WriteLine();

            return pfad;
        }

        // ═══════════════════════════════════════════════════════════════
        // EXCEL EXPORT
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Exportiert Artikel als Excel (CSV-Format)
        /// </summary>
        private static void ExportiereArtikelAlsExcel(string dateipfad)
        {
            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("Inv-Nr;Gerät;Mitarbeiter;Seriennummer;Preis;Anschaffung;Hersteller;Kategorie;Anzahl;Mindestbestand;Status;Erstellt Von;Erstellt Am");

            // Daten
            foreach (var artikel in DataManager.Inventar.OrderBy(a => a.InvNmr))
            {
                csv.AppendLine($"{artikel.InvNmr};{artikel.GeraeteName};{artikel.MitarbeiterBezeichnung};{artikel.SerienNummer};{artikel.Preis:F2};{artikel.Anschaffungsdatum:dd.MM.yyyy};{artikel.Hersteller};{artikel.Kategorie};{artikel.Anzahl};{artikel.Mindestbestand};{artikel.GetBestandsStatusText()};{artikel.ErstelltVon};{artikel.ErstelltAm:dd.MM.yyyy HH:mm}");
            }

            File.WriteAllText(dateipfad, csv.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("Excel (CSV)", dateipfad, DataManager.Inventar.Count);
        }

        /// <summary>
        /// Exportiert Mitarbeiter als Excel
        /// </summary>
        private static void ExportiereMitarbeiterAlsExcel(string dateipfad)
        {
            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("Vorname;Nachname;Vollständiger Name;Abteilung");

            // Daten
            foreach (var mitarbeiter in DataManager.Mitarbeiter.OrderBy(m => m.NName))
            {
                csv.AppendLine($"{mitarbeiter.VName};{mitarbeiter.NName};{mitarbeiter.VName} {mitarbeiter.NName};{mitarbeiter.Abteilung}");
            }

            File.WriteAllText(dateipfad, csv.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("Excel (CSV)", dateipfad, DataManager.Mitarbeiter.Count);
        }

        /// <summary>
        /// Exportiert Benutzer als Excel
        /// </summary>
        private static void ExportiereBenutzerAlsExcel(string dateipfad)
        {
            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("Benutzername;Berechtigung");

            // Daten
            foreach (var benutzer in DataManager.Benutzer.OrderBy(b => b.Benutzername))
            {
                csv.AppendLine($"{benutzer.Benutzername};{benutzer.Berechtigung}");
            }

            File.WriteAllText(dateipfad, csv.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("Excel (CSV)", dateipfad, DataManager.Benutzer.Count);
        }

        // ═══════════════════════════════════════════════════════════════
        // PDF EXPORT
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Exportiert Artikel als PDF (Text-basiert, druckfertig)
        /// </summary>
        private static void ExportiereArtikelAlsPDF(string dateipfad)
        {
            StringBuilder pdf = new StringBuilder();

            // Header
            pdf.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            pdf.AppendLine("║                              ARTIKEL-EXPORT                                          ║");
            pdf.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            pdf.AppendLine();
            pdf.AppendLine($"Exportiert am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            pdf.AppendLine($"Anzahl Artikel: {DataManager.Inventar.Count}");
            pdf.AppendLine();
            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine();

            // Statistik
            var stats = DataManager.GetBestandsStatistik();
            pdf.AppendLine("STATISTIK:");
            pdf.AppendLine($"  • Gesamt: {stats.gesamt} Artikel");
            pdf.AppendLine($"  • OK: {stats.ok}");
            pdf.AppendLine($"  • Niedrig: {stats.niedrig}");
            pdf.AppendLine($"  • Leer: {stats.leer}");
            pdf.AppendLine($"  • Gesamtwert: {DataManager.Inventar.Sum(a => a.Anzahl * a.Preis):C2}");
            pdf.AppendLine();
            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine();

            // Artikel nach Kategorie gruppiert
            var nachKategorie = DataManager.Inventar.GroupBy(a => a.Kategorie).OrderBy(g => g.Key);

            foreach (var gruppe in nachKategorie)
            {
                pdf.AppendLine($"📂 KATEGORIE: {gruppe.Key.ToUpper()} ({gruppe.Count()} Artikel)");
                pdf.AppendLine(new string('─', 90));
                pdf.AppendLine();

                foreach (var artikel in gruppe.OrderBy(a => a.InvNmr))
                {
                    pdf.AppendLine($"  [{artikel.InvNmr}] {artikel.GeraeteName}");
                    pdf.AppendLine($"  ├─ Mitarbeiter: {artikel.MitarbeiterBezeichnung}");
                    pdf.AppendLine($"  ├─ Seriennummer: {artikel.SerienNummer}");
                    pdf.AppendLine($"  ├─ Hersteller: {artikel.Hersteller}");
                    pdf.AppendLine($"  ├─ Preis: {artikel.Preis:C2}");
                    pdf.AppendLine($"  ├─ Anschaffung: {artikel.Anschaffungsdatum:dd.MM.yyyy}");
                    pdf.AppendLine($"  ├─ Bestand: {artikel.Anzahl}/{artikel.Mindestbestand} {artikel.GetBestandsStatusText()}");
                    pdf.AppendLine($"  └─ Erstellt: {artikel.ErstelltVon} am {artikel.ErstelltAm:dd.MM.yyyy HH:mm}");
                    pdf.AppendLine();
                }

                pdf.AppendLine();
            }

            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine("Ende des Exports");

            File.WriteAllText(dateipfad, pdf.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("PDF (TXT)", dateipfad, DataManager.Inventar.Count);
        }

        /// <summary>
        /// Exportiert Mitarbeiter als PDF
        /// </summary>
        private static void ExportiereMitarbeiterAlsPDF(string dateipfad)
        {
            StringBuilder pdf = new StringBuilder();

            pdf.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            pdf.AppendLine("║                            MITARBEITER-EXPORT                                        ║");
            pdf.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            pdf.AppendLine();
            pdf.AppendLine($"Exportiert am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            pdf.AppendLine($"Anzahl Mitarbeiter: {DataManager.Mitarbeiter.Count}");
            pdf.AppendLine();
            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine();

            // Nach Abteilung gruppiert
            var nachAbteilung = DataManager.Mitarbeiter.GroupBy(m => m.Abteilung).OrderBy(g => g.Key);

            foreach (var gruppe in nachAbteilung)
            {
                pdf.AppendLine($"🏢 ABTEILUNG: {gruppe.Key.ToUpper()} ({gruppe.Count()} Mitarbeiter)");
                pdf.AppendLine(new string('─', 90));
                pdf.AppendLine();

                foreach (var mitarbeiter in gruppe.OrderBy(m => m.NName))
                {
                    pdf.AppendLine($"  👤 {mitarbeiter.VName} {mitarbeiter.NName}");
                    pdf.AppendLine($"     Abteilung: {mitarbeiter.Abteilung}");
                    pdf.AppendLine();
                }

                pdf.AppendLine();
            }

            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine("Ende des Exports");

            File.WriteAllText(dateipfad, pdf.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("PDF (TXT)", dateipfad, DataManager.Mitarbeiter.Count);
        }

        /// <summary>
        /// Exportiert Benutzer als PDF
        /// </summary>
        private static void ExportiereBenutzerAlsPDF(string dateipfad)
        {
            StringBuilder pdf = new StringBuilder();

            pdf.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            pdf.AppendLine("║                             BENUTZER-EXPORT                                          ║");
            pdf.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            pdf.AppendLine();
            pdf.AppendLine($"Exportiert am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            pdf.AppendLine($"Anzahl Benutzer: {DataManager.Benutzer.Count}");
            pdf.AppendLine();
            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine();

            // Nach Berechtigung gruppiert
            var nachBerechtigung = DataManager.Benutzer.GroupBy(b => b.Berechtigung).OrderBy(g => g.Key);

            foreach (var gruppe in nachBerechtigung)
            {
                pdf.AppendLine($"🔐 BERECHTIGUNG: {gruppe.Key.ToString().ToUpper()} ({gruppe.Count()} Benutzer)");
                pdf.AppendLine(new string('─', 90));
                pdf.AppendLine();

                foreach (var benutzer in gruppe.OrderBy(b => b.Benutzername))
                {
                    pdf.AppendLine($"  👨‍💼 {benutzer.Benutzername}");
                    pdf.AppendLine($"     Berechtigung: {benutzer.Berechtigung}");
                    pdf.AppendLine();
                }

                pdf.AppendLine();
            }

            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine("Ende des Exports");

            File.WriteAllText(dateipfad, pdf.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("PDF (TXT)", dateipfad, DataManager.Benutzer.Count);
        }

        /// <summary>
        /// Exportiert alle Daten als ein kombiniertes PDF
        /// </summary>
        private static void ExportiereAllesAlsPDF(string dateipfad)
        {
            StringBuilder pdf = new StringBuilder();

            // Deckblatt
            pdf.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════════════╗");
            pdf.AppendLine("║                         VOLLSTÄNDIGER DATEN-EXPORT                                   ║");
            pdf.AppendLine("║                      Inventarverwaltungssystem                                       ║");
            pdf.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════════════╝");
            pdf.AppendLine();
            pdf.AppendLine($"Exportiert am: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            pdf.AppendLine();
            pdf.AppendLine("INHALT:");
            pdf.AppendLine($"  1. Artikel ({DataManager.Inventar.Count})");
            pdf.AppendLine($"  2. Mitarbeiter ({DataManager.Mitarbeiter.Count})");
            pdf.AppendLine($"  3. Benutzer ({DataManager.Benutzer.Count})");
            pdf.AppendLine();
            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine();
            pdf.AppendLine();

            // Artikel-Sektion
            pdf.AppendLine("═══════════════════════════════════════════════════════════════════════════════════════");
            pdf.AppendLine("                                  1. ARTIKEL                                          ");
            pdf.AppendLine("═══════════════════════════════════════════════════════════════════════════════════════");
            pdf.AppendLine();

            foreach (var artikel in DataManager.Inventar.OrderBy(a => a.InvNmr))
            {
                pdf.AppendLine($"[{artikel.InvNmr}] {artikel.GeraeteName}");
                pdf.AppendLine($"  Status: {artikel.GetBestandsStatusText()} | Bestand: {artikel.Anzahl}/{artikel.Mindestbestand}");
                pdf.AppendLine($"  Kategorie: {artikel.Kategorie} | Preis: {artikel.Preis:C2}");
                pdf.AppendLine();
            }

            pdf.AppendLine();
            pdf.AppendLine();

            // Mitarbeiter-Sektion
            pdf.AppendLine("═══════════════════════════════════════════════════════════════════════════════════════");
            pdf.AppendLine("                                2. MITARBEITER                                        ");
            pdf.AppendLine("═══════════════════════════════════════════════════════════════════════════════════════");
            pdf.AppendLine();

            foreach (var mitarbeiter in DataManager.Mitarbeiter.OrderBy(m => m.NName))
            {
                pdf.AppendLine($"👤 {mitarbeiter.VName} {mitarbeiter.NName} - {mitarbeiter.Abteilung}");
            }

            pdf.AppendLine();
            pdf.AppendLine();

            // Benutzer-Sektion
            pdf.AppendLine("═══════════════════════════════════════════════════════════════════════════════════════");
            pdf.AppendLine("                                 3. BENUTZER                                          ");
            pdf.AppendLine("═══════════════════════════════════════════════════════════════════════════════════════");
            pdf.AppendLine();

            foreach (var benutzer in DataManager.Benutzer.OrderBy(b => b.Benutzername))
            {
                pdf.AppendLine($"👨‍💼 {benutzer.Benutzername} - {benutzer.Berechtigung}");
            }

            pdf.AppendLine();
            pdf.AppendLine(new string('═', 90));
            pdf.AppendLine("Ende des Exports");

            File.WriteAllText(dateipfad, pdf.ToString(), Encoding.UTF8);

            ZeigeExportErfolg("Komplett-PDF (TXT)", dateipfad,
                DataManager.Inventar.Count + DataManager.Mitarbeiter.Count + DataManager.Benutzer.Count);
        }

        // ═══════════════════════════════════════════════════════════════
        // HILFSFUNKTIONEN
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Stellt sicher, dass ein Verzeichnis existiert
        /// </summary>
        private static void StelleVerzeichnisSicher(string pfad)
        {
            if (!Directory.Exists(pfad))
            {
                Directory.CreateDirectory(pfad);
            }
        }

        /// <summary>
        /// Zeigt eine Erfolgsmeldung nach dem Export
        /// </summary>
        private static void ZeigeExportErfolg(string format, string pfad, int anzahlDatensaetze)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    ✓ EXPORT ERFOLGREICH                    ║");
            Console.WriteLine("  ╚════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  📄 Format: {format}");
            Console.WriteLine($"  📊 Datensätze: {anzahlDatensaetze}");
            Console.WriteLine($"  📁 Pfad: {pfad}");
            Console.ResetColor();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  💡 Die Datei wurde erfolgreich gespeichert!");
            Console.WriteLine("     Sie können sie nun mit Excel, einem PDF-Reader");
            Console.WriteLine("     oder einem Texteditor öffnen.");
            Console.ResetColor();

            Console.WriteLine();

            // Log-Eintrag
           // LogManager.LogExport(format, anzahlDatensaetze);

            ConsoleHelper.PressKeyToContinue();
        }
    }
}