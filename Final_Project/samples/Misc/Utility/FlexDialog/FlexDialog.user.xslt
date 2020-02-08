<?xml version="1.0" encoding="UTF-8" ?>
<!-- 
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: FlexDialog.user.xslt $ $Revision: 6 $
-->
<xsl:stylesheet version="1.0" 
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:fd="http://schemas.microsoft.com/robotics/2007/08/flexdialog.user.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        Flexible Dialog Service
      </xsl:with-param>
      <xsl:with-param name="description">
        Flexible dialog service that allows configurable dialogs for user interaction.
      </xsl:with-param>
      <xsl:with-param name="head">
        <script>
          <![CDATA[<!--

var xmlUpdateControl = null;

function updateControl(id, value, text)
{
  if (xmlUpdateControl == null)
  {
    xmlUpdateControl = new ActiveXObject("Microsoft.XMLHTTP");

    xmlUpdateControl.open("POST", self.location.href, true);
    xmlUpdateControl.onreadystatechange = handleUpdateControlState;
    xmlUpdateControl.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    xmlUpdateControl.send("Operation=UpdateControl&Id=" + escape(id) + "&Value=" + escape(value) + "&Text=" + escape(text));
  }
}

function handleUpdateControlState()
{
  if (xmlUpdateControl.readyState == 4)
  {
    xmlUpdateControl = null;
  }
}

function textChange()
{
  updateControl(event.srcElement.id, event.srcElement.value, "");
}

function checkBoxChange()
{
  updateControl(event.srcElement.id, event.srcElement.checked, event.srcElement.value);
}

function comboBoxChange()
{
  var combo = event.srcElement;

  var option = combo.options[combo.selectedIndex];

  var text = ""

  for (var index = 0; index < combo.options.length; index++)
  {
     if (index > 0)
     {
       text = text + "|";
     }
     text = text + combo.options[index].text;
  }

  updateControl(combo.id, option.text, text);
}

var xmlButtonPress = null;

function buttonPress(id, pressed)
{
  if (xmlButtonPress == null)
  {
    xmlButtonPress = new ActiveXObject("Microsoft.XMLHTTP");

    xmlButtonPress.open("POST", self.location.href, true);
    xmlButtonPress.onreadystatechange = handleButtonPressState;
    xmlButtonPress.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    xmlButtonPress.send("Operation=ButtonPress&Id=" + escape(id) + "&Pressed=" + escape(pressed));
  }
}

function handleButtonPressState()
{
  if (xmlButtonPress.readyState == 4)
  {
    xmlButtonPress = null;
  }
}

function buttonMouseDown()
{
  buttonPress(event.srcElement.id, true);
}

function buttonMouseUp()
{
  buttonPress(event.srcElement.id, false);
}

var xmlNotification = null;
var timeStamp = new Date();
var sessionId = timeStamp.getTime();
var sequence = 0;

function getNextNotification()
{
  xmlNotification = new ActiveXObject("Microsoft.XMLHTTP");

  sequence++;
  xmlNotification.open("GET", self.location.href + "?Session=" + sessionId + "&Sequence=" + sequence, true);
  xmlNotification.onreadystatechange = handleNotificationState;
  xmlNotification.send();
}

function handleNotificationState()
{
  if (xmlNotification.readyState == 4)
  {
    var timeout = 2000;

    try
    {
      var notification = xmlNotification.responseXML;
      var document = notification.documentElement;
      var root = document.baseName;
      if (root == "HttpNotification")
      {
        timeout = processNotification(objectify(document))
      }
    }
    catch(expression)
    {
    }
    setTimeout("getNextNotification()",timeout);
  }
}

function objectify(xmlElement)
{
  var children = xmlElement.childNodes;
  var index;
  var obj;

  for (index = 0; index < children.length; index++)
  {
    var child = children.item(index);
    if (child.nodeType == 3) // text
    {
      obj = child.nodeValue;
    }
    else if (child.nodeType == 1) // element
    {
      if (obj == undefined)
      {
        obj = new Object();
      }
      obj[child.baseName] = objectify(child);
    }
  }

  return obj;
}

function processNotification(notification)
{
  var timeout = 0;
  try
  {
    if (notification.Operation == "None")
    {
      timeout = 250;
    }
    else if (notification.Operation == "UpdateControl")
    {
      with(notification.Control)
      {
        var obj = document.all(Id);

        if (ControlType == "Button")
        {
          obj.innerText = Text;
        }
        else if (ControlType == "TextBox" ||
                 ControlType == "MultiLineTextBox")
        {
          if (Value == undefined)
          {
            obj.value = "";
          }
          else
          {
            obj.value = Value;
          }
        }
        else if (ControlType == "CheckBox" ||
                 ControlType == "RadioButton")
        {
          obj.value = Text;
          obj.checked = (Value == "True");
          var span = document.all(Id + "." + ControlType);
          span.innerText = Text;
        }
        else if (ControlType == "ComboBox")
        {
          var options = Text.split("|");
          for (var index = options.length - 1; index >= 0; index--)
          {
            obj.options.remove(index);
          }
          for(var index = 0; index < options.length; index++)
          {
            var text = options[index];
            var option = document.createElement("OPTION");
            obj.options.add(option);
            option.innerText = text;
            option.selected = (text == Value);
          }
        }
      }
    }
    else if (notification.Operation == "UpdateButton")
    {
      with (notification.Button)
      {
        var obj = document.all(Id);
        obj.innerText = Text;
      }
    }
    else if (notification.Operation == "Show")
    {
      with (notification.Show)
      {
        var table = document.all("mainTable");
        if (Show == "true")
        {
          table.style.display = "block";
        }
        else
        {
          table.style.display = "none";
        }
      }
    }
    else if (notification.Operation == "SetTitle")
    {
      with (notification.SetTitle)
      {
        var dlgTitle = document.all("dialogTitle");
        dlgTitle.innerText = Title;
      }
    }
    else if (notification.Operation == "ButtonPress")
    {
    }
    else if (notification.Operation == "HandOff")
    {
      with (notification.HandOff)
      {
        location.href = Service
      }
    }
    else
    {
      location.reload();
    }
  }
  catch(exception)
  {
    timeout = 250;
  }

  return timeout;
}

setTimeout("getNextNotification()",100);

          //-->]]>
        </script>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!--<xsl:output method="html"/>-->

  <xsl:template match="/fd:FlexDialogState">
    <xsl:attribute name="align">
      <xsl:text>center</xsl:text>
    </xsl:attribute>
    <table width="{80 * (1 + count(fd:Buttons/fd:FlexButton))}"
           align="center" id="mainTable">
      <xsl:attribute name="style">
        <xsl:choose>
          <xsl:when test="fd:Visible = 'true'">
            <xsl:text>display:block;border-width: 2px; border-style: outset; border-color: windowframe; background-color:threedface;</xsl:text>
          </xsl:when>
          <xsl:otherwise>
              <xsl:text>display:none;border-width: 2px; border-style: outset; border-color: windowframe; background-color:threedface;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <tr>
        <th style="background-color: activecaption;color:captiontext; align:left">
          <span id="dialogTitle">
            <xsl:value-of select="fd:Title"/>
          </span>
        </th>
      </tr>
      <xsl:apply-templates select="fd:Controls/fd:FlexControl"/>
      <xsl:apply-templates select="fd:Buttons"/>
    </table>
  </xsl:template>

  <xsl:template match="fd:Controls/fd:FlexControl">
    <tr>
      <td align="left">
        <xsl:choose>
          <xsl:when test="fd:ControlType = 'Label'">
            <xsl:value-of select="fd:Text"/>
          </xsl:when>
          <xsl:when test="fd:ControlType = 'TextBox'">
            <input type="text" id="{fd:Id}" value="{fd:Value}" style="width:90%;" onchange="textChange()"/>
          </xsl:when>
          <xsl:when test="fd:ControlType = 'MultiLineTextBox'">
            <textarea id="{fd:Id}" style="width:90%" rows="8" onchange="textChange()">
              <xsl:value-of select="fd:Value"/>
            </textarea>
          </xsl:when>
          <xsl:when test="fd:ControlType = 'Button'">
            <button type="submit" id="{fd:Id}" onmousedown="buttonMouseDown()" onmouseup="buttonMouseUp()">
              <xsl:value-of select="fd:Text"/>
            </button>
          </xsl:when>
          <xsl:when test="fd:ControlType = 'CheckBox'">
            <input type="checkbox" id="{fd:Id}" value="{fd:Text}" onclick="checkBoxChange()">
              <xsl:if test="fd:Value = 'True'">
                <xsl:attribute name="checked">
                  <xsl:text>true</xsl:text>
                </xsl:attribute>
              </xsl:if>
            </input>
            <span id="{concat(fd:Text,'.CheckBox')}">
              <xsl:value-of select="fd:Text"/>
            </span>
          </xsl:when>
          <xsl:when test="fd:ControlType = 'RadioButton'">
            <input type="radio" name="radio" id="{fd:Id}" value="{fd:Text}" onclick="checkBoxChange()">
              <xsl:if test="fd:Value = 'True'">
                <xsl:attribute name="checked">
                  <xsl:text>true</xsl:text>
                </xsl:attribute>
              </xsl:if>
            </input>
            <span id="{concat(fd:Text,'.RadioButton')}">
              <xsl:value-of select="fd:Text"/>
            </span>
          </xsl:when>
          <xsl:when test="fd:ControlType = 'ComboBox'">
            <select id="{fd:Id}" size="1" onchange="comboBoxChange()">
              <xsl:if test="fd:Text">
                <xsl:call-template name="makeOptions">
                  <xsl:with-param name="Delimited" select="fd:Text"/>
                  <xsl:with-param name="Selected" select="fd:Value"/>
                </xsl:call-template>
              </xsl:if>
            </select>
          </xsl:when>
          <xsl:otherwise>
            <hr/>
          </xsl:otherwise>
        </xsl:choose>
      </td>
    </tr>
  </xsl:template>

  <xsl:template name="makeOptions">
    <xsl:param name="Delimited"/>
    <xsl:param name="Delimitor">|</xsl:param>
    <xsl:param name="Selected"/>
    <xsl:variable name="before" select="substring-before($Delimited, $Delimitor)"/>
    <xsl:variable name="after" select="substring-after($Delimited, $Delimitor)"/>

    <xsl:choose>
      <xsl:when test="string-length($before) &gt; 0">
        <option>
          <xsl:if test="$Selected = $before">
            <xsl:attribute name="Selected">true</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="$before"/>
        </option>
        <xsl:call-template name="makeOptions">
          <xsl:with-param name="Delimited" select="$after"/>
          <xsl:with-param name="Delimitor" select="$Delimitor"/>
          <xsl:with-param name="Selected" select="$Selected"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <option>
          <xsl:if test="$Selected = $Delimited">
            <xsl:attribute name="Selected">true</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="$Delimited"/>
        </option>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="fd:Buttons">
    <tr>
      <td align="right" valign="center" height="30">
        <xsl:apply-templates select="fd:FlexButton"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="fd:FlexButton">
    <xsl:text> </xsl:text>
    <button style="width:75px;height:23px;" id="{fd:Id}" value="{fd:Text}" onmousedown="buttonMouseDown()" onmouseup="buttonMouseUp()">
      <xsl:value-of select="fd:Text"/>
    </button>
  </xsl:template>

</xsl:stylesheet>