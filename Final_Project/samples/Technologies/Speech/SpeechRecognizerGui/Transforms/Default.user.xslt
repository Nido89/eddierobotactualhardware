<?xml version="1.0" encoding="utf-8" ?>
<!--
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Default.user.xslt $ $Revision: 2 $
-->
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:srg="http://schemas.microsoft.com/robotics/2008/03/speechrecognizergui.user.html"
  xmlns:sr="http://schemas.microsoft.com/robotics/2008/02/speechrecognizer.user.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        SpeechRecognizer GUI Service
      </xsl:with-param>
      <xsl:with-param name="description">
        User interface for the SpeechRecognizer service
      </xsl:with-param>
      <xsl:with-param name="head">
        <link rel="stylesheet" type="text/css" href="/resources/User.SpeechRecognizerGui.Y2008.M03/Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui.Transforms.Style.css" />
        <script language="javascript" type="text/javascript" src="/resources/User.SpeechRecognizerGui.Y2008.M03/Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui.Transforms.Ajax.js"></script>
        <script language="javascript" type="text/javascript" src="/resources/User.SpeechRecognizerGui.Y2008.M03/Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui.Transforms.Html.js"></script>
        <script language="javascript" type="text/javascript" src="/resources/User.SpeechRecognizerGui.Y2008.M03/Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui.Transforms.Main.js"></script>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="/srg:SpeechRecognizerGuiState">
    <h4>Speech Events</h4>
    <div id="EventLog"></div>
    <div id="ClearEventLogDiv">
      <form>  
        <input type="button" id="ClearEventLog" onclick="onClearEventLogClick()" value="Clear Log" />
      </form>
    </div>

    <h4>Speech Configuration</h4>
    <form name="ConfigForm" id="ConfigForm" target="SrgsFileUploadFrame" method="post" enctype="multipart/form-data">
      <table>
        <tr>
          <th>Grammar Type</th>
          <td>
            <select name="GrammarType" id="GrammarType" onchange="onGrammarTypeChange();">
              <xsl:call-template name="ListOption">
                <xsl:with-param name="value">DictionaryStyle</xsl:with-param>
                <xsl:with-param name="text">Dictionary</xsl:with-param>
                <xsl:with-param name="selected" select="srg:SpeechRecognizerState/sr:GrammarType"></xsl:with-param>
              </xsl:call-template>
              <xsl:call-template name="ListOption">
                <xsl:with-param name="value">Srgs</xsl:with-param>
                <xsl:with-param name="text">SRGS file</xsl:with-param>
                <xsl:with-param name="selected" select="srg:SpeechRecognizerState/sr:GrammarType"></xsl:with-param>
              </xsl:call-template>
            </select>
          </td>
        </tr>
      </table>
      <div id="Config"></div>
    </form>
  </xsl:template>

  <xsl:template name="ListOption">
    <xsl:param name="value"/>
    <xsl:param name="text"/>
    <xsl:param name="selected"/>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="$value"/>
      </xsl:attribute>
      <xsl:if test="$selected = $value">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:value-of select="$text"/>
    </option>
  </xsl:template>
</xsl:stylesheet>
