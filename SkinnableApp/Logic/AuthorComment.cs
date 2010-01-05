using System;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;
using SIinformer.Utils;
using System.Text.RegularExpressions;
using SkinnableApp;
using System.Windows.Threading;
using System.Threading;
using System.Linq;
using ThreadSafeWPF;

namespace SIinformer.Logic
{
    /// <summary>
    /// Детальная информация на страничке автора по произведению
    /// </summary>
    public class AuthorComment : BindableObject
    {
        private bool _isNew;
        private bool _isIgnored;
        private bool _isUpdating;
        public int Order { get; set; }        
        public string Name { get; set; }
        public string Link { get; set; }
        private readonly object _lockObj = new object();

        private int last_max_comment = 0;

        public int Last_Max_Comment { get { return last_max_comment; } set { last_max_comment = value; RaisePropertyChanged("Last_Max_Comment"); } }

        [XmlIgnore]
        public BindingList<Comment> Comments { get; set; }
        /// <summary>
        /// Дата обновления комментов
        /// </summary>
        DateTime _UpdateDate ;
        public DateTime UpdateDate
        {
            get { return _UpdateDate; }
            set
            {
                _UpdateDate = value;
                RaisePropertyChanged("UpdateDate");
                RaisePropertyChanged("UpdateDateVisual");                
            }
        }

        /// <summary>
        /// Время следующего обновления
        /// </summary>
        [XmlIgnore]
        public DateTime NextUpdateDate { get; set; }
        /// <summary>
        /// Текущий индекс обновления
        /// </summary>
        int _CurrentUpdateIntervalIndex = 0;
        [XmlIgnore]
        public int CurrentUpdateIntervalIndex { get { return _CurrentUpdateIntervalIndex; } set { _CurrentUpdateIntervalIndex = value; } }

        public string AuthorName { get; set; }        

        /// <summary>
        /// Идет обновление
        /// </summary>
        public bool isUpdating { get { return _isUpdating; } set { _isUpdating = value; 
            RaisePropertyChanged("isUpdating");
            RaisePropertyChanged("isVisibleUpdating"); 
        } }

        public System.Windows.Visibility isVisibleUpdating
        {
            get
            {
                return isUpdating ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Автор, обратная ссылка. Для байндинга
        /// </summary>
        public Author author { get; set; }

        BackgroundWorker _worker = null;

        /// <summary>
        /// Отображаемая дата обновления комментариев
        /// </summary>
        [XmlIgnore]
        public string UpdateDateVisual
        {
            get { return UpdateDate.ToShortDateString() + " " + UpdateDate.ToShortTimeString(); }
        }

        string _Rating = "";
        public string Rating { get { return _Rating; } set { _Rating = value; RaisePropertyChanged("Rating"); } }


        /// <summary>
        /// Конструктор
        /// </summary>
        public AuthorComment()
        {
            UpdateDate = DateTime.Now;
            Comments = new BindingList<Comment>();
            _isUpdating = false;
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);            
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindow.isUpdating = isUpdating = false;
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            isUpdating = true;
            ParseComment(GetCommentPage());                        
        }

        /// <summary>
        /// Признак обновления комментов
        /// </summary>
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (value == _isNew)
                    return;

                _isNew = value;

                RaisePropertyChanged("IsNew");
                RaisePropertyChanged("Star");
            }
        }

        /// <summary>
        /// Не обновлять комментарии
        /// </summary>
        public bool IsIgnored
        {
            get { return _isIgnored; }
            set
            {
                if (value != _isIgnored)
                {
                    _isIgnored = value;
                    RaisePropertyChanged("IsIgnored");
                }
            }
        }

        bool IsManualUpdating = false;
        public void UpdateComments(bool manual)
        {            
            while (MainWindow.isUpdating)
                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            IsManualUpdating = manual;
            MainWindow.isUpdating = true;
            _worker.RunWorkerAsync();                                        
            isUpdating = true;
            //ParseComment(GetCommentPage());
            //MainWindow.isUpdating = isUpdating = false;
        }

        #region Обновление комментариев
        public string GetCommentPage()
        {
            string url = Link;
            return WEB.DownloadPageSilent(url);
        }

        int pages = 0;
        public bool ParseComment(string page)
        {
            bool retValue = false;
            //lock (_lockObj)
            //{                
            //    return retValue;
            //} // lock           
            if (page == null) return false;
            Match match = Regex.Match(page, @"<title>Самиздат:\s*(.*?)\s*</title>");
            if (match.Success)
            {
                Name = match.Groups[1].Value;
            }
            pages = 1;
            foreach (Match match2 in Regex.Matches(page, @"<a href=/comment/.*?\?PAGE=(\d+)>"))
            {
                int num = int.Parse(match2.Groups[1].Value);
                if (num > this.pages)
                {
                    this.pages = num;
                }
            }
            //(Оценка:<b><a href=/cgi-bin/vote_show?DIR=p/pupkin_wasja_ibragimowich&FILE=memories_defagmentation>9.15*52</a></b>,)
            Match rating_match = Regex.Match(page, "Оценка:<b><a href=(?<authorUrl>.*?)[^>]*>(?<rating>.*?)</a></b>,", RegexOptions.Singleline);
            if (rating_match.Success)
             {
                 Rating = "Оценка: " + rating_match.Groups["rating"].Value;
             }

             MatchCollection matchs = Regex.Matches(page, "<small>(?<order>\\d+)\\.</small>\\s*<b>\\*?(?:<noindex><a href=\"(?<authorUrl>.*?)\"[^>]*>)?(?:<font[^>]*>)?(?<author>.*?)(?:</font>)?(?:</a></noindex>)?</b>\\s*(?:\\(<u>(?<email>.*?)</u>\\)\\s*)?<small><i>(?<date>.*?)\\s*</i>\\s*(?:\\[<a href=\"(?<editUrl>\\S+?)\"\\s*>исправить</a>\\]\\s*)?(?:\\[<a href=\"(?<deleteUrl>\\S+?)\"\\s*><font[^>]*>удалить</font></a>\\]\\s*)?(?:\\[<a href=\"(?<replyUrl>\\S+?)\"\\s*>ответить</a>\\]\\s*)?</small></i>\\s*<br>(?<comment>.*?)\\s*<hr noshade>", RegexOptions.Singleline);
             if (matchs.Count == 0)
             {
                 return false;
             }
             else
             {
                 
                 MainWindow.mainWindow.InvokeIfRequired(() =>
                 {
                     Comments.Clear();
                 },
                 DispatcherPriority.Background);

                 int max_number = 0;
                 foreach (Match match3 in matchs)
                 {
                     Comment item = new Comment();
                     item.Number = int.Parse(match3.Groups["order"].Value);
                     item.CommenterName = NormalizeHTML(match3.Groups["author"].Value);
                     item.CommenterURL = match3.Groups["authorUrl"].Value;
                     item.eMail = NormalizeHTML(match3.Groups["email"].Value);
                     item.Date = match3.Groups["date"].Value;
                     item.EditURL = match3.Groups["editUrl"].Value;
                     item.DeleteURL = match3.Groups["deleteUrl"].Value;
                     item.ReplyURL = match3.Groups["replyUrl"].Value;
                     item.CommentText = NormalizeHTML(match3.Groups["comment"].Value).TrimEnd(new char[0]);
                     max_number = Math.Max(max_number, item.Number);
                     item.CommenterImage = GetAuthorImage(item.CommenterURL);
                     MainWindow.mainWindow.InvokeIfRequired(() =>
                     {
                         Comments.Add(item);
                     },
                     DispatcherPriority.Background);                     
                     
                 }
                 if (max_number != Last_Max_Comment && Last_Max_Comment != 0) // если есть новые комменты и это не первый запуск, говорим, что есть обновления && last_max_comment != 0
                 {
                     MainWindow.mainWindow.InvokeIfRequired(() =>
                     {
                         IsNew = true;
                         MainWindow.mainWindow.ShowUpdateMessage(Name);
                     },
                     DispatcherPriority.Background);
                     CurrentUpdateIntervalIndex = 0; // если есть комменты, сбрасываем счетчик, то есть ставим минимальное время обновления
                 }
                 else
                 {
                     if (!IsManualUpdating)
                     CurrentUpdateIntervalIndex++; // елси нет обновления, то увеличиваем время следующей проверки, если это был не ручной запуск
                 }
                 Last_Max_Comment = max_number;
                 retValue = true;

             }
             UpdateDate = DateTime.Now;
             int newIntervalIndex;
             NextUpdateDate = MainWindow.mainWindow.GetNextUpdateTimeByInterval(CurrentUpdateIntervalIndex, out newIntervalIndex);
             CurrentUpdateIntervalIndex = newIntervalIndex; // корректировка последнего индекса, чтобы не рос бесконтрольно
            return retValue;
        }

        private string GetAuthorImage(string url)
        {
            string retValue = @"\Resources\Pictures\blacknik.png";//@"\Resources\Pictures\gina_kingsley.jpg";
            string author_wo_photo = @"\Resources\Pictures\author.png";//@"\Resources\Pictures\david_greene.jpg";
            if (url == "") return retValue;
            string p = url.Replace("http://","").Replace("https://","").Replace("HTTP://","").Replace("HTTPS://","");
            string[] url_parts = p.Split("//".ToCharArray());
            if (url_parts.Length < 4) return retValue;

            string authors_folder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Commenters");
            string author_path = Path.Combine(authors_folder, url_parts[1]);
            author_path = Path.Combine(author_path, url_parts[2]);
            author_path = Path.Combine(author_path, "image");
            if (File.Exists(author_path))
                return author_path;

            if (!url.EndsWith("//")) url = url + "//";

            string page = WEB.DownloadPageSilent(url + "indexdate.shtml");
            if (!string.IsNullOrEmpty(page))
            {
                Match match = Regex.Match(page, "<img src=(.*?).width");
                if (match.Success)
                {
                    string image = match.Groups[1].Value;
                    try
                    {
                        if (!image.StartsWith("http://"))
                            image = url + image;
                        byte[] _image =  WEB.DownloadPageSilentBinary(image, null,null);
                        if (_image != null && _image.Length > 0)
                        {

                            if (!Directory.Exists(authors_folder))
                                Directory.CreateDirectory(authors_folder);
                            author_path = Path.Combine(authors_folder, url_parts[1]);
                            if (!Directory.Exists(author_path))
                                Directory.CreateDirectory(author_path);
                            author_path = Path.Combine(author_path, url_parts[2]);
                            if (!Directory.Exists(author_path))
                                Directory.CreateDirectory(author_path);
                            author_path = Path.Combine(author_path, "image");
                            if (File.Exists(author_path))
                                return author_path;

                            File.WriteAllBytes(author_path, _image);
                            retValue = author_path;
                        }
                        else
                        {
                            retValue = author_wo_photo;
                        }
                    }
                    catch
                    {
                        retValue = author_wo_photo;
                    }
                }else
                    retValue = author_wo_photo;

            }

            return retValue;
        }



        #endregion


        /// <summary>
        /// Звезда, отображаемая для книги (желтая - для новой, серая - для старой)
        /// </summary>
        public string Star
        {
            get
            {
                return IsNew
                           ? "pack://application:,,,/Resources/star_yellow_new16.png"
                           : "pack://application:,,,/Resources/star_grey16.png";
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #region Нормализация строк

        private static string NormalizeHTML(string s)
        {
            return
                NormalizeHTMLEsc(
                    Regex.Replace(
                        Regex.Replace(
                            Regex.Replace(
                                Regex.Replace(
                                    Regex.Replace(
                                        Regex.Replace(Regex.Replace(s, @"[\r\n\x85\f]+", ""), "<(br|li)[^>]*>", "\n",
                                                      RegexOptions.IgnoreCase), "<td[^>]*>", "\t",
                                        RegexOptions.IgnoreCase), @"<script[^>]*>.*?</\s*script[^>]*>", "",
                                    RegexOptions.IgnoreCase), "<[^>]*>", ""), @"\n[\p{Z}\t]+\n", "\n\n"), @"\n\n+",
                        "\n\n"));
        }

        private static string NormalizeHTMLEsc(string s)
        {
            return
                Regex.Replace(
                    Regex.Replace(
                        Regex.Replace(
                            Regex.Replace(
                                Regex.Replace(
                                    Regex.Replace(
                                        Regex.Replace(
                                            Regex.Replace(
                                                Regex.Replace(
                                                    Regex.Replace(Regex.Replace(s, "&#([0-9]+);?", delegate(Match match)
                                                    {
                                                        var ch =
                                                            (char)
                                                            int.Parse
                                                                (match
                                                                     .
                                                                     Groups
                                                                     [
                                                                     1
                                                                     ]
                                                                     .
                                                                     Value,
                                                                 NumberStyles
                                                                     .
                                                                     Integer);
                                                        return
                                                            ch.
                                                                ToString
                                                                ();
                                                    }), "&bull;?",
                                                                  " * ", RegexOptions.IgnoreCase), "&lsaquo;?", "<",
                                                    RegexOptions.IgnoreCase), "&rsaquo;?", ">", RegexOptions.IgnoreCase),
                                            "&trade;?", "(tm)", RegexOptions.IgnoreCase), "&frasl;?", "/",
                                        RegexOptions.IgnoreCase), "&lt;?", "<", RegexOptions.IgnoreCase), "&gt;?", ">",
                                RegexOptions.IgnoreCase), "&copy;?", "(c)", RegexOptions.IgnoreCase), "&reg;?", "(r)",
                        RegexOptions.IgnoreCase), "&nbsp;?", " ", RegexOptions.IgnoreCase);
        }

        #endregion
    }
}