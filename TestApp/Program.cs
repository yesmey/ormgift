using Microsoft.Data.SqlClient;
using OrmGift.DataModeling;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await using var context = Product.CreateDataContext(new SqlConnection("secret :)"));
            var allProducts = await context.GetAllAsync();
            var guid = allProducts.First(x => x.Name == "Jesper").Id;
            var product = await context.GetAsync(guid);
            Console.WriteLine(product.ToString());

            var otherGuid = allProducts.First(x => x.Name != "Jesper").Id;
            var product2 = await context.GetAsync(otherGuid);
            Console.WriteLine(product2.ToString());

            Console.WriteLine(product == product2);
        }
    }

    [DataModel("CustomDatabaseName")]
    public partial class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public uint Number { get; set; }
    }

    [DataModel]
    public partial class AnnanProduct
    {
        [DataKey]
        public int CustomId { get; }
        public object Value { get; set; }
    }
}
