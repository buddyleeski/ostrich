using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ostrich.Tracker.Model
{
    public class Suggestion
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public DateTime TimeStamp { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }
    }
}
