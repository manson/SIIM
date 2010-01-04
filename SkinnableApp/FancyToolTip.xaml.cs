using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;

namespace SkinnableApp
{
	/// <summary>
	/// Interaction logic for FancyToolTip.xaml
	/// </summary>
	public partial class FancyToolTip
	{
	  #region InfoText dependency property

	  /// <summary>
	  /// The tooltip details.
	  /// </summary>
	  public static readonly DependencyProperty InfoTextProperty =
	      DependencyProperty.Register("InfoText",
	                                  typeof (string),
	                                  typeof (FancyToolTip),
	                                  new FrameworkPropertyMetadata(""));

	  /// <summary>
	  /// A property wrapper for the <see cref="InfoTextProperty"/>
	  /// dependency property:<br/>
	  /// The tooltip details.
	  /// </summary>
	  public string InfoText
	  {
	    get { return (string) GetValue(InfoTextProperty); }
	    set { SetValue(InfoTextProperty, value); }
	  }

	  #endregion



		public FancyToolTip()
		{
			this.InitializeComponent();
		}

        private void me_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            object d =e.OriginalSource;
            if (d is Image && (d as Image).Name=="imgClose")
            {
            }
            else
            {
                // открыть окно 
                if (MainWindow.mainWindow.Visibility==System.Windows.Visibility.Hidden)
                    MainWindow.mainWindow.InvertWindowVisibility();
            }
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();

        }

        private void me_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TaskbarIcon taskbarIcon = TaskbarIcon.GetParentTaskbarIcon(this);
            taskbarIcon.CloseBalloon();
        }

	}
}