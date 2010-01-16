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
using SIinformer.Logic;
using System.ComponentModel;
using System.Windows.Threading;
using ThreadSafeWPF;
using SIinformer.Utils;
using System.Text.RegularExpressions;

namespace SkinnableApp
{
	public partial class AgentSelectorControl : System.Windows.Controls.UserControl
	{
        public string HeaderText { get { return "Авторы";} }
        BackgroundWorker add_worker = null;
        private AuthorComment _selectedAuthorComments;

	    public AgentSelectorControl()
		{
			InitializeComponent();
            add_worker = new BackgroundWorker();
            add_worker.DoWork += new DoWorkEventHandler(add_worker_DoWork);
            add_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(add_worker_RunWorkerCompleted);
            
		}


        void add_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindow.mainWindow.InvokeIfRequired(() =>
            {
                MainWindow.mainWindow._logger.Working = false;                
                Cursor = Cursors.Arrow;
            },
                DispatcherPriority.Background);
        }

        void add_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string url = "";
            MainWindow.mainWindow.InvokeIfRequired(() =>
            {
                MainWindow.mainWindow._logger.Working = true;
                Cursor = Cursors.Wait;
                url = new_url.Text;
            },
                DispatcherPriority.Background);

            string page = WEB.DownloadPageSilent(url);
			if (String.IsNullOrEmpty(page))
				return;
            string reg_matcher = "<small><a href=\"(?<authorUrl>.*?)\">другие произведения.</a></small></h3>";                
            MatchCollection matchs = Regex.Matches(page, reg_matcher, RegexOptions.Singleline);
            if (matchs.Count != 0)
            {
                string author_url = matchs[0].Groups["authorUrl"].Value;
                string title = "";
                string author_name = "";
                reg_matcher = "<title>Самиздат:(?<title>.*?)</title>";
                matchs = Regex.Matches(page, reg_matcher, RegexOptions.Singleline);
                if (matchs.Count != 0)
                {
                    title = matchs[0].Groups["title"].Value.Trim();
                    reg_matcher = "<h3>(?<author>.*?): <small><a href=";
                    matchs = Regex.Matches(page, reg_matcher, RegexOptions.Singleline);
                    if (matchs.Count != 0)
                    {
                        author_name = matchs[0].Groups["author"].Value.Trim();
                        Author aut = MainWindow.mainWindow.Authors.FindAuthor(author_url);
                        if (aut == null)
                        {
                            aut = new Author
                            {
                                Name = author_name,
                                IsNew = true,
                                UpdateDate = DateTime.Now,
                                URL = author_url
                            };
                            MainWindow.mainWindow.InvokeIfRequired(() =>
                            {
                                MainWindow.mainWindow.Authors.Add(aut);
                            },
                                DispatcherPriority.Background);                            
                        }
                        AuthorComment ac = null;
                        foreach (var comm in aut.AuthorComments)
                        {
                            if (comm.Link.Trim().ToLower() == url.Trim().ToLower())
                            {
                                MainWindow.mainWindow._logger.Add(url + " уже есть в списке.", false, true);
                                return;
                            }
                        }
                        ac = new AuthorComment()
                        {
                            Link = url,
                            Name = title,
                            AuthorName = author_name
                        };
                            MainWindow.mainWindow.InvokeIfRequired(() =>
                            {
                                aut.AuthorComments.Add(ac);
                                new_url.Text = "";
                                MainWindow.mainWindow.Authors.Save();
                            },
                                DispatcherPriority.Background);                            
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }


        private void AuthorCommentsList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is AuthorComment)
            {
                MainWindow.GetCommentsDetail().DataContext = e.NewValue;
                (e.NewValue as AuthorComment).IsNew = false;
                _selectedAuthorComments = (AuthorComment)e.NewValue;
            }
            else
            {
                _selectedAuthorComments = null;
            }
        }

        
        private void Add_URL_Click(object sender, RoutedEventArgs e)
        {
            if (new_url.Text.Trim() == "") return;
            adding_panel.DataContext = MainWindow.mainWindow._logger;
            add_worker.RunWorkerAsync();
        }

        private void RemoveAuthorCommentClick(object sender, RoutedEventArgs e)
        {
            if (_selectedAuthorComments==null) return;
            ((AuthorList)DataContext).RemoveAuthorComments(_selectedAuthorComments);
        }

        private void OpenWithSiClick(object sender, RoutedEventArgs e)
        {
            if (_selectedAuthorComments == null) return;
            WEB.OpenURL(_selectedAuthorComments.Link);
        }
	}
}