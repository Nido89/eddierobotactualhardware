<?xml version="1.0" encoding="utf-8" ?>
<!--
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: WebCam.user.xslt $ $Revision: 8 $
-->
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:ps ="http://schemas.microsoft.com/robotics/2011/08/kinect.user.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        Kinect Service
      </xsl:with-param>
      <xsl:with-param name="description">
        Viewer showing current rgb and depth images from the Kinect Depth Camera
      </xsl:with-param>
      <xsl:with-param name="head">
        <style type="text/css">
          .storeuserData {behavior:url(#default#userData);}
        </style>
        <script language="javascript" type="text/javascript">
          <xsl:comment>
            <![CDATA[
var refreshTime = 250;
var targetUrl = self.location.href + "?type=depthplusrgb";
var feedRunning = false;
var frameCount;
var startTime;
var sStore = "DssKinect";
var fLoaded = false;

function loadImage()
{
    var img = document.getElementById("TargetImg");
    var timeStamp = new Date();

    img.src = targetUrl + "&time=" + timeStamp.getTime();

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
    document.getElementById("TargetImg").alt = "Depth Camera Image";

    //fire the function every refreshRate
    if (feedRunning)
    {
        setTimeout("loadImage()", refreshTime);
    }
}

function onImageError()
{
    document.getElementById("TargetImg").alt = "Depth Camera Image - failed to load";
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
    document.getElementById("TargetImg").src = targetUrl;
    loadInput();
}

//        ]]>
          </xsl:comment>
        </script>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>


  <xsl:template match="/ps:KinectState">
    <table>
      <tr class="odd">
        <th colspan="2">
          Live Image
        </th>
      </tr>
      <tr class="even">
        <td colspan="2" align="center">
          <img id="TargetImg" name="TargetImg" src="." alt="Depth Camera Image"
               onload="onImageLoad()" onerror="onImageError()"/>
        </td>
      </tr>
      <tr class="odd">
        <th>
          Browser Refresh Interval (ms)
        </th>
        <td>
          <input type="Text" class="storeuserData" id="txtInterval"  name="Interval" value="250" size="1" onchange="setRefresh(this.value)" />
        </td>
      </tr>
      <tr class="even">
        <th>Actual Browser Refresh Rate</th>
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
            <option value="?type=depthplusrgb" selected="selected">DepthPlusRGB</option>
            <option value="?type=depth">Depth</option>
            <option value="?type=rgb">RGB</option>            
          </select>
        </td>
      </tr>
     
      <tr class="even">
        <th>Camera Version</th>
        <td>
          <xsl:value-of select="ps:Version"/>
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
        <tr class="even">
            <th>Tilt (- 27 to 27 degrees) </th>
            <td>
                <form method="POST" style="display:inline">
                    <input type="hidden" Name="SoapAction" Value="UpdateRequest"/>
                    <input type="hidden"
                           Name="DsspOperationBodyType"
                           Value="Microsoft.Robotics.Services.Sensors.Kinect.UpdateTiltRequest"/>
                    <input type="Text" Name="Tilt" Value="0"/>
                    <input type="Submit" Value="Set"/>
                </form>
            </td>
        </tr>
        <tr class="odd">
            <th>Kinect Frame Rate (1-30 fps)</th>
            <td>
                <form method="POST" style="display:inline">
                    <input type="hidden" Name="SoapAction" Value="UpdateRequest"/>
                    <input type="hidden"
                           Name="DsspOperationBodyType"
                           Value="Microsoft.Robotics.Services.Sensors.Kinect.SetFrameRateRequest"/>
                    <input type="Text" Name="FrameRate" Value="30"/>
                    <input type="Submit" Value="Set"/>
                </form>
            </td>
        </tr>
    </table>
  </xsl:template>

</xsl:stylesheet>
