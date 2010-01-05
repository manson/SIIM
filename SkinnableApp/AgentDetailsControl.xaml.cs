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
using SIinformer.Utils;

namespace SkinnableApp
{
	public partial class AgentDetailsControl : System.Windows.Controls.UserControl
	{
		public AgentDetailsControl()
		{
			InitializeComponent();
            this.DataContext = null;
			DataContextChanged += new DependencyPropertyChangedEventHandler(AgentDetailsControl_DataContextChanged);
		}

		void AgentDetailsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			SendCommentBorder.IsEnabled = this.DataContext != null;
		}

        private void PostComment_Click(object sender, RoutedEventArgs e)
        {
			if (this.DataContext == null)
				return;
			MainWindow.mainWindow._setting.CommentName = CommentName.Text;
            MainWindow.mainWindow._setting.CommentEmail = CommentEmail.Text;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string link = ((AuthorComment)this.DataContext).Link.Substring(MainWindow.mainWindow._setting.CommentsLink.Length);

			if (String.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentCookie))
			{
				System.Net.HttpWebResponse response = WEB.SendHttpGETRequest(MainWindow.mainWindow._setting.PostCommentLink + "?COMMENT=" + link);
				if (response == null)
					return;
				if (response.Headers["Set-cookie"] != null) // Этот код стремен. Почему то не работает установка куки у HttpWebResponse
				{
					string s = response.Headers["Set-cookie"];
					MainWindow.mainWindow._setting.CommentCookie = s.Substring(s.IndexOf("=") + 1, s.IndexOf(";") - s.IndexOf("=") - 1);
				}
			}

            data.Add("FILE", link);
            data.Add("MSGID", "");
            data.Add("OPERATION", "store_new");
            data.Add("NAME", MainWindow.mainWindow._setting.CommentName);
            data.Add("EMAIL", MainWindow.mainWindow._setting.CommentEmail);
            data.Add("URL", "");
            data.Add("TEXT", CommentText.Text);
            WEB.SendHttpPOSTRequest(MainWindow.mainWindow._setting.PostCommentLink,
									data, MainWindow.mainWindow._setting.PostCommentLink + "?COMMENT=" + link, 
									"COMMENT=" + MainWindow.mainWindow._setting.CommentCookie);
			CommentText.Text = "";
            commentsExpander.IsExpanded = false;
			((AuthorComment)this.DataContext).UpdateComments(true);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CommentName.Text = MainWindow.mainWindow._setting.CommentName;
            CommentEmail.Text = MainWindow.mainWindow._setting.CommentEmail;
        }
	}
}