using SoftOne.Soe.Common.Util;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SoftOne.Soe.Business.Util.AltInn
{
    [Serializable]
    [XmlRoot("Skjema")]
    public class RF002_VatDeclaration
    {
        /*
         * Fra og med 2012 øker matmomsen fra 14% til 15%. Dette innbærer at det må gjøres endringer i MVA-skjemaene. Imidlertid kan vi beholde dagens utgaver av skjemaene i Altinn. Det er derfor ikke nødvendig med nye OR-spesifikasjoner eller nye utgavekoder på skjemaene. Nåværende utgavekoder vil fortsatt gjelde:
         * RF-0002: 
         * ServiceCode: 1046
         * ServiceEdition: 100219
         * DataFormatId: 212 (skjemanr.)
         * DataFormatVersion: 10420 (spesifikasjonsnr.)
         */

        #region Fields

        private Skjema baseNode;

        #endregion

        #region Constants

        /// <summary>
        /// 1046
        /// </summary>
        public const string ExternalServiceCode = "1046";
        /// <summary>
        /// 313
        /// </summary>
        public const int ExternalServiceEditionCode = 100219; // old code that I found somewhere but doesn't work 313;

        /// <summary>
        /// 212
        /// </summary>
        public string DataFormatId
        {
            get
            {
                return this.baseNode.skjemanummer;
            }
        }

        /// <summary>
        /// 10420
        /// </summary>
        public int DataFormatVersion
        {
            get
            {
                return int.Parse(this.baseNode.spesifikasjonsnummer);
            }
        }

        #endregion

        #region Properties

        public string ErrorMessage { get; private set; }
        public bool Completed { get; set; }

        #region FormInputs

        // These fields are just shortcuts from baseNode

        #region GenerellInformasjon

        public RapporteringsenhetAdressedatadef21773 Address
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Avgiftspliktiggrp50.RapporteringsenhetAdressedatadef21773;
            }
        }

        public RapporteringsenhetPoststeddatadef21775 City
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Avgiftspliktiggrp50.RapporteringsenhetPoststeddatadef21775;
            }
        }

        public RapporteringsenhetPostnummerdatadef21774 PostalCode
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Avgiftspliktiggrp50.RapporteringsenhetPostnummerdatadef21774;
            }
        }

        public RapporteringsenhetOrganisasjonsnummerdatadef21772 OrgNr
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Avgiftspliktiggrp50.RapporteringsenhetOrganisasjonsnummerdatadef21772;
            }
        }

        public RapporteringsenhetKontonummerdatadef21776 AccountNr
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Avgiftspliktiggrp50.RapporteringsenhetKontonummerdatadef21776;
            }
        }

        public RapporteringsenhetNavndatadef21771 CompanyName
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Avgiftspliktiggrp50.RapporteringsenhetNavndatadef21771;
            }
        }

        public OmsetningsoppgaverTilleggsopplysningerdatadef19684 Comment
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Tilleggsopplysningergrp197.OmsetningsoppgaverTilleggsopplysningerdatadef19684;
            }
        }

        /// <summary>
        /// Yearly monthly etc
        /// </summary>
        public AltInnPeriodTypeEnum PeriodType
        {
            get
            {
                return (AltInnPeriodTypeEnum)int.Parse(this.baseNode.GenerellInformasjongrp2581.Termingrp2582.TerminTypedatadef10092.Value);
            }
            set
            {
                this.baseNode.GenerellInformasjongrp2581.Termingrp2582.TerminTypedatadef10092.Value = value.ToString();
            }
        }

        public int PeriodMonth
        {
            get
            {
                return (int)this.baseNode.GenerellInformasjongrp2581.Termingrp2582.Termindatadef10093.Value;
            }
            set
            {
                // Months ends with 5 and contains 3 chars
                var enumName = "Item" + (value < 10 ? "0" : "") + value + "5";
                var valueEnum = (KodelisteEttValg93Terminrepformat69)Enum.Parse(typeof(KodelisteEttValg93Terminrepformat69), enumName, true);

                //if(!Enum.IsDefined(typeof(KodelisteEttValg93Terminrepformat69), value))
                //    throw new NotSupportedException(typeof(KodelisteEttValg93Terminrepformat69).ToString() + " is not supported");
                
                this.baseNode.GenerellInformasjongrp2581.Termingrp2582.Termindatadef10093.Value = valueEnum;
            }
        }

        public int PeriodTwoMonthly
        {
            get
            {
                return (int)this.baseNode.GenerellInformasjongrp2581.Termingrp2582.Termindatadef10093.Value;
            }
            set
            {
                // Two months ends with 4 and contains 3 chars
                var enumName = "Item0" + value + "4";
                var valueEnum = (KodelisteEttValg93Terminrepformat69)Enum.Parse(typeof(KodelisteEttValg93Terminrepformat69), enumName, true);

                this.baseNode.GenerellInformasjongrp2581.Termingrp2582.Termindatadef10093.Value = valueEnum;
            }
        }

        public TerminArdatadef10094 Year
        {
            get
            {
                return this.baseNode.GenerellInformasjongrp2581.Termingrp2582.TerminArdatadef10094;
            }
        }

        #endregion

        #region Avgiftspostergrp2577

        /// <summary>
        /// Her føres summen av: 
        /// a. all omsetning og uttak av varer og tjenester som er unntatt fra merverdiavgiftsloven 
        /// b. all omsetning og uttak av varer og tjenester som er omfattet av merverdiavgiftsloven, dvs. post 2 
        /// Beløpet innhentes til bruk i offisiell statistikk. Finansielle inntekter, eventuelle offentlige tilskudd og merverdiavgift skal ikke tas med.
        /// </summary>
        public OmsetningTermindatadef8446 Post1
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.OmsetningTermindatadef8446;
            }
        }

        /// <summary>
        /// Feltet fylles automatisk.
        /// Summen av post 3, 4, 5 og 6. Avgift ikke medregnet
        /// </summary>
        public OmsetningTerminAvgiftspliktigdatadef10095 Post2
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.OmsetningTerminAvgiftspliktigdatadef10095;
            }
        }

        /// <summary>
        /// Her føres omsetning og uttak av varer og tjenester som er innenfor merverdiavgiftsloven, men som er fritatt for merverdiavgift etter lovens kapittel 6, og selgers omsetning av klimakvoter til næringsdrivende og offentlig virksomhet med omvendt avgiftsplikt, jf mval. § 11-1(2).
        /// Post 3, 4, 5 og 6 summeres i post 2.
        /// </summary>
        public OmsetningTerminAvgiftsfridatadef10096 Post3
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.OmsetningTerminAvgiftsfridatadef10096;
            }
        }

        public MerverdiavgiftUtgaendeTerminHoySatsGrunnlagdatadef10097 Post4
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.MerverdiavgiftUtgaendeTerminHoySatsGrunnlagdatadef10097;
            }
        }

        public MerverdiavgiftUtgaendeTerminHoySatsBeregnetdatadef10098 Post4Calculated
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftUtgaendeTerminHoySatsBeregnetdatadef10098;
            }
        }

        /// <summary>
        /// I grunnlagsfeltet føres grunnlag for utgående merverdiavgift redusert sats 15 % (14% til og med 6. termin 2011), dvs. omsetning av næringsmidler (mat- og drikkevarer).
        /// Avgiftsfeltet fylles ut automatisk med 15 % (14% til og med 6. termin 2011), av grunnlaget når du går videre i skjemaet. Tallet her kan du endre selv, men da må du gi forklaring nederst på siden.
        /// </summary>
        public MerverdiavgiftUtgaendeTerminMiddelsSatsGrunnlagdatadef20319 Post5
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.MerverdiavgiftUtgaendeTerminMiddelsSatsGrunnlagdatadef20319;
            }
        }

        public MerverdiavgiftUtgaendeTerminMiddelsSatsBeregningdatadef20320 Post5Calculated
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftUtgaendeTerminMiddelsSatsBeregningdatadef20320;
            }
        }

        
        public MerverdiavgiftUtgaendeTerminLavSatsGrunnlagdatadef14360 Post6
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.MerverdiavgiftUtgaendeTerminLavSatsGrunnlagdatadef14360;
            }
        }

        public MerverdiavgiftUtgaendeTerminLavSatsBeregnetdatadef14361 Post6Calculated
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftUtgaendeTerminLavSatsBeregnetdatadef14361;
            }
        }

        public MerverdiavgiftUtgaendeTjenesterUtlandTerminGrunnlagdatadef14362 Post7
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.Grunnlaggrp2578.MerverdiavgiftUtgaendeTjenesterUtlandTerminGrunnlagdatadef14362;
            }
        }

        public MerverdiavgiftUtgaendeTjenesterUtlandTerminBeregnetdatadef14363 Post7Calculated
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftUtgaendeTjenesterUtlandTerminBeregnetdatadef14363;
            }
        }

        public MerverdiavgiftInngaendeTerminHoySatsdatadef8450 Post8
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftInngaendeTerminHoySatsdatadef8450;
            }
        }

        public MerverdiavgiftInngaendeTerminMiddelsSatsdatadef20322 Post9
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftInngaendeTerminMiddelsSatsdatadef20322;
            }
        }

        public MerverdiavgiftInngaendeTerminLavSatsdatadef14364 Post10
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.MerverdiavgiftInngaendeTerminLavSatsdatadef14364;
            }
        }

        public AvgiftTerminTilGodedatadef8452 Post11ToRecieve
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.AvgiftTerminTilGodedatadef8452;
            }
        }

        public AvgiftTerminABetaledatadef8453 Post11ToPay
        {
            get
            {
                return this.baseNode.Avgiftspostergrp2577.PosteneIOppgavengrp5639.BeregnetAvgiftgrp2579.AvgiftTerminABetaledatadef8453;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Constructor

        public RF002_VatDeclaration()
        {
            // We need to call the constructor of all members in order to generate orid ids.
            this.baseNode = new Skjema()
            {
                GenerellInformasjongrp2581 = new GenerellInformasjongrp2581()
                {
                    Avgiftspliktiggrp50 = new Avgiftspliktiggrp50()
                    {
                        RapporteringsenhetAdressedatadef21773 = new RapporteringsenhetAdressedatadef21773()
                        {
                            // Value = "Nygårdsveien 6", //Adress
                        },
                        RapporteringsenhetPoststeddatadef21775 = new RapporteringsenhetPoststeddatadef21775()
                        {
                            // Value = "OSLO", //Poststad
                        },
                        RapporteringsenhetPostnummerdatadef21774 = new RapporteringsenhetPostnummerdatadef21774()
                        {
                            // Value = "0872", //Postnummer
                        },
                        RapporteringsenhetOrganisasjonsnummerdatadef21772 = new RapporteringsenhetOrganisasjonsnummerdatadef21772()
                        {
                            // Value = "910232592", //ORG-nr
                        },
                        RapporteringsenhetKontonummerdatadef21776 = new RapporteringsenhetKontonummerdatadef21776()
                        {
                            // Value = "1312312",
                        },
                        RapporteringsenhetNavndatadef21771 = new RapporteringsenhetNavndatadef21771()
                        {
                            // Value = "MOEN OG RYFOSS REVISJON"
                        },
                        RapportingsenhetKontonummerEndretdatadef24632 = new RapportingsenhetKontonummerEndretdatadef24632(),
                        KIDnummerOmsetningdatadef18616 = new KIDnummerOmsetningdatadef18616(),
                    },
                    Termingrp2582 = new Termingrp2582()
                    {
                        OppgaveTypedatadef5659 = new OppgaveTypedatadef5659()
                        {
                            Value = KodelisteEttValg3Mvaoppgavetyperepformat67.Item1,
                        },
                        TerminTypedatadef10092 = new TerminTypedatadef10092()
                        {
                            Value = "1",
                        },
                        Termindatadef10093 = new Termindatadef10093()
                        {
                            Value = KodelisteEttValg93Terminrepformat69.Item011,
                        },
                        TerminArdatadef10094 = new TerminArdatadef10094()
                        {
                            Value = DateTime.Now.Year.ToString(),
                        },
                    }
                },
                Avgiftspostergrp2577 = new Avgiftspostergrp2577()
                {
                    PosteneIOppgavengrp5639 = new PosteneIOppgavengrp5639()
                    {
                        Grunnlaggrp2578 = new Grunnlaggrp2578()
                        {
                            OmsetningTermindatadef8446 = new OmsetningTermindatadef8446(),
                            OmsetningTerminAvgiftspliktigdatadef10095 = new OmsetningTerminAvgiftspliktigdatadef10095(),
                            OmsetningTerminAvgiftsfridatadef10096 = new OmsetningTerminAvgiftsfridatadef10096(),
                            MerverdiavgiftUtgaendeTerminHoySatsGrunnlagdatadef10097 = new MerverdiavgiftUtgaendeTerminHoySatsGrunnlagdatadef10097(),
                            MerverdiavgiftUtgaendeTerminMiddelsSatsGrunnlagdatadef20319 = new MerverdiavgiftUtgaendeTerminMiddelsSatsGrunnlagdatadef20319(),
                            MerverdiavgiftUtgaendeTerminLavSatsGrunnlagdatadef14360 = new MerverdiavgiftUtgaendeTerminLavSatsGrunnlagdatadef14360(),
                            MerverdiavgiftUtgaendeTjenesterUtlandTerminGrunnlagdatadef14362 = new MerverdiavgiftUtgaendeTjenesterUtlandTerminGrunnlagdatadef14362(),
                        },
                        BeregnetAvgiftgrp2579 = new BeregnetAvgiftgrp2579()
                        {
                            MerverdiavgiftUtgaendeTerminHoySatsBeregnetdatadef10098 = new MerverdiavgiftUtgaendeTerminHoySatsBeregnetdatadef10098(),
                            MerverdiavgiftUtgaendeTerminMiddelsSatsBeregningdatadef20320 = new MerverdiavgiftUtgaendeTerminMiddelsSatsBeregningdatadef20320(),
                            MerverdiavgiftUtgaendeTerminLavSatsBeregnetdatadef14361 = new MerverdiavgiftUtgaendeTerminLavSatsBeregnetdatadef14361(),
                            MerverdiavgiftUtgaendeTjenesterUtlandTerminBeregnetdatadef14363 = new MerverdiavgiftUtgaendeTjenesterUtlandTerminBeregnetdatadef14363(),
                            MerverdiavgiftInngaendeTerminHoySatsdatadef8450 = new MerverdiavgiftInngaendeTerminHoySatsdatadef8450(),
                            MerverdiavgiftInngaendeTerminMiddelsSatsdatadef20322 = new MerverdiavgiftInngaendeTerminMiddelsSatsdatadef20322(),
                            MerverdiavgiftInngaendeTerminLavSatsdatadef14364 = new MerverdiavgiftInngaendeTerminLavSatsdatadef14364(),
                            AvgiftTerminTilGodedatadef8452 = new AvgiftTerminTilGodedatadef8452(),
                            AvgiftTerminABetaledatadef8453 = new AvgiftTerminABetaledatadef8453(),
                        },
                        Tilleggsopplysningergrp197 = new Tilleggsopplysningergrp197()
                        {
                            OmsetningsoppgaverTilleggsopplysningerdatadef19684 = new OmsetningsoppgaverTilleggsopplysningerdatadef19684(),
                            TilleggsopplysningerForklaringSendtdatadef8458 = new TilleggsopplysningerForklaringSendtdatadef8458(),
                        }
                    },
                },
            };
        }

        #endregion Constructor

        #region Methods

        static public void SerializeToXML<T>(T skjema, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            TextWriter textWriter = new StreamWriter(path);
            serializer.Serialize(textWriter, skjema);
            textWriter.Close();
        }

        public XmlDocument ToXmlDocument()
        {
            var serialiseToDocument = new XmlDocument();
            var serializer = new XmlSerializer(this.baseNode.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, this.baseNode);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                serialiseToDocument.Load(stream);
            }

            return serialiseToDocument;
        }

        #endregion

        public bool Validate()
        {
            if (int.Parse(Post1.Value) < int.Parse(Post2.Value))
            {
                this.ErrorMessage = "Post 1 must be greater or equal to Post 2";
                return false;
            }

            return true; 
        }


        
    }
}
