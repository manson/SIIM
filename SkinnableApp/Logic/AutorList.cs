using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using SkinnableApp;

namespace SIinformer.Logic
{
    public class AuthorList : BindingList<Author>
    {
        private void Retreive()
        {
//            Author aut = new Author
//                    {
//                        Name = "Чужин",
//                        IsNew = false,
//                        UpdateDate = DateTime.Now,
//                        URL = "http://zhurnal.lib.ru/c/chushin_i_a/"
//                    };
//            AuthorComment ac = new AuthorComment()
//            {
//                Link = "http://zhurnal.lib.ru/comment/c/chushin_i_a/strannik3",
//                Name = "Странник (Книга третья)",
//                AuthorName = "Чужин"     
//            };
//            aut.AuthorComments.Add(ac);
//            Add(aut);


//            aut = new Author
//                    {
//                        Name = "Конюшевский В.Н.",
//                        IsNew = false,
//                        UpdateDate = DateTime.Now,
//                        URL = "http://zhurnal.lib.ru/k/kotow_w_n/"
//                    };
//            ac = new AuthorComment()
//            {
//                Link = "http://zhurnal.lib.ru/comment/k/kotow_w_n/1",
//                Name = "Иной вариант",
//                AuthorName = "Конюшевский В.Н."
//            };
//            aut.AuthorComments.Add(ac);
//            Add(aut);


//            aut = new Author
//                    {
//                        Name = "Ясинский Анджей",
//                        IsNew = false,
//                        UpdateDate = DateTime.Now,
//                        URL = "/p/pupkin_wasja_ibragimowich/"
//                    };
//            ac = new AuthorComment()
//            {
//                Link = "http://zhurnal.lib.ru/comment/p/pupkin_wasja_ibragimowich/updatetxt",
//                Name = "Ник. Последнее обновление",
//                AuthorName = "Ясинский Анджей"
//            };
//            aut.AuthorComments.Add(ac);
//            ac = new AuthorComment()
//            {
//                Link = "http://zhurnal.lib.ru/comment/p/pupkin_wasja_ibragimowich/siinformer",
//                Name = "Информатор Си",
//                AuthorName = "Ясинский Анджей"
//            };

//            ac.Comments.Add(new Comment()
//            {
//                CommenterName="Вася пупкин",
//                CommenterURL = "http://google.com",
//                eMail = "m@gmail.com",
//                Number = 1,
//                Date = "2009/12/27 10:39",
//                CommentText = @"Я думаю, когда ее напишут :)
//  
//  Вообще хочу предостеречь, любителей клянчить продолжение.
//  Когда творческого человека пытаются заставить делать что-то (даже если он сам любит этим заниматься :) ), не важно как, угрозами ли, посулами (материальным вознаграждением), давлением на жалость, особенно если человек поддастся давлению и начнет соответствовать, есть очень большая вероятность отбить всякое желание творить.
//  
//  Советую набраться терпения и просто ждать, молча.Ну если совсем невмоготу, можно постучаться головой об стену, желательно бетонную и с разбегу, обязательно предварительно хорошо разогнавшись. Пользы для ускорения выхода продолжения будет намного больше."
//            });
//            ac.Comments.Add(new Comment()
//            {
//                CommenterName = "Вася пупкин",
//                CommenterURL = "http://google.com",
//                eMail = "m@gmail.com",
//                Number = 2,
//                Date = "2009/12/27 10:39",
//                CommentText = @"Я думаю, когда ее напишут :)"
//            });

//            ac.Comments.Add(new Comment()
//            {
//                CommenterName = "Талипов Артём",
//                CommenterURL = "http://google.com",
//                eMail = "eric-s@mail.ru",
//                Number = 1025,
//                Date="2009/12/27 02:13",
//                CommentText = @"> > 871.Шпильман Александр
//  >Мать принцессы дроу. Положим потому от рождения у Эль темный волос...
//  
//  > 872.Талипов Артём
//  >Предлагаю на этом варианте и остановиться, т.к. он удовлетворит всех, кроме штучных извращенцев.
//  
//  > > 873.Шпильман Александр
//  >Пологаю Анджий изначально так и задумал. Это мы тормазнули...
//  
//  > > 874.Ясинский Анджей
//  >:-
//  
//  Не... Ещё не тормознули. До нас всё же дошло. После второй подсказки. А может и третьей. Так что рано, на нас, крест ставить.
//  
//  > > 877.Гулевар
//  >А на голове?
//  
//  Поясню. Она эльфийка. А у них волосы растут только на голове. Уточняю: череп, (обычный скальп, брови, ресницы).
//  Под носом, на подбородке, щеках, в ушах и ноздрях волосы не растут.
//  А так же ногах руках, подмышками, на лобке и между ягодицами, а так же на груди и на пятках...
//  Это уже не эльф, а шимпанзе какая-то будет! Ну в крайнем случае хоббетиха из рода махногрудых.
//  И вообще, все кто задумываеться о такой ерунде - извращенцы! (В том числе и я).
//  
//  А вообще, наверное жаль... Я себе представил принцессу на балу, с оригинальной интимной стрижкой, которая станет самой модной у гномок на следующие дцать лет. А так... Даже и не выпендриться, т.к. подстригать нечего. Это ж на неё никто и не посмотрит! Эльфы, они же ведь ущербные существа... Они по этому и одежду носят...
//  
//  > > 876.Олег
//  >Ну да крута !!!
//  >А потом самогипноз и пошло все нах...
//  
//  Не! Показывать false, если детектируеться гипноз, внушение, нлп, кодирование, опьянение и прочее. И только когда всё чисто true.
//  
//  >Если у принцесы найдут ментальные закладки ее закопают, живой и очень глубоко, такое никому не надо.
//  
//  Не... Закапывать не будут. Её выдадут замуж за какого-нибудь приграничного баронета. Это в крайнем случае.
//  А ещё можно провести ритуал изгнания из семьи (хотябы формальный) и пойдёт она в наёмницы, шататься по человеческим землям.
//  Это стандартные схемы.
//  
//  А закапывают, немного в других краях. Но это касаеться мужиков, а девок там сдают в бордели. Принцесс в самые элитные, только для членов королевских семейй, их друзей, врагов и фантастически багатых бизнесменов, плюс гросмейстеров магии.
//  
//  > > 883.Viy
//  >Вообще хочу предостеречь, любителей клянчить продолжение.
//  
//  Правильно! Это чревато!
//  А на самиздате водиться такая порода.
//  Вон например у Труниной, уже почти год клянчуют и что? Тишина!
//  
//  Но я заметил, что клянчут проду, в основном, те, кому сказать больше нечего. Да и коменты лень читать.
//  А прода, боюсь, будет не раньше середины января."
//            });
//            aut.AuthorComments.Add(ac);

//            Add(aut);

        }

        private static string _authorsFileName = "";
        public static AuthorList Load(string authorsFileName)
        {
            _authorsFileName = authorsFileName;
            AuthorList result;

            #region Бекапим данные
            string current_folder = System.AppDomain.CurrentDomain.BaseDirectory;            
            if (File.Exists(authorsFileName))
            {
                string backup_folder = Path.Combine(current_folder, "backup");
                if (!Directory.Exists(backup_folder))
                    Directory.CreateDirectory(backup_folder);
                backup_folder = Path.Combine(backup_folder, DateTime.Today.ToString("yyyyMMdd"));
                if (!Directory.Exists(backup_folder))
                    Directory.CreateDirectory(backup_folder);
                string backup_authors_file = Path.Combine(backup_folder, "Comments.xml" + DateTime.Now.Hour + "_" + DateTime.Now.Minute);
                if (!File.Exists(backup_authors_file))
                    File.Copy(authorsFileName, backup_authors_file);
            }             
            #endregion

            try
            {
                using (var st = new StreamReader(authorsFileName))
                {
                    var sr = new XmlSerializer(typeof (AuthorList));
                    result = (AuthorList) sr.Deserialize(st);
                }
                //result.Save(authorsFileName+".bak");
            }
            catch (Exception)
            {
                result = new AuthorList();
                //result.Retreive();
            }
            foreach (var item in result)
            {
                item.IsUpdated = false;
                foreach (var c in item.AuthorComments)
                {
                    c.isUpdating = false;
                    c.IsNew = false;
                }
            }
            return result;
        }

        public void Save()
        {
            Save(_authorsFileName);
        }
        public void Save(string authorsFileName)
        {
            if (authorsFileName == "") return;
            using (var st = new StreamWriter(authorsFileName))
            {
                var sr = new XmlSerializer(typeof (AuthorList));
                sr.Serialize(st, this);
            }
        }

        public Author FindAuthor(string url)
        {
            foreach (Author a in this)
                if (a.URL.Trim().ToLower() == url.Trim().ToLower()) return a;
            return null;
        }

        public string[] GetCategoryNames()
        {
            List<string> result = new List<string>();
            foreach (Author author in this)
            {
                if (!result.Contains(author.Category))
                    result.Add(author.Category);
            }
            return result.ToArray();
        }

        //public static Author FindAuthor(string author_url)
        //{
        //    foreach (var aut in MainWindow.mainWindow.Authors)
        //    {
        //        if (aut.URL.Trim().ToLower() == author_url.Trim().ToLower())
        //            return aut;
        //    }
        //    return null;
        //}
    }
}