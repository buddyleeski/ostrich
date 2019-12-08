using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class Todo
    {
        public virtual int Id {get; set;}
        public virtual User User { get; set; }
        public virtual string Label { get; set; }
        public virtual DateTime DueDate { get; set; }
    }

    public class TodoMapping : ClassMap<Todo>
    {
        public TodoMapping()
        {
            Id(x => x.Id);
            References(x => x.User).ForeignKey("UserId");
            Map(x => x.Label).Length(500).Not.Nullable();
            Map(x => x.DueDate);
        }
    }
}
