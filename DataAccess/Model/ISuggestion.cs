using System;
namespace Ostrich.Tracker.DataAccess.Model
{
    public interface ISuggestion
    {
        int Id { get; set; }
        string Label { get; set; }
        DateTime TimeStamp { get; set; }
        IUser User { get; set; }
        int UserId { get; set; }
    }
}
