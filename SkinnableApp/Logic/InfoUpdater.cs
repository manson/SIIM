using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using SIinformer.Utils;
using SIinformer.Window;
using Timer=System.Timers.Timer;

namespace SIinformer.Logic
{
    public static class InfoUpdater
    {
        public static void Init(Setting setting, Logger logger)
        {
            _setting = setting;
            _logger = logger;

            Authors = AuthorList.Load(AuthorsFileName);

            Categories = CategoryList.Load(CategoriesFileName);
            Authors.ListChanged += ((o, e) => Refresh());
            OutputCollection = new ObservableCollection<object>();
            Refresh();

            _updater = new Updater(_setting, _logger);
            _updater.UpdaterComplete += UpdaterComplete;
#if !DEBUG
            UpdateAuthors();
#endif

#if !DEBUG
            _updateTimer = new Timer {Interval = 3600000, AutoReset = false};
#else
            _updateTimer = new Timer { Interval = 60000, AutoReset = false};
#endif
            _updateTimer.Elapsed += (o, e) => UpdateAuthors();

            _setting.PropertyChanged += (o, e) =>
                                            {
                                                if (e.PropertyName == "IntervalOfUpdate")
                                                {
                                                    UpdateIntervalAndStart();
                                                    _logger.Add("Периодичность обновления: " + IntervalOfUpdateConverter.Parse(_setting.IntervalOfUpdate));
                                                }
                                            };
        }

        public static CategoryList Categories { get; private set; }
        public static AuthorList Authors { get; private set; }
        public static ObservableCollection<object> OutputCollection { get; private set; }
        private static Logger _logger;

        #region Операции с авторами (+, -, поиск и др.)

        public static Author AddAuthor(string url)
        {
            _logger.Add("Добавление автора...");

            // аналог DoEvents в WPF, иначе "Добавление автора..." вообще не появляется, т.к. метод синхронный
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

            if (!url.ToLowerInvariant().StartsWith("http://")) url = "http://" + url;
            if (!url.EndsWith("indexdate.shtml"))
                url = (url.EndsWith("/")) ? url + "indexdate.shtml" : url + "/indexdate.shtml";

            Author author = Authors.FindAuthor(url);
            if (author != null)
            {
                _logger.Add("Этот автор уже присутствует в списке", true, true);
                return author;
            }

            try
            {
                string pageContent = WEB.DownloadPageSilent(url);
                if (pageContent == null)
                {
                    _logger.Add("Не удалось открыть страницу автора", true, true);
                    return null;
                }

                int index = pageContent.IndexOf('.', pageContent.IndexOf("<title>")) + 1;
                string authorName = pageContent.Substring(index, pageContent.IndexOf('.', index) - index);
                DateTime updateDate = GetUpdateDate(pageContent);

                if (updateDate == DateTime.MinValue)
                {
                    _logger.Add("Не удалось получить дату со страницы автора", true, true);
                    return null;
                }
                if (authorName.Trim() == "")
                {
                    _logger.Add("Не удалось получить имя автора", true, true);
                    return null;
                }
                author = new Author {Name = authorName, IsNew = false, UpdateDate = updateDate, URL = url};
                Authors.Add(author);
                author.UpdateAuthorInfo(pageContent, SynchronizationContext.Current);
                _logger.Add("Добавлен: " + author.Name);
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add("Необработанная ошибка при добавлении автора", true, true);
            }
            return author;
        }

        private static DateTime GetUpdateDate(string page)
        {
            Match match = Regex.Match(page, @"Обновлялось:</font></a></b>\s*(.*?)\s*$", RegexOptions.Multiline);
            DateTime date = DateTime.MinValue;
            if (match.Success)
            {
                string[] newDateStr = match.Groups[1].Value.Split('/');
                date = new DateTime(int.Parse(newDateStr[2]), int.Parse(newDateStr[1]), int.Parse(newDateStr[0]));
            }
            return date;
        }

        public static void DeleteAuthor(Author author)
        {
            AuthorUpdates au = AuthorUpdates.FindWindow(author);
            if (au != null) au.Close();
            Authors.Remove(author);
        }

        #endregion

        private static void UpdaterComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_setting.UseRSS)
            {
                RssChannel channel = File.Exists(_setting.RSSFileName.Trim())
                                         ? new RssChannel(File.ReadAllText(_setting.RSSFileName.Trim(),
                                                                           Encoding.GetEncoding(1251)))
                                         : new RssChannel();
                foreach (Author author in Authors)
                {
                    channel.Add(author);
                }
                File.WriteAllText(_setting.RSSFileName.Trim(), channel.GenerateRss(_setting.RSSCount), Encoding.GetEncoding(1251));
            }

            if (!e.Cancelled)
            {
                string baloonInfo = (string) e.Result;
                if (baloonInfo != "")
                {
                    MainWindow.ShowTrayInfo(baloonInfo.Trim().Trim(new[] {';'}));
                }
            }
            try
            {
                if (_setting.BeforeUpdater.Trim() != "")
                {
                    Process.Start(_setting.BeforeUpdater.Trim(), _setting.BeforeUpdaterParam.Trim());
                    _logger.Add(string.Format("'{0}' запущен.", Path.GetFileName(_setting.BeforeUpdater.Trim())), false);
                }
            }
            catch (Exception ex)
            {
                _logger.Add(ex.StackTrace, false, true);
                _logger.Add(ex.Message, false, true);
                _logger.Add(string.Format("Ошибка при запуске '{0}'.", _setting.BeforeUpdater.Trim()), false, true);
            }

            UpdateIntervalAndStart();
            Save();

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant()=="autoclose")
                    Application.Current.MainWindow.Close();               
            }
        }

        public static void Save()
        {
            Authors.Save(AuthorsFileName);
            Categories.Save(CategoriesFileName);
        }

        public static void UpdateIntervalAndStart()
        {
            if (_setting.IntervalOfUpdate == 0) _updateTimer.Stop();
            else
            {
                _updateTimer.Interval = _setting.IntervalOfUpdate*3600000;
                _updateTimer.Start();
            }
        }

        private static void UpdateAuthors()
        {
            if (!_updater.IsBusy)
            {
                Save();
                _logger.Add("Производится проверка обновлений...");

                List<Author> updatedAuthor = new List<Author>();
                foreach (Author author in Authors)
                {
                    if (!author.IsIgnored)
                        updatedAuthor.Add(author);
                }
                try
                {
                    if (_setting.AfterUpdater.Trim() != "")
                    {
                        Process.Start(_setting.AfterUpdater.Trim(), _setting.AfterUpdaterParam.Trim());
                        _logger.Add(string.Format("'{0}' запущен.", Path.GetFileName(_setting.AfterUpdater.Trim())), false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Add(ex.StackTrace, false, true);
                    _logger.Add(ex.Message, false, true);
                    _logger.Add(string.Format("Ошибка при запуске '{0}'.", _setting.AfterUpdater.Trim()), false, true);
                }
                _updater.RunWorkerAsync(updatedAuthor);
            }
        }

        public static void CancelUpdater()
        {
            _updater.CancelAsync();
        }

        /// <summary>
        /// Вручную останавливает или запускает обновление в зависимости от текущего состояния
        /// </summary>
        public static void ManualProcessing()
        {
            if (_updater.IsBusy)
            {
                _logger.Add("Проверка обновлений останавливается...");
                _updater.CancelAsync();
            }
            else
                UpdateAuthors();
        }

        #region Перестройка представления данных

        private static string _filter = "";
        private static bool _isListUpdates;
        private static ListSortDirection _sortDirection = ListSortDirection.Ascending;
        private static string _sortProperty = "Name";
        private static bool _useCategory;

        public static string Filter
        {
            get { return _filter; }
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    Refresh();
                }
            }
        }

        public static bool UseCategory
        {
            get { return _useCategory; }
            set
            {
                if (_useCategory != value)
                {
                    _useCategory = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Представление данных изменено
        /// </summary>
        public static event InfoUpdaterRefreshEventHandler InfoUpdaterRefresh;

        private static void OnInfoUpdaterRefresh()
        {
            InfoUpdaterRefreshEventHandler refresh = InfoUpdaterRefresh;
            if (refresh != null) refresh();
        }

        /// <summary>
        /// Запрещает обновление представления при большом количестве обновленных объектов
        /// </summary>
        public static void BeginUpdate()
        {
            _isListUpdates = true;
        }

        /// <summary>
        /// Разрешает обновление представления
        /// </summary>
        public static void EndUpdate()
        {
            _isListUpdates = false;
        }

        /// <summary>
        /// Обновляет выходную коллекцию, стараясь изменить ее по минимуму, чтобы не перестраивать визуальное представление списка авторов
        /// </summary>
        public static void Refresh()
        {
            if (_isListUpdates) return;

            #region Создаем преставление данных из списка Authors, фильруем, сортируем

            ListCollectionView authorCollectionView = new ListCollectionView(Authors);
            authorCollectionView.Filter += CheckIncludeAuthorInCollection;
            switch (_sortProperty)
            {
                case "UpdateDate":
                    authorCollectionView.SortDescriptions.Add(new SortDescription("Group", ListSortDirection.Ascending));
                    authorCollectionView.SortDescriptions.Add(new SortDescription(_sortProperty, _sortDirection));
                    break;
                case "Name":
                    authorCollectionView.SortDescriptions.Add(new SortDescription(_sortProperty, _sortDirection));
                    break;
            }

            #endregion

            #region Создаем из представления промежуточный список, который учитывает наличие категорий и их раскрытость

            List<object> tempList = new List<object>();
            if (UseCategory)
            {
                string[] categoryFromAuthors = Authors.GetCategoryNames();
                // подергиваем все категории из Authors, чтоб они создались в Categories, если их еще не было
                foreach (string categoryName in categoryFromAuthors)
                {
                    Categories.GetCategoryFromName(categoryName);
                }
                // заполняем промежуточный список
                foreach (Category category in Categories)
                {
                    category.SetVisualNameAndIsNew(authorCollectionView);
                    tempList.Add(category);
                    if (category.Collapsed) continue;
                    foreach (Author author in authorCollectionView)
                    {
                        if (author.Category == category.Name)
                            tempList.Add(author);
                    }
                }
            }
            else
            {
                foreach (var collectionItem in authorCollectionView)
                {
                    tempList.Add(collectionItem);
                }
            }

            authorCollectionView.Filter -= CheckIncludeAuthorInCollection;

            #endregion

            #region Заполняем выходную коллекцию, стараясь по максимуму использовать имеющиеся элементы

            // Просматриваем выходную коллекцию, удаляя элементы, отсутствующие во временном списке
            for (int i = OutputCollection.Count - 1; i >= 0; i--)
            {
                if (!tempList.Contains(OutputCollection[i]))
                {
                    OutputCollection.RemoveAt(i);
                }
            }
            // Просматриваем временную коллекцию, добавляя из нее в выходную те элементы, 
            // которые отсутствуют в выходной коллекции. Одновременно выходная сортируется по временной
            for (int i = 0; i < tempList.Count; i++)
            {
                object currentItem = tempList[i];
                int outPos = OutputCollection.IndexOf(currentItem);
                if (outPos == i) continue;
                if (outPos >= 0)
                    OutputCollection.Move(outPos, i);
                else
                    OutputCollection.Insert(i, currentItem);
            }

            #endregion

            OnInfoUpdaterRefresh();
        }

        /// <summary>
        /// Прверяет object на соответствие фильтру
        /// </summary>
        /// <param name="obj">Автор</param>
        /// <returns> true - включить в отображение, false - исключить</returns>
        private static bool CheckIncludeAuthorInCollection(object obj)
        {
            Author author = (Author) obj;
            if (_filter.Trim() == "")
                return true;
            if (_filter.StartsWith("~"))
                return author.Name.ToLowerInvariant().StartsWith(_filter.Substring(1).ToLowerInvariant());
            return (author.Name.ToLowerInvariant().Contains(_filter.ToLowerInvariant()) ||
                    (author.Comment.ToLowerInvariant().Contains(_filter.ToLowerInvariant())));
        }

        public static void Sort(string sortProperty, ListSortDirection sortDirection)
        {
            if ((_sortProperty != sortProperty) || (_sortDirection != sortDirection))
            {
                _sortProperty = sortProperty;
                _sortDirection = sortDirection;
                Refresh();
            }
        }

        #endregion

        #region внутренние переменные

        private static Timer _updateTimer;
        private static Updater _updater;
        private static Setting _setting;

        private static string AuthorsFileName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "authorts.xml"); }
        }

        private static string CategoriesFileName
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "categories.xml"); }
        }

        #endregion
    }

    public delegate void InfoUpdaterRefreshEventHandler();
}