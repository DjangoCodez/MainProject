using System;
using System.IO;
using System.Diagnostics;

namespace SoftOne.EdiAdmin.Business.Util
{
    public class EdiAdminStreamWriter : TextWriter
    {
        internal event EventHandler<EdiAdminStreamWriterEventArgs> Output;
        private bool sendToConsole;
        private string text;
        private EventLogEntryType defaultEntryType;

        public EdiAdminStreamWriter(EventLogEntryType defaultEntryType, bool sendToConsole = false)
        {
            this.defaultEntryType = defaultEntryType;
            this.sendToConsole = sendToConsole;
        }

        //return default encoding
        public override System.Text.Encoding Encoding { get { return System.Text.Encoding.Default; } }

        /// <summary>
        /// All Write calls ends up here
        /// </summary>
        /// <param name="value"></param>
        /// <param name="entryType"></param>
        public void Write(string value, EventLogEntryType entryType)
        {
            this.text = string.Concat(this.text, value);
            if (value.EndsWith(this.NewLine))
            {
                InvokeOutput(entryType);
                this.text = string.Empty;
            }
        }

        //write a string
        public override void Write(string value)
        {
            this.Write(value, defaultEntryType);
        }        

        //write a string + new line
        public override void WriteLine(string value)
        {
            Write(value + this.NewLine);
        }

        public void WriteLine(string value, EventLogEntryType entryType)
        {
            Write(value + Environment.NewLine, entryType);
        }

        //overwrite all other write methods
        public override void Write(bool value) { this.Write(value.ToString()); }
        public override void Write(char value) { this.Write(value.ToString()); }
        public override void Write(char[] buffer) { this.Write(new string(buffer)); }
        public override void Write(char[] buffer, int index, int count) { this.Write(new string(buffer, index, count)); }
        public override void Write(decimal value) { this.Write(value.ToString()); }
        public override void Write(double value) { this.Write(value.ToString()); }
        public override void Write(float value) { this.Write(value.ToString()); }
        public override void Write(int value) { this.Write(value.ToString()); }
        public override void Write(long value) { this.Write(value.ToString()); }
        public override void Write(string format, object arg0) { this.WriteLine(string.Format(format, arg0)); }
        public override void Write(string format, object arg0, object arg1) { this.WriteLine(string.Format(format, arg0, arg1)); }
        public override void Write(string format, object arg0, object arg1, object arg2) { this.WriteLine(string.Format(format, arg0, arg1, arg2)); }
        public override void Write(string format, params object[] arg) { this.WriteLine(string.Format(format, arg)); }
        public override void Write(uint value) { this.WriteLine(value.ToString()); }
        public override void Write(ulong value) { this.WriteLine(value.ToString()); }
        public override void Write(object value) { this.WriteLine(value.ToString()); }
        public override void WriteLine() { this.Write(Environment.NewLine); }
        public override void WriteLine(bool value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(char value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(char[] buffer) { this.WriteLine(new string(buffer)); }
        public override void WriteLine(char[] buffer, int index, int count) { this.WriteLine(new string(buffer, index, count)); }
        public override void WriteLine(decimal value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(double value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(float value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(int value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(long value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(string format, object arg0) { this.WriteLine(string.Format(format, arg0)); }
        public override void WriteLine(string format, object arg0, object arg1) { this.WriteLine(string.Format(format, arg0, arg1)); }
        public override void WriteLine(string format, object arg0, object arg1, object arg2) { this.WriteLine(string.Format(format, arg0, arg1, arg2)); }
        public override void WriteLine(string format, params object[] arg) { this.WriteLine(string.Format(format, arg)); }
        public override void WriteLine(uint value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(ulong value) { this.WriteLine(value.ToString()); }
        public override void WriteLine(object value) { this.WriteLine(value.ToString()); }
        
        private void InvokeOutput(EventLogEntryType entryType)
        {
            if (Output != null)
                Output(this, new EdiAdminStreamWriterEventArgs(entryType, this.text));
            else if (sendToConsole)
                Console.Out.WriteLine(this.text);
        }
    }
}