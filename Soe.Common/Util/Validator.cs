using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Common.Util
{
    public static class Validator
    {
        #region Primitive types

        /// <summary>
        /// Indicates whether the specified System.String object is a valid e-mail address.
        /// </summary>
        /// <param name="source">System.String object to verify</param>
        /// <remarks>
        /// The exact same method is implemented in JavaScript in FormValidation.js
        /// for client side validation.
        /// </remarks>
        public static bool ValidateEmail(string source)
        {
            if (String.IsNullOrEmpty(source))
                return false;

            const string validCharsPostAt = "abcdefghijklmnopqrstuvwxyz0123456789-";
            string validChars = validCharsPostAt + "!£#$%&'*+-/=?^_`{|}~";
            bool foundAt = false;
            bool foundDotAfterAt = false;
            char lastc = 'X';
            char[] chars = source.ToLower().ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '@')
                {
                    if (foundAt || lastc.Equals('.') || i.Equals(0))
                        return false;
                    foundAt = true;
                    validChars = validCharsPostAt;
                }
                else if (chars[i] == '.')
                {
                    if (lastc == '.' || lastc == '@' || i == 0 || i == chars.Length - 1)
                        return false;
                    foundDotAfterAt = foundAt;
                }
                else if (validChars.IndexOf(chars[i]) < 0)
                {
                    return false;
                }
                lastc = chars[i];
            }

            if (!(foundAt && foundDotAfterAt))
                return false;

            return true;
        }

        /// <summary>
        /// Indicates whether the specified System.String object is a valid Luhn number.
        /// </summary>
        /// <param name="source">System.String object to verify</param>
        /// <remarks>
        ///	<para>
        ///	The Luhn Algorithm is used for verifying Swedish Personal Numbers.
        ///	</para>
        ///	<para>
        ///	The exact same method is implemented in JavaScript in FormValidation.js
        ///	for client side validation.
        ///	</para>
        /// </remarks>
        public static bool ValidateLuhn(string source)
        {
            if (source == null)
                return false;

            char[] chars = source.ToLower().ToCharArray();
            int sum = 0;
            for (var i = source.Length - 1; i >= 0; i--)
            {
                if (!Int32.TryParse(chars[i].ToString(), out int val))
                    return false;
                val = val * (i % 2 - 2) * -1;
                if (val > 9)
                    val -= 9;
                sum += val;
            }
            return sum % 10 == 0;
        }

        /// <summary>
        /// Indicates whether the specified System.String object is a valid Date.
        /// </summary>
        /// <param name="source">System.String object to verify</param>
        /// <remarks>
        ///	The exact same method is implemented in JavaScript in FormValidation.js
        ///	for client side validation.
        /// </remarks>
        public static bool ValidateDate(string source)
        {
            bool resultvalue = false;
            try
            {
                // SWE DATE
                if (source.IndexOf("-") != -1)
                {
                    string[] dateSplitted = source.Split('-');
                    DateTime testDate = new DateTime(Convert.ToInt32(dateSplitted[0]), Convert.ToInt32(dateSplitted[1]), Convert.ToInt32(dateSplitted[2]));
                    resultvalue = testDate.Date >= DateTime.MinValue;
                }

                // US DATE
                if (source.IndexOf("/") != -1)
                {
                    string[] dateSplitted = source.Split('/');
                    DateTime testDate = new DateTime(Convert.ToInt32(dateSplitted[2]), Convert.ToInt32(dateSplitted[0]), Convert.ToInt32(dateSplitted[1]));
                    resultvalue = testDate.Date >= DateTime.MinValue;
                }
                // FI DATE
                if (source.IndexOf(".") != -1)
                {
                    string[] dateSplitted = source.Split('.');
                    DateTime testDate = new
                    DateTime(Convert.ToInt32(dateSplitted[2]), Convert.ToInt32(dateSplitted[1]), Convert.ToInt32(dateSplitted[0]));
                    resultvalue = testDate.Date >= DateTime.MinValue;
                }

            }
            catch
            {
                // if date "creation" crashes, return false
                resultvalue = false;
            }
            return resultvalue;
        }

        public static bool ValidateSelectInterval(string from, string to)
        {
            bool fromHasValue = !String.IsNullOrEmpty(from) || from != "0";
            bool toHasValue = !String.IsNullOrEmpty(to) || to != "0";
            return fromHasValue == toHasValue;
        }

        public static bool ValidateTextInterval(string from, string to)
        {
            bool fromHasValue = !String.IsNullOrEmpty(from);
            bool toHasValue = !String.IsNullOrEmpty(to);
            return fromHasValue == toHasValue;
        }

        public static bool ValidateDateInterval(DateTime? from, DateTime? to)
        {
            if (!from.HasValue && !to.HasValue)
                return true;
            if (from.HasValue && to.HasValue && ValidateDateInterval(from.Value, to.Value))
                return true;
            return false;
        }

        public static bool ValidateDateInterval(DateTime from, DateTime to)
        {
            return from <= to;
        }

        public static bool ValidateDateInterval(DateTime value, DateTime? intervalFrom, DateTime? intervalTo)
        {
            if (!intervalFrom.HasValue && !intervalTo.HasValue)
                return true;
            if (!ValidateDateInterval(intervalFrom, intervalTo))
                return false;
            return ValidateDateInterval(intervalFrom, value) && ValidateDateInterval(value, intervalTo);
        }

        public static bool ValidateNumericInterval(int? from, int? to)
        {
            if (!from.HasValue && !to.HasValue)
                return true;
            if (from.HasValue && to.HasValue && ValidateNumericInterval(from.Value, to.Value))
                return true;
            return false;
        }

        public static bool ValidateNumericInterval(int from, int to)
        {
            return from >= 0 && from <= to;
        }

        public static bool ExistInInterval(int value, int from, int to)
        {
            if ((value >= from && value <= to))
                return true;
            else
                return false;
        }

        public static bool ValidateStringInterval(string from, string to)
        {
            if (String.IsNullOrEmpty(from) && String.IsNullOrEmpty(to))
                return true;

            if (!String.IsNullOrEmpty(from) && !String.IsNullOrEmpty(to))
            {
                if (Int32.TryParse(from, out int iFrom) && Int32.TryParse(to, out int iTo))
                    return ValidateNumericInterval(iFrom, iTo);
                else
                    return from.CompareTo(to) <= 0;
            }

            return false;
        }

        public static bool ValidateStringInterval(string value, string intervalFrom, string intervalTo)
        {
            if (String.IsNullOrEmpty(intervalFrom) && String.IsNullOrEmpty(intervalTo))
                return true;
            if (!ValidateTextInterval(intervalFrom, intervalTo))
                return false;
            return ValidateStringInterval(intervalFrom, value) && ValidateStringInterval(value, intervalTo);
        }

        public static bool ValidateTrue(int maxTrue, params bool[] values)
        {
            return values.Count(val => val) <= maxTrue;
        }

        public static TermGroup_ScanningInterpretation ValidateScanningEntryRow(string newText, string validationError)
        {
            TermGroup_ScanningInterpretation interpretation = TermGroup_ScanningInterpretation.ValueNotFound;
            if (!String.IsNullOrEmpty(newText))
            {
                //Value is changed by user
                interpretation = TermGroup_ScanningInterpretation.ValueIsValid;
            }
            else
            {
                //0 - No errors. The value is correct.
                //1 - Possible interpretation error. For example, the interpretation seems correct, but it does not match a calculated value.
                //2 - Error. The field has probably not been interpreted correctly.
                if (validationError == "0")
                    interpretation = TermGroup_ScanningInterpretation.ValueIsValid;
                else if (validationError == "1")
                    interpretation = TermGroup_ScanningInterpretation.ValueIsUnsettled;
                else if (validationError == "2")
                    interpretation = TermGroup_ScanningInterpretation.ValueNotFound;
            }
            return interpretation;
        }

        #endregion

        #region Accounts

        /// <summary>
        /// Checks if a accountNr is in a given interval.
        /// Compare int if the accountNr is a int, otherwise it compares string
        /// </summary>
        /// <param name="accountNr">The accountNr to check</param>
        /// <param name="accountDimId">The AccountDimId</param>
        /// <param name="accountInterval">The AccountInterval</param>
        /// <returns>True if the accountNr is in the AccountInterval, otherwise false</returns>
        public static bool IsAccountInInterval(string accountNr, int accountDimId, AccountIntervalDTO accountInterval)
        {
            if (accountDimId == accountInterval.AccountDimId)
                return IsAccountInInterval(accountNr, accountInterval.AccountNrFrom, accountInterval.AccountNrTo);
            return false;
        }

        /// <summary>
        /// Checks if a accountNr is in a given interval.
        /// Compare int if the accountNr is a int, otherwise it compares string
        /// </summary>
        /// <param name="accountNr">The accountNr to check</param>
        /// <param name="accountNrFrom">The accountNr from</param>
        /// <param name="accountNrTo">The accountNr to</param>
        /// <returns>/// <returns>True if the accountNr is between from and to</returns></returns>
        public static bool IsAccountInInterval(string accountNr, string accountNrFrom, string accountNrTo)
        {
            return StringUtility.IsInInterval(accountNr, accountNrFrom, accountNrTo);
        }

        /// <summary>
        /// Checks if acounts in accounInternals1 is are valid in accountInternal2
        /// </summary>
        /// <param name="acountInternals1">The accounts to compare</param>
        /// <param name="accountInternals2">The accounts to compare with</param>
        /// <returns>True if the collections are valid</returns>
        public static bool IsAccountInInterval<T>(List<T> acountInternals1, List<T> accountInternals2, bool approveOneAccountInternal = false, string idName = "AccountId")
        {
            //No accounts to compare
            if (acountInternals1 == null || acountInternals1.Count == 0)
                return true;

            //No accounts to compare with
            if (accountInternals2 == null)
                return false;

            //Special case for one AccountInternal since item: 7245
            //Must have less or equal nr of accounts to compare
            if (acountInternals1.Count == 1 && approveOneAccountInternal && acountInternals1.Count > accountInternals2.Count)
                return false;

            //Each account must exist
            foreach (var acountInternal1 in acountInternals1)
            {
                bool exists = false;

                object acountInternal1Id = acountInternal1.GetType().GetProperty(idName).GetValue(acountInternal1, null);
                foreach (var accountInternal2 in accountInternals2)
                {
                    object acountInternal2Id = accountInternal2.GetType().GetProperty(idName).GetValue(accountInternal2, null);
                    if (Convert.ToInt32(acountInternal1Id) == Convert.ToInt32(acountInternal2Id))
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                    return true;
            }

            return false;
        }

        public static bool IsAccountInInterval(List<AccountInternalDTO> acountInternals1, List<AccountYearBalanceRowDTO> accountInternals2, bool approveOneAccountInternal = false, string idName = "AccountId")
        {
            //No accounts to compare
            if (acountInternals1 == null || acountInternals1.Count == 0)
                return true;

            //No accounts to compare with
            if (accountInternals2 == null)
                return false;

            //Special case for one AccountInternal since item: 7245
            //Must have less or equal nr of accounts to compare
            if (acountInternals1.Count == 1 && approveOneAccountInternal && acountInternals1.Count > accountInternals2.Count)
                return false;

            //Each account must exist
            foreach (var acountInternal1 in acountInternals1)
            {
                bool exists = false;

                object acountInternal1Id = acountInternal1.GetType().GetProperty(idName).GetValue(acountInternal1, null);
                foreach (var accountInternal2 in accountInternals2)
                {
                    object acountInternal2Id = accountInternal2.GetType().GetProperty(idName).GetValue(accountInternal2, null);
                    if (Convert.ToInt32(acountInternal1Id) == Convert.ToInt32(acountInternal2Id))
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                    return true;
            }

            return false;
        }

        public static bool IsAccountInInterval(List<AccountInternalDTO> acountInternals1, List<AccountInternalDTO> accountInternals2, bool approveOneAccountInternal = false, string idName = "AccountId")
        {
            //No accounts to compare
            if (acountInternals1 == null || acountInternals1.Count == 0)
                return true;

            //No accounts to compare with
            if (accountInternals2 == null || accountInternals2.Count == 0)
                return false;

            //Each account must exist
            foreach (var acountInternal1 in acountInternals1)
            {

                object acountInternal1Id = acountInternal1.GetType().GetProperty(idName).GetValue(acountInternal1, null);
                foreach (var accountInternal2 in accountInternals2)
                {
                    object acountInternal2Id = accountInternal2.GetType().GetProperty(idName).GetValue(accountInternal2, null);
                    if (Convert.ToInt32(acountInternal1Id) == Convert.ToInt32(acountInternal2Id))
                    {
                        return true;
                    }
                }

            }
            return false;
        }


        #endregion

        #region Email

        //Function to check email address validity
        public static bool IsValidEmailAddress(string eMail)
        {
            if (string.IsNullOrEmpty(eMail))
                return false;

            string strPattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z_]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z_-])@))" +
                        @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";

            if (Regex.IsMatch(eMail, strPattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
            return false;

        }

        #endregion

        #region Password

        public static bool IsPasswordStrong(string password, int passwordMinLength, int passwordMaxLength)
        {
            string expression = @"^(?=.*\d)(?=.*[a-zåäö])(?=.*[A-ZÅÄÖ]).{" + passwordMinLength + "," + passwordMaxLength + "}$";
            return Regex.IsMatch(password, expression);
        }

        /// <summary>
        /// FI - We need reference number with checksum. Minimum length is 4 characters. 
        /// Valid reference consists of a number which length is 3-19 characters + 1 checksum that is calculated here and added into
        /// reference. Banks are using reference to check validity of previous numbers in reference. 
        /// We copy all the characters, except last one and create reference based on copied value. If it's identical with reference 
        /// passed here, we can say it's a valid one. 
        /// </summary>  
        public static bool ValidateFIBankPaymentReference(string referencenumber)
        {
            string line = Convert.ToString(referencenumber);
            int linelength = line.Length;
            if (linelength < 4)  // First check that it's long enough
            {
                return false;
            }

            // Original ref without checksum
            string baseReference = referencenumber.Substring(0, line.Length - 1);
            line = Convert.ToString(baseReference);
            linelength = line.Length;

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

            string formedFIReference;
            if (summa != 10)
                formedFIReference = line + summa.ToString();
            else
                formedFIReference = line + "0";

            return formedFIReference == referencenumber;
        }
        #endregion

        #region BankAccount

        public static bool IsValidBankNumberSE(int? sysPaymentType, string clearing, string bankAccountNr)
        {
            if (bankAccountNr == "")
                return false;

            bankAccountNr = Regex.Replace(bankAccountNr, "[^0-9]", "");
            clearing = Regex.Replace(clearing, "[^0-9]", "");

            // Sometimes clearing will be 5 digits, only the first four are used.
            int clearingInt = 0;
            if (clearing != "")
            {
                if (clearing.Length > 4)
                    clearing = clearing.Substring(0, 4);

                clearingInt = Convert.ToInt32(clearing);
            }

            // 5 Different type, explain below
            int type = 0;

            if (ExistInInterval(clearingInt, 9660, 9669)) //Amfa Bank AB 	9660-9669
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9550, 9569)) //Avanza Bank AB	9550-9569
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9470, 9479))  //BNP Paribas Fortis Bank 	9470-9479
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9040, 9049)) //Citybank 	1200-1399 
                type = 2;

            else if (type == 0 && (ExistInInterval(clearingInt, 1200, 1399) || ExistInInterval(clearingInt, 2400, 2499)))   //Danske Bank 	1200-1399   	2400-2499 
                type = 1;

            else if (type == 0 && (ExistInInterval(clearingInt, 9190, 9199) || ExistInInterval(clearingInt, 9260, 9269)))   //DNB Bank 	9190-9199        9260-9269
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9590, 9599)) //Erik Penser AB	9590–9599 
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9400, 9449)) //Forex Bank 	9400-9449 
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9460, 9469)) //GE Money Bank	9460-9469
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9270, 9279))  //ICA Banken AB 	9270-9279
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9170, 9179)) //IKANO Bank 	9170-9179
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9390, 9399)) //Landshypotek AB	9390-9399
                type = 2;

            else if (type == 0 && (ExistInInterval(clearingInt, 3400, 3409) || ExistInInterval(clearingInt, 9060, 9069))) //Länsförsäkringar Bank	3400-3409  	9060-9069
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9020, 9029)) //Länsförsäkringar Bank 	9020-9029
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9230, 9239)) //Marginalen Bank 	9230-9239
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9640, 9649)) //Nordax Finans AB 	9640-9649
                type = 2;

            else if (type == 0 && (ExistInInterval(clearingInt, 1100, 1199) || ExistInInterval(clearingInt, 1400, 2099))) //Nordea 	1100-1199  1400-2099
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 4000, 4999)) //Nordea 	4000-4999
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9100, 9109)) //Nordnet Bank	9100-9109
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9280, 9289)) //Resurs Bank	9280-9289
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9880, 9889)) //Riksgälden	9880-9889
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9090, 9099))  //Royal bank of Scotland	9090-9099
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9250, 9259)) //SBAB	9250-9259
                type = 1;

            else if (type == 0 && (ExistInInterval(clearingInt, 5000, 5999) || ExistInInterval(clearingInt, 9120, 9124) || ExistInInterval(clearingInt, 9130, 9149))) //SEB	5000-5999 9120-9124 9130-9149 
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9150, 9169)) //Skandiabanken	9150-9169
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 7000, 7999)) //Swedbank	7000-7999
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 2300, 2399)) //Ålandsbanken Sverige AB	2300-2399
                type = 2;

            else if (type == 0 && ExistInInterval(clearingInt, 9180, 9189)) //Danske Bank 9180-9189
                type = 3;

            else if (type == 0 && ExistInInterval(clearingInt, 6000, 6999)) //Handelsbanken 6000-6999
                type = 4;

            else if (type == 0 && (ExistInInterval(clearingInt, 9500, 9549) || ExistInInterval(clearingInt, 9960, 9969))) //ÅNordea/Plusgirot  9500-9549 9960-9969
                type = 5;

            else if (type == 0 && (clearingInt == 3300 || clearingInt == 3782)) //Nordea - personkonto 3300/3782
                type = 3;

            else if (type == 0 && ExistInInterval(clearingInt, 9890, 9899)) //Riksgälden 9890 -9899
                type = 3;

            else if (type == 0 && (ExistInInterval(clearingInt, 9300, 9329) || ExistInInterval(clearingInt, 9330, 9330))) //Sparbanken Öresund AB(f.d. Sparbanken Finn och Gripen) 9300-9329  9330-9349
                type = 3;

            else if (type == 0 && ExistInInterval(clearingInt, 9570, 9579)) //Sparbanken Syd 9570- 9579
                type = 3;

            else if (type == 0 && ExistInInterval(clearingInt, 8000, 8999)) //Swedbank 8000-8999
                type = 5;

            else if (type == 0 && (ExistInInterval(clearingInt, 3000, 3299) || ExistInInterval(clearingInt, 3301, 3399))) //Nordea 3300-3399 exkl 3300
                type = 1;
            else if (type == 0 && (ExistInInterval(clearingInt, 3410, 3781) || ExistInInterval(clearingInt, 3783, 3999))) //Nordea 3410-3999 exkl 3782
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9680, 9689)) //BlueStep Finans AB
                type = 1;

            else if (type == 0 && ExistInInterval(clearingInt, 9630, 9639)) //Lån & Spar Bank Sverige
                type = 1;


            if (type == 0)
                return false;

            string validatestring, actualBankAccountNr;
            string controldigit = bankAccountNr.Substring(bankAccountNr.Length - 1, 1);
            int modulus = 11;

            // Type 1 and Type 2
            // The account number consists of a total of eleven digits – the clearing number and the
            // actual account number, including a check digit ( C ) according to Modulus-11, using the
            // weights 1, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1. For check sum calculation, please see the Comment.   
            // Type 3,4 and 5
            // The Clearing number is not part of the Bank Account number.
            // Significant digits in the Account Number Field are designating the Account Number.

            if (type == 1) // type 1 = Checksum calculation is made on the Clearing number with the exception of the first digit, and seven digits of the actual account number.
            {
                if (bankAccountNr.Length > 7 && clearing.Length < 5)
                    return false;

                bankAccountNr = AddZeros(bankAccountNr, 7);

                actualBankAccountNr = bankAccountNr.Substring(Math.Max(0, bankAccountNr.Length - 7));
                validatestring = clearing.Substring(1, 3) + actualBankAccountNr;
                return ValidateModulus(modulus, validatestring, controldigit);
            }

            if (type == 2)  // type 2 = Checksum calculation is made on the entire Clearing number, and seven digits of the actual account number.
            {
                if (bankAccountNr.Length > 7 && clearing.Length < 5)
                    return false;

                bankAccountNr = AddZeros(bankAccountNr, 7);

                actualBankAccountNr = bankAccountNr.Substring(Math.Max(0, bankAccountNr.Length - 7));
                validatestring = clearing + actualBankAccountNr;
                return ValidateModulus(modulus, validatestring, controldigit);
            }

            if (type == 3)  // type 3 = Checksum calculation is made on the last 10 digits. Modus 10
            {
                if (bankAccountNr.Length > 10 && clearing.Length < 5)
                    return false;

                bankAccountNr = AddZeros(bankAccountNr, 10);

                actualBankAccountNr = bankAccountNr.Substring(Math.Max(0, bankAccountNr.Length - 10));
                validatestring = actualBankAccountNr;
                modulus = 10;
                return ValidateModulus(modulus, validatestring, controldigit);
            }

            if (type == 4)  // type 4 = The account number consists of 9 digits. Checksum calculation uses the 9 last digits in field 5 using the modulus 11 check
            {
                if (bankAccountNr.Length > 9 && clearing.Length < 5)
                    return false;

                bankAccountNr = AddZeros(bankAccountNr, 9);

                actualBankAccountNr = bankAccountNr.Substring(Math.Max(0, bankAccountNr.Length - 9));
                validatestring = actualBankAccountNr;
                return ValidateModulus(modulus, validatestring, controldigit);
            }

            if (type == 5)  // type 5 = The account number consists of 10 digits. Checksum calculation uses the last ten digits in field 5 using the modulus 10 check. However in rare occasions some of Swedbank’s accounts cannot be validated by a checksum calculation.
            {
                if (bankAccountNr.Length != 10 && clearing.Length < 5)
                    return false;

                bankAccountNr = AddZeros(bankAccountNr, 10);

                actualBankAccountNr = bankAccountNr.Substring(Math.Max(0, bankAccountNr.Length - 10));
                validatestring = actualBankAccountNr;
                modulus = 10;
                return ValidateModulus(modulus, validatestring, controldigit);
            }

            return false;

        }

        public static bool IsValidIBANNumber(string iban)
        {
            if (String.IsNullOrEmpty(iban))
                return false;

            iban = iban.ToUpper();

            if (System.Text.RegularExpressions.Regex.IsMatch(iban, "^[A-Z]{2,2}[0-9]{2,2}[a-zA-Z0-9]{1,30}"))
            {
                iban = iban.Replace(" ", String.Empty);

                string newIBan = iban.Substring(4, iban.Length - 4) + iban.Substring(0, 4);
                int asciiShift = 55;

                StringBuilder sb = new StringBuilder();
                foreach (char c in newIBan)
                {
                    int v;
                    if (Char.IsLetter(c))
                        v = c - asciiShift;
                    else
                        v = int.Parse(c.ToString());
                    sb.Append(v);
                }

                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
                return false;
        }

        private static string AddZeros(string str, int limit)
        {
            string fillchar = "0";

            // Fill zeroes or empties before value
            string mystring = !string.IsNullOrEmpty(str) ? str : string.Empty;
            mystring = mystring.Trim();

            //Remove ,
            mystring = mystring.Replace(",", "");

            for (int i = 0; i < limit; i++)
            {
                if (mystring.Length < limit)
                {
                    mystring = fillchar + mystring;
                }
            }

            return mystring;
        }

        public static bool ValidateModulus(int modulus, string validatestring, string controldigit)
        {
            if (modulus == 10)
            {
                List<int> digits = new List<int>();
                foreach (char c in validatestring.ToLower())
                {
                    digits.Add(int.Parse(String.Format("{0}", c)));
                }

                int digitSum = 0;
                int paritet = digits.Count - 1 % 2;
                for (int index = digits.Count - 2; index >= 0; index--)
                {
                    int digitValue = digits[index];
                    // varannan multipliceras med 2 och varannan med 1...
                    digitValue = digitValue * (((index + paritet) % 2) + 1);
                    if (digitValue > 9)
                    {
                        digitSum += digitValue / 10;
                        digitSum += digitValue % 10;
                    }
                    else
                    {
                        digitSum += digitValue;
                    }
                }

                int checkDigit = (10 - (digitSum % 10)) % 10;

                if (checkDigit == Convert.ToInt32(controldigit))
                    return true;
            }

            if (modulus == 11)
            {
                List<int> digits = new List<int>();
                foreach (char c in validatestring.ToLower())
                {
                    digits.Add(int.Parse(String.Format("{0}", c)));
                }

                int digitSum = 0;
                int paritet = 2;
                for (int index = digits.Count - 2; index >= 0; index--)
                {
                    int digitValue = digits[index];

                    // varannan multipliceras med 2 och varannan med 1...
                    digitValue = digitValue * paritet;
                    digitSum += digitValue;
                    paritet++;

                    if (paritet == 11)
                        paritet = 1;
                }

                int checkDigit = (11 - (digitSum % 11)) % 11;
                if (checkDigit == Convert.ToInt32(controldigit))
                    return true;
            }

            return false;
        }


        #endregion
    }


}
