using System;
using System.Collections.Generic;
using System.Text;
using SIinformer.Utils;
using System.Windows;

namespace SIinformer.Logic
{
    /// <summary>
    /// Строка одного комментария
    /// </summary>
    public class Comment : BindableObject
    {
        public int Number { get; set; }
        public string CommenterName { get; set; }
        public string eMail { get; set; }
        public string CommenterURL { get; set; }
        public string CommenterImage { get; set; }
        public Uri CommenterPicture { get; set; }
        public string CommentText { get; set; }
        public string Date { get; set; }

        public string DeleteURL { get; set; }
        public string EditURL { get; set; }
        public string ReplyURL { get; set; }

        
        public List<CommentItem> FormattedComment
        {
            get
            {
                List<CommentItem> _FormattedComment = new List<CommentItem>();
                string[] sentences = CommentText.Split("\n".ToCharArray());
                string block = ""; bool simple_text = true;
                for (int i = 0; i < sentences.Length; i++)
                {
                    string sent = sentences[i].Trim();
                    if (sent.StartsWith(">"))
                    {
                        if (simple_text && block != "")
                        {
                            _FormattedComment.Add(new CommentItem()
                              {
                                  IsQuote = Visibility.Collapsed,
                                  NotQuote = Visibility.Visible,
                                  Text = block
                              });
                            block = "";
                        }
                        simple_text = false;
                        block = (block == "") ? sent.Replace(">", " ") : block + "\n     " + sent.Replace(">", " ");
                    }
                    else
                    {
                        if (!simple_text && block != "")
                        {

                            _FormattedComment.Add(new CommentItem()
                                  {
                                      IsQuote = Visibility.Visible,
                                      NotQuote = Visibility.Collapsed,
                                      Text = block
                                  });
                            block = "";
                        }
                        simple_text = true;
                        block = (block == "") ? "     " + sent : block + "\n     " + sent.Replace(">", " ");
                    }
                }
                if (block != "")
                {
                    if (simple_text)
                        _FormattedComment.Add(new CommentItem()
                        {
                            IsQuote = Visibility.Collapsed,
                            NotQuote = Visibility.Visible,
                            Text = block
                        });
                    else
                        _FormattedComment.Add(new CommentItem()
                        {
                            IsQuote = Visibility.Visible,
                            NotQuote = Visibility.Collapsed,
                            Text = block
                        });                                        
                }
                return _FormattedComment;
            }
        }
    }

    public class CommentItem
    {
        public string Text { get; set; }
        public Visibility IsQuote { get; set; }
        public Visibility NotQuote { get; set; }
    }

}
