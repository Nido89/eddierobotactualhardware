<?xml version="1.0" encoding="UTF-8" ?>
<!--
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: ServiceTutorial6.user.xslt $ $Revision: 4 $
-->
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:rst6="http://schemas.microsoft.com/2006/06/servicetutorial6.user.html">

  <xsl:output method="html"/>

  <xsl:template match="/rst6:ServiceTutorial6State">
    <html>
      <head>
        <title>Service Tutorial 6</title>
        <link rel="stylesheet" type="text/css" href="/resources/dss/Microsoft.Dss.Runtime.Home.Styles.Common.css" />
      </head>
      <body style="margin:10px">
        <h1>Service Tutorial 6</h1>
        <table border="1">
          <tr class="odd">
            <th colspan="2">Service State</th>
          </tr>
          <tr class="even">
            <th>Clock:</th>
            <td>
              <xsl:value-of select="rst6:Clock"/>
            </td>
          </tr>
          <tr class="odd">
            <th>Initial Tick Count:</th>
            <td>
              <xsl:value-of select="rst6:InitialTicks"/>
            </td>
          </tr>
          <tr class="even">
            <th>Current Tick Count:</th>
            <td>
              <xsl:value-of select="rst6:TickCount"/>
            </td>
          </tr>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>