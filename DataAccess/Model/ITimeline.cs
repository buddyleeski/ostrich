using System;
using System.Collections.Generic;
namespace Ostrich.Tracker.DataAccess.Model
{
    public interface ITimeline
    {
        int Id { get; set; }
        string Label { get; set; }
        IList<ITimelineTag> TimelineTags { get; set; }
        DateTime? TimeStamp { get; set; }
        User User { get; set; }
        int UserId { get; set; }
    }
}
