using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace clocks.Functions.Entities
{
    public class TodoEntityC : TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime EntryExit { get; set; }

        public bool Consolidated { get; set; }
    }
}
