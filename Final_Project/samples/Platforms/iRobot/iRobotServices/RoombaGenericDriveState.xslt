<?xml version="1.0" encoding="utf-8"?>
<!-- 
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: RoombaGenericDriveState.xslt $ $Revision: 6 $
-->
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:s="http://www.w3.org/2003/05/soap-envelope"
    xmlns:genericdifferentialdrive="http://schemas.microsoft.com/robotics/2006/05/drive.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        Roomba Differential Drive
      </xsl:with-param>
      <xsl:with-param name="description">
        View the Roomba Differential Drive State
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="s:Header">

  </xsl:template>

  <xsl:template match="genericdifferentialdrive:DriveDifferentialTwoWheelState">
    <table width="100%">
      <tr>
        <th width="20%">Is Enabled:</th>
        <td width="80%">
          <xsl:value-of select="genericdifferentialdrive:IsEnabled"/>
        </td>
      </tr>
      <tr class="odd">
        <th>Distance Between Wheels:</th>
        <td>
          <xsl:value-of select="genericdifferentialdrive:DistanceBetweenWheels"/>
        </td>
      </tr>
    </table>
  </xsl:template>
</xsl:stylesheet>
