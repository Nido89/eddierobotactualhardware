   M S B A M L   `   `   `  ����!  User.DssLogAnalyzer.Y2011.M10;    5Microsoft.Robotics.Tools.DssLogAnalyzer.UserInterface   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      $
��Auto��5      $
��Auto��6     � Title$  DSS Log Analyzer��6     J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command$ ApplicationCommands.Open��5      5        5	   
   -   5        $ ApplicationCommands.Close��5	      5      .��5      � $
��Auto��6   $
��Auto��6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   o� -   5        $��	_mainMenu��5      $��0��6   $	��Top=�6-   .@�5      m� $��_File��6   .@�5      m� $��_Open��6    m� Command$ ApplicationCommands.Open��6*   6O   m� 5      $��E_xit��6   $ ApplicationCommands.Close��6*   6P   5      m� 5      $��_Help��6   .@�5      m� -   5        $��_About��5      6D   5      5      � 5   
   $��1��6   .+�5      �� -   5        $��mainProgressBarStackPanel��5       �� Orientation$ 
Horizontal=�6:   $y�Hidden=�6S   $
��Auto��6g   $
��Auto��6t   $��Center=�6�   $��Center=�6�   .+�5      �� -   5        $��mainProgressBarLabel��5      $��Center=�64   $��Center=�6V   .��6{   Conversion progress...6�   � 5      -   5        $��mainProgressBar��5      $��Center=�65   $��Center=�6W   5       �� 5!      -   5        $
��Auto=�5"      $
��Auto=�5#      .��5&      �� -	   5        $��
mainCanvas��5&      $
��Auto��6+   $
��Auto��68   
������ $	��Top=�5'   V   $��Stretch=�6n   5)      5*      � 5+   
   $��2��6   .+�5,      �� $��Bottom=�6   .+�5-      �  � ColumnDefinitions 5.      �� 5/      $
��Auto �6+   �� 50      $��* �6+   �� 51      $
��Auto �6+   .+�53      �� -
   5        $��leftArrowTimelineBtn��53   -   $��0��6   #��Z��6I   .��54      �� $A��;/DssLogAnalyzer.Y2009.M11;component/Resources/LeftArrow.png��6     � ToolTip$ Select previous envelope��6e   55      �� 56      -   5        $��timelineSlider��56   -   $��1��6   $
��Auto��6C   $
��Auto��6P    �� Orientation$ 
Horizontal=�6^   	 � Maximum$	 10Q�6�   $ Global timeline slider��6�   6�   �� 57      -   5        $��rightArrowTimelineBtn��57   -   $��2��6   #��Z��6J   .��58      �� $B��</DssLogAnalyzer.Y2009.M11;component/Resources/RightArrow.png��6    $ Select next envelope��6f   59      5:      �� 5;      -   5        $��timelineScrollBar��5;      $
��Auto��65   $
��Auto��6B   
 �� Orientation$
 
Horizontal=�6P    �� ViewportSize$ 1Q�6�   $	 0Q�6�   �� 5<      -   5        $��	statusBar��5<      .@�5=      �� .��5>      �� -   5        $��statusTextBlock��5>   [   $
��Left=�6$   $��Stretch=�6?   6[   5?      �� 5@      $��Right=�6$   $ Change zoom level��6@   .��5A      �� $ 
Horizontal=�6%   .+�5B      �� 6&   �� 5C      -   5        $��zoomComboBox��5C   '   $
��Auto��6;    �� Text$
 100%��6H    �� SelectedIndex$ 6��6T   $ Change zoom level��5D   T    �� 
IsEditable . .@�5E   "   �� .��6/   2000%66   �� 5F   "   .��6/   1000%66   �� 5G   "   .��6/   400%65   �� 5H   "   .��6/   200%65   �� 5I   "   .��6/   150%65   �� 5J   "   .��6/   125%65   �� 5K   "   .��6/   100%65   �� 5L   "   .��6/   75%64   �� 5M   "   .��6/   50%64   �� 5N   "   .��6/   25%64   5O      5P      5Q      5R      5S      5T      5U      5V      