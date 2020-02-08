<?xml version="1.0" encoding="UTF-8" ?>
<!-- 
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: TextToSpeech.xslt $ $Revision: 9 $
-->
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:tts="http://schemas.microsoft.com/2006/05/texttospeech.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        Text to Speech (TTS)
      </xsl:with-param>
      <xsl:with-param name="description">
        Converts text to speech using the .NET speech synthesis framework.
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="/tts:TextToSpeechState">
    <table>
      <tr class="even">
        <td>Voice</td>
        <td>
          <form method="post" style="display:inline;">
            <xsl:variable name="voice" select="tts:Voice"/>
            <select name="Voice">
              <xsl:for-each select="tts:Voices/tts:string">
                <option value="{text()}">
                  <xsl:if test="text() = $voice">
                    <xsl:attribute name="selected">true</xsl:attribute>
                  </xsl:if>
                  <xsl:value-of select="text()"/>
                </option>
              </xsl:for-each>
            </select>
            <xsl:text> </xsl:text>
            <input type="submit" name="VoiceSubmit" value="Set"/>
          </form>
        </td>
      </tr>
      <tr class="odd">
        <td>Rate</td>
        <td>
          <form method="post" style="display:inline;">
            <input type="text" size="4" name="Rate" value="{tts:Rate}" title="Speech Rate, between -10 and 10" />
            <xsl:text> </xsl:text>
            <input type="submit" name="RateSubmit" value="Set"/>
          </form>
        </td>
      </tr>
      <tr class="even">
        <td>Volume</td>
        <td>
          <form method="post" style="display:inline;">
            <input type="text" size="4" name="Volume" value="{tts:Volume}" title="Speech Volume, between 0 and 100" />
            <xsl:text> </xsl:text>
            <input type="submit" name="VolumeSubmit" value="Set"/>
          </form>
        </td>
      </tr>
      <tr class="odd">
        <td>Speech Text</td>
        <td>
          <form method="post" style="display:inline;">
            <input type="text" name="SpeechText" value="{tts:SpeechText}" title="Text to say" />
            <xsl:text> </xsl:text>
            <input type="submit" name="SpeechTextSubmit" value="Say"/>
          </form>
        </td>
      </tr>
    </table>
  </xsl:template>

</xsl:stylesheet>
