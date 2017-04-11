<xsl:stylesheet version="1.0"
  xmlns="http://www.w3.org/2005/Atom"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:bk="http://library.by/catalog"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:cs="urn:cs"
  exclude-result-prefixes="bk cs msxsl">
  <xsl:output method="xml" indent="yes"/>
  <msxsl:script language="C#" implements-prefix="cs">
    <![CDATA[
          public string datenow()
          {
             return(DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"));
          }
         ]]>
  </msxsl:script>
  
  <xsl:template match="/">
    <html>
      <head>
        <title>Текущие фонды по жанрам</title>
        <style>
          td, th{
            border: 1px solid black;
          }
        </style>
      </head>
      <body>
        <h2 align="center">
          <xsl:value-of select="cs:datenow()"/>
        </h2>
        <h3>Computer</h3>
        <table>
          <tr>
            <th>Автор</th>
            <th>Название</th>
            <th>Дата издания</th>
            <th>Дата регистрации</th>
          </tr>
          <xsl:for-each select="/bk:catalog/bk:book[bk:genre = 'Computer']">
            <tr>
              <td>
                <xsl:value-of select="bk:author"/>
              </td>
              <td>
                <xsl:value-of select="bk:title"/>
              </td>
              <td>
                <xsl:value-of select="bk:publish_date"/>
              </td>
              <td>
                <xsl:value-of select="bk:registration_date"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <div>
          Всего: <xsl:value-of select="count(/bk:catalog/bk:book[bk:genre = 'Computer'])"/>
        </div>
        <br />
        <h3>Fantasy</h3>
        <table>
          <tr>
            <th>Автор</th>
            <th>Название</th>
            <th>Дата издания</th>
            <th>Дата регистрации</th>
          </tr>
          <xsl:for-each select="/bk:catalog/bk:book[bk:genre = 'Fantasy']">
            <tr>
              <td>
                <xsl:value-of select="bk:author"/>
              </td>
              <td>
                <xsl:value-of select="bk:title"/>
              </td>
              <td>
                <xsl:value-of select="bk:publish_date"/>
              </td>
              <td>
                <xsl:value-of select="bk:registration_date"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <div>
          Всего: <xsl:value-of select="count(/bk:catalog/bk:book[bk:genre = 'Fantasy'])"/>
        </div>
        <br />
        <h3>Horror</h3>
        <table>
          <tr>
            <th>Автор</th>
            <th>Название</th>
            <th>Дата издания</th>
            <th>Дата регистрации</th>
          </tr>
          <xsl:for-each select="/bk:catalog/bk:book[bk:genre = 'Horror']">
            <tr>
              <td>
                <xsl:value-of select="bk:author"/>
              </td>
              <td>
                <xsl:value-of select="bk:title"/>
              </td>
              <td>
                <xsl:value-of select="bk:publish_date"/>
              </td>
              <td>
                <xsl:value-of select="bk:registration_date"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <div>
          Всего: <xsl:value-of select="count(/bk:catalog/bk:book[bk:genre = 'Horror'])"/>
        </div>
        <br />
        <h3>Romance</h3>
        <table>
          <tr>
            <th>Автор</th>
            <th>Название</th>
            <th>Дата издания</th>
            <th>Дата регистрации</th>
          </tr>
          <xsl:for-each select="/bk:catalog/bk:book[bk:genre = 'Romance']">
            <tr>
              <td>
                <xsl:value-of select="bk:author"/>
              </td>
              <td>
                <xsl:value-of select="bk:title"/>
              </td>
              <td>
                <xsl:value-of select="bk:publish_date"/>
              </td>
              <td>
                <xsl:value-of select="bk:registration_date"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <div>
          Всего: <xsl:value-of select="count(/bk:catalog/bk:book[bk:genre = 'Romance'])"/>
        </div>
        <br />
        <h3>Science Fiction</h3>
        <table>
          <tr>
            <th>Автор</th>
            <th>Название</th>
            <th>Дата издания</th>
            <th>Дата регистрации</th>
          </tr>
          <xsl:for-each select="/bk:catalog/bk:book[bk:genre = 'Science Fiction']">
            <tr>
              <td>
                <xsl:value-of select="bk:author"/>
              </td>
              <td>
                <xsl:value-of select="bk:title"/>
              </td>
              <td>
                <xsl:value-of select="bk:publish_date"/>
              </td>
              <td>
                <xsl:value-of select="bk:registration_date"/>
              </td>
            </tr>
          </xsl:for-each>
        </table>
        <div>
          Всего Fiction: <xsl:value-of select="count(/bk:catalog/bk:book[bk:genre = 'Science Fiction'])"/>
        </div>
        <br />
        <br />
        <div>
          Общее количество книг: <xsl:value-of select="count(/bk:catalog/bk:book)"/>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>