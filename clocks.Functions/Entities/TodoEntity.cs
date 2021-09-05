using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace clocks.Functions.Entities
{
    public class TodoEntity : TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime EntryExit { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
