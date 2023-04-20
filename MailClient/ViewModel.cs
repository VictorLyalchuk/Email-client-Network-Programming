using MailKit.Net.Imap;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xaml;
using System.IO;
using System.Collections;

namespace MailClient
{
    [AddINotifyPropertyChangedInterface]
    class ViewModel
    {
        private ObservableCollection<ShowMessage> _ShowMessage;
        private ObservableCollection<InBox> _InBox;
        private ObservableCollection<Drafts> _Drafts;
        private ObservableCollection<Send> _Send;
        private ObservableCollection<Spam> _Spam;
        private ObservableCollection<Trash> _Trash;
        public ViewModel()
        {
            _ShowMessage = new ObservableCollection<ShowMessage>();
            _InBox = new ObservableCollection<InBox>();
            _Drafts = new ObservableCollection<Drafts>();
            _Send = new ObservableCollection<Send>();
            _Spam = new ObservableCollection<Spam>();
            _Trash = new ObservableCollection<Trash>();
        }
        public IEnumerable<ShowMessage> ShowMessage => _ShowMessage;
        public IEnumerable<InBox> InBox => _InBox;
        public IEnumerable<Drafts> Drafts => _Drafts;
        public IEnumerable<Send> Send => _Send;
        public IEnumerable<Spam> Spam => _Spam;
        public IEnumerable<Trash> Trash => _Trash;
        public int modelCount { get; set; }
        public int InboxCount { get; set; }
        public int DraftsCount { get; set; }
        public int SendCount { get; set; }
        public int SpamCount { get; set; }
        public int TrashCount { get; set; }
        public string PRORITY { get; set; }
        public int Flag { get; set; }
        public string Path { get; set; }
        public void AddInBox(InBox inbox)
        {
            _InBox.Add(inbox);
        }
        public void AddDrafts(Drafts drafts)
        {
            _Drafts.Add(drafts);
        }
        public void AddSend(Send send)
        {
            _Send.Add(send);
        }
        public void AddSpam(Spam spam)
        {
            _Spam.Add(spam);
        }
        public void AddTrash(Trash trash)
        {
            _Trash.Add(trash);
        }
        public void DeleteInbox(InBox inbox)
        {
            _InBox.Remove(inbox);
        }
        public void DeleteDrafts(Drafts drafts)
        {
            _Drafts.Remove(drafts);
        }
        public void DeleteSend(Send send)
        {
            _Send.Remove(send);
        }
        public void DeleteSpam(Spam spam)
        {
            _Spam.Remove(spam);
        }
        public void DeleteTrash(Trash trash)
        {
            _Trash.Remove(trash);
        }
        public void ClearInbox()
        {
            _InBox.Clear();
        }
        public void ClearDrafts()
        {
            _Drafts.Clear();
        }
        public void ClearSend()
        {
            _Send.Clear();
        }
        public void ClearSpam()
        {
            _Spam.Clear();
        }
        public void ClearTrash()
        {
            _Trash.Clear();
        }
        public void LoadHTML(WebBrowser MyWebBrowser)
        {
            try
            {
                string mytext;
                using (StreamReader streamreader = File.OpenText("HelloUser.txt"))
                {
                    mytext = streamreader.ReadToEnd();
                }
                MyWebBrowser.NavigateToString(mytext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public Trash Adapter(ShowMessage message)
        {
            Trash trash = new Trash();
            trash.ID = message.ID;
            trash.From = message.From;
            trash.Subject = message.Subject;
            trash.TextBody = message.TextBody;
            trash.time = message.time;
            trash.attachedFile = message.attachedFile;
            trash.AttachedFiles = message.AttachedFiles;
            trash.Count = message.Count;
            return trash;
        }
        public string BorderColor { get; set; }
    }
}