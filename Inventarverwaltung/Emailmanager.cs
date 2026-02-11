using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Inventarverwaltung
{
    /// <summary>
    /// Verwaltet den automatischen E-Mail-Versand für Bestandswarnungen
    /// Sendet eine E-Mail, wenn mindestens 4 Artikel einen niedrigen Bestand haben
    /// </summary>
    public static class EmailManager
    {
        // E-Mail-Konfiguration (hier können Sie Ihre Einstellungen anpassen)
        private static readonly string SMTP_SERVER = "Dein SMTP Server";  
        private static readonly int SMTP_PORT = 587;
        private static readonly string SENDER_EMAIL = "SenderEmail@beispiel.com";  
        private static readonly string SENDER_PASSWORD = "Dein Passwort";  
        private static readonly string RECIPIENT_EMAIL = "Empfänger E-Mail@beispiel.com";  

        private const int MINDEST_ARTIKEL_ANZAHL = 4;  // Ab 4 Artikeln wird E-Mail gesendet

        /// <summary>
        /// Prüft Bestände und versendet E-Mail bei kritischem Zustand
        /// Wird beim Programmstart aufgerufen
        /// </summary>
        public static void PruefeUndSendeBestandswarnung()
        {
            try
            {
                // Hole alle Artikel mit niedrigem Bestand
                var artikelUnterMindest = DataManager.Inventar
                    .Where(a => a.Anzahl <= a.Mindestbestand)
                    .OrderBy(a => a.Anzahl)
                    .ThenBy(a => a.GeraeteName)
                    .ToList();

                // Prüfe ob mindestens 4 Artikel betroffen sind
                if (artikelUnterMindest.Count >= MINDEST_ARTIKEL_ANZAHL)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n  📧 {artikelUnterMindest.Count} Artikel mit niedrigem Bestand erkannt...");
                    Console.WriteLine("  📤 Versende Benachrichtigungs-E-Mail...");
                    Console.ResetColor();

                    bool erfolg = SendeBestandswarnungEmail(artikelUnterMindest);

                    if (erfolg)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("  ✓ E-Mail erfolgreich versendet!");
                        Console.ResetColor();
                        // Log erfolgreichen Versand (wird über Logmanager protokolliert wenn verfügbar)
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  ✗ E-Mail konnte nicht versendet werden!");
                        Console.ResetColor();
                    }

                    System.Threading.Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Fehler beim E-Mail-Versand: {ex.Message}");
                Console.ResetColor();
                System.Threading.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Sendet die Bestandswarnung per E-Mail
        /// </summary>
        private static bool SendeBestandswarnungEmail(List<InvId> artikelListe)
        {
            try
            {
                // Kategorisiere die Artikel
                var leereArtikel = artikelListe.Where(a => a.Anzahl == 0).ToList();
                var niedrigeArtikel = artikelListe.Where(a => a.Anzahl > 0).ToList();

                // Erstelle E-Mail
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(SENDER_EMAIL);
                    mail.To.Add(RECIPIENT_EMAIL);
                    mail.Subject = $"⚠️ BESTANDSWARNUNG - {artikelListe.Count} Artikel benötigen Nachbestellung";
                    mail.IsBodyHtml = true;
                    mail.Body = ErstelleEmailInhalt(artikelListe, leereArtikel, niedrigeArtikel);

                    // SMTP-Konfiguration
                    using (SmtpClient smtp = new SmtpClient(SMTP_SERVER, SMTP_PORT))
                    {
                        smtp.Credentials = new NetworkCredential(SENDER_EMAIL, SENDER_PASSWORD);
                        smtp.EnableSsl = false;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\n  Fehlerdetails: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Erstellt den HTML-Inhalt der E-Mail
        /// </summary>
        private static string ErstelleEmailInhalt(List<InvId> alle, List<InvId> leer, List<InvId> niedrig)
        {
            string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }
        .container { background-color: white; padding: 30px; border-radius: 10px; max-width: 800px; margin: 0 auto; }
        .header { background-color: #dc3545; color: white; padding: 20px; border-radius: 5px; text-align: center; }
        .warning-box { background-color: #fff3cd; border-left: 5px solid #ffc107; padding: 15px; margin: 20px 0; }
        .critical-box { background-color: #f8d7da; border-left: 5px solid #dc3545; padding: 15px; margin: 20px 0; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th { background-color: #343a40; color: white; padding: 12px; text-align: left; }
        td { padding: 10px; border-bottom: 1px solid #ddd; }
        .status-leer { color: #dc3545; font-weight: bold; }
        .status-niedrig { color: #ffc107; font-weight: bold; }
        .footer { margin-top: 30px; padding-top: 20px; border-top: 2px solid #ddd; color: #666; font-size: 12px; }
        .summary { background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 20px 0; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ BESTANDSWARNUNG</h1>
            <p>Automatische Benachrichtigung - Inventarverwaltungssystem</p>
        </div>
        
        <div class='summary'>
            <h2>📊 Zusammenfassung</h2>
            <p><strong>Zeitpunkt:</strong> " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + @"</p>
            <p><strong>Betroffene Artikel:</strong> " + alle.Count + @" Artikel benötigen Nachbestellung</p>
            <ul>
                <li>🔴 <strong>Komplett aufgebraucht:</strong> " + leer.Count + @" Artikel (0 Stück)</li>
                <li>🟡 <strong>Niedriger Bestand:</strong> " + niedrig.Count + @" Artikel</li>
            </ul>
        </div>";

            // Leere Artikel
            if (leer.Count > 0)
            {
                html += @"
        <div class='critical-box'>
            <h2>🔴 KRITISCH - Komplett aufgebraucht (" + leer.Count + @" Artikel)</h2>
            <p>Diese Artikel sind komplett aufgebraucht und müssen dringend nachbestellt werden!</p>
        </div>
        
        <table>
            <tr>
                <th>Inventar-Nr</th>
                <th>Gerätename</th>
                <th>Hersteller</th>
                <th>Aktuell</th>
                <th>Mindest</th>
                <th>Status</th>
            </tr>";

                foreach (var artikel in leer)
                {
                    html += @"
            <tr>
                <td>" + artikel.InvNmr + @"</td>
                <td>" + artikel.GeraeteName + @"</td>
                <td>" + artikel.Hersteller + @"</td>
                <td class='status-leer'>" + artikel.Anzahl + @"</td>
                <td>" + artikel.Mindestbestand + @"</td>
                <td class='status-leer'>❌ LEER</td>
            </tr>";
                }

                html += @"
        </table>";
            }

            // Niedrige Artikel
            if (niedrig.Count > 0)
            {
                html += @"
        <div class='warning-box'>
            <h2>🟡 WARNUNG - Niedriger Bestand (" + niedrig.Count + @" Artikel)</h2>
            <p>Diese Artikel haben einen niedrigen Bestand und sollten zeitnah nachbestellt werden.</p>
        </div>
        
        <table>
            <tr>
                <th>Inventar-Nr</th>
                <th>Gerätename</th>
                <th>Hersteller</th>
                <th>Aktuell</th>
                <th>Mindest</th>
                <th>Status</th>
            </tr>";

                foreach (var artikel in niedrig)
                {
                    html += @"
            <tr>
                <td>" + artikel.InvNmr + @"</td>
                <td>" + artikel.GeraeteName + @"</td>
                <td>" + artikel.Hersteller + @"</td>
                <td class='status-niedrig'>" + artikel.Anzahl + @"</td>
                <td>" + artikel.Mindestbestand + @"</td>
                <td class='status-niedrig'>⚠️ NIEDRIG</td>
            </tr>";
                }

                html += @"
        </table>";
            }

            // Empfehlungen
            html += @"
        <div class='summary'>
            <h2>💡 Empfohlene Maßnahmen</h2>
            <ol>
                <li>Prüfen Sie die kritischen Artikel umgehend</li>
                <li>Bestellen Sie Nachschub für leere Positionen</li>
                <li>Planen Sie Bestellungen für Artikel mit niedrigem Bestand</li>
                <li>Passen Sie bei Bedarf die Mindestbestände an</li>
            </ol>
        </div>
        
        <div class='footer'>
            <p>Diese E-Mail wurde automatisch vom Inventarverwaltungssystem generiert.</p>
            <p>Zeitpunkt: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + @"</p>
            <p>📦 Inventarverwaltung mit Bestandsführung | 🔐 AES-256 verschlüsselt</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }

        /// <summary>
        /// Testet die E-Mail-Konfiguration
        /// </summary>
        public static void TesteEmailKonfiguration()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("E-Mail-Konfiguration testen", ConsoleColor.Cyan);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📧 Aktuelle E-Mail-Konfiguration:");
            Console.WriteLine("  ═══════════════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  SMTP-Server:    {SMTP_SERVER}:{SMTP_PORT}");
            Console.WriteLine($"  Absender:       {SENDER_EMAIL}");
            Console.WriteLine($"  Empfänger:      {RECIPIENT_EMAIL}");
            Console.WriteLine($"  Schwellwert:    {MINDEST_ARTIKEL_ANZAHL} Artikel");
            Console.WriteLine();

            string antwort = ConsoleHelper.GetInput("Möchten Sie eine Test-E-Mail versenden? (j/n)");

            if (antwort.ToLower() == "j" || antwort.ToLower() == "ja")
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  📤 Versende Test-E-Mail...");
                Console.ResetColor();

                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(SENDER_EMAIL);
                        mail.To.Add(RECIPIENT_EMAIL);
                        mail.Subject = "✓ Test - Inventarverwaltung E-Mail-System";
                        mail.Body = $"Dies ist eine Test-E-Mail vom Inventarverwaltungssystem.\n\n" +
                                   $"Zeitpunkt: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n" +
                                   $"Die E-Mail-Konfiguration funktioniert korrekt!";

                        using (SmtpClient smtp = new SmtpClient(SMTP_SERVER, SMTP_PORT))
                        {
                            smtp.Credentials = new NetworkCredential(SENDER_EMAIL, SENDER_PASSWORD);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  ✓ Test-E-Mail erfolgreich versendet!");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  ✗ Fehler beim Versenden der Test-E-Mail!");
                    Console.WriteLine($"  Fehler: {ex.Message}");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  💡 Hinweise:");
                    Console.WriteLine("  • Prüfen Sie die E-Mail-Adresse und das Passwort");
                    Console.WriteLine("  • Bei Gmail: Aktivieren Sie 'App-Passwörter'");
                    Console.WriteLine("  • Prüfen Sie Ihre Firewall-Einstellungen");
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }

        /// <summary>
        /// Zeigt Informationen zur E-Mail-Konfiguration
        /// </summary>
        public static void ZeigeEmailInfo()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("E-Mail-Benachrichtigungen", ConsoleColor.Cyan);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ║               📧 AUTOMATISCHE E-MAIL-WARNUNGEN                    ║");
            Console.WriteLine("  ║                                                                   ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚙️  FUNKTIONSWEISE:");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  • Beim Programmstart wird der Bestand geprüft");
            Console.WriteLine($"  • Ab {MINDEST_ARTIKEL_ANZAHL} Artikeln mit niedrigem Bestand wird eine E-Mail versendet");
            Console.WriteLine("  • Die E-Mail enthält eine detaillierte Übersicht");
            Console.WriteLine("  • Kategorisierung: KRITISCH (leer) und WARNUNG (niedrig)");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  📋 KONFIGURATION:");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  SMTP-Server:        {SMTP_SERVER}:{SMTP_PORT}");
            Console.WriteLine($"  Absender-Email:     {SENDER_EMAIL}");
            Console.WriteLine($"  Empfänger-Email:    {RECIPIENT_EMAIL}");
            Console.WriteLine($"  Benachrichtigungs-Schwelle: {MINDEST_ARTIKEL_ANZAHL} Artikel");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠️  WICHTIGE HINWEISE:");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  1. Konfigurieren Sie die E-Mail-Einstellungen in EmailManager.cs");
            Console.WriteLine("  2. Bei Gmail: Erstellen Sie ein App-spezifisches Passwort");
            Console.WriteLine("  3. Alternativ: Nutzen Sie andere SMTP-Server (Outlook, etc.)");
            Console.WriteLine("  4. Die Konfiguration muss im Quellcode angepasst werden");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  📧 Gmail App-Passwort erstellen:");
            Console.WriteLine("     1. Google-Konto → Sicherheit");
            Console.WriteLine("     2. 2-Faktor-Authentifizierung aktivieren");
            Console.WriteLine("     3. App-Passwörter → Neue App → Passwort generieren");
            Console.WriteLine("     4. Generiertes Passwort in SENDER_PASSWORD eintragen");
            Console.ResetColor();

            Console.WriteLine();
            ConsoleHelper.PressKeyToContinue();
        }
    }
}