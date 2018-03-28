using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orders.Models
{
    public class ProductDatabase
    {
        static ProductDatabase()
        {
            CreateProductDatabase(10);
        }
        public static List<Product> productsList = new List<Product>();
        static List<string> descriptions = new List<string>()
            {
                "Mobile telephone, new generation",
                "Television for children",
                "Gulchitai in world",
                "Something fresh",
                "Lovely meat",
                "Wanna eat me?",
                "Faster , eat me",
                "Meat fresh, faster man",
                "Oh no, don't wanna be eaten",
                "Yes sir, eat me"
            };
        public static void CreateProductDatabase(int number)
        {
            for (int i = 0; i < number; i++)
            {
                Product product = new Product();
                product.Id = i;
                product.Name = descriptions[i].Split(' ')[0];
                product.Description = descriptions[i];
                product.Price = (i + 1) * 12;
                productsList.Add(product);
            }
        }
        public static Product GetProductById(int id)
        {
            return productsList.Find(product => product.Id == id);
        }
        public static void DeleteProduct(int id)
        {
            productsList.Remove(productsList.Find(product => product.Id == id));
        }
    }
}