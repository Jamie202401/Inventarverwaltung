using Inventarverwaltung;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace Invverw
{
    public enum Berechtigungen
    {
        Unbekannt = 0,
        User = 1,
        Admin = 2,
    }

    class Programm
    {

        // Definiert die Erstellung der Text Dateien  und benennt diese dem Entsprechend
        static List<InvId> Inventar = new List<InvId>();
        static List<MID> Mitarbeiter = new List<MID>();
        static List<Accounts> Benutzer = new List<Accounts>();
        static List<Anmelder> Anmeldung = new List<Anmelder>();
        // Gibt dem Namen der Datei an und zeigt den Dateipfad an in Unserem Fall ist es der Standardpfad (C:\Program Files\Microsoft Visual Studio\2022\)
        static string filepath = "Inventar.txt";
        static string filepath2 = "Mitarbeiter.txt";
        static string filepath3 = "Accounts.txt";
        static string filepath4 = "Anmelder.txt";

        //    public static string? VName { get; private set; }
        //    public static string? NName { get; private set; }

       static void CreateAndHideFile(string path)
        {
            // Datei erstellen, falls sie nicht existiert
            if (!File.Exists(path))
            {
                File.Create(path).Close(); // Datei muss geschlossen werden
            }

            // Datei verstecken
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
        }
        static void HideAllFiles()
        {
            CreateAndHideFile(filepath);
            CreateAndHideFile(filepath2);
            CreateAndHideFile(filepath3);
            CreateAndHideFile(filepath4);
        }






        static void SetupConsole()
        {
            Console.Title = "INVENTARVERWALTUNG";

            Console.SetWindowSize(120, 40);

            Console.SetBufferSize(120, 40);

            Console.CursorVisible = true;
        }
        
        

        static void Anmeldung2()
        {
            
            // Lade vorhandene Accounts

            LoadAnmeldung();

            Console.Clear();
            Console.WriteLine("==============================================");
            Console.WriteLine("               BENUTZERANMELDUNG             ");
            Console.WriteLine("==============================================\n");

            Console.Write("Bitte Benutzernamen eingeben: ");
            string anmeldename = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(anmeldename))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ungültiger Benutzername!");
                Console.ResetColor();
                return;
            }

            // Prüfen, ob der Benutzer bereits existiert
            Anmelder existierenderBenutzer = Anmeldung.FirstOrDefault(a => a.Anmeldename.Equals(anmeldename, StringComparison.OrdinalIgnoreCase));

            if (existierenderBenutzer != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nWillkommen zurück, '{anmeldename}'!");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nBenutzer nicht gefunden. Möchten Sie ein neues Konto erstellen? (1 = Ja, 0 = Nein)");
                string input = Console.ReadLine()?.Trim();

                if (input == "1")
                {
                    // Neuen Benutzer anlegen
                    Anmelder neuerBenutzer = new Anmelder(anmeldename);
                    Anmeldung.Add(neuerBenutzer);

                    // In Datei speichern
                    SaveIntoNewAccounts();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nBenutzer '{anmeldename}' wurde erfolgreich erstellt!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nAnmeldung abgebrochen.");
                    Console.ResetColor();
                }
            }

          //  Console.WriteLine("\nDrücken Sie eine Taste, um fortzufahren...");
          //  Console.ReadKey();
        }

        public class Anmelder
        {
            public string Anmeldename { get; set; }

            public Anmelder(string anmeldename)
            {
                this.Anmeldename = anmeldename;
            }
        }
        static void LoadAnmeldung()
        {
            Anmeldung.Clear(); // Wichtig: Liste vorher leeren

            if (!File.Exists(filepath4)) return;

            string[] lines = File.ReadAllLines(filepath4);

            foreach (var line in lines)
            {
                string name = line.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    Anmeldung.Add(new Anmelder(name));
                }
            }
        }
        static void SaveIntoNewAccounts()
        {
            using (StreamWriter sw = new StreamWriter(filepath4, true)) // false = gesamte Liste überschreiben
            {
                foreach (var benutzer in Anmeldung)
                {
                    sw.WriteLine(benutzer.Anmeldename);
                }
            }
        }
        static void Main(string[] args)
        {
           HideAllFiles();
            Anmeldung2();
            SetupConsole();
            ShowMenu();
            //Ändern der Farbe und schreiben des Titels in die Konsole
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==============================================");
            Console.WriteLine("           INVENTARVERWALTUNG SYSTEM           ");
            Console.WriteLine("==============================================");
            //Zurücksetzen der Farbe, damit der nächste Text normal geschrieben wird
            Console.ResetColor();

            //Aufrufen der Mitarbeiter und der Inventarsdatei
            LoadBenutzer();
            LoadMitarbeiter();
            LoadInventar();
            //Schleife, dass sich das Menü nach jedem Hop aus einer Klasse angestuert wird
            bool running = true;
            while (running)
            {

                ShowMenu();
                String Auswahl = Console.ReadLine();

                switch (Auswahl)
                {
                    //Verzweigungen zu den Verschiedenen Klassen
                    case "1":
                        NeuenArtikelErstellen();
                        break;

                    case "2":
                        NeuenMitarbeiterhinzufuegen();
                        break;

                    case "3":
                        SeheMitarbeiter();
                        break;

                    case "4":
                        SeheInventar();
                        break;

                    case "5":
                        Benutzerdaten();
                        break;

                    case "6":
                        SeheBenutzer();
                        break;


                }

            }
            LoadMitarbeiter();
            LoadInventar();
            Console.WriteLine("Danke für die Nutzung des Systemes");

        }

        static void PrintHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==============================================");
            Console.WriteLine("           INVENTARVERWALTUNG SYSTEM           ");
            Console.WriteLine("==============================================");
            Console.ResetColor();
        }

        static void PrintMenuItem(string key, string text)
        {
            Console.WriteLine($"  [{key}]  {text}");
        }


        static void ShowMenu()
        {
            PrintHeader();

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" Bitte wählen Sie eine Aktion:");
            Console.ResetColor();
            Console.WriteLine();

            PrintMenuItem("1", "Neuen Artikel hinzufügen");
            PrintMenuItem("2", "Neuen Mitarbeiter hinzufügen");
            PrintMenuItem("3", "Mitarbeiter anzeigen");
            PrintMenuItem("4", "Inventar anzeigen");
            PrintMenuItem("5", "Benutzer anlegen");
            PrintMenuItem("6", "Benutzer anzeigen");

            Console.WriteLine();
            PrintMenuItem("0", "Programm beenden");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" Auswahl: ");
            Console.ResetColor();
        }




        static void NeuenArtikelErstellen()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("---------------------------------");
            Console.WriteLine("     Neues Gerät hinzufügen      ");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();

            // Eingabe der Inventarnummer
            Console.WriteLine("Geben Sie bitte die Inventar-Nummer ein (INVxyz):");
            string InvNmr = Console.ReadLine();

            // Eingabe des Gerätenamens
            Console.WriteLine("Geben Sie nun den Namen für das Gerät ein:");
            string GeraeteName = Console.ReadLine();

            // Eingabe des Mitarbeiters
            Console.WriteLine("Geben Sie den Mitarbeiter ein (Vorname Nachname):");
            string MitarbeiterBezeichnung = Console.ReadLine();


            bool mitarbeiterExistiert = Mitarbeiter.Any(m =>
                $"{m.VName} {m.NName}"
                    .Equals(MitarbeiterBezeichnung, StringComparison.OrdinalIgnoreCase)
            );

            if (!mitarbeiterExistiert)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nDer angegebene Mitarbeiter existiert nicht!");
                Console.WriteLine("Bitte legen Sie den Mitarbeiter zuerst an.");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }


            bool inventarExistiert = Inventar.Exists(i =>
                i.InvNmr.Equals(InvNmr, StringComparison.OrdinalIgnoreCase) ||
                i.GeraeteName.Equals(GeraeteName, StringComparison.OrdinalIgnoreCase)
            );

            if (inventarExistiert)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nEin Gerät mit dieser Inventar-Nummer oder diesem Namen existiert bereits!");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }


            InvId newInventar = new InvId(InvNmr, GeraeteName, MitarbeiterBezeichnung);
            Inventar.Add(newInventar);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"\nDas Gerät '{newInventar.GeraeteName}' mit der ID '{newInventar.InvNmr}' " +
                $"wurde erfolgreich dem Mitarbeiter '{newInventar.MitarbeiterBezeichnung}' zugeordnet."
            );
            Console.ResetColor();


            SaveInvToFile();

            Console.ReadLine();
        }

        static void SeheInventar()
        {
            Console.Clear();
            if (Inventar.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n Keine Artikel vorhanden");
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("---------------------------------");
            Console.WriteLine("         Inventar Übersicht:       ");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Nr    Vorname        Nachname       Abteilung");
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.ResetColor();

            for (int i = 0; i < Inventar.Count; i++)
            {
                InvId e = Inventar[i];
                Console.WriteLine($"#{i + 1,-4} {e.InvNmr,-15} {e.GeraeteName,-15}  {e.MitarbeiterBezeichnung,-15}");

            }
            Console.WriteLine("\nDrücken Sie eine Taste um fortzufahren...");
            Console.ReadKey();



        }

        static void NeuenMitarbeiterhinzufuegen()
        {

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Clear();
            Console.WriteLine("---------------------------------");
            Console.WriteLine("     Mitarbeiter Hinzufügen      ");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();



            Console.WriteLine("Bitte geben Sie den Namen des neuen Mitarbeiters ein !");

            Console.WriteLine("Vorname des Mitarbeiters:   ");
            String VName = Console.ReadLine();

            Console.WriteLine("Nachanme des Mitarbeiters:   ");
            string NName = Console.ReadLine();

            Console.WriteLine("Abteilung des Mitarbeiters:  ");
            String Abteilung = Console.ReadLine();


            MID newMitarbeiter = new MID(VName, NName, Abteilung);
            bool exestiertbereits = Mitarbeiter.Exists(m => m.VName.Equals(VName, StringComparison.OrdinalIgnoreCase) && (m.NName.Equals(NName, StringComparison.OrdinalIgnoreCase)));
            if (exestiertbereits)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Der Benutzer mit dem Namen ' {newMitarbeiter.NName}' exestiert bereits ");

                Console.ResetColor();
                Console.ReadLine();
                return;
            }
            Mitarbeiter.Add(newMitarbeiter);


            SaveMitarbeiterToFile();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nMitarbeiter '{newMitarbeiter.NName} ' wurde erfolgreich gespeichert.");
            Console.ResetColor();




            Console.WriteLine("\nDrücken Sie eine Taste um fortzufahren...");
            Console.ReadKey();

        }

        static void SeheMitarbeiter()
        {
            Console.Clear();
            if (Mitarbeiter.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n Keine Mitarbeiter vorhanden");
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("---------------------------------");
            Console.WriteLine("         Alle Mitarbeiter:       ");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Nr    Vorname        Nachname       Abteilung");
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.ResetColor();
            for (int i = 0; i < Mitarbeiter.Count; i++)
            {
                MID m = Mitarbeiter[i];
                Console.WriteLine($"#{i + 1,-4} {m.VName,-15} {m.NName,-15}  {m.Abteilung,-15}");
            }


            Console.WriteLine("\nDrücken Sie eine Taste um fortzufahren...");
            Console.ReadKey();
        }

        static void Benutzerdaten()
        {
            string eingabe;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Clear();
            Console.WriteLine("---------------------------------");
            Console.WriteLine("     Benutzer Hinzufügen      ");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("Bitte geben Sie den Namen des Benutzers ein");
            string BenutzerName = Console.ReadLine();
            Console.WriteLine("Welche Berechtigungen wollen Sie den Benutzer geben ?");
            Console.WriteLine("1 - Normaler Benutzer (Nur hinzufügen)");
            Console.WriteLine("2 - Admin (Alle Rechte)");

            Berechtigungen berechtigung;

            while (true)
            {
                Console.WriteLine("Auswahl: ");
                eingabe = Console.ReadLine();

                if (int.TryParse(eingabe, out int auswahl) && Enum.IsDefined(typeof(Berechtigungen), auswahl))
                {
                    berechtigung = (Berechtigungen)auswahl;
                    break;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ungültige Auswahl! Bitte entweder 1 oder 2 Eingeben");
                Console.ResetColor();


            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Benutzer: '{BenutzerName}' wurde als '{berechtigung}' angelegt.");
            Console.ResetColor();
            Accounts newBenutzer = new Accounts(BenutzerName, berechtigung);

            Benutzer.Add(newBenutzer);
            SafeBenutzerToFile();

            Console.WriteLine("Bitte drücken Sie eine Taste um fortzufahren...");
            Console.ReadKey();



        }

        static void SeheBenutzer()
        {
            Console.Clear();
            if (Benutzer.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n Keine Benutzer vorhanden");
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("---------------------------------");
            Console.WriteLine("         Alle Benutzer:       ");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Nr    Vorname        Berechtigung       ");
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.ResetColor();
            for (int i = 0; i < Benutzer.Count; i++)
            {
                Accounts a = Benutzer[i];
                Console.WriteLine($"#{i + 1,-4} {a.Benutzername} {a.Berechtigung,-15}  ");
            }


            Console.WriteLine("\nDrücken Sie eine Taste um fortzufahren...");
            Console.ReadKey();
        }



        static void LoadMitarbeiter()
        {
            if (File.Exists(filepath2))
            {
                string[] mitarbeiter = File.ReadAllLines(filepath2);
                foreach (var line in mitarbeiter)
                {
                    string[] MitarbeiterData = line.Split(';');
                    if (MitarbeiterData.Length >= 3)
                    {
                        Mitarbeiter.Add(new MID(MitarbeiterData[0], MitarbeiterData[1], MitarbeiterData[2]));
                    }
                }
            }
        }

        static void SaveMitarbeiterToFile()
        {
            using (StreamWriter sw = new StreamWriter(filepath2, true))
            {
                foreach (MID Mitarbeiter in Mitarbeiter)
                {
                    sw.WriteLine($"{Mitarbeiter.VName};{Mitarbeiter.NName};{Mitarbeiter.Abteilung}");

                }
            }
        }

        static void LoadInventar()
        {
            if (File.Exists(filepath))
            {
                string[] inventar = File.ReadAllLines(filepath);
                foreach (var line in inventar)
                {
                    string[] inventarData = line.Split(';');
                    if (inventarData.Length >= 3)
                    {
                        Inventar.Add(new InvId(inventarData[0], inventarData[1], inventarData[2]));
                    }
                }
            }
        }

        static void SaveInvToFile()
        {
            if (Inventar.Count == 0) return;

            // Anhängen nur des letzten Eintrags
            InvId letzterInventar = Inventar[Inventar.Count - 1];
            using (StreamWriter sw2 = new StreamWriter(filepath, true))
            {
                sw2.WriteLine($"{letzterInventar.InvNmr};{letzterInventar.GeraeteName};{letzterInventar.MitarbeiterBezeichnung}");
            }
        }

        /*      static void LoadBenutzer()
              {
                  if (File.Exists(filepath3))
                  {
                      string[] benutzer = File.ReadAllLines(filepath3);
                      foreach (var line in benutzer)
                      {
                          string[] benutzerData = line.Split('|');
                          if (benutzerData.Length >= 2)
                          {
                              Benutzer.Add(new Accounts(benutzerData[0], benutzerData[1]));
                          }
                      }
                  }
              }*/


        static void LoadBenutzer()
        {
            if (!File.Exists(filepath3)) return;

            string[] lines = File.ReadAllLines(filepath3);

            Benutzer.Clear(); // ⚠️ wichtig bei erneutem Laden

            foreach (var line in lines)
            {
                string[] data = line.Split(';');

                if (data.Length != 2) continue;

                string name = data[0].Trim();
                string rolleText = data[1].Trim();

                if (!Enum.TryParse(rolleText, out Berechtigungen rolle))
                {
                    rolle = Berechtigungen.Unbekannt;
                }

                Benutzer.Add(new Accounts(name, rolle));
            }
        }



        static void SafeBenutzerToFile()
        {
            if (Benutzer.Count == 0) return;

            Accounts letzter = Benutzer[Benutzer.Count - 1];

            using (StreamWriter sw3 = new StreamWriter(filepath3, true))
            {
                foreach (Accounts Acc in Benutzer)
                {
                    sw3.WriteLine($"{Acc.Benutzername};{Acc.Berechtigung}");
                }
            }


        }




        public class MID
        {


            public string NName { get; set; }

            public string VName { get; set; }

            public string Abteilung { get; set; }




            public MID(string VName, string NName, string Abteilung)
            {
                this.VName = VName;

                this.NName = NName;

                this.Abteilung = Abteilung;
            }
        }

        public class InvId
        {
            public string InvNmr { get; set; }

            public string GeraeteName { get; set; }

            public string MitarbeiterBezeichnung { get; set; }

            public InvId(string InvNmr, string GeraeteName, string MitarbeiterBezeichnung)
            {
                this.InvNmr = InvNmr;

                this.GeraeteName = GeraeteName;

                this.MitarbeiterBezeichnung = MitarbeiterBezeichnung;
            }
        }
        public class Accounts
        {
            public string Benutzername { get; set; }
            public Berechtigungen Berechtigung { get; set; }
            public string V1 { get; }
            public string V2 { get; }

            public Accounts(string benutzername, Berechtigungen berechtigung)
            {
                Benutzername = benutzername;
                Berechtigung = berechtigung;
            }

            public Accounts(string v1, string v2)
            {
                V1 = v1;
                V2 = v2;
            }
        }

        

    }
        
    }

