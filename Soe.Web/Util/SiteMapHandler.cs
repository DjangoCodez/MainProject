using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SoftOne.Soe.Web.Util
{
    public class SiteMapHandler
    {
        #region Cache variables

        private List<Node> nodesInSession;
        private List<Node> NodesInSession
        {
            get
            {
                if (nodesInSession == null)
                    nodesInSession = new List<Node>();
                return nodesInSession;
            }
            set
            {
                nodesInSession = value;
            }
        }

        #endregion

        public string GetUrlQS(string title, string url, string qs)
        {
            var nodes = (from n in NodesInSession
                         where ((n.Title == title) && (n.Url == url))
                         select n).ToList<Node>();

            foreach (var node in nodes)
            {
                NameValueCollection values = node.QS;
                string[] keys = values.AllKeys;
                for (int i = 0; i < keys.Length; i++)
                {
                    string key = keys[i];
                    if (key == qs)
                        return values.Get(key);
                }
            }

            return String.Empty;
        }

        public void AddNode(System.Web.SiteMapNode node, NameValueCollection qs)
        {
            int level = Convert.ToInt32(node["level"]);

            //Remove obsolete
            var nodes = (from n in NodesInSession
                         where n.Level < level
                         select n).ToList<Node>();

            //Add new
            nodes.Add(new Node()
            {
                Level = level,
                Title = node.Title,
                Url = node.Url,
                QS = qs,
            });

            NodesInSession = nodes;
        }
    }

    #region Help-classes

    class Node
    {
        public int Level { get; set; }
        public string Title { get; set; }
        public string Url { get; set; } //without qs
        public NameValueCollection QS { get; set; }
    }

    #endregion
}
