﻿#pragma checksum "C:\Users\EWHA\Microsoft Robotics Dev Studio 4\samples\UX\XBoxControllerViewer\Page.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FEE6FD3A4DF527B5DDE83C8073131369"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;
using XBoxCtrlViewer;


namespace XBoxCtrlViewer {
    
    
    public partial class Page : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.TextBox Server_Uri;
        
        internal XBoxCtrlViewer.XBoxCtrlGraphic Controller_Graphic;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/User.XBoxCtrlViewer;component/Page.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.Server_Uri = ((System.Windows.Controls.TextBox)(this.FindName("Server_Uri")));
            this.Controller_Graphic = ((XBoxCtrlViewer.XBoxCtrlGraphic)(this.FindName("Controller_Graphic")));
        }
    }
}

