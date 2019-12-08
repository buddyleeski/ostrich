using System;
namespace Ostrich.Tracker.DataAccess.Model
{
    public interface IUserReminderSetting
    {
        bool Enabled { get; set; }
        int Frequency { get; set; }
        IUser User { get; set; }
        int UserId { get; set; }
    }
}
