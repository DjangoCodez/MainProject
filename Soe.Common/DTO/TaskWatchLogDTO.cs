using System;

namespace SoftOne.Soe.Common.DTO
{
    public class TaskWatchLogDTO
    {
        public DateTime Start { get; private set; }
        public DateTime? Stop { get; private set; }
        public TimeSpan Duration { get; private set; }
        public int? ActorCompanyId { get; private set; }
        public int? UserId { get; private set; }
        public int? RoleId { get; private set; }
        public int? SupportActorCompanyId { get; private set; }
        public int? SupportUserId { get; private set; }
        public int? SupportRoleId { get; private set; }
        public string Batch { get; private set; }
        public string Name { get; private set; }
        public string ClassName { get; private set; }
        public string Parameters { get; private set; }
        public int? IdCount { get; private set; }
        public int? IntervalCount { get; private set; }

        //Additional fields not persisted in db
        public string Description { get; private set; }
        public int Iteration { get; private set; }
        public decimal? DurationPercent { get; private set; }
        public string IterationString
        {
            get
            {
                return this.Iteration > 0 ? $" [{this.Iteration.ToString()}] " : String.Empty;
            }
        }
        public string DurationString
        {
            get
            {
                return Duration.ToString("hh':'mm':'ss':'fff");
            }
        }
        public string DurationPercentString
        {
            get
            {
                return this.DurationPercent.HasValue ? $" ({this.DurationPercent.Value}%) " : String.Empty;
            }
        }
        public bool IsRunning
        {
            get
            {
                return !this.Stop.HasValue;
            }
        }

        #region Ctor

        public static TaskWatchLogDTO StartTask(
            string name, 
            string description = null, 
            int actorCompanyId = 0, 
            int? userId = null, 
            int? roleId = null,
            int? supportActorCompanyId = null,
            int? supportUserId = null,
            int? supportRoleId = null,
            string className = null,
            string batch = null, 
            string parameters = null, 
            int? idCount = null, 
            int? intervalCount = null
            )
        {
            return new TaskWatchLogDTO
            {
                Name = name,
                Description = description,
                ActorCompanyId = actorCompanyId,
                UserId = userId,
                RoleId = roleId,
                SupportActorCompanyId = supportActorCompanyId,
                SupportUserId = supportUserId,
                SupportRoleId = supportRoleId,
                ClassName = className,
                Batch = string.IsNullOrEmpty(batch) ? batch : Guid.NewGuid().ToString(),
                Parameters = parameters,
                IdCount = idCount,
                IntervalCount = intervalCount,
                Start = DateTime.Now,
            };
        }

        private TaskWatchLogDTO() {}

        #endregion

        #region Methods

        public void SetAsRunning()
        {
            this.Stop = null;
        }

        public void StopTask(TimeSpan duration, int iteration)
        {
            this.Duration = this.Duration.Add(duration);
            this.Stop = this.Start.Add(duration);
            this.Iteration = iteration;
        }

        public void UpdatePercent(decimal percent)
        {
            this.DurationPercent = percent;
        }

        public override string ToString()
        {
            return $"{this.Name} {this.IterationString}{this.DurationString}{this.DurationPercentString} <br />";
        }

        #endregion
    }
}
