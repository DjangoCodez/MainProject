using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {

        public static ProjectGridDTO ToGridDTO(this Project e, bool includeManagerName, bool loadOrders, string sortByOrderNr = null)
        {
            if (e == null)
                return null;
            string managerName = string.Empty;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.CustomerReference.IsLoaded)
                    {
                        e.CustomerReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.CustomerReference");
                    }

                    if (loadOrders)
                    {
                        if (!e.Invoice.IsLoaded)
                        {
                            e.Invoice.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.Invoice");
                        }

                        foreach (var invoice in e.Invoice)
                        {
                            if (!invoice.OriginReference.IsLoaded)
                            {
                                invoice.OriginReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("invoice.OriginReference");
                            }
                        }
                    }

                    if (includeManagerName)
                    {
                        if (!e.ProjectUser.IsLoaded)
                        {
                            e.ProjectUser.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.ProjectUser");
                        }

                        var projectManagers = e.ProjectUser.Where(pu => pu.Type == (int)TermGroup_ProjectUserType.Manager && pu.State == (int)SoeEntityState.Active);
                        if (projectManagers != null)
                        {
                            bool first = true;
                            foreach (var projectManager in projectManagers)
                            {
                                if (!projectManager.UserReference.IsLoaded)
                                {
                                    projectManager.UserReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("projectManager.UserReference");
                                }

                                if (!projectManager.User.ContactPersonReference.IsLoaded)
                                {
                                    projectManager.User.ContactPersonReference.Load();
                                    DataProjectLogCollector.LogLoadedEntityInExtension("projectManager.User.ContactPersonReference");
                                }

                                if (projectManager.User != null)
                                    managerName += (first ? "" : ", ") + (projectManager.User.ContactPerson != null ? projectManager.User.ContactPerson.Name : projectManager.User.Name);

                                first = false;
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            ProjectGridDTO dto = new ProjectGridDTO()
            {
                ProjectId = e.ProjectId,
                ParentProjectId = e.ParentProjectId,
                CustomerNr = e.Customer != null ? e.Customer.CustomerNr : string.Empty,
                CustomerName = e.Customer != null ? e.Customer.Name : string.Empty,
                Status = (TermGroup_ProjectStatus)e.Status,
                StatusName = e.StatusName,
                Number = e.Number,
                Name = e.Name,
                Description = e.Description,
                State = (SoeEntityState)e.State,
                ManagerName = managerName,
                CustomerId = e.CustomerId,
                Categories = e.Categories,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
            };

            if (loadOrders)
            {
                dto.OrderNr = e.Invoice.Where(i => i.Type == (int)SoeInvoiceType.CustomerInvoice && i.Origin.Type == (int)SoeOriginType.Order).OrderByDescending(i => i.InvoiceNr == sortByOrderNr).Select(i => i.InvoiceNr).JoinToString(", ");
            }
            else
            {
                dto.OrderNr = e.OrderNumbers;
            }

            if (e.ChildProjects != null)
            {
                bool first = true;
                foreach (Project p in e.ChildProjects)
                {
                    if (first)
                        dto.ChildProjects += p.Number + " " + p.Name;
                    else
                        dto.ChildProjects += ", " + p.Number + " " + p.Name;

                    first = false;
                }
            }

            // Extensions
            dto.StatusName = e.StatusName;

            return dto;
        }

        public static IEnumerable<ProjectGridDTO> ToGridDTOs(this IEnumerable<Project> l, bool includeManagerName, bool loadOrders, string sortByOrderNr = null)
        {
            var dtos = new List<ProjectGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(includeManagerName, loadOrders, sortByOrderNr));
                }
            }
            return dtos;
        }

        public static ProjectSearchResultDTO ToSearchResultDTO(this Project e)
        {
            return new ProjectSearchResultDTO
            {
                ProjectId = e.ProjectId,
                Number = e.Number,
                Name = e.Name,
                CustomerNr = e.Customer?.CustomerNr,
                CustomerName = e.Customer?.Name,
                CustomerId = e.CustomerId,
                Status = (TermGroup_ProjectStatus)e.Status,
                OrderNbrs = e.Invoice.Where(i => i.Type == (int)SoeInvoiceType.CustomerInvoice && i.Origin.Type == (int)SoeOriginType.Order).OrderByDescending(i => i.InvoiceNr).Select(i => i.InvoiceNr).ToList()
            };
        }

        public readonly static Expression<Func<Project, ProjectSearchResultDTO>> ProjectSearchResultDTO =
        p => new ProjectSearchResultDTO
        {
            ProjectId = p.ProjectId,
            Number = p.Number,
            Name = p.Name,
            CustomerNr = p.Customer.CustomerNr,
            CustomerName = p.Customer.Name,
            CustomerId = p.CustomerId,
            Status = (TermGroup_ProjectStatus)p.Status,
            ManagerUserId = p.ProjectUser.FirstOrDefault(u => u.State == (int)SoeEntityState.Active && u.Type == (int)TermGroup_ProjectUserType.Manager).UserId,
            OrderNbrs = p.Invoice.Where(i => i.Type == (int)SoeInvoiceType.CustomerInvoice && i.Origin.Type == (int)SoeOriginType.Order).OrderByDescending(i => i.InvoiceNr).Select(i => i.InvoiceNr).ToList()
        };

    }
}
