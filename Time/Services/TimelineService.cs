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
    internal class TimelineService
    {
        internal static Timeline InsertTimeline(User user, DateTime time, string label, List<String> tags)
        {
            var timeline = new Timeline()
            {
                User = user,
                TimeStamp = time,
                Label = label
            };

            timeline.TimelineTags = new List<TimelineTag>();

            tags.ForEach(tag => {
                timeline.TimelineTags.Add(new TimelineTag() 
                { 
                    Tag = tag,
                    Timeline = timeline
                });
            });

            Helpers.Transaction(s => s.SaveOrUpdate(timeline));

            return timeline;
        }

        #region List Methods

        internal static List<Timeline> GetCurrentTimeline(int userId)
        {
            return GetCurrentTimeline(userId, null);
        }
        internal static List<Timeline> GetCurrentTimeline(int userId, List<string> tags)
        {
            List<Timeline> results;

            DateTime timeFrom = DateTime.Now.Date;
            DateTime timeTo = DateTime.Now;

            IEnumerable<string> temp = (tags == null ? null : tags.Select(t => t.TrimStart('#').ToUpper()).AsEnumerable());

            results = Helpers.NonTransact<List<Timeline>>(s => 
            {
                return s.Query<Timeline>().Where(tl => tl.User.Id == userId
                                                && timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            });

            return results.Where(tl => (tags == null || tags.Count == 0 || tl.TimelineTags.Select(tg => tg.Tag).Intersect(temp).Count() == temp.Count())).ToList();
        }

        internal static List<Timeline> GetTimelines(int userId, DateTime start, DateTime end)
        {
            return GetTimelines(userId, start, end, null);
        }

        internal static List<Timeline> GetTimelines(int userId, DateTime start, DateTime end, List<string> tags)
        {
            List<Timeline> results;

            DateTime timeFrom = start;
            DateTime timeTo = end;

            IEnumerable<string> temp = (tags == null ? null : tags.Select(t => t.TrimStart('#').ToUpper()).AsEnumerable());

            results = Helpers.NonTransact(s =>
            {
                return s.Query<Timeline>().Where(tl => tl.User.Id == userId
                                                && timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            });

            return results.Where(tl => (tags == null || tags.Count == 0 || tl.TimelineTags.Select(tg => tg.Tag).Intersect(temp).Count() == temp.Count())).ToList();
        }

        internal static List<Timeline> GetTimelines(int userId)
        {
            return GetTimelines(userId, null);
        }

        internal static List<Timeline> GetTimelines(int userId, List<string> tags)
        {
            List<Timeline> results;

            IEnumerable<string> temp = (tags == null ? null : tags.Select(t => t.TrimStart('#').ToUpper()).AsEnumerable());

            results = Helpers.NonTransact(s =>
            {
                return s.Query<Timeline>().Where(tl => tl.User.Id == userId).OrderBy(tl => tl.TimeStamp).ToList();
            });

            return results.Where(tl => (tags == null || tags.Count == 0 || tl.TimelineTags.Select(tg => tg.Tag).Intersect(temp).Count() == temp.Count())).ToList();
        }

        internal static List<Timeline> GetTimelinesForWeek(int userId, DateTime date)
        {
            return GetTimelinesForWeek(userId, date, null);
        }

        internal static List<Timeline> GetTimelinesForWeek(int userId, DateTime date, List<string> tags)
        {
            DateTime start = date;
            DateTime end = new DateTime(date.Year, date.Month, date.Day);
            end = end.AddDays(1).AddSeconds(-1);

            return TimelineService.GetTimelines(userId, start, end, tags).OrderBy(timeline => timeline.TimeStamp).ToList();
        }

        #endregion // List Methods

        internal static Timeline GetTimeline(int id)
        {
            Timeline result;

            result = Helpers.NonTransact(s =>  s.Query<Timeline>().SingleOrDefault(timeline => timeline.Id == id));

            return result;
        }

        internal static void DeleteTimeline(Timeline timeline)
        {
            Helpers.Transaction(s =>
            {
                var item = s.Query<Timeline>().SingleOrDefault(tl => tl.Id == timeline.Id);
                s.Delete(item);
            });
        }

        internal static int DeleteTimeline(int userId, DateTime start, DateTime end)
        {
            int count = 0;

            Helpers.Transaction(s =>
            {
                var items = s.Query<Timeline>().Where(t => t.User.Id == userId
                                                && t.TimeStamp >= start
                                                && t.TimeStamp <= end).ToList();
                count = items.Count;

                items.ForEach(t =>
                {
                    s.Delete(t);
                    
                });
            });

            return count;
        }


    }
}
