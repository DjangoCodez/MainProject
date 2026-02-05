using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public sealed partial class TimeEngineManager : TimeEngine
    {
        public TimeEngineManager(ParameterObject parameterObject, int actorCompanyId, int userId)
            : base(parameterObject, actorCompanyId, userId)
        {

        }

        public TimeEngineManager(ParameterObject parameterObject, int actorCompanyId, int userId, CompEntities entities)
            : base(parameterObject, actorCompanyId, userId, entities)
        {
            this.actorCompanyId = actorCompanyId;
            this.userId = userId;
        }
    }

    public partial class TimeEngine : ManagerBase
    {
        public TimeEngine(ParameterObject parameterObject, int actorCompanyId, int userId)
            : base(parameterObject)
        {
            this.actorCompanyId = actorCompanyId;
            this.userId = userId;
        }

        public TimeEngine(ParameterObject parameterObject, int actorCompanyId, int userId, CompEntities entities)
            : base(parameterObject)
        {
            this.actorCompanyId = actorCompanyId;
            this.userId = userId;
            this.entities = entities;
            this.forcedExternalEntities = true;
        }
    }
}

