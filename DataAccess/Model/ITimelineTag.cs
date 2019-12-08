using System;
namespace Ostrich.Tracker.DataAccess.Model
{
    public interface ITimelineTag
    {
        string Tag { get; set; }
        ITimeline Timeline { get; set; }
        int TimelineId { get; set; }
    }
}
