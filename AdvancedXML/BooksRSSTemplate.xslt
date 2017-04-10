<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:bk="http://library.by/catalog"
  xmlns:atom="http://www.w3.org/2005/Atom"
  exclude-result-prefixes="bk">
  <xsl:output method="xml" indent="yes" cdata-section-elements="atom:content"/>
  <xsl:template match="/bk:catalog">
    <xsl:element name="feed">
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <xsl:template match="/bk:catalog/bk:book">
    <xsl:element name="news">
      <xsl:element name="date">
        <xsl:value-of select="bk:registration_date"/>
        <xsl:apply-templates />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="/bk:catalog/bk:book/bk:isbn">
    <xsl:element name="ref">
      <xsl:text>http://my.safaribooksonline.com/</xsl:text>
      <xsl:apply-templates />
    </xsl:element>
  </xsl:template>

  <xsl:template match="text() | @* "/>

</xsl:stylesheet>
