//------------------------------------------------------------------------------
//  <copyright file="UserInterface.xaml.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Robotics.Tools.DssLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Microsoft.Ccr.Core;
    using W3C.Soap;
    using design = System.Windows.Forms.Design;
    using wf = System.Windows.Forms;

    /// <summary>
    /// Interaction logic for UserInterface.xaml
    /// </summary>
    public partial class UserInterface : Window
    {
        /// <summary>
        /// Maximum combination box length
        /// </summary>
        private const int MaxComboBoxLength = 10;

        /// <summary>
        /// The minimum zoom level
        /// </summary>
        private const double MinZoomLevel = 0.1;

        /// <summary>
        /// The maximum zoom level
        /// </summary>
        private const double MaxZoomLevel = 1000.0;

        /// <summary>
        /// The dssloganalyzer service
        /// </summary>
        private DssLogAnalyzerService analyzerService;

        /// <summary>
        /// The current zoom level
        /// </summary>
        private double zoomLevel = 1.0;

        /// <summary>
        /// The selection rectangle start value
        /// </summary>
        private double selectionRectangleStart = 0.0;

        /// <summary>
        /// The selection rectangle
        /// </summary>
        private Rectangle selectionRectangle;

        /// <summary>
        /// The timeline y axis offsets
        /// </summary>
        private double[] timelineYOffsets;

        /// <summary>
        /// List of selected rectangles and envelopes
        /// </summary>
        private List<RectangleEnvelopePair> selectedRectanglesAndEnvelopes;

        /// <summary>
        /// The zoom out scalar value
        /// </summary>
        private double zoomOutScalar = 0.95;

        /// <summary>
        /// The zoom in sacalar value
        /// </summary>
        private double zoomInScalar = 1.05;

        /// <summary>
        /// The timeline x axis offset
        /// </summary>
        private double timelineXOffset;

        /// <summary>
        /// The array of envelope and rectangle pairs
        /// </summary>
        private RectangleEnvelopePair[] envelopesAndRectangles = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInterface"/> class.
        /// </summary>
        /// <param name="service">The dssloganalyzer service</param>
        public UserInterface(DssLogAnalyzerService service)
        {
            this.selectionRectangle = new Rectangle
            {
                Height = 2 * this.EnvelopeHeight,
                RadiusX = this.DefaultMargin,
                RadiusY = this.DefaultMargin,
                Stroke = SystemColors.WindowFrameBrush,
                StrokeThickness = this.DefaultMargin,
                Fill = SystemColors.ControlDarkBrush,
                Opacity = 0.5
            };
            Canvas.SetTop(this.selectionRectangle, -this.selectionRectangle.Height);

            this.analyzerService = service;

            this.InitializeComponent();

            this.analyzerService = service;
        }

        /// <summary>
        /// Gets the default margin
        /// </summary>
        private double DefaultMargin
        {
            get { return System.Windows.SystemParameters.BorderWidth; }
        }

        /// <summary>
        /// Gets the text height
        /// </summary>
        private double TextHeight
        {
            get { return System.Windows.SystemParameters.CaptionHeight; }
        }

        /// <summary>
        /// Gets the envelope height
        /// </summary>
        private double EnvelopeHeight
        {
            get { return System.Windows.SystemParameters.HorizontalScrollBarHeight; }
        }

        /// <summary>
        /// Gets the default envelope brush
        /// </summary>
        private Brush DefaultEnvelopeBrush
        {
            get { return SystemColors.ControlDarkDarkBrush; }
        }

        /// <summary>
        /// Gets the selected envelope brush
        /// </summary>
        private Brush SelectedEnvelopeBrush
        {
            get { return SystemColors.HighlightBrush; }
        }

        /// <summary>
        /// Gets the maximum envelope width
        /// </summary>
        private double MaxEnvelopeWidth
        {
            get { return 2.0 * Math.Max(2.0, System.Windows.SystemParameters.ThickVerticalBorderWidth); }
        }

        /// <summary>
        /// Gets the minimum envelope width
        /// </summary>
        private double MinEnvelopeWidth
        {
            get { return Math.Max(2.0, System.Windows.SystemParameters.ThinVerticalBorderWidth); }
        }

        /// <summary>
        /// Show progress bar
        /// </summary>
        internal void ShowProgressBar()
        {
            this.mainProgressBar.Height = System.Windows.SystemParameters.MenuHeight;
            this.mainProgressBar.Width = 5 * System.Windows.SystemParameters.MenuHeight;

            this.mainProgressBarStackPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide progress bar
        /// </summary>
        internal void HideProgressBar()
        {
            this.mainProgressBarStackPanel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Update progress bar
        /// </summary>
        /// <param name="newProgress">The value to update to</param>
        internal void UpdateProgress(double newProgress)
        {
            this.mainProgressBar.Value = newProgress;
        }

        /// <summary>
        /// Display error string
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        internal void DisplayError(string errorMessage)
        {
            this.statusTextBlock.Foreground = SystemColors.HighlightBrush;
            this.statusTextBlock.Text = errorMessage;
        }

        /// <summary>
        /// Display info string
        /// </summary>
        /// <param name="infoMessage">The info message to display</param>
        internal void DisplayInfo(string infoMessage)
        {
            this.statusTextBlock.Foreground = SystemColors.ControlTextBrush;
            this.statusTextBlock.Text = infoMessage;
        }

        /// <summary>
        /// Refresh the window UI
        /// </summary>
        /// <param name="logFileEnvelopes">List of data envelopes</param>
        /// <param name="commonPath">The common log file path</param>
        internal void RefreshUI(Dictionary<string, List<EnvelopeList>> logFileEnvelopes, string commonPath)
        {
            this.mainCanvas.Children.Clear();
            if (logFileEnvelopes == null)
            {
                return;
            }

            var topMargin = this.DefaultMargin;

            var maxTimeStamp = DateTime.MinValue;
            var minTimeStamp = DateTime.MaxValue;

            int groupCount = 0;

            foreach (var list in logFileEnvelopes)
            {
                groupCount += list.Value.Count;
            }

            this.timelineXOffset = 0.0;
            this.timelineYOffsets = new double[groupCount];

            int maxNumEnvelopesInRow = 0;
            int totalEnvelopeCount = 0;
            List<EnvelopeList> envList = null;
            int groupIndex = 0;
            foreach (var fileEnvList in logFileEnvelopes)
            {
                envList = fileEnvList.Value;
                for (int i = 0; i < envList.Count; i++)
                {
                    var list = envList[i];
                    var numEnvelopes = list.Envelopes.Count;

                    if (numEnvelopes == 0)
                    {
                        continue;
                    }

                    totalEnvelopeCount += numEnvelopes;

                    var text = new TextBlock
                    {
                        Text = list.Envelopes[0].Filename.Remove(0, commonPath.Length + 1),
                        Height = this.TextHeight
                    };
                    Canvas.SetLeft(text, this.DefaultMargin);

                    var y = topMargin;
                    Canvas.SetTop(text, y);
                    this.timelineYOffsets[groupIndex] = y;

                    topMargin += this.DefaultMargin + this.TextHeight;

                    this.mainCanvas.Children.Add(text);

                    var formattedText = new FormattedText(
                        text.Text,
                        CultureInfo.CurrentCulture, 
                        FlowDirection.LeftToRight,
                        new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch),
                        text.FontSize, 
                        text.Foreground);

                    this.timelineXOffset = Math.Max(formattedText.Width + this.DefaultMargin, this.timelineXOffset);

                    maxNumEnvelopesInRow = Math.Max(numEnvelopes, maxNumEnvelopesInRow);

                    // work around around for serialization bug where all timestamps are DateTime.MinValue
                    var now = DateTime.UtcNow;

                    if (list.Envelopes[0].Header.TimeStamp < minTimeStamp)
                    {
                        minTimeStamp = list.Envelopes[0].Header.TimeStamp;
                    }

                    if (list.Envelopes[numEnvelopes - 1].Header.TimeStamp > maxTimeStamp)
                    {
                        maxTimeStamp = list.Envelopes[numEnvelopes - 1].Header.TimeStamp;
                    }

                    groupIndex++;
                }
            }

            if (this.timelineSlider.Maximum != totalEnvelopeCount)
            {
                this.timelineSlider.Maximum = totalEnvelopeCount;
                this.timelineSlider.Value = 0;
            }

            var canvasVisibleWidth = this.mainCanvas.ActualWidth;
            var usableWidthForRectables = canvasVisibleWidth - this.timelineXOffset - (2 * this.DefaultMargin);
            long maxTicks = maxTimeStamp.Ticks;
            long minTicks = minTimeStamp.Ticks;
            var envelopeWidth = Math.Min(this.MaxEnvelopeWidth, Math.Max(this.MinEnvelopeWidth, usableWidthForRectables / maxNumEnvelopesInRow));

            var scrollBarRange = this.timelineScrollBar.Maximum - this.timelineScrollBar.Minimum;
            var scrollZeroToOne = 0.0;
            if (scrollBarRange != 0.0)
            {
                // _zoomLevel is always > 0
                var av = (1.0 - (1.0 / this.zoomLevel)) / (double)scrollBarRange;
                var bv = -av * this.timelineScrollBar.Minimum;

                scrollZeroToOne = (av * this.timelineScrollBar.Value) + bv;
            }
            
            this.envelopesAndRectangles = new RectangleEnvelopePair[totalEnvelopeCount];
            var envelopesAndFileNameTimestamps = new long[totalEnvelopeCount];
            int nextEnvelope = -1;

            if (maxTicks > minTicks)
            {
                groupIndex = 0;
                foreach (var fileEnvList in logFileEnvelopes)
                {
                    envList = fileEnvList.Value;
                    for (int i = 0; i < envList.Count; i++)
                    {
                        foreach (var envelope in envList[i].Envelopes)
                        {
                            ++nextEnvelope;
                            this.envelopesAndRectangles[nextEnvelope] = new RectangleEnvelopePair
                            {
                                Envelope = envelope
                            };
                            envelopesAndFileNameTimestamps[nextEnvelope] = envelope.Header.TimeStamp.Ticks;

                            // map onto canvas position
                            var ticks = envelope.Header.TimeStamp.Ticks;
                            var zeroToOneValue = (double)(ticks - minTicks) / (double)(maxTicks - minTicks);
                            var normalizedHorizontalLocation = (zeroToOneValue - scrollZeroToOne) * this.zoomLevel;
                            var mappedTimelinePosition = usableWidthForRectables * normalizedHorizontalLocation;

                            if (mappedTimelinePosition + this.timelineXOffset > canvasVisibleWidth)
                            {
                                continue;
                            }

                            if (mappedTimelinePosition >= -envelopeWidth)
                            {
                                var r = new Rectangle
                                {
                                    Width = Math.Min(envelopeWidth, envelopeWidth + mappedTimelinePosition),
                                    Height = this.EnvelopeHeight,
                                    Fill = this.DefaultEnvelopeBrush,
                                    Tag = envelope
                                };

                                var existingRectangleIndex = this.IndexOfSelectedEnvelope(envelope);
                                if (existingRectangleIndex >= 0)
                                {
                                    this.selectedRectanglesAndEnvelopes[existingRectangleIndex] = new RectangleEnvelopePair
                                    {
                                        Rectangle = r,
                                        Envelope = envelope
                                    };
                                    r.Fill = this.SelectedEnvelopeBrush;
                                }

                                Canvas.SetTop(r, this.timelineYOffsets[groupIndex]);
                                Canvas.SetLeft(r, Math.Max(this.timelineXOffset, mappedTimelinePosition + this.timelineXOffset));

                                this.mainCanvas.Children.Add(r);
                                this.envelopesAndRectangles[nextEnvelope].Rectangle = r;
                            }
                        }

                        groupIndex++;
                    }
                }
            }

            Array.Sort(envelopesAndFileNameTimestamps, this.envelopesAndRectangles);

            this.mainCanvas.Children.Add(this.selectionRectangle);

            if (this.timelineYOffsets.Length > 0)
            {
                var newHeight = this.timelineYOffsets[this.timelineYOffsets.Length - 1];

                if (this.mainCanvas.Height != newHeight)
                {
                    this.mainCanvas.Height = newHeight;                    
                }
            }
        }

        /// <summary>
        /// Prompt for New log folder
        /// </summary>
        /// <param name="resultPort">The result port to post result</param>
        internal void PromptForNewLogFolder(PortSet<string, Fault> resultPort)
        {
            if (MessageBox.Show(
                Properties.Resources.XmlToBinaryConversionWarning,
                Properties.Resources.XmlToBinaryConversionWarningCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                resultPort.Post(this.PresentLoadFileDialog(Properties.Resources.SelectFolderForConvertedBinaryFiles));
            }
            else
            {
                resultPort.Post(Fault.FromException(new Exception(Properties.Resources.UserCanceledLoad)));
            }
        }

        /// <summary>
        /// Returns -1 if envelope was not previously selected
        /// </summary>
        /// <param name="selectedEnvelope">The envelope which has been selected</param>
        /// <returns>Selected envelope index</returns>
        private int IndexOfSelectedEnvelope(Envelope selectedEnvelope)
        {
            if (this.selectedRectanglesAndEnvelopes == null)
            {
                return -1;
            }

            for (int i = 0; i < this.selectedRectanglesAndEnvelopes.Count; i++)
            {
                if (this.selectedRectanglesAndEnvelopes[i].Envelope == selectedEnvelope)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Open dialog event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Routed event arguments</param>
        private void CommandBindingOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.analyzerService.Load(this.PresentLoadFileDialog(Properties.Resources.SelectFolderPrompt));
        }

        /// <summary>
        /// Show the file folder selection dialog
        /// </summary>
        /// <param name="dialogTitle">Dialog title string</param>
        /// <returns>Log file folder path</returns>
        private string PresentLoadFileDialog(string dialogTitle)
        {
            // Configure open file dialog box
            var dlg = new OpenFolderDialog();

            // Show open file dialog box
            var result = dlg.ShowDialog(dialogTitle);

            // Process open file dialog box results
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dlg.FolderPath;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Shutdown analyzer event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Routed event arguments</param>
        private void CommandBindingClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.analyzerService.ShutdownLogAnalyzer();
        }

        /// <summary>
        /// Can execute event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Routed event arguments</param>
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Menu item click handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Routed event arguments</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var winAbout = new About();
            var src = wf.Screen.PrimaryScreen;
            winAbout.Top = (src.Bounds.Height / 2) - winAbout.Height;
            winAbout.Left = (src.Bounds.Width - winAbout.Height) / 2;
            winAbout.ShowDialog();
        }
        
        /// <summary>
        /// Mouse up event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Mouse button event arguments</param>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var top = Canvas.GetTop(this.selectionRectangle);
            var left = Canvas.GetLeft(this.selectionRectangle);
            var width = this.selectionRectangle.Width;

            this.SelectEnvelopesFromGeometry(new RectangleGeometry(new Rect(left, top, width, this.selectionRectangle.Height)));
        }

        /// <summary>
        /// Select envelopes from geometry
        /// </summary>
        /// <param name="geometry">The geometry to select from</param>
        private void SelectEnvelopesFromGeometry(Geometry geometry)
        {
            this.ResetSelectedRectangles();

            var selectedEnvelopes = new EnvelopeList { Envelopes = new List<Envelope>() };

            // do envelope hit test
            VisualTreeHelper.HitTest(
                this.mainCanvas,
                null, 
                hitTestResult =>
                {
                    var r = hitTestResult.VisualHit as Rectangle;
                    if (r != null)
                    {
                        var selectedEnvelope = r.Tag as Envelope;
                        if (selectedEnvelope != null)
                        {
                            selectedEnvelopes.Envelopes.Add(selectedEnvelope);

                            this.AddToSelectedRectangleEnvelopePairs(r, selectedEnvelope);
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(geometry));

            selectedEnvelopes.Envelopes.Sort(new SortByTimestamp());
            this.analyzerService.UpdateSelectedEnvelopes(selectedEnvelopes);
        }

        /// <summary>
        /// Add to the selected rectangle envelope pair list
        /// </summary>
        /// <param name="r">The rectangle to add</param>
        /// <param name="e">The envelope to add</param>
        private void AddToSelectedRectangleEnvelopePairs(Rectangle r, Envelope e)
        {
            this.selectedRectanglesAndEnvelopes.Add(new RectangleEnvelopePair
            {
                 Rectangle = r,
                Envelope = e
            });
            if (r != null)
            {
                r.Fill = this.SelectedEnvelopeBrush;
            }
        }

        /// <summary>
        /// Reset the selected rectangle envelope pair list brushes
        /// </summary>
        private void ResetSelectedRectangles()
        {
            if (this.selectedRectanglesAndEnvelopes != null)
            {
                foreach (var rectangleEnvelopePair in this.selectedRectanglesAndEnvelopes)
                {
                    if (rectangleEnvelopePair.Rectangle != null)
                    {
                        rectangleEnvelopePair.Rectangle.Fill = this.DefaultEnvelopeBrush;
                    }
                }
            }

            this.selectedRectanglesAndEnvelopes = new List<RectangleEnvelopePair>();
            this.selectionRectangle.Width = 0;
        }

        /// <summary>
        /// Select single envelope
        /// </summary>
        /// <param name="selectedRectangle">Selected rectangle</param>
        /// <param name="selectedEnvelope">Selected envelope</param>
        private void SelectSingleEnvelope(Rectangle selectedRectangle, Envelope selectedEnvelope)
        {
            this.ResetSelectedRectangles();
            this.AddToSelectedRectangleEnvelopePairs(selectedRectangle, selectedEnvelope);
            this.analyzerService.UpdateSelectedEnvelopes(new EnvelopeList
            {
                Envelopes = new List<Envelope> { selectedEnvelope }
            });
        }

        /// <summary>
        /// Window size changed event
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (double.IsNaN(this.ActualWidth) == false)
            {
                this.timelineScrollBar.MaxWidth = this.ActualWidth;
            }

            this.analyzerService.RefreshUserInterface();
        }

        /// <summary>
        /// Mouse wheel event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void _mainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var s = Math.Abs(e.Delta / (double)Mouse.MouseWheelDeltaForOneLine);
                if (e.Delta < 0)
                {
                    this.SetZoomLevel(this.zoomOutScalar / s * this.zoomLevel);
                }
                else
                {
                    this.SetZoomLevel(s * this.zoomInScalar * this.zoomLevel);
                }
            }
        }

        /// <summary>
        /// Set zoom level
        /// </summary>
        /// <param name="newZoomLevel">New zoom level</param>
        private void SetZoomLevel(double newZoomLevel)
        {
            this.zoomLevel = Math.Min(Math.Max(newZoomLevel, MinZoomLevel), MaxZoomLevel);
            this.zoomComboBox.Text = string.Format("{0}%", (int)(this.zoomLevel * 100.0));
            this.timelineScrollBar.Maximum = Math.Max(0.0, this.zoomLevel - 1.0);
            this.analyzerService.RefreshUserInterface();
        }

        /// <summary>
        /// Scrollbar value changed event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.analyzerService.RefreshUserInterface();
        }

        /// <summary>
        /// Mouse move event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // draw partially transparent rectangle over the main canvas
                var mousePosition = Mouse.GetPosition(this.mainCanvas);
                var mouseX = Math.Max(this.timelineXOffset - this.DefaultMargin, mousePosition.X);

                var newWidth = mouseX - this.selectionRectangleStart;
                if (newWidth > 0)
                {
                    Canvas.SetLeft(this.selectionRectangle, this.selectionRectangleStart);
                    this.selectionRectangle.Width = newWidth;
                }
                else
                {
                    Canvas.SetLeft(this.selectionRectangle, this.selectionRectangleStart + newWidth);
                    this.selectionRectangle.Width = -newWidth;
                }
            }
        }

        /// <summary>
        /// Mouse down event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(this.mainCanvas);
            if (mousePosition.X >= this.timelineXOffset - this.DefaultMargin)
            {
                this.selectionRectangleStart = mousePosition.X;
                Canvas.SetLeft(this.selectionRectangle, this.selectionRectangleStart);

                // snap to a row
                var snappedY = 0.0;
                for (int i = 0; i < this.timelineYOffsets.Length; i++)
                {
                    if (this.timelineYOffsets[i] - (this.EnvelopeHeight * 0.5) > mousePosition.Y)
                    {
                        break;
                    }

                    snappedY = this.timelineYOffsets[i];
                }

                Canvas.SetTop(this.selectionRectangle, snappedY - (this.EnvelopeHeight * 0.5));
            }
        }

        /// <summary>
        /// Preview mouse wheel event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                this._mainCanvas_MouseWheel(sender, e);
            }
        }

        /// <summary>
        /// Comination box selection changed event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = this.zoomComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                this.ComboBox_SelectionChanged(selectedItem.Content as string);
            }
        }

        /// <summary>
        /// Selection changed processing
        /// </summary>
        /// <param name="selectedText">Selected text</param>
        private void ComboBox_SelectionChanged(string selectedText)
        {
            int integerZoomLevel;

            // TryParse will not throw if conversion fails
            if (selectedText != null && int.TryParse(selectedText.Replace("%", string.Empty), out integerZoomLevel))
            {
                this.SetZoomLevel(integerZoomLevel / 100.0);
            }
            else
            {
                this.ConstrainZoomComboBoxLength();
            }
        }
        
        /// <summary>
        /// Contrain zoom combination box length
        /// </summary>
        private void ConstrainZoomComboBoxLength()
        {
            // don't allow arbitrarily large strings
            if (this.zoomComboBox.Text != null && this.zoomComboBox.Text.Length > MaxComboBoxLength)
            {
                this.zoomComboBox.Text = this.zoomComboBox.Text.Substring(0, MaxComboBoxLength);
            }
        }

        /// <summary>
        /// Zoom combination box key down event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Key event arguments</param>
        private void _zoomComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            this.ConstrainZoomComboBoxLength();
        }

        /// <summary>
        /// Zoom combination box key up event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Key event arguments</param>
        private void _zoomComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            this.ComboBox_SelectionChanged(this.zoomComboBox.Text);
        }

        /// <summary>
        /// Timeline slider value changed event handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Event arguments</param>
        private void _timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // select the 'ith' envelope in chronological order
            int idx = (int)this.timelineSlider.Value;
            if (this.envelopesAndRectangles != null)
            {
                var envelopeAndFilename = this.envelopesAndRectangles[idx];
                this.SelectSingleEnvelope(envelopeAndFilename.Rectangle, envelopeAndFilename.Envelope);
            }
        }
        
        /// <summary>
        /// Right arrow timeline button click handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Routed event arguments</param>
        private void _rightArrowTimelineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.timelineSlider.Value < this.timelineSlider.Maximum)
            {
                this.timelineSlider.Value = this.timelineSlider.Value + 1;
            }
        }

        /// <summary>
        /// Left arrow timeline button click handler
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Routed event arguments</param>
        private void _leftArrowTimelineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.timelineSlider.Value > this.timelineSlider.Minimum)
            {
                this.timelineSlider.Value = this.timelineSlider.Value - 1;
            }
        }

        /// <summary>
        /// An internal class to manage log file folder selection
        /// </summary>
        internal class OpenFolderDialog : design.FolderNameEditor
        {
            /// <summary>
            /// The folder browser dialog
            /// </summary>
            private FolderBrowser folderBrowser = new FolderBrowser();

            /// <summary>
            /// Gets the folder path string
            /// </summary>
            public string FolderPath
            {
                get { return this.folderBrowser.DirectoryPath; }
            }

            /// <summary>
            /// Show the file folder selection dialog
            /// </summary>
            /// <param name="description">Folder description</param>
            /// <returns>Dialog result</returns>
            public wf.DialogResult ShowDialog(string description)
            {
                this.folderBrowser.Style = FolderBrowserStyles.ShowTextBox;
                this.folderBrowser.Description = description;

                return this.folderBrowser.ShowDialog();
            }
        }

        /// <summary>
        /// Class to hold rectangle and enevelope pairs
        /// </summary>
        private class RectangleEnvelopePair
        {
            /// <summary>
            /// Gets or sets the rectangle for this pair
            /// </summary>
            public Rectangle Rectangle { get; set; }

            /// <summary>
            /// Gets or sets the envelope for this pair
            /// </summary>
            public Envelope Envelope { get; set; }
        }

        /// <summary>
        /// Time stamp sorter helper class
        /// </summary>
        private class SortByTimestamp : IComparer<Envelope>
        {
            #region IComparer<Envelope> Members

            /// <summary>
            /// Compare function
            /// </summary>
            /// <param name="x">Left side</param>
            /// <param name="y">Right side</param>
            /// <returns>Compare result</returns>
            public int Compare(Envelope x, Envelope y)
            {
                return x.Header.TimeStamp.CompareTo(y.Header.TimeStamp);
            }

            #endregion
        }
    }
}