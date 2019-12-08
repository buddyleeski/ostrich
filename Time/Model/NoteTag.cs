using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class NoteTag
    {
        public virtual int Id { get; set; }
        public virtual string Tag { get; set; }
        public virtual Note Note { get; set; }
    }

    public class NoteTagMapping : ClassMap<NoteTag>
    {
        public NoteTagMapping()
        {
            Id(x => x.Id);
            Map(x => x.Tag).Not.Nullable().Length(100);
            References(x => x.Note).Column("NoteId").Not.Nullable();

            Table("NoteTag");
        }
    }
}
