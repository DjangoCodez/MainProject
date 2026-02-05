using System.Web.UI;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.UI.WebControls
{
    public class SoeFeatureTreeNode : TreeNode
	{
        public int FeatureId { get; set; }
        public string FeatureName { get; set; }
        public int Permission { get; set; }

		public SoeFeatureTreeNode() : base()
		{
		}

		public SoeFeatureTreeNode(TreeView owner, bool isRoot) : base(owner, isRoot)
		{
		}

		protected override void LoadViewState(object state)
		{
			object[] arrState = state as object[];

			FeatureId = (int)arrState[1];
			FeatureName = (string)arrState[2];
			Permission = (int)arrState[3];
			base.LoadViewState(arrState[0]);
		}

		protected override object SaveViewState()
		{
			object[] arrState = new object[4];

			arrState[0] = base.SaveViewState();
			arrState[1] = FeatureId;
			arrState[2] = FeatureName;
			arrState[3] = Permission;

			return arrState;
		}

		protected override void RenderPreText(HtmlTextWriter writer)
		{
			base.RenderPreText(writer);
		}

		protected override void RenderPostText(HtmlTextWriter writer)
		{
			base.RenderPostText(writer);
		}
	}

    public class SoeCompanyRoleTreeNode : TreeNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SoeCompanyRoleTreeNodeType Type { get; set; }

        public SoeCompanyRoleTreeNode()
            : base()
        {
        }

        public SoeCompanyRoleTreeNode(TreeView owner, bool isRoot)
            : base(owner, isRoot)
        {
        }

        protected override void LoadViewState(object state)
        {
            object[] arrState = state as object[];

            Id = (int)arrState[1];
            Name = (string)arrState[2];
            Type = ((SoeCompanyRoleTreeNodeType)((int)arrState[3]));
            base.LoadViewState(arrState[0]);
        }

        protected override object SaveViewState()
        {
            object[] arrState = new object[4];

            arrState[0] = base.SaveViewState();
            arrState[1] = Id;
            arrState[2] = Name;
            arrState[3] = (int)Type;

            return arrState;
        }

        protected override void RenderPreText(HtmlTextWriter writer)
        {
            base.RenderPreText(writer);
        }

        protected override void RenderPostText(HtmlTextWriter writer)
        {
            base.RenderPostText(writer);
        }
    }

    public enum SoeCompanyRoleTreeNodeType
    {
        License = 1,
        Company = 2,
        Role = 3,
    }
}
