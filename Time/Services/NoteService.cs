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
    internal class NoteService
    {
        internal static Note InsertNote(User user, DateTime time, string label, List<String> tags)
        {
            var note = new Note()
            {
                User = user,
                TimeStamp = time,
                Label = label
            };

            note.NoteTags = new List<NoteTag>();

            tags.ForEach(tag =>
            {
                note.NoteTags.Add(new NoteTag()
                {
                    Tag = tag,
                    Note = note
                });
            });

            Helpers.Transaction(s => s.SaveOrUpdate(note));

            return note;
        }

        #region List Methods

        internal static List<Note> GetCurrentNote(int userId)
        {
            return GetCurrentNote(userId, null);
        }
        internal static List<Note> GetCurrentNote(int userId, List<string> tags)
        {
            List<Note> results;

            DateTime timeFrom = DateTime.Now.Date;
            DateTime timeTo = DateTime.Now;

            IEnumerable<string> temp = (tags == null ? null : tags.Select(t => t.TrimStart('#').ToUpper()).AsEnumerable());

            results = Helpers.NonTransact<List<Note>>(s =>
            {
                return s.Query<Note>().Where(tl => tl.User.Id == userId
                                                && timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            });

            return results.Where(tl => (tags == null || tags.Count == 0 || tl.NoteTags.Select(tg => tg.Tag).Intersect(temp).Count() == temp.Count())).ToList();
        }

        internal static List<Note> GetNotes(int userId, DateTime start, DateTime end)
        {
            return GetNotes(userId, start, end, null);
        }

        internal static List<Note> GetNotes(int userId, DateTime start, DateTime end, List<string> tags)
        {
            List<Note> results;

            DateTime timeFrom = start;
            DateTime timeTo = end;

            IEnumerable<string> temp = (tags == null ? null : tags.Select(t => t.TrimStart('#').ToUpper()).AsEnumerable());

            results = Helpers.NonTransact(s =>
            {
                return s.Query<Note>().Where(tl => tl.User.Id == userId
                                                && timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            });

            return results.Where(tl => (tags == null || tags.Count == 0 || tl.NoteTags.Select(tg => tg.Tag).Intersect(temp).Count() == temp.Count())).ToList();
        }

        internal static List<Note> GetNotes(int userId)
        {
            return GetNotes(userId, null);
        }

        internal static List<Note> GetNotes(int userId, List<string> tags)
        {
            List<Note> results;

            IEnumerable<string> temp = (tags == null ? null : tags.Select(t => t.TrimStart('#').ToUpper()).AsEnumerable());

            results = Helpers.NonTransact(s =>
            {
                return s.Query<Note>().Where(tl => tl.User.Id == userId).OrderBy(tl => tl.TimeStamp).ToList();
            });

            return results.Where(tl => (tags == null || tags.Count == 0 || tl.NoteTags.Select(tg => tg.Tag).Intersect(temp).Count() == temp.Count())).ToList();
        }

        internal static List<Note> GetNotesForWeek(int userId, DateTime date)
        {
            return GetNotesForWeek(userId, date, null);
        }

        internal static List<Note> GetNotesForWeek(int userId, DateTime date, List<string> tags)
        {
            DateTime start = date;
            DateTime end = new DateTime(date.Year, date.Month, date.Day);
            end = end.AddDays(1).AddSeconds(-1);

            return NoteService.GetNotes(userId, start, end, tags).OrderBy(note => note.TimeStamp).ToList();
        }

        #endregion // List Methods

        internal static Note GetNote(int id)
        {
            Note result;

            result = Helpers.NonTransact(s => s.Query<Note>().SingleOrDefault(note => note.Id == id));

            return result;
        }

        internal static void DeleteNote(Note note)
        {
            Helpers.Transaction(s =>
            {
                var item = s.Query<Note>().SingleOrDefault(tl => tl.Id == note.Id);
                s.Delete(item);
            });
        }

        internal static int DeleteNote(int userId, DateTime start, DateTime end)
        {
            int count = 0;

            Helpers.Transaction(s =>
            {
                var items = s.Query<Note>().Where(t => t.User.Id == userId
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
