<?xml version="1.0" encoding="utf-8" ?>
<!--
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: WebCam.user.xslt $ $Revision: 11 $
-->
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:md="http://schemas.microsoft.com/robotics/2006/05/multidevicewebcamservice.user.html"
  xmlns:wc="http://schemas.microsoft.com/robotics/2006/05/webcamservice.html"
  xmlns:pm="http://schemas.microsoft.com/robotics/2006/07/physicalmodel.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        Webcam Service
      </xsl:with-param>
      <xsl:with-param name="description">
        Webcam Viewer
      </xsl:with-param>
      <xsl:with-param name="head">
        <style type="text/css">
          .storeuserData {behavior:url(#default#userData);}
        </style>
        <script language="javascript" type="text/javascript">
          <xsl:comment>
            <![CDATA[
var refreshTime = 250;
var targetUrl = self.location.href + "/jpeg";
var feedRunning = false;
var frameCount;
var startTime;
var sStore = "DssWebCam";
var sPersistObject = "txtInterval";
var fLoaded = false;

function loadImage()
{
    var img = document.all("TargetImg");
    var timeStamp = new Date();

    img.src = targetUrl + "?time=" + timeStamp.getTime();

    frameCount++;
    if (frameCount % 4 == 0)
    {
        var now = new Date();
        var interval = now.valueOf() - startTime.valueOf();
        document.all("spanFrameRate").innerText = (1000 * frameCount / interval).toFixed(1) + " fps"
    }
}

function onImageLoad()
{
    document.all("TargetImg").alt = "Webcam Image";

    //fire the function every refreshRate
    if (feedRunning)
    {
        setTimeout("loadImage()", refreshTime);
    }
}

function onImageError()
{
    document.all("TargetImg").alt = "Webcam Image - failed to load";
    stopFeed();
}

function startFeed()
{
    feedRunning = true;
    frameCount = 0;
    startTime = new Date();
    loadImage();

    document.all("btnStart").disabled = true;
    document.all("btnRefresh").disabled = true;
    document.all("btnStop").disabled = false;

    saveInput();
}

function stopFeed()
{
    feedRunning = false;

    document.all("btnStart").disabled = false;
    document.all("btnRefresh").disabled = false;
    document.all("btnStop").disabled = true;

    document.all("spanFrameRate").innerText = "Stopped";
    saveInput();
}

function setRefresh(value)
{
    var data = parseInt(value);

    if (isNaN(data))
    {
        document.all(sPersistObject).value = refreshTime;
    }
    else
    {
        refreshTime = data;
    }
    frameCount = 0;
    startTime = new Date();

    saveInput();
}

function saveInput()
{
    if (fLoaded)
    {
        var oPersist = document.all(sPersistObject);

        oPersist.setAttribute("sInterval", oPersist.value);
        oPersist.setAttribute("sRunning", feedRunning);

        oPersist.save(sStore);
    }
}

function loadInput()
{
    var oPersist = document.all(sPersistObject);
    oPersist.load(sStore);
    var vValue = oPersist.getAttribute("sInterval");
    if (vValue != null)
    {
        var refresh = parseInt(vValue);
        if (!isNaN(refresh))
        {
            oPersist.value = refresh;
            setRefresh(refresh);
        }
    }
    vValue = oPersist.getAttribute("sRunning");
    if (vValue == "true")
    {
        startFeed();
    }
    else
    {
        stopFeed();
    }

    fLoaded = true;
}

dssRuntime.init = function()
{
    document.all("TargetImg").src = self.location.href + "/jpeg";
    loadInput();
}

//        ]]>
          </xsl:comment>
        </script>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="/md:WebCamState">
    <xsl:call-template name="Root"/>
  </xsl:template>

  <xsl:template match="/wc:WebCamState">
    <xsl:call-template name="Root"/>
  </xsl:template>

  <xsl:template name="Root">
    <form id="oForm" action="" method="post">
      <table>
        <tr class="odd">
          <th colspan="2">
            Webcam
          </th>
        </tr>
        <tr class="even">
          <td colspan="2" align="center">
            <img id="TargetImg" name="TargetImg" src="jpeg" alt="Webcam Image"
              width="{wc:ImageSize/pm:X}" height="{wc:ImageSize/pm:Y}"
              onload="onImageLoad()" onerror="onImageError()"/>
          </td>
        </tr>
        <tr class="odd">
          <th>
            Refresh Interval
          </th>
          <td>
            <input type="Text" class="storeuserData" id="txtInterval"  name="Interval" value="250" size="1" onchange="setRefresh(this.value)" />
          </td>
        </tr>
        <tr class="even">
          <th>Frame Rate</th>
          <td>
            <span id="spanFrameRate">Stopped</span>
          </td>
        </tr>
        <tr class="odd">
          <th>
            Display Format
          </th>
          <td>
            <select name="Format" onchange='targetUrl = self.location.href + this.value; loadImage();'>
              <option value="/jpeg" selected="selected">JPEG</option>
              <option value="/gif">GIF</option>
              <option value="/bmp">BMP</option>
            </select>
          </td>
        </tr>
        <xsl:if test="count(md:Cameras/md:CameraInstance) > 0">
          <tr class="even">
            <th>Capture Format</th>
            <td>
              <xsl:apply-templates select="md:Selected/md:SupportedFormats">
                <xsl:with-param name="current" select="md:Selected/md:Format"/>
              </xsl:apply-templates>
            </td>
          </tr>
          <tr class="odd">
            <th>Camera</th>
            <td>
              <xsl:apply-templates select="md:Cameras"/>
            </td>
          </tr>
        </xsl:if>
        <tr class="even">
          <th>
            Control
          </th>
          <td>
            <button id="btnStart" name="btnStart" onclick="startFeed()">
              Start
            </button>
            <button id="btnStop" name="btnStop" onclick="stopFeed()" disabled="disabled">
              Stop
            </button>
            <button id="btnRefresh" name="btnRefresh" onclick="loadImage()">
              Refresh
            </button>
          </td>
        </tr>
      </table>
    </form>
  </xsl:template>

  <xsl:template match="md:Cameras">
    <select name="Camera">
      <xsl:apply-templates select="md:CameraInstance">
        <xsl:with-param name="selected" select="/md:WebCamState/md:Selected/md:DevicePath"/>
        <xsl:sort select="md:FriendlyName"/>
      </xsl:apply-templates>
    </select>
    <xsl:text> </xsl:text>
    <input name="ChangeCamera" type="Submit" value="Change"/>
  </xsl:template>

  <xsl:template match="md:CameraInstance">
    <xsl:param name="selected"/>
    <option value="{md:DevicePath}">
      <xsl:if test="$selected = md:DevicePath">
        <xsl:attribute name="selected">
          <xsl:text>selected</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:value-of select="md:FriendlyName"/>
    </option>
  </xsl:template>

  <xsl:template match="md:Selected/md:SupportedFormats">
    <xsl:param name="current"/>
    <select name="CaptureFormat">
      <xsl:if test="count(md:Format) = 0">
        <option value="0" selected="selected">
          <xsl:value-of select="$current/wc:Width"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="$current/wc:Height"/>
          <xsl:if test="string-length($current/wc:Compression) > 0">
            <xsl:text> - </xsl:text>
            <xsl:value-of select="$current/wc:Compression"/>
          </xsl:if>
        </option>
      </xsl:if>
      <xsl:apply-templates select="md:Format">
        <xsl:with-param name="current" select="$current"/>
      </xsl:apply-templates>
    </select>
    <xsl:text> </xsl:text>
    <input name="ChangeFormat" type="Submit" value="Change"/>
  </xsl:template>

  <xsl:template match="md:Format">
    <xsl:param name="current"/>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="position()"/>
      </xsl:attribute>
      <xsl:if test="$current/md:Width = md:Width and $current/md:Height = md:Height and $current/md:Compression = md:Compression">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:value-of select="md:Width"/>
      <xsl:text>x</xsl:text>
      <xsl:value-of select="md:Height"/>
      <xsl:if test="string-length(md:Compression) > 0">
        <xsl:text> - </xsl:text>
        <xsl:value-of select="md:Compression"/>
      </xsl:if>
    </option>
  </xsl:template>
</xsl:stylesheet>
