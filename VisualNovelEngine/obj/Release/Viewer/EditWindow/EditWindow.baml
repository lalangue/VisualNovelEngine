   M S B A M L   `   `   `  ���� \  XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35ZAclr-namespace:System.Windows.Shell;assembly=PresentationFrameworkSystem.Windows.Shell  L HVisualNovelEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:VisualNovelEngine.Viewer.EditWindow#VisualNovelEngine.Viewer.EditWindow 4   .VisualNovelEngine.Viewer.EditWindow.EditWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml      @d2http://schemas.microsoft.com/expression/blend/2008      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006      SshellAclr-namespace:System.Windows.Shell;assembly=PresentationFramework      Clocal1clr-namespace:VisualNovelEngine.Viewer.EditWindow      -     � Title$  Visual Novel��$
��1280��$	��720��$	��800��$	��450�� � WindowStyle$
 None=� � WindowStartupLocation$ CenterScreen=� � AllowsTransparency .    WindowBackgroundBrush#���    � 
ResizeMode$ 	CanResize=� � 	Resources   LightBackgroundBrush �� Color  LightTextBackgroundBrush  LightForegroundBrush  LightAccentBrush  DarkBackgroundBrush  DarkTextBackgroundBrush  DarkForegroundBrush  DarkAccentBrush 	 TextBackgroundBrush 
 ForegroundBrush  AccentBrush  FadeInStoryboard �� TargetProperty W� From		 W� To
 g� Duration �� 
TargetType �� Property �� Value��%&  &	       &	      &	 (     &	 ;     &	 N     &	 a     &	 v     &	 �     &	  �     &		 �     &	
 �     &	 �     &	 �     '_� #    �� $ #F5F5F5���� $ 	#EEEEEEEE���� $ #333333���� $ #5C7AEA���� $ #1E1E1E���� $ 	#CC1A1A1A���� $ #E0E0E0���� $ #7C9AFF���� $ #1E1E1E���� $ 	#CC1A1A1A���� $ #E0E0E0���� $ #7C9AFF���� .��W� $ Opacity�$ 0X�$	 1X�$
 0:0:0.3D��� " _�.���  � ��# � 	 ��  � ��# � 
 ��  � ��$ 0q���  � ��$	 8,4q���  � ��$ 12(�'   !System.Windows.Shell.WindowChrome   WindowChrome      CaptionHeight$ 0Q�   ResizeBorderThickness$ 4q�   UseAeroCaptionButtons .  .��� .+��� -    ��Name$ BackgroundImage��$��UniformToFill=� J� Effect -  'System.Windows.Media.Effects.BlurEffect     Radius$ 0Q�-   BackgroundBlurEffect��� -    �Name$ TopControlBar��$	��Top=�$��40��#��� 	  J� Opacity$ 0Q� � Triggers :�  :� RoutedEvent$ Grid.MouseEnter��.[��� .���� .��W� $ Opacity�$	 1X�$
 0:0:0.2D�:� $ Grid.MouseLeave��.[��� .���� .��W� $ Opacity�$	 0X�$
 0:0:0.5D�.+���  �� Orientation$ 
Horizontal=�$��Right=�$��0,0,10,0q�.+��� -    ��Name$ MinimizeButton��$��30��$��30��
������ $��0q� � ToolTip$ 	最小化��.���� $	��—��#��� 
 $��16(�$��Center=��� -   $ MaximizeButton��$��30��$��30��
������ $��0q�$ 	最大化��.���� $	��□��#��� 
 $��16(�$��Center=��� -   $ CloseButton��$��30��$��30��
������ $��0q�$ 关闭��.���� $��×��#��� 
 $��16(�$��Center=�� -   $ PageNavigationPanel��$
��Left=�$	��200��#��� 	 $ 0Q�$y�	Collapsed=� :� $ Grid.MouseEnter��.[��� .���� .��W� $ Opacity�$	 1X�$
 0:0:0.2D�:� $ Grid.MouseLeave��.[��� .���� .��W� $ Opacity�$	 0X�$
 0:0:0.5D�.+��� -	    ��Name$ PageThumbnailList��
������ $��0q�#��� 
 ���� .R�� $��5q� � RowDefinitions �� $	��100 ��� $
��Auto �.+��� $��1q�#���  ._��� $��UniformToFill=����� *ThumbnailImage+�� $��1��$��Center=�$��0,5,0,0q����� *
PageName+�� -
    ��Name$ DialogueText��$��
50,0,50,20q�$��Bottom=�$
��Auto��$	��300��#��� 	 #��� 
 $��18(�$��20q�$��0q�" �� VerticalScrollBarVisibility$
 Auto=�|�.  ��%Y   '��       �� " ��.���  � ��
 ���� ��  � ��$ 0q���  � ��$ 8�� 3  -System.Windows.Media.Effects.DropShadowEffect      ShadowDepth$  0Q�!   
BlurRadius$! 20Q�"   Opacity$	" 0.3Q�� $��Center=�$��Right=�$��0,0,10,0q�.+��� -   $ NextPageButton��$��40��$��40��#��� 	 $��0q�$	 0.5Q�$ 	下一页�� :� $ Button.MouseEnter��.[��� .���� .��W� $ Opacity�$	 1X�$
 0:0:0.2D�:� $ Button.MouseLeave��.[��� .���� .��W� $ Opacity�$		 0.5X�$
 0:0:0.2D�.���� $	��→��#��� 
 $��20(�$��Center=�� $��Center=�$
��Left=�$��10,0,0,0q�.+��� -   $ PrevPageButton��$��40��$��40��#��� 	 $��0q�$	 0.5Q�$ 	上一页�� :� $ Button.MouseEnter��.[��� .���� .��W� $ Opacity�$	 1X�$
 0:0:0.2D�:� $ Button.MouseLeave��.[��� .���� .��W� $ Opacity�$		 0.5X�$
 0:0:0.2D�.���� $	��←��#��� 
 $��20(�$��Center=��� -   $ ThemeToggleButton��$��40��$��40��#��� 	 $��0q�$	 0.5Q�$	��Top=�$��Right=�$��	0,10,10,0q�$ 切换主题�� :� $ Button.MouseEnter��.[��� .���� .��W� $ Opacity�$	 1X�$
 0:0:0.2D�:� $ Button.MouseLeave��.[��� .���� .��W� $ Opacity�$		 0.5X�$
 0:0:0.2D�.���� -   # ��Name$# 	ThemeIcon��$	��☀��#��� 
 $��16(�$��Center=��� -   $ PageNavigationButton��$��40��$��40��#��� 	 $��0q�$	 0.5Q�$	��Top=�$
��Left=�$��	10,10,0,0q�$ 页面导航�� :� $ Button.MouseEnter��.[��� .���� .��W� $ Opacity�$	 1X�$
 0:0:0.2D�:� $ Button.MouseLeave��.[��� .���� .��W� $ Opacity�$		 0.5X�$
 0:0:0.2D�.���� $	��≡��#��� 
 $��16(�$��Center=�r� -   $ r�Name$$ MusicPlayer��$y�	Collapsed=�