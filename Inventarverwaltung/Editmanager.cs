using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Inventarverwaltung
{
   public static class Editmanager
    {
        public static void ZeigeBearbeitungsMenu()
        {
            while (true)
            {
                Console.Clear();
                ConsoleHelper.PrintSectionHeader("Bearbeitungs-Menü", ConsoleColor.DarkYellow);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Was möchten Sie bearbeiten?");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  [1] 📦 Artikel bearbeiten");
                Console.WriteLine("  [2] 👥 Mitarbeiter bearbeiten");
                Console.WriteLine("  [3] 👨‍💼 Benutzer bearbeiten");
                Console.WriteLine();
                Console.WriteLine("  [0] ↩️  Zurück zum Hauptmenü");
                Console.ResetColor();

                Console.WriteLine();
                string auswahl = ConsoleHelper.GetInput("Ihre Auswahl");

                switch (auswahl)
                {
                    case "1":
                        BearbeiteArtikel();
                        break;
                    case "2":
                        BearbeiteMitarbeiter();
                        break;
                    case "3":
                        BearbeiteBenutzer();
                        break;
                    case "0":
                        return;
                }
            }
        }

        #region Artikel Bearbeiten

        public static void BearbeiteArtikel()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Artikel bearbeiten", ConsoleColor.DarkYellow);

            if(DataManager.Inventar.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Artikel vorhanden!");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Artikel:");
            Console.ResetColor();
            Console.WriteLine();

            for(int i = 0; i < DataManager.Inventar.Count; i++)
            {
                var artikel  = DataManager.Inventar[i];
                Console.WriteLine($"[{i + 1}] {artikel.InvNmr} - {artikel.GeraeteName} - {artikel.Anzahl} STK ");
            }
            Console.WriteLine("");
            string auswahl = ConsoleHelper.GetInput("Artikel-Nummer oder Inventar-nr ( Oder 'X' zum Abbrechen)");

            if (auswahl.ToLower() == "X") return;

            InvId zuBerarbeitenderArtikel = null;

            if(int.TryParse(auswahl, out int nummer) && nummer > 0 && nummer <= DataManager.Inventar.Count)
            {
                zuBerarbeitenderArtikel = DataManager.Inventar[nummer - 1];
            }
            else
            {
                zuBerarbeitenderArtikel =DataManager.Inventar.FirstOrDefault(a => a.InvNmr.Equals(auswahl, StringComparison.OrdinalIgnoreCase));
            }

            if(zuBerarbeitenderArtikel == null)
            {
                ConsoleHelper.PrintError("Artikel nicht gefunden");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    AKTUELLE DATEN                                 ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  [1] Inventar-Nr:      {zuBerarbeitenderArtikel.InvNmr}");
            Console.WriteLine($"  [2] Gerätename:       {zuBerarbeitenderArtikel.GeraeteName}");
            Console.WriteLine($"  [3] Mitarbeiter:      {zuBerarbeitenderArtikel}");
            Console.WriteLine($"  [4] Seriennummer:     {zuBerarbeitenderArtikel.SerienNummer}");
            Console.WriteLine($"  [5] Preis:            {zuBerarbeitenderArtikel.Preis:F2}€");
            Console.WriteLine($"  [6] Anschaffung:      {zuBerarbeitenderArtikel.Anschaffungsdatum:dd.MM.yyyy}");
            Console.WriteLine($"  [7] Hersteller:       {zuBerarbeitenderArtikel.Hersteller}");
            Console.WriteLine($"  [8] Kategorie:        {zuBerarbeitenderArtikel.Kategorie}");
            Console.WriteLine($"  [9] Anzahl:           {zuBerarbeitenderArtikel.Anzahl}");
            Console.WriteLine($"  [10] Mindestbestand:  {zuBerarbeitenderArtikel.Mindestbestand}");
            Console.WriteLine();

            string feld = ConsoleHelper.GetInput("Welches Feld möchten Sie ändern? ( 1- 10 oder 'X' zum Abbrechen)");

            if (feld.ToLower() == "x") return;

            bool geaendert = false;
            string altWert = "";
            string neuWert = "";

            switch ( feld)
            {
                case "1":
                    string neueInvNr = ConsoleHelper.GetInput($"Neue Inventar-NR (aktuell: {altWert}");
                    if(!string.IsNullOrWhiteSpace(neueInvNr) && !neueInvNr.Equals(altWert))
                    {
                        if(DataManager.Inventar.Any(a => a.InvNmr.Equals(neueInvNr, StringComparison.OrdinalIgnoreCase)))
                        {
                            ConsoleHelper.PrintError("Diese Inevntar-Nr exestiert bereits!");
                            ConsoleHelper.PressKeyToContinue();
                            return;
                        }
                        zuBerarbeitenderArtikel.InvNmr = neueInvNr;
                        neuWert = neueInvNr;
                        geaendert =true;
                    } 
                    break;

                    case "2":
                    altWert = zuBerarbeitenderArtikel.GeraeteName;
                        string neuerName = ConsoleHelper.GetInput($" Neuer Gerätename (aktuell: {altWert}");
                    if(!string.IsNullOrWhiteSpace(neuerName) && !neuerName.Equals(altWert))
                    {
                        zuBerarbeitenderArtikel.GeraeteName = neuerName;
                        neuWert = neuerName;
                        geaendert =true;
                    }
                    break;

                    case "3":
                    altWert = zuBerarbeitenderArtikel.MitarbeiterBezeichnung;
                    Console.WriteLine("");
                    ConsoleHelper.PrintInfo($"Verfügbare Mitarbeiter");
                    foreach(var m in DataManager.Mitarbeiter)
                    {
                        Console.WriteLine($"    .{m.VName} {m.NName}");
                    }
                    string neuerMitarbeiter = ConsoleHelper.GetInput($"Neuer Mitarbeiter aktuell {altWert})");
                    if(!string.IsNullOrWhiteSpace(neuerMitarbeiter) && !neuerMitarbeiter.Equals(altWert))
                    {
                        zuBerarbeitenderArtikel.MitarbeiterBezeichnung = neuerMitarbeiter;
                        neuWert = neuerMitarbeiter;
                        geaendert =true;
                    } 
                    break;

                    case "4":
                    altWert = zuBerarbeitenderArtikel.SerienNummer;
                    string neueSNR = ConsoleHelper.GetInput($"Neue Seriennummer (aktuell: {altWert})");
                    if(!string.IsNullOrWhiteSpace(neueSNR) && !neueSNR.Equals(altWert))
                    {
                        zuBerarbeitenderArtikel.SerienNummer = neueSNR;
                        neuWert = neueSNR;
                        geaendert =true;
                    }
                    break;

                    case "5":
                    altWert = zuBerarbeitenderArtikel.Preis.ToString("F2");
                    string neuerPreis = ConsoleHelper.GetInput($"Neuer Preis in € (aktuell: {altWert})");
                    if (!string.IsNullOrWhiteSpace(neuerPreis))
                    {
                        if(decimal.TryParse(neuerPreis.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preis))
                        {
                            zuBerarbeitenderArtikel.Preis = preis;
                            neuWert = preis.ToString("F2");
                            geaendert=true;

                        }
                        else
                        {
                            ConsoleHelper.PrintError("Ungültiger Preis");
                            ConsoleHelper.PressKeyToContinue();
                            return;
                        }
                    }
                    break ;

                    case "6":
                    altWert = zuBerarbeitenderArtikel.Anschaffungsdatum.ToString("dd.MM.yyyy");
                    string neuesDatum = ConsoleHelper.GetInput($"Neues Datum (TT.MM.JJJJ) (aktuell: {altWert})");
                    if(!string.IsNullOrWhiteSpace(neuesDatum))
                    {
                        if(DateTime.TryParseExact(neuesDatum, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum))
                        {
                            zuBerarbeitenderArtikel.Anschaffungsdatum = datum;
                            neuWert = datum.ToString("dd.MM.yyyy");
                            geaendert=true;
                        }
                        else
                        {
                            ConsoleHelper.PrintError("Ungültiges Datum");
                            ConsoleHelper.PressKeyToContinue();
                            return;
                        }
                    }
                    break ;

                case "7":
                    altWert = zuBerarbeitenderArtikel.Kategorie;
                    string neuerHersteller = ConsoleHelper.GetInput($"Neuer Hersteller: (aktuell: {altWert})");
                    if(!string.IsNullOrWhiteSpace (neuerHersteller) && !neuerHersteller.Equals(altWert))
                    {
                        zuBerarbeitenderArtikel.Hersteller = neuerHersteller;
                        neuWert = neuerHersteller;
                        geaendert = true;
                    }
                    break ;

                case "8":
                    altWert = zuBerarbeitenderArtikel.Kategorie;
                    string neueKategorie = ConsoleHelper.GetInput($"Neue Kategorie (aktuell: {altWert})");
                    if(!string.IsNullOrWhiteSpace(neueKategorie) && neueKategorie.Equals(altWert))
                    {
                        zuBerarbeitenderArtikel.Kategorie = neueKategorie;
                        neuWert=neueKategorie;
                        geaendert = true;
                    }
                    break ;

                case "9":
                    altWert = zuBerarbeitenderArtikel.Anzahl.ToString();
                    string neueAnzahl = ConsoleHelper.GetInput($"Neue Anzahl (aktuell: {altWert})");
                    if (!string.IsNullOrWhiteSpace(neueAnzahl))
                    {
                        if(int.TryParse(neueAnzahl, out int anzahl) && anzahl <= 0)
                        {
                            zuBerarbeitenderArtikel.Anzahl =anzahl;
                            neuWert = anzahl.ToString() ;
                            geaendert=true;
                        }
                        else
                        {
                            ConsoleHelper.PrintError("Ungültige Anzahl");
                            ConsoleHelper.PressKeyToContinue();
                            return;
                        }
                    }
                    break;

                    case "10":
                    altWert = zuBerarbeitenderArtikel.Mindestbestand.ToString();
                    string neuerMindest = ConsoleHelper.GetInput($"Neuer Mindestbestand (aktuell : {altWert})");
                    if (!string.IsNullOrWhiteSpace(neuerMindest))
                    {
                        if(int.TryParse(neuerMindest, out int mindest) && mindest >= 0)
                        {
                            zuBerarbeitenderArtikel.Mindestbestand =mindest;
                            neuWert = mindest.ToString();
                                 geaendert=true;
                        }
                        else
                        {
                            ConsoleHelper.PrintError("Ungültiger Mindestbestand");
                            ConsoleHelper.PressKeyToContinue();
                            return;
                        }
                    }
                    break;


                default:
                    ConsoleHelper.PrintError("Ungültige Auswahl");
                    ConsoleHelper.PressKeyToContinue();
                    return;

            }
            if (geaendert)
            {
                DataManager.SaveKomplettesInventar();

                Console.WriteLine();
                ConsoleHelper.PrintSuccess($"✓ Artikel erfolgreich geändert!");
                Console.WriteLine($"  Alt: {altWert}");
                Console.WriteLine($"  Neu: {neuWert}");


                LogManager.LogDatenGespeichert("Artikel-Beabeitung", $"{zuBerarbeitenderArtikel.InvNmr}: Feld {feld} geändert von: '{altWert}' zu: '{neuWert}'");

                IntelligentAssistant.IniializeAI();
            }
            else
            {
                ConsoleHelper.PrintInfo("Keine Änderungen vorgenommen");
            }
            ConsoleHelper.PressKeyToContinue();

         
            
        }

        #endregion


        #region Mitarbeiter bearbeiten

        public static void BearbeiteMitarbeiter()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Mitarbeiter Bearbeiten", ConsoleColor.DarkYellow);

            if(DataManager.Mitarbeiter.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Mitarbeiter vorhanden");
                ConsoleHelper.PressKeyToContinue();
                return;
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Mitarbeiter:");
            Console.ResetColor();
            Console.WriteLine();

            for(int i= 0; i < DataManager.Mitarbeiter.Count; i++)
            {
                var m = DataManager.Mitarbeiter[i];
                Console.WriteLine($" [{i + 1}] {m.VName} {m.NName} - {m.Abteilung}");
            }
            Console.WriteLine("");
            string auswahl = ConsoleHelper.GetInput("Mitarbeiter-Nummer ( oder 'X' zum Abbrechen)");

            if (auswahl.ToLower() == "X") return;

            if(!int.TryParse(auswahl, out int nummer) || nummer < 1 || nummer > DataManager.Mitarbeiter.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            MID mitarbeiter = DataManager.Mitarbeiter[nummer - 1];

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    AKTUELLE DATEN                                 ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  [1] Vorname:    {mitarbeiter.VName}");
            Console.WriteLine($"  [2] Nachname:   {mitarbeiter.NName}");
            Console.WriteLine($"  [3] Abteilung:  {mitarbeiter.Abteilung}");
            Console.WriteLine();


            string feld = ConsoleHelper.GetInput("Welches Feld möchten Sie ändern ( 1- 3 oder 'X' zum Abbrechen");

            if (feld.ToLower() == "X") return;

            bool geandert = false;
            string altWert = "";
            string neuWert = "";

            switch (feld)
            {
                case "1":
                    altWert = mitarbeiter.VName;
                    string neuerVorname = ConsoleHelper.GetInput($" Neuer Vorname (aktuell: {altWert})");
                    if(!string.IsNullOrWhiteSpace(neuerVorname) && neuerVorname.Equals(altWert))
                    {
                        mitarbeiter.VName = neuerVorname;
                        neuWert = neuerVorname;
                        geandert = true;
                    }
                    else
                    {
                        ConsoleHelper.PrintError("Ungültiger Vorname ");
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                    break;

                    case "2":
                    altWert = mitarbeiter.NName;
                    string neuerNachname = ConsoleHelper.GetInput($"Neuer Nachname (aktuell: {altWert})");
                    if(!string.IsNullOrWhiteSpace(neuerNachname) && neuerNachname.Equals(altWert))
                    {
                        mitarbeiter.NName = neuerNachname;
                        neuWert=neuerNachname;
                        geandert = true;
                    }
                 
                    break;

                    case "3":
                    altWert = mitarbeiter.Abteilung;
                    string neueAbteilung = ConsoleHelper.GetInput($"Neue Abteilung (aktuell: {altWert}");
                    if(!string.IsNullOrWhiteSpace(neueAbteilung) && neueAbteilung.Equals(altWert))
                    {
                        mitarbeiter.Abteilung = neueAbteilung;
                        neuWert = neueAbteilung;
                        geandert = true;
                    }
                    else
                    {
                        ConsoleHelper.PrintError("Ungültige Abteilung");
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                    break;

                default:
                    ConsoleHelper.PrintError("Ungültige Auswahl");
                    ConsoleHelper.PressKeyToContinue();
                    return;
            }

            if (geandert)
            {
                DataManager.SaveKompletteMitarbeiter();

                Console.WriteLine();
                ConsoleHelper.PrintSuccess($"✓ Mitarbeiter erfolgreich geändert!");
                Console.WriteLine($"  Alt: {altWert}");
                Console.WriteLine($"  Neu: {neuWert}");

                LogManager.LogDatenGespeichert("Mitarbeiter-Bearbeitung",
                    $"{mitarbeiter.VName} {mitarbeiter.NName}: Feld {feld} geändert von '{altWert}' zu '{neuWert}'");

                IntelligentAssistant.IniializeAI();
            }
            else
            {
                ConsoleHelper.PrintInfo("Keine Änderung vorgenommen");
            }
            ConsoleHelper.PressKeyToContinue();

        }

        #endregion

        #region Benutzer bearbeiten


        public static void BearbeiteBenutzer()
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Benutzer bearbeiten", ConsoleColor.DarkYellow);

            if(DataManager.Benutzer.Count == 0)
            {
                ConsoleHelper.PrintWarning("Noch keine Benutzer vorhanden");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Verfügbare Benutzer:");
            Console.ResetColor();
            Console.WriteLine();

            for (int i = 0; i < DataManager.Benutzer.Count; i++)
            {
                var b = DataManager.Benutzer[i];
                string icon = b.Berechtigung == Berechtigungen.Admin ? "👑" : "👤";
                Console.WriteLine($" [{i + 1}] {icon} {b.Benutzername} - {b.Berechtigung}");
            }
            Console.WriteLine();
            string auswahl = ConsoleHelper.GetInput($"Benutzer - Nummer ( Oder 'X' zum Abbrechen)");

            if (auswahl.ToLower() == "X") return;

            if(!int.TryParse(auswahl, out int nummer) || nummer < 1 || nummer > DataManager.Benutzer.Count)
            {
                ConsoleHelper.PrintError("Ungültige Auswahl");
                ConsoleHelper.PressKeyToContinue();
                return;
            }

            Accounts benutzer = DataManager.Benutzer[nummer - 1];

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ╔═══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║                    AKTUELLE DATEN                                 ║");
            Console.WriteLine("  ╚═══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"  [1] Benutzername:  {benutzer.Benutzername}");
            Console.WriteLine($"  [2] Berechtigung:  {benutzer.Berechtigung}");
            Console.WriteLine();

            string feld = ConsoleHelper.GetInput("Welches feld möchten Sie ändern ?");

            if(feld.ToLower() == "X") return;

            bool geandert = false;
            string altWert = "";
            string neuWert = "";


            switch (feld)
            {
                case "1":
                    altWert = benutzer.Benutzername;
                    string neuerName = ConsoleHelper.GetInput($"Neuen Benutzernamen eingeben (aktuell: {altWert})");
                    if (!string.IsNullOrWhiteSpace(neuerName) && !neuerName.Equals(altWert))
                    {
                        if (DataManager.Benutzer.Any(b => b.Benutzername.Equals(neuerName, StringComparison.OrdinalIgnoreCase) && b != benutzer))
                        {
                            ConsoleHelper.PrintError("Dieser Benutzername exestiert bereits");
                            ConsoleHelper.PressKeyToContinue();
                            return;
                        }
                        benutzer.Benutzername = neuerName;
                        neuWert = neuerName;
                        geandert = true;
                    }
                    break;

                case "2":
                    altWert = benutzer.Berechtigung.ToString();
                    Console.WriteLine();
                    Console.WriteLine("  [1] 👤 User");
                    Console.WriteLine("  [2] 👑 Admin");
                    Console.WriteLine();

                    string neueberechtigung = ConsoleHelper.GetInput($"Neue berechtigung (aktuell: {altWert})");

                    Berechtigungen berechtigung = benutzer.Berechtigung;

                    if (neueberechtigung == "1")
                    {
                        berechtigung = Berechtigungen.User;
                    }
                    else if (neueberechtigung == "2")
                    {
                        berechtigung = Berechtigungen.Admin;
                    }
                    else
                    {
                        ConsoleHelper.PrintError("Ungültige Auswahl");
                        ConsoleHelper.PressKeyToContinue();
                        return;
                    }
                    if (berechtigung != benutzer.Berechtigung)
                    {
                        benutzer.Berechtigung = berechtigung;
                        neuWert = berechtigung.ToString();
                        geandert = true;
                    }
                    break;

                default:
                    ConsoleHelper.PrintError("Ungültige Auswahl");
                    ConsoleHelper.PressKeyToContinue();
                    return;
            }
            if (geandert)
            {
                DataManager.SaveBenutzerToFile();

                Console.WriteLine();
                ConsoleHelper.PrintSuccess($"✓ Benutzer erfolgreich geändert!");
                Console.WriteLine($"  Alt: {altWert}");
                Console.WriteLine($"  Neu: {neuWert}");

                
                LogManager.LogDatenGespeichert("Benutzer-Bearbeitung",
                    $"{benutzer.Benutzername}: Feld {feld} geändert von '{altWert}' zu '{neuWert}'");
            }
            else
            {
                ConsoleHelper.PrintInfo("Keine Änderung vorgenommen");
            }
            ConsoleHelper.PressKeyToContinue();
        }
        #endregion


    }
}

