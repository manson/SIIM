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
		}

        private void PostComment_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow._setting.CommentName = CommentName.Text;
            MainWindow.mainWindow._setting.CommentEmail = CommentEmail.Text;
            Dictionary<string, string> data = new Dictionary<string, string>();
            string link = ((AuthorComment)this.DataContext).Link.Substring(MainWindow.mainWindow._setting.CommentsLink.Length);
            data.Add("FILE", link);
            data.Add("MSGID", "");
            data.Add("OPERATION", "store_new");
            data.Add("NAME", MainWindow.mainWindow._setting.CommentName);
            data.Add("EMAIL", MainWindow.mainWindow._setting.CommentEmail);
            data.Add("URL", "");
            data.Add("TEXT", CommentText.Text);
            WEB.SendHttpPOSTRequest(MainWindow.mainWindow._setting.PostCommentLink,
                MainWindow.mainWindow._setting.PostCommentLink + "?COMMENT=" + link, data);
            // Manual update comment list here!
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CommentName.Text = MainWindow.mainWindow._setting.CommentName;
            CommentEmail.Text = MainWindow.mainWindow._setting.CommentEmail;
        }
	}
}