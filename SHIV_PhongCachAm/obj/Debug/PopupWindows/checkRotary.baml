   M S B A M L   `   `   `  ����K  GSHIV_PhongCachAm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullM+clr-namespace:SHIV_PhongCachAm.PopupWindowsSHIV_PhongCachAm.PopupWindows  5   	   /    )SHIV_PhongCachAm.PopupWindows.checkRotary   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   =local+clr-namespace:SHIV_PhongCachAm.PopupWindows     5   	     � Title$  Kiểm tra hướng quay��5   	   $	��180��6%   $	��560��62    � WindowStartupLocation$ CenterScreen=�6>    � 	Resources 5         
MyCommand1 �� Text  
MyCommand2��%X   &	        &	      �� 5   
   $
 Text��6-   �� 5   
   $ Another Text��6-   5       J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command# [  5      5        5   
   -   5        # [ 5      5       J� InputBindings 5      �� 5   
   
 �� Key$ F1��6    �� Command# [  6   �� 5   
   $ F5��6   # [ 6   .��5      � .+�5   
   �� .+�5      �� -   5        $��lblTitle��5   `   	 �� ContentStringFormat$.	 (Kiểm tra hướng quay động cơ {0}��6   $��30(�6p   $��50��6~   $��20q�6�   ��6L   �� 5      � 5       
 � ColumnDefinitions
 5!      �� 5"      $��* �6'   �� 5#      $��* �6'   .+�5%      �� -   5        $��btnOK��5%      $��0��6Q   $��20, 10, 10, 10q�6a   $��25��6y   .��6�   OK6�   �� 5&      -   5        $��btnNG��5&      $��1��6P   $��10, 10, 20, 10q�6`   $��25��6x   .��6�   NG6�   5'      5(      5)      5*      