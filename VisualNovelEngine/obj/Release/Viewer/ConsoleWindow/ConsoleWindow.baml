   M S B A M L   `   `   `  ���� L  HVisualNovelEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null_4clr-namespace:VisualNovelEngine.Viewer.ConsoleWindow&VisualNovelEngine.Viewer.ConsoleWindow  :    4VisualNovelEngine.Viewer.ConsoleWindow.ConsoleWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Flocal4clr-namespace:VisualNovelEngine.Viewer.ConsoleWindow       � Title$  	控制台��$	��600��$	��800��   BackgroundColor#���   .���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* �.+��� -   $��PathTextBox�� �� 
IsReadOnly . $��Stretch=�$	��Top=�$��10q�$��30��  SecondaryBackgroundColor#���    ForegroundColor#���    BorderColor#���   �� TextWrapping$
 Wrap=����� *CurrentPagePath+ �� Mode$ OneWay=�`� $��1�� `� 
Background# �  .�c�  c� Band$ 0��.@��� -   $��FontFamilyComboBox��$	��150���� -   $��FontSizeComboBox��$��50��.@��� .��8�� .��9�� .��10�� .��11�� .��12�� .��14�� .��16�� .��18�� .��20�� .��22�� .��24�� .��26�� .��28�� .��36�� .��48�� .��72c� $ 1��.@�d� -   $��
BoldButton�� � ToolTip$ 粗体��.���� $
��Bold"�Bd� -   $��ItalicButton��$ 斜体��.���� $��Italic$�Id� -   $��UnderlineButton��$ 	下划线��.���� $��	Underline{�U�� �� -   $��TextColorButton��$ 文本颜色��.���� -   $��TextColorRectangle��$��16��$��16��
���   ��� -	   $��HighlightColorButton��$ 背景颜色��.���� -
   $��HighlightColorRectangle��$��16��$��16��
��� ����� �� -   $��InsertJumpButton��$ 插入跳转点��.���� 
[[跳转]]� $��2�� � ColumnDefinitions �� $	��250 ��� $��* �.+�� $��0��$��10q� �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��页面列表��$��16(�$��0,0,0,10q�#���  �� -   $��PageListBox��$��1��#���  #���  #���  �� $��2��	 �� Orientation$	 
Horizontal=�$��0,10,0,0q�.+��� -   $��NewPageButton��$��新建页面��$��0,0,5,0q��� -   $��DeletePageButton��$��删除页面���� -   $��RichTextEditor��$��1��$��10q�#���  #���  #���  