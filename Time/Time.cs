using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ostrich.Logic.Base;
using Ostrich.Tracker.Services;
using Ostrich.Logic.Attributes;
using Ostrich.Logic.Enums;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Ostrich.Tracker.Model;

namespace Ostrich.Tracker
{
    public class Time : ApplicationBase
    {
        private bool IsValid { get; set; }
        private bool IsAdmin { get; set; }
        private int UserId { get; set; }
        private string UserName { get; set; }
        private DateTime LastEntryDate { get; set; }
        private bool RemindersEnabled { get; set; }
        private int ReminderFrequency { get; set; }
        private bool RecordAway { get; set; }
        private Model.User TimeUser {get; set;}


        #region Constant Values
        private const int MAXLABELSIZE = 1000;

        #endregion // Constant values

        #region Help

        public override void Help()
        {
            StringBuilder response = new StringBuilder();

            response.AppendLine("TL [input] -> Make Entry");
            response.AppendLine("TL [input] [minutes] MINUTES AGO -> Make entry some minutes ago");
            response.AppendLine("TL [input] [hours] HOURS AGO -> Make entry some minutes ago");
            response.AppendLine("*tip: Use #tags to tag entries that you can filter by");
            response.AppendLine("*tip: instead of TL, you can use ... to add time lines");
            response.AppendLine("");
            SendUpdate(response.ToString());
            Thread.Sleep(1000);

            response.Clear();
            response.AppendLine("STL -> Show today");
            response.AppendLine("STL [date] -> Show date");
            response.AppendLine("STL WEEK -> Show current week");
            response.AppendLine("STL WEEK [date] -> Show week of date");
            response.AppendLine("STL SINCE [date] -> Show since date");
            response.AppendLine("STL FROM [date] TO [date] -> Show between dates");
            response.AppendLine("*tip: Any stl command can be followed by a tag");
            response.AppendLine("*ex: stl WEEK tag1 tag2");
            SendUpdate(response.ToString());
            Thread.Sleep(1000);

            response.Clear();
            response.AppendLine("EXPORT WEEK -> Export current week");
            response.AppendLine("EXPORT WEEK [date] -> Export week of date");
            response.AppendLine("EXPORT SINCE [date] -> Export since date");
            response.AppendLine("EXPORT FROM [date] TO [date] -> Export between dates");
            SendUpdate(response.ToString());
            Thread.Sleep(1000);

            response.Clear();
            response.AppendLine("DTL [id] -> Delete entry with this id");
            response.AppendLine("DTL FROM [date] to [date] -> Delete all entries in date range");
            SendUpdate(response.ToString());
            Thread.Sleep(1000);

            response.Clear();
            response.AppendLine("START -> Start reminders");
            response.AppendLine("STOP -> Stop reminders");
            response.AppendLine("SET REMINDER FREQUENCY [minutes]");
            SendUpdate(response.ToString());
        }

        #endregion // Help

        #region Overrides and Events

        public override void AppLoad()
        {
            base.AppLoad();
            this.BeforeCommand += new Ostrich.Logic.BeforeCommandHandler(Time_BeforeCommand);
            this.ApplicationEvent += new Ostrich.Logic.ApplicationEventHandler(Time_ApplicationEvent);

            this.UserName = this.User.UserId.ToLowerInvariant();

            this.IsValid = UserService.IsUserValid(this.UserName);
            var user = UserService.GetUser(this.UserName);
            if (user == null)
            {
                this.IsValid = false;
                SendUpdate("You are not registered to use this system.");
                WriteLog(new Exception(string.Format("Unregistered user: {0}", this.UserName)), "Unregistered User!");
                return;
            }


            this.UserId = user.Id;
            this.IsAdmin = user.IsAdmin;
            this.TimeUser = user;


            this.RemindersEnabled = user.UserReminderSetting.Enabled;
            this.ReminderFrequency = user.UserReminderSetting.Frequency;

            this.RecordAway = false;

            //SendUpdate("Welcome back to Ostrich Timeline");
            //SendUpdate("Type \"Help\" to get a list of available commands.");
        }

        void Time_ApplicationEvent(Ostrich.Logic.AppMan.ApplicationUserWrapper sender, Ostrich.Logic.ApplicationEventArgs e)
        {
            if (e.EventName == "MessageFromOstrich" && sender != this.User)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Message From Ostrich:");
                sb.AppendLine(e.Data.ToString());
                SendUpdate(sb.ToString());
            }
            else if (e.EventName == "RefreshUserInfo")
            {

                this.UserName = this.User.UserId.ToLowerInvariant();

                this.IsValid = UserService.IsUserValid(this.UserName);
                var user = UserService.GetUser(this.UserName);

                if (user == null)
                {
                    this.IsValid = false;
                    WriteLog(new Exception(string.Format("Unregistered user: {0}", this.UserName)), "Unregistered User!");
                    return;
                }


                this.UserId = user.Id;
                this.IsAdmin = user.IsAdmin;

                //this.RemindersEnabled = user.UserReminderSetting.Enabled;
                //this.ReminderFrequency = user.UserReminderSetting.Frequency;
            }
        }

        void Time_BeforeCommand(object sender, Ostrich.Logic.BeforeCommandEventArgs e)
        {
            if (!this.IsValid)
            {
                SendUpdate(string.Format("You are not registered to use this system. {0}", this.User.UserId));
                e.Continue = false;
                WriteLog(new Exception(string.Format("Unregistered user: {0}", this.UserName)), "Unregistered User!");
            }
        }

        public new static bool InitiateConversation(string user)
        {
            if (UserService.IsUserValid(user))
                return true;
            else
                return false;
        }

        #endregion // Overrides and Events

        #region Timeline

        #region Create

        [MethodName("TL [label] [minutes] MINUTES AGO"), MethodRank(1)]
        [MethodName("TL [label] [minutes] MINUTE AGO")]
        [MethodName("\\.\\.\\. [label] [minutes] MINUTES AGO")]
        public void Timeline(List<string> label, double minutes)
        {
            string input = string.Join(" ", label);

            if (!string.IsNullOrEmpty(input))
            {
                var tags = label.Where(l => l.StartsWith("#")).Select(l => l.ToUpper().Substring(1)).ToList();


                if (input.Length > MAXLABELSIZE)
                {
                    SendUpdate(string.Format("The label has been truncated to {0} characters", MAXLABELSIZE.ToString()));
                    input = input.Substring(0, 1000);
                }

                var tl = TimelineService.InsertTimeline(this.TimeUser, DateTime.Now.AddMinutes(-1 * minutes), input, tags);

                this.LastEntryDate = DateTime.Now;
                SendUpdate(string.Format("Timeline #{0} has been added.", tl.Id));
                this.RecordAway = true;
            }
        }

        [MethodName("TL [label] [hours] HOURS AGO"), MethodRank(2)]
        [MethodName("TL [label] [hours] HOUR AGO")]
        [MethodName("\\.\\.\\. [label] [hours] HOUR AGO")]
        public void TimelineHours(List<string> label, double hours)
        {
            Timeline(label, hours * 60);
        }

        [MethodName("TL [label]"), MethodRank(3)]
        [MethodName("\\.\\.\\. [label]")]
        public void Timeline(List<string> label)
        {
            Timeline(label, 0);
        }

        #endregion // Create

        #region Show

        private void SendTimelinesShort(List<Timeline> timelines)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            timelines.ForEach(timeline =>
            {
                count++;
                sb.AppendLine(string.Format("{2} - {0}: {1}", timeline.Id, timeline.Label, timeline.TimeStamp.ToString("hh:mm tt")));
                if (count % 10 == 0)
                {
                    SendUpdate(sb.ToString());
                    sb.Clear();
                }
            });

            if (sb.Length > 0)
                SendUpdate(sb.ToString());
            else
                SendUpdate("No timelines have been found");
        }

        private void SendTimelinesWeek(List<Timeline> timelines)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            timelines.ForEach(timeline =>
            {
                count++;

                sb.AppendLine(string.Format("{0} {1} - {2}: {3}",
                                                timeline.TimeStamp.DayOfWeek.ToString().Substring(0, 2),
                                                timeline.TimeStamp.ToString("hh:mm tt"),
                                                timeline.Id,
                                                timeline.Label));

                if (count % 10 == 0)
                {
                    SendUpdate(sb.ToString());
                    sb.Clear();
                }
            });

            if (sb.Length > 0)
                SendUpdate(sb.ToString());
            else
                SendUpdate("No timelines have been found");
        }

        private void SendTimelinesLong(List<Timeline> timelines)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            timelines.ForEach(timeline =>
            {
                count++;
                sb.AppendLine(string.Format("{2} - {3} {0}: {1}", timeline.Id, timeline.Label, timeline.TimeStamp.ToString("MM/dd/yy hh:mm tt"), timeline.TimeStamp.DayOfWeek.ToString().Substring(0, 2)));


                if (count % 10 == 0)
                {
                    SendUpdate(sb.ToString());
                    sb.Clear();
                }
            });

            if (sb.Length > 0)
                SendUpdate(sb.ToString());
            else
                SendUpdate("No timelines have been found");
        }

        #region STL WEEK

        [MethodName("STL WEEK"), MethodRank(4)]
        public void ShowTimelineForCurrentWeek()
        {
            DateTime currentDate = DateTime.Now;

            DateTime start = currentDate.AddDays((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek);
            DateTime end = currentDate.AddDays((int)DayOfWeek.Saturday - (int)currentDate.DayOfWeek);


            List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end).OrderBy(timeline => timeline.TimeStamp).ToList();

            SendTimelinesWeek(timelines);
        }

        [MethodName("STL WEEK [date]"), MethodRank(5)]
        public void ShowTimelineForCurrentWeek(DateTime date)
        {
            DateTime start = date.AddDays((int)DayOfWeek.Sunday - (int)date.DayOfWeek);
            DateTime end = date.AddDays((int)DayOfWeek.Saturday - (int)date.DayOfWeek);


            List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end).OrderBy(timeline => timeline.TimeStamp).ToList();

            SendTimelinesWeek(timelines);
        }

        [MethodName("STL WEEK [date] [tags]"), MethodRank(6)]
        public void ShowTimelineForCurrentWeek(DateTime date, params string[] tags)
        {
            DateTime start = date.AddDays((int)DayOfWeek.Sunday - (int)date.DayOfWeek);
            DateTime end = date.AddDays((int)DayOfWeek.Saturday - (int)date.DayOfWeek);

            List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end, tags.ToList()).OrderBy(timeline => timeline.TimeStamp).ToList();

            SendTimelinesWeek(timelines);
        }

        [MethodName("STL WEEK [tags]"), MethodRank(7)]
        public void ShowTimelineForCurrentWeek(params string[] tags)
        {
            DateTime currentDate = DateTime.Now;

            DateTime start = currentDate.AddDays((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek);
            DateTime end = currentDate.AddDays((int)DayOfWeek.Saturday - (int)currentDate.DayOfWeek);

            List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end, tags.ToList()).OrderBy(timeline => timeline.TimeStamp).ToList();

            SendTimelinesWeek(timelines);
        }

        #endregion // STL WEEK

        #region STL FROM

        [MethodName("STL FROM [start] TO [end]"), MethodRank(8)]
        public void ShowTimelines(DateTime start, DateTime end)
        {
            List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end).OrderBy(timeline => timeline.TimeStamp).ToList();
            SendTimelinesLong(timelines);

        }

        [MethodName("STL FROM [start] TO [end] [tags]"), MethodRank(9)]
        public void ShowTimelines(DateTime start, DateTime end, params string[] tags)
        {
            List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end, tags.ToList()).OrderBy(timeline => timeline.TimeStamp).ToList();
            SendTimelinesLong(timelines);

        }

        #endregion // STL FROM

        #region STL SINCE

        [MethodName("STL SINCE [date]"), MethodRank(10)]
        public void ShowTimelinesSince(DateTime date)
        {
            ShowTimelines(date, DateTime.Now);
        }

        [MethodName("STL SINCE [date] [tags]"), MethodRank(11)]
        public void ShowTimelinesSince(DateTime date, params string[] tags)
        {
            ShowTimelines(date, DateTime.Now, tags);
        }

        #endregion // STL SINCE

        #region STL

        [MethodName("STL [date]"), MethodRank(12)]
        public void ShowTimeline(DateTime date)
        {
            List<Timeline> timelines = TimelineService.GetTimelinesForWeek(this.UserId, date).OrderBy(timeline => timeline.TimeStamp).ToList();
            SendTimelinesShort(timelines);
        }

        [MethodName("STL [date] [tags]"), MethodRank(13)]
        public void ShowTimeline(DateTime date, params string[] tags)
        {
            List<Timeline> timelines = TimelineService.GetTimelinesForWeek(this.UserId, date, tags.ToList()).OrderBy(timeline => timeline.TimeStamp).ToList();
            SendTimelinesShort(timelines);
        }

        [MethodName("STL"), MethodRank(14)]
        public void ShowCurrentTimeline()
        {
            List<Timeline> timelines = TimelineService.GetCurrentTimeline(this.UserId).OrderBy(timeline => timeline.TimeStamp).ToList();
            SendTimelinesShort(timelines);
        }

        [MethodName("STL [tags]"), MethodRank(15)]
        public void ShowCurrentTimeline(params string[] tags)
        {
            List<Timeline> timelines = TimelineService.GetCurrentTimeline(this.UserId, tags.ToList()).OrderBy(timeline => timeline.TimeStamp).ToList();
            SendTimelinesShort(timelines);
        }

        #endregion // STL

        #endregion // Show

        #region Delete

        [MethodName("DTL [ID]"), MethodRank(16)]
        public void DeleteTimeline(int id)
        {
            Timeline timeline = TimelineService.GetTimeline(id);
            if (timeline == null)
            {
                SendUpdate("Unable to find a timeline with that id");
                return;
            }

            TimelineService.DeleteTimeline(timeline);

            SendUpdate(string.Format("Timeline entry has been deleted: {0}: \"{1}\" at {2}", timeline.Id, timeline.Label, timeline.TimeStamp));
        }

        [MethodName("DTL FROM [start] TO [end]"), MethodRank(17)]
        public void DeleteTimeline(DateTime start, DateTime end)
        {
            int count = TimelineService.DeleteTimeline(this.UserId, start, end);

            if (count == 0)
            {
                SendUpdate("Unable to find a timelines in this range");
                return;
            }

            SendUpdate(string.Format("{0} entries have been deleted", count));
        }

        #endregion // Delete

        #endregion // Timeline

        #region Suggestions

        [MethodName("SUGGESTION [label]")]
        [Description("Adds a new Suggestion entry")]
        public void Suggestion(params string[] label)
        {
            //string input = string.Join(" ", label);

            //if (!string.IsNullOrEmpty(input))
            //{
            //    if (input.Length > MAXLABELSIZE)
            //    {
            //        SendUpdate(string.Format("The label has been truncated to {0} characters", MAXLABELSIZE.ToString()));
            //        input = input.Substring(0, 1000);
            //    }

            //    var tl = SuggestionService.InsertSuggestion(this.UserId, DateTime.Now, input);

            //    this.LastEntryDate = DateTime.Now;
            //    SendUpdate(string.Format("Suggestion #{0} has been added.", tl.Id));
            //}
        }

        [MethodName("SHOW SUGGESTION SINCE [date]")]
        [MethodName("SS SINCE [date]")]
        [Description("Shows a list of Suggestion events between the given date and today")]
        public void ShowSuggestionsSince(DateTime date)
        {
            ShowSuggestions(date, DateTime.Now);
        }


        [MethodName("SHOW SuggestionS FROM [start] TO [end]")]
        [MethodName("SS FROM [start] TO [end]")]
        [MethodName("SS [start] TO [end]")]
        [Description("Shows a list of Suggestion events between the start and end dates")]
        public void ShowSuggestions(DateTime start, DateTime end)
        {
            //List<Suggestion> Suggestions = null;
            //if (this.IsAdmin)
            //    Suggestions = SuggestionService.GetSuggestions(start, end).OrderBy(Suggestion => Suggestion.TimeStamp).ToList();
            //else
            //    Suggestions = SuggestionService.GetSuggestions(start, end).OrderBy(Suggestion => Suggestion.TimeStamp).ToList();

            //StringBuilder sb = new StringBuilder();
            //int count = 0;
            //Suggestions.ForEach(Suggestion =>
            //{
            //    count++;
            //    sb.AppendLine(string.Format("{2} - {0}: {1}", Suggestion.Id, Suggestion.Label, Suggestion.TimeStamp.ToString("MM/dd/yy hh:mm tt")));


            //    if (count % 10 == 0)
            //    {
            //        SendUpdate(sb.ToString());
            //        sb.Clear();
            //    }
            //});

            //if (sb.Length > 0)
            //    SendUpdate(sb.ToString());
        }
        #endregion // Suggestions

        #region Notes

        [MethodName("NT [label]"), MethodRank(1)]
        [MethodName("!!! [label]")]
        public void Note(List<string> label)
        {
            string input = string.Join(" ", label);

            if (!string.IsNullOrEmpty(input))
            {
                var tags = label.Where(l => l.StartsWith("#")).Select(l => l.ToUpper().Substring(1)).ToList();


                if (input.Length > MAXLABELSIZE)
                {
                    SendUpdate(string.Format("The label has been truncated to {0} characters", MAXLABELSIZE.ToString()));
                    input = input.Substring(0, 1000);
                }

                var tl = NoteService.InsertNote(this.TimeUser, DateTime.Now, input, tags);

                this.LastEntryDate = DateTime.Now;
                SendUpdate(string.Format("Note #{0} has been added.", tl.Id));
                this.RecordAway = true;
            }
        }


        #region Show

        private void SendNotesShort(List<Note> Notes)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            Notes.ForEach(Note =>
            {
                count++;
                sb.AppendLine(string.Format("{0}: {1}", Note.Id, Note.Label));
                if (count % 10 == 0)
                {
                    SendUpdate(sb.ToString());
                    sb.Clear();
                }
            });

            if (sb.Length > 0)
                SendUpdate(sb.ToString());
            else
                SendUpdate("No Notes have been found");
        }

        private void SendNotesWeek(List<Note> Notes)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            Notes.ForEach(Note =>
            {
                count++;

                sb.AppendLine(string.Format("{0} {1} - {2}: {3}",
                                                Note.TimeStamp.DayOfWeek.ToString().Substring(0, 2),
                                                Note.TimeStamp.ToString("hh:mm tt"),
                                                Note.Id,
                                                Note.Label));

                if (count % 10 == 0)
                {
                    SendUpdate(sb.ToString());
                    sb.Clear();
                }
            });

            if (sb.Length > 0)
                SendUpdate(sb.ToString());
            else
                SendUpdate("No Notes have been found");
        }

        private void SendNotesLong(List<Note> Notes)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            Notes.ForEach(Note =>
            {
                count++;
                sb.AppendLine(string.Format("{2} - {3} {0}: {1}", Note.Id, Note.Label, Note.TimeStamp.ToString("MM/dd/yy hh:mm tt"), Note.TimeStamp.DayOfWeek.ToString().Substring(0, 2)));


                if (count % 10 == 0)
                {
                    SendUpdate(sb.ToString());
                    sb.Clear();
                }
            });

            if (sb.Length > 0)
                SendUpdate(sb.ToString());
            else
                SendUpdate("No Notes have been found");
        }

        #region SNT WEEK

        [MethodName("SNT WEEK"), MethodRank(4)]
        public void ShowNoteForCurrentWeek()
        {
            DateTime currentDate = DateTime.Now;

            DateTime start = currentDate.AddDays((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek);
            DateTime end = currentDate.AddDays((int)DayOfWeek.Saturday - (int)currentDate.DayOfWeek);


            List<Note> Notes = NoteService.GetNotes(this.UserId, start, end).OrderBy(Note => Note.TimeStamp).ToList();

            SendNotesWeek(Notes);
        }

        [MethodName("SNT WEEK [date]"), MethodRank(5)]
        public void ShowNoteForCurrentWeek(DateTime date)
        {
            DateTime start = date.AddDays((int)DayOfWeek.Sunday - (int)date.DayOfWeek);
            DateTime end = date.AddDays((int)DayOfWeek.Saturday - (int)date.DayOfWeek);


            List<Note> Notes = NoteService.GetNotes(this.UserId, start, end).OrderBy(Note => Note.TimeStamp).ToList();

            SendNotesWeek(Notes);
        }

        [MethodName("SNT WEEK [date] [tags]"), MethodRank(6)]
        public void ShowNoteForCurrentWeek(DateTime date, params string[] tags)
        {
            DateTime start = date.AddDays((int)DayOfWeek.Sunday - (int)date.DayOfWeek);
            DateTime end = date.AddDays((int)DayOfWeek.Saturday - (int)date.DayOfWeek);

            List<Note> Notes = NoteService.GetNotes(this.UserId, start, end, tags.ToList()).OrderBy(Note => Note.TimeStamp).ToList();

            SendNotesWeek(Notes);
        }

        [MethodName("SNT WEEK [tags]"), MethodRank(7)]
        public void ShowNoteForCurrentWeek(params string[] tags)
        {
            DateTime currentDate = DateTime.Now;

            DateTime start = currentDate.AddDays((int)DayOfWeek.Sunday - (int)currentDate.DayOfWeek);
            DateTime end = currentDate.AddDays((int)DayOfWeek.Saturday - (int)currentDate.DayOfWeek);

            List<Note> Notes = NoteService.GetNotes(this.UserId, start, end, tags.ToList()).OrderBy(Note => Note.TimeStamp).ToList();

            SendNotesWeek(Notes);
        }

        #endregion // SNT WEEK

        #region SNT FROM

        [MethodName("SNT FROM [start] TO [end]"), MethodRank(8)]
        public void ShowNotes(DateTime start, DateTime end)
        {
            List<Note> Notes = NoteService.GetNotes(this.UserId, start, end).OrderBy(Note => Note.TimeStamp).ToList();
            SendNotesLong(Notes);

        }

        [MethodName("SNT FROM [start] TO [end] [tags]"), MethodRank(9)]
        public void ShowNotes(DateTime start, DateTime end, params string[] tags)
        {
            List<Note> Notes = NoteService.GetNotes(this.UserId, start, end, tags.ToList()).OrderBy(Note => Note.TimeStamp).ToList();
            SendNotesLong(Notes);

        }

        #endregion // SNT FROM

        #region SNT SINCE

        [MethodName("SNT SINCE [date]"), MethodRank(10)]
        public void ShowNotesSince(DateTime date)
        {
            ShowNotes(date, DateTime.Now);
        }

        [MethodName("SNT SINCE [date] [tags]"), MethodRank(11)]
        public void ShowNotesSince(DateTime date, params string[] tags)
        {
            ShowNotes(date, DateTime.Now, tags);
        }

        #endregion // SNT SINCE

        #region SNT

        [MethodName("SNT [date]"), MethodRank(12)]
        public void ShowNote(DateTime date)
        {
            List<Note> Notes = NoteService.GetNotesForWeek(this.UserId, date).OrderBy(Note => Note.TimeStamp).ToList();
            SendNotesShort(Notes);
        }

        [MethodName("SNT [date] [tags]"), MethodRank(13)]
        public void ShowNote(DateTime date, params string[] tags)
        {
            List<Note> Notes = NoteService.GetNotesForWeek(this.UserId, date, tags.ToList()).OrderBy(Note => Note.TimeStamp).ToList();
            SendNotesShort(Notes);
        }

        [MethodName("SNT"), MethodRank(14)]
        public void ShowCurrentNote()
        {
            List<Note> Notes = NoteService.GetCurrentNote(this.UserId).OrderBy(Note => Note.TimeStamp).ToList();
            SendNotesShort(Notes);
        }

        [MethodName("SNT [tags]"), MethodRank(15)]
        public void ShowCurrentNote(params string[] tags)
        {
            List<Note> Notes = NoteService.GetCurrentNote(this.UserId, tags.ToList()).OrderBy(Note => Note.TimeStamp).ToList();
            SendNotesShort(Notes);
        }

        #endregion // SNT

        #endregion // Show

        #region Delete

        [MethodName("DNT [ID]"), MethodRank(16)]
        public void DeleteNote(int id)
        {
            Note note = NoteService.GetNote(id);
            if (note == null)
            {
                SendUpdate("Unable to find a note with that id");
                return;
            }

            NoteService.DeleteNote(note);

            SendUpdate(string.Format("Note entry has been deleted: {0}: \"{1}\" at {2}", note.Id, note.Label, note.TimeStamp));
        }

        [MethodName("DNT FROM [start] TO [end]"), MethodRank(17)]
        public void DeleteNote(DateTime start, DateTime end)
        {
            int count = NoteService.DeleteNote(this.UserId, start, end);

            if (count == 0)
            {
                SendUpdate("Unable to find a notes in this range");
                return;
            }

            SendUpdate(string.Format("{0} entries have been deleted", count));
        }

        #endregion // Delete

        #endregion // Notes

        #region Export

        [MethodName("EXPORT ALL")]
        public void ExportAll()
        {
            try
            {
                var timelines = TimelineService.GetTimelines(this.UserId);
                this.ExportTimelines(this.UserName, timelines, "AllTimelines.csv");
                SendUpdate("Sent, should be in your inbox soon!");
            }
            catch (Exception ex)
            {
                SendUpdate("Sorry, I was unable to send the email.");
                WriteLog(ex, "EmailError");
            }


        }

        [MethodName("EXPORT WEEK")]
        public void ExportWeek()
        {
            Export(DateTime.Now);
        }

        [MethodName("EXPORT WEEK [date]")]
        public void Export(DateTime date)
        {
            try
            {
                DateTime start = date.AddDays((int)DayOfWeek.Sunday - (int)date.DayOfWeek);
                DateTime end = date.AddDays((int)DayOfWeek.Saturday - (int)date.DayOfWeek);

                List<Timeline> timelines = TimelineService.GetTimelines(this.UserId, start, end).OrderBy(timeline => timeline.TimeStamp).ToList();
                this.ExportTimelines(this.UserName, timelines, string.Format("ExportWeek{0}.csv", date.ToString("MM-dd-yy")));
                SendUpdate("Sent, should be in your inbox soon!");
            }
            catch (Exception ex)
            {
                SendUpdate("Sorry, I was unable to send the email.");
                WriteLog(ex, "EmailError");
            }


        }

        [MethodName("EXPORT FROM [start] TO [end]")]
        public void Export(DateTime start, DateTime end)
        {
            try
            {
                var timelines = TimelineService.GetTimelines(this.UserId, start, end);
                this.ExportTimelines(this.UserName, timelines, string.Format("ExportFrom{0}To{1}.csv", start.ToString("MM-dd-yy"), end.ToString("MM-dd-yy")));
                SendUpdate("Sent, should be in your inbox soon!");
            }
            catch (Exception ex)
            {
                SendUpdate("Sorry, I was unable to send the email.");
                WriteLog(ex, "EmailError");
            }

        }

        [MethodName("EXPORT SINCE [date]")]
        public void ExportSince(DateTime date)
        {
            try
            {
                var timelines = TimelineService.GetTimelines(this.UserId, date, DateTime.Now);
                this.ExportTimelines(this.UserName, timelines, string.Format("ExportSince{0}.csv", date.ToString("MM-dd-yy")));
                SendUpdate("Sent, should be in your inbox soon!");
            }
            catch (Exception ex)
            {

                SendUpdate("Sorry, I was unable to send the email.");
                WriteLog(ex, "EmailError");
            }
        }

        private void ExportTimelines(string emailAddress, List<Timeline> timelines, string name)
        {
            var fromAddress = new MailAddress("OstrichTimeline@gmail.com", "Ostrich Timeline");
            var toAddress = new MailAddress(emailAddress, emailAddress);
            const string fromPassword = "OstrichProj";
            const string subject = "Timeline Export";
            const string body = "Your exported Timeline";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Day, TimeStamp, Label, Tags, Difference, Id");

            int row = 1;
            int startRow = 0;
            int endRow = 0;
            DateTime? lastDate = null;
            TimeSpan span = new TimeSpan();

            //string formula = "\"=TEXT(INDIRECT(ADDRESS(ROW(), COLUMN()-3)) - INDIRECT(ADDRESS(ROW()-1, COLUMN()-3)), \"\"HH:MM\"\")\"";
            timelines.ForEach(tl =>
            {
                row++;

                StringBuilder tagString = new StringBuilder();
                var tags = tl.TimelineTags.ToList();
                string day = tl.TimeStamp.DayOfWeek.ToString().Substring(0, 2);

                tags.ForEach(tag =>
                {
                    tagString.Append(tag.Tag + " ");
                });

                if (lastDate == null)
                {
                    sb.AppendLine(string.Format("{0},{1},\"{2}\",\"{3}\",,{4},",
                                day,
                                tl.TimeStamp.ToString("MM/dd/yy hh:mm tt"),
                                tl.Label,
                                tagString,
                                tl.Id));
                    startRow = row;
                    endRow = row;
                }
                else if (lastDate.Value.Date != tl.TimeStamp.Date)
                {
                    int totalMinutes = (int)span.TotalMinutes;
                    endRow = row;
                    sb.AppendLine(string.Format(",,Total Estimated Hours For Day,\"=TEXT(SUM(E{0}:E{1}),\"\"HH:MM\"\")\"", startRow, endRow));
                    //sb.AppendLine(string.Format(",,Total Estimated Hours For Day,\"=TEXT(C{0} - C{1}, \"\"HH:MM\"\"\"", startRow, endRow));
                    row++;

                    span = new TimeSpan();
                    sb.AppendLine(",,,,");
                    row++;

                    startRow = row;
                    sb.AppendLine(string.Format("{0},{1},\"{2}\",\"{3}\",,{4},",
                                day,
                                tl.TimeStamp.ToString("MM/dd/yy hh:mm tt"),
                                tl.Label,
                                tagString,
                                tl.Id));
                    endRow = row;
                }
                else
                {
                    DateTime last = lastDate ?? tl.TimeStamp;
                    var diff = (new TimeSpan(tl.TimeStamp.Hour, tl.TimeStamp.Minute, 0) - new TimeSpan(last.Hour, last.Minute, 0));
                    int totalMinutes = (int)diff.TotalMinutes;

                    sb.AppendLine(string.Format("{0},{1},\"{2}\",\"{3}\",{4},{5},",
                                day,
                                tl.TimeStamp.ToString("MM/dd/yy hh:mm tt"),
                                tl.Label,
                                tagString,
                                string.Format("{0}:{1}", totalMinutes / 60, totalMinutes % 60),
                        //formula,
                                tl.Id));//totalMinutes%60));

                    span += diff;
                }

                lastDate = tl.TimeStamp;
            });


            sb.AppendLine(string.Format(",,Total Estimated Hours For Day,\"=TEXT(SUM(E{0}:E{1}),\"\"HH:MM\"\")\"", startRow, endRow));

            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            MemoryStream ms = new MemoryStream(data);

            Attachment attachment = new Attachment(ms, name);
            attachment.ContentType = new System.Net.Mime.ContentType("text/plain");
            attachment.NameEncoding = Encoding.UTF8;

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            message.Attachments.Add(attachment);
            smtp.Send(message);

        }

        #endregion // Export

        #region Settings Methods

        [MethodName("SET REMINDER FREQUENCY [minutes]")]
        [MethodName("SET REMINDER FREQUENCY [minutes] MINUTES")]
        [Description("Sets the frequency of the reminders (default 5 minutes)")]
        public void SetReminderFrequency(int min)
        {
            UserService.UpdateReminderFrequency(this.UserId, min);
            this.ReminderFrequency = min;
            SendUpdate(string.Format("Reminder frequency has been updated to {0} minutes", this.ReminderFrequency));
        }

        [MethodName("SET REMINDER FREQUENCY [minutes] [NOTUSED]")]
        public void SetReminderFrequency(int min, params string[] extra)
        {
            SetReminderFrequency(min);
        }

        [MethodName("ENABLE REMINDERS")]
        [MethodName("BEGIN")]
        [MethodName("START")]
        public void EnableReminders()
        {
            UserService.UpdateReminderEnabled(this.UserId, true);
            this.RemindersEnabled = true;
            this.LastEntryDate = DateTime.Now;
            SendUpdate(string.Format("You will recieve a reminder every {0} minutes.", this.ReminderFrequency));

        }

        [MethodName("DISABLE REMINDERS")]
        [MethodName("END")]
        [MethodName("GOODBYE")]
        [MethodName("STOP")]
        public void DisableReminders()
        {
            UserService.UpdateReminderEnabled(this.UserId, false);
            this.RemindersEnabled = false;
            SendUpdate("Reminders have been disabled");
        }

        #endregion // Settings Methods

        #region Interval Methods

        [Interval(60000)]
        public void Reminder()
        {

            if (this.RemindersEnabled && (DateTime.Now - this.LastEntryDate).TotalSeconds > this.ReminderFrequency * 60)
            {
                if (this.User.Presence != PresenceEnum.Offline
                        && this.User.Presence != PresenceEnum.DoNotDisturb)
                {
                    SendUpdate("What are you working on right now?");
                    string input = this.GetInput(90000);

                    if (input.Length > 0)
                    {
                        Timeline(new List<string>() { input });
                        this.RecordAway = true;
                        this.LastEntryDate = DateTime.Now;
                    }
                    else if (this.RecordAway)
                    {
                        Timeline(new List<string>() { string.Format("Status: {0}", this.User.Presence.ToString()) });
                        SendUpdate(string.Format("Status: {0}", this.User.Presence.ToString()));
                        this.RecordAway = false;
                    }
                }
            }
        }

        #endregion // Interval Methods

        #region Random Test Methods

        //[MethodName("test [num] [params] done")]
        //[Description("Tests the params")]
        //public void Test(int test, params string[] values)
        //{

        //    SendUpdate(string.Join(" ", values) + " and num " + test.ToString());
        //}

        //[MethodName("Int [params] done")]
        //[Description("Tests the params")]
        //public void IntTest(params int[] values)
        //{

        //    SendUpdate(string.Join(" ", values));
        //}

        //[MethodName("date [params] done")]
        //[Description("Tests the params")]
        //public void DateTest(params DateTime[] values)
        //{

        //    SendUpdate(string.Join(" ", values));
        //}

        //void User_TimelinePresenceEvent(object sender, Ostrich.Logic.PresenceEventArgs e)
        //{
        //    SendUpdate(e.Presence.ToString());
        //}

        #endregion // Random Test Methods

        #region Admin Methods

        [MethodName("News Update: [label]")]
        [MethodName("nu [label]")]
        public void NewsUpdate(params string[] label)
        {
            if (!this.IsAdmin)
            {
                SendUpdate("Sorry you're not an administrator.");
                return;
            }

            string input = string.Join(" ", label);

            if (!string.IsNullOrEmpty(input))
            {
                this.TriggerEvent("MessageFromOstrich", input);
            }
        }

        [MethodName("Restart Ostrich Service")]
        public void RestartService()
        {
            if (!this.IsAdmin)
                return;

            throw new Exception(string.Format("Forced Restart By {0}", this.UserName));
        }

        [MethodName("Refresh Ostrich Service")]
        public void RefreshService()
        {
            if (!this.IsAdmin)
                return;

            this.TriggerEvent("RefreshUserInfo", "");
        }

        [MethodName("Add User")]
        public void AddUser()
        {
            if (!this.IsAdmin)
                return;

            string email = this.GetValue("Email?");
            if (email.Length == 0)
            {
                SendUpdate("canceled");
                return;
            }

            string firstName = this.GetValue("First Name?");
            if (firstName.Length == 0)
            {
                SendUpdate("canceled");
                return;
            }

            string lastName = this.GetValue("Last Name?");
            if (lastName.Length == 0)
            {
                SendUpdate("canceled");
                return;
            }

            UserService.AddUser(email, firstName, lastName);
        }

        #endregion // Admin Methods

        #region Extras

        [MethodName("THANK YOU")]
        public void ThankYou()
        {
            SendUpdate("You're Welcome!");
        }

        [MethodName("THANKS")]
        [MethodName("TY")]
        public void Thanks()
        {
            SendUpdate("No problem");
        }

        #endregion // Extras

        private void WriteLog(Exception ex, string shortDesc)
        {
            EventLog.WriteEntry("Ostrich Timeline", string.Format("{0}\n{1}\n{2}", shortDesc, ex.Message, ex.StackTrace ?? ""), EventLogEntryType.Error, 0);
        }

    }
}
