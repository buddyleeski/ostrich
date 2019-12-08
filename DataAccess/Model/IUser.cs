using System;
using System.Collections.Generic;
namespace Ostrich.Tracker.DataAccess.Model
{
    public interface IUser
    {
        string FirstName { get; set; }
        int Id { get; set; }
        bool IsAdmin { get; set; }
        string LastName { get; set; }
        IList<ISuggestion> Suggestions { get; set; }
        IList<ITimeline> Timelines { get; set; }
        string UserName { get; set; }
        IUserReminderSetting UserReminderSetting { get; set; }
    }
}
