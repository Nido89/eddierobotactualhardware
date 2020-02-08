//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: Page.xaml.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using System.Xml.Linq;
using System.IO;
using System.Net;
using System.Threading;

namespace XBoxCtrlViewer
{
    public partial class Page : UserControl
    {
        Uri webServiceUri = null;

        public Page()
        {
            InitializeComponent();
            try
            {
                webServiceUri = HtmlPage.Document.DocumentUri;
                Server_Uri.Text = "Connecting ...";
                Dispatcher.BeginInvoke(RefreshControllerState);
            }
            catch (Exception e)
            {
                Server_Uri.Text = "Inner ERROR! :" + e.GetType().Name + " " + e.Message;
            }
        }

        void RefreshControllerState()
        {
            Server_Uri.Text = "Connected";
            if (webServiceUri != null)
            {
                // make the uri unique so that it's not grabbed from IE cache
                Uri uniqueUri = new Uri(webServiceUri.AbsoluteUri + @"/" + DateTime.Now.Ticks.ToString());
                HttpWebRequest _request = (HttpWebRequest)WebRequest.Create(uniqueUri);
                _request.Method = "get";
                _request.BeginGetResponse(new AsyncCallback(ControllerResponseCallback), _request);
            }
        }

        private void ControllerResponseCallback(IAsyncResult asyncResult)
        {
            try
            {
                HttpWebRequest _request = (HttpWebRequest)asyncResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)_request.EndGetResponse(asyncResult);

                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception("HttpStatusCode " +
                        response.StatusCode.ToString() + " was returned.");

                // Read response
                Stream content = response.GetResponseStream();
                StreamReader responseReader = new StreamReader(content);

                string rawResponse = responseReader.ReadToEnd();
                content.Close();

                XElement root = XElement.Parse(rawResponse);
                Dispatcher.BeginInvoke(delegate()
                {
                    Controller_Graphic.ProcessXInputGamepadState(root);
                });

                Thread.Sleep(100);
                Dispatcher.BeginInvoke(RefreshControllerState);

            }
            catch (Exception e)
            {
                Server_Uri.Text = "DR ERROR! :" + e.GetType().Name + " " + e.Message;
            }
        }
    }
}
