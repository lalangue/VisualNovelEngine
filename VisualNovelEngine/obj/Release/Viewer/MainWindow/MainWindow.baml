   M S B A M L   `   `   `  ���� L  HVisualNovelEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:VisualNovelEngine.ViewerVisualNovelEngine.Viewer  )    #VisualNovelEngine.Viewer.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     8local&clr-namespace:VisualNovelEngine.Viewer       � Title$  Visual Novel Engine��$	��720��$
��1280�� � WindowStartupLocation$ CenterScreen=�   WindowBackgroundBrush#���     ForegroundBrush#���   � WindowStyle$
 None=� � AllowsTransparency .  � 
ResizeMode$ CanResizeWithGrip=� � 	Resources 3  -System.Windows.Media.Effects.DropShadowEffect  WindowShadow   
BlurRadius   	Direction   Opacity	   ShadowDepth
   Color 
 FadeIn �� TargetProperty W� From	 W� To g� Duration��%�   &	       &	 6       $ 15Q�$ 0Q�$	 0.5Q�$	 0Q�$
 #000000���� .��W� $ Opacity�$ 0X�$ 1X�$ 0:0:0.3D�.���� $��1q����	#33000000 �� CornerRadius$ 8��#���    J� Effect# [ ._��  � RowDefinitions �� $��40 ��� $��* ��� $��30 �.+�� -   $��0��
������  � ColumnDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$��Visual Novel Engine��$��15,0,0,0q�$��Center=�$��16(�$��Medium"�#���  �� $��2�� �� Orientation$ 
Horizontal=�$��Right=�.+��� -    ��Name$ MinimizeButton��  ModernButton#��[ $��40��$��40�� � ToolTip$ 	最小化��.���� $	��—��$��16(��� -   $ MaximizeButton��#��[ $��40��$��40��$ 	最大化��.���� $	��□��$��16(��� -   $ CloseButton��#��[ $��40��$��40��$ 关闭��.���� $	��✕��$��16(�� $��1�� �� $��* ��� $	��300 �.+�� $��0��$��15q� �� $
��Auto ��� $��* ��� $	��200 �.+��� $��0��  TextBackgroundBrush#���  $ 8��$��0,0,0,10q�._��� $ 
Horizontal=�$
��10,5q�.+��� -   $ NewProjectButton��#��[ $��新建项目��$��0,0,5,0q�$ 创建新项目���� -   $ OpenProjectButton��#��[ $��打开项目��$	��5,0q�$ 打开现有项目���� #��["�$	��5,0q��� -   $ 
BackButton��#��[ $	��◀��$	��5,0q�$ 返回上一页���� -   $ ForwardButton��#��[ $	��▶��$	��5,0q�$ 前往下一页���� #��["�$	��5,0q��� -	   $ OpenConsoleButton��#��[ $��	控制台��$	��5,0q�$ 打开控制台窗口���� #��["�$	��5,0q�d� -
    d�Name$ ThemeToggle��  ModernToggleButton#��[ $
��🌙��$	��5,0q�$ 切换明暗主题���� $��1��
������ $��0,0,0,10q�._��� -    ��Name$ BackgroundImage��$��Uniform=��� $��2��#���  $ 8��._��� -    ��Name$ TextDisplay��  ModernRichTextBox#��[  �� 
IsReadOnly . $��10q��� $��1��#���  $ 8��$��
0,15,15,15q�._��  �� $
��Auto ��� $��* �.+��� $��控制面板��$��16(�$��Medium"�$��
15,15,15,5q�#���  �� -    ��Name$ LogList��$��1��$��
15,5,15,15q�  ModernListBox#��[ $��0q��� $��2��#���  $ 0,0,8,8��._�� .+��� -    ��Name$ 
StatusText��$��就绪��$��15,0,0,0q�$��Center=�#���  �� -   $ VersionText��$��v1.0.0��$��0,0,15,0q�$��Right=�$��Center=�#���   J� Opacity$	 0.7Q�