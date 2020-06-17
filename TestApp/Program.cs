using Microsoft.Data.SqlClient;
using OrmGift.DataModeling;
using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = Product.CreateDataContext(new SqlConnection(""));
            Console.WriteLine();
        }
    }

    [DataModel]
    public partial class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public uint Number { get; set; }
    }

    [DataModel]
    public readonly partial struct StackExempel
    {
        [DataKey]
        public int CustomId { get; }
        public string Name { get; }
    }
}
