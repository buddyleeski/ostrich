using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Tracker.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace Ostrich.Tracker.Services
{
    internal class TagServices
    {
        internal static List<TimelineTag> GetTags(int userId)
        {

            List<TimelineTag> tags;

            tags = Helpers.NonTransact(s => s.Query<TimelineTag>().Where(tg => tg.Timeline.User.Id == userId).ToList());

            return tags;
        }
    }
}
