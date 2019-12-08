using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class Timeline
    {
        public virtual int Id { get; set; }
        public virtual string Label { get; set; }
        public virtual IList<TimelineTag> TimelineTags { get; set; }
        public virtual DateTime TimeStamp { get; set; }
        public virtual User User { get; set; }
    }

    public class TimelineMapping : ClassMap<Timeline>
    {
        public TimelineMapping()
        {
            Id(x => x.Id);
            Map(x => x.Label).Length(100);
            Map(x => x.TimeStamp).Not.Nullable();
            References(x => x.User).Column("UserId").Not.Nullable();
            HasMany(x => x.TimelineTags).KeyColumn("TimelineId").Not.LazyLoad().Cascade.All();

            Table("Timeline");
        }
    }
}
