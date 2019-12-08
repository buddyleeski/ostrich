using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Tracker.DataAccess.Model
{
    public class TimelineTag : ITimelineTag
    {
        public string Tag { get; set; }

        public ITimeline Timeline { get; set; }

        public int TimelineId { get; set; }
    }
}
