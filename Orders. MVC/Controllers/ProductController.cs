using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orders.Models;
namespace Orders.Controllers
{
    public class ProductController : Controller
    {
        //
        // GET: /Product/

        public ActionResult Index()
        {
            return View(ProductDatabase.productsList);
        }
        public ActionResult ProductTable()
        {
            return PartialView("ProductTable", ProductDatabase.productsList);
        }
        public ActionResult DeleteProduct(int id)
        {
            ProductDatabase.DeleteProduct(id);
            return View("Index", ProductDatabase.productsList);
        }
        public ActionResult CreateProduct()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateProduct(string Name, string Description, int Price)
        {
            Product product = new Product();
            product.Id = ProductDatabase.productsList[ProductDatabase.productsList.Count - 1].Id + 1;
            product.Name = Name;
            product.Description = Description;
            product.Price = Price;
            ProductDatabase.productsList.Add(product);
            return View("Index", ProductDatabase.productsList);
        }
        public ActionResult ViewEditProduct(int Id, string Name, string Description, int Price)
        {
            //Response.Write(Id + " " + Name + " " + Description + " " + Price);
            Product product = new Product(Id, Name, Description, Price);
            return View("EditProduct", product);
        }
        [HttpPost]
        public ActionResult EditProduct(int Id, string Name, string Description, int Price)
        {
            Product product = ProductDatabase.GetProductById(Id);
            product.Name = Name;
            product.Description = Description;
            product.Price = Price;
            return View("Index", ProductDatabase.productsList);
        }
    }
}