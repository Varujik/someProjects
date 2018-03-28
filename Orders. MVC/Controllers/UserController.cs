using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orders.Models;
namespace Orders.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        public ActionResult Index()
        {  
            return View(UserDatabase.usersList);
        }
        public ActionResult DeleteUser(int id)
        {
            User user = UserDatabase.GetUserById(id);
            UserDatabase.DeleteUser(id);
            OrderDatabase.DeleteOrder(user.OrderId);
            return View("Index", UserDatabase.usersList);
        }
        public ActionResult CreateUser()
        {
            return View(ProductDatabase.productsList);
        }
        [HttpPost]
        public ActionResult CreateUser(string Name, string Email, int ProductId)
        {
            User user = new User();
            user.Id = UserDatabase.usersList[UserDatabase.usersList.Count - 1].Id + 1;
            user.Name = Name;
            user.Email = Email;   
            //
            Order order = new Order();
            order.Id = OrderDatabase.ordersList[OrderDatabase.ordersList.Count - 1].Id + 1;
            order.ProductId = ProductId;
            order.UserId = user.Id;
            //
            user.OrderId = order.Id;
            OrderDatabase.ordersList.Add(order);
            UserDatabase.usersList.Add(user);
            return View("Index", UserDatabase.usersList);
        }
        public ActionResult ViewEditUser(User user)
        {
            return View("EditUser", user);
        }
        [HttpPost]
        public ActionResult EditUser(int Id, string Name, string Email, int ProductId)
        {
            User user = UserDatabase.GetUserById(Id);
            user.Name = Name;
            user.Email = Email;
            Order order = OrderDatabase.GetOrderById(user.OrderId);
            order.ProductId = ProductId;
            return View("Index",UserDatabase.usersList);
        }
    }
}
