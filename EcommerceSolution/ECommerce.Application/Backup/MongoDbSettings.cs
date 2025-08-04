using MongoDB.Driver;
using ECommerce.Domain.Entities; // Para as entidades que serão salvas
using Microsoft.Extensions.Options; // Para IOptions
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ECommerce.Infrastructure.Backup
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "ECommerce";
        public string ProductsCollectionName { get; set; } = "ProductsBackup";
        public string OrdersCollectionName { get; set; } = "OrdersBackup";
        public string ReviewsCollectionName { get; set; } = "ReviewsBackup";
        // Adicione outras coleções para backup conforme necessário
    }

    public interface IMongoDbBackupService
    {
        Task BackupProductsAsync(IEnumerable<Product> products);
        Task BackupOrdersAsync(IEnumerable<Order> orders);
        Task BackupReviewsAsync(IEnumerable<Review> reviews);
        // ... outros backups
    }

    public class MongoDbBackupService : IMongoDbBackupService
    {
        private readonly IMongoDatabase _database;

        public MongoDbBackupService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public async Task BackupProductsAsync(IEnumerable<Product> products)
        {
            var collection = _database.GetCollection<Product>(nameof(Product) + "Backup"); // Usa o nome da entidade para a coleção
            await collection.InsertManyAsync(products);
        }

        public async Task BackupOrdersAsync(IEnumerable<Order> orders)
        {
            var collection = _database.GetCollection<Order>(nameof(Order) + "Backup");
            await collection.InsertManyAsync(orders);
        }

        public async Task BackupReviewsAsync(IEnumerable<Review> reviews)
        {
            var collection = _database.GetCollection<Review>(nameof(Review) + "Backup");
            await collection.InsertManyAsync(reviews);
        }

        // Implemente outros métodos de backup para outras entidades
    }
}