using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using SIinformer.Utils;

namespace SIinformer.Logic
{
    public class Author : BindableObject
    {
        #region Private Fields

        private string _category = "Default";
        private bool _isIgnored;
        private bool _isNew;
        private bool _isUpdated;
        private string _name;
        private BindingList<AuthorComment> _comments;
        private DateTime _updateDate = DateTime.Now;
        private string _comment = "";
        private readonly object _lockObj = new object();

        #endregion

        public Author()
        {
            _comments = new BindingList<AuthorComment>();
            _comments.ListChanged += TextsListChanged;
        }

        /// <summary>
        /// Отслеживает авторские тексты для коррекции IsNew автора
        /// </summary>
        /// <param name="sender">игнорируется</param>
        /// <param name="e">игнорируется</param>
        private void TextsListChanged(object sender, ListChangedEventArgs e)
        {
            bool summaryIsNew = false;
            foreach (AuthorComment authorComment in AuthorComments)
            {
                if (authorComment.IsNew)
                {
                    summaryIsNew = true;
                    break;
                }
            }
            IsNew = summaryIsNew;
        }

        #region Public Property

        public BindingList<AuthorComment> AuthorComments
        {
            get { return _comments; }
            set
            {
                // отвязываемся от оповещения, чтоб _texts собрал GC
                _comments.ListChanged -= TextsListChanged;
                _comments = value;
                // привязываем новое оповещение
                _comments.ListChanged += TextsListChanged;
                // оповещаем о перезагрузке авторских текстов
                TextsListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, null));
                RaisePropertyChanged("AuthorComments");
            }
        }

        /// <summary>
        /// Не обновлять автора
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

        /// <summary>
        /// Комментарий автора
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set
            {
                value = value.Trim();
                if (value != _comment)
                {
                    _comment = value;
                    RaisePropertyChanged("Comment");
                }
            }
        }

        /// <summary>
        /// Категория автора
        /// </summary>
        public string Category
        {
            get { return _category; }
            set
            {
                if (value != _category)
                {
                    _category = value.Trim();
                    RaisePropertyChanged("Category");
                }
            }
        }

        /// <summary>
        /// Автор находится в процессе обновления
        /// </summary>
        [XmlIgnore]
        public bool IsUpdated
        {
            get { return _isUpdated; }
            set
            {
                if (_isUpdated != value)
                {
                    _isUpdated = value;
                    RaisePropertyChanged("IsUpdated");
                }
            }
        }

        /// <summary>
        /// Имя автора
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Группирует авторов (1-isNew, 2-normal, 3-ignored)
        /// </summary>
        [XmlIgnore]
        public int Group
        {
            get
            {
                if (IsNew) return 1;
                if (IsIgnored) return 3;
                return 2;
            }
        }

        /// <summary>
        /// Адрес автора
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Дата/время обновления
        /// </summary>
        public DateTime UpdateDate
        {
            get { return _updateDate; }
            set
            {
                if (value != _updateDate)
                {
                    _updateDate = value;
                    RaisePropertyChanged("UpdateDate");
                    RaisePropertyChanged("UpdateDateVisual");
                }
            }
        }

        /// <summary>
        /// Дата/время обновления в виде строки для binding'а
        /// </summary>
        public string UpdateDateVisual
        {
            get { return "Обновлено: " + UpdateDate.ToShortDateString() + " " + _updateDate.ToShortTimeString(); }
        }


        /// <summary>
        /// У автора есть новые комментарии
        /// </summary>
        public bool IsNew
        {
            get {
                return _isNew; 
            }
            set
            {
                if (value != _isNew)
                {
                    _isNew = value;
                    if (value == false)
                    {
                        foreach (AuthorComment txt in AuthorComments)
                            txt.IsNew = false;
                    }
                    RaisePropertyChanged("IsNew");
                    RaisePropertyChanged("Star");
                }
            }
        }

        /// <summary>
        /// Ресурс (звезда) автора, для binding'а
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

        #endregion

        #region Обновление автора

        /// <summary>
        /// Обновляет информацию о комментариях.        
        /// </summary>
        /// <exception <exception cref="System.Exception">Когда страница не загружена</exception>
        /// <returns>true-есть новые комментарии, false-нет</returns>
        public void UpdateAuthor(bool manual)
        {
            bool isTimeToUpdate = false;
            foreach (var item in AuthorComments)
                if (item.NextUpdateDate == null || item.NextUpdateDate < DateTime.Now || manual) // обновляем, если время подошло или первый запуск или запустили ручками
                {
                    isTimeToUpdate = true;
                    item.UpdateComments(manual);
                }
            if (isTimeToUpdate)
            {
                foreach (var txt in AuthorComments)
                    _isNew = _isNew | txt.IsNew;
                UpdateDate = DateTime.Now;
            }
        }


        #endregion

    

        #region Override

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}


