   M S B A M L   `   `   `  ����V  RPC Uzaktan Kontrol Wifi PRO, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:wifiuzaktankontrolprowifiuzaktankontrolpro  5   	   &     wifiuzaktankontrolpro.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:wifiuzaktankontrolpro     5   	   \ XHardcodet.Wpf.TaskbarNotification, Version=1.0.5.0, Culture=neutral, PublicKeyToken=null)tb http://www.hardcodet.net/taskbar  5   	   -   5          � Title$&   PC Uzaktan Kontrol Wifi PRO v1.2��5	   	   $��249.615��62   $��566.934��6C    � Icon$ iconlast.ico��6j   .��5
      � .+�5   
   �� -   5         ��Name$ txtPort��5      $��Right=�6%   $��0,10,9,0q�6A   $
��Wrap=�6S   $��PORT:��6g   $	��Top=�6t   $��18(�6�   $
��Bold"�6�   �� 5   
   -   5        $ txtBaglanan��5      $
��Left=�6)   $��	10,10,0,0q�6D   $
��Wrap=�6W   $��
Bağlanan:��6k   $	��Top=�6|   $��18(�6�   $
��Bold"�6�    J� RenderTransformOrigin$ 1.706,1.486&�6�   �� 5   
   -   5        $ txtDrm��5      $
��Left=�6$   $��	10,61,0,0q�6?   $
��Wrap=�6R   $��Komut:��6f   $	��Top=�6t   $��18(�6�   $
��Bold"�6�   �� 5   
   -   5        $ 	txtIpInfo��5      $
��Left=�6'   $��10,0,0,9q�6B   $
��Wrap=�6T   $��Bağlanmanız için IP:��6h   $��Bottom=�6�   $��16(�6�   $
��Bold"�6�   $ 1.706,1.486&�6�   �� 5   
   -   5         ��Name$ chkBaslangic��5      $%��Başlangıçta Açılsın Mı ?��6)   $��Right=�6L   $��	0,0,10,10q�6h   $��Bottom=�6{   6�   3  -Hardcodet.Wpf.TaskbarNotification.TaskbarIcon  5   
   -   5          Name$ 
ntfTskIcon��5         
IconSource$ /iconlast.ico��5         ToolTipText$! PC Uzaktan Kontrol Wifi PRO��5       � ContextMenu 5      �� 5      .@�5      m� -   5        $��Programı Kapat��5      68   m� 5      -	   5        $��Programı Göster��5      68   5      5       5#      5$      