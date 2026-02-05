using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.DTO
{
    public class ForaColletiveAgrementDTO
    {
        public int id { get; set; }
        public string shortText { get; set; }
        public string LongText { get; set; }

        public List<ForaColletiveAgrementDTO> GetForColletiveAgrements()
        {
            List<ForaColletiveAgrementDTO> foraColletiveAgrements = new List<ForaColletiveAgrementDTO>();

            ForaColletiveAgrementDTO unkown = new ForaColletiveAgrementDTO()
            {
                id = 0,
                shortText = "",
                LongText = " "
            };

            foraColletiveAgrements.Add(unkown);
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 1, shortText = "M", LongText = "Målare" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 2, shortText = "I", LongText = "Installationsavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 3, shortText = "K", LongText = "Kraftverksavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 4, shortText = "E", LongText = "Electroscandiaavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 5, shortText = "C", LongText = "Teknikavtalet IF Metall" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 6, shortText = "D", LongText = "TEKO avtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 7, shortText = "L", LongText = "Livsmedelsavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 8, shortText = "F", LongText = "Tobaksavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 9, shortText = "V", LongText = "Avtalet för vin- och spritindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 10, shortText = "Y", LongText = "Kafferosterier och kryddfabriker" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 11, shortText = "B", LongText = "Byggnadsämnesindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 12, shortText = "U", LongText = "Buteljglasindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 13, shortText = "Z", LongText = "Motorbranschavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 14, shortText = "1", LongText = "Industriavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 15, shortText = "2", LongText = "Kemiska fabriker" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 16, shortText = "G", LongText = "Glasindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 17, shortText = "3", LongText = "Gemensamma Metall" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 18, shortText = "4", LongText = "Explosivämnesindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 19, shortText = "A", LongText = "I-avtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 27, shortText = "5", LongText = "Återvinningsföretag" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 28, shortText = "R", LongText = "Tvättindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 29, shortText = "8", LongText = "Oljeraffinaderier" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 30, shortText = "H", LongText = "Sockerindustrin (Nordic Sugar AB)" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 31, shortText = "6", LongText = "IMG-avtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 32, shortText = "J", LongText = "Sågverksavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 33, shortText = "7", LongText = "Skogsbruk" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 34, shortText = "W", LongText = "Virkesmätning" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 35, shortText = "P", LongText = "Stoppmöbelindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 36, shortText = "T", LongText = "Träindustri" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 37, shortText = "9", LongText = "Infomediaavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 38, shortText = "O", LongText = "Förpackningsavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 40, shortText = "+", LongText = "Handel- & Metallavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 41, shortText = "@", LongText = "Studsviksavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 42, shortText = "|", LongText = "Flygtekniker med typcertifikat (medarbetaravtal)" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 43, shortText = "]", LongText = "Massa- och pappersindustrin" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 44, shortText = "^", LongText = "Stål- och metallindustrin blå avtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 45, shortText = "$", LongText = "Tidningsavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 47, shortText = "?", LongText = "Bemanningsföretag" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 48, shortText = "Å", LongText = "Byggavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 49, shortText = "=", LongText = "Dalslands kanal" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 50, shortText = "Ä", LongText = "Detaljhandeln" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 51, shortText = "Ö", LongText = "Entreprenadmaskinavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 52, shortText = "[", LongText = "Glasmästeriavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 53, shortText = "< ", LongText = "Göta kanalbolag AB" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 54, shortText = "%", LongText = "Lageravtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 55, shortText = "0", LongText = "Lager- och E-handelsavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 56, shortText = "€", LongText = "Lagerpersonal vid glassföretag, filialer och depålager samt direktsäljare" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 57, shortText = "~", LongText = "Larm- och säkerhetsteknikavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 58, shortText = ":", LongText = "Maskinföraravtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 60, shortText = "*", LongText = "Plåt- och ventilationsavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 61, shortText = @"\", LongText = "Privatteateravtalet(medarbetaravtal)" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 62, shortText = "-", LongText = "Städavtalet" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 63, shortText = "£", LongText = "Teknikinstallation VVS och Kyl" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 64, shortText = "/", LongText = "Restaurang- och caféanställda" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 65, shortText = "{", LongText = "Skärgårdstrafik ASL" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 66, shortText = "> ", LongText = "Värdepapper" });
            foraColletiveAgrements.Add(new ForaColletiveAgrementDTO() { id = 67, shortText = "}", LongText = "Väg- och banavtalet" });

            return foraColletiveAgrements;
        }

        public ForaColletiveAgrementDTO GetForaColletiveAgrement(int Id)
        {
            var list = GetForColletiveAgrements();
            return list.Any(l => l.id == id) ? list.FirstOrDefault(l => l.id == id) : list.FirstOrDefault(l => l.id == 0);
        }
    }


}
