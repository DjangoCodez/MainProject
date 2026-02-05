namespace SoftOne.Soe.Business.Util.SoftOne_Server_Utility.TFS
{

    public class BuildDefinitionRoot
    {
        public int count { get; set; }
        public BuildDefinition[] BuildDefinition { get; set; }
    }

    public class BuildDefinition
    {
        public string quality { get; set; }
        public Authoredby authoredBy { get; set; }
        public Queue queue { get; set; }
        public string uri { get; set; }
        public string type { get; set; }
        public int revision { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Project project { get; set; }
    }

    public class Authoredby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }

    public class Queue
    {
        public object pool { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public int revision { get; set; }
    }
}
