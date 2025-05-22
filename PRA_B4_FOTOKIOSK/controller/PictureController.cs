
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

        // Start methode die wordt aangeroepen wanneer de foto pagina opent.
        public void Start()
        {
            // Determine today's folder name based on the current day of the week
            string todayFolder = GetTodayFolder();

            // Initialize the list of photos
            // Load photos only from today's folder
            foreach (string file in Directory.GetFiles(Path.Combine(@"../../../fotos/", todayFolder)))
            {
                /**
                 * file string is de file van de foto. Bijvoorbeeld:
                 * \fotos\0_Zondag\10_05_30_id8824.jpg
                 */
                PicturesToDisplay.Add(new KioskPhoto() { Id = 0, Source = file });
            }

            // Update the photos
            PictureManager.UpdatePictures(PicturesToDisplay);
        }

        // Wordt uitgevoerd wanneer er op de Refresh knop is geklikt
        public void RefreshButtonClick()
        {
            // Re-load photos from today's folder
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
    }
}