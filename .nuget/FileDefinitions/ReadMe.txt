The .cs classes in this folder are created with the tool xsd.exe that creates cs classes from xsd defintion files.
The tool is avalible from VS Developer command prompt and is used as follows:
c:\SoftOne\XE\Main\NewSource\EDIAdmin.Business\FileDefinitions>xsd /c /n:SoftOne.Soe.EdiAdmin.Business.FileDefinitions Message.xsd
XSD tool should be in the path:
"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\xsd.exe" /c /n:SoftOne.Soe.EdiAdmin.Business.FileDefinitions Message.xsd

Whenever the standard template (StandardMall.xml) is updated the Message.xsd file should also be updated via xsd.exe.