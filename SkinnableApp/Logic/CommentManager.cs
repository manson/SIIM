using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using SIinformer.Logic;
using SIinformer.Utils;

namespace SkinnableApp
{
	public class Comment
	{
		public string Location { get; private set; }
		public string Text { get; private set; }

		public Comment(string Location, string Text)
		{
			this.Location = Location;
			this.Text = Text;
		}
	}

	public class CommentManager
	{
		Queue<Comment> queue = new Queue<Comment>();
		bool IsLogin = false;
		BackgroundWorker send_worker;

		public CommentManager()
		{
			send_worker = new BackgroundWorker();
			send_worker.DoWork += new DoWorkEventHandler(send_worker_DoWork);
		}

		public void AddComment(Comment Comment)
		{
			lock(queue)
			{
				queue.Enqueue(Comment);
			}
			if (!send_worker.IsBusy)
				send_worker.RunWorkerAsync();
		}

		private void send_worker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (!IsLogin)
				Login();
			Comment c;
			while(queue.Count > 0)
			{
				lock(queue)
				{
					c = queue.Dequeue();
				}
				SendComment(c);
			}
		}

		private void Login()
		{
			if (String.IsNullOrEmpty(MainWindow.mainWindow._setting.CommentCookie))
			{
				System.Net.HttpWebResponse response = WEB.SendHttpGETRequest(MainWindow.mainWindow._setting.LoginURL);
				if (response == null)
					return;
				if (response.Headers["Set-cookie"] != null) // Этот код стремен. Почему то не работает установка куки у HttpWebResponse
				{
					string s = response.Headers["Set-cookie"];
					MainWindow.mainWindow._setting.CommentCookie = s.Substring(s.IndexOf("=") + 1, s.IndexOf(";") - s.IndexOf("=") - 1);
				}
			}

			IsLogin = true;
		}

		private void SendComment(Comment Comment)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			data.Add("FILE", Comment.Location);
			data.Add("MSGID", "");
			data.Add("OPERATION", "store_new");
			data.Add("NAME", MainWindow.mainWindow._setting.CommentName);
			data.Add("EMAIL", MainWindow.mainWindow._setting.CommentEmail);
			data.Add("URL", "");
			data.Add("TEXT", Comment.Text);
			WEB.SendHttpPOSTRequest(MainWindow.mainWindow._setting.PostCommentURL,
									data, MainWindow.mainWindow._setting.PostCommentURL + "?COMMENT=" + Comment.Location,
									"COMMENT=" + MainWindow.mainWindow._setting.CommentCookie);
		}

	}
}
