using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    // Deze service zorgt voor de logica rondom producten
    public class ProductService : IProductService
    {
        // Bewaar de repository zodat we hem kunnen gebruiken
        private readonly IProductRepository _productRepository;

        // Constructor: krijgt de repository binnen
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Haal alle producten op
        public List<Product> GetAll()
        {
            // Vraag de repository om alle producten
            return _productRepository.GetAll();
        }

        // Voeg een nieuw product toe
        public Product Add(Product item)
        {
            // Vraag de repository om het product toe te voegen
            // De repository geeft het product terug met een nieuw Id
            return _productRepository.Add(item);
        }

        // Verwijder een product (niet gebruikt in UC19, maar moet wel bestaan)
        public Product? Delete(Product item)
        {
            return _productRepository.Delete(item);
        }

        // Haal één product op (niet gebruikt in UC19, maar moet wel bestaan)
        public Product? Get(int id)
        {
            return _productRepository.Get(id);
        }

        // Update een product
        public Product? Update(Product item)
        {
            return _productRepository.Update(item);
        }
    }
}