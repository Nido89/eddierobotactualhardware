//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Ajax.js $ $Revision: 2 $
//-----------------------------------------------------------------------

var xmlHttpRequestList = new Object();
var stateUpdateCallbacks = new Object();

// General functions
function createXmlHttpRequest()
{
    try
    {
        // Microsoft Internet Explorer 6.0+
        return new ActiveXObject("Msxml2.XMLHTTP");
    }
    catch (exception) {}

    try
    {
        // Microsoft Internet Explorer 5.5+
        return new ActiveXObject("Microsoft.XMLHTTP");
    }
    catch (exception) {}

    try
    {
        // Other browsers
        return new XMLHttpRequest();
    }
    catch (exception)
    {
        return null;
    }
}

// HTTP POST-related functions
function postForm(url, parameters, callbackFunction, showResponseWindowOnError)
{
    var request = createXmlHttpRequest();
    if (request == null)
    {
        return;
    }
    requestId = "formPost" + (new Date().getTime());
    xmlHttpRequestList[requestId] = request;
    
    request.open("POST", url, true);
    request.onreadystatechange = function()
    {
        processXmlHttpFormPostResponse(requestId, callbackFunction, showResponseWindowOnError);
    }
    request.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    request.setRequestHeader("Content-length", parameters.length);
    request.send(parameters);
}

function processXmlHttpFormPostResponse(requestId, callbackFunction, showResponseWindowOnError)
{
    try
    {
        if (xmlHttpRequestList[requestId].readyState == 4)
        {
            if (xmlHttpRequestList[requestId].status == 200)
            {
                callbackFunction(true);
            }
            else
            {
                if (showResponseWindowOnError)
                {
                    alert("Post failed:\n\n" + xmlHttpRequestList[requestId].responseText);
                }
                    
                callbackFunction(false);
            }
            
            xmlHttpRequestList[requestId] = undefined;
        }
    }
    catch (exception)
    {
        xmlHttpRequestList[requestId] = undefined;
    }
}

// HTTP GET-related functions
function processXmlHttpResponse(stateName)
{
    try
    {
        if (xmlHttpRequestList[stateName].readyState == 4)
        {
            if (xmlHttpRequestList[stateName].status == 200)
            {
                var response = xmlHttpRequestList[stateName].responseXML;
                var state = objectify(response.documentElement);
          
                stateUpdateCallbacks[stateName](state, true);
            }
            else
            {
                stateUpdateCallbacks[stateName](null, false);
            }
            
            xmlHttpRequestList[stateName] = undefined;
        }
    }
    catch(exception)
    {
        xmlHttpRequestList[stateName] = undefined;
    }
}

function updateState(stateName, url)
{
    if (xmlHttpRequestList[stateName] == undefined)
    {
        var request = createXmlHttpRequest()
        if (request == null)
        {
            return;
        }
        xmlHttpRequestList[stateName] = request;
    
        request.open("GET", url, true);
        request.onreadystatechange = function()
        {
            try
            {
                processXmlHttpResponse(stateName);
            }
            catch (exception) {}
        }
        request.setRequestHeader("If-Modified-Since", new Date(0));
        request.send(null);
    }
}

function updateStateTimed(interval, stateName, url)
{
    setTimeout('updateState("' + stateName + '", "' + url +'")', interval);
}

function setStateUpdateCallback(stateName, callbackName)
{
    stateUpdateCallbacks[stateName] = new Function(
        "state", "success", callbackName + "(state, success);"
    );
}

function createArrayAccessFunction(arrayName)
{
    return (function (index) {
        if (index == undefined)
        {
            index = 0;
        }
        return this[arrayName][index];
    });
}

function objectify(xmlElement)
{
    // Element has only one child and it is of type text? This would
    // be the element's text then
    if (xmlElement.childNodes.length == 1
        && xmlElement.firstChild.nodeType == 3)
    {
        return xmlElement.firstChild.nodeValue;
    }
    else if (xmlElement.childNodes.length > 0)
    {
        // Traverse children
        var obj = new Object();
        var child = xmlElement.firstChild;
        
        while (child != null)
        {
            if (obj[child.tagName] == undefined)
            {
                obj[child.tagName] = createArrayAccessFunction(child.tagName + "Array");
                obj[child.tagName + "Array"] = new Array();
            }
            obj[child.tagName + "Array"].push(objectify(child));
      
            child = getNextSibling(child);
        }
    
        return obj;
    }
    else
    {
        return null;
    }
}

function getNextSibling(element)
{
    var sibling = element.nextSibling;
  
    // Skip white-space text nodes (which are introduced
    // by some browsers other than IE while parsing XML)
    while (sibling != null && sibling.nodeType != 1)
    {
        sibling = sibling.nextSibling;
    }
  
    return sibling;
}

function getLastChild(element)
{
    var child = element.lastChild;
    
    // Skip white-space text nodes (which are introduced
    // by some browsers other than IE while parsing XML)
    while (child != null && child.nodeType != 1)
    {
        child = child.previousSibling;
    }
    
    return child;
}
