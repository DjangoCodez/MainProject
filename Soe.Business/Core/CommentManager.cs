using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public class CommentManager : ManagerBase
    {
        #region Variables


        #endregion

        #region Ctor

        public CommentManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        #region Add

        public ActionResult AddComment(string title, string commentText, int? userId, int? companyId, int? roleId, int featureId)
        {
            if (userId == null && companyId == null && roleId == null)
                return new ActionResult(false);

            using (CompEntities entities = new CompEntities())
            {
                CommentHeader commentHeader = new CommentHeader()
                {
                    Title = title,
                    Date = DateTime.Now,
                    FeatureId = featureId,
                };

                Comment comment = new Comment()
                {
                    Comment1 = commentText,
                };
                commentHeader.Comment.Add(comment);

                if (companyId.HasValue)
                    commentHeader.Company = CompanyManager.GetCompany(entities, companyId.Value);
                if (userId.HasValue)
                    commentHeader.User = UserManager.GetUser(entities, userId.Value);
                if (roleId.HasValue)
                    commentHeader.Role = RoleManager.GetRole(entities, roleId.Value);

                return AddEntityItem(entities, commentHeader, "CommentHeader");
            }
        }

        #endregion

        #region Delete

        public ActionResult DeleteCommentHeader(int commentHeaderId)
        {
            using (CompEntities entities = new CompEntities())
            {
                CommentHeader originalCommentHeader = GetCommentHeader(entities, commentHeaderId);
                if (originalCommentHeader == null)
                    return new ActionResult((int)ActionResultDelete.EntityNotFound, "CommentHeader");

                if (!originalCommentHeader.Comment.IsNullOrEmpty())
                    DeleteComment(entities, originalCommentHeader.Comment.First().CommentId);

                return DeleteEntityItem(entities, originalCommentHeader);
            }
        }

        public ActionResult DeleteComment(CompEntities entities, int commentId)
        {
            Comment originalComment = GetComment(entities, commentId);
            if (originalComment == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "Comment");

            return DeleteEntityItem(entities, originalComment);
        }

        #endregion

        #region Get

        #region CommentBody

        public Comment GetComment(CompEntities entities, int commentId)
        {
            return (from e in entities.Comment
                    where (e.CommentId == commentId)
                    select e).First();
        }

        public Comment GetComment(int? userId, int? companyId, int commentId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.Comment.NoTracking();
            return GetComment(entities, userId, companyId, commentId);
        }

        public Comment GetComment(CompEntities entities, int? userId, int? companyId, int commentId)
        {
            if (userId == null && companyId == null)
                return null;

            return (from e in entities.Comment
                    where (e.CommentId == commentId)
                    select e).First();
        }

        #endregion

        #region CommentHeader

        private CommentHeader GetCommentHeader(CompEntities entities, int commentHeaderId)
        {
            return (from e in entities.CommentHeader
                        .Include("Comment")
                    where (e.CommentHeaderId == commentHeaderId)
                    select e).FirstOrDefault();
        }

        public List<CommentHeader> GetCommentHeaders(int? userId, int? companyId, int? roleId, int featureId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CommentHeader.NoTracking();
            return GetCommentHeaders(entities, userId, companyId, roleId, featureId);
        }

        public List<CommentHeader> GetCommentHeaders(CompEntities entities, int? userId, int? companyId, int? roleId, int featureId)
        {
            if (userId == null && companyId == null && roleId == null)
                return null;

            if (userId != null && companyId != null && roleId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where
                            (e.FeatureId == featureId) && ((e.Company.ActorCompanyId == companyId) || (e.User.UserId == userId) || (e.Role.RoleId == roleId))
                        select e).ToList();
            }
            else if (companyId != null && roleId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where (e.FeatureId == featureId) && ((e.Company.ActorCompanyId == companyId) || (e.Role.RoleId == roleId))
                        select e).ToList();
            }
            else if (userId != null && roleId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where (e.FeatureId == featureId) && ((e.User.UserId == userId) || (e.Role.RoleId == roleId))
                        select e).ToList();
            }
            else if (userId != null && companyId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where (e.FeatureId == featureId) && ((e.Company.ActorCompanyId == companyId) || (e.User.UserId == userId))
                        select e).ToList();
            }
            else if (userId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where (e.FeatureId == featureId) && (e.User.UserId == userId)
                        select e).ToList();
            }
            else if (companyId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where (e.Company.ActorCompanyId == companyId) && (e.FeatureId == featureId)
                        select e).ToList();
            }
            else if (roleId != null)
            {
                return (from e in entities.CommentHeader
                            .Include("Company")
                            .Include("Comment")
                        where (e.FeatureId == featureId) && (e.Role.RoleId == roleId)
                        select e).ToList();
            }

            return null;
        }

        #endregion

        #endregion

        #region Exists

        public bool HasComments(int? userId, int? companyId, int? roleId, int featureId)
        {
            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            entities.CommentHeader.NoTracking();
            return HasComments(entities, userId, companyId, roleId, featureId);
        }

        public bool HasComments(CompEntities entities, int? userId, int? companyId, int? roleId, int featureId)
        {
            if (userId == null && companyId == null)
                return false;

            bool exists = false;

            if (userId != null && companyId != null && roleId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.FeatureId == featureId) &&
                         ((e.Company.ActorCompanyId == companyId) || (e.User.UserId == userId) || (e.Role.RoleId == roleId))
                         select e).Any();
            }
            else if (companyId != null && roleId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.FeatureId == featureId) &&
                         ((e.Company.ActorCompanyId == companyId) || (e.Role.RoleId == roleId))
                         select e).Any();
            }
            else if (userId != null && roleId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.FeatureId == featureId) &&
                         ((e.User.UserId == userId) || (e.Role.RoleId == roleId))
                         select e).Any();
            }
            else if (userId != null && companyId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.FeatureId == featureId) &&
                         ((e.Company.ActorCompanyId == companyId) || (e.User.UserId == userId))
                         select e).Any();
            }
            else if (userId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.FeatureId == featureId) &&
                         (e.User.UserId == userId)
                         select e).Any();
            }
            else if (companyId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.Company.ActorCompanyId == companyId) &&
                         (e.FeatureId == featureId)
                         select e).Any();
            }
            else if (roleId != null)
            {
                exists = (from e in entities.CommentHeader
                         where (e.FeatureId == featureId) &&
                         (e.Role.RoleId == roleId)
                         select e).Any();
            }
            return exists;
        }

        #endregion
    }
}

