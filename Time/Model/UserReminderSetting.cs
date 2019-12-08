using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Ostrich.Tracker.Model
{
    public class UserReminderSetting
    {
        public virtual int Id { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual int Frequency { get; set; }
        public virtual User User { get; set; }
    }

    public class UserReminderSettingMappings : ClassMap<UserReminderSetting>
    {
        public UserReminderSettingMappings()
        {
            Id(x => x.Id);
            Map(x => x.Frequency).Not.Nullable();
            Map(x => x.Enabled).Not.Nullable();
            References(x => x.User).Column("UserId").Not.Nullable();

            Table("UserReminderSetting");
        }
    }
}
