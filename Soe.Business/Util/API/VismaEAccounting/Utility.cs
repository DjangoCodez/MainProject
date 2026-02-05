using SoftOne.Soe.Business.Util.API.Shared;
using SoftOne.Soe.Common.DTO;

namespace SoftOne.Soe.Business.Util.API.VismaEAccounting
{
    static class VismaUtility
    {
        public static VismaHouseWorkType GetVismaHouseWorkType(int number)
        {
            var type = (SysHouseholdDeductionWorkTypes)number;
            switch (type)
            {
                case SysHouseholdDeductionWorkTypes.Construction:
                    return VismaHouseWorkType.RotConstructionWork;
                case SysHouseholdDeductionWorkTypes.Electricity:
                    return VismaHouseWorkType.RotElectricalWork;
                case SysHouseholdDeductionWorkTypes.GlassMetalWork:
                    return VismaHouseWorkType.RotGlassSheetMetalWork;
                case SysHouseholdDeductionWorkTypes.GroundDrainageWork:
                    return VismaHouseWorkType.RotGroundWork;
                case SysHouseholdDeductionWorkTypes.Masonry:
                    return VismaHouseWorkType.RotBrickWork;
                case SysHouseholdDeductionWorkTypes.PaintingWallPapering:
                    return VismaHouseWorkType.RotPaintDecorateWork;
                case SysHouseholdDeductionWorkTypes.HVAC:
                    return VismaHouseWorkType.RotPlumbWork;
                case SysHouseholdDeductionWorkTypes.Cleaning:
                    return VismaHouseWorkType.RutCleanJobWork;
                case SysHouseholdDeductionWorkTypes.TextileClothing:
                    return VismaHouseWorkType.RutCareClothTextile;
                case SysHouseholdDeductionWorkTypes.Cooking:
                    return VismaHouseWorkType.RutCook;
                case SysHouseholdDeductionWorkTypes.SnowPlowing:
                    return VismaHouseWorkType.RutSnowRemove;
                case SysHouseholdDeductionWorkTypes.Gardening:
                    return VismaHouseWorkType.RutGarden;
                case SysHouseholdDeductionWorkTypes.Babysitting:
                    return VismaHouseWorkType.RutBabySitt;
                case SysHouseholdDeductionWorkTypes.OtherCare:
                    return VismaHouseWorkType.RutOtherCare;
                case SysHouseholdDeductionWorkTypes.MovingServices:
                    return VismaHouseWorkType.RutRemovalServices; //!!! Is this correct?
                case SysHouseholdDeductionWorkTypes.ITServices:
                    return VismaHouseWorkType.RutITServices;
                case SysHouseholdDeductionWorkTypes.MajorApplianceRepair:
                    return VismaHouseWorkType.RutHomeAppliances;
                case SysHouseholdDeductionWorkTypes.Furnishing:
                    return VismaHouseWorkType.RutFurnishing;
                case SysHouseholdDeductionWorkTypes.HouseSupervision:
                    return VismaHouseWorkType.RutHomeSupervision;
                case SysHouseholdDeductionWorkTypes.SalesTransport:
                    return VismaHouseWorkType.RutGoodsTransport;
                case SysHouseholdDeductionWorkTypes.LaundryAtFacility:
                    return VismaHouseWorkType.RutLaundry;
                case SysHouseholdDeductionWorkTypes.Tutoring:
                case SysHouseholdDeductionWorkTypes.OtherCost:
                default:
                    return VismaHouseWorkType.None; //!!!Could not find matching type.

            }
        }
        public static VismaGreenTechnologyType GetVismaGreenTechnologyType(int number)
        {
            var type = (SysHouseholdDeductionWorkTypes)number;
            switch (type)
            {
                case SysHouseholdDeductionWorkTypes.NetworkConnectedSolarPower:
                    return VismaGreenTechnologyType.SolarCellInstallation;
                case SysHouseholdDeductionWorkTypes.SystemForEnergyStorage:
                    return VismaGreenTechnologyType.ElectricEnergyStorageInstallation;
                case SysHouseholdDeductionWorkTypes.ChargingForElectricalVehicle:
                    return VismaGreenTechnologyType.ElectricVehicleChargingPostStation;
                default:
                    return VismaGreenTechnologyType.None;
            }
        }

        public static bool IsGreenWork(int number)
        {
            return HouseWorkType(number) == 1;
        }
        public static bool IsHouseWork(int number)
        {
            return HouseWorkType(number) == -1;
        }

        public static int HouseWorkType(int number)
        {
            var type = (SysHouseholdDeductionWorkTypes)number;
            switch (type)
            {
                case SysHouseholdDeductionWorkTypes.Construction:
                case SysHouseholdDeductionWorkTypes.Electricity:
                case SysHouseholdDeductionWorkTypes.GlassMetalWork:
                case SysHouseholdDeductionWorkTypes.GroundDrainageWork:
                case SysHouseholdDeductionWorkTypes.Masonry:
                case SysHouseholdDeductionWorkTypes.PaintingWallPapering:
                case SysHouseholdDeductionWorkTypes.HVAC:
                case SysHouseholdDeductionWorkTypes.Cleaning:
                case SysHouseholdDeductionWorkTypes.TextileClothing:
                case SysHouseholdDeductionWorkTypes.SnowPlowing:
                case SysHouseholdDeductionWorkTypes.Gardening:
                case SysHouseholdDeductionWorkTypes.Babysitting:
                case SysHouseholdDeductionWorkTypes.OtherCare:
                case SysHouseholdDeductionWorkTypes.Tutoring:
                case SysHouseholdDeductionWorkTypes.MovingServices:
                case SysHouseholdDeductionWorkTypes.ITServices:
                case SysHouseholdDeductionWorkTypes.MajorApplianceRepair:
                case SysHouseholdDeductionWorkTypes.Cooking:
                case SysHouseholdDeductionWorkTypes.Furnishing:
                case SysHouseholdDeductionWorkTypes.HouseSupervision:
                case SysHouseholdDeductionWorkTypes.SalesTransport:
                case SysHouseholdDeductionWorkTypes.LaundryAtFacility:
                    return -1;
                case SysHouseholdDeductionWorkTypes.NetworkConnectedSolarPower:
                case SysHouseholdDeductionWorkTypes.SystemForEnergyStorage:
                case SysHouseholdDeductionWorkTypes.ChargingForElectricalVehicle:
                    return 1;
                default:
                    return 0;
            }
        }

        public static VismaHouseholdDeductionType GetHouseworkDeductionType(int number)
        {
            var type = (SysHouseholdDeductionWorkTypes)number;
            switch (type)
            {
                case SysHouseholdDeductionWorkTypes.Construction:
                case SysHouseholdDeductionWorkTypes.Electricity:
                case SysHouseholdDeductionWorkTypes.GlassMetalWork:
                case SysHouseholdDeductionWorkTypes.GroundDrainageWork:
                case SysHouseholdDeductionWorkTypes.Masonry:
                case SysHouseholdDeductionWorkTypes.PaintingWallPapering:
                case SysHouseholdDeductionWorkTypes.HVAC:
                    return VismaHouseholdDeductionType.Rot;
                case SysHouseholdDeductionWorkTypes.Cleaning:
                case SysHouseholdDeductionWorkTypes.TextileClothing:
                case SysHouseholdDeductionWorkTypes.SnowPlowing:
                case SysHouseholdDeductionWorkTypes.Gardening:
                case SysHouseholdDeductionWorkTypes.Babysitting:
                case SysHouseholdDeductionWorkTypes.OtherCare:
                case SysHouseholdDeductionWorkTypes.Tutoring:
                case SysHouseholdDeductionWorkTypes.MovingServices:
                case SysHouseholdDeductionWorkTypes.ITServices:
                case SysHouseholdDeductionWorkTypes.MajorApplianceRepair:
                case SysHouseholdDeductionWorkTypes.Cooking:
                case SysHouseholdDeductionWorkTypes.Furnishing:
                case SysHouseholdDeductionWorkTypes.HouseSupervision:
                case SysHouseholdDeductionWorkTypes.SalesTransport:
                case SysHouseholdDeductionWorkTypes.LaundryAtFacility:
                    return VismaHouseholdDeductionType.Rut;
                case SysHouseholdDeductionWorkTypes.NetworkConnectedSolarPower:
                case SysHouseholdDeductionWorkTypes.SystemForEnergyStorage:
                case SysHouseholdDeductionWorkTypes.ChargingForElectricalVehicle:
                case SysHouseholdDeductionWorkTypes.OtherCost:
                default:
                    return VismaHouseholdDeductionType.Normal;

            }
        }
        public static bool IsOtherCosts(CustomerInvoiceRowDistributionDTO row)
        {
            return row.Product != null && row.Product.HouseholdDeductionType == (int)SysHouseholdDeductionWorkTypes.OtherCost;
        }
        public static decimal MinutesToHours(int minutes)
        {
            return (decimal)minutes / 60;
        }
    }
}
