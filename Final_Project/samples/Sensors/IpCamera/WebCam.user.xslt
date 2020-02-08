<?xml version="1.0" encoding="utf-8" ?>
<!--
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: WebCam.user.xslt $ $Revision: 5 $
-->
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:wc ="http://schemas.microsoft.com/robotics/2006/05/webcamservice.html"
  xmlns:pm="http://schemas.microsoft.com/robotics/2006/07/physicalmodel.html"
  xmlns:ip="http://schemas.microsoft.com/robotics/2007/02/ipcamera.user.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        TCP/IP Camera Webcam Service
      </xsl:with-param>
      <xsl:with-param name="description">
        Webcam Viewer showing images from a camera accessed using TCP/IP
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
var sStore = "DssSimWebCam";
var fLoaded = false;

function loadImage()
{
    var img = document.getElementById("TargetImg");
    var timeStamp = new Date();

    img.src = targetUrl + "?time=" + timeStamp.getTime();

    frameCount++;
    if (frameCount % 4 == 0)
    {
        var now = new Date();
        var interval = now.valueOf() - startTime.valueOf();

        document.getElementById("spanFrameRate").innerText = (1000 * frameCount / interval).toFixed(1) + " fps"
    }
}

function onImageLoad()
{
    document.getElementById("TargetImg").alt = "Webcam Image";

    //fire the function every refreshRate
    if (feedRunning)
    {
        setTimeout("loadImage()", refreshTime);
    }
}

function onImageError()
{
    document.getElementById("TargetImg").alt = "Webcam Image - failed to load";
    stopFeed();
}

function startFeed()
{
    feedRunning = true;
    frameCount = 0;
    startTime = new Date();
    loadImage();

    document.getElementById("btnStart").disabled = true;
    document.getElementById("btnStop").disabled = false;

    saveInput();
}

function stopFeed()
{
    feedRunning = false;

    document.getElementById("btnStart").disabled = false;
    document.getElementById("btnStop").disabled = true;

    document.getElementById("spanFrameRate").innerText = "Stopped";
    saveInput();
}

function setRefresh(value)
{
    var data = parseInt(value);

    if (isNaN(data))
    {
        document.getElementById("txtInterval").value = refreshTime;
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
        var oPersist = document.getElementById("txtInterval");
        oPersist.setAttribute("sInterval", oPersist.value);
        oPersist.setAttribute("sRunning", feedRunning);
        oPersist.save(sStore);
    }
}

function loadInput()
{
    var oPersist = document.getElementById("txtInterval");
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
    document.getElementById("TargetImg").src = self.location.href + "/jpeg";
    loadInput();
}

//        ]]>
          </xsl:comment>
        </script>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="/ip:IpCameraState">
    <xsl:call-template name="CameraState"/>
  </xsl:template>

  <xsl:template match="/wc:WebCamState">
    <xsl:call-template name="CameraState"/>
  </xsl:template>

  <xsl:template name="CameraState">
    <table>
      <tr class="odd">
        <th colspan="2">
          Webcam
        </th>
      </tr>
      <tr class="even">
        <td colspan="2" align="center">
          <img id="TargetImg" name="TargetImg" src="jpeg" alt="Webcam Image"
            width="{wc:ImageSize/pm:X}"
            height="{wc:ImageSize/pm:Y}" onload="onImageLoad()" onerror="onImageError()"/>
        </td>
      </tr>
      <xsl:if test="/ip:IpCameraState">
        <tr class="odd">
          <th>
            Image Source
          </th>
          <td>
            <form style="display:inline;" method="Post">
              <input type="Text" id="txtLocation" name="Location" value="{ip:CameraImageLocation}" size="30"/>
              <xsl:text> </xsl:text>
              <input type="Submit" value="Change"/>
              <br/>
              Directly access <a href="{ip:CameraImageLocation}">image</a>.
            </form>
          </td>
        </tr>
      </xsl:if>
      <tr class="even">
        <th>
          Refresh Interval
        </th>
        <td>
          <input type="Text" class="storeuserData" id="txtInterval"  name="Interval" value="250" size="1" onchange="setRefresh(this.value)" />
        </td>
      </tr>
      <tr class="odd">
        <th>Frame Rate</th>
        <td>
          <span id="spanFrameRate">Stopped</span>
        </td>
      </tr>
      <tr class="even">
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
      <tr class="odd">
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
        </td>
      </tr>
    </table>
  </xsl:template>

</xsl:stylesheet>
