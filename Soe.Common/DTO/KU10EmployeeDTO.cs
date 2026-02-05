using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class KU10EmployeeDTO
    {

        public string Inkomstar { get; set; }
        public bool Borttag { get; set; }
        public string Inkomsttagare { get; set; }
        public decimal AvdragenSkatt { get; set; }
        public string AnstalldFrom { get; set; }
        public string AnstalldTom { get; set; }
        public decimal KontantBruttolonMm { get; set; }
        public decimal FormanUtomBilDrivmedel { get; set; }
        public decimal BilformanUtomDrivmedel { get; set; }
        public string KodForFormansbil { get; set; }
        public int AntalManBilforman { get; set; }
        public decimal KmBilersVidBilforman { get; set; }
        public decimal BetaltForBilforman { get; set; }
        public decimal DrivmedelVidBilforman { get; set; }
        public decimal AndraKostnadsers { get; set; }
        public decimal UnderlagRutarbete { get; set; }
        public decimal UnderlagRotarbete { get; set; }
        public decimal ErsMEgenavgifter { get; set; }
        public decimal Tjanstepension { get; set; }
        public decimal ErsEjSocAvg { get; set; }
        public decimal ErsEjSocAvgEjJobbavd { get; set; }
        public decimal Forskarskattenamnden { get; set; }
        public decimal VissaAvdrag { get; set; }
        public decimal Hyresersattning { get; set; }
        public bool? BostadSmahus { get; set; }
        public bool? Kost { get; set; }
        public bool? BostadEjSmahus { get; set; }
        public bool? Ranta { get; set; }
        public bool? Parkering { get; set; }
        public bool? AnnanForman { get; set; }
        public bool? FormanHarJusterats { get; set; }
        public bool? FormanSomPension { get; set; }
        public bool? Bilersattning { get; set; }
        public bool? TraktamenteInomRiket { get; set; }
        public bool? TraktamenteUtomRiket { get; set; }
        public bool? TjansteresaOver3MInrikes { get; set; }
        public bool? TjansteresaOver3MUtrikes { get; set; }
        public bool? Resekostnader { get; set; }
        public bool? Logi { get; set; }
        public decimal PersonaloptionForvarvAndel { get; set; }
        public string Arbetsstallenummer { get; set; }
        public bool? Delagare { get; set; }
        public string SpecAvAnnanForman { get; set; }
        public string SpecVissaAvdrag { get; set; }
        public string LandskodTIN { get; set; }
        public decimal SocialAvgiftsAvtal { get; set; }
        public string TIN { get; set; }
        public decimal Specifikationsnummer { get; set; }
        public List<KU10EmployeeTransactionDTO> Transactions { get; set; }
        public bool GiltigaUppgifter { get; set; }

    }

    public class KU10EmployeeTransactionDTO
    {
        public string Type { get; set; }
        public string PayrollProductNumber { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }

        public bool IsPayrollStartValue { get; set; }
        public string TimePeriodName { get; set; }
        public DateTime Date { get; set; }
    }

    public class AgdEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public int AndelStyrelsearvode { get; set; }
        public string ArbetsgivareIUGROUP { get; set; }
        public string BetalningsmottagareIUGROUP { get; set; }
        public string BetalningsmottagarId { get; set; }
        public string RedovisningsPeriod { get; set; }
        public bool ForstaAnstalld { get; set; }
        public bool AndraAnstalld { get; set; }
        public string Specifikationsnummer { get; set; }
        public bool Borttag { get; set; }
        public string Arbetsstallenummer { get; set; }
        public int AvdrPrelSkatt { get; set; }
        public int AvdrSkattSINK { get; set; }
        public int AvdrSkattASINK { get; set; }
        public decimal DecAvdrPrelSkatt { get; set; }
        public decimal DecAvdrSkattSINK { get; set; }
        public decimal DecAvdrSkattASINK { get; set; }
        public string SkattebefrEnlAvtal { get; set; }
        public string Lokalanstalld { get; set; }
        public string AmbassadanstISvMAvtal { get; set; }
        public string EjskatteavdragEjbeskattningSv { get; set; }
        public int AvrakningAvgiftsfriErs { get; set; }
        public int KontantErsattningUlagAG { get; set; }
        public int RegionaltStodUlagAG { get; set; }
        public int SkatteplOvrigaFormanerUlagAG { get; set; }
        public int SkatteplBilformanUlagAG { get; set; }
        public int DrivmVidBilformanUlagAG { get; set; }
        public int AvdragUtgiftArbetet { get; set; }
        public decimal DecAvrakningAvgiftsfriErs { get; set; }
        public decimal DecKontantErsattningUlagAG { get; set; }
        public decimal DecRegionaltStodUlagAG { get; set; }
        public decimal DecSkatteplOvrigaFormanerUlagAG { get; set; }
        public decimal DecSkatteplBilformanUlagAG { get; set; }
        public decimal DecDrivmVidBilformanUlagAG { get; set; }
        public decimal DecAvdragUtgiftArbetet { get; set; }
        public decimal DecAndelStyrelsearvode { get; set; }
        public bool BostadsformanSmahusUlagAG { get; set; }
        public bool BostadsformanEjSmahusUlagAG { get; set; }
        public bool Bilersattning { get; set; }
        public bool Traktamente { get; set; }
        public int AndraKostnadsers { get; set; }
        public int KontantErsattningEjUlagSA { get; set; }
        public int SkatteplOvrigaFormanerEjUlagSA { get; set; }
        public int SkatteplBilformanEjUlagSA { get; set; }
        public int DrivmVidBilformanEjUlagSA { get; set; }
        public int FormanSomPensionEjUlagSA { get; set; }
        public int BostadsformanSmahusEjUlagSA { get; set; }
        public int BostadsformanEjSmahusEjUlagSA { get; set; }
        public int ErsEjSocAvgEjJobbavd { get; set; }
        public int Tjanstepension { get; set; }
        public decimal DecAndraKostnadsers { get; set; }
        public decimal DecKontantErsattningEjUlagSA { get; set; }
        public decimal DecSkatteplOvrigaFormanerEjUlagSA { get; set; }
        public decimal DecSkatteplBilformanEjUlagSA { get; set; }
        public decimal DecDrivmVidBilformanEjUlagSA { get; set; }
        public decimal DecFormanSomPensionEjUlagSA { get; set; }
        public decimal DecBostadsformanSmahusEjUlagSA { get; set; }
        public decimal DecBostadsformanEjSmahusEjUlagSA { get; set; }
        public decimal DecErsEjSocAvgEjJobbavd { get; set; }
        public decimal DecTjanstepension { get; set; }
        public string ErsattningsKod1 { get; set; }
        public string ErsattningsKod2 { get; set; }
        public string ErsattningsKod3 { get; set; }
        public string ErsattningsKod4 { get; set; }
        public string ErsattningsBelopp1 { get; set; }
        public string ErsattningsBelopp2 { get; set; }
        public string ErsattningsBelopp3 { get; set; }
        public string ErsattningsBelopp4 { get; set; }

        public string Forskarskattenamnden { get; set; }
        public string VissaAvdrag { get; set; }
        public string ErsFormanBostadMmSINK { get; set; }
        public string LandskodArbetsland { get; set; }
        public string UtsandUnderTid { get; set; }
        public string KonventionMed { get; set; }
        public string KontantErsattningUlagEA { get; set; }
        public string SkatteplOvrigaFormanerUlagEA { get; set; }
        public string SkatteplBilformanUlagEA { get; set; }
        public string DrivmVidBilformanUlagEA { get; set; }
        public string BostadsformanSmahusUlagEA { get; set; }
        public string BostadsformanEjSmahusUlagEA { get; set; }
        public string Fartygssignal { get; set; }
        public string AntalDagarSjoinkomst { get; set; }
        public string NarfartFjarrfart { get; set; }
        public string FartygetsNamn { get; set; }
        public int UnderlagRutarbete { get; set; }
        public int UnderlagRotarbete { get; set; }
        public int Hyresersattning { get; set; }
        public decimal DecUnderlagRutarbete { get; set; }
        public decimal DecUnderlagRotarbete { get; set; }
        public decimal DecHyresersattning { get; set; }
        public string VerksamhetensArt { get; set; }
        public bool FormanHarJusterats { get; set; }
        public string Personaloption { get; set; }
        public int ArbetsArbAvgSlf { get; set; }
        public int Sjuklön { get; set; }
        public decimal SjuklönExport { get; set; }
        public int Skatt { get; set; }
        public decimal DecArbetsgivarintygKredit { get; set; }
        public decimal DecSjuklön { get; set; }
        public decimal DecSkatt { get; set; }
        public string PlaceOfEmploymentAddress { get; set; }
        public string PlaceOfEmploymentCity { get; set; }
        public string TimePeriodName { get; set; }
        public string Fodelseort { get; set; }
        public string LandskodFodelseort { get; set; }
        public string LandskodMedborgare { get; set; }
        public List<KU10EmployeeTransactionDTO> Transactions { get; set; }
        public decimal EmploymentTaxRate { get; set; }
        public decimal DecCalculatedArbetsgivarintygKredit { get; set; }
        public bool TemporaryParentalLeave { get; set; }
        public bool ParentalLeave { get; set; }
    }

}
