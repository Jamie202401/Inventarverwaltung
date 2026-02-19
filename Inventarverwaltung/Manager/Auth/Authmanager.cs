using Inventarverwaltung.Manager.UI;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Inventarverwaltung.Manager.Auth
{
    /// <summary>
    /// Verwaltet die Benutzeranmeldung mit Passwort-Hashing (SHA-256)
    /// </summary>
    public static class AuthManager
    {
        public static string AktuellerBenutzer { get; private set; }

        private const int MaxFehlversuche = 5;
        private const int SperrZeitSekunden = 30;

        // ─────────────────────────────────────────────────────────────────────
        // ANMELDUNG
        // ─────────────────────────────────────────────────────────────────────

        public static void Anmeldung()
        {
            Console.Clear();
            ZeigeLogo();

            DataManager.LoadBenutzer();

            if (DataManager.Benutzer.Count == 0)
            {
                ErsterStart();
                DataManager.LoadBenutzer();
            }

            int fehlversuche = 0;

            while (true)
            {
                if (fehlversuche >= MaxFehlversuche)
                {
                    ZeigeSperre(SperrZeitSekunden);
                    fehlversuche = 0;
                }

                ZeigeAnmeldeFormular();

                // Benutzername
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  \U0001f464  Benutzername  : ");
                Console.ResetColor();
                string benutzername = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(benutzername))
                {
                    ZeigeFehler("Benutzername darf nicht leer sein.");
                    continue;
                }

                var benutzer = DataManager.Benutzer.FirstOrDefault(b =>
                    b.Benutzername.Equals(benutzername, StringComparison.OrdinalIgnoreCase));

                if (benutzer == null)
                {
                    fehlversuche++;
                    ZeigeFehler($"Benutzer '{benutzername}' existiert nicht.  [{fehlversuche}/{MaxFehlversuche}]");
                    Thread.Sleep(600);
                    continue;
                }

                // Passwort
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  \U0001f511  Passwort       : ");
                Console.ResetColor();
                string passwort = PasswortEingabe();

                // Kein Passwort gesetzt → erstmalig vergeben
                if (benutzer.HatKeinPasswort)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("  Fuer diesen Account wurde noch kein Passwort gesetzt.");
                    Console.WriteLine("  Bitte vergeben Sie jetzt ein Passwort.");
                    Console.ResetColor();
                    Console.WriteLine();
                    PasswortErstmaligSetzen(benutzer);
                    DataManager.SaveBenutzerToFile();
                    ZeigeFehler("Passwort gesetzt. Bitte erneut anmelden.");
                    Thread.Sleep(1200);
                    continue;
                }

                string eingabeHash = HashPasswort(passwort);

                if (!eingabeHash.Equals(benutzer.PasswortHash, StringComparison.Ordinal))
                {
                    fehlversuche++;
                    Console.WriteLine();
                    ZeigeFehler($"Falsches Passwort.  [{fehlversuche}/{MaxFehlversuche}]");
                    Thread.Sleep(700);
                    continue;
                }

                fehlversuche = 0;
                AktuellerBenutzer = benutzer.Benutzername;
                LogManager.LogAnmeldungErfolgreich(AktuellerBenutzer);
                ZeigeErfolg(benutzer);
                break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ERSTER START
        // ─────────────────────────────────────────────────────────────────────

        private static void ErsterStart()
        {
            Console.Clear();
            ZeigeLogo();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  +----------------------------------------------------------+");
            Console.WriteLine("  |   ERSTER START - Admin-Account einrichten                |");
            Console.WriteLine("  +----------------------------------------------------------+");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Es existieren noch keine Benutzer-Accounts.");
            Console.WriteLine("  Bitte richten Sie den ersten Administrator-Account ein.");
            Console.ResetColor();
            Console.WriteLine();

            string benutzername;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Admin-Benutzername : ");
                Console.ResetColor();
                benutzername = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(benutzername) || benutzername.Length < 3)
                {
                    ZeigeFehler("Benutzername muss mindestens 3 Zeichen haben.");
                    continue;
                }
                break;
            }

            Console.WriteLine();
            var adminAccount = new Accounts(benutzername, Berechtigungen.Admin);
            PasswortErstmaligSetzen(adminAccount);

            DataManager.Benutzer.Add(adminAccount);
            DataManager.SaveBenutzerToFile();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Admin-Account '{benutzername}' wurde erfolgreich angelegt!");
            Console.ResetColor();
            Thread.Sleep(1800);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSWORT ERSTMALIG SETZEN
        // ─────────────────────────────────────────────────────────────────────

        private static void PasswortErstmaligSetzen(Accounts account)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Neues Passwort        : ");
                Console.ResetColor();
                string pw1 = PasswortEingabe();
                Console.WriteLine();

                if (string.IsNullOrEmpty(pw1) || pw1.Length < 6)
                {
                    ZeigeFehler("Passwort muss mindestens 6 Zeichen lang sein.");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Passwort bestaetigen  : ");
                Console.ResetColor();
                string pw2 = PasswortEingabe();
                Console.WriteLine();

                if (pw1 != pw2)
                {
                    ZeigeFehler("Passwoerter stimmen nicht ueberein. Bitte erneut versuchen.");
                    continue;
                }

                account.PasswortHash = HashPasswort(pw1);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Passwort wurde sicher gesetzt (SHA-256).");
                Console.ResetColor();
                break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSWORT AENDERN (oeffentlich)
        // ─────────────────────────────────────────────────────────────────────

        public static void PasswortAendern()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Passwort aendern", ConsoleColor.Yellow);

            var benutzer = DataManager.Benutzer.FirstOrDefault(b =>
                b.Benutzername.Equals(AktuellerBenutzer, StringComparison.OrdinalIgnoreCase));

            if (benutzer == null)
            {
                ConsoleHelper.PrintError("Benutzer nicht gefunden.");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            if (!benutzer.HatKeinPasswort)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("  Aktuelles Passwort : ");
                Console.ResetColor();
                string altPw = PasswortEingabe();
                Console.WriteLine();

                if (!HashPasswort(altPw).Equals(benutzer.PasswortHash, StringComparison.Ordinal))
                {
                    ZeigeFehler("Falsches Passwort. Abbruch.");
                    ConsoleHelper.PressKeyToContinue();
                    return;
                }
            }

            Console.WriteLine();
            PasswortErstmaligSetzen(benutzer);
            DataManager.SaveBenutzerToFile();
            LogManager.LogDatenGespeichert("Passwort", $"Passwort von '{AktuellerBenutzer}' geaendert");
            ConsoleHelper.PressKeyToContinue();
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSWORT-EINGABE mit Show/Hide Toggle [Tab]
        // ─────────────────────────────────────────────────────────────────────

        public static string PasswortEingabe()
        {
            var pw = new StringBuilder();
            bool sichtbar = false;

            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;

            Console.WriteLine();
            Console.SetCursorPosition(startLeft, startTop);

            void ZeigeHinweis()
            {
                int cl = Console.CursorLeft, ct = Console.CursorTop;
                Console.SetCursorPosition(2, startTop + 1);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(sichtbar
                    ? "[Tab] Verbergen                              "
                    : "[Tab] Anzeigen                               ");
                Console.ResetColor();
                Console.SetCursorPosition(cl, ct);
            }

            void AktualisierePW()
            {
                Console.SetCursorPosition(startLeft, startTop);
                Console.ForegroundColor = sichtbar ? ConsoleColor.Yellow : ConsoleColor.DarkCyan;
                string anzeige = sichtbar ? pw.ToString() : new string('\u25CF', pw.Length);
                Console.Write(anzeige + "   ");
                Console.SetCursorPosition(startLeft + pw.Length, startTop);
                Console.ResetColor();
            }

            ZeigeHinweis();

            while (true)
            {
                var taste = Console.ReadKey(intercept: true);

                if (taste.Key == ConsoleKey.Enter) break;
                if (taste.Key == ConsoleKey.Escape) { pw.Clear(); break; }

                if (taste.Key == ConsoleKey.Tab)
                {
                    sichtbar = !sichtbar;
                    AktualisierePW();
                    ZeigeHinweis();
                    continue;
                }

                if (taste.Key == ConsoleKey.Backspace)
                {
                    if (pw.Length > 0)
                    {
                        pw.Remove(pw.Length - 1, 1);
                        AktualisierePW();
                    }
                    continue;
                }

                if (taste.KeyChar >= ' ')
                {
                    pw.Append(taste.KeyChar);
                    AktualisierePW();
                }
            }

            Console.SetCursorPosition(0, startTop + 1);
            Console.Write(new string(' ', Console.WindowWidth > 0 ? Console.WindowWidth : 80));
            Console.SetCursorPosition(0, startTop + 1);

            return pw.ToString();
        }

        // ─────────────────────────────────────────────────────────────────────
        // SHA-256 HASHING
        // ─────────────────────────────────────────────────────────────────────

        public static string HashPasswort(string passwort)
        {
            const string appSalt = "InvVerwaltung#2025$Salt!";
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(appSalt + passwort));
                return Convert.ToBase64String(bytes);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI
        // ─────────────────────────────────────────────────────────────────────

        private static void ZeigeLogo()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  +==================================================================+");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  |                                                                  |");
            Console.WriteLine("  |        INVENTARVERWALTUNG  -  ANMELDUNG                         |");
            Console.WriteLine("  |                                                                  |");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  |          KI-gestuetzte Verwaltungssoftware  |  v2.0             |");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  |          Sicherheit: SHA-256  |  Passworte gehasht              |");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  |                                                                  |");
            Console.WriteLine("  +==================================================================+");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeAnmeldeFormular()
        {
            Console.Clear();
            ZeigeLogo();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  +------------------------------------------------------------------+");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  |  Bitte melden Sie sich an                                        |");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  +------------------------------------------------------------------+");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeFehler(string meldung)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  X  {meldung}");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void ZeigeSperre(int sekunden)
        {
            for (int i = sekunden; i > 0; i--)
            {
                Console.Clear();
                ZeigeLogo();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  +============================================================+");
                Console.WriteLine("  |                                                            |");
                Console.WriteLine("  |   ACCOUNT TEMPORAER GESPERRT                              |");
                Console.WriteLine("  |   Zu viele Fehlversuche.                                  |");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"  |   Bitte warten: {i,3} Sekunden...                          |");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  |                                                            |");
                Console.WriteLine("  +============================================================+");
                Console.ResetColor();
                Thread.Sleep(1000);
            }
        }

        private static void ZeigeErfolg(Accounts benutzer)
        {
            Console.Clear();
            ZeigeLogo();

            string icon = benutzer.Berechtigung == Berechtigungen.Admin ? "[ADMIN]" : "[USER] ";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  +==================================================================+");
            Console.WriteLine("  |                                                                  |");
            Console.WriteLine("  |   ANMELDUNG ERFOLGREICH                                         |");
            Console.WriteLine("  |                                                                  |");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  |   Willkommen,  {benutzer.Benutzername,-51}|");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  |   Berechtigung: {icon} {benutzer.Berechtigung,-47}|");
            Console.WriteLine($"  |   Angemeldet:  {DateTime.Now:dd.MM.yyyy  HH:mm:ss,-51}|");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  |                                                                  |");
            Console.WriteLine("  +==================================================================+");
            Console.ResetColor();

            Thread.Sleep(1600);
        }
    }
}