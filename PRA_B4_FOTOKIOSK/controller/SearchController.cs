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
        public static Home Window { get; set; } // Houdt een referentie naar het hoofdvenster

        // Start methode die wordt aangeroepen wanneer de zoek pagina opent.
        public void Start()
        {
            // Leeg het zoekveld en het info label
            if (Window != null) // Controleer of het venster bestaat
            {
                Window.tbZoeken.Text = ""; // Zet het zoekveld leeg
                Window.lbSearchInfo.Content = ""; // Zet het info label leeg
            }
        }

        // Wordt uitgevoerd wanneer er op de Zoeken knop is geklikt
        public void SearchButtonClick()
        {
            // Controleer of het Window object bestaat
            if (Window == null) return; // Stop als het venster niet bestaat

            // Haal de tekst uit het zoekveld en trim spaties
            string zoekText = Window.tbZoeken.Text.Trim(); // Haal en trim de zoektekst
            Window.lbSearchInfo.Content = ""; // Maak het info label leeg
            Window.imgBig.Source = null; // Maak de grote afbeelding leeg

            // Controleer of het zoekveld leeg is
            if (string.IsNullOrEmpty(zoekText)) // Check of er iets is ingevuld
            {
                Window.lbSearchInfo.Content = "Voer een datum en tijd of foto ID in (bv. 2025-05-28 14:23:00 of 12)."; // Toon instructie
                return; // Stop de methode
            }

            // Haal de lijst met foto's op die getoond worden
            var pictures = Window.PictureController.PicturesToDisplay; // Verkrijg de te tonen foto's

            // Zoek op ID als het zoekveld een getal bevat
            if (int.TryParse(zoekText, out int zoekId)) // Probeer de zoektekst als getal te interpreteren
            {
                // Zoek de foto met het opgegeven ID
                var foto = pictures.FirstOrDefault(f => f.Id == zoekId); // Zoek foto met dit ID
                if (foto != null) // Als foto gevonden
                {
                    // Toon de gevonden foto
                    ToonFoto(foto, $"Foto gevonden: {foto.Id}"); // Toon foto en info
                }
                else
                {
                    // Geen foto gevonden met dit ID
                    Window.lbSearchInfo.Content = "Geen foto gevonden met dit ID."; // Toon foutmelding
                }
                return; // Stop de methode
            }

            // Zoek op datum/tijd als het zoekveld een geldige datum/tijd bevat
            if (DateTime.TryParse(zoekText, out DateTime zoekMoment)) // Probeer de zoektekst als datum/tijd te interpreteren
            {
                // Haal de private methode GetPhotoTime op via reflectie
                var getPhotoTime = Window.PictureController
                    .GetType()
                    .GetMethod("GetPhotoTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // Vind de private methode

                // Zoek de foto die het dichtst bij het opgegeven tijdstip ligt
                var gevonden = pictures
                    .Select(f => new
                    {
                        Photo = f, // De foto zelf
                        Time = (DateTime?)getPhotoTime.Invoke(Window.PictureController, new object[] { f.Source, zoekMoment }) // Haal de tijd van de foto op
                    })
                    .Where(x => x.Time.HasValue) // Alleen foto's met een geldige tijd
                    .OrderBy(x => Math.Abs((x.Time.Value - zoekMoment).TotalSeconds)) // Sorteer op tijdsverschil
                    .FirstOrDefault(); // Pak de dichtstbijzijnde

                if (gevonden?.Photo != null) // Als er een foto gevonden is
                {
                    // Toon de gevonden foto met tijd
                    ToonFoto(gevonden.Photo, $"Foto gevonden: {gevonden.Photo.Id} ({gevonden.Time:HH:mm:ss})"); // Toon foto en tijd
                }
                else
                {
                    // Geen foto gevonden op deze dag/tijd of ID
                    Window.lbSearchInfo.Content = "Geen foto gevonden op deze dag/tijd of ID."; // Toon foutmelding
                }
                return; // Stop de methode
            }

            // Ongeldige invoer
            Window.lbSearchInfo.Content = "Ongeldige datum/tijd of ID. Gebruik formaat: yyyy-MM-dd HH:mm:ss of een getal voor ID."; // Toon foutmelding
        }

        // Methode om een foto te tonen in het grote afbeeldingsvak
        private void ToonFoto(KioskPhoto foto, string info)
        {
            try
            {
                string path = foto.Source; // Pad naar de foto
                                           // Controleer of het pad een geldige absolute URI is, anders converteer naar volledig pad
                if (!Uri.IsWellFormedUriString(path, UriKind.Absolute)) // Check of het pad een absolute URI is
                    path = Path.GetFullPath(path); // Zet om naar volledig pad

                // Controleer of het bestand bestaat
                if (!File.Exists(path)) // Check of het bestand bestaat
                {
                    Window.lbSearchInfo.Content = "Bestand niet gevonden: " + path; // Toon foutmelding
                    Window.imgBig.Source = null; // Maak afbeelding leeg
                    return; // Stop de methode
                }

                // Laad de afbeelding in een BitmapImage
                var bitmap = new BitmapImage(); // Maak een nieuwe BitmapImage aan
                bitmap.BeginInit(); // Start initialisatie
                bitmap.UriSource = new Uri(path, UriKind.Absolute); // Zet het pad als bron
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Laad direct in het geheugen
                bitmap.EndInit(); // Beëindig initialisatie
                Window.imgBig.Source = bitmap; // Zet de afbeelding in het grote afbeeldingsvak
                Window.lbSearchInfo.Content = info; // Toon info over de foto
            }
            catch (Exception ex) // Als er een fout optreedt
            {
                // Fout bij het laden van de foto
                Window.lbSearchInfo.Content = "Kan de foto niet laden: " + ex.Message; // Toon foutmelding
                Window.imgBig.Source = null; // Maak afbeelding leeg
            }
        }
    }
}   