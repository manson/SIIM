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
            Dictionary<string, string> data = new Dictionary<string,string>();
            string link = ((AuthorComment)this.DataContext).Link;
            data.Add("FILE", link);
            data.Add("MSGID", "");
            data.Add("OPERATION", "store_new");
            data.Add("NAME", "Полосухин Илья Дмитриевич");
            data.Add("EMAIL", "ilblackdragon@gmail.com");
            data.Add("URL", "http://zhurnal.lib.ru/p/polosuhin_i_d/");
            data.Add("TEXT", CommentText.Text);
            WEB.SendHttpPOSTRequest("http://zhurnal.lib.ru/cgi-bin/comment",
                "http://zhurnal.lib.ru/cgi-bin/comment?COMMENT=" + link, data);
            // Manual update comment list here!
        }
	}
}