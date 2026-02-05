using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFeatureTreeView : SoeTreeView
	{
        public SoeFeatureTreeView() : base()
        {

        }

		protected override TreeNode CreateNode()
		{
			return new SoeFeatureTreeNode(this, false);
		}
	}

    public class SoeCompanyRoleTreeView : SoeTreeView
    {
        public SoeCompanyRoleTreeView() : base()
        {

        }

        protected override TreeNode CreateNode()
        {
            return new SoeCompanyRoleTreeNode(this, false);
        }
    }

    public class SoeTreeView : TreeView
    {
        public SoeTreeView() : base()
        {
            this.CssClass = "TreeView";
        }
    }
}
