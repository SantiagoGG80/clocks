using System;

namespace clocks.Common.Models
{
    public class Todo
    {
        public int IdEmployee { get; set; }
        public DateTime EntryExit { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
