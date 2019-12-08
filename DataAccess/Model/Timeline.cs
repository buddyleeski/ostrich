using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Tracker.DataAccess.Model
{
    public class Timeline : ITimeline
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public IList<ITimelineTag> TimelineTags { get; set; }

        public DateTime? TimeStamp { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }
    }
}
