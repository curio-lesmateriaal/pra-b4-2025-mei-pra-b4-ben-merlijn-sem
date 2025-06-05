using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController // Definieert de PictureController klasse
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; } // Statische property voor het hoofdvenster

        // De lijst met fotos die we laten zien
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>(); // Lijst van foto's die getoond worden

        public void Start() // Methode om te starten met het laden van foto's
        {
            string todayFolder = GetTodayFolder(); // Haalt de map van vandaag op
            var now = DateTime.Now; // Huidige tijd
            var minTime = now.AddMinutes(-30); // Minimale tijd (30 minuten geleden)
            var maxTime = now.AddMinutes(-2); // Maximale tijd (2 minuten geleden)

            var allPhotos = new List<KioskPhoto>(); // Lijst voor alle gevonden foto's

            // Laad en filter foto's uit de juiste map
            foreach (string file in Directory.GetFiles(Path.Combine(@"../../../fotos/", todayFolder))) // Loopt door alle bestanden in de map van vandaag
            {
                DateTime? fotoTijd = GetPhotoTime(file, now); // Haalt de tijd van de foto op

                if (fotoTijd.HasValue && fotoTijd.Value >= minTime && fotoTijd.Value <= maxTime) // Controleert of de foto binnen het tijdsvenster valt
                {
                    allPhotos.Add(new KioskPhoto() // Voegt de foto toe aan de lijst
                    {
                        Id = GetPhotoId(file), // Haalt het ID van de foto op
                        Source = file // Zet het pad van de foto
                    });
                }
            }

            // Sorteer op tijd
            allPhotos = allPhotos.OrderBy(p => GetPhotoTime(p.Source, now)).ToList(); // Sorteert de foto's op tijd

            // Groepeer foto's die 60 seconden verschillen
            var ordered = new List<KioskPhoto>(); // Nieuwe lijst voor geordende foto's
            var used = new HashSet<string>(); // HashSet om gebruikte foto's bij te houden

            for (int i = 0; i < allPhotos.Count; i++) // Loopt door alle foto's
            {
                var foto1 = allPhotos[i]; // Haalt de eerste foto op
                if (used.Contains(foto1.Source)) continue; // Slaat over als de foto al gebruikt is

                DateTime? tijd1 = GetPhotoTime(foto1.Source, now); // Haalt de tijd van de eerste foto op
                ordered.Add(foto1); // Voegt de eerste foto toe aan de geordende lijst
                used.Add(foto1.Source); // Markeert de foto als gebruikt

                for (int j = i + 1; j < allPhotos.Count; j++) // Loopt door de volgende foto's
                {
                    var foto2 = allPhotos[j]; // Haalt de tweede foto op
                    if (used.Contains(foto2.Source)) continue; // Slaat over als de foto al gebruikt is

                    DateTime? tijd2 = GetPhotoTime(foto2.Source, now); // Haalt de tijd van de tweede foto op

                    if (tijd1.HasValue && tijd2.HasValue && (tijd2.Value - tijd1.Value).TotalSeconds == 60) // Controleert of het tijdsverschil 60 seconden is
                    {
                        ordered.Add(foto2); // Voegt de tweede foto toe aan de geordende lijst
                        used.Add(foto2.Source); // Markeert de foto als gebruikt
                        break; // Stopt de inner loop
                    }
                }
            }

            PicturesToDisplay = ordered; // Zet de geordende lijst als de te tonen foto's
            PictureManager.UpdatePictures(PicturesToDisplay); // Update de foto's op het scherm
        }

        public void RefreshButtonClick() // Methode die wordt aangeroepen bij het klikken op de refresh-knop
        {
            PicturesToDisplay.Clear(); // Leegt de lijst met te tonen foto's
            Start(); // Laadt de foto's opnieuw
        }

        private string GetTodayFolder() // Methode om de map van vandaag op te halen
        {
            string folder = string.Empty; // Variabele voor de mapnaam
            switch (DateTime.Now.DayOfWeek) // Switch op de dag van de week
            {
                case DayOfWeek.Sunday: // Zondag
                    folder = "0_Zondag"; // Zet mapnaam
                    break;
                case DayOfWeek.Monday: // Maandag
                    folder = "1_Maandag"; // Zet mapnaam
                    break;
                case DayOfWeek.Tuesday: // Dinsdag
                    folder = "2_Dinsdag"; // Zet mapnaam
                    break;
                case DayOfWeek.Wednesday: // Woensdag
                    folder = "3_Woensdag"; // Zet mapnaam
                    break;
                case DayOfWeek.Thursday: // Donderdag
                    folder = "4_Donderdag"; // Zet mapnaam
                    break;
                case DayOfWeek.Friday: // Vrijdag
                    folder = "5_Vrijdag"; // Zet mapnaam
                    break;
                case DayOfWeek.Saturday: // Zaterdag
                    folder = "6_Zaterdag"; // Zet mapnaam
                    break;
            }
            return folder; // Geeft de mapnaam terug
        }

        private DateTime? GetPhotoTime(string file, DateTime now) // Methode om de tijd van een foto op te halen
        {
            string naam = Path.GetFileNameWithoutExtension(file); // Haalt de bestandsnaam zonder extensie op
            string[] parts = naam.Split('_'); // Splitst de naam op underscores

            if (parts.Length >= 4 && // Controleert of er genoeg delen zijn
                int.TryParse(parts[0], out int uur) && // Probeert het uur te parsen
                int.TryParse(parts[1], out int minuut) && // Probeert de minuut te parsen
                int.TryParse(parts[2], out int seconde)) // Probeert de seconde te parsen
            {
                return new DateTime(now.Year, now.Month, now.Day, uur, minuut, seconde); // Maakt een nieuwe DateTime met de parsed waarden
            }

            return null; // Geeft null terug als het niet lukt
        }

        private int GetPhotoId(string file) // Methode om het ID van een foto op te halen
        {
            string naam = Path.GetFileNameWithoutExtension(file); // Haalt de bestandsnaam zonder extensie op
            string[] parts = naam.Split(new[] { "_id" }, StringSplitOptions.None); // Splitst de naam op "_id"

            if (parts.Length == 2 && int.TryParse(parts[1], out int id)) // Controleert of het tweede deel een getal is
            {
                return id; // Geeft het ID terug
            }

            return 0; // Geeft 0 terug als het niet lukt
        }
    }
}
