using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using SIinformer.Logic;
using SIinformer.Utils;
using System.Windows.Shapes;
using System.Timers;
using ThreadSafeWPF;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;

namespace SkinnableApp
{
	public partial class MainWindow : Window
    {
        public AuthorList authors = null;
        public AuthorList Authors { get { return authors;} }
        public Logger _logger;
        public Setting _setting;
		public CommentManager _comments;
        // таймер обновлений. ќтрабатывает каждую минуту, но смотрит у каждой ленты свое врем€
        Timer update_timer = null;

        public bool IsInit { get; private set; }

        public static bool isUpdating { get; set; }

        // временные интервалы обновлений. ѕоследовательно увеличиваютс€, если комментариев нет
        public int[] UpdateIntervals = new int[] {3,3,5,10,15,20,30,40,50,60};

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CTOR Exception: " + ex.Message);
            }
            _setting = Setting.LoadFromXml();
            _setting.PropertyChanged += SettingPropertyChanged;
            _logger = new Logger();
			_comments = new CommentManager();


			// Load the default skin.
			Grid mainGrid = this.Content as Grid;
			MenuItem item = mainGrid.ContextMenu.Items[0] as MenuItem;
            this.ApplySkinFromMenuItem(item);


            string current_folder = System.AppDomain.CurrentDomain.BaseDirectory;
            string current_authors_file = System.IO.Path.Combine(current_folder, "Comments.xml");

            authors = AuthorList.Load(current_authors_file); ;
            Root.DataContext = Authors;

            WEB.Init(_setting.ProxySetting, _logger);
            MainWindow.mainWindow = this;

            update_timer = new Timer();
            update_timer.Elapsed += (o, e) =>
                {
                    MainWindow.mainWindow.InvokeIfRequired(() =>
                    {
                        try
                        {
                            update_timer.Stop();
                            UpdateAllAuthors(false);
                            update_timer.Interval = 60000; // 1 минута дл€ проверки            
                            update_timer.Start();
                        }
                        catch (Exception ex)
                        {
                            _logger.Add(ex.StackTrace, false, true);
                            _logger.Add(ex.Message, false, true);
                            _logger.Add(" ака€ то лажа здесь происходит", false, true);
                        }
                    },
                    System.Windows.Threading.DispatcherPriority.Background);
                };

            update_timer.Interval = 1000; // 1 секунда дл€ проверки при запуске
            update_timer.Start();

            IsInit = true;
        }

        public DateTime GetNextUpdateTimeByInterval(int intervalIndex, out int newIntervalIndex)
        {
            intervalIndex = (intervalIndex > UpdateIntervals.Length-1) ? UpdateIntervals.Length-1 : intervalIndex;
            newIntervalIndex = intervalIndex;
            return DateTime.Now.AddMinutes(UpdateIntervals[intervalIndex]);
        }


        private void SettingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            }
        }
		void OnMenuItemClick(object sender, RoutedEventArgs e)
		{
			MenuItem item = e.OriginalSource as MenuItem;

			// Update the checked state of the menu items.
			Grid mainGrid = this.Content as Grid;
			foreach (MenuItem mi in mainGrid.ContextMenu.Items)
				mi.IsChecked = mi == item;

			// Load the selected skin.
			this.ApplySkinFromMenuItem(item);
		}

		void ApplySkinFromMenuItem(MenuItem item)
		{
			// Get a relative path to the ResourceDictionary which
			// contains the selected skin.
			string skinDictPath = item.Tag as string;
			Uri skinDictUri = new Uri(skinDictPath, UriKind.Relative);

			// Tell the Application to load the skin resources.
			DemoApp app = Application.Current as DemoApp;
			app.ApplySkin(skinDictUri);
		}

        /// <summary>
        /// ќбновить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateAllAuthors(true);
        }

        public void UpdateAllAuthors(bool manual)
        {
            foreach (var item in authors)
            {                
                item.UpdateAuthor(manual);
            }
            Authors.Save();
        }

        public static MainWindow mainWindow;

        public static AgentDetailsControl _CommentsDetail;
        public static AgentDetailsControl GetCommentsDetail()
        {
            return _CommentsDetail;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _CommentsDetail = CommentsDetail;
        }


	    public void InvertWindowVisibility()
        {
            if (this.Visibility == System.Windows.Visibility.Hidden)
            {
				if (WindowState == WindowState.Minimized)
				{
					WindowState = WindowState.Maximized;
					Activate();
				}
				if (Visibility != Visibility.Visible) Visibility = Visibility.Visible;                
                Uri uri = new Uri("pack://application:,,/Resources/Icons/Inactive.ico");
                BitmapImage bitmap = new BitmapImage(uri);
                Image img = new Image();
                MyNotifyIcon.IconSource = bitmap;
            }
            else
                this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.Visibility = System.Windows.Visibility.Hidden;                
            }
        }


        public void ShowUpdateMessage(string message)
        {
            FancyToolTip balloon = new FancyToolTip();            
            balloon.InfoText = message;            
            MyNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);

            Uri uri = new Uri("pack://application:,,/Resources/Icons/Active.ico");
            BitmapImage bitmap = new BitmapImage(uri);
            Image img = new Image();
            MyNotifyIcon.IconSource = bitmap;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _setting.SaveToXML(authors);
            Authors.Save();
        }
        public void ShowWindow()
        {
            WindowState = WindowState.Maximized;
            Visibility = Visibility.Visible;
            Activate();
        }
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InvertWindowVisibility_Click(object sender, RoutedEventArgs e)
        {
            InvertWindowVisibility();
        }

    }
}