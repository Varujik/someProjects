using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orders.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int OrderId { get; set; }
        public User()
        {

        }
        public User(int id, string name, string email, int orderId)
        {
            Id = id;
            Name = name;
            Email = email;
            OrderId = orderId;
        }
    }
}