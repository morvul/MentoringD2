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
             return(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
          }
         ]]>
  </msxsl:script>
  <xsl:template match="/bk:catalog">
    <feed>
      <title>New arrivals</title>
      <subtitle>List of our new books</subtitle>
      <link href="http://library.by/catalog"/>
      <updated>
        <xsl:value-of select="cs:datenow()"/>
      </updated>
      <xsl:apply-templates />
    </feed>
  </xsl:template>

  <xsl:template match="/bk:catalog/bk:book">
    <entry>
      <title>
        <xsl:value-of select="bk:title"/>
      </title>
      <author>
        <xsl:value-of select="bk:author"/>
      </author>
      <updated>
        <xsl:value-of select="bk:registration_date"/>
      </updated>
      <summary>
         <xsl:value-of select="bk:description"/>
      </summary>
      <xsl:apply-templates />
    </entry>
  </xsl:template>

  <xsl:template match="/bk:catalog/bk:book/bk:isbn">
    <xsl:element name="link">
      <xsl:text>http://my.safaribooksonline.com/</xsl:text>
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <xsl:template match="text() | @* "/>

</xsl:stylesheet>
