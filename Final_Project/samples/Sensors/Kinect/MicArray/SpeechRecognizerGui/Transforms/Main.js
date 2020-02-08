//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Main.js $ $Revision: 2 $
//-----------------------------------------------------------------------

var serviceUrl = self.location.href;
var dictEntryRowId = 0;
var latestEventLogTimestamp = 0;

dssRuntime.init = function()
{
    loadEventLog();
    onGrammarTypeChange();
}

function E(elementId)
{
    return document.getElementById(elementId);
}

// ******************* Event log *******************
function loadEventLog()
{
    var eventLogDiv = E("EventLog");

    setStateUpdateCallback("EventLog", "onEventLogStateUpdated");
    updateState("EventLog", serviceUrl + "?cmd=events&timestamp=" + latestEventLogTimestamp);
}

function onEventLogStateUpdated(state, success)
{
    var eventLogDiv = E("EventLog");
    
    if (success && state.EventsArray && state.Events() != null)
    {
        for (var i = 0; i < state.Events().EventListEntryArray.length; i++)
        {
            var entry = state.Events().EventListEntry(i);
            var textDiv = createDiv("EventLogEntry");
            var startTime = null;
            var list = null;
            var entryCreated = false;
            
            if (entry.Timestamp() > latestEventLogTimestamp)
            {
                latestEventLogTimestamp = entry.Timestamp();
            }

            if (entry.SpeechDetectedArray) {
                entry = entry.SpeechDetected();
                setCssClass(textDiv, "SpeechDetected");

                startTime = parseISO8601(entry.StartTime());
                textDiv.appendChild(createText(formatDateTime(startTime) + ": Speech detected"));

                textDiv.appendChild(createListItem(createText(
                    "Beam Angle: " + entry.Angle()
                )));

                textDiv.appendChild(createListItem(createText(
                    "Sound Source Position Confidence: " + entry.DirectionConfidence()
                )));

                entryCreated = true;
            }
            else if (entry.SpeechRecognizedArray) {
                entry = entry.SpeechRecognized();
                setCssClass(textDiv, "SpeechRecognized");

                startTime = parseISO8601(entry.StartTime());
                textDiv.appendChild(createText(formatDateTime(startTime) + ": Speech recognized"));

                list = createUnsortedList();
                textDiv.appendChild(list);
                list.appendChild(createListItem(createText(
                    "Speech Duration: " + convertTicksToTime(entry.DurationInTicks())
                )));
                list.appendChild(createListItem(createText(
                    "Confidence: " + entry.Confidence()
                )));
                list.appendChild(createListItem(createText(
                    "Text: " + entry.Text()
                )));

                list.appendChild(createListItem(createText(
                    "Beam Angle: " + entry.Angle()
                )));

                list.appendChild(createListItem(createText(
                    "Sound Source Position Confidence: " + entry.DirectionConfidence()
                )));

                entryCreated = true;
            }
            else if (entry.RecognitionRejectedArray) {
                entry = entry.RecognitionRejected();
                setCssClass(textDiv, "RecognitionRejected");

                startTime = parseISO8601(entry.StartTime());
                textDiv.appendChild(createText(formatDateTime(startTime) + ": Speech recognition rejected"));

                list = createUnsortedList();
                textDiv.appendChild(list);
                list.appendChild(createListItem(createText(
                    "Speech Duration: " + convertTicksToTime(entry.DurationInTicks())
                )));

                list.appendChild(createListItem(createText(
                    "Beam Angle: " + entry.Angle()
                )));

                list.appendChild(createListItem(createText(
                    "Sound Source Position Confidence: " + entry.DirectionConfidence()
                )));

                entryCreated = true;
            }
            else if (entry.BeamDirectionChangedArray) {
                entry = entry.BeamDirectionChanged();
                setCssClass(textDiv, "BeamDirectionChangedStyle");

                startTime = parseISO8601(entry.StartTime());
                textDiv.appendChild(createText(formatDateTime(startTime) + ": Beam Direction Changed"));
                
                list = createUnsortedList();
                textDiv.appendChild(list);

                list.appendChild(createListItem(createText(
                    "Beam Angle: " + entry.Angle()
                )));

                list.appendChild(createListItem(createText(
                    "Sound Source Position Confidence: " + entry.DirectionConfidence()
                )));

                entryCreated = true;                
            }
            
            if (entryCreated)
            {
                if (eventLogDiv.childNodes.length > 0)
                {
                    prependChild(eventLogDiv, createHorizontalRow());
                }
                
                prependChild(eventLogDiv, textDiv);
            }
        }
    }

    setTimeout("loadEventLog()", 1000);
}

function onClearEventLogClick()
{
    removeChildren(E("EventLog"));
}

// ******************* Configuration in general *******************
function onGrammarTypeChange()
{
    var selector = E("GrammarType");
    var grammarType = selector[selector.selectedIndex].value;
    
    if (grammarType == "DictionaryStyle")
    {
        loadDictStyleConfig();
    }
    else if (grammarType == "Srgs")
    {
        loadSrgsConfig();
    }
}

// ******************* Dictionary-style configuration *******************
function loadDictStyleConfig()
{
    var configDiv = E("Config");
    removeChildren(configDiv);
    configDiv.appendChild(createText("Loading dictionary configuration..."));
    
    setStateUpdateCallback("DictStyleConfig", "onDictStyleConfigStateUpdated");
    updateState("DictStyleConfig", serviceUrl + "?cmd=speechrecognizerstate");
}

function onDictStyleConfigStateUpdated(state, success)
{
    if (!success)
    {
        return;
    }
    
    var configDiv = E("Config");
    removeChildren(configDiv);

    var configTable = createTable();
    configDiv.appendChild(configTable);

    var configTableHead = createTableHead("DictTableHead");
    configTable.appendChild(configTableHead);
    
    // Table header row
    var headerRow = createRow();
    configTableHead.appendChild(headerRow);
    var headerTextCell = createHeaderCell(createText("Text"));
    headerRow.appendChild(headerTextCell);
    var headerSemanticValueCell = createHeaderCell(createText("Semantic Value"));
    headerRow.appendChild(headerSemanticValueCell);
    headerRow.appendChild(createCell());

    var configTableBody = createTableBody("DictTableBody");
    configTable.appendChild(configTableBody);
    
    // "Add entry" button row
    var addEntryRow = createRow();
    configTableBody.appendChild(addEntryRow);
    addEntryRow.appendChild(createCell());
    addEntryRow.appendChild(createCell());
    var addEntryCell = createCell();
    addEntryRow.appendChild(addEntryCell);
    var addEntryBtn = createButton("AddDictEntryButton", "+");
    addEntryCell.appendChild(addEntryBtn);
    addEntryBtn.onclick = onAddDictEntryButtonClick;
    
    if (state.DictionaryGrammarArray == undefined
        || state.DictionaryGrammar() == null)
    {
        addDictRow("", "");
    }
    else
    {
        // All dictionary entry rows
        var semanticValue = null;
        for (var i in state.DictionaryGrammar().ElemArray)
        {
            if (state.DictionaryGrammar().Elem(i).string(1) != null)
            {
                semanticValue = state.DictionaryGrammar().Elem(i).string(1);
            }
            else
            {
                semanticValue = "";
            }
            
            addDictRow(
                state.DictionaryGrammar().Elem(i).string(0),
                semanticValue
            );
        }
    }
    
    // Save button
    var saveBtn = createButton("SubmitConfig", "Save");
    saveBtn.onclick = onSaveDictConfigButtonClick;
    configDiv.appendChild(saveBtn);
}

function onAddDictEntryButtonClick()
{
    var index = addDictRow("", "");
    E("DictEntryText" + index).focus();
}

function onDelDictEntryButtonClick(rowId)
{
    E("DictTableBody").removeChild(E("DictEntryRow" + rowId));
}

function addDictRow(text, semanticValue)
{
    dictEntryRowId++;
    var rowId = dictEntryRowId;
    var configTableBody = E("DictTableBody");
    
    var row = createRow(configTableBody.childNodes.length, "DictEntryRow" + rowId);
    configTableBody.insertBefore(row, getLastChild(configTableBody));
    
    var textField = createTextField("DictEntryText[]", text, "DictEntryText" + rowId);
    row.appendChild(createCell(textField));
    setCssClass(textField, "DictEntryTextField");
    textField.onkeypress = onDictEntryFieldKeyPress;
    
    var semanticValueField = createTextField(
        "DictEntrySemanticValue[]",
        semanticValue, "DictEntrySemanticValue" + rowId);
    row.appendChild(createCell(semanticValueField));
    setCssClass(semanticValueField, "DictEntryTextField");
    semanticValueField.onkeypress = onDictEntryFieldKeyPress;
    
    var delDictEntryButton = createButton("DelDictEntryButton" + rowId, "-", "DelDictEntryButton");
    delDictEntryButton.onclick = function () {
        onDelDictEntryButtonClick(rowId);
    };
    row.appendChild(createCell(delDictEntryButton));
    
    return rowId;
}

function onSaveDictConfigButtonClick()
{
    // Construct HTTP POST parameters from form
    var textParamName = "DictEntryText";
    var semanticValueParamName = "DictEntrySemanticValue";
    var textFields = document.getElementsByName(textParamName + "[]");
    
    var idStartPos = textParamName.length;
    var parameters = "cmd=SaveDictionary";
    var rowId = 0;
    
    for (var i = 0; i < textFields.length; i++)
    {
        // Text of dictionary entry
        parameters += "&" + textParamName + "[]=" + encodeURIComponent(textFields[i].value);
        
        // Semantic value of dictionary entry
        rowId = textFields[i].id.substr(idStartPos);
        parameters += "&" + semanticValueParamName + "[]=";
        parameters += encodeURIComponent(E(semanticValueParamName + rowId).value);
    }
    
    var saveBtn = E("SubmitConfig");
    saveBtn.disabled = true;
    saveBtn.value = "Saving... Please Wait";
    
    postForm(serviceUrl, parameters, onDictConfigFormSubmitted, true);
}

function onDictConfigFormSubmitted(successful)
{
    if (successful)
    {
        alert("Successfully saved dictionary grammar!");
    }
    else
    {
        alert("Failed to save dictionary grammar!");
    }
    
    var saveBtn = E("SubmitConfig");
    saveBtn.disabled = false;
    saveBtn.value = "Save";
}

function onDictEntryFieldKeyPress(event)
{
    var keyNumber;
    if (window.event)
    {
        // IE
        keyNumber = window.event.keyCode;
    }
    else if (event.which)
    {
        keyNumber = event.which;
    }
    
    if (keyNumber == 13)
    {
        // 13 == enter
        onSaveDictConfigButtonClick();
    }
    
    return true;
}

// ******************* SRGS configuration *******************
function loadSrgsConfig()
{
    var configDiv = E("Config");
    removeChildren(configDiv);
    configDiv.appendChild(createText("Loading SRGS configuration..."));
    
    setStateUpdateCallback("SrgsConfig", "onSrgsConfigStateUpdated");
    updateState("SrgsConfig", serviceUrl + "?cmd=speechrecognizerstate");
}

function onSrgsConfigStateUpdated(state, success)
{
    if (!success)
    {
        return;
    }
    
    var useExistingFile = state.SrgsFileLocationArray != undefined;

    var configDiv = E("Config");
    removeChildren(configDiv);
    
    var configTable = createTable();
    configDiv.appendChild(configTable);
    var configTableBody = createTableBody();
    configTable.appendChild(configTableBody);
    
    // Add file upload controls
    var uploadRow = createRow();
    configTableBody.appendChild(uploadRow);
    
    var uploadRadioBtnCell = createCell(undefined, "SrgsInputCell");
    uploadRow.appendChild(uploadRadioBtnCell);
    var uploadRadioBtn = createRadioButton(
        "SrgsInput",
        "Upload",
        !useExistingFile,
        "SrgsInputUpload"
    );
    uploadRadioBtnCell.appendChild(uploadRadioBtn);
    
    var uploadControlCell = createCell();
    uploadRow.appendChild(uploadControlCell);
    uploadControlCell.appendChild(createLabel(
        "Upload SRGS XML file to mount service:",
        "SrgsInputUpload",
        "SrgsInputUploadLabel"
    ));
    
    var fileUploadField = createFileUploadField("SrgsFile");
    uploadControlCell.appendChild(fileUploadField);
    fileUploadField.onclick = function () {
        switchSrgsInput(true);
    };
    fileUploadField.onchange = onSrgsFileLocationChange;
    
    uploadControlCell.appendChild(createLabel(
        "Mount point save location:",
        "MountPointSaveLocation",
        "MountPointSaveLocationLabel"
    ));
    
    var mountPointSaveLocationField = createTextField("MountPointSaveLocation");
    uploadControlCell.appendChild(mountPointSaveLocationField);
    
    uploadControlCell.appendChild(createHiddenField("cmd", "UploadSrgsFile"));
    
    // Add hidden IFrame for file upload
    if (E("SrgsFileUploadFrame") == undefined)
    {
        var fileUploadFrame = createIFrame("SrgsFileUploadFrame", "SrgsFileUploadFrame", "onSrgsFileUploaded();");
        fileUploadFrame.style.display = "none";
        fileUploadFrame.style.visibility = "hidden";
        fileUploadFrame.src = "about:blank";
        document.body.appendChild(fileUploadFrame);
    }
    
    // Add controls for using existing file
    var useExistingRow = createRow();
    configTableBody.appendChild(useExistingRow);
    
    var useExistingRadioBtnCell = createCell(undefined, "SrgsInputCell");
    useExistingRow.appendChild(useExistingRadioBtnCell);
    useExistingRadioBtn = createRadioButton(
        "SrgsInput",
        "UseExisting",
        useExistingFile,
        "SrgsInputUseExisting"
    );
    useExistingRadioBtnCell.appendChild(useExistingRadioBtn);
    
    var useExistingControlCell = createCell();
    useExistingRow.appendChild(useExistingControlCell);
    useExistingControlCell.id = "UseExistingControlCell";
    useExistingControlCell.appendChild(createLabel(
        "Use existing file on mount service:",
        "SrgsInputUseExisting",
        "SrgsInputUseExistingLabel"
    ));
    
    var fileLocationField = createTextField(
        "SrgsFileLocation",
        useExistingFile ? state.SrgsFileLocation() : ""
    );
    useExistingControlCell.appendChild(fileLocationField);
    fileLocationField.onclick = function () {
        switchSrgsInput(false);
    };
    
    if (useExistingFile)
    {
        addSrgsFileLocationLink(state.SrgsFileLocation());
    }
    
    // Add save button
    var saveBtn = createSubmitButton("SubmitConfig", "Save");//createButton("SubmitConfig", "Save");
    //saveBtn.onclick = onSaveSrgsConfigButtonClick;
    E("ConfigForm").onsubmit = onSaveSrgsConfigButtonClick;
    configDiv.appendChild(saveBtn);
    
    // Fire event changes
    onSrgsInputChange();
}

function addSrgsFileLocationLink(srgsFileLocation)
{
    // Build URL to SRGS file
    var serviceUrlParts = serviceUrl.match(/(https?:\/\/[\w\d\-_\.]+(:\d+)?\/).*/);
    var url = serviceUrlParts[1] + "mountpoint";
    
    if (srgsFileLocation.charAt(0) != "/")
    {
        url += "/";
    }
    
    url += srgsFileLocation;

    // Remove already existing link
    var useExistingControlCell = E("UseExistingControlCell");
    var existingFileLink = E("SrgsFileLocationLink");
    if (existingFileLink != undefined)
    {
        useExistingControlCell.removeChild(existingFileLink);
    }
    
    // Add new link
    existingFileLink = createLink(url, createText(url), "_blank",
        "SrgsFileLocationLink");
    useExistingControlCell.appendChild(existingFileLink);
}

function switchSrgsInput(toUpload)
{
    E("SrgsInputUpload").checked = toUpload;
    E("SrgsInputUseExisting").checked = !toUpload;
}

function onSrgsFileLocationChange()
{
    var saveLocation = "";
    var fileLocationField = E("SrgsFile");
    if (fileLocationField.value != "")
    {
        saveLocation = "/store/" + extractFileName(fileLocationField.value);
    }
    
    E("MountPointSaveLocation").value = saveLocation;
}

function onSaveSrgsConfigButtonClick()
{
    var saveBtn = E("SubmitConfig");
    saveBtn.disabled = true;
    saveBtn.value = "Saving... Please Wait";
    
    if (E("SrgsInputUpload").checked)
    {
        // Post form for file upload
        var configForm = E("ConfigForm");
        configForm.action = serviceUrl;
        return true;
    }
    else
    {
        // Post form for using existing file
        var parameters = "";
        parameters += "cmd=UseExistingSrgsFile";
        parameters += "&SrgsFileLocation=" + encodeURIComponent(E("SrgsFileLocation").value);
        postForm(serviceUrl, parameters, onSrgsConfigFormSubmitted, true);
        return false;
    }
    
}

function onSrgsConfigFormSubmitted(successful, uploadedFile)
{
    if (successful)
    {
        alert("Successfully saved SRGS grammar!");
        
        if (uploadedFile)
        {
            E("SrgsInputUseExisting").checked = true;
            E("SrgsFileLocation").value = E("MountPointSaveLocation").value;
        }
        
        addSrgsFileLocationLink(E("SrgsFileLocation").value);
    }
    else
    {
        alert("Failed to save SRGS grammar!");
    }
    
    var saveBtn = E("SubmitConfig");
    saveBtn.disabled = false;
    saveBtn.value = "Save";
}

function onSrgsFileUploaded()
{
    var frameElement = E("SrgsFileUploadFrame");
    
    var doc = null;
    var xmlRootElement = null;
    try
    {
        if (frameElement.contentDocument)
        {
            doc = frameElement.contentDocument;
            if (doc.childNodes.length > 0)
            {
                xmlRootElement = doc.firstChild;
            }
        }
        else if (frameElement.contentWindow)
        {
            // IE handling
            doc = frameElement.contentWindow.document;
            for (var i = 0; i < doc.XMLDocument.childNodes.length; i++)
            {
                if (doc.XMLDocument.childNodes[i].nodeType == 1)
                {
                    xmlRootElement = doc.XMLDocument.childNodes[i];
                    break;
                }
            }
        }
        else
        {
            // Fallback method
            doc = window.frames["SrgsFileUploadFrame"].document;
            if (doc.childNodes.length > 0)
            {
                xmlRootElement = doc.firstChild;
            }
        }
    }
    catch (exception) {}
    
    if (doc.location.href == "about:blank")
    {
        return;
    }
    
    if (xmlRootElement == null)
    {
        alert("Unable to determine whether save operation was successful.\n\n"
            + "Either the service responded with an unknown message or\n"
            + "your browser was not able to parse it.");
        onSrgsConfigFormSubmitted(false);
    }
    else
    {
        if (xmlRootElement.nodeName == "s:Fault")
        {
            alert("Post failed:\n\n" + convertXmlToString(xmlRootElement, 0));
            onSrgsConfigFormSubmitted(false);
        }
        else
        {
            onSrgsConfigFormSubmitted(true, true);
        }
    }
}


// ******************* Miscellaneous utility functions *******************
function parseISO8601(str)
{
    var regExp = "([+-]?)(\\d{4,})(?:-?(\\d{2})(?:-?(\\d{2})" // Year, month and day
        + "(?:[T ](\\d{2})(?::?(\\d{2})(?::?(\\d{2})" // Hours, minutes and seconds
        + "(?:\\.(\\d+))?)?)?(?:Z|(?:([-+])(\\d{2})(?::?(\\d{2}))?)?)?)?)?)?"; // Milliseconds and timezone offset
        
    var dateArray = str.match(new RegExp(regExp));
    if (!dateArray)
    {
        return null;
    }
    
    var year = dateArray[2];
    if (dateArray[1] == "-") { year *= -1; }
    
    var date = new Date(year, 0, 1);
    if (dateArray[3])
    {
        date.setMonth(dateArray[3] - 1);
    }
    
    if (dateArray[4])
    {
        date.setDate(dateArray[4]);
    }
    
    if (dateArray[5])
    {
        date.setHours(dateArray[5]);
    }
    
    if (dateArray[6])
    {
        date.setMinutes(dateArray[6]);
    }
    
    if (dateArray[7])
    {
        date.setSeconds(dateArray[7]);
    }
    
    if (dateArray[8])
    {
        date.setMilliseconds(Number("0." + dateArray[8]) * 1000);
    }
    
    var offset = 0;
    if (dateArray[9] && dateArray[10] && dateArray[11])
    {
        offset = (Number(dateArray[10]) * 60) + Number(dateArray[11]);
        
        if (dateArray[9] != "-")
        {
            offset *= -1;
        }
    }
    offset -= date.getTimezoneOffset();
    
    date.setTime(date.getTime() + (offset * 60 * 1000));
    return date;
}

Date.prototype.toISO8601String = function ()
{
    var prependZero = function (number) {
        return (number < 10 ? '0' : '') + number;
    }
    
    var secs = Number(
        this.getUTCSeconds() + "."
        + ((this.getUTCMilliseconds() < 100) ? '0' : '')
        + prependZero(this.getUTCMilliseconds())
    );

    var str = "";
    str += this.getUTCFullYear();
    str += "-" + prependZero(this.getUTCMonth() + 1);
    str += "-" + prependZero(this.getUTCDate());
    str += "T" + prependZero(this.getUTCHours());
    str += ":" + prependZero(this.getUTCMinutes());    
    str += ":" + prependZero(secs);
    str += "Z";
    
    return str;
}

function formatDateTime(dateTime)
{
    var prependZero = function (number) {
        return (number < 10 ? '0' : '') + number;
    }

    var str = prependZero(dateTime.getMonth() + 1);
    str += "/" + prependZero(dateTime.getDate());
    str += "/" + dateTime.getFullYear();
    str += " " + prependZero(dateTime.getHours());
    str += ":" + prependZero(dateTime.getMinutes());
    str += ":" + prependZero(dateTime.getSeconds());
    
    return str;
}

function convertTicksToTime(ticks)
{
    var milliseconds = ticks / 10000;
    var seconds = milliseconds / 1000;
    var minutes = Math.floor(seconds / 60);
    seconds %= 60;
    var hours = Math.floor(minutes / 60);
    minutes %= 60;
    
    var str = new Number(seconds).toFixed(2) + " sec";
    if (hours > 0 || minutes > 0)
    {
        str = minutes + " min " + str;
        
        if (hours > 0)
        {
            str = hours + " hr " + str;
        }
    }
    
    return str;
}

function parseUrl(url)
{
var regExp = "(https?://[\\w\\-_]+(:\\d+)?/).*"; // Milliseconds and timezone offset
        
    var dateArray = str.match(new RegExp(regExp));
}

function extractFileName(path)
{
    var backslashStart = path.lastIndexOf("\\");
    var slashStart = path.lastIndexOf("/");
    var fileNameStart = backslashStart > slashStart ? backslashStart : slashStart;
    
    if (fileNameStart == -1)
    {
        return path;
    }
    else
    {
        return path.substr(fileNameStart + 1);
    }
}

function convertXmlToString(element, indentLevel)
{
    var str = "";
    var indentStr = "";
    for (var indentIdx = 0; indentIdx < indentLevel; indentIdx++)
    {
        indentStr += "  ";
    }
    
    if (element.nodeType == 3)
    {
        str = indentStr + element.nodeValue + "\n";
    }
    else if (element.nodeType == 1)
    {
        // Node itself with its attributes
        str += indentStr + "<" + element.nodeName;
        if (element.attributes.length > 0)
        {
            for (var attrIdx = 0; attrIdx < element.attributes.length; attrIdx++)
            {
                var attr = element.attributes.item(attrIdx);
                str += " " + attr.name + "=\"" + attr.value + "\"";
            }
        }
        
        // Child nodes
        if (element.hasChildNodes())
        {
            str += ">\n";
            
            for (var childIdx = 0; childIdx < element.childNodes.length; childIdx++)
            {
                str += convertXmlToString(element.childNodes[childIdx], indentLevel + 1);
            }
            
            // Closing tag
            str += indentStr + "</" + element.nodeName + ">\n";
        }
        else
        {
            str += " />\n";
        }
    }
    
    return str;
}
