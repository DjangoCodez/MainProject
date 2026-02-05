using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public static class SysVehicleXMLUtility
    {
        public const string NODE_MANUFACTURINGYEAR = "Tillverkningsar";
        public const string NODE_MAKE = "Marke";
        public const string NODE_MODEL = "Modell";
        public const string NODE_MODEL_CODE = "Kod";
        public const string NODE_PRICE_PER_MODEL = "NybilsprisPerModell";
        public const string NODE_PRICE = "Nybilspris";
        public const string NODE_FUEL_TYPE = "BransleTyp";
        public const string NODE_CODE_FOR_COMPARABLE_MODEL = "KodForJamforbarModell";
        public const string NODE_PRICE_ADJUSTMENT = "Justering";
        public const string NODE_PIRCE_AFTERREDUCTION = "VardeEfterSchablon";

        public const string FUEL_TYPE_GASOLINE = "Bensin";
        public const string FUEL_TYPE_DIESEL = "Diesel";
        public const string FUEL_TYPE_ELECTRICITY = "El";
        public const string FUEL_TYPE_GAS = "Gas";
        public const string FUEL_TYPE_ALCOHOL = "Alkohol";
        public const string FUEL_TYPE_ELECTRIC_HYBRID = "Elhybrid";
        public const string FUEL_TYPE_PLUGIN_HYBRID = "Laddhybrid";
        public const string FUEL_TYPE_HYDROGEN_GAS = "Vätgas";

        public static string GetSysVehicleTypeString(TermGroup_VehicleType type)
        {
            // Get vehicle type xml tag
            string typeString = String.Empty;
            switch (type)
            {
                case TermGroup_VehicleType.Car:
                    typeString = "Personbil";
                    break;
                case TermGroup_VehicleType.Lorry:
                    typeString = "LattLastbil";
                    break;
            }

            return typeString;
        }

        public static TermGroup_SysVehicleFuelType GetFuelTypeByString(String fuelTypeString)
        {
            if (fuelTypeString == FUEL_TYPE_GASOLINE)
                return TermGroup_SysVehicleFuelType.Gasoline;
            else if (fuelTypeString == FUEL_TYPE_DIESEL)
                return TermGroup_SysVehicleFuelType.Diesel;
            else if (fuelTypeString == FUEL_TYPE_ELECTRICITY)
                return TermGroup_SysVehicleFuelType.Electricity;
            else if (fuelTypeString == FUEL_TYPE_GAS)
                return TermGroup_SysVehicleFuelType.Gas;
            else if (fuelTypeString == FUEL_TYPE_ALCOHOL)
                return TermGroup_SysVehicleFuelType.Alcohol;
            else if (fuelTypeString == FUEL_TYPE_ELECTRIC_HYBRID)
                return TermGroup_SysVehicleFuelType.ElectricHybrid;
            else if (fuelTypeString == FUEL_TYPE_PLUGIN_HYBRID)
                return TermGroup_SysVehicleFuelType.PlugInHybrid;
            else if (fuelTypeString == FUEL_TYPE_HYDROGEN_GAS)
                return TermGroup_SysVehicleFuelType.HydrogenGas;
            else
                return TermGroup_SysVehicleFuelType.Unknown;
        }

        public static string GetSysVehicleXMLNamespace(XDocument xmlDoc)
        {
            // Default namespace
            string xmlns = "se/skatteverket/bilformansberakning/nybilspriser/3.0";

            if (xmlDoc != null)
            {
                XElement elem = xmlDoc.Elements().FirstOrDefault();
                if (elem != null)
                {
                    XNamespace ns = elem.GetDefaultNamespace();
                    if (ns != null)
                        xmlns = ns.NamespaceName;
                }
            }

            return xmlns;
        }

        public static string GetNodeNameWithNamespace(XDocument xmlDoc, string nodeName)
        {
            return "{" + GetSysVehicleXMLNamespace(xmlDoc) + "}" + nodeName;
        }

        public static List<XElement> GetVehicleElements(XDocument xmlDoc, TermGroup_VehicleType type)
        {
            return XmlUtil.GetChildElements(xmlDoc, GetSysVehicleTypeString(type));
        }

        public static List<string> GetVehicleMakes(XDocument xmlDoc, TermGroup_VehicleType type)
        {
            List<string> makes = new List<string>();

            List<XElement> vehicles = GetVehicleElements(xmlDoc, type);
            if (vehicles != null)
            {
                foreach (XElement vehicle in vehicles)
                {
                    string make = XmlUtil.GetChildElementValue(vehicle, NODE_MAKE);
                    if (!String.IsNullOrEmpty(make))
                        makes.Add(make);
                }
            }

            return makes;
        }

        public static List<GenericType<string, string>> GetSysVehicleModels(XDocument xmlDoc, TermGroup_VehicleType type, string make)
        {
            List<GenericType<string, string>> models = new List<GenericType<string, string>>();

            List<XElement> vehicles = GetVehicleElements(xmlDoc, type);
            if (vehicles != null)
            {
                foreach (XElement vehicle in vehicles)
                {
                    if (XmlUtil.GetChildElementValue(vehicle, NODE_MAKE) == make)
                    {
                        List<XElement> mdls = XmlUtil.GetChildElements(vehicle, NODE_PRICE_PER_MODEL);
                        foreach (XElement mdl in mdls)
                        {
                            string model = XmlUtil.GetChildElementValue(mdl, NODE_MODEL);
                            string modelCode = XmlUtil.GetChildElementValue(mdl, NODE_MODEL_CODE);
                            if (!String.IsNullOrEmpty(model))
                                models.Add(new GenericType<string, string>() { Field1 = modelCode, Field2 = model });
                        }
                        break;
                    }
                }
            }

            return models;
        }
    }
}
