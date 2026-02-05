using System;
namespace SoftOne.Soe.Business.Util.ImportSpecials.Models
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class paxml
    {
        private headerTYPE headerField;
        private dimensionerTYPE dimensionerField;
        private resultatenheterTYPE resultatenheterField;
        private koderTYPE koderField;
        private resetransaktionerTYPE resetransaktionerField;
        private tidtransaktionerTYPE tidtransaktionerField;
        private lonetransaktionerTYPE lonetransaktionerField;
        private schematransaktionerTYPE schematransaktionerField;
        private personalTYPE personalField;
        private loneutbetalningTYPE loneutbetalningField;
        private saldonTYPE saldonField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public headerTYPE header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public dimensionerTYPE dimensioner
        {
            get
            {
                return this.dimensionerField;
            }
            set
            {
                this.dimensionerField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resultatenheterTYPE resultatenheter
        {
            get
            {
                return this.resultatenheterField;
            }
            set
            {
                this.resultatenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public koderTYPE koder
        {
            get
            {
                return this.koderField;
            }
            set
            {
                this.koderField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resetransaktionerTYPE resetransaktioner
        {
            get
            {
                return this.resetransaktionerField;
            }
            set
            {
                this.resetransaktionerField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public tidtransaktionerTYPE tidtransaktioner
        {
            get
            {
                return this.tidtransaktionerField;
            }
            set
            {
                this.tidtransaktionerField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonetransaktionerTYPE lonetransaktioner
        {
            get
            {
                return this.lonetransaktionerField;
            }
            set
            {
                this.lonetransaktionerField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public schematransaktionerTYPE schematransaktioner
        {
            get
            {
                return this.schematransaktionerField;
            }
            set
            {
                this.schematransaktionerField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public personalTYPE personal
        {
            get
            {
                return this.personalField;
            }
            set
            {
                this.personalField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public loneutbetalningTYPE loneutbetalning
        {
            get
            {
                return this.loneutbetalningField;
            }
            set
            {
                this.loneutbetalningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public saldonTYPE saldon
        {
            get
            {
                return this.saldonField;
            }
            set
            {
                this.saldonField = value;
            }
        }
    }
    public class headerTYPE
    {
        private string formatField;
        private stringversionTYPE versionField;
        private DateTime datumField;
        private bool datumFieldSpecified;
        private nyexportTYPE nyexportField;
        private string foretagidField;
        private string foretagorgnrField;
        private string foretagnamnField;
        private string extraadressField;
        private string postadressField;
        private string postnrField;
        private string ortField;
        private string landField;
        private string epostField;
        private string hemsidaField;
        private string kontaktpersonField;
        private string personalansvarigField;
        private string attestansvarigField;
        private string telefonField;
        private string telefaxField;
        private string programnamnField;
        private string programlicensField;
        private string infoField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public stringversionTYPE version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public nyexportTYPE nyexport
        {
            get
            {
                return this.nyexportField;
            }
            set
            {
                this.nyexportField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string foretagid
        {
            get
            {
                return this.foretagidField;
            }
            set
            {
                this.foretagidField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string foretagorgnr
        {
            get
            {
                return this.foretagorgnrField;
            }
            set
            {
                this.foretagorgnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string foretagnamn
        {
            get
            {
                return this.foretagnamnField;
            }
            set
            {
                this.foretagnamnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string extraadress
        {
            get
            {
                return this.extraadressField;
            }
            set
            {
                this.extraadressField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string postadress
        {
            get
            {
                return this.postadressField;
            }
            set
            {
                this.postadressField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string postnr
        {
            get
            {
                return this.postnrField;
            }
            set
            {
                this.postnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ort
        {
            get
            {
                return this.ortField;
            }
            set
            {
                this.ortField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string land
        {
            get
            {
                return this.landField;
            }
            set
            {
                this.landField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string epost
        {
            get
            {
                return this.epostField;
            }
            set
            {
                this.epostField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string hemsida
        {
            get
            {
                return this.hemsidaField;
            }
            set
            {
                this.hemsidaField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontaktperson
        {
            get
            {
                return this.kontaktpersonField;
            }
            set
            {
                this.kontaktpersonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string personalansvarig
        {
            get
            {
                return this.personalansvarigField;
            }
            set
            {
                this.personalansvarigField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string attestansvarig
        {
            get
            {
                return this.attestansvarigField;
            }
            set
            {
                this.attestansvarigField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string telefon
        {
            get
            {
                return this.telefonField;
            }
            set
            {
                this.telefonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string telefax
        {
            get
            {
                return this.telefaxField;
            }
            set
            {
                this.telefaxField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string programnamn
        {
            get
            {
                return this.programnamnField;
            }
            set
            {
                this.programnamnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string programlicens
        {
            get
            {
                return this.programlicensField;
            }
            set
            {
                this.programlicensField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
    }
    public enum stringversionTYPE
    {
        [System.Xml.Serialization.XmlEnumAttribute("2.0")]
        Item20,
    }
    public class nyexportTYPE
    {
        private DateTime datumField;
        private DateTime datumfromField;
        private bool datumfromFieldSpecified;
        private DateTime datumtomField;
        private bool datumtomFieldSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datumfrom
        {
            get
            {
                return this.datumfromField;
            }
            set
            {
                this.datumfromField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumfromSpecified
        {
            get
            {
                return this.datumfromFieldSpecified;
            }
            set
            {
                this.datumfromFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datumtom
        {
            get
            {
                return this.datumtomField;
            }
            set
            {
                this.datumtomField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumtomSpecified
        {
            get
            {
                return this.datumtomFieldSpecified;
            }
            set
            {
                this.datumtomFieldSpecified = value;
            }
        }
    }
    public class dimensionerTYPE
    {
        private dimensionTYPE[] dimensionField;
        [System.Xml.Serialization.XmlElementAttribute("dimension", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public dimensionTYPE[] dimension
        {
            get
            {
                return this.dimensionField;
            }
            set
            {
                this.dimensionField = value;
            }
        }
    }
    public class dimensionTYPE
    {
        private int dimField;
        private string namnField;
        private string infoField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int dim
        {
            get
            {
                return this.dimField;
            }
            set
            {
                this.dimField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string namn
        {
            get
            {
                return this.namnField;
            }
            set
            {
                this.namnField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
    }
    public class resultatenheterTYPE
    {
        private resultatenhetTYPE[] resultatenhetField;
        [System.Xml.Serialization.XmlElementAttribute("resultatenhet", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resultatenhetTYPE[] resultatenhet
        {
            get
            {
                return this.resultatenhetField;
            }
            set
            {
                this.resultatenhetField = value;
            }
        }
    }
    public class resultatenhetTYPE
    {
        private bool deleteField;
        private bool deleteFieldSpecified;
        private int dimField;
        private string idField;
        private string namnField;
        private string infoField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool delete
        {
            get
            {
                return this.deleteField;
            }
            set
            {
                this.deleteField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deleteSpecified
        {
            get
            {
                return this.deleteFieldSpecified;
            }
            set
            {
                this.deleteFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int dim
        {
            get
            {
                return this.dimField;
            }
            set
            {
                this.dimField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string namn
        {
            get
            {
                return this.namnField;
            }
            set
            {
                this.namnField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
    }
    public class koderTYPE
    {
        private kodTYPE[] kodField;
        [System.Xml.Serialization.XmlElementAttribute("kod", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public kodTYPE[] kod
        {
            get
            {
                return this.kodField;
            }
            set
            {
                this.kodField = value;
            }
        }
    }
    public class kodTYPE
    {
        private string idField;
        private string namnField;
        private string infoField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string namn
        {
            get
            {
                return this.namnField;
            }
            set
            {
                this.namnField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
    }
    public class resetransaktionerTYPE
    {
        private resetransTYPE[] resetransField;
        private string landskodstdField;
        [System.Xml.Serialization.XmlElementAttribute("resetrans", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resetransTYPE[] resetrans
        {
            get
            {
                return this.resetransField;
            }
            set
            {
                this.resetransField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string landskodstd
        {
            get
            {
                return this.landskodstdField;
            }
            set
            {
                this.landskodstdField = value;
            }
        }
    }
    public class resetransTYPE
    {
        private DateTime tidpunktField;
        private reskodTYPE resekodField;
        private fortsattTYPE fortsattField;
        private string landskodField;
        private string valutakodField;
        private decimal valutafaktorField;
        private bool valutafaktorFieldSpecified;
        private decimal beloppField;
        private bool beloppFieldSpecified;
        private decimal momsField;
        private bool momsFieldSpecified;
        private bool ftgkortField;
        private bool ftgkortFieldSpecified;
        private int antdeltagField;
        private bool antdeltagFieldSpecified;
        private deltagarlistaTYPE deltagarlistaField;
        private string varugruppField;
        private string specifikationField;
        private string kontonrField;
        private string bilnrField;
        private string bilmodellField;
        private string foretagField;
        private string kontaktField;
        private string syfteField;
        private string ortField;
        private int kmstartField;
        private bool kmstartFieldSpecified;
        private int kmstoppField;
        private bool kmstoppFieldSpecified;
        private int kilometerField;
        private bool kilometerFieldSpecified;
        private int antpassField;
        private bool antpassFieldSpecified;
        private int antlastField;
        private bool antlastFieldSpecified;
        private decimal timmarField;
        private bool timmarFieldSpecified;
        private string samlingsidField;
        private kundnrTYPE kundnrField;
        private resenheterTYPE resenheterField;
        private string anteckningField;
        private string infoField;
        private int postidField;
        private bool postidFieldSpecified;
        private string anstidField;
        private string persnrField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime tidpunkt
        {
            get
            {
                return this.tidpunktField;
            }
            set
            {
                this.tidpunktField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public reskodTYPE resekod
        {
            get
            {
                return this.resekodField;
            }
            set
            {
                this.resekodField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public fortsattTYPE fortsatt
        {
            get
            {
                return this.fortsattField;
            }
            set
            {
                this.fortsattField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string landskod
        {
            get
            {
                return this.landskodField;
            }
            set
            {
                this.landskodField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string valutakod
        {
            get
            {
                return this.valutakodField;
            }
            set
            {
                this.valutakodField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal valutafaktor
        {
            get
            {
                return this.valutafaktorField;
            }
            set
            {
                this.valutafaktorField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valutafaktorSpecified
        {
            get
            {
                return this.valutafaktorFieldSpecified;
            }
            set
            {
                this.valutafaktorFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool beloppSpecified
        {
            get
            {
                return this.beloppFieldSpecified;
            }
            set
            {
                this.beloppFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal moms
        {
            get
            {
                return this.momsField;
            }
            set
            {
                this.momsField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool momsSpecified
        {
            get
            {
                return this.momsFieldSpecified;
            }
            set
            {
                this.momsFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ftgkort
        {
            get
            {
                return this.ftgkortField;
            }
            set
            {
                this.ftgkortField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ftgkortSpecified
        {
            get
            {
                return this.ftgkortFieldSpecified;
            }
            set
            {
                this.ftgkortFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int antdeltag
        {
            get
            {
                return this.antdeltagField;
            }
            set
            {
                this.antdeltagField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool antdeltagSpecified
        {
            get
            {
                return this.antdeltagFieldSpecified;
            }
            set
            {
                this.antdeltagFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public deltagarlistaTYPE deltagarlista
        {
            get
            {
                return this.deltagarlistaField;
            }
            set
            {
                this.deltagarlistaField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string varugrupp
        {
            get
            {
                return this.varugruppField;
            }
            set
            {
                this.varugruppField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string specifikation
        {
            get
            {
                return this.specifikationField;
            }
            set
            {
                this.specifikationField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontonr
        {
            get
            {
                return this.kontonrField;
            }
            set
            {
                this.kontonrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bilnr
        {
            get
            {
                return this.bilnrField;
            }
            set
            {
                this.bilnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bilmodell
        {
            get
            {
                return this.bilmodellField;
            }
            set
            {
                this.bilmodellField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string foretag
        {
            get
            {
                return this.foretagField;
            }
            set
            {
                this.foretagField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontakt
        {
            get
            {
                return this.kontaktField;
            }
            set
            {
                this.kontaktField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string syfte
        {
            get
            {
                return this.syfteField;
            }
            set
            {
                this.syfteField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ort
        {
            get
            {
                return this.ortField;
            }
            set
            {
                this.ortField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int kmstart
        {
            get
            {
                return this.kmstartField;
            }
            set
            {
                this.kmstartField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kmstartSpecified
        {
            get
            {
                return this.kmstartFieldSpecified;
            }
            set
            {
                this.kmstartFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int kmstopp
        {
            get
            {
                return this.kmstoppField;
            }
            set
            {
                this.kmstoppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kmstoppSpecified
        {
            get
            {
                return this.kmstoppFieldSpecified;
            }
            set
            {
                this.kmstoppFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int kilometer
        {
            get
            {
                return this.kilometerField;
            }
            set
            {
                this.kilometerField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kilometerSpecified
        {
            get
            {
                return this.kilometerFieldSpecified;
            }
            set
            {
                this.kilometerFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int antpass
        {
            get
            {
                return this.antpassField;
            }
            set
            {
                this.antpassField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool antpassSpecified
        {
            get
            {
                return this.antpassFieldSpecified;
            }
            set
            {
                this.antpassFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int antlast
        {
            get
            {
                return this.antlastField;
            }
            set
            {
                this.antlastField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool antlastSpecified
        {
            get
            {
                return this.antlastFieldSpecified;
            }
            set
            {
                this.antlastFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal timmar
        {
            get
            {
                return this.timmarField;
            }
            set
            {
                this.timmarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timmarSpecified
        {
            get
            {
                return this.timmarFieldSpecified;
            }
            set
            {
                this.timmarFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string samlingsid
        {
            get
            {
                return this.samlingsidField;
            }
            set
            {
                this.samlingsidField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public kundnrTYPE kundnr
        {
            get
            {
                return this.kundnrField;
            }
            set
            {
                this.kundnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenheterTYPE resenheter
        {
            get
            {
                return this.resenheterField;
            }
            set
            {
                this.resenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string anteckning
        {
            get
            {
                return this.anteckningField;
            }
            set
            {
                this.anteckningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int postid
        {
            get
            {
                return this.postidField;
            }
            set
            {
                this.postidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool postidSpecified
        {
            get
            {
                return this.postidFieldSpecified;
            }
            set
            {
                this.postidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
    }
    public enum reskodTYPE
    {
        START,
        STOPP,
        FLYG,
        LAND,
        FRU_NEJ,
        FRU_HOT,
        FRU_BET,
        FRU_ARB,
        FRU_REP,
        LUN_NEJ,
        LUN_BET,
        LUN_ARB,
        LUN_REP,
        MID_NEJ,
        MID_BET,
        MID_ARB,
        MID_REP,
        LOGI_BET,
        LOGI_ARB,
        MIL_PRI,
        MIL_FTG,
        MIL_TJT,
        MIL_DIS,
        UTLÄGG,
        REPR_ENK,
        REPR_LCH,
        REPR_MID,
        REPR_INT,
        RESTID,
    }
    public class fortsattTYPE
    {
        private int dagnrField;
        private bool valueField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int dagnr
        {
            get
            {
                return this.dagnrField;
            }
            set
            {
                this.dagnrField = value;
            }
        }
        [System.Xml.Serialization.XmlTextAttribute()]
        public bool Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    public class deltagarlistaTYPE
    {
        private deltagareTYPE[] deltagareField;
        [System.Xml.Serialization.XmlElementAttribute("deltagare", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public deltagareTYPE[] deltagare
        {
            get
            {
                return this.deltagareField;
            }
            set
            {
                this.deltagareField = value;
            }
        }
    }
    public class deltagareTYPE
    {
        private string foretagField;
        private string namnField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string foretag
        {
            get
            {
                return this.foretagField;
            }
            set
            {
                this.foretagField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string namn
        {
            get
            {
                return this.namnField;
            }
            set
            {
                this.namnField = value;
            }
        }
    }
    public class kundnrTYPE
    {
        private string infoField;
        private string valueField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    public class resenheterTYPE
    {
        private resenhetTYPE[] resenhetField;
        [System.Xml.Serialization.XmlElementAttribute("resenhet", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenhetTYPE[] resenhet
        {
            get
            {
                return this.resenhetField;
            }
            set
            {
                this.resenhetField = value;
            }
        }
    }
    public class resenhetTYPE
    {
        private int dimField;
        private string idField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int dim
        {
            get
            {
                return this.dimField;
            }
            set
            {
                this.dimField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }
    public class tidtransaktionerTYPE
    {
        private handelseTYPE[] klarField;
        private handelseTYPE[] attesteratField;
        private tidtransTYPE[] tidtransField;
        [System.Xml.Serialization.XmlElementAttribute("klar", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public handelseTYPE[] klar
        {
            get
            {
                return this.klarField;
            }
            set
            {
                this.klarField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute("attesterat", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public handelseTYPE[] attesterat
        {
            get
            {
                return this.attesteratField;
            }
            set
            {
                this.attesteratField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute("tidtrans", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public tidtransTYPE[] tidtrans
        {
            get
            {
                return this.tidtransField;
            }
            set
            {
                this.tidtransField = value;
            }
        }
    }
    public class handelseTYPE
    {
        private int postidField;
        private bool postidFieldSpecified;
        private string anstidField;
        private string persnrField;
        private DateTime datumField;
        private bool datumFieldSpecified;
        private DateTime datumfromField;
        private bool datumfromFieldSpecified;
        private DateTime datumtomField;
        private bool datumtomFieldSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int postid
        {
            get
            {
                return this.postidField;
            }
            set
            {
                this.postidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool postidSpecified
        {
            get
            {
                return this.postidFieldSpecified;
            }
            set
            {
                this.postidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datumfrom
        {
            get
            {
                return this.datumfromField;
            }
            set
            {
                this.datumfromField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumfromSpecified
        {
            get
            {
                return this.datumfromFieldSpecified;
            }
            set
            {
                this.datumfromFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datumtom
        {
            get
            {
                return this.datumtomField;
            }
            set
            {
                this.datumtomField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumtomSpecified
        {
            get
            {
                return this.datumtomFieldSpecified;
            }
            set
            {
                this.datumtomFieldSpecified = value;
            }
        }
    }
    public class tidtransTYPE
    {
        private tidkodTYPE tidkodField;
        private DateTime datumField;
        private bool datumFieldSpecified;
        private DateTime datumfromField;
        private bool datumfromFieldSpecified;
        private DateTime datumtomField;
        private bool datumtomFieldSpecified;
        private DateTime starttidField;
        private bool starttidFieldSpecified;
        private DateTime sluttidField;
        private bool sluttidFieldSpecified;
        private decimal timmarField;
        private bool timmarFieldSpecified;
        private decimal omfattningField;
        private bool omfattningFieldSpecified;
        private string barnField;
        private string samlingsidField;
        private bool semgrundField;
        private bool semgrundFieldSpecified;
        private string kontonrField;
        private kundnrTYPE kundnrField;
        private resenheterTYPE resenheterField;
        private string infoField;
        private int postidField;
        private bool postidFieldSpecified;
        private string anstidField;
        private string persnrField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public tidkodTYPE tidkod
        {
            get
            {
                return this.tidkodField;
            }
            set
            {
                this.tidkodField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime datumfrom
        {
            get
            {
                return this.datumfromField;
            }
            set
            {
                this.datumfromField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumfromSpecified
        {
            get
            {
                return this.datumfromFieldSpecified;
            }
            set
            {
                this.datumfromFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime datumtom
        {
            get
            {
                return this.datumtomField;
            }
            set
            {
                this.datumtomField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumtomSpecified
        {
            get
            {
                return this.datumtomFieldSpecified;
            }
            set
            {
                this.datumtomFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime starttid
        {
            get
            {
                return this.starttidField;
            }
            set
            {
                this.starttidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool starttidSpecified
        {
            get
            {
                return this.starttidFieldSpecified;
            }
            set
            {
                this.starttidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime sluttid
        {
            get
            {
                return this.sluttidField;
            }
            set
            {
                this.sluttidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sluttidSpecified
        {
            get
            {
                return this.sluttidFieldSpecified;
            }
            set
            {
                this.sluttidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal timmar
        {
            get
            {
                return this.timmarField;
            }
            set
            {
                this.timmarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timmarSpecified
        {
            get
            {
                return this.timmarFieldSpecified;
            }
            set
            {
                this.timmarFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal omfattning
        {
            get
            {
                return this.omfattningField;
            }
            set
            {
                this.omfattningField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool omfattningSpecified
        {
            get
            {
                return this.omfattningFieldSpecified;
            }
            set
            {
                this.omfattningFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string barn
        {
            get
            {
                return this.barnField;
            }
            set
            {
                this.barnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string samlingsid
        {
            get
            {
                return this.samlingsidField;
            }
            set
            {
                this.samlingsidField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool semgrund
        {
            get
            {
                return this.semgrundField;
            }
            set
            {
                this.semgrundField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semgrundSpecified
        {
            get
            {
                return this.semgrundFieldSpecified;
            }
            set
            {
                this.semgrundFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontonr
        {
            get
            {
                return this.kontonrField;
            }
            set
            {
                this.kontonrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public kundnrTYPE kundnr
        {
            get
            {
                return this.kundnrField;
            }
            set
            {
                this.kundnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenheterTYPE resenheter
        {
            get
            {
                return this.resenheterField;
            }
            set
            {
                this.resenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int postid
        {
            get
            {
                return this.postidField;
            }
            set
            {
                this.postidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool postidSpecified
        {
            get
            {
                return this.postidFieldSpecified;
            }
            set
            {
                this.postidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
    }
    public enum tidkodTYPE
    {
        SJK,
        SJK_KAR,
        SJK_LÖN,
        SJK_ERS,
        SJK_PEN,
        ASK,
        HAV,
        FPE,
        VAB,
        SMB,
        UTB,
        MIL,
        SVE,
        NÄR,
        TJL,
        SEM,
        SEM_BET,
        SEM_SPA,
        SEM_OBE,
        SEM_FÖR,
        KOM,
        PEM,
        PER,
        FAC,
        ATK,
        KON,
        PAP,
        ATF,
        FR1,
        FR2,
        FR3,
        FR4,
        FR5,
        FR6,
        FR7,
        FR8,
        FR9,
        FLX,
        SCH,
        TS1,
        TS2,
        TS3,
        TS4,
        TS5,
        TS6,
        TS7,
        TS8,
        TS9,
        TID,
        ARB,
        MER,
        ÖT1,
        ÖT2,
        ÖT3,
        ÖT4,
        ÖT5,
        ÖK1,
        ÖK2,
        ÖK3,
        ÖK4,
        ÖK5,
        OB1,
        OB2,
        OB3,
        OB4,
        OB5,
        OS1,
        OS2,
        OS3,
        OS4,
        OS5,
        JR1,
        JR2,
        JR3,
        JS1,
        JS2,
        JS3,
        BE1,
        BE2,
        BE3,
        BS1,
        BS2,
        BS3,
        RE1,
        RE2,
        RE3,
        HLG,
        SKI,
        LT1,
        LT2,
        LT3,
        LT4,
        LT5,
        LT6,
        LT7,
        LT8,
        LT9,
        NV1,
        NV2,
        NV3,
        NV4,
        NV5,
        NV6,
        NV7,
        NV8,
        NV9,
    }
    public class lonetransaktionerTYPE
    {
        private lonetransTYPE[] lonetransField;
        [System.Xml.Serialization.XmlElementAttribute("lonetrans", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonetransTYPE[] lonetrans
        {
            get
            {
                return this.lonetransField;
            }
            set
            {
                this.lonetransField = value;
            }
        }
    }
    public class lonetransTYPE
    {
        private lonekodTYPE lonkodField;
        private bool lonkodFieldSpecified;
        private string lonartField;
        private string benamningField;
        private string kommentarField;
        private DateTime datumField;
        private bool datumFieldSpecified;
        private DateTime datumfromField;
        private bool datumfromFieldSpecified;
        private DateTime datumtomField;
        private bool datumtomFieldSpecified;
        private decimal antalField;
        private bool antalFieldSpecified;
        private decimal aprisField;
        private bool aprisFieldSpecified;
        private decimal beloppField;
        private bool beloppFieldSpecified;
        private string varugruppField;
        private decimal momsField;
        private bool momsFieldSpecified;
        private string samlingsidField;
        private string kontonrField;
        private kundnrTYPE kundnrField;
        private resenheterTYPE resenheterField;
        private string infoField;
        private int postidField;
        private bool postidFieldSpecified;
        private string anstidField;
        private string persnrField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonekodTYPE lonkod
        {
            get
            {
                return this.lonkodField;
            }
            set
            {
                this.lonkodField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lonkodSpecified
        {
            get
            {
                return this.lonkodFieldSpecified;
            }
            set
            {
                this.lonkodFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string lonart
        {
            get
            {
                return this.lonartField;
            }
            set
            {
                this.lonartField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string benamning
        {
            get
            {
                return this.benamningField;
            }
            set
            {
                this.benamningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kommentar
        {
            get
            {
                return this.kommentarField;
            }
            set
            {
                this.kommentarField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime datumfrom
        {
            get
            {
                return this.datumfromField;
            }
            set
            {
                this.datumfromField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumfromSpecified
        {
            get
            {
                return this.datumfromFieldSpecified;
            }
            set
            {
                this.datumfromFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime datumtom
        {
            get
            {
                return this.datumtomField;
            }
            set
            {
                this.datumtomField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumtomSpecified
        {
            get
            {
                return this.datumtomFieldSpecified;
            }
            set
            {
                this.datumtomFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal antal
        {
            get
            {
                return this.antalField;
            }
            set
            {
                this.antalField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool antalSpecified
        {
            get
            {
                return this.antalFieldSpecified;
            }
            set
            {
                this.antalFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal apris
        {
            get
            {
                return this.aprisField;
            }
            set
            {
                this.aprisField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool aprisSpecified
        {
            get
            {
                return this.aprisFieldSpecified;
            }
            set
            {
                this.aprisFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool beloppSpecified
        {
            get
            {
                return this.beloppFieldSpecified;
            }
            set
            {
                this.beloppFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string varugrupp
        {
            get
            {
                return this.varugruppField;
            }
            set
            {
                this.varugruppField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal moms
        {
            get
            {
                return this.momsField;
            }
            set
            {
                this.momsField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool momsSpecified
        {
            get
            {
                return this.momsFieldSpecified;
            }
            set
            {
                this.momsFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string samlingsid
        {
            get
            {
                return this.samlingsidField;
            }
            set
            {
                this.samlingsidField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontonr
        {
            get
            {
                return this.kontonrField;
            }
            set
            {
                this.kontonrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public kundnrTYPE kundnr
        {
            get
            {
                return this.kundnrField;
            }
            set
            {
                this.kundnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenheterTYPE resenheter
        {
            get
            {
                return this.resenheterField;
            }
            set
            {
                this.resenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int postid
        {
            get
            {
                return this.postidField;
            }
            set
            {
                this.postidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool postidSpecified
        {
            get
            {
                return this.postidFieldSpecified;
            }
            set
            {
                this.postidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
    }
    public enum lonekodTYPE
    {
        TID,
        ARB,
        MER,
        ÖT1,
        ÖT2,
        ÖT3,
        ÖT4,
        ÖT5,
        ÖK1,
        ÖK2,
        ÖK3,
        ÖK4,
        ÖK5,
        JR1,
        JR2,
        JR3,
        BE1,
        BE2,
        BE3,
        RE1,
        RE2,
        RE3,
        NV1,
        NV2,
        NV3,
        NV4,
        NV5,
        NV6,
        NV7,
        NV8,
        NV9,
        OB1,
        OB2,
        OB3,
        OB4,
        OB5,
        OS1,
        OS2,
        OS3,
        OS4,
        OS5,
        HLG,
        SKI,
        LT1,
        LT2,
        LT3,
        LT4,
        LT5,
        LT6,
        LT7,
        LT8,
        LT9,
        MÅNLÖN,
        TIMLÖN,
        BONUS,
        PROVISION,
        FÖRSKOTT,
        UTLÄGG,
        RESERS,
        INR_FRI,
        INR_RED,
        INR_SKT,
        INRHEL_FRI,
        INRHEL_RED,
        INRHEL_SKT,
        INRHLV_FRI,
        INRHLV_RED,
        INRHLV_SKT,
        INRDAG_SKT,
        INRNAT_FRI,
        INRNAT_SKT,
        UTR_FRI,
        UTR_RED,
        UTR_SKT,
        UTRHEL_FRI,
        UTRHEL_RED,
        UTRHEL_SKT,
        UTRHLV_FRI,
        UTRHLV_RED,
        UTRHLV_SKT,
        UTRDAG_SKT,
        UTRNAT_FRI,
        UTRNAT_SKT,
        MIL_FRI,
        MIL_SKT,
        MILPRI_FRI,
        MILPRI_SKT,
        MILFTG_FRI,
        MILFTG_SKT,
        MILDIS_FRI,
        MILDIS_SKT,
        MATFRM,
        MATFRM_FRU,
        MATFRM_LCH,
        MATFRM_MID,
        UTRFRM,
        UTRFRM_FRU,
        UTRFRM_LCH,
        UTRFRM_MID,
        MATRED,
        MATRED_FRU,
        MATRED_LCH,
        MATRED_MID,
        UTRRED,
        UTRRED_FRU,
        UTRRED_LCH,
        UTRRED_MID,
        REPEXT,
        REPEXT_FRI,
        REPEXT_SKT,
        REPINT,
        REPINT_FRI,
        REPINT_SKT,
    }
    public class schematransaktionerTYPE
    {
        private schemaTYPE[] schemaField;
        [System.Xml.Serialization.XmlElementAttribute("schema", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public schemaTYPE[] schema
        {
            get
            {
                return this.schemaField;
            }
            set
            {
                this.schemaField = value;
            }
        }
    }
    public class schemaTYPE
    {
        private dagTYPE[] dagField;
        private string anstidField;
        private string persnrField;
        [System.Xml.Serialization.XmlElementAttribute("dag", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public dagTYPE[] dag
        {
            get
            {
                return this.dagField;
            }
            set
            {
                this.dagField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
    }
    public class dagTYPE
    {
        private DateTime datumField;
        private DateTime starttidField;
        private bool starttidFieldSpecified;
        private DateTime sluttidField;
        private bool sluttidFieldSpecified;
        private decimal timmarField;
        private bool timmarFieldSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime starttid
        {
            get
            {
                return this.starttidField;
            }
            set
            {
                this.starttidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool starttidSpecified
        {
            get
            {
                return this.starttidFieldSpecified;
            }
            set
            {
                this.starttidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
        public DateTime sluttid
        {
            get
            {
                return this.sluttidField;
            }
            set
            {
                this.sluttidField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sluttidSpecified
        {
            get
            {
                return this.sluttidFieldSpecified;
            }
            set
            {
                this.sluttidFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal timmar
        {
            get
            {
                return this.timmarField;
            }
            set
            {
                this.timmarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timmarSpecified
        {
            get
            {
                return this.timmarFieldSpecified;
            }
            set
            {
                this.timmarFieldSpecified = value;
            }
        }
    }
    public class personalTYPE
    {
        private personTYPE[] personField;
        [System.Xml.Serialization.XmlElementAttribute("person", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public personTYPE[] person
        {
            get
            {
                return this.personField;
            }
            set
            {
                this.personField = value;
            }
        }
    }
    public class personTYPE
    {
        private string fornamnField;
        private string efternamnField;
        private string extraadressField;
        private string postadressField;
        private string postnrField;
        private string ortField;
        private string landField;
        private string mobiltelefonField;
        private string hemtelefonField;
        private string arbetstelefonField;
        private string epostarbField;
        private string eposthemField;
        private personaltypTYPE personaltypField;
        private bool personaltypFieldSpecified;
        private string kategoriField;
        private string befattningField;
        private string befattningskodField;
        private string anstformField;
        private string semesteravtalField;
        private string bankclearingField;
        private string bankkontoField;
        private DateTime anstdatumField;
        private bool anstdatumFieldSpecified;
        private DateTime avgdatumField;
        private bool avgdatumFieldSpecified;
        private lonformTYPE lonformField;
        private bool lonformFieldSpecified;
        private bool innevarandeField;
        private bool innevarandeFieldSpecified;
        private intAttribDatumBelopp timlonField;
        private intAttribDatumBelopp manlonField;
        private fribelopplistaTYPE personbeloppField;
        private fritextlistaTYPE persontexterField;
        private intAttribDatumBelopp sysgradField;
        private decimal semesterdagarField;
        private bool semesterdagarFieldSpecified;
        private decimal skattetabellField;
        private bool skattetabellFieldSpecified;
        private int skattekolumnField;
        private bool skattekolumnFieldSpecified;
        private jamkningTYPE skattejamkningField;
        private utmatningTYPE loneutmatningField;
        private resenheterTYPE resenheterField;
        private string infoField;
        private string anstidField;
        private string persnrField;
        private bool deleteField;
        private bool deleteFieldSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string fornamn
        {
            get
            {
                return this.fornamnField;
            }
            set
            {
                this.fornamnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string efternamn
        {
            get
            {
                return this.efternamnField;
            }
            set
            {
                this.efternamnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string extraadress
        {
            get
            {
                return this.extraadressField;
            }
            set
            {
                this.extraadressField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string postadress
        {
            get
            {
                return this.postadressField;
            }
            set
            {
                this.postadressField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string postnr
        {
            get
            {
                return this.postnrField;
            }
            set
            {
                this.postnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ort
        {
            get
            {
                return this.ortField;
            }
            set
            {
                this.ortField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string land
        {
            get
            {
                return this.landField;
            }
            set
            {
                this.landField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string mobiltelefon
        {
            get
            {
                return this.mobiltelefonField;
            }
            set
            {
                this.mobiltelefonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string hemtelefon
        {
            get
            {
                return this.hemtelefonField;
            }
            set
            {
                this.hemtelefonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arbetstelefon
        {
            get
            {
                return this.arbetstelefonField;
            }
            set
            {
                this.arbetstelefonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string epostarb
        {
            get
            {
                return this.epostarbField;
            }
            set
            {
                this.epostarbField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string eposthem
        {
            get
            {
                return this.eposthemField;
            }
            set
            {
                this.eposthemField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public personaltypTYPE personaltyp
        {
            get
            {
                return this.personaltypField;
            }
            set
            {
                this.personaltypField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool personaltypSpecified
        {
            get
            {
                return this.personaltypFieldSpecified;
            }
            set
            {
                this.personaltypFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kategori
        {
            get
            {
                return this.kategoriField;
            }
            set
            {
                this.kategoriField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string befattning
        {
            get
            {
                return this.befattningField;
            }
            set
            {
                this.befattningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string befattningskod
        {
            get
            {
                return this.befattningskodField;
            }
            set
            {
                this.befattningskodField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string anstform
        {
            get
            {
                return this.anstformField;
            }
            set
            {
                this.anstformField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string semesteravtal
        {
            get
            {
                return this.semesteravtalField;
            }
            set
            {
                this.semesteravtalField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bankclearing
        {
            get
            {
                return this.bankclearingField;
            }
            set
            {
                this.bankclearingField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bankkonto
        {
            get
            {
                return this.bankkontoField;
            }
            set
            {
                this.bankkontoField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime anstdatum
        {
            get
            {
                return this.anstdatumField;
            }
            set
            {
                this.anstdatumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool anstdatumSpecified
        {
            get
            {
                return this.anstdatumFieldSpecified;
            }
            set
            {
                this.anstdatumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime avgdatum
        {
            get
            {
                return this.avgdatumField;
            }
            set
            {
                this.avgdatumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool avgdatumSpecified
        {
            get
            {
                return this.avgdatumFieldSpecified;
            }
            set
            {
                this.avgdatumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonformTYPE lonform
        {
            get
            {
                return this.lonformField;
            }
            set
            {
                this.lonformField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lonformSpecified
        {
            get
            {
                return this.lonformFieldSpecified;
            }
            set
            {
                this.lonformFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool innevarande
        {
            get
            {
                return this.innevarandeField;
            }
            set
            {
                this.innevarandeField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool innevarandeSpecified
        {
            get
            {
                return this.innevarandeFieldSpecified;
            }
            set
            {
                this.innevarandeFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public intAttribDatumBelopp timlon
        {
            get
            {
                return this.timlonField;
            }
            set
            {
                this.timlonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public intAttribDatumBelopp manlon
        {
            get
            {
                return this.manlonField;
            }
            set
            {
                this.manlonField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public fribelopplistaTYPE personbelopp
        {
            get
            {
                return this.personbeloppField;
            }
            set
            {
                this.personbeloppField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public fritextlistaTYPE persontexter
        {
            get
            {
                return this.persontexterField;
            }
            set
            {
                this.persontexterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public intAttribDatumBelopp sysgrad
        {
            get
            {
                return this.sysgradField;
            }
            set
            {
                this.sysgradField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semesterdagar
        {
            get
            {
                return this.semesterdagarField;
            }
            set
            {
                this.semesterdagarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semesterdagarSpecified
        {
            get
            {
                return this.semesterdagarFieldSpecified;
            }
            set
            {
                this.semesterdagarFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal skattetabell
        {
            get
            {
                return this.skattetabellField;
            }
            set
            {
                this.skattetabellField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattetabellSpecified
        {
            get
            {
                return this.skattetabellFieldSpecified;
            }
            set
            {
                this.skattetabellFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int skattekolumn
        {
            get
            {
                return this.skattekolumnField;
            }
            set
            {
                this.skattekolumnField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattekolumnSpecified
        {
            get
            {
                return this.skattekolumnFieldSpecified;
            }
            set
            {
                this.skattekolumnFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public jamkningTYPE skattejamkning
        {
            get
            {
                return this.skattejamkningField;
            }
            set
            {
                this.skattejamkningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public utmatningTYPE loneutmatning
        {
            get
            {
                return this.loneutmatningField;
            }
            set
            {
                this.loneutmatningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenheterTYPE resenheter
        {
            get
            {
                return this.resenheterField;
            }
            set
            {
                this.resenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool delete
        {
            get
            {
                return this.deleteField;
            }
            set
            {
                this.deleteField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool deleteSpecified
        {
            get
            {
                return this.deleteFieldSpecified;
            }
            set
            {
                this.deleteFieldSpecified = value;
            }
        }
    }
    public enum personaltypTYPE
    {
        ARB,
        TJM,
    }
    public enum lonformTYPE
    {
        TIM,
        MÅN,
    }
    public class intAttribDatumBelopp
    {
        private DateTime datumField;
        private bool datumFieldSpecified;
        private decimal valueField;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    public class fribelopplistaTYPE
    {
        private fribeloppTYPE[] beloppField;
        [System.Xml.Serialization.XmlElementAttribute("belopp", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public fribeloppTYPE[] belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
    }
    public class fribeloppTYPE
    {
        private string idField;
        private DateTime datumField;
        private bool datumFieldSpecified;
        private decimal valueField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    public class fritextlistaTYPE
    {
        private fritextTYPE[] textField;
        [System.Xml.Serialization.XmlElementAttribute("text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public fritextTYPE[] text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }
    public class fritextTYPE
    {
        private string idField;
        private string valueField;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
    public class jamkningTYPE
    {
        private int procentField;
        private bool procentFieldSpecified;
        private decimal beloppField;
        private bool beloppFieldSpecified;
        private decimal maxbeloppField;
        private bool maxbeloppFieldSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int procent
        {
            get
            {
                return this.procentField;
            }
            set
            {
                this.procentField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool procentSpecified
        {
            get
            {
                return this.procentFieldSpecified;
            }
            set
            {
                this.procentFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool beloppSpecified
        {
            get
            {
                return this.beloppFieldSpecified;
            }
            set
            {
                this.beloppFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal maxbelopp
        {
            get
            {
                return this.maxbeloppField;
            }
            set
            {
                this.maxbeloppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maxbeloppSpecified
        {
            get
            {
                return this.maxbeloppFieldSpecified;
            }
            set
            {
                this.maxbeloppFieldSpecified = value;
            }
        }
    }
    public class utmatningTYPE
    {
        private decimal beloppField;
        private decimal forbehallField;
        private bool forbehallFieldSpecified;
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal forbehall
        {
            get
            {
                return this.forbehallField;
            }
            set
            {
                this.forbehallField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool forbehallSpecified
        {
            get
            {
                return this.forbehallFieldSpecified;
            }
            set
            {
                this.forbehallFieldSpecified = value;
            }
        }
    }
    public class loneutbetalningTYPE
    {
        private lonebeskedTYPE[] lonebeskedField;
        [System.Xml.Serialization.XmlElementAttribute("lonebesked", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonebeskedTYPE[] lonebesked
        {
            get
            {
                return this.lonebeskedField;
            }
            set
            {
                this.lonebeskedField = value;
            }
        }
    }
    public class lonebeskedTYPE
    {
        private string periodidField;
        private string periodtextField;
        private DateTime betaldatumField;
        private bool betaldatumFieldSpecified;
        private string fornamnField;
        private string efternamnField;
        private string extraadressField;
        private string postadressField;
        private string postnrField;
        private string ortField;
        private string landField;
        private string clearingnrField;
        private string bankkontoField;
        private decimal skattprocentField;
        private bool skattprocentFieldSpecified;
        private int skattetabellField;
        private bool skattetabellFieldSpecified;
        private decimal jamkningprcField;
        private bool jamkningprcFieldSpecified;
        private decimal jamkningbelField;
        private bool jamkningbelFieldSpecified;
        private int skattekolumnField;
        private bool skattekolumnFieldSpecified;
        private decimal tabellskattField;
        private bool tabellskattFieldSpecified;
        private decimal engangsskattField;
        private bool engangsskattFieldSpecified;
        private decimal kapitalskattField;
        private bool kapitalskattFieldSpecified;
        private decimal extraskattField;
        private bool extraskattFieldSpecified;
        private decimal utbetaltField;
        private bool utbetaltFieldSpecified;
        private decimal arbavgiftprcField;
        private bool arbavgiftprcFieldSpecified;
        private decimal arbavgiftbelField;
        private bool arbavgiftbelFieldSpecified;
        private loneraderTYPE loneraderField;
        private decimal ackbruttolonField;
        private bool ackbruttolonFieldSpecified;
        private decimal ackprelskattField;
        private bool ackprelskattFieldSpecified;
        private decimal acknettolonField;
        private bool acknettolonFieldSpecified;
        private decimal flexsaldoField;
        private bool flexsaldoFieldSpecified;
        private decimal kompsaldoField;
        private bool kompsaldoFieldSpecified;
        private decimal tidbanktimField;
        private bool tidbanktimFieldSpecified;
        private decimal tidbankbelField;
        private bool tidbankbelFieldSpecified;
        private decimal sembettotField;
        private bool sembettotFieldSpecified;
        private decimal sembetutbField;
        private bool sembetutbFieldSpecified;
        private decimal semobetotField;
        private bool semobetotFieldSpecified;
        private decimal semobeutbField;
        private bool semobeutbFieldSpecified;
        private decimal semfortotField;
        private bool semfortotFieldSpecified;
        private decimal semforutbField;
        private bool semforutbFieldSpecified;
        private decimal semspatotField;
        private bool semspatotFieldSpecified;
        private decimal semspautbField;
        private bool semspautbFieldSpecified;
        private decimal semlontotField;
        private bool semlontotFieldSpecified;
        private decimal semlonutbField;
        private bool semlonutbFieldSpecified;
        private konteringTYPE konteringField;
        private string infoField;
        private string anstidField;
        private string persnrField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string periodid
        {
            get
            {
                return this.periodidField;
            }
            set
            {
                this.periodidField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string periodtext
        {
            get
            {
                return this.periodtextField;
            }
            set
            {
                this.periodtextField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime betaldatum
        {
            get
            {
                return this.betaldatumField;
            }
            set
            {
                this.betaldatumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool betaldatumSpecified
        {
            get
            {
                return this.betaldatumFieldSpecified;
            }
            set
            {
                this.betaldatumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string fornamn
        {
            get
            {
                return this.fornamnField;
            }
            set
            {
                this.fornamnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string efternamn
        {
            get
            {
                return this.efternamnField;
            }
            set
            {
                this.efternamnField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string extraadress
        {
            get
            {
                return this.extraadressField;
            }
            set
            {
                this.extraadressField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string postadress
        {
            get
            {
                return this.postadressField;
            }
            set
            {
                this.postadressField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string postnr
        {
            get
            {
                return this.postnrField;
            }
            set
            {
                this.postnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ort
        {
            get
            {
                return this.ortField;
            }
            set
            {
                this.ortField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string land
        {
            get
            {
                return this.landField;
            }
            set
            {
                this.landField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string clearingnr
        {
            get
            {
                return this.clearingnrField;
            }
            set
            {
                this.clearingnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string bankkonto
        {
            get
            {
                return this.bankkontoField;
            }
            set
            {
                this.bankkontoField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal skattprocent
        {
            get
            {
                return this.skattprocentField;
            }
            set
            {
                this.skattprocentField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattprocentSpecified
        {
            get
            {
                return this.skattprocentFieldSpecified;
            }
            set
            {
                this.skattprocentFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int skattetabell
        {
            get
            {
                return this.skattetabellField;
            }
            set
            {
                this.skattetabellField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattetabellSpecified
        {
            get
            {
                return this.skattetabellFieldSpecified;
            }
            set
            {
                this.skattetabellFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal jamkningprc
        {
            get
            {
                return this.jamkningprcField;
            }
            set
            {
                this.jamkningprcField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool jamkningprcSpecified
        {
            get
            {
                return this.jamkningprcFieldSpecified;
            }
            set
            {
                this.jamkningprcFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal jamkningbel
        {
            get
            {
                return this.jamkningbelField;
            }
            set
            {
                this.jamkningbelField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool jamkningbelSpecified
        {
            get
            {
                return this.jamkningbelFieldSpecified;
            }
            set
            {
                this.jamkningbelFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int skattekolumn
        {
            get
            {
                return this.skattekolumnField;
            }
            set
            {
                this.skattekolumnField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattekolumnSpecified
        {
            get
            {
                return this.skattekolumnFieldSpecified;
            }
            set
            {
                this.skattekolumnFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal tabellskatt
        {
            get
            {
                return this.tabellskattField;
            }
            set
            {
                this.tabellskattField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tabellskattSpecified
        {
            get
            {
                return this.tabellskattFieldSpecified;
            }
            set
            {
                this.tabellskattFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal engangsskatt
        {
            get
            {
                return this.engangsskattField;
            }
            set
            {
                this.engangsskattField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool engangsskattSpecified
        {
            get
            {
                return this.engangsskattFieldSpecified;
            }
            set
            {
                this.engangsskattFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal kapitalskatt
        {
            get
            {
                return this.kapitalskattField;
            }
            set
            {
                this.kapitalskattField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kapitalskattSpecified
        {
            get
            {
                return this.kapitalskattFieldSpecified;
            }
            set
            {
                this.kapitalskattFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal extraskatt
        {
            get
            {
                return this.extraskattField;
            }
            set
            {
                this.extraskattField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extraskattSpecified
        {
            get
            {
                return this.extraskattFieldSpecified;
            }
            set
            {
                this.extraskattFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal utbetalt
        {
            get
            {
                return this.utbetaltField;
            }
            set
            {
                this.utbetaltField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool utbetaltSpecified
        {
            get
            {
                return this.utbetaltFieldSpecified;
            }
            set
            {
                this.utbetaltFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal arbavgiftprc
        {
            get
            {
                return this.arbavgiftprcField;
            }
            set
            {
                this.arbavgiftprcField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool arbavgiftprcSpecified
        {
            get
            {
                return this.arbavgiftprcFieldSpecified;
            }
            set
            {
                this.arbavgiftprcFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal arbavgiftbel
        {
            get
            {
                return this.arbavgiftbelField;
            }
            set
            {
                this.arbavgiftbelField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool arbavgiftbelSpecified
        {
            get
            {
                return this.arbavgiftbelFieldSpecified;
            }
            set
            {
                this.arbavgiftbelFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public loneraderTYPE lonerader
        {
            get
            {
                return this.loneraderField;
            }
            set
            {
                this.loneraderField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal ackbruttolon
        {
            get
            {
                return this.ackbruttolonField;
            }
            set
            {
                this.ackbruttolonField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ackbruttolonSpecified
        {
            get
            {
                return this.ackbruttolonFieldSpecified;
            }
            set
            {
                this.ackbruttolonFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal ackprelskatt
        {
            get
            {
                return this.ackprelskattField;
            }
            set
            {
                this.ackprelskattField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ackprelskattSpecified
        {
            get
            {
                return this.ackprelskattFieldSpecified;
            }
            set
            {
                this.ackprelskattFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal acknettolon
        {
            get
            {
                return this.acknettolonField;
            }
            set
            {
                this.acknettolonField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool acknettolonSpecified
        {
            get
            {
                return this.acknettolonFieldSpecified;
            }
            set
            {
                this.acknettolonFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal flexsaldo
        {
            get
            {
                return this.flexsaldoField;
            }
            set
            {
                this.flexsaldoField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool flexsaldoSpecified
        {
            get
            {
                return this.flexsaldoFieldSpecified;
            }
            set
            {
                this.flexsaldoFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal kompsaldo
        {
            get
            {
                return this.kompsaldoField;
            }
            set
            {
                this.kompsaldoField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kompsaldoSpecified
        {
            get
            {
                return this.kompsaldoFieldSpecified;
            }
            set
            {
                this.kompsaldoFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal tidbanktim
        {
            get
            {
                return this.tidbanktimField;
            }
            set
            {
                this.tidbanktimField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tidbanktimSpecified
        {
            get
            {
                return this.tidbanktimFieldSpecified;
            }
            set
            {
                this.tidbanktimFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal tidbankbel
        {
            get
            {
                return this.tidbankbelField;
            }
            set
            {
                this.tidbankbelField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tidbankbelSpecified
        {
            get
            {
                return this.tidbankbelFieldSpecified;
            }
            set
            {
                this.tidbankbelFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal sembettot
        {
            get
            {
                return this.sembettotField;
            }
            set
            {
                this.sembettotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sembettotSpecified
        {
            get
            {
                return this.sembettotFieldSpecified;
            }
            set
            {
                this.sembettotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal sembetutb
        {
            get
            {
                return this.sembetutbField;
            }
            set
            {
                this.sembetutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sembetutbSpecified
        {
            get
            {
                return this.sembetutbFieldSpecified;
            }
            set
            {
                this.sembetutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semobetot
        {
            get
            {
                return this.semobetotField;
            }
            set
            {
                this.semobetotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semobetotSpecified
        {
            get
            {
                return this.semobetotFieldSpecified;
            }
            set
            {
                this.semobetotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semobeutb
        {
            get
            {
                return this.semobeutbField;
            }
            set
            {
                this.semobeutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semobeutbSpecified
        {
            get
            {
                return this.semobeutbFieldSpecified;
            }
            set
            {
                this.semobeutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semfortot
        {
            get
            {
                return this.semfortotField;
            }
            set
            {
                this.semfortotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semfortotSpecified
        {
            get
            {
                return this.semfortotFieldSpecified;
            }
            set
            {
                this.semfortotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semforutb
        {
            get
            {
                return this.semforutbField;
            }
            set
            {
                this.semforutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semforutbSpecified
        {
            get
            {
                return this.semforutbFieldSpecified;
            }
            set
            {
                this.semforutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semspatot
        {
            get
            {
                return this.semspatotField;
            }
            set
            {
                this.semspatotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semspatotSpecified
        {
            get
            {
                return this.semspatotFieldSpecified;
            }
            set
            {
                this.semspatotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semspautb
        {
            get
            {
                return this.semspautbField;
            }
            set
            {
                this.semspautbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semspautbSpecified
        {
            get
            {
                return this.semspautbFieldSpecified;
            }
            set
            {
                this.semspautbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semlontot
        {
            get
            {
                return this.semlontotField;
            }
            set
            {
                this.semlontotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semlontotSpecified
        {
            get
            {
                return this.semlontotFieldSpecified;
            }
            set
            {
                this.semlontotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semlonutb
        {
            get
            {
                return this.semlonutbField;
            }
            set
            {
                this.semlonutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semlonutbSpecified
        {
            get
            {
                return this.semlonutbFieldSpecified;
            }
            set
            {
                this.semlonutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public konteringTYPE kontering
        {
            get
            {
                return this.konteringField;
            }
            set
            {
                this.konteringField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
    }
    public class loneraderTYPE
    {
        private lonradTYPE[] lonradField;
        [System.Xml.Serialization.XmlElementAttribute("lonrad", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonradTYPE[] lonrad
        {
            get
            {
                return this.lonradField;
            }
            set
            {
                this.lonradField = value;
            }
        }
    }
    public class lonradTYPE
    {
        private string lonartField;
        private fontTYPE fontField;
        private bool fontFieldSpecified;
        private string benamningField;
        private string kommentarField;
        private DateTime datumfromField;
        private bool datumfromFieldSpecified;
        private DateTime datumtomField;
        private bool datumtomFieldSpecified;
        private decimal timmarField;
        private bool timmarFieldSpecified;
        private decimal arbetsdagarField;
        private bool arbetsdagarFieldSpecified;
        private decimal dagarField;
        private bool dagarFieldSpecified;
        private string enhetField;
        private decimal antalField;
        private bool antalFieldSpecified;
        private decimal aprisField;
        private bool aprisFieldSpecified;
        private decimal beloppField;
        private bool beloppFieldSpecified;
        private lonetypTYPE lonetypField;
        private bool lonetypFieldSpecified;
        private skattetypTYPE skattetypField;
        private bool skattetypFieldSpecified;
        private decimal skattprocentField;
        private bool skattprocentFieldSpecified;
        private avgifttypTYPE avgifttypField;
        private bool avgifttypFieldSpecified;
        private decimal avgiftprocentField;
        private bool avgiftprocentFieldSpecified;
        private bool regionalField;
        private bool regionalFieldSpecified;
        private string kontonrField;
        private kundnrTYPE kundnrField;
        private resenheterTYPE resenheterField;
        private string statistikkodField;
        private string kontrolluppgiftField;
        private string infoField;
        private int radnrField;
        private bool radnrFieldSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string lonart
        {
            get
            {
                return this.lonartField;
            }
            set
            {
                this.lonartField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public fontTYPE font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fontSpecified
        {
            get
            {
                return this.fontFieldSpecified;
            }
            set
            {
                this.fontFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string benamning
        {
            get
            {
                return this.benamningField;
            }
            set
            {
                this.benamningField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kommentar
        {
            get
            {
                return this.kommentarField;
            }
            set
            {
                this.kommentarField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime datumfrom
        {
            get
            {
                return this.datumfromField;
            }
            set
            {
                this.datumfromField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumfromSpecified
        {
            get
            {
                return this.datumfromFieldSpecified;
            }
            set
            {
                this.datumfromFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DateTime datumtom
        {
            get
            {
                return this.datumtomField;
            }
            set
            {
                this.datumtomField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumtomSpecified
        {
            get
            {
                return this.datumtomFieldSpecified;
            }
            set
            {
                this.datumtomFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal timmar
        {
            get
            {
                return this.timmarField;
            }
            set
            {
                this.timmarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timmarSpecified
        {
            get
            {
                return this.timmarFieldSpecified;
            }
            set
            {
                this.timmarFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal arbetsdagar
        {
            get
            {
                return this.arbetsdagarField;
            }
            set
            {
                this.arbetsdagarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool arbetsdagarSpecified
        {
            get
            {
                return this.arbetsdagarFieldSpecified;
            }
            set
            {
                this.arbetsdagarFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal dagar
        {
            get
            {
                return this.dagarField;
            }
            set
            {
                this.dagarField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dagarSpecified
        {
            get
            {
                return this.dagarFieldSpecified;
            }
            set
            {
                this.dagarFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string enhet
        {
            get
            {
                return this.enhetField;
            }
            set
            {
                this.enhetField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal antal
        {
            get
            {
                return this.antalField;
            }
            set
            {
                this.antalField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool antalSpecified
        {
            get
            {
                return this.antalFieldSpecified;
            }
            set
            {
                this.antalFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal apris
        {
            get
            {
                return this.aprisField;
            }
            set
            {
                this.aprisField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool aprisSpecified
        {
            get
            {
                return this.aprisFieldSpecified;
            }
            set
            {
                this.aprisFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool beloppSpecified
        {
            get
            {
                return this.beloppFieldSpecified;
            }
            set
            {
                this.beloppFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public lonetypTYPE lonetyp
        {
            get
            {
                return this.lonetypField;
            }
            set
            {
                this.lonetypField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool lonetypSpecified
        {
            get
            {
                return this.lonetypFieldSpecified;
            }
            set
            {
                this.lonetypFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public skattetypTYPE skattetyp
        {
            get
            {
                return this.skattetypField;
            }
            set
            {
                this.skattetypField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattetypSpecified
        {
            get
            {
                return this.skattetypFieldSpecified;
            }
            set
            {
                this.skattetypFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal skattprocent
        {
            get
            {
                return this.skattprocentField;
            }
            set
            {
                this.skattprocentField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool skattprocentSpecified
        {
            get
            {
                return this.skattprocentFieldSpecified;
            }
            set
            {
                this.skattprocentFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public avgifttypTYPE avgifttyp
        {
            get
            {
                return this.avgifttypField;
            }
            set
            {
                this.avgifttypField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool avgifttypSpecified
        {
            get
            {
                return this.avgifttypFieldSpecified;
            }
            set
            {
                this.avgifttypFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal avgiftprocent
        {
            get
            {
                return this.avgiftprocentField;
            }
            set
            {
                this.avgiftprocentField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool avgiftprocentSpecified
        {
            get
            {
                return this.avgiftprocentFieldSpecified;
            }
            set
            {
                this.avgiftprocentFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool regional
        {
            get
            {
                return this.regionalField;
            }
            set
            {
                this.regionalField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool regionalSpecified
        {
            get
            {
                return this.regionalFieldSpecified;
            }
            set
            {
                this.regionalFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontonr
        {
            get
            {
                return this.kontonrField;
            }
            set
            {
                this.kontonrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public kundnrTYPE kundnr
        {
            get
            {
                return this.kundnrField;
            }
            set
            {
                this.kundnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenheterTYPE resenheter
        {
            get
            {
                return this.resenheterField;
            }
            set
            {
                this.resenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string statistikkod
        {
            get
            {
                return this.statistikkodField;
            }
            set
            {
                this.statistikkodField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string kontrolluppgift
        {
            get
            {
                return this.kontrolluppgiftField;
            }
            set
            {
                this.kontrolluppgiftField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int radnr
        {
            get
            {
                return this.radnrField;
            }
            set
            {
                this.radnrField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool radnrSpecified
        {
            get
            {
                return this.radnrFieldSpecified;
            }
            set
            {
                this.radnrFieldSpecified = value;
            }
        }
    }
    public enum fontTYPE
    {
        DOLD,
        FET,
    }
    public enum lonetypTYPE
    {
        BRUTTO,
        FÖRMÅN,
        NETTO,
        SKATT,
        KAPITAL,
        KAPITALSKATT,
        PENSION,
    }
    public enum skattetypTYPE
    {
        TABELL,
        ENGÅNGS,
        KAPITAL,
    }
    public enum avgifttypTYPE
    {
        FULL,
        UNGDOM,
        PENSIONÄR,
        AMBASSAD,
        USA,
        LÖNESKATT,
    }
    public class konteringTYPE
    {
        private transaktionTYPE[] transaktionField;
        [System.Xml.Serialization.XmlElementAttribute("transaktion", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public transaktionTYPE[] transaktion
        {
            get
            {
                return this.transaktionField;
            }
            set
            {
                this.transaktionField = value;
            }
        }
    }
    public class transaktionTYPE
    {
        private kundnrTYPE kundnrField;
        private resenheterTYPE resenheterField;
        private string kontonrField;
        private decimal beloppField;
        private bool beloppFieldSpecified;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public kundnrTYPE kundnr
        {
            get
            {
                return this.kundnrField;
            }
            set
            {
                this.kundnrField = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public resenheterTYPE resenheter
        {
            get
            {
                return this.resenheterField;
            }
            set
            {
                this.resenheterField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string kontonr
        {
            get
            {
                return this.kontonrField;
            }
            set
            {
                this.kontonrField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal belopp
        {
            get
            {
                return this.beloppField;
            }
            set
            {
                this.beloppField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool beloppSpecified
        {
            get
            {
                return this.beloppFieldSpecified;
            }
            set
            {
                this.beloppFieldSpecified = value;
            }
        }
    }
    public class saldonTYPE
    {
        private saldoTYPE[] saldoField;
        [System.Xml.Serialization.XmlElementAttribute("saldo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public saldoTYPE[] saldo
        {
            get
            {
                return this.saldoField;
            }
            set
            {
                this.saldoField = value;
            }
        }
    }
    public class saldoTYPE
    {
        private DateTime datumField;
        private bool datumFieldSpecified;
        private decimal ackbruttolonField;
        private bool ackbruttolonFieldSpecified;
        private decimal ackprelskattField;
        private bool ackprelskattFieldSpecified;
        private decimal acknettolonField;
        private bool acknettolonFieldSpecified;
        private decimal flexsaldoField;
        private bool flexsaldoFieldSpecified;
        private decimal kompsaldoField;
        private bool kompsaldoFieldSpecified;
        private decimal tidbanktimField;
        private bool tidbanktimFieldSpecified;
        private decimal tidbankbelField;
        private bool tidbankbelFieldSpecified;
        private decimal sembettotField;
        private bool sembettotFieldSpecified;
        private decimal sembetutbField;
        private bool sembetutbFieldSpecified;
        private decimal semobetotField;
        private bool semobetotFieldSpecified;
        private decimal semobeutbField;
        private bool semobeutbFieldSpecified;
        private decimal semfortotField;
        private bool semfortotFieldSpecified;
        private decimal semforutbField;
        private bool semforutbFieldSpecified;
        private decimal semspatotField;
        private bool semspatotFieldSpecified;
        private decimal semspautbField;
        private bool semspautbFieldSpecified;
        private decimal semlontotField;
        private bool semlontotFieldSpecified;
        private decimal semlonutbField;
        private bool semlonutbFieldSpecified;
        private string infoField;
        private string anstidField;
        private string persnrField;
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "date")]
        public DateTime datum
        {
            get
            {
                return this.datumField;
            }
            set
            {
                this.datumField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool datumSpecified
        {
            get
            {
                return this.datumFieldSpecified;
            }
            set
            {
                this.datumFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal ackbruttolon
        {
            get
            {
                return this.ackbruttolonField;
            }
            set
            {
                this.ackbruttolonField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ackbruttolonSpecified
        {
            get
            {
                return this.ackbruttolonFieldSpecified;
            }
            set
            {
                this.ackbruttolonFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal ackprelskatt
        {
            get
            {
                return this.ackprelskattField;
            }
            set
            {
                this.ackprelskattField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ackprelskattSpecified
        {
            get
            {
                return this.ackprelskattFieldSpecified;
            }
            set
            {
                this.ackprelskattFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal acknettolon
        {
            get
            {
                return this.acknettolonField;
            }
            set
            {
                this.acknettolonField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool acknettolonSpecified
        {
            get
            {
                return this.acknettolonFieldSpecified;
            }
            set
            {
                this.acknettolonFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal flexsaldo
        {
            get
            {
                return this.flexsaldoField;
            }
            set
            {
                this.flexsaldoField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool flexsaldoSpecified
        {
            get
            {
                return this.flexsaldoFieldSpecified;
            }
            set
            {
                this.flexsaldoFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal kompsaldo
        {
            get
            {
                return this.kompsaldoField;
            }
            set
            {
                this.kompsaldoField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kompsaldoSpecified
        {
            get
            {
                return this.kompsaldoFieldSpecified;
            }
            set
            {
                this.kompsaldoFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal tidbanktim
        {
            get
            {
                return this.tidbanktimField;
            }
            set
            {
                this.tidbanktimField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tidbanktimSpecified
        {
            get
            {
                return this.tidbanktimFieldSpecified;
            }
            set
            {
                this.tidbanktimFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal tidbankbel
        {
            get
            {
                return this.tidbankbelField;
            }
            set
            {
                this.tidbankbelField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tidbankbelSpecified
        {
            get
            {
                return this.tidbankbelFieldSpecified;
            }
            set
            {
                this.tidbankbelFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal sembettot
        {
            get
            {
                return this.sembettotField;
            }
            set
            {
                this.sembettotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sembettotSpecified
        {
            get
            {
                return this.sembettotFieldSpecified;
            }
            set
            {
                this.sembettotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal sembetutb
        {
            get
            {
                return this.sembetutbField;
            }
            set
            {
                this.sembetutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sembetutbSpecified
        {
            get
            {
                return this.sembetutbFieldSpecified;
            }
            set
            {
                this.sembetutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semobetot
        {
            get
            {
                return this.semobetotField;
            }
            set
            {
                this.semobetotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semobetotSpecified
        {
            get
            {
                return this.semobetotFieldSpecified;
            }
            set
            {
                this.semobetotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semobeutb
        {
            get
            {
                return this.semobeutbField;
            }
            set
            {
                this.semobeutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semobeutbSpecified
        {
            get
            {
                return this.semobeutbFieldSpecified;
            }
            set
            {
                this.semobeutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semfortot
        {
            get
            {
                return this.semfortotField;
            }
            set
            {
                this.semfortotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semfortotSpecified
        {
            get
            {
                return this.semfortotFieldSpecified;
            }
            set
            {
                this.semfortotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semforutb
        {
            get
            {
                return this.semforutbField;
            }
            set
            {
                this.semforutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semforutbSpecified
        {
            get
            {
                return this.semforutbFieldSpecified;
            }
            set
            {
                this.semforutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semspatot
        {
            get
            {
                return this.semspatotField;
            }
            set
            {
                this.semspatotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semspatotSpecified
        {
            get
            {
                return this.semspatotFieldSpecified;
            }
            set
            {
                this.semspatotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semspautb
        {
            get
            {
                return this.semspautbField;
            }
            set
            {
                this.semspautbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semspautbSpecified
        {
            get
            {
                return this.semspautbFieldSpecified;
            }
            set
            {
                this.semspautbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semlontot
        {
            get
            {
                return this.semlontotField;
            }
            set
            {
                this.semlontotField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semlontotSpecified
        {
            get
            {
                return this.semlontotFieldSpecified;
            }
            set
            {
                this.semlontotFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal semlonutb
        {
            get
            {
                return this.semlonutbField;
            }
            set
            {
                this.semlonutbField = value;
            }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool semlonutbSpecified
        {
            get
            {
                return this.semlonutbFieldSpecified;
            }
            set
            {
                this.semlonutbFieldSpecified = value;
            }
        }
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string info
        {
            get
            {
                return this.infoField;
            }
            set
            {
                this.infoField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string anstid
        {
            get
            {
                return this.anstidField;
            }
            set
            {
                this.anstidField = value;
            }
        }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string persnr
        {
            get
            {
                return this.persnrField;
            }
            set
            {
                this.persnrField = value;
            }
        }
    }
    public enum utbetperiodTYPE
    {
        MÅN,
        VEC,
    }

}