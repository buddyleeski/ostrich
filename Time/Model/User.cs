using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class User
    {
        public virtual int Id { get; set; }

        public virtual string FirstName { get; set; }

        public virtual bool IsAdmin { get; set; }

        public virtual string LastName { get; set; }

        public virtual IList<Suggestion> Suggestions { get; set; }

        //private IList<Timeline> _timelines;
        //public virtual IList<Timeline> Timelines
        //{
        //    get { return _timelines ?? (_timelines = new List<Timeline>()); }
        //    set { _timelines = value; }
        //}

        public virtual IList<Timeline> Timelines { get; set; }

        public virtual string UserName { get; set; }

        public virtual UserReminderSetting UserReminderSetting { get; set; }

    }


    public class UserMapping : ClassMap<User>
    {
        public UserMapping()
        {
            Id(x => x.Id);
            Map(x => x.UserName).Not.Nullable().Length(100);
            Map(x => x.FirstName).Not.Nullable().Length(100);
            Map(x => x.LastName).Not.Nullable().Length(100);
            Map(x => x.IsAdmin).Not.Nullable();

            HasMany(x => x.Timelines).KeyColumn("UserId").Cascade.All();
            HasOne<UserReminderSetting>(x => x.UserReminderSetting).PropertyRef("User");
            //References<UserReminderSetting>(s => s.UserReminderSetting).Column("UserId");
            Table("User");
        }
    }
}
