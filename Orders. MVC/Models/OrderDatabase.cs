using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Orders.Models
{
    public class OrderDatabase
    {
        static OrderDatabase()
        {
            CreateOrdersDatabase(10);
        }
        public static List<Order> ordersList = new List<Order>();
        public static void CreateOrdersDatabase(int number)
        {
            for (int i = 0; i < number; i++)
            {
                Order order = new Order();
                order.Id = i;
                order.ProductId = i;
                order.UserId = i;
                ordersList.Add(order);
            }
        }
        public static Order GetOrderById(int id)
        {
            return ordersList.Find(order => order.Id == id);
        }
        public static void DeleteOrder(int id)
        {
            ordersList.Remove(ordersList.Find(order => order.Id == id));
        }
    }
}