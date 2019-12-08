using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Tracker.DataAccess.Model
{
    public class User : IUser
    {

        public string FirstName { get; set; }

        public int Id { get; set; }

        public bool IsAdmin { get; set; }

        public string LastName { get; set; }

        public IList<ISuggestion> Suggestions { get; set; }

        public IList<ITimeline> Timelines { get; set; }

        public string UserName { get; set; }

        public IUserReminderSetting UserReminderSetting { get; set; }
    }
}
