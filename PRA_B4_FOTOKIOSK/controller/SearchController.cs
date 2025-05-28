using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class SearchController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }

        // Start methode die wordt aangeroepen wanneer de zoek pagina opent.
        public void Start()
        {
            // Leeg het zoekveld en het info label
            if (Window != null)
            {
                Window.tbZoeken.Text = "";
                Window.lbSearchInfo.Content = "";
            }
        }

        // Wordt uitgevoerd wanneer er op de Zoeken knop is geklikt
        public void SearchButtonClick()
        {
            if (Window == null) return;

            string zoekText = Window.tbZoeken.Text.Trim();
            Window.lbSearchInfo.Content = ""; // Verwijder eerdere foutmelding direct

            if (string.IsNullOrEmpty(zoekText))
            {
                Window.lbSearchInfo.Content = "Voer een datum en tijd of foto ID in (bv. 2025-05-28 14:23:00 of 12).";
                Window.imgBig.Source = null;
                return;
            }

            var pictures = Window.PictureController.PicturesToDisplay;

            // Probeer eerst als ID te zoeken
            if (int.TryParse(zoekText, out int zoekId))
            {
                var gevondenFoto = pictures.FirstOrDefault(f => f.Id == zoekId);
                if (gevondenFoto != null)
                {
                    ToonFoto(gevondenFoto, $"Foto gevonden: {gevondenFoto.Id}");
                    return;
                }
                else
                {
                    Window.imgBig.Source = null;
                    Window.lbSearchInfo.Content = "Geen foto gevonden met dit ID.";
                    return;
                }
            }

            // Probeer de invoer te parsen als DateTime
            if (!DateTime.TryParse(zoekText, out DateTime zoekMoment))
            {
                Window.lbSearchInfo.Content = "Ongeldige datum/tijd of ID. Gebruik formaat: yyyy-MM-dd HH:mm:ss of een getal voor ID.";
                Window.imgBig.Source = null;
                return;
            }

            // Gebruik reflection om de private GetPhotoTime-methode te benaderen
            var getPhotoTimeMethod = Window.PictureController
                .GetType()
                .GetMethod("GetPhotoTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var gevonden = pictures
                .Select(f => new
                {
                    Photo = f,
                    Time = (DateTime?)getPhotoTimeMethod.Invoke(Window.PictureController, new object[] { f.Source, zoekMoment })
                })
                .Where(x => x.Time.HasValue)
                .OrderBy(x => Math.Abs((x.Time.Value - zoekMoment).TotalSeconds))
                .FirstOrDefault();

            if (gevonden != null && gevonden.Photo != null)
            {
                ToonFoto(gevonden.Photo, $"Foto gevonden: {gevonden.Photo.Id} ({gevonden.Time:HH:mm:ss})");
            }
            else
            {
                Window.imgBig.Source = null;
                Window.lbSearchInfo.Content = "Geen foto gevonden op deze dag/tijd of ID.";
            }
        }

        private void ToonFoto(KioskPhoto foto, string info)
        {
            try
            {
                string path = foto.Source;
                if (!Uri.IsWellFormedUriString(path, UriKind.Absolute))
                    path = Path.GetFullPath(path);

                if (!File.Exists(path))
                {
                    Window.lbSearchInfo.Content = "Bestand niet gevonden: " + path;
                    Window.imgBig.Source = null;
                    return;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                Window.imgBig.Source = bitmap;
                Window.lbSearchInfo.Content = info;
            }
            catch (Exception ex)
            {
                Window.lbSearchInfo.Content = "Kan de foto niet laden: " + ex.Message;
                Window.imgBig.Source = null;
            }
        }
    }
}