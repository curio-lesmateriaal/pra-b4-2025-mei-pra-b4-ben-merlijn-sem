using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home Window { get; set; }
        private List<(string ProductName, double Price, int Amount)> receiptItems = new List<(string, double, int)>();

        public void Start()
        {
            // Stel de prijslijst in aan de rechterkant.
            ShopManager.SetShopPriceList("Prijzen:\nFoto 10x15: $2.50\nFoto 20x30: $4.95\nMok met foto: $9.95\nSleutelhanger met foto: $6.12\nT-shirt met foto: $11.99");

            // Stel de bon in onderaan het scherm
            ShopManager.SetShopReceipt("Eindbedrag\n€0.00");

            // Vul de productlijst met producten en hun prijzen
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 10x15", Price = 2.50 });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 20x30", Price = 4.95 });
            ShopManager.Products.Add(new KioskProduct() { Name = "Mok met foto", Price = 9.95 });
            ShopManager.Products.Add(new KioskProduct() { Name = "Sleutelhanger met foto", Price = 6.12 });
            ShopManager.Products.Add(new KioskProduct() { Name = "T-shirt met foto", Price = 11.99 });

            // Update dropdown met producten
            ShopManager.UpdateDropDownProducts();
        }

        // Wordt uitgevoerd wanneer er op de Toevoegen knop is geklikt
        public void AddButtonClick()
        {
            var product = ShopManager.GetSelectedProduct();
            var fotoId = ShopManager.GetFotoId();
            var amount = ShopManager.GetAmount();

            if (product != null && fotoId != null && amount != null)
            {
                double total = amount.Value * product.Price;
                receiptItems.Add((product.Name, product.Price, amount.Value));
                UpdateReceipt();
            }
            else
            {
                MessageBox.Show("Controleer of alle velden correct zijn ingevuld.");
            }
        }

        private void UpdateReceipt()
        {
            StringBuilder receiptBuilder = new StringBuilder();
            double totalAmount = 0;

            foreach (var item in receiptItems)
            {
                double itemTotal = item.Price * item.Amount;
                receiptBuilder.AppendLine($"{item.Amount} x {item.ProductName}: €{itemTotal:F2}");
                totalAmount += itemTotal;
            }

            receiptBuilder.AppendLine($"Eindbedrag\n€{totalAmount:F2}");
            ShopManager.SetShopReceipt(receiptBuilder.ToString());
        }

        // Wordt uitgevoerd wanneer er op de Resetten knop is geklikt
        public void ResetButtonClick()
        {
            receiptItems.Clear();
            ShopManager.SetShopReceipt("Eindbedrag\n€0.00");
        }

        // Wordt uitgevoerd wanneer er op de Save knop is geklikt
        public void SaveButtonClick()
        {

        }
    }
}
