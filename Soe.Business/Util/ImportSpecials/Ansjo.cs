using System;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.ImportSpecials
{
    public class Ansjo
    {
        public string ApplyAnsjoSupplierInvoiceSpecialModification(string content)
        {
            string modifiedContent = string.Empty;
            char[] delimiter = new char[1];
            delimiter[0] = ';';

            byte[] byteArray = Encoding.Default.GetBytes(content);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.Default);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                string[] inputRow = line.Split(delimiter);

                modifiedContent += "H;" + line + Environment.NewLine;

                //Amount        //accountnr     //dim2,dim3,debit
                modifiedContent += "R;" + inputRow[1] + ";" + inputRow[2] + ";;;True;" + Environment.NewLine;
                modifiedContent += "R;" + inputRow[3] + ";" + inputRow[4] + ";" + inputRow[22] + ";" + inputRow[21] + ";True" + Environment.NewLine;
                modifiedContent += "R;" + inputRow[5] + ";" + inputRow[6] + ";" + inputRow[22] + ";" + inputRow[21] + ";True" + Environment.NewLine;

                decimal amount = 0;
                decimal temp1 = 0;
                decimal temp2 = 0;
                decimal temp3 = 0;

                decimal.TryParse(inputRow[1], out temp1);
                decimal.TryParse(inputRow[3], out temp2);
                decimal.TryParse(inputRow[5], out temp3);
                amount = temp1 + temp2 + temp3;

                modifiedContent += "R;" + amount + ";" + "2440" + ";;;False;" + Environment.NewLine;
            }

            return modifiedContent;
        }

    }
}
