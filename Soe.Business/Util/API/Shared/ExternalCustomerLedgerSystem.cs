using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.Shared
{
    public interface IExternalInvoiceSystem
    {
        string Name { get; }
        ExternalInvoiceSystemParameters Params { get; }
        void SetAuthFromRefreshToken(string refreshToken);
        void SetAuthFromCode(string code);
        string GetRefreshToken();
        ActionResult AddInvoice(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customer, List<CustomerInvoiceRowDistributionDTO> invoiceRows);
        void AddCustomer(CustomerDistributionDTO customer, CustomerDTO customerInput);
    }

    public class ExternalInvoiceSystemParameters
    {
        public Feature Feature { get; set; }
        public TermGroup_EDistributionType DistributionStatusType { get; set; }
        public CompanySettingType RefreshTokenStoragePoint { get; set; }
        public CompanySettingType LastSyncStoragePoint { get; set; }
    }

    public static class InvoiceIntegrationUtility
    {
        public static bool IsDeductionTextRow(CustomerInvoiceRowDistributionDTO row)
        {
            //Only Swedish for now
            var text = row.Text;
            if (string.IsNullOrEmpty(text))
                return false;

            return text.StartsWith("Grön teknik-avdrag") || text.StartsWith("ROT-avdrag") || text.StartsWith("RUT-avdrag");
        }
        public static string GetPropertyDesignation(HouseholdTaxDeductionRowDTO hhd)
        {
            if (!string.IsNullOrEmpty(hhd.Property) && !string.IsNullOrEmpty(hhd.ApartmentNr))
            {
                return $"{hhd.Property} {hhd.ApartmentNr}";
            }
            return $"{hhd.Property}{hhd.ApartmentNr}";
        }
    }

    public enum SysHouseholdDeductionWorkTypes
    {
        Construction = 1,
        Electricity = 2,
        GlassMetalWork = 3,
        GroundDrainageWork = 4,
        Masonry = 5,
        PaintingWallPapering = 6,
        HVAC = 7,
        Cleaning = 8,
        TextileClothing = 9,
        Cooking = 10,
        SnowPlowing = 11,
        Gardening = 12,
        Babysitting = 13,
        OtherCare = 14,
        Tutoring = 15,
        OtherCost = 16,
        MovingServices = 17,
        ITServices = 18,
        MajorApplianceRepair = 19,
        NetworkConnectedSolarPower = 20,
        SystemForEnergyStorage = 21,
        ChargingForElectricalVehicle = 22,
        Furnishing = 23,
        HouseSupervision = 24,
        SalesTransport = 25,
        LaundryAtFacility = 26,
    }
}
