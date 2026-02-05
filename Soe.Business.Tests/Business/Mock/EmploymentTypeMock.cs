using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace Soe.Business.Tests.Business.Mock
{
    public static class EmploymentTypeMock
    {
        private static int id = 1;

        public static List<EmploymentTypeDTO> Mock()
        {
            #region Generate scipt
            /*
             * 
             * select top 30 'l.Add(Create(' + CAST(EmploymentType.Type as nvarchar) + ',"' + ISNULL(EmploymentType.Name, '') + '","' + ISNULL(EmploymentType.Code, '') + '","' + ISNULL(EmploymentType.Description, '') + '"));' from EmploymentType where EmploymentType.ActorCompanyId = 701609 and EmploymentType.State = 0 order by EmploymentType.Name
             */
            #endregion

            id = 1;
            var l = new List<EmploymentTypeDTO>();

            //Sys
            l.Add(Create(0, "Okänd"));
            l.Add(Create(1, "Provanställning"));
            l.Add(Create(2, "Vikariat"));
            l.Add(Create(3, "Semestervikarie"));
            l.Add(Create(4, "Tillsvidareanställning"));
            l.Add(Create(5, "Allmän visstidsanställning2"));
            l.Add(Create(6, "Säsongsarbete"));
            l.Add(Create(7, "Visst arbete"));
            l.Add(Create(8, "Praktikantanställning"));
            l.Add(Create(9, "Tjänsteman som uppnått den ordinarie pensionsåldern enligt ITP-planen"));
            l.Add(Create(10, "Behovsanställning"));
            l.Add(Create(11, "Tidsbegränsad anställning för personer fyllda 67 år(enligt lag)"));
            l.Add(Create(12, "Allmän visstidsanställning 14 dagar"));
            l.Add(Create(13, "Lärling"));
            //Comp
            l.Add(Create(5, "Temporärt behov av extra arbetskraft", "", ""));

            return l;
        }

        private static EmploymentTypeDTO Create(int type, string name)
        {
            return new EmploymentTypeDTO(type, name);
        }
        private static EmploymentTypeDTO Create(int type, string name, string code, string description)
        {
            return new EmploymentTypeDTO()
            {
                EmploymentTypeId = id++,
                Type = type,
                Name = name,
                Description = description,
                Code = code,
                State = SoftOne.Soe.Common.Util.SoeEntityState.Active,
                Active = true
            };
        }
    }
}
