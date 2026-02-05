<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <html>
      <head>
        <title>
          <xsl:value-of select="/rss/channel/title" />
        </title>
      </head>
      <body>
        <table width="75%" border="1" cellspacing="1" cellpadding="1">
          <tr>
            <td bgcolor="#cccccc">
              <xsl:value-of select="/rss/channel/title" />
            </td>
          </tr>
        </table>
        <table width="75%" border="0" cellspacing="0" cellpadding="0">
          <tr>
            <td height="15"></td>
          </tr>
            <xsl:for-each select="/rss/channel/item">
              <tr>
                <td>
                  <a href="{link}">
                    <xsl:value-of select="title" />
                  </a>
                </td>
              </tr>
              <tr>
                <td height="5">
                  <xsl:value-of select="description" />
                </td>
              </tr>
              <tr>
                <td height="10">

                </td>
              </tr>
            </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
