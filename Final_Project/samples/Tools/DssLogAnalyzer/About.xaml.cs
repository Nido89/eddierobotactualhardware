//------------------------------------------------------------------------------
//  <copyright file="About.xaml.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Tools.DssLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        public About()
        {
            this.InitializeComponent();

            var versionInfo = Microsoft.Dss.Tools.VersionTools.VersionInformation.CurrentVersion;
            this.versionInfoTxt.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "Version ") +
                versionInfo.DisplayName + string.Format(System.Globalization.CultureInfo.CurrentCulture, " (") + versionInfo.VersionId
                + string.Format(System.Globalization.CultureInfo.CurrentCulture, ")");
            this.productIdTxt.Text = string.Format(System.Globalization.CultureInfo.CurrentCulture, "Product ID: ") + versionInfo.ProductId;
        }

        /// <summary>
        /// Event handler for key down
        /// </summary>
        /// <param name="sender">Button object</param>
        /// <param name="e">Key event arguments</param>
        private void Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Event handler for mouse button down
        /// </summary>
        /// <param name="sender">Mouse button object</param>
        /// <param name="e">Mouse button event arguments</param>
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
