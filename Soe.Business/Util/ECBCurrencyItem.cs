using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class ECBCurrencyItem
    {
        #region Variables

        private XDocument xdoc;
        private List<string> currencyCodesFilter;
        private bool hasCurrencyCodesFilter
        {
            get
            {
                return currencyCodesFilter != null && currencyCodesFilter.Count > 0;
            }
        }
        private XElement ElementCube { get; set; }
        private XElement ElementCubeTime { get; set; }
        private List<XElement> ElementCubes { get; set; }

        public DateTime Date { get; set; }
        public Dictionary<TermGroup_Currency, decimal> EuroRates { get; set; }
        public string Error { get; set; } = string.Empty;

        #endregion

        #region Ctor

        public ECBCurrencyItem(XDocument xdoc, List<string> currencyCodesFilter = null)
        {
            this.xdoc = xdoc;
            this.currencyCodesFilter = currencyCodesFilter;
            this.EuroRates = new Dictionary<TermGroup_Currency, decimal>();

            Parse();
        }

        #endregion

        #region Public methods

        #endregion

        #region Private methods

        private void Parse()
        {
            ElementCube = XmlUtil.GetChildElement(xdoc, "Cube");
            if (ElementCube != null)
            {
                ElementCubeTime = XmlUtil.GetChildElement(ElementCube, "Cube", true);
                if (ElementCubeTime != null)
                {
                    var attributeTime = ElementCubeTime.Attributes("time").FirstOrDefault();

                    DateTime date;
                    if (DateTime.TryParse(attributeTime.Value, out date))
                    {
                        this.Date = date;

                        ElementCubes = XmlUtil.GetChildElements(ElementCubeTime, "Cube");
                        foreach (var cube in ElementCubes)
                        {
                            var attributeCurrency = cube.Attributes("currency").FirstOrDefault();
                            var attributeRate = cube.Attributes("rate").FirstOrDefault();
                            if (attributeCurrency != null && attributeRate != null)
                            {
                                if (IsCurrencyCodeValid(attributeCurrency.Value))
                                {
                                    foreach (TermGroup_Currency currency in Enum.GetValues(typeof(TermGroup_Currency)))
                                    {
                                        if (currency.ToString() == attributeCurrency.Value)
                                        {
                                            if (decimal.TryParse(attributeRate.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
                                            {
                                                this.EuroRates.Add(currency, rate);
                                                break;
                                            }
                                            else
                                            {
                                                this.Error = $"Error parsing rate for currency {attributeCurrency.Value}: '{attributeRate.Value}'";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsCurrencyCodeValid(string code)
        {
            if (!this.hasCurrencyCodesFilter)
                return true;

            return this.currencyCodesFilter.Contains(code);
        }

        #endregion
    }
}
