﻿#pragma checksum "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2702DCE788E254979595D12C77C1ED93"
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
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Microsoft.Robotics.Tools.DssLogAnalyzerPlugins.RawViewer {
    
    
    /// <summary>
    /// RawViewerPluginUi
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class RawViewerPluginUi : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox _displayLongContent;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock _rawXmlText;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider _selectFrameSlider;
        
        #line default
        #line hidden
        
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
            System.Uri resourceLocater = new System.Uri("/User.DssLogAnalyzer.Y2011.M10;component/plugins/rawviewer/rawviewerpluginui.xaml" +
                    "", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this._displayLongContent = ((System.Windows.Controls.CheckBox)(target));
            
            #line 14 "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml"
            this._displayLongContent.Checked += new System.Windows.RoutedEventHandler(this._displayLongContent_Checked);
            
            #line default
            #line hidden
            return;
            case 2:
            this._rawXmlText = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this._selectFrameSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 18 "..\..\..\..\Plugins\RawViewer\RawViewerPluginUi.xaml"
            this._selectFrameSlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this._selectFrameSlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

