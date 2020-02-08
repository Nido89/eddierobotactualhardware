<?xml version="1.0" encoding="utf-8"?>
<!-- 
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: GamePad.xslt $ $Revision: 5 $
-->
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:s="http://www.w3.org/2003/05/soap-envelope"
    xmlns:xinputgamepad="http://schemas.microsoft.com/robotics/2006/09/xinputgamepad.html" >

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        XInput Controller
      </xsl:with-param>
      <xsl:with-param name="description">
        Provides access to a Xbox 360 controller such as a gamepad.
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="s:Header">

  </xsl:template>

  <xsl:template match="xinputgamepad:XInputGamepadState">
    <div id="silverlightControlHost" style="display:none">
      <object data="data:application/x-silverlight," type="application/x-silverlight-2" width="800" height="600">
        <param name="source" value="/mountpoint/bin/XBoxCtrlViewer.xap"/>
        <param name="background" value="white" />
        <a href="http://go.microsoft.com/fwlink/?LinkID=115261" style="text-decoration: none;">
          <img src="http://go.microsoft.com/fwlink/?LinkId=108181" alt="Get Microsoft Silverlight" style="border-style: none"/>
        </a>
      </object>
      <iframe style='visibility:hidden;height:0;width:0;border:0px'></iframe>
    </div>
    <table id="htmlXboxTable" width="100%" border="0" cellpadding="5" cellspacing="5">
      <tr>
        <td>
          <table width="100%" border="0" cellpadding="5" cellspacing="5">
            <tr>
              <th>
                Conected:
              </th>
              <td>
                <xsl:choose>
                  <xsl:when test='xinputgamepad:Controller/xinputgamepad:IsConnected = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Controller/xinputgamepad:IsConnected"/>
              </td>
            </tr>
            <tr>
              <th width="40%">
                Controller #:
              </th>
              <td width="60%">
                <xsl:value-of select="xinputgamepad:Controller/xinputgamepad:PlayerIndex"/>
              </td>
            </tr>
          </table>
        </td>
      </tr>
      <tr>
        <xsl:if test="position() mod 2 = 1">
          <xsl:attribute name="class">
            <xsl:text disable-output-escaping="yes">odd</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <th width="50%">Buttons / Triggers</th>
        <th width="50%">Directional Pad</th>
      </tr>
      <tr>
        <xsl:if test="position() mod 2 = 1">
          <xsl:attribute name="class">
            <xsl:text disable-output-escaping="yes">odd</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <td width="50%"  valign="top">
          <table width="100%" border="0" cellpadding="5" cellspacing="5">
            <tr>
              <th>A:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:A = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:A"/>
              </td>
            </tr>
            <tr>
              <th>B:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:B = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:B"/>
              </td>
            </tr>
            <tr>
              <th>X:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:X = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:X"/>
              </td>
            </tr>
            <tr>
              <th>Y:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:Y = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:Y"/>
              </td>
            </tr>
            <tr>
              <th width="40%">Left Trigger:</th>
              <td width="60%">
                <xsl:value-of select="xinputgamepad:Triggers/xinputgamepad:Left"/>
              </td>
            </tr>
            <tr>
              <th>Right Trigger:</th>
              <td>
                <xsl:value-of select="xinputgamepad:Triggers/xinputgamepad:Right"/>
              </td>
            </tr>
            <tr>
              <th>Start:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:Start = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:Start"/>
              </td>
            </tr>
            <tr>
              <th>Left Stick:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:LeftStick = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:LeftStick"/>
              </td>
            </tr>
            <tr>
              <th>Right Stick:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:RightStick = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:RightStick"/>
              </td>
            </tr>
            <tr>
              <th>Left Shoulder:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:LeftShoulder = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:LeftShoulder"/>
              </td>
            </tr>
            <tr>
              <th>Right Shoulder:</th>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Buttons/xinputgamepad:RightShoulder = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Buttons/xinputgamepad:RightShoulder"/>
              </td>
            </tr>
          </table>
        </td>
        <td width="50%"  valign="top">
          <table width="100%" border="0" cellpadding="5" cellspacing="5">
            <tr>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when test='xinputgamepad:DPad/xinputgamepad:Up = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:DPad/xinputgamepad:Up"/>
              </td>
              <td></td>
            </tr>
            <tr>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:DPad/xinputgamepad:Left = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:DPad/xinputgamepad:Left"/>
              </td>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:DPad/xinputgamepad:Right = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:DPad/xinputgamepad:Right"/>
              </td>
            </tr>
            <tr>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:DPad/xinputgamepad:Down = "true"'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:DPad/xinputgamepad:Down"/>
              </td>
              <td></td>
            </tr>
          </table>
        </td>
      </tr>
      <tr class="odd">
        <xsl:if test="position() mod 2 = 1">
          <xsl:attribute name="class">
            <xsl:text disable-output-escaping="yes">odd</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <th width="50%">Left Thumbstick</th>
        <th width="50%">Right Thumbstick</th>
      </tr>
      <tr>
        <xsl:if test="position() mod 2 = 1">
          <xsl:attribute name="class">
            <xsl:text disable-output-escaping="yes">odd</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <td width="50%" valign="top">
          <table width="100%" border="0" cellpadding="5" cellspacing="5">
            <tr>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:LeftY &gt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:LeftY"/>
              </td>
              <td></td>
            </tr>
            <tr>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:LeftX &lt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:LeftX"/>
              </td>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:LeftX &gt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:LeftX"/>
              </td>
            </tr>
            <tr>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:LeftY &lt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:LeftY"/>
              </td>
              <td></td>
            </tr>
          </table>
        </td>
        <td width="50%" valign="top">
          <table width="100%" border="0" cellpadding="5" cellspacing="5">
            <tr>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:RightY &gt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:RightY"/>
              </td>
              <td></td>
            </tr>
            <tr>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:RightX &lt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:RightX"/>
              </td>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:RightX &gt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:RightX"/>
              </td>
            </tr>
            <tr>
              <td></td>
              <td>
                <xsl:choose>
                  <xsl:when  test='xinputgamepad:Thumbsticks/xinputgamepad:RightY &lt; 0'>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#9ecba3</xsl:text>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:attribute name="bgcolor">
                      <xsl:text disable-output-escaping="yes">#f67f7f</xsl:text>
                    </xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="xinputgamepad:Thumbsticks/xinputgamepad:RightY"/>
              </td>
              <td></td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
    <script>
      <![CDATA[
function isSilverlightInstalled(version)
{
    var isVersionSupported=false;
    var container = null;
    
    try 
    {
        var control = null;
        
        try
        {
            control = new ActiveXObject('AgControl.AgControl');
            if ( version == null )
            {
                isVersionSupported = true;
            }
            else if ( control.IsVersionSupported(version) )
            {
                isVersionSupported = true;
            }
            control = null;
        }
        catch (e)
        {
            var plugin = navigator.plugins["Silverlight Plug-In"] ;
            if ( plugin )
            {
                if ( version === null )
                {
                    isVersionSupported = true;
                }
                else
                {
                    var actualVer = plugin.description;
                    if ( actualVer === "1.0.30226.2")
                        actualVer = "2.0.30226.2";
                    var actualVerArray =actualVer.split(".");
                    while ( actualVerArray.length > 3)
                    {
                        actualVerArray.pop();
                    }
                    while ( actualVerArray.length < 4)
                    {
                        actualVerArray.push(0);
                    }
                    var reqVerArray = version.split(".");
                    while ( reqVerArray.length > 4)
                    {
                        reqVerArray.pop();
                    }
                    
                    var requiredVersionPart ;
                    var actualVersionPart
                    var index = 0;
                    
                    do
                    {
                        requiredVersionPart = parseInt(reqVerArray[index]);
                        actualVersionPart = parseInt(actualVerArray[index]);
                        index++;
                    }
                    while (index < reqVerArray.length && requiredVersionPart === actualVersionPart);
                    
                    if ( requiredVersionPart <= actualVersionPart && !isNaN(requiredVersionPart) )
                    {
                        isVersionSupported = true;
                    }
                }
            }
        }
    }
    catch (e) 
    {
        isVersionSupported = false;
    }
    if (container) 
    {
        document.body.removeChild(container);
    }
    
    return isVersionSupported;
}
if(isSilverlightInstalled("2.0"))
{
  if (document.getElementById)
  { 
    document.getElementById("silverlightControlHost").style.display=""; 
    document.getElementById("htmlXboxTable").style.display="none"; 
  }
}
      ]]>
    </script>
  </xsl:template>

</xsl:stylesheet>
