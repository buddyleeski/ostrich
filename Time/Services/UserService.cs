using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Tracker.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Ostrich.Tracker.Services
{
    public class UserService
    {

        public static void AddUser(string email, string first, string last)
        {
            var user = new User()
            {
                UserName = email,
                FirstName = first,
                LastName = last,
                
            };

            user.UserReminderSetting = new UserReminderSetting()
            {
                Enabled = false,
                Frequency = 10,
                User = user
            };

            Helpers.Transaction(s => s.SaveOrUpdate(user));
        }

        public static bool IsUserValid(string username)
        {
            bool isValid = false;

            isValid = Helpers.NonTransact<bool>(s => s.Query<User>().Any(u => u.UserName == username));

            return isValid;
        }

        public static int GetUserId(string username)
        {
            int id;

            id = Helpers.NonTransact<int>(s =>
                    s.Query<User>().Single(user => user.UserName == username).Id);

            return id;
        }

        public static User GetUser(string username)
        {
          User user = Helpers.NonTransact<User>(s =>
                    s.Query<User>().SingleOrDefault(u => u.UserName == username));

            return user;
        }

        public static void UpdateReminderFrequency(int userId, int min)
        {
            Helpers.Transaction(s => 
            {
                var user = s.Query<User>().SingleOrDefault(u => u.Id == userId);

                if (user != null && user.UserReminderSetting != null)
                {
                    user.UserReminderSetting.Frequency = min;
                    s.SaveOrUpdate(user);
                }
            });
        }

        public static List<User> GetAllUsers()
        {
            return Helpers.NonTransact<List<User>>(s => s.Query<User>().ToList());
        }

        public static void UpdateReminderEnabled(int userId, bool enabled)
        {
            Helpers.Transaction(s => 
            {
                var user = s.Query<User>().SingleOrDefault(u => u.Id == userId);
                user.UserReminderSetting.Enabled = enabled;

                s.SaveOrUpdate(user);
            });
        }
    }
}
