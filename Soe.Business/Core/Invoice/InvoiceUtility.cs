using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
    public static class InvoiceUtility
    {
        public static string GetISO11649(string line)
        {
            /// <summary>
            /// We need to calculate international bank reference (RF) number. For backward compatibility, a Finnish
            /// Reference is first calculated and used as a base for RF reference. As in Swedes reference is not used, it 
            /// can be adapted as Finnish version. 
            /// Requirements for RF - reference makes it mandatory to calculate new checksum which is moved
            /// in front of reference. 
            /// 
            /// After 2013 switchover period alphabetic uppercase characters may be used in RF- reference from A-Z
            /// These are replaced by numbers in string starting A=10, B=11 etc. 
            /// </summary>
            /// <param name="line">Maximum length of refrence line is 20 characters (19+1)</param>
            /// <returns>RF reference</returns>
            ///////////////////
            // adding "RF00" as last characters to calculate checksum - obsolete
            ///////////////////
            string origline = line;
            line = line + "271500";
            string newline = "";

            ///////////////////
            // recode alphabets, we change characters to numeric values A=10, B=11 etc....
            ///////////////////
            foreach (char cc in line)
            {
                int numcharval = (Convert.ToInt32(cc) - 55);  // Value for character. A = 10 , B = 11 etc...
                                                              // We have a value.
                if (numcharval > 9 && numcharval < 36)
                {
                    newline = newline + Convert.ToString(numcharval);
                }
                else
                {
                    newline = newline + cc;
                }
            }
            /////////////////////////
            // newline has all the details to calculate modulo, we need to add prezeroes so we can split 
            // calculation over to 3 parts as 32 - bit systems maybe can't handle integers so big...
            /////////////////////////
            while (newline.Length < 22)
            {
                newline = '0' + newline;
            }

            #region OLD

            /*var Cchecksum = newline.Substring(1, 7);
            var Checksum = Convert.ToInt64(Cchecksum) % 97;

            Cchecksum = Convert.ToString(Checksum) + newline.Substring(8, 7);
            Checksum = Convert.ToInt64(Cchecksum) % 97;

            Cchecksum = Convert.ToString(Checksum) + newline.Substring(15, newline.Length - 15);
            Checksum = Convert.ToInt64(Cchecksum) % 97;*/

            #endregion

            var checksum = 0;

            // One by one process all
            // digits of 'num'
            for (int i = 0; i < newline.Length; i++)
            {
                checksum = (checksum * 10 + (int)newline[i] - '0') % 97;
            }

            checksum = 98 - checksum;

            if (checksum < 10)
            {
                return "RF0" + Convert.ToString(checksum) + origline;
            }
            else
            {
                return "RF" + Convert.ToString(checksum) + origline;
            }

        }

        public static string GetCheckedBankPaymentReference(string referencenumber)
        {
            /// <summary>
            /// We need reference number with minimum length of 3 characters. Valid reference consists of a 
            /// number which length is 3-19 characters + 1 checksum that is calculated here and added into
            /// reference. Banks are using reference to check validity of previous numbers in reference. 
            /// Minimum reference number lenght is 3 characters due to checksum calculation rules. 
            /// Finland suggests that reference is created from customer number * 1 000 000
            /// which will be added as last number
            /// </summary>
            /// <param name="line">Reference base. Minimum length of 3 numbers!</param>
            /// <returns>Reference with checksum</returns>
            string line;
            line = Convert.ToString(referencenumber);
            int linelength = line.Length;
            int summa = 0;
            int multiplier = 7;  // Starting right, weighted by 7,2,1,7,3,1 etc...
            while (linelength > 0)
            {
                char character = line[linelength - 1];
                summa += multiplier * (character - 48);
                switch (multiplier)
                {
                    case 7: multiplier = 3; break;
                    case 3: multiplier = 1; break;
                    case 1: multiplier = 7; break;
                }
                summa %= 10;
                linelength -= 1;
            }
            summa = 10 - summa;
            if (summa != 10)
                return line + summa.ToString();
            else
                return line + "0";
        }

        public static string GetNorwegianKIDNumber(string invoiceNr, string CustomerNr, DateTime year, int count)
        {
            //Norwegian KID nr is calculated using mod11 or mod10 algorithms. For now only 11 is used.

            string yearText = "0";

            //Create String
            if (year.Year == 2012)
            {
                yearText = "2";
            }
            if (year.Year == 2013)
            {
                yearText = "3";
            }
            if (year.Year == 2014)
            {
                yearText = "4";
            }
            if (year.Year == 2015)
            {
                yearText = "5";
            }
            if (year.Year == 2016)
            {
                yearText = "1";
            }
            if (year.Year == 2017)
            {
                yearText = "2";
            }
            if (year.Year == 2018)
            {
                yearText = "3";
            }

            string customerNrText;
            customerNrText = CustomerNr;
            int targetSize = 8;

            if (targetSize > customerNrText.Length)
            {
                string blanks = string.Empty;
                int diff = targetSize - customerNrText.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks += "0";
                }
                customerNrText = blanks + customerNrText;
            }
            else
            {
                customerNrText = customerNrText.Substring(0, targetSize - 1);
            }

            string invoiceNrText;
            invoiceNrText = invoiceNr;
            int targetSizeInvoiceNr = 6;

            if (targetSizeInvoiceNr > invoiceNrText.Length)
            {
                string blanks = string.Empty;
                int diff = targetSizeInvoiceNr - invoiceNrText.Length;
                for (int i = 0; i < diff; i++)
                {
                    blanks += "0";
                }
                invoiceNrText = blanks + invoiceNrText;
            }
            else
            {
                customerNrText = customerNrText.Substring(0, targetSizeInvoiceNr - 1);
            }

            string YearCNrINr = yearText + customerNrText + invoiceNrText;
            char[] chars = YearCNrINr.ToArray();
            int addition = 2;
            int additionNext = 1;
            int sum = 0;

            foreach (var item in chars)
            {

                int valueFirstDigit = 0;
                int valueSecondDigit = 0;
                bool secondDigit = false;

                if (addition == 2)
                    additionNext = 1;

                if (addition == 1)
                    additionNext = 2;

                int value = Convert.ToInt32(item.ToString());

                if ((value * addition) >= 10)
                {
                    string dubbleDigit = Convert.ToString((value * addition));

                    char[] charDubbleDigit = dubbleDigit.ToArray();

                    foreach (var item2 in charDubbleDigit)
                    {

                        if (secondDigit == false)
                        {
                            valueFirstDigit = Convert.ToInt32(item2.ToString());
                        }
                        else
                        {
                            valueSecondDigit = Convert.ToInt32(item2.ToString());
                        }

                        secondDigit = true;

                    }

                    sum += valueFirstDigit + valueSecondDigit;

                }
                else
                {
                    sum += value * addition;
                }

                addition = additionNext;
            }

            int preCheck = sum % 10;

            if (preCheck == 0)
            {
                return YearCNrINr + "0";
            }
            else
            {
                return YearCNrINr + Convert.ToString(10 - preCheck);
            }
        }

        public static string GetSwedishOCRNumber(string customerInvoiceRef)
        {
            string temp = customerInvoiceRef.Count() > 24 ? customerInvoiceRef.Substring(0, 24) : customerInvoiceRef;
            char[] chars = temp.ToArray();
            int addition = 2;
            int sum = 0;

            for (int i = chars.Length - 1; i >= 0; i--)
            {
                int s = Convert.ToInt32(chars[i].ToString()) * addition;

                if (s > 9)
                    s = Convert.ToInt32(s.ToString()[0].ToString()) + Convert.ToInt32(s.ToString()[1].ToString());

                sum += s;
                addition = addition == 2 ? 1 : 2;
            }

            int check = (sum % 10 > 0 ? (sum + (10 - sum % 10)) - sum : 0);

            return temp + check.ToString();
        }

        public static bool ValidateSwedishOCRNumber(string invoiceNr)
        {
            if (string.IsNullOrEmpty(invoiceNr))
            {
                return false;
            }

            var pos = 0;
            int sum = 0;            

            //Step 1 - Starting with the check digit double the value of every other digit(right to left every 2nd digit)
            //Step 2 - If doubling of a number results in a two digits number, add up the digits to get a single digit number.

            foreach (var c in invoiceNr.Take(invoiceNr.Length).Reverse())
            {
                int digit = 0;
                if (!int.TryParse(c.ToString(), out digit))
                {
                    throw new ActionFailedException(TermCacheManager.Instance.GetText(92040, (int)TermGroup.General, "Betalningsförslag kunde inte skapas, OCR får inte innehålla kommatecken."));                   
                }

                if ( (pos % 2) == 0 )
                {
                    sum += digit;
                }
                else
                {
                    var digitx2 = digit * 2;
                    if (digitx2 < 10)
                        sum += digitx2;
                    else
                        sum += digitx2.ToString().ToCharArray().Sum(d => int.Parse(d.ToString()));
                }
                pos++; 
            }

            return (sum % 10) == 0; 
        }

    }
}
