<?xml version="1.0" encoding="UTF-8" ?>
<!--
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: ServiceTutorial7.user.xslt $ $Revision: 4 $
-->
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:rst7="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html">

  <xsl:output method="html"/>

  <xsl:template match="/rst7:ServiceTutorial7State">
    <html>
      <head>
        <title>Service Tutorial 7</title>
        <link rel="stylesheet" type="text/css" href="/resources/dss/Microsoft.Dss.Runtime.Home.Styles.Common.css" />
      </head>
      <body>
        <h1>Service Tutorial 7</h1>
        <table>
          <tr>
            <th colspan="3">Clocks</th>
          </tr>
          <xsl:apply-templates select="rst7:Clocks/rst7:string"/>
          <tr>
            <th colspan="3">Counts</th>
          </tr>
          <xsl:apply-templates select="rst7:TickCounts/rst7:TickCount"/>
        </table>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="rst7:Clocks/rst7:string">
    <tr>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="position() mod 2 = 0">even</xsl:when>
          <xsl:otherwise>odd</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <th>
        <xsl:value-of select="position()"/>
      </th>
      <td colspan="2">
        <xsl:value-of select="text()"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="rst7:TickCounts/rst7:TickCount">
    <tr>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="position() mod 2 = 0">even</xsl:when>
          <xsl:otherwise>odd</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <th>
        <xsl:value-of select="position()"/>
      </th>
      <td>
        <xsl:value-of select="rst7:Name"/>
      </td>
      <td>
        <xsl:value-of select="rst7:Count"/>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>