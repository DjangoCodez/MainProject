using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SoftOne.Soe.Business.Util
{
    public class ProjectIOItem
    {

        #region Collections

        public List<ProjectIODTO> Projects = new List<ProjectIODTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "ProjectIO";
        public const string XML_PROJECT_TAG = "ProjectIO";
        #endregion

        public ProjectIOItem()
        {
        }

        public ProjectIOItem(List<string> contents)
        {
            this.CreateObjects(contents);
        }

        #region Parse

        private void CreateObjects(List<string> contents)
        {
            foreach (var content in contents)
            {
                CreateObjects(content);
            }
        }


        private void CreateObjects(string content)
        {
            var xdoc = XDocument.Parse(content);
            List<XElement> ProjectIOElements = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            PropertyInfo[] properties = typeof(ProjectIODTO).GetProperties()
            .Where(x => Attribute.IsDefined(x, typeof(XmlElementAttribute), false)).ToArray();

            foreach (var element in ProjectIOElements)
	        {
                var item = new ProjectIODTO();
                foreach (var node in element.Elements())
                {
                    var prop = properties.FirstOrDefault(f => f.Name == node.Name.LocalName);
                    if (prop != null)
                    {
                        SetObjectValue(item, element, prop, node.Name.LocalName);
                    }
                }
                Projects.Add(item);
	        }
        }

        public static void SetObjectValue(object objectWithProperty, XElement element, PropertyInfo field, string name)
        {
            if (field.PropertyType == typeof(string))
                field.SetValue(objectWithProperty, XmlUtil.GetChildElementValue(element, name));
            else if (field.PropertyType == typeof(int))
                field.SetValue(objectWithProperty, XmlUtil.GetElementIntValue(element, name));
            else if (field.PropertyType == typeof(int?))
                field.SetValue(objectWithProperty, XmlUtil.GetElementNullableIntValue(element, name));
            else if (field.PropertyType == typeof(DateTime))
                field.SetValue(objectWithProperty, CalendarUtility.GetDateTime(XmlUtil.GetChildElementValue(element, name)));
            else if (field.PropertyType == typeof(DateTime?))
                field.SetValue(objectWithProperty, CalendarUtility.GetDateTime(XmlUtil.GetElementNullableValue(element, name)));
            else if (field.PropertyType == typeof(decimal))
                field.SetValue(objectWithProperty, XmlUtil.GetElementDecimalValue(element, name));
            else if (field.PropertyType == typeof(decimal?))
                field.SetValue(objectWithProperty, XmlUtil.GetElementNullableDecimalValue(element, name));
            else
                field.SetValue(objectWithProperty, XmlUtil.GetChildElementValue(element, name));
        }

        #endregion
    }
}
