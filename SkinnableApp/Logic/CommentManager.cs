using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Net;

using SIinformer.Logic;
using SIinformer.Utils;

namespace SkinnableApp
{
    /// <summary>
    /// Хреновое название. Коммент, отправляемый пользователем. Имя пересекается с классом комментов, получаемых с сервера. Но пока оставим.
    /// </summary>
	public class Comment
	{
		public string Location { get; private set; }
		public string Text { get; private set; }
		public AuthorComment AuthorComment { get; private set; }

		public Comment(string Location, string Text)
			: this(Location, Text, null)
		{ }

		public Comment(string Location, string Text, AuthorComment AuthorComment)
		{
			this.Location = Location;
			this.Text = Text;
			this.AuthorComment = AuthorComment;
		}
	}
    /// <summary>
    /// Менеджер отправки комментариев
    /// Сделаем BindableObject, чтобы привязаться к количеству неотправленных комментариев в потоке
    /// </summary>
    public class CommentManager : BindableObject
	{
		Queue<Comment> queue = new Queue<Comment>();
		bool IsLogin = false;
		BackgroundWorker send_worker;

		public CommentManager()
		{
			send_worker = new BackgroundWorker();
			send_worker.DoWork += new DoWorkEventHandler(send_worker_DoWork);
            RaisePropertyChanged("CommentsInQueue");
		}

        public string CommentsInQueue
        {
            get
            {
                return "Неотправленных комментариев: " + queue.Count.ToString();
            }
        }

        private bool sendingComment = false;
        public string ManagerState
        {
            get
            {
                return sendingComment? "Производится отсылка...": "";
            }
        }

        

		public void AddComment(Comment Comment)
		{            
			lock (queue)
			{
				queue.Enqueue(Comment);
                RaisePropertyChanged("CommentsInQueue"); // событие об изменении кол-ва комментов
			}
			if (!send_worker.IsBusy)
				send_worker.RunWorkerAsync();
		}

		private void send_worker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (!IsLogin && !string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentPassword))
			{
				Login();
				if (!IsLogin) // Если не смогли зайти, надо попытаться позже
					return;
			}
			Comment c;
			while (queue.Count > 0)
			{
				lock (queue)
				{
					c = queue.Dequeue();
				}
				SendComment(c);
			}
		}

		private void Login()
		{
			if (!string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentPassword))
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("OPERATION", "login");
				data.Add("DATA0", MainWindow.mainWindow._setting.CommentName);
				data.Add("DATA1", MainWindow.mainWindow._setting.CommentPassword);
				int trys = 0; // число попыток
				HttpWebResponse response = null;
				while (response == null && trys++ < 3)
					response = WEB.SendHttpPOSTRequest(MainWindow.mainWindow._setting.LoginURL,
					   								   data, MainWindow.mainWindow._setting.LoginURL, "");
				if (response == null) 
					return;
				CookieCollection cookies = response.GetCookies();
				MainWindow.mainWindow._setting.CommentCookieName = cookies["NAME"].Value;
				MainWindow.mainWindow._setting.CommentCookiePassword = cookies["PASSWORD"].Value;
				string ZUI = cookies["ZUI"].Value;
				string[] zui = ZUI.Split('&');
				MainWindow.mainWindow._setting.CommentZUIName = zui[0];
				MainWindow.mainWindow._setting.CommentZUIEmail = zui[1];
				MainWindow.mainWindow._setting.CommentZUIUrl = zui[2];
			}

			IsLogin = true;
		}

		private void SendComment(Comment Comment)
		{
            sendingComment = true;
            RaisePropertyChanged("ManagerState");

			if (string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentCookieComment))
			{
				int trys = 0; // число попыток
				HttpWebResponse response = null;
				while (response == null && trys++ < 3)
					response = WEB.SendHttpGETRequest(MainWindow.mainWindow._setting.PostCommentURL + "?COMMENT=" + Comment.Location);
				if (response == null)
				{
					AddComment(Comment);
					sendingComment = false;
					RaisePropertyChanged("ManagerState");
					return;
				}

				CookieCollection cookies = response.GetCookies();
				MainWindow.mainWindow._setting.CommentCookieComment = cookies["COMMENT"].Value;
			}
	
			string Cookies = "NAME=" + MainWindow.mainWindow._setting.CommentCookieName + ";PASSWORD=" + 
								   MainWindow.mainWindow._setting.CommentCookiePassword + ";COMMENT=" + MainWindow.mainWindow._setting.CommentCookieComment;

			Dictionary<string, string> data = new Dictionary<string, string>();

			data.Add("FILE", Comment.Location);
			data.Add("MSGID", "");
			data.Add("OPERATION", "store_new");

			if (IsLogin)
			{
				data.Add("NAME", MainWindow.mainWindow._setting.CommentZUIName);
				data.Add("EMAIL", MainWindow.mainWindow._setting.CommentZUIEmail);
				data.Add("URL", MainWindow.mainWindow._setting.CommentZUIUrl);
			}
			else
			{
				data.Add("NAME", MainWindow.mainWindow._setting.CommentName);
				data.Add("EMAIL", MainWindow.mainWindow._setting.CommentEmail);
				data.Add("URL", "");
			}
			data.Add("TEXT", Comment.Text);

			WEB.SendHttpPOSTRequest(MainWindow.mainWindow._setting.PostCommentURL,
									data, MainWindow.mainWindow._setting.PostCommentURL + "?COMMENT=" + Comment.Location,
									Cookies);
            RaisePropertyChanged("CommentsInQueue"); // событие об изменении кол-ва комментов
			if (Comment.AuthorComment != null)
			{
				Comment.AuthorComment.UpdateComments(true);
			}
            sendingComment = false;
            RaisePropertyChanged("ManagerState");
		}

	}
}
