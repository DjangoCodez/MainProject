using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SoftOne.Soe.Common.Util
{
    public static class XmlUtil
    {
        #region XDocument

        public static XDocument GetXDocument(string xml)
        {
            return XDocument.Parse(xml);
        }

        public static XDocument CreateDocument()
        {
            return CreateDocument(Encoding.Unicode);
        }

        public static XDocument CreateDocument(String encoding)
        {
            return CreateDocument(Encoding.Unicode, declarationOnly: true);
        }

        public static XDocument CreateDocument(Encoding encoding, bool declarationOnly = false, bool addTargetData = true)
        {
            if (declarationOnly)
            {
                return new XDocument(
                    new XDeclaration("1.0", encoding.ToString(), "true"));
            }
            else
            {
                if (addTargetData)
                    return new XDocument(
                        new XDeclaration("1.0", encoding.ToString(), "true"),
                        new XProcessingInstruction("target", "data"));
                else
                    return new XDocument(
                        new XDeclaration("1.0", encoding.ToString(), "true"));
            }
        }

        public static XDocument CreateDocument(string rootName, List<XElement> elements)
        {
            XDocument document = XmlUtil.CreateDocument();
            XElement root = new XElement(rootName);
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    root.Add(element);
                }
            }
            document.Add(root);
            return document;
        }

        public static XDocument CreateDocument(string element, string value)
        {
            XDocument document = XmlUtil.CreateDocument();
            document.Add(new XElement(element, value));
            return document;
        }

        #endregion

        #region XElement

        public static List<XElement> GetChildElements(XDocument xdoc, string elementName)
        {
            return xdoc?.Root?.Elements()?.Where(e => e.Name.LocalName == elementName).ToList();
        }

        public static List<XElement> GetChildElements(XElement parentElement, string name)
        {
            return parentElement?.Elements()?.Where(e => e.Name.LocalName == name).ToList();
        }

        public static XElement GetRootElement(XDocument xdoc, string elementName = null)
        {
            if (xdoc?.Root != null && (elementName.IsNullOrEmpty() || xdoc.Root.Name == elementName))
                return xdoc.Root;
            return null;
        }

        public static XElement GetChildElement(XDocument xdoc, string elementName)
        {
            return xdoc?.Root?.Elements()?.FirstOrDefault(e => e.Name.LocalName == elementName);
        }

        public static XElement GetChildElement(XElement parentElement, string name, bool mustHaveAttributes = false)
        {
            return parentElement?.Elements()?.FirstOrDefault(e => e.Name.LocalName == name && (!mustHaveAttributes || e.HasAttributes));
        }

        public static XElement GetChildElementLowerCase(XElement parentElement, string name, bool mustHaveAttributes = false)
        {
            return parentElement?.Elements()?.FirstOrDefault(e => e.Name.LocalName.ToLower() == name.ToLower() && (!mustHaveAttributes || e.HasAttributes));
        }

        public static XElement GetDescendantElement(XElement parentElement, string name, bool mustHaveAttributes = false)
        {
            return parentElement?.Descendants()?.FirstOrDefault(e => e.Name.LocalName == name && (!mustHaveAttributes || e.HasAttributes));
        }

        public static XElement GetDescendantElement(XDocument xdoc, string rootElementName, string elementName)
        {
            return GetDescendantElement(GetChildElement(xdoc, rootElementName), elementName);
        }

        public static XElement GetDescendantElementWithChildElements(List<XElement> elements, string name)
        {
            return elements?.FirstOrDefault(e => e.Name.LocalName == name && e.HasElements);
        }

        public static List<XElement> GetDescendantElements(XDocument xdoc, string rootElementName, string elementName)
        {
            List<XElement> elements = new List<XElement>();
            List<XElement> parents = GetChildElements(xdoc, rootElementName);
            foreach (XElement parent in parents)
            {
                elements.AddRange(GetChildElements(parent, elementName));
            }
            return elements;
        }

        public static string GetDescendantElementValue(XElement parentElement, string rootElementName, string elementName)
        {
            XElement rootElement = GetChildElement(parentElement, rootElementName);
            return GetDescendantElement(rootElement, elementName)?.Value ?? string.Empty;
        }

        public static string GetChildElementValue(XDocument xdoc, string name)
        {
            return GetChildElement(xdoc, name)?.Value ?? string.Empty;
        }

        public static string GetChildElementValue(XElement parentElement, string name, bool mustHaveAttributes = false)
        {
            return GetChildElement(parentElement, name, mustHaveAttributes)?.Value ?? string.Empty;
        }

        public static string GetChildElementValue(XElement parentElement, string name, string elementName)
        {
            return GetChildElementValue(XmlUtil.GetChildElement(parentElement, name), elementName);
        }

        public static int GetChildElementValueId(XElement parentElement, string name, string elementName)
        {
            return GetChildElementValueId(GetChildElement(parentElement, name), elementName);
        }

        public static int GetChildElementValueId(XElement element, string name)
        {
            Int32.TryParse(GetChildElementValue(element, name), out int result);
            return result;
        }

        public static decimal GetChildElementValueDecimal(XElement parentElement, string name, string elementName)
        {
            return GetChildElementValueDecimal(GetChildElement(parentElement, name), elementName);
        }

        public static decimal GetChildElementValueDecimal(XElement element, string name)
        {
            Decimal.TryParse(GetChildElementValue(element, name), out decimal result);
            return result;
        }

        public static int GetChildElementValueInt(XElement element, string name, int defaultValue)
        {
            if (int.TryParse(GetChildElementValue(element, name), out int result))
                return result;
            return defaultValue;
        }

        public static List<string> GetChildElementValues(List<XElement> elements, string childElementName)
        {
            if (elements.IsNullOrEmpty())
                return new List<string>();

            List<string> childElementValues = new List<string>();
            foreach (XElement element in elements)
            {
                string childElementValue = XmlUtil.GetChildElementValue(element, childElementName);
                if (!String.IsNullOrEmpty(childElementValue))
                    childElementValues.Add(childElementValue);
            }
            return childElementValues;
        }

        public static string GetElementValueLowerCase(XElement parentElement, string elementName)
        {
            return GetChildElementLowerCase(parentElement, elementName)?.Value ?? string.Empty;
        }

        public static string GetElementNullableValue(XElement parentElement, string elementName)
        {
            return GetChildElementValue(parentElement, elementName).EmptyToNull();
        }

        public static int GetNrOfRootElements(XDocument xdoc, string elementName)
        {
            return xdoc?.Root?.Elements()?.Count(e => e.Name.LocalName == elementName) ?? 0;
        }

        public static int GetElementIntValue(XElement parentElement, string elementName)
        {
            string valueAsString = GetChildElementValue(parentElement, elementName);
            if (string.IsNullOrEmpty(valueAsString))
                return 0;
            if (!int.TryParse(valueAsString, out int valueAsInt))
                return 0;
            return valueAsInt;
        }

        public static int? GetElementNullableIntValue(XElement parentElement, string elementName)
        {
            string valueAsString = GetChildElementValue(parentElement, elementName);
            if (String.IsNullOrEmpty(valueAsString))
                return null;
            if (!int.TryParse(valueAsString, out int valueAsInt))
                return null;
            return valueAsInt;
        }

        public static DateTime GetAttributeDateTimeValue(XElement parentElement, string elementName, string format)
        {
            DateTime value = CalendarUtility.DATETIME_DEFAULT;
            if (parentElement != null && parentElement.HasAttributes && parentElement.Attribute(elementName) != null)
            {
                string attrValue = parentElement.Attribute(elementName).Value;
                value = DateTime.ParseExact(attrValue, format, CultureInfo.InvariantCulture);
            }                
            return value;
        }

        public static int GetAttributeIntValue(XElement parentElement, string elementName)
        {
            int value = 0;
            if (parentElement != null && parentElement.HasAttributes && parentElement.Attribute(elementName) != null)
                value = Convert.ToInt32(parentElement.Attribute(elementName).Value);
            return value;
        }

        public static string GetAttributeStringValue(XElement parentElement, string elementName)
        {
            string value = "";
            if (parentElement != null && parentElement.HasAttributes && parentElement.Attribute(elementName) != null)
                value = parentElement.Attribute(elementName).Value;
            return value;
        }

        public static decimal GetElementDecimalValue(XElement parentElement, string elementName)
        {
            string valueAsString = GetChildElementValue(parentElement, elementName);
            if (String.IsNullOrEmpty(valueAsString))
                return 0;

            //Dont replace for english
            if (!(CultureInfo.CurrentCulture?.IsEnglish() == true))
            {
                valueAsString = valueAsString.Replace(".", ",");
            }

            if (!decimal.TryParse(valueAsString, out decimal valueAsDecimal))
                return 0;
            return valueAsDecimal;
        }

        public static decimal? GetElementNullableDecimalValue(XElement parentElement, string elementName)
        {
            string valueAsString = GetChildElementValue(parentElement, elementName);
            if (String.IsNullOrEmpty(valueAsString))
                return null;

            //Dont replace for english
            if (!(CultureInfo.CurrentCulture?.IsEnglish() == true))
            {
                valueAsString = valueAsString.Replace(".", ",");
            }


            if (!decimal.TryParse(valueAsString, out decimal valueAsDecimal))
                return null;
            return valueAsDecimal;
        }

        public static bool GetElementBoolValue(XElement parentElement, string elementName)
        {
            string valueAsString = GetChildElementValue(parentElement, elementName);
            if (String.IsNullOrEmpty(valueAsString))
                return false;

            if (bool.TryParse(valueAsString, out bool valueAsbool))
                return valueAsbool;
            else if (valueAsString == "1")
                return true;
            else if (valueAsString == "0")
                return false;
            else
                return false;
        }

        public static bool? GetElementNullableBoolValue(XElement parentElement, string elementName)
        {
            string valueAsString = GetChildElementValue(parentElement, elementName);
            if (String.IsNullOrEmpty(valueAsString))
                return null;

            if (bool.TryParse(valueAsString, out bool valueAsbool))
                return valueAsbool;
            else if (valueAsString == "1")
                return true;
            else if (valueAsString == "0")
                return false;
            else
                return null;
        }

        public static DateTime? GetElementNullableDateTimeValue(XElement parentElement, string elementName)
        {
            string value = GetElementNullableValue(parentElement, elementName);
            if (string.IsNullOrEmpty(value))
                return null;

            if (!DateTime.TryParse(value, out DateTime date) && value.Length == 6)
                DateTime.TryParse("20" + value, out date);
            return date;
        }

        public static DateTime GetElementDateTimeValue(XElement parentElement, string elementName)
        {
            string value = GetElementNullableValue(parentElement, elementName);
            if (value != null && DateTime.TryParse(value, out DateTime date))
                return date;
            return CalendarUtility.DATETIME_DEFAULT;
        }

        public static XDocument ReplaceElement(XDocument doc, string elementName, XElement newElement)
        {
            XElement element = GetDescendantElement(doc, elementName, newElement.Name.ToString());
            if (element != null)
                element.ReplaceWith(newElement);
            return doc;
        }

        #endregion

        #region String

        public static string CreateXml(string rootName, string xml)
        {
            return
                String.Format("<{0}>", rootName) +
                    xml +
                String.Format("</{0}>", rootName);
        }

        public static string FormatXml(string xml)
        {
            if (xml != null && !xml.StartsWith("<"))
            {
                int startIndex = xml.IndexOf('<');
                int length = xml.Length;
                xml = xml.Substring(startIndex, length - startIndex);
            }
            return xml;
        }

        public static string PrettyXml(string xml)
        {
            try
            {
                var stringBuilder = new StringBuilder();
                var element = XElement.Parse(xml);
                var settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Indent = true,
                    NewLineOnAttributes = true,
                };
                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    element.Save(xmlWriter);
                }
                return stringBuilder.ToString();
            }
            catch (Exception)
            {
                return xml;
            }
        }

        public static Object XMLToObject(string XMLString, Object oObject)
        {
            XmlSerializer oXmlSerializer = new XmlSerializer(oObject.GetType());
            oObject = oXmlSerializer.Deserialize(new StringReader(XMLString));
            return oObject;
        }

        #endregion
    }
}
