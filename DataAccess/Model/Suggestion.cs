using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Tracker.DataAccess.Model
{
    public class Suggestion : ISuggestion
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public DateTime TimeStamp { get; set; }

        public IUser User { get; set; }

        public int UserId { get; set; }
    }
}
