   M S B A M L   `   `   `  ���� I  EDocumentFinder, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:DocumentFinderDocumentFinder      DocumentFinder.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:DocumentFinder     -     � Title$  
MainWindow�� � WindowStyle$
 None=� � WindowState$ 	Maximized=����#ffffff � 	Resources    ButtonTemplate �� 
TargetType �� CornerRadius  HeaderButtonTemplate ��Name �� Color �� Triggers	 P� Property
 P� Value �� 
TargetName �� Property �� Value  AutoButtonTemplate  PickButtonTemplate  PickButtonStyle �� 
TargetType �� Triggers  ButtonExitTemplate  ButtonStyle  ListViewStyle  TextBoxStyle 	 CheckboxStyle 
 TextBlockStyle  ContextMenuStyle 
 cmScan m� IsCheckable��%�  &	        &	 W     &	 m    &	 �    &	     &	 	    &	 M    &	 <    &	 �    &		 �    &	
 l    &	 %    &	 �    7  �� " ��.R��� $ 30��$��50��$	��170��#��z��._��� $��Center=�$��Center=��� " ��.R��� $ BorderBackground������ $ #022140��._��� $��Center=�$��Center=� P� 	 � {�
 .  . ��� $ BorderBackground�� � �� �� $ #022140��P� 	 �@J�IsMouseOver
 . . ��� $ BorderBackground�� � �� �� $ #265077���� " ��.R��� $ 30��$��50��$	��300��#��z��._��� $��Center=�$��Center=��� " ��.R��� $ 15��$��30��$��80��#��z��._��� $��Center=�$��Center=��� " ��.���  � �� �#022140��  � �� �#ffffff��  � ��$ 	Agency FB)���  � ��$
 Bold"���  � ��$ 18(� P� 	 �@J�IsMouseOver
 . . ���  � �� �#265077��  � �� �#ffffff�� " ��.R��� $ 20��#��z��._��� $��Center=�$��Center=��� " ��.���  � �� �#022140��  � �� �#ffffff��  � ��$ 	Agency FB)���  � ��$
 Bold"���  � ��$ 25(� P� 	 �@J�IsMouseOver
 . . ���  � �� �#265077��  � �� �#ffffff�� " ��.���  � ��$ Center=���  � ��$ Center=���  � �� �#18173f��  � ��
 ���� ��  � ��$ 	Agency FB)���  � ��$ 20(��� " ��.���  � ��$ Center=���  � ��$ Center=���  � �� �#18173f��  � ��
 ���� ��  � ��$ 	Agency FB)���  � ��$ 20(���  � �� �#0f0f35�� " ��.���  � �� �#18173f��  � ��$ 	Agency FB)���  � ��$ 20(���  � ��$ Center=���  � ��$ Bottom=���  � ��$ Center=���  � ��$ Center=��� " ��.���  � �� �#18173f��  � ��$ 	Agency FB)���  � ��$ 20(���  � ��$ Center=���  � ��$ Bottom=���  � ��$ 0q���  � |� .  �� " ��.���  � �� �#ffffff��  � �� �#18173f��  � ��$ 	Agency FB)���  � ��$ 20(��� 8��  $y�Hidden=�.@�m� -   $��Scan for files��m� -   $��Convert files��m� -   $��Scan and convert files��m� -   $��Multi Threading�� . .��]� -   $��mainUiHolder��.+��� $��Bottom=�.@��� $��Stretch=�$��25��.���  � ColumnDefinitions �� $
��auto ��� $��* ��� $��70��$��70��.+��� -   $��	statusBar��$��0��$��	Agency FB)�$
��Bold"�$
��Wrap=�$
��Left=�$��15(��� -   $��progressStatus��$��1��$
��Wrap=�$��5,0,5,0q�$��	Agency FB)�$
��Bold"�$��Right=�$��15(�� -	    �Name$ progressBar��$��2�� � Value$ 0Q�$��15��$��50�� � Minimum$ 0Q� � Maximum$	 100Q�$��Right=��  � RowDefinitions �� $��50 ��� $	��300 ��� $��* � �� $��* ��� $��* �.+�� ���#022140$��2�� �� $	��200 ��� $	��140 ��� $	��100 ��� $��* ��� $	��100 ��� $	��100 �.+��� $��Image/search.png��$	��100��$��40��$��20,0,0,0q�$��0��$��Center=�$
��Left=��� $��	DocFinder�����#ffffff$��0��$��Center=�$��Right=�$��	Agency FB)�$
��Bold"�$��40(��� -
    ��Name$ 
OptionsBtn��$��1��#��[ $��	10,0,10,0q�$��0q�$��OPTIONS��#��[ �� -   $ HelpBtn��$��2��#��[ $��	10,0,10,0q�$��0q�$
��HELP��#��[ �� -   $��btnStopWork��#��[ #��[ $
��STOP��$��0q�$y�Hidden=�$��70��$��40�����#D82148$��4��$��Right=�$��0,0,10,0q��� -   #��[ #��[ $
��EXIT��$��0q�$��70��$��40�����#D82148$��5��$��Right=�$��0,0,10,0q�� $��0��$��1�� �� $	��100 ��� $��30 ��� $��30 ��� $��30 ��� $��30 ��� $��30 ��� $��* �.+�� $��0�� �� $��* ��� $��* �.+��� -   $��btnFindAuto��#��[ $��SCAN AND CONVERT��#��[ $y�	Collapsed=�$��2��$��Stretch=�$��Stretch=�$��Center=��� -   $��btnFind��#��[  $
��SCAN��#��[ $��0��$��Stretch=�$��Stretch=�$��Center=��� -   $��
btnConvert��#��[  $��CONVERT��#��[ $��1���� -   $��	ocrOption��$��1��$��Use OCR��#��[	 �� -   $��cbxCopy��$��2��$��Copy original files��#��[	 �� -   $��autoScanConvert��$��3��$ ��Automatic Scan and Convert��#��[	 $��0,7,0,0q�$	��Top=��� -   $��conversionDestination��$��4��$��20(�$��	Agency FB)����#18173f$��Center=�$��Stretch=��� -   $��btnPick��#��[ $
��PICK��$��5��#��[ $��Center=��� -   $	��tb1��$��6��$��Stretch=�$��Stretch=�$��50,50,50,50q�$y�	Collapsed=�#��[ " �� VerticalScrollBarVisibility$
 Auto=�$ �� HorizontalScrollBarVisibility$
 Auto=�.��� $��1��$��1�� �� $��80 ��� $��50 ��� $	��100 ��� $��* �.+��� -   $��searchTb��$��50�� �� TextAlignment$ Center=�$��Stretch=�$��Center=�$��
50,20,50,0q�$��30(�$��0��#��[ .���� -   $��cbxCS��$��1��$��Case sensitive��$��Center=�#��[	 �� -   $��	btnSearch��#��[ #��[  $��SEARCH��$��2��$��2��� $��2��$��2��.+�&   System.Windows.Controls.DataGrid  -     Name$ searchResultTB��   AutoGenerateColumns .  ���#022140���#ffffff$��	Agency FB)�$��24(�   
IsReadOnly . $��50q�$��0q����#ffffff   CanUserSortColumns .     SelectionMode$ Single=�    GridLinesVisibility$
  None=�����  =  7System.Windows.Controls.Primitives.DataGridColumnHeader*  $System.Windows.Controls.DataGridCell��%  '        '  ?     �� "  .���  � ��$ Center=���  � ��$
 Bold"��� "  .���  �@�ToolTip �� .+��� $��
File path:��$��10(�$
��Bold"��� ���� *OriginalPath+��  � ��$
 Auto=���  � ��$
 Auto=���  � �� . !   Columns! 4  .System.Windows.Controls.DataGridTemplateColumn  ,  &System.Windows.Controls.DataGridColumn"   Width5  /System.Windows.Controls.DataGridLengthConverter$" 20 #   CellTemplate# �� .R��� ���� *Image+0  *System.Windows.Controls.DataGridTextColumn  $" * $   Header$$ 	File name��1  +System.Windows.Controls.DataGridBoundColumn%   Binding% �� *
FileName+  $" * $$ Terms found��% �� *	Display+