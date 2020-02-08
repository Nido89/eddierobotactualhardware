<?xml version="1.0" encoding="utf-8"?>
<!-- 
    This file is part of Microsoft Robotics Developer Studio Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
    $File: iRobot.user.xslt $ $Revision: 12 $
-->
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:s="http://www.w3.org/2003/05/soap-envelope"
    xmlns:roomba="http://schemas.microsoft.com/robotics/2007/01/irobot.user.html"
    xmlns:create="http://schemas.microsoft.com/robotics/2007/01/irobot/create.user.html">

  <xsl:import href="/resources/dss/Microsoft.Dss.Runtime.Home.MasterPage.xslt" />

  <xsl:template match="/">
    <xsl:call-template name="MasterPage">
      <xsl:with-param name="serviceName">
        iRobot® Roomba and Create
      </xsl:with-param>
      <xsl:with-param name="description">
        iRobot Current State
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="s:Header">

  </xsl:template>

  <xsl:template match="roomba:RoombaState">
    <table width="100%" border="0" cellpadding="5" cellspacing="5">
      <form method="POST" action="" name="ConfigurationForm">
        <tr>
          <td align="top">
            <input type="hidden" name="Action" value="iRobotConfig" />

            <table>
              <tr>
                <th colspan="3" class="Major">Configuration</th>
              </tr>
              <xsl:call-template name="roomba:SerialPortConfig"/>
            </table>
          </td>
        </tr>
      </form>
      <tr>
        <td align="top">
          <xsl:apply-templates select="roomba:SongDefinitions"/>
        </td>
      </tr>
      <tr>
        <td align="top">
          <table width="100%">
            <tr>
              <th colspan="3">
                All Sensor Readings
              </th>
            </tr>
            <tr>
              <td size="10">Status</td>
              <td size="21">
                <xsl:value-of select="roomba:Mode"/>
              </td>
            </tr>
            <tr>
              <th width="30%">
                Last Updated:
              </th>
              <td width="70%">
                <xsl:value-of select="roomba:LastUpdated"/>
              </td>
            </tr>
            <xsl:if test="roomba:FirmwareDate != '0001-01-01T00:00:00'">
              <tr class="odd">
                <th width="30%">
                  Firmware Date:
                </th>
                <td width="70%">
                  <xsl:value-of select="roomba:FirmwareDate"/>
                </td>
              </tr>
            </xsl:if>
            <xsl:if test="roomba:IRobotModel != 'NotSpecified'">
              <tr>
                <th colspan="2">
                  Sensors
                </th>
              </tr>
              <tr class="odd">
                <th>
                  Bumps Wheel Drops:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:BumpsWheeldrops"/>
                </td>
              </tr>
              <tr>
                <th>
                  Wall:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:Wall"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Cliff Left:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:CliffLeft"/>
                </td>
              </tr>
              <tr>
                <th>
                  Cliff Front Left:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:CliffFrontLeft"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Cliff Front Right:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:CliffFrontRight"/>
                </td>
              </tr>
              <tr>
                <th>
                  Cliff Right:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:CliffRight"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Virtual Wall:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:VirtualWall"/>
                </td>
              </tr>
              <tr>
                <th>
                  Motor Overcurrents:
                </th>
                <td>
                  <xsl:value-of select="roomba:Sensors/roomba:MotorOvercurrents"/>
                </td>
              </tr>
              <xsl:if test="roomba:IRobotModel = 'Roomba'">
                <tr class="odd">
                  <th>
                    Dirt Detector Left:
                  </th>
                  <td>
                    <xsl:value-of select="roomba:Sensors/roomba:DirtDetectorLeft"/>
                  </td>
                </tr>
                <tr>
                  <th>
                    Dirt Detector Right:
                  </th>
                  <td>
                    <xsl:value-of select="roomba:Sensors/roomba:DirtDetectorRight"/>
                  </td>
                </tr>
              </xsl:if>
              <tr>
                <th colspan="2">
                  Pose
                </th>
              </tr>
              <tr class="odd">
                <th>
                  Remote Control Command:
                </th>
                <td>
                  <xsl:value-of select="roomba:Pose/roomba:RemoteControlCommand"/>
                </td>
              </tr>
              <tr>
                <th>
                  Buttons:
                </th>
                <td>
                  <xsl:if test="roomba:IRobotModel = 'Roomba'">
                    <xsl:value-of select="roomba:Pose/roomba:ButtonsRoomba"/>
                  </xsl:if>
                  <xsl:if test="roomba:IRobotModel = 'Create'">
                    <xsl:value-of select="roomba:Pose/roomba:ButtonsCreate"/>
                  </xsl:if>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Distance:
                </th>
                <td>
                  <xsl:value-of select="roomba:Pose/roomba:Distance"/>
                </td>
              </tr>
              <tr>
                <th>
                  Angle:
                </th>
                <td>
                  <xsl:value-of select="roomba:Pose/roomba:Angle"/>
                </td>
              </tr>
              <tr>
                <th colspan="2">
                  Power
                </th>
              </tr>
              <tr class="odd">
                <th>
                  Charging State:
                </th>
                <td>
                  <xsl:value-of select="roomba:Power/roomba:ChargingState"/>
                </td>
              </tr>
              <tr>
                <th>
                  Voltage (mV):
                </th>
                <td>
                  <xsl:value-of select="roomba:Power/roomba:Voltage"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Current (mA):
                </th>
                <td>
                  <xsl:value-of select="roomba:Power/roomba:Current"/>
                </td>
              </tr>
              <tr>
                <th>
                  Temperature (Celsius):
                </th>
                <td>
                  <xsl:value-of select="roomba:Power/roomba:Temperature"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Charge (mAh):
                </th>
                <td>
                  <xsl:value-of select="roomba:Power/roomba:Charge"/>
                </td>
              </tr>
              <tr>
                <th>
                  Capacity (mAh):
                </th>
                <td>
                  <xsl:value-of select="roomba:Power/roomba:Capacity"/>
                </td>
              </tr>
            </xsl:if>
            <xsl:if test="roomba:IRobotModel = 'Create'">
              <tr>
                <th colspan="2">
                  Cliff Detail
                </th>
              </tr>
              <tr class="odd">
                <th>
                  Wall Signal:
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:WallSignal"/>
                </td>
              </tr>
              <tr>
                <th>
                  Cliff Left Signal (0-4095):
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:CliffLeftSignal"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Cliff Front Left Signal (0-4095):
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:CliffFrontLeftSignal"/>
                </td>
              </tr>
              <tr>
                <th>
                  Cliff Front Right Signal (0-4095):
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:CliffFrontRightSignal"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Cliff Right Signal (0-4095):
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:CliffRightSignal"/>
                </td>
              </tr>
              <tr>
                <th>
                  User Digital Inputs (0-31):
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:UserDigitalInputs"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  User Analog Input (0-1023):
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:UserAnalogInput"/>
                </td>
              </tr>
              <tr>
                <th>
                  Charging Sources Available:
                </th>
                <td>
                  <xsl:value-of select="roomba:CliffDetail/create:ChargingSourcesAvailable"/>
                </td>
              </tr>
              <tr>
                <th colspan="2">
                  Telemetry
                </th>
              </tr>
              <tr class="odd">
                <th>
                  Current Roomba Mode:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:OIMode"/>
                </td>
              </tr>
              <tr>
                <th>
                  Song Number:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:SongNumber"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Song Playing:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:SongPlaying"/>
                </td>
              </tr>
              <tr>
                <th>
                  Number Of Stream Packets:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:NumberOfStreamPackets"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Requested Velocity:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:RequestedVelocity"/>
                </td>
              </tr>
              <tr>
                <th>
                  Requested Radius:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:RequestedRadius"/>
                </td>
              </tr>
              <tr class="odd">
                <th>
                  Requested Right Velocity:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:RequestedRightVelocity"/>
                </td>
              </tr>
              <tr>
                <th>
                  Requested Left Velocity:
                </th>
                <td>
                  <xsl:value-of select="roomba:Telemetry/create:RequestedLeftVelocity"/>
                </td>
              </tr>
            </xsl:if>

          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="roomba:SerialPortConfig">
    <xsl:if test="roomba:Mode = 'Uninitialized'">
      <tr>
        <td colspan="2" style="color:red" align="center">
          The iRobot
          <xsl:if test="roomba:IRobotModel != 'NotSpecified'">
            <xsl:value-of select="roomba:IRobotModel"/>
          </xsl:if>
        is not connected. Please configure below.</td>
      </tr>
    </xsl:if>
    <xsl:if test="roomba:Name != ''">
      <tr>
        <td size="10">Name</td>
        <td size="21">
          <input type="text" name="Name" size="20">
            <xsl:attribute name="value">
              <xsl:value-of select="roomba:Name"/>
            </xsl:attribute>
          </input>
        </td>
        <td size="10">The name of this iRobot</td>
      </tr>
    </xsl:if>
    <tr>
      <td size="10">Serial Port</td>
      <td size="21">
        <input type="text" name="SerialPort" size="20">
          <xsl:attribute name="value">
            <xsl:value-of select="roomba:SerialPort"/>
          </xsl:attribute>
          <xsl:if test="roomba:SerialPort = '0'">
            <xsl:attribute name="style">
              <xsl:text>background:#f67f7f;</xsl:text>
            </xsl:attribute>
          </xsl:if>
        </input>
      </td>
      <td size="10">COM Port connected to iRobot</td>
    </tr>
    <tr>
      <td size="10">Baud Rate</td>
      <td size="21">
        <input type="text" name="BaudRate" size="20">
          <xsl:attribute name="value">
            <xsl:value-of select="roomba:BaudRate"/>
          </xsl:attribute>
        </input>
      </td>
      <td>(0:default)</td>
    </tr>
    <tr>
      <td size="10">Polling Interval</td>
      <td size="21">
        <input type="text" name="PollingInterval" size="20">
          <xsl:attribute name="value">
            <xsl:value-of select="roomba:PollingInterval"/>
          </xsl:attribute>
        </input>
      </td>
      <td size="10">(0:default; -1:Off; # ms)</td>
    </tr>
    <tr>
      <td size="31" colspan="2">
        <INPUT TYPE="checkbox" ID="cbWaitForConnect" Name="WaitForConnect">
          <xsl:if test="roomba:WaitForConnect = 'true'">
            <xsl:attribute name="CHECKED">CHECKED</xsl:attribute>
          </xsl:if>
          Always wait for Connect.
        </INPUT>
      </td>
    </tr>
    <tr></tr>
    <tr>
      <th>Connection Type</th>
      <th>iRobot Model</th>
      <th>Maintain Status</th>
    </tr>
    <tr>
      <td align="center" size="21">
        <select name="ConnectionType" size="4">
          <xsl:attribute name="value">
            <xsl:value-of select="roomba:ConnectionType"/>
          </xsl:attribute>
          <option>
            <xsl:if test="roomba:ConnectionType = 'RoombaSerialPort'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            RoombaSerialPort
          </option>
          <option>
            <xsl:if test="roomba:ConnectionType = 'CreateSerialPort'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            CreateSerialPort
          </option>
          <option>
            <xsl:if test="roomba:ConnectionType = 'RooTooth'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            RooTooth
          </option>
          <option>
            <xsl:if test="roomba:ConnectionType = 'BluetoothAdapterModule'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            BluetoothAdapterModule
          </option>
        </select>
      </td>
      <td align="center" size="21">
        <select name="IRobotModel" size="3">
          <xsl:attribute name="value">
            <xsl:value-of select="roomba:IRobotModel"/>
          </xsl:attribute>
          <option>
            <xsl:if test="roomba:IRobotModel = 'NotSpecified'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            NotSpecified
          </option>
          <option>
            <xsl:if test="roomba:IRobotModel = 'Roomba'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            Roomba
          </option>
          <option>
            <xsl:if test="roomba:IRobotModel = 'Create'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            Create
          </option>
        </select>
      </td>
      <td align="center" size="15">
        <select name="MaintainMode" size="4">
          <xsl:attribute name="value">
            <xsl:value-of select="roomba:MaintainMode"/>
          </xsl:attribute>
          <option>
            <xsl:if test="roomba:MaintainMode = 'NotSpecified'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            Not Specified
          </option>
          <option>
            <xsl:if test="roomba:MaintainMode = 'Passive'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            Passive
          </option>
          <option>
            <xsl:if test="roomba:MaintainMode = 'Safe'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            Safe
          </option>
          <option>
            <xsl:if test="roomba:MaintainMode = 'Full'">
              <xsl:attribute name="selected">true</xsl:attribute>
            </xsl:if>
            Full
          </option>
        </select>
      </td>
    </tr>
    <xsl:if test="roomba:IRobotModel != 'Roomba'">
    <tr>
      <td align="top" colspan="3">
        <xsl:apply-templates select="roomba:CreateNotifications"/>
      </td>
    </tr>
    </xsl:if>
    <tr>
      <td>
        <input id="Button1" type="submit" value="Connect" name="buttonOk" title="Update and Connect"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="roomba:CreateNotifications">
    <table>
      <th>Create Notifications</th>
      <xsl:apply-templates select="roomba:CreateSensorPacket"/>
    </table>
  </xsl:template>

  <xsl:template match="roomba:CreateSensorPacket">
    <tr>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="position() mod 2 = 0">
            <xsl:text>even</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>odd</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <td>
        <xsl:value-of select="."/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="roomba:SongDefinitions">
    <table>
      <tr>
        <th>Song</th>
        <th>Notes</th>
      </tr>
      <xsl:apply-templates select="roomba:CmdDefineSong"/>
    </table>
  </xsl:template>

  <xsl:template match="roomba:CmdDefineSong">
    <tr>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="position() mod 2 = 0">
            <xsl:text>even</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>odd</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <td>
        <xsl:value-of select="roomba:SongNumber"/>
      </td>
      <td>
        <xsl:for-each select="roomba:Notes/roomba:RoombaNote">
          <xsl:if test="position() != 1">
            <xsl:text>, </xsl:text>
          </xsl:if>
          <xsl:value-of select="substring-before(roomba:Tone,'_Hz')"/>
          <xsl:text> - </xsl:text>
          <xsl:value-of select="roomba:Duration"/>
        </xsl:for-each>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
