using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class Note
    {
        public virtual int Id { get; set; }
        public virtual User User { get; set; }
        public virtual string Label { get; set; }
        public virtual DateTime TimeStamp { get; set; }
        public virtual IList<NoteTag> NoteTags { get; set; }
    }

    public class NoteMapping : ClassMap<Note>
    {
        public NoteMapping()
        {
            Id(x => x.Id);
            Map(x => x.Label).Length(500).Not.Nullable();
            Map(x => x.TimeStamp).Not.Nullable();
            References(x => x.User).Column("UserId");
            HasMany(x => x.NoteTags).KeyColumn("NoteId").Not.LazyLoad().Cascade.All();

            Table("Note");
        }
    }
}
