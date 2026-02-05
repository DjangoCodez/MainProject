using Soe.Edi.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util.IO
{
    public class EdiMessageItem
    {

        #region Collections

        public List<SysEdiMessageHeadDTO> sysEdiMessageHeadDTOs = new List<SysEdiMessageHeadDTO>();

        #endregion

        #region XML Nodes

        public const string XML_PARENT_TAG = "EdiMessageDTO";

        #endregion

        #region Constructors

        public EdiMessageItem()
        {
        }

        public EdiMessageItem(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(contents, headType, actorCompanyId);
        }

        public EdiMessageItem(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            CreateObjects(content, headType, actorCompanyId);
        }


        #endregion

        #region Parse

        private void CreateObjects(List<string> contents, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var content in contents)
            {
                CreateObjects(content, headType, actorCompanyId);
            }
        }

        private void CreateObjects(string content, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            XDocument xdoc = XDocument.Parse(content);

            List<XElement> elementTextBlocks = XmlUtil.GetChildElements(xdoc, XML_PARENT_TAG);
            CreateObjects(elementTextBlocks, headType, actorCompanyId);
        }

        public void CreateObjects(List<XElement> elementTextBlocks, TermGroup_IOImportHeadType headType, int actorCompanyId)
        {
            foreach (var elementTextBlock in elementTextBlocks)
            {
                SysEdiMessageHeadDTO ediMessageDTO = new SysEdiMessageHeadDTO();
                sysEdiMessageHeadDTOs.Add(ediMessageDTO);
            }
        }
        #endregion
    }
}