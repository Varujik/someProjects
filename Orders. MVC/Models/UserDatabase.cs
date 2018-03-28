using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Orders.Models
{
    public class UserDatabase
    {
        static UserDatabase()
        {
            CreateUsersDatabase(10);
        }
        public static List<User> usersList = new List<User>();
        public static void CreateUsersDatabase(int number)
        {
            for (int i = 0; i < number; i++)
            {
                User user = new User();
                user.Id = i;
                user.Name = string.Format("User №: {0}", i);
                user.Email = string.Format("someone-{0}@google.com", i);
                user.OrderId = i;
                usersList.Add(user);
            }
        }
        public static User GetUserById(int id)
        {
            return usersList.Find(user => user.Id == id);
        }
        public static void DeleteUser(int id)
        {
            usersList.Remove(usersList.Find(user => user.Id == id));
        }
    }
}