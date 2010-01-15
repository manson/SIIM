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
		string AllCookies;

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
			if (!IsLogin)
				Login();
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
			// Getting cookie, if there are password
			//if (string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentCookie))
			//{
			//}
			// Logination
			if (!string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentPassword))
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("OPERATION", "login");
				data.Add("DATA0", MainWindow.mainWindow._setting.CommentName);
				data.Add("DATA1", MainWindow.mainWindow._setting.CommentPassword);
				HttpWebResponse response = WEB.SendHttpPOSTRequest(MainWindow.mainWindow._setting.LoginURL,
										data, MainWindow.mainWindow._setting.LoginURL,
										MainWindow.mainWindow._setting.CommentCookie);
				AllCookies = response.GetCookies().GetString();
			}
			IsLogin = true;
		}

		private void SendComment(Comment Comment)
		{
            sendingComment = true;
            RaisePropertyChanged("ManagerState");
			MainWindow.mainWindow._setting.CommentCookie = "";
			if (string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentCookie))
			{
                int trys = 0; // число попыток
                HttpWebResponse response = null;
                while (response == null && trys++ < 3)
                    response = WEB.SendHttpGETRequest(MainWindow.mainWindow._setting.PostCommentURL + "?COMMENT=" + Comment.Location);
                if (response == null) // если косяк, возвращаем коммент в пул и выходим
                {
                    AddComment(Comment);
                    sendingComment = false;
                    RaisePropertyChanged("ManagerState");
                    return;
                }
				MainWindow.mainWindow._setting.CommentCookie = response.GetCookies().GetString();
			}
			AllCookies += MainWindow.mainWindow._setting.CommentCookie;

			Dictionary<string, string> data = new Dictionary<string, string>();

			data.Add("FILE", Comment.Location);
			data.Add("MSGID", "");
			data.Add("OPERATION", "store_new");

			if (!string.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentPassword))
			{
                int trys = 0; // число попыток                
				System.Net.HttpWebResponse response =null;
                while (response==null && trys++<3)
                    response = WEB.SendHttpGETRequest(MainWindow.mainWindow._setting.PostCommentURL + "?COMMENT=" +
																			 Comment.Location, "", AllCookies);
                if (response == null) // если косяк, возвращаем коммент в пул и выходим
                {
                    AddComment(Comment);
                    sendingComment = false;
                    RaisePropertyChanged("ManagerState");
                    return;
                }
				string page = (new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1251"))).ReadToEnd();
				Match r = Regex.Match(page, "<input[^>]*name=\"EMAIL\"[^>]*value=\"(?<email>[^>]*?)\"[^>]*>");
				if (r.Length != 0) { data.Add("EMAIL", r.Groups["email"].Value); }
				r = Regex.Match(page, "<input[^>]*name=\"NAME\"[^>]*value=\"(?<name>[^>]*?)\"[^>]*>");
				if (r.Length != 0) { data.Add("NAME", r.Groups["name"].Value); }
				r = Regex.Match(page, "<input[^>]*name=\"URL\"[^>]*value=\"(?<url>[^>]*?)\"[^>]*>");
				if (r.Length != 0) { data.Add("URL", r.Groups["url"].Value); }
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
									AllCookies);
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
