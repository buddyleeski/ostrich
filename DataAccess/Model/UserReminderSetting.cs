using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Tracker.DataAccess.Model
{
    class UserReminderSetting : IUserReminderSetting
    {
        public bool Enabled { get; set; }

        public int Frequency { get; set; }

        public IUser User { get; set; }

        public int UserId { get; set; }
    }
}
