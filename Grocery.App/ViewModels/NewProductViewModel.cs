using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly GlobalViewModel _global;

        [ObservableProperty]
        private string productName = "";

        [ObservableProperty]
        private int stock = 0;

        [ObservableProperty]
        private DateTime shelfLifeDateTime = DateTime.Now.AddMonths(1);

        [ObservableProperty]
        private decimal price = 0;

        [ObservableProperty]
        private string message = "";

        public NewProductViewModel(IProductService productService, GlobalViewModel global)
        {
            _productService = productService;
            _global = global;
        }

        [RelayCommand]
        private async Task AddNewProduct()
        {
            // Checks if the user is an admin
            if (_global.Client.Role != Role.Admin)
            {
                Message = "Je hebt geen rechten om producten toe te voegen.";
                return;
            }

            // Check if the product name is empty
            if (string.IsNullOrWhiteSpace(ProductName))
            {
                Message = "Vul een productnaam in.";
                return;
            }

            // Check if the price is greater than 0
            if (Price <= 0)
            {
                Message = "De prijs moet groter zijn dan 0.";
                return;
            }

            // Checks if stock is negative
            if (Stock < 0)
            {
                Message = "Voorraad kan niet negatief zijn.";
                return;
            }

            // Convert DateTime to DateOnly for ShelfLife
            DateOnly shelfLife = DateOnly.FromDateTime(ShelfLifeDateTime);

            // Make a new product
            Product newProduct = new Product(0, ProductName, Stock, shelfLife, Price);

            // Add the new product to the database
            _productService.Add(newProduct);

            Message = "Product succesvol toegevoegd!";

            // Wait for a second and go back to the previous page
            await Task.Delay(1000);
            await Shell.Current.GoToAsync("..");
        }
    }
}