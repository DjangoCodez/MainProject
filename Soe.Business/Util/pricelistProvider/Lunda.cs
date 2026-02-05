using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;


//Förutsatt att ni läser in priser från olika kolumner i samma fil är det tre filer ni behöver (och bortse helt från de filer vi kallar Mängd/Amount där vi bara bytt plats på mängd och netto).
//Och att det i ert system från dessa skapas tre olika prislistor. Lunda_Netto, Lunda_Mängd och Lunda_Brutto.
//(Lunda_Netto skulle kanske vara tydligare om ni döpte till Lunda_Styck. Att vi kallar den netto har en historisk anledning, innan vi hade några rent nettoprissatta artiklar)

//Alla dessa filer innehåller samma kolumner:
//”Rsk”, ”Artikelbenämning”, ”Mängdpris”, ”Nettopris”, ”Bruttopris”, ”Förp enh”, ”Lundanr” samt ”Förp stl”.

//1.	”price_list_ea_lundagr.csv” 
//Stora prisfilen med drygt 20.000 artiklar. Kunderna kan få kundrabatt eller bonus på både Mängdpris och Nettopris, mellan 2 och 12% som då gäller på hela denna lista. 
//Vet att många av våra kunder använder Bruttopriset som en utgångspunkt för hur mycket dom ska debitera sin kund. Så en prislista som heter Lunda_Brutto också i ert system tror jag skulle uppskattas mycket.
//2.	”NTO_lundanto.csv” 
//Knappt 400 artiklar. Skillnaden är att på dessa kan kunderna inte få någon kundrabatt eller bonus. Så de kan ju vara med i samma prislista i ert system men behöver då märkas med någon info om att ev kundrabatt eller bonus inte utgår.
//Ska såklart inte heller kalkyleras med det om kunderna har möjlighet att lägga in det i ert system.
//Innehåller båda kolumnerna Mängdpris och Nettopris men det är ingen skillnad på dessa. 
//3.	”cu_ea_cug.csv”
//Dessa priser förändras beroende på kopparpriset, uppdateras vid behov varje vecka. Ev kundrabatt eller bonus utgår på dessa artiklar också.


namespace SoftOne.Soe.Business.Util.PricelistProvider
{
    public class Lunda : IPriceListProvider
    {
        #region Members

        readonly private List<LundaPost> posts;
        readonly SoeCompPriceListProvider LundaType;
        
        #endregion

        #region Constructors

        public Lunda(SoeCompPriceListProvider lundaType)
        {
            posts = new List<LundaPost>();
            LundaType = lundaType;
        }

        #endregion

        #region Public methods

        public ActionResult Read(Stream stream, string fileName)
        {
            if (!Lunda.ValidFileName(fileName) )
            {
                return new ActionResult(false, 0, "Ogiltigt filnamn för Lunda filerna, ska vara price_list_ea_lundagr.csv och NTO_lundanto.csv");
            }

            posts.Clear();

            stream.Position = 0;
            var sr = new StreamReader(stream, Constants.ENCODING_IBM437);

            //read first caption line....
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if ( string.IsNullOrEmpty(line)) continue;
                string[] items = line.Split(";".ToCharArray());
                try
                {
                    var post = new LundaPost(items);
                    if (post.PriceBrutto == 0 && post.PriceStyckNetto == 0)
                        continue; //skip empty rows

                    posts.Add(post);
                }
                catch (Exception ex) //hide error
                {
                    return new ActionResult(fileName+ "\n" + line + "\n" +  ex.Message );
                }
            }
            return new ActionResult(true);
        }

        public static bool ValidFileName(string fileName)
        {
            //price_list_ea_lundagr.csv = huvudprislista
            //NTO_lundanto.csv = nettoprislista
            var fileNames = new List<string> { "price_list_ea_lundagr.csv", "nto_lundanto.csv" };
            return fileNames.Contains( fileName.ToLower() );
        }

        public GenericProvider ToGeneric()
        {
            var wholeSeller = Enum.GetName(typeof(SoeCompPriceListProvider), LundaType);

            var result = new GenericProvider(wholeSeller);
            result.header = new GenericHeader(DateTime.Now); //not version controlled

            foreach (LundaPost product in posts)
            {
                var gp = new GenericProduct
                {
                    //Samma för alla
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Code = product.MaterialCode,
                    SalesUnit = product.StorageUnit,
                    PurchaseUnit = product.StorageUnit,
                    WholesellerName = wholeSeller, //for relation
                    PackageSize = product.PackageSize
                };

                //Olika beroende på typ
                switch (LundaType)
                {
                    case SoeCompPriceListProvider.LundaBrutto:
                        gp.Price = product.PriceBrutto;
                        break;
                    //case SoeCompPriceListProvider.LundaMängdNetto:
                    //    gp.Price = product.PriceQuantityNetto;
                    //    break;
                    case SoeCompPriceListProvider.LundaStyckNetto:
                        gp.Price = product.PriceStyckNetto;
                        break;
                    case SoeCompPriceListProvider.Lunda:
                        gp.Price = product.PriceBrutto;
                        gp.NetPrice = product.PriceStyckNetto;
                        break;
                    default:
                        throw new Exception($"Lunda price list import got unknown providertype {LundaType}");
                }

                result.products.Add(result.products.Count, gp);
            }
            return result;
        }

        #endregion

        public class LundaPost
        {
            #region Members

            public string ProductId { get; set; }
            public decimal PriceBrutto { get; set; }
            public decimal PriceStyckNetto { get; set; }
            public string MaterialCode { get; set; }
            public string StorageUnit { get; set; }
            public string Name { get; set; }
            public decimal PackageSize { get; set; }
            #endregion

            #region Constructors

            /// <summary>
            /// Used to mock in unit test
            /// </summary>
            public LundaPost()
            {
                //Empty constructor
            }

            public LundaPost(string[] items)
            {
                ProductId = items[0].Trim();
                Name = items[1].Trim();
                PriceStyckNetto = GetAmount(items[3].Trim());
                PriceBrutto = GetAmount(items[4].Trim());
                StorageUnit = items[5].Trim();
                MaterialCode = items[6].Trim();
                //package can be decimals...
                PackageSize = StringUtility.GetAmount( items[7].Trim() );
            }

            #endregion

            #region Help methods

            private static decimal GetAmount(string item)
            {
                if (string.IsNullOrEmpty(item))
                    return 0;

                item = item.Replace(",", "");
                item = item.Replace(".", "");
                return Convert.ToDecimal(item) / 100;
            }

            #endregion
        }
    }

}
