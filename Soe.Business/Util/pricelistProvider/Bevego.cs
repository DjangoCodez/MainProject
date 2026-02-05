using SoftOne.Soe.Common.Util;
using System;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Bevego : CSVProviderBase
    {
        private enum BevegoColumnPositions
        {
            /// <summary>
            /// Artikelnummer
            /// </summary>
            ProductId = 0,
            /// <summary>
            /// Benämning
            /// </summary>
            Name = 1,
            /// <summary>
            /// Bruttopris
            /// </summary>
            Price = 2,
            /// <summary>
            /// Artikelgrupp
            /// </summary>
            ArticleGroup = 3,
            Unit = 4,
            Discount = 5,
            /// <summary>
            /// Nettopris
            /// </summary>
            PurchasePrice = 6
        }

        protected override string WholesellerName
        {
            get { return Enum.GetName(typeof(SoeCompPriceListProvider), SoeCompPriceListProvider.Bevego); }
        }

        protected override bool ContainsHeaderRow { get { return true; } }

        protected override GenericProduct ToGenericProduct(string[] columns)
        {
            return new GenericProduct()
            {
                ProductId = columns[(int)BevegoColumnPositions.ProductId].Trim(),
                Name = columns[(int)BevegoColumnPositions.Name].Trim(),
                // Fetch from purchase price since discount is added to that price.
                Price = Convert.ToDecimal(columns[(int)BevegoColumnPositions.PurchasePrice].Replace('.', ',').Trim()),
                Code = columns[(int)BevegoColumnPositions.ArticleGroup].Trim(),
                PurchaseUnit = columns[(int)BevegoColumnPositions.Unit].Trim(),
                SalesUnit = columns[(int)BevegoColumnPositions.Unit].Trim(),
                WholesellerName = WholesellerName,
            };
        }

        public static string NameToUrl(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            //Sedan tar man artikelbeskrivningen och byter ut åäö mot aao och sedan mellanslag mot ’–’ och kommatecken mfl mot ’-’.
            //Exempel artikelbeskrivning TÄTNINGSMEDEL A95 GRÅ 0,3L
            //För att då kunna bygga rätt sökväg utifrån denna så skulle det då bli https://www.bevego.se/kategorier/TATNINGSMEDEL-A95-GRA-0-3L 
            //TAK- & VÄGGTÄTNING FLEX ULTIPRO GRÅ 4000X1250 MM				
            //ska bli tak-vaggtatning-flex-ultipro-gra-4000x1250-mm
            var value = ReplaceSpecialCharacters(name.ToLower()).Replace(" ", "-").Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
            return StringUtility.RemoveConsecutiveCharacters(value, '-');
        }

        public static string ReplaceSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string pattern = @"[,\.#&/!%<>_:+=]";
            return Regex.Replace(input, pattern, "-");
        }
    }
}
