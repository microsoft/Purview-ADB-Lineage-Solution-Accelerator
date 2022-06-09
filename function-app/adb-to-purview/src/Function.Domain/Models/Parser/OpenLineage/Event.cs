using System.Collections.Generic;
using System;

namespace Function.Domain.Models.OL
{
    public class Event
    {
        public string EventType = "";
        public DateTime EventTime = new DateTime();
        public Run Run = new Run();
        public Job Job = new Job();
        public List<Inputs> Inputs = new List<Inputs>();
        public List<Outputs> Outputs = new List<Outputs>();
        public string Producer = "";
        public string SchemaUrl = "";
    }
}