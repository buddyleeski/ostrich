using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Tracker.Model;

namespace Ostrich.Tracker.Services
{
    internal class SuggestionService
    {
        internal static Suggestion InsertSuggestion(int userId, DateTime time, string label)
        {
            var Suggestion = new Suggestion()
            {
                UserId = userId,
                TimeStamp = time,
                Label = label
            };

            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                en.Suggestions.AddObject(Suggestion);

                en.SaveChanges();
            }

            return Suggestion;
        }



        #region List Methods

        internal static List<Suggestion> GetCurrentSuggestion(int userId)
        {
            List<Suggestion> results;

            DateTime timeFrom = DateTime.Now.Date;
            DateTime timeTo = DateTime.Now;

            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                results = en.Suggestions.Where(tl => tl.UserId == userId
                                                && timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            }

            return results;
        }

        internal static List<Suggestion> GetSuggestions(int userId, DateTime start, DateTime end)
        {
            List<Suggestion> results;

            DateTime timeFrom = start;
            DateTime timeTo = end;

            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                results = en.Suggestions.Where(tl => tl.UserId == userId
                                                && timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            }

            return results;
        }

        internal static List<Suggestion> GetSuggestions(DateTime start, DateTime end)
        {
            List<Suggestion> results;

            DateTime timeFrom = start;
            DateTime timeTo = end;

            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                results = en.Suggestions.Where(tl => timeFrom <= tl.TimeStamp
                                                && timeTo >= tl.TimeStamp).ToList();
            }

            return results;
        }

        //internal static List<Suggestion> GetSuggestions(int userId, DateTime start, DateTime end)
        //{
        //    List<Suggestion> results;

        //    DateTime timeFrom = start;
        //    DateTime timeTo = end;

        //    using (OstrichTimeEntities en = new OstrichTimeEntities())
        //    {
        //        results = en.Suggestions.Where(tl => timeFrom <= tl.TimeStamp
        //                                        && timeTo >= tl.TimeStamp
        //                                        && tl.UserId == userId).ToList();
        //    }

        //    return results;
        //}

        internal static List<Suggestion> GetSuggestions(int userId)
        {
            List<Suggestion> results;

            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                results = en.Suggestions.Where(tl => tl.UserId == userId).OrderBy(tl => tl.TimeStamp).ToList();
            }

            return results;
        }

        internal static List<Suggestion> GetSuggestionsForWeek(int userId, DateTime date)
        {
            DateTime start = date;
            DateTime end = new DateTime(date.Year, date.Month, date.Day);
            end = end.AddDays(1).AddSeconds(-1);

            return SuggestionService.GetSuggestions(userId, start, end).OrderBy(Suggestion => Suggestion.TimeStamp).ToList();
        }

        #endregion // List Methods

        internal static Suggestion GetSuggestion(int id)
        {
            Suggestion result;

            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                result = en.Suggestions.SingleOrDefault(Suggestion => Suggestion.Id == id);
            }

            return result;
        }

        internal static void DeleteSuggestion(Suggestion Suggestion)
        {
            using (OstrichTimeEntities en = new OstrichTimeEntities())
            {
                en.Attach(Suggestion);
                en.DeleteObject(Suggestion);
                en.SaveChanges();
            }
        }
    }
}
