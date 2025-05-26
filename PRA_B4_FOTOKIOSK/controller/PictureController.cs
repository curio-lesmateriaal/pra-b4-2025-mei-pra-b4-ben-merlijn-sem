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
    public class PictureController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }

        // De lijst met fotos die we laten zien
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();

        public void Start()
        {
            string todayFolder = GetTodayFolder();
            var now = DateTime.Now;
            var minTime = now.AddMinutes(-30);
            var maxTime = now.AddMinutes(-2);

            var allPhotos = new List<KioskPhoto>();

            // Laad en filter foto's uit de juiste map
            foreach (string file in Directory.GetFiles(Path.Combine(@"../../../fotos/", todayFolder)))
            {
                DateTime? fotoTijd = GetPhotoTime(file, now);

                if (fotoTijd.HasValue && fotoTijd.Value >= minTime && fotoTijd.Value <= maxTime)
                {
                    allPhotos.Add(new KioskPhoto()
                    {
                        Id = GetPhotoId(file),
                        Source = file
                    });
                }
            }

            // Sorteer op tijd
            allPhotos = allPhotos.OrderBy(p => GetPhotoTime(p.Source, now)).ToList();

            // Groepeer foto's die 60 seconden verschillen
            var ordered = new List<KioskPhoto>();
            var used = new HashSet<string>();

            for (int i = 0; i < allPhotos.Count; i++)
            {
                var foto1 = allPhotos[i];
                if (used.Contains(foto1.Source)) continue;

                DateTime? tijd1 = GetPhotoTime(foto1.Source, now);
                ordered.Add(foto1);
                used.Add(foto1.Source);

                for (int j = i + 1; j < allPhotos.Count; j++)
                {
                    var foto2 = allPhotos[j];
                    if (used.Contains(foto2.Source)) continue;

                    DateTime? tijd2 = GetPhotoTime(foto2.Source, now);

                    if (tijd1.HasValue && tijd2.HasValue && (tijd2.Value - tijd1.Value).TotalSeconds == 60)
                    {
                        ordered.Add(foto2);
                        used.Add(foto2.Source);
                        break;
                    }
                }
            }

            PicturesToDisplay = ordered;
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        public void RefreshButtonClick()
        {
            PicturesToDisplay.Clear();
            Start();
        }

        private string GetTodayFolder()
        {
            string folder = string.Empty;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    folder = "0_Zondag";
                    break;
                case DayOfWeek.Monday:
                    folder = "1_Maandag";
                    break;
                case DayOfWeek.Tuesday:
                    folder = "2_Dinsdag";
                    break;
                case DayOfWeek.Wednesday:
                    folder = "3_Woensdag";
                    break;
                case DayOfWeek.Thursday:
                    folder = "4_Donderdag";
                    break;
                case DayOfWeek.Friday:
                    folder = "5_Vrijdag";
                    break;
                case DayOfWeek.Saturday:
                    folder = "6_Zaterdag";
                    break;
            }
            return folder;
        }

        private DateTime? GetPhotoTime(string file, DateTime now)
        {
            string naam = Path.GetFileNameWithoutExtension(file);
            string[] parts = naam.Split('_');

            if (parts.Length >= 4 &&
                int.TryParse(parts[0], out int uur) &&
                int.TryParse(parts[1], out int minuut) &&
                int.TryParse(parts[2], out int seconde))
            {
                return new DateTime(now.Year, now.Month, now.Day, uur, minuut, seconde);
            }

            return null;
        }

        private int GetPhotoId(string file)
        {
            string naam = Path.GetFileNameWithoutExtension(file);
            string[] parts = naam.Split(new[] { "_id" }, StringSplitOptions.None);

            if (parts.Length == 2 && int.TryParse(parts[1], out int id))
            {
                return id;
            }

            return 0;
        }
    }
}
