<?xml version="1.0" encoding="UTF-8" ?>
<!-- 
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: ColorSegment.xslt $ $Revision: 2 $
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:cs="http://schemas.microsoft.com/robotics/2007/07/colorsegment.html"
                xmlns:u="http://schemas.microsoft.com/robotics/2007/05/utilities.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        Color Segmentation Service
      </xsl:with-param>
      <xsl:with-param name="description">
        Color Segmentation Service Viewer
      </xsl:with-param>
      <xsl:with-param name="head">
        <style type="text/css">
          .storeuserData {behavior:url(#default#userData);}
        </style>
        <script language="javascript" type="text/javascript">
          <xsl:comment>
            <![CDATA[
var refreshTime = 250;
var targetUrl = self.location.href + "/SegmentedImage";
var feedRunning = false;
var frameCount;
var startTime;
var sStore = "DssColorSegment";
var sPersistObject = "txtInterval";
var fLoaded = false;

function loadImage()
{
    var img = document.all("SegmentedImg");
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

function loadSource()
{
    var img = document.all("SourceImg");
    var imgSrc = document.all("SourceImgUri");
    var timeStamp = new Date();

    img.src = imgSrc.value + "?time=" + timeStamp.getTime();
}

function onImageLoad()
{
    document.all("SegmentedImg").alt = "Segmented Image";

    //fire the function every refreshRate
    if (feedRunning)
    {
        setTimeout("loadImage()", refreshTime);
    }
}

function onImageError()
{
    document.all("SegmentedImg").alt = "Segmented Image - failed to load";
    stopFeed();
}

function onSourceLoad()
{
    document.all("SourceImg").alt = "Source Image";

    //fire the function every refreshRate
    if (feedRunning)
    {
        setTimeout("loadSource()", refreshTime);
    }
}

function onSourceError()
{
    document.all("SourceImg").alt = "Source Image - failed to load";
    stopFeed();
}

function startFeed()
{
    feedRunning = true;
    frameCount = 0;
    startTime = new Date();
    loadImage();
    loadSource();

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
    document.all("SegmentedImg").src = self.location.href + "/SegmentedImage";
    loadInput();
}

function updateState()
{
    document.all["RawState"].src = null;
    var timeStamp = new Date();
    document.all["RawState"].src = self.location.href + "?Time=" + timeStamp.getTime();
}

setTimeout("updateState()", 1000);

function RawStateComplete()
{
  var state = document.all["RawState"].XMLDocument;

  var stateFrameCount = state.selectSingleNode("ColorSegmentState/FrameCount").text;
  var stateDroppedFrames = state.selectSingleNode("ColorSegmentState/DroppedFrames").text;
  var uriNode = state.selectSingleNode("ColorSegmentState/ImageSource/UriValue");

  document.all["FrameCount"].innerText = stateFrameCount;
  document.all["DroppedFrames"].innerText = stateDroppedFrames + " " + parseInt(parseInt(stateDroppedFrames) * 100 / parseInt(stateFrameCount)) + "%";

  if (uriNode != null)
  {
    var webcamUri = uriNode.text + "/jpeg";

    if (document.all["SourceImgUri"].value != webcamUri)
    {
        document.all["SourceImgUri"].value = webcamUri;
        loadSource();
        loadImage();
    }
  }

  var colorAreas = state.selectNodes("ColorSegmentState/FoundColorAreas/Areas/ColorArea");
  for (var index = 0; index < 10; index++)
  {
    var div = document.all("SegmentedBlobDiv" + index);

    if (div != null)
    {
      if (index < colorAreas.length)
      {
        var area = colorAreas.item(index);
        var minX = parseInt(area.selectSingleNode("MinX").text);
        var maxX = parseInt(area.selectSingleNode("MaxX").text);
        var minY = parseInt(area.selectSingleNode("MinY").text);
        var maxY = parseInt(area.selectSingleNode("MaxY").text);


        div.style.left = minX - 1;
        div.style.top = minY - 1;
        div.style.width = maxX - minX;
        div.style.height = maxY - minY;
        div.style.display = "block";
      }
      else
      {
        div.style.display = "none";
      }
    }
  }

  setTimeout("updateState()", 250);
}

var click = 0;
var startx = 0;
var starty = 0;
var endx = 0;
var endy = 0;

function onMouseUp()
{
  if (click == 0)
  {
    startx = endx = event.x;
    starty = endy = event.y;
    displayDrag(true);
    click++;
  }
  else if (click == 1)
  {
    if (event.x < startx)
    {
      startx = event.x;
    }
    else
    {
      endx = event.x;
    }
    if (event.y < starty)
    {
      starty = event.y;
    }
    else
    {
      endy = event.y;
    }
    displayDrag(true);
    click++;
    enableControls(true);
  }
  else
  {
    enableControls(false);
    displayDrag(false);
    click = 0;
  }
}

function onMouseMove()
{
  if (click == 1)
  {
    if (event.x < startx)
    {
      startx = event.x;
    }
    else
    {
      endx = event.x;
    }
    if (event.y < starty)
    {
      starty = event.y;
    }
    else
    {
      endy = event.y;
    }
    displayDrag(true);
  }
}

function enableControls(enable)
{
  var f;
  for (f = 0; f < document.forms.length; f++)
  {
    var i;
    var form = document.forms(f);
    for (i = 0; i < form.elements.length; i++)
    {
      var element = form.elements(i);

      if (element.name.indexOf("New.") != -1)
      {
        element.disabled = !enable;
      }
    }
  }
}

function displayDrag(display)
{
  //var obj = document.all("SourceDrag");
  var div = document.all("SourceDragDiv");

  if (display)
  {
    //obj.innerText = "(" + startx + ", " + starty + ") -> (" + endx + ", " + endy + ")";
    div.style.display = "block";
    div.style.left = startx - 2;
    div.style.top = starty - 2;
    div.style.width = endx - startx;
    div.style.height = endy - starty;

    document.all("New.Left").value = startx;
    document.all("New.Top").value = starty;
    document.all("New.Width").value = endx - startx;
    document.all("New.Height").value = endy - starty;
  }
  else
  {
    //obj.innerText = "";
    div.style.display = "none";

    document.all("New.Left").value = "";
    document.all("New.Top").value = "";
    document.all("New.Width").value = "";
    document.all("New.Height").value = "";
  }
}

//        ]]>
          </xsl:comment>
        </script>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="/cs:ColorSegmentState">
    <h1>Color Segmentation Service</h1>
    <xml id="RawState" ondatasetcomplete="RawStateComplete()"/>
    <form id="oForm" action="" method="post">
      <table>
        <tr>
          <th colspan="2">
            Source Image
          </th>
          <th colspan="2">
            Segmented Image
          </th>
        </tr>
        <tr>
          <td colspan="2">
            <div style="position: relative"
                   onmouseup="onMouseUp()" onmousemove="onMouseMove()">
              <input id ="SourceImgUri" name="SourceImgUri" type="hidden" value="{cs:ImageSource/u:UriValue}/jpeg" />
              <img id="SourceImg" name="SourceImg" src="{cs:ImageSource/u:UriValue}/jpeg" alt="SourceImg"
                   onload="onSourceLoad()" onerror="onSourceError()"/>
              <div id="SourceDragDiv" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: Red; border-width: 2px; border-style: solid; display: none;"> </div>
              <br/>
              <span id="SourceDrag"></span>
            </div>
          </td>
          <td colspan="2">
            <div style="position: relative">
              <img id="SegmentedImg" name="SegmentedImg" src="colorsegment/SegmentedImage" alt="Segmented Image"
                onload="onImageLoad()" onerror="onImageError()"/>
              <div id="SegmentedBlobDiv0" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv1" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv2" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv3" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv4" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv5" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv6" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv7" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv8" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
              <div id="SegmentedBlobDiv9" style="position: absolute; left: 10; top: 10; width: 100; height: 100; border-color: White; border-width: 1px; border-style: solid; display: none;"> </div>
            </div>
          </td>
        </tr>
        <tr class="odd">
          <th>
            Refresh Interval
          </th>
          <td colspan="3">
            <input type="Text" class="storeuserData" id="txtInterval"  name="Interval" value="250" size="1" onchange="setRefresh(this.value)" />
          </td>
        </tr>
        <tr class="even">
          <th>Frame Rate</th>
          <td colspan="3">
            <span id="spanFrameRate">Stopped</span>
          </td>
        </tr>
        <tr class="odd">
          <th>
            Control
          </th>
          <td colspan="3">
            <button id="btnStart" name="btnStart" onclick="startFeed()">
              Start
            </button>
            <button id="btnStop" name="btnStop" onclick="stopFeed()" disabled="disabled">
              Stop
            </button>
            <button id="btnRefresh" name="btnRefresh" onclick="loadImage(); loadSource();">
              Refresh
            </button>
          </td>
        </tr>
        <tr class="odd">
          <th>Frame Count</th>
          <td colspan="3">
            <span id="FrameCount">
              <xsl:value-of select="cs:FrameCount"/>
            </span>
          </td>
        </tr>
        <tr class="even">
          <th>Dropped Frames</th>
          <td colspan="3">
            <span id="DroppedFrames">
              <xsl:value-of select="cs:DroppedFrames"/>
            </span>
          </td>
        </tr>
      </table>
      <table>
        <tr class="odd">
          <th>Settings</th>
          <td>
            <xsl:text>Threshold: </xsl:text>
            <input type="text" name="Threshold" id="Threshold" value="{cs:Settings/cs:Threshold}" size="3"/>
            <xsl:text> Show partial matches: </xsl:text>
            <input type="checkbox" name="ShowPartial" id="ShowPartial">
              <xsl:if test="cs:Settings/cs:ShowPartialMatches = 'true'">
                <xsl:attribute name="checked">
                  <xsl:text>true</xsl:text>
                </xsl:attribute>
              </xsl:if>
            </input>
            <xsl:text> Despeckle: </xsl:text>
            <input type="checkbox" name="Despeckle" id="Despeckle">
              <xsl:if test="cs:Settings/cs:Despeckle = 'true'">
                <xsl:attribute name="checked">
                  <xsl:text>true</xsl:text>
                </xsl:attribute>
              </xsl:if>
            </input>
            <xsl:text> Minimum Blob Size: </xsl:text>
            <input type="text" name="MinBlobSize" id="MinBlobSize" value="{cs:Settings/cs:MinBlobSize}" size="3"/>
            <xsl:text> </xsl:text>
            <input type="Submit" name="UpdateSettings" id="UpdateSettings" value="Change"/>
          </td>
        </tr>
      </table>
      <xsl:apply-templates select="cs:Colors"/>
    </form>
  </xsl:template>

  <xsl:template match="cs:Colors">
    <table>
      <tr>
        <th colspan="3">Color Definitions</th>
      </tr>
      <xsl:apply-templates select="cs:ColorSet"/>
      <tr>
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="count(cs:ColorDefinition) mod 2 = 0">
              <xsl:text>even</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>odd</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <th>
          <xsl:text>Add Entry: </xsl:text>
        </th>
        <td colspan="2">
          <xsl:text>Name: </xsl:text>
          <input type="Text" name="New.Name" id="New.Name" disabled="true"/>
          <xsl:text>Top: </xsl:text>
          <input type="Text" name="New.Top" id="New.Top"  disabled="true" size="2" readonly="true"/>
          <xsl:text>Left: </xsl:text>
          <input type="Text" name="New.Left" id="New.Left" disabled="true" size="2" readonly="true"/>
          <xsl:text>Width: </xsl:text>
          <input type="Text" name="New.Width" id="New.Width" disabled="true" size="2" readonly="true"/>
          <xsl:text>Height: </xsl:text>
          <input type="Text" name="New.Height" id="New.Height" disabled="true" size="2" readonly="true"/>
          <xsl:text> - </xsl:text>
          <input type="submit" name="New.Add" disabled="true" value="Add"/>
        </td>
      </tr>
      <tr>
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="count(cs:ColorDefinition) mod 2 = 0">
              <xsl:text>odd</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>even</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <td colspan="3" align="right">
          <input type="submit" name="Save" value="Save"/>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="cs:ColorSet">
    <xsl:variable name="class">
      <xsl:choose>
        <xsl:when test="position() mod 2 = 0">
          <xsl:text>even</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>odd</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="rows" select="count(cs:Colors/cs:ColorDefinition)"/>
    <xsl:for-each select="cs:Colors/cs:ColorDefinition">
      <tr class="{$class}">
        <xsl:choose>
          <xsl:when test="position() = 1">
            <xsl:variable name="red" select="cs:R"/>
            <xsl:variable name="green" select="cs:G"/>
            <xsl:variable name="blue" select="cs:B"/>

            <xsl:variable name="r">
              <xsl:value-of select="substring('0123456789ABCDEF',1 + floor($red div 16),1)"/>
              <xsl:value-of select="substring('0123456789ABCDEF',1 + floor($red mod 16),1)"/>
            </xsl:variable>
            <xsl:variable name="g">
              <xsl:value-of select="substring('0123456789ABCDEF',1 + floor($green div 16),1)"/>
              <xsl:value-of select="substring('0123456789ABCDEF',1 + floor($green mod 16),1)"/>
            </xsl:variable>
            <xsl:variable name="b">
              <xsl:value-of select="substring('0123456789ABCDEF',1 + floor($blue div 16),1)"/>
              <xsl:value-of select="substring('0123456789ABCDEF',1 + floor($blue mod 16),1)"/>
            </xsl:variable>
            <th rowspan="{$rows}" style="background-color: #{concat($r,$g,$b)};" onclick="document.all('New.Name').value = event.srcElement.innerText;">
              <font>
                <xsl:attribute name="color">
                  <xsl:choose>
                    <xsl:when test="cs:Y &gt; 100">
                      <xsl:text>black</xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text>white</xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:attribute>
                <xsl:value-of select="cs:Name"/>
              </font>
            </th>
          </xsl:when>
          <xsl:otherwise>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:call-template name="ColorDefinition"/>
      </tr>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="ColorDefinition">
    <td>
      <xsl:text>Y, Cb, Cr: </xsl:text>
      <xsl:value-of select="concat('(', cs:Y, '&#177;', cs:SigmaY, ', ', cs:Cb, '&#177;', cs:SigmaCb, ', ', cs:Cr, '&#177;', cs:SigmaCr,') ')"/>
      <xsl:text> </xsl:text>
      <xsl:text>R, G, B: </xsl:text>
      <xsl:value-of select="concat('(', cs:R, ', ', cs:G, ', ', cs:B,') ')"/>
      <xsl:if test="position() = 1">
        <span id="{concat(cs:Name,'.Center')}"/>
      </xsl:if>
    </td>
    <td>
      <input type="Submit" name="{concat('Delete.', cs:Name, '.', cs:Y, '.', cs:Cb, '.', cs:Cr)}" value="Delete"/>
      <xsl:text> </xsl:text>
      <input type="Submit" name="{concat('ExpandY.', cs:Name, '.', cs:Y, '.', cs:Cb, '.', cs:Cr)}" value="Expand Y"/>
    </td>
  </xsl:template>
</xsl:stylesheet>