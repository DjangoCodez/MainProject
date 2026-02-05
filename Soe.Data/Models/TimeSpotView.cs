using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region TimeSpot

        #region TimeSpotEmployeeView

        public static TimeSpotEmployeeViewDTO ToDTO(this TimeSpotEmployeeView e)
        {
            if (e == null)
                return null;

            return new TimeSpotEmployeeViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                CardNumber = e.CardNumber,
                Company = e.Company,
                EmployeeNr = e.EmployeeNr,
                Name = e.Name,
                Status = e.Status,
                Dialog = e.Dialog,
                CostPlace = e.CostPlace,
                Created = e.Created,
                modified = e.modified,
            };
        }

        public static List<TimeSpotEmployeeViewDTO> ToDTOs(this List<TimeSpotEmployeeView> l)
        {
            var dtos = new List<TimeSpotEmployeeViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }            
            }
            return dtos;
        }

        #endregion

        #region TimeSpotTimeCodeView

        public static TimeSpotTimeCodeViewDTO ToDTO(this TimeSpotTimeCodeView e)
        {
            if (e == null)
                return null;

            return new TimeSpotTimeCodeViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                id = e.id,
                Name = e.Name,
                Type = e.Type,
                Created = e.Created,
                Modified = e.Modified,
            };
        }

        public static List<TimeSpotTimeCodeViewDTO> ToDTOs(this List<TimeSpotTimeCodeView> l)
        {
            var dtos = new List<TimeSpotTimeCodeViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeSpotTimeCodeViewForEmployee

        public static TimeSpotTimeCodeViewForEmployeeDTO ToDTO(this TimeSpotTimeCodeViewForEmployee e)
        {
            if (e == null)
                return null;

            return new TimeSpotTimeCodeViewForEmployeeDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                id = e.id,
                Name = e.Name,
                Type = e.Type,
                Created = e.Created,
                Modified = e.Modified,
            };
        }

        public static List<TimeSpotTimeCodeViewForEmployeeDTO> ToDTOs(this List<TimeSpotTimeCodeViewForEmployee> l)
        {
            var dtos = new List<TimeSpotTimeCodeViewForEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeSpotTimeStampView

        public static TimeSpotTimeStampViewDTO ToDTO(this TimeSpotTimeStampView e)
        {
            if (e == null)
                return null;

            return new TimeSpotTimeStampViewDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                Id = e.Id,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.EmployeeNr,
                Time = e.Time,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Type = e.Type,
                TimeTerminalName = e.TimeTerminalName,
                TimeTerminalId = e.TimeTerminalId,
                TimeDeviationCauseName = e.TimeDeviationCauseName,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                AccountNr = e.AccountNr,
                AccountName = e.AccountName,
                Changed = e.Changed,
            };
        }

        public static List<TimeSpotTimeStampViewDTO> ToDTOs(this List<TimeSpotTimeStampView> l)
        {
            var dtos = new List<TimeSpotTimeStampViewDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #endregion
    }
}
