using Microsoft.Data.SqlClient;
using OrmGift.DataModeling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using var context = Product.CreateDataContext(new SqlConnection(""));

            await context.InsertAsync(new Product(Guid.NewGuid(), "Bertil", 200));
            await context.InsertAsync(new Product(Guid.NewGuid(), "Potatismos", 900));

            List<Product> allProducts = await context.GetAllAsync();

            Guid firstGuid = allProducts.First(x => x.Name == "Bertil").Id;

            Product product = await context.GetAsync(firstGuid);
            Console.WriteLine(product.ToString());

            Guid otherGuid = allProducts.First(x => x.Name == "Potatismos").Id;
            Product product2 = await context.GetAsync(otherGuid);
            Console.WriteLine(product2.ToString());

            Console.WriteLine(product == product2);

            await using var annanContext = AnnanProduct.CreateDataContext(new SqlConnection(""));
            AnnanProduct enAnnanProdukt = await annanContext.GetAsync(10);
            Console.WriteLine(enAnnanProdukt.Value);
        }
    }

    [DataModel("CustomDatabaseName")]
    public partial class Product
    {
        public Guid Id { get; }
        public string Name { get; }
        public int Number { get; }
    }

    [DataModel]
    public partial class AnnanProduct
    {
        [DataKey]
        public int CustomId { get; }
        public object Value { get; set; }
    }
}
