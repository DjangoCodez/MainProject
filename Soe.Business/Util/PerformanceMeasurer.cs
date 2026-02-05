using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util
{
    public class PerformanceMeasurer
    {
        private string Label { get; set; }
        private List<PerformanceMeasurerItem> Items { get; set; }
        
        private DateTime StartUtc { get; set; }
        private DateTime LastUtc { get; set; }

        private string Metadata { get; set; }

        public PerformanceMeasurer(string label)
        {
            Label = label;
            Items = new List<PerformanceMeasurerItem>();
        }

        public void Start(object Metadata = null)
        {
            StartUtc = DateTime.UtcNow;
            LastUtc = StartUtc;
            Checkpoint("Start");
            if (Metadata != null)
                AddMetaData(Metadata);
        }

        public void Checkpoint(string label)
        {
            if (StartUtc == null) Start();
            var now = DateTime.UtcNow;
            var secondsFromStart = (int)(now - StartUtc).TotalSeconds;
            var secondsFromLastMeasure = (int)(now - LastUtc).TotalSeconds;
            Items.Add(new PerformanceMeasurerItem(Items.Count, label, secondsFromStart, secondsFromLastMeasure));
            LastUtc = now;
        }

        private void AddMetaData<T>(T metadata)
        {
            Metadata = JsonConvert.SerializeObject(metadata);
        }

        public string Done()
        {
            Checkpoint("End");
            return ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"PerformanceMeasurer: {Label}");
            sb.AppendLine();
            foreach (var item in Items)
            {
                sb.AppendLine(item.ToString());
            }
            sb.AppendLine();
            if (!string.IsNullOrEmpty(Metadata))
            {
                sb.AppendLine("Metadata:");
                sb.AppendLine(Metadata);
            }
            return sb.ToString();
        }
    }

    public class PerformanceMeasurerItem
    {
        private int Step { get; set; }
        private string Label { get; set; }
        private int SecondsFromStart { get; set; }
        private int SecondsFromLastMeasure { get; set; }

        public PerformanceMeasurerItem(int step, string label, int secondFromStart, int secondsFromLastMeasure)
        {
            Step = step;
            Label = label;
            SecondsFromStart = secondFromStart;
            SecondsFromLastMeasure = secondsFromLastMeasure;
        }

        public override string ToString()
        {
            return $"{Step}. {Label}: {SecondsFromStart} (from start), {SecondsFromLastMeasure} (from last measure)";
        }
    }
}
