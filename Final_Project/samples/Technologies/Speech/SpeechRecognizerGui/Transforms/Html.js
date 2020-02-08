//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Html.js $ $Revision: 2 $
//-----------------------------------------------------------------------

function createDiv(id)
{
    var div = document.createElement("div");
    div.id = id;
    return div;
}

function createHorizontalRow()
{
    return document.createElement("hr");
}

function createBreak()
{
    return document.createElement("br");
}

function createText(text)
{
    return document.createTextNode(text);
}

function createLink(url, content, target, id)
{
    var link = document.createElement("a");
    link.href = url;
    if (content != undefined)
    {
        link.appendChild(content);
    }
    
    if (target != undefined)
    {
        link.target = target;
    }
    
    if (id != undefined)
    {
        link.id = id;
    }
    return link;
}

function createLabel(text, controlId, id)
{
    var label = document.createElement("label");
    label.setAttribute("for", controlId);
    // Make IE happy as well
    label.htmlFor = controlId;
    label.appendChild(createText(text));
    label.id = id;
    return label;
}

function createTextField(name, value, id)
{
    var field = createNamedElement("input", name, id);
    field.type = "text";
    if (value != undefined)
    {
        field.value = value;
    }
    return field;
}

function createFileUploadField(name, id)
{
    var field = createNamedElement("input", name, id);
    field.type = "file";
    return field;
}

function createHiddenField(name, value, id)
{
    var field = createNamedElement("input", name, id);
    field.type = "hidden";
    field.value = value;
    return field;
}

function createRadioButton(name, value, checked, id)
{
    var button = createNamedElement("input", name, id);
    button.type = "radio";
    button.value = value;
    button.checked = checked;
    button.defaultChecked = checked;
    return button;
}

function createButton(name, value, id)
{
    var button = createNamedElement("input", name, id);
    button.type = "button";
    button.value = value;
    return button;
}

function createSubmitButton(name, value, id)
{
    var button = createNamedElement("input", name, id);
    button.type = "submit";
    button.value = value;
    return button;
}

function createUnsortedList()
{
    return document.createElement("ul");
}

function createListItem(content)
{
    var item = document.createElement("li");
    if (content != undefined)
    {
        item.appendChild(content);
    }
    return item;
}

function createIFrame(name, id, onLoadFunc)
{
    var element = null;
    // IE does not support setting the name and onload attribute at run time on dynamically
    // created elements (http://msdn2.microsoft.com/en-us/library/ms534184.aspx).
    // The only way to set it seems to be to use IE's proprietary createElement()
    // syntax and to let onload call another method.
    try
    {
        var elementStr = '<iframe name="' + name + '"';
        if (onLoadFunc != undefined)
        {
            elementStr += ' onload="' + onLoadFunc + '"';
        }
        elementStr += '>';
        element = document.createElement(elementStr);
    }
    catch (exception) {}
    
    if (!element || element.nodeName.toUpperCase() != "IFRAME")
    {
        // Use W3C standard-compliant method to create element
        element = document.createElement("iframe");
        element.name = name;
        if (onLoadFunc != undefined)
        {
            element.onload = new Function(onLoadFunc);
        }
    }
    
    if (id == undefined)
    {
        element.id = name;
    }
    else
    {
        element.id = id;
    }
    
    return element;
}

function createTable(id)
{
    var table = document.createElement("table");
    if (id != undefined)
    {
        table.id = id;
    }
    return table;
}

function createTableHead(id)
{
    var head = document.createElement("thead");
    if (id != undefined)
    {
        head.id = id;
    }
    return head;
}

function createTableBody(id)
{
    var body = document.createElement("tbody");
    if (id != undefined)
    {
        body.id = id;
    }
    return body;
}

function createRow(rowNumber, id)
{
    var row = document.createElement("tr");
    if (rowNumber != undefined)
    {
        var className = rowNumber % 2 ? "odd" : "even";
        row.setAttribute("class", className);
        row.setAttribute("className", className);
    }
    if (id != undefined)
    {
        row.id = id;
    }
    return row;
}

function createHeaderCell(child)
{
    var cell = document.createElement("th");
    if (child != undefined)
    {
        cell.appendChild(child);
    }
    return cell;
}

function createCell(child, id)
{
    var cell = document.createElement("td");
    if (child != undefined)
    {
        cell.appendChild(child);
    }
    if (id != undefined)
    {
        cell.id = id;
    }
    return cell;
}

function setCssClass(element, className)
{
    element.setAttribute("class", className);
    element.setAttribute("className", className);
}

function createNamedElement(type, name, id)
{
    var element = null;
    // IE does not support setting the name attribute at run time on dynamically
    // created elements (http://msdn2.microsoft.com/en-us/library/ms534184.aspx).
    // The only way to set it seems to be to use IE's proprietary createElement()
    // syntax.
    try
    {
        element = document.createElement('<' + type + ' name="' + name + '">');
    }
    catch (exception) {}
    
    if (!element || element.nodeName.toUpperCase() != type.toUpperCase())
    {
        // Use W3C standard-compliant method to create element
        element = document.createElement(type);
        element.name = name;
    }
    
    if (id == undefined)
    {
        element.id = name;
    }
    else
    {
        element.id = id;
    }
    
    return element;
}

function removeChildren(node)
{
    while (node.lastChild != null)
    {
        node.removeChild(node.lastChild);
    }
}

function prependChild(node, child)
{
    if (node.firstChild)
    {
        node.insertBefore(child, node.firstChild);
    }
    else
    {
        node.appendChild(child);
    }
}
