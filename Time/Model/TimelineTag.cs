using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class TimelineTag
    {
        public virtual int Id { get; set; }
        public virtual string Tag { get; set; }
        public virtual Timeline Timeline { get; set; }

    }

    public class TimelineTagMapping : ClassMap<TimelineTag>
    {
        public TimelineTagMapping()
        {
            Id(x => x.Id);
            Map(x => x.Tag).Not.Nullable().Length(100);
            References(x => x.Timeline).Column("TimelineId").Not.Nullable();

            Table("TimelineTag");
        }
    }
}
