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
			MainWindow.mainWindow._setting.CommentPassword = CommentPassword.Text;
			MainWindow.mainWindow._comments.AddComment(
				new Comment(((AuthorComment)this.DataContext).Link.Substring(MainWindow.mainWindow._setting.CommentsURL.Length), CommentText.Text, (AuthorComment)this.DataContext));
			CommentText.Text = "";
            commentsExpander.IsExpanded = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CommentName.Text = MainWindow.mainWindow._setting.CommentName;
            CommentEmail.Text = MainWindow.mainWindow._setting.CommentEmail;
			CommentPassword.Text = MainWindow.mainWindow._setting.CommentPassword;
        }
	}
}