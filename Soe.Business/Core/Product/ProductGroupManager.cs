using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoftOne.Soe.Data;


namespace SoftOne.Soe.Business.Core
{
    public class ProductGroupManager : ManagerBase
    {

        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public ProductGroupManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region ProductGroup

        public List<ProductGroup> GetProductGroups(int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductGroup.NoTracking();
            return GetProductGroups(entities, actorCompanyId);
        }

        public List<ProductGroup> GetProductGroups(CompEntities entites, int actorCompanyId)
        {
            return (from p in entites.ProductGroup
                    where p.Company.ActorCompanyId == actorCompanyId
                    orderby p.Code
                    select p).ToList();
        }

        public ProductGroup GetProductGroup(int productGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductGroup.NoTracking();
            return GetProductGroup(entities, productGroupId);
        }

        public ProductGroup GetProductGroup(CompEntities entites, int productGroupId)
        {
            return (from x in entites.ProductGroup
                    where x.ProductGroupId == productGroupId
                    select x).FirstOrDefault();
        }

        public ProductGroup GetProductGroup(string code)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductGroup.NoTracking();
            return GetProductGroup(entities, base.ActorCompanyId, code);
        }
        public ProductGroup GetProductGroup(CompEntities entities, int actorCompanyId, string code)
        {
            return (from x in entities.ProductGroup
                    where x.Company.ActorCompanyId == actorCompanyId && x.Code.ToLower() == code.ToLower()
                    select x).FirstOrDefault();
        }

        public ProductGroup GetProductGroup(int actorCompanyId, int productGroupId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductGroup.NoTracking();
            return GetProductGroup(entities, actorCompanyId, productGroupId);
        }

        public ProductGroup GetProductGroup(CompEntities entites, int actorCompanyId, int productGroupId)
        {
            return (from x in entites.ProductGroup
                    where x.ProductGroupId == productGroupId && x.Company.ActorCompanyId == actorCompanyId
                    select x).FirstOrDefault();
        }

        public ProductGroupDTO GetProductGroupDTO(CompEntities entites, int productGroupId, int actorCompanyId)
        {
            if (productGroupId == 0)
                return null;

            string cacheKey = $"ProductGroupDTO#productGroupId{productGroupId}#actorCompanyId{actorCompanyId}";

            var group = BusinessMemoryCache<ProductGroupDTO>.Get(cacheKey);

            if (group == null)
            {
                group = (from x in entites.ProductGroup
                         where x.ProductGroupId == productGroupId && x.Company.ActorCompanyId == actorCompanyId
                         select x).Select(x => new ProductGroupDTO
                         {
                             Code = x.Code,
                             Name = x.Name
                         }).FirstOrDefault();

                if (group != null)
                {
                    BusinessMemoryCache<ProductGroupDTO>.Set(cacheKey, group, 120);
                }

            }

            return group;
        }
        public ProductGroup GetPrevNextProductGroup(int productGroupId, int actorCompanyId, SoeFormMode mode)
        {
            ProductGroup productGroup = null;
            List<ProductGroup> productGroups = GetProductGroups(actorCompanyId);

            if (mode == SoeFormMode.Next)
            {
                productGroup = (from pg in productGroups
                                where pg.ProductGroupId > productGroupId
                                orderby pg.ProductGroupId ascending
                                select pg).FirstOrDefault();
            }
            else
            {
                productGroup = (from pg in productGroups
                                where pg.ProductGroupId < productGroupId
                                orderby pg.ProductGroupId descending
                                select pg).FirstOrDefault();
            }

            return productGroup;
        }

        public bool PriceGroupExist(string code, int actorCompanyId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.ProductGroup.NoTracking();
            return PriceGroupExist(entities, code, actorCompanyId);
        }

        public bool PriceGroupExist(CompEntities entities, string code, int actorCompanyId)
        {
            return (from pg in entities.ProductGroup
                    where pg.Code == code &&
                    pg.Company.ActorCompanyId == actorCompanyId
                    select pg).Any();
        }

        public ActionResult AddProductGroup(int actorCompanyId, ProductGroup productGroup)
        {
            if (productGroup == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ProductGroup");

            if (PriceGroupExist(productGroup.Code, actorCompanyId))
                return new ActionResult((int)ActionResultSave.ProductGroupExists);

            using (CompEntities entities = new CompEntities())
            {
                productGroup.Company = CompanyManager.GetCompany(entities, actorCompanyId);
                if (productGroup.Company == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11888, "Företaget hittades inte"));

                return AddEntityItem(entities, productGroup, "ProductGroup");
            }
        }

        public ActionResult UpdateProductGroup(ProductGroup productGroup, int actorCompanyId)
        {
            if (productGroup == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "ProductGroup");

            using (CompEntities entities = new CompEntities())
            {
                var originalProductGroup = GetProductGroup(entities, actorCompanyId, productGroup.ProductGroupId);
                if (originalProductGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ProductGroup");

                return UpdateEntityItem(entities, originalProductGroup, productGroup, "ProductGroup");
            }
        }

        public ActionResult DeleteProductGroup(ProductGroup productGroup, int actorCompanyId)
        {
            if (productGroup == null)
                return new ActionResult((int)ActionResultDelete.EntityIsNull, "ProductGroup");

            using (CompEntities entities = new CompEntities())
            {
                var originalProductGroup = GetProductGroup(entities, actorCompanyId, productGroup.ProductGroupId);
                if (originalProductGroup == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "ProductGroup");
                return DeleteEntityItem(entities, originalProductGroup);
            }
        }

        #endregion
    }
}
