using Login;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Win32;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Collections;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;

namespace MailClient
{
    public partial class InBoxWindow : Window
    {
        ViewModel model;
        ImapClient client;
        TimeValue time = new TimeValue();
        string serverAddress;
        short serverPort;
        public InBoxWindow()
        {
            InitializeComponent();
            serverAddress = ConfigurationManager.AppSettings["IMAP_gmail_server"]!;
            serverPort = short.Parse(ConfigurationManager.AppSettings["IMAP_gmail_port"]!);
            DarkMode.IsChecked = ColorThemes.Value;
            Themes();
            client = new ImapClient();
            model = new ViewModel();
            DataContext = model;
            model.LoadHTML(MyWebBrowser);
        }
        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            MailWindow mailWindow = new MailWindow();
            mailWindow.Show();
        }
        private async void Update_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Updatebutton.IsEnabled = false;
                await Authenticate();
                await GetCountMessages();
                time.MyTime = CountMessages.TotalCount + 0.001;
                MyBar.Maximum = CountMessages.TotalCount;
                await GetInboxMessages();
                await GetDraftsMessages();
                await GetSentMessages();
                await GetJunkMessages();
                await GetTrashMessages();
                await Disconnect();
                MessageBox.Show("Messages loaded!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                Updatebutton.IsEnabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private Task Authenticate()
        {
            return Task.Run(() =>
            {
                client.Connect(serverAddress, serverPort, SecureSocketOptions.SslOnConnect);
                client.Authenticate(User.Login, User.Password);
            });
        }
        private Task Disconnect()
        {
            return Task.Run(() =>
            {
                client.Disconnect(true);
            });
        }
        private Task GetCountMessages()
        {
            CountMessages.TotalCount = 0;
            return Task.Run(() =>
            {
                client.Inbox.Open(FolderAccess.ReadWrite);
                var uids = client.Inbox.Search(SearchQuery.All);
                CountMessages.TotalCount += uids.Count();
                model.InboxCount = uids.Count();

                client.GetFolder(SpecialFolder.Drafts).Open(FolderAccess.ReadWrite);
                var Drafts = client.GetFolder(SpecialFolder.Drafts);
                var Draftsitems = Drafts.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags).OrderByDescending(o => o.Date);
                CountMessages.TotalCount += Draftsitems.Count();
                model.DraftsCount = Draftsitems.Count();

                client.GetFolder(SpecialFolder.Sent).Open(FolderAccess.ReadWrite);
                var Sent = client.GetFolder(SpecialFolder.Sent);
                var Sentitems = Sent.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags).OrderByDescending(o => o.Date);
                CountMessages.TotalCount += Sentitems.Count();
                model.SendCount = Sentitems.Count();

                client.GetFolder(SpecialFolder.Junk).Open(FolderAccess.ReadWrite);
                var Spam = client.GetFolder(SpecialFolder.Junk);
                var Spamitems = Spam.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags).OrderByDescending(o => o.Date);
                CountMessages.TotalCount += Spamitems.Count();
                model.SpamCount = Spamitems.Count();

                client.GetFolder(SpecialFolder.Trash).Open(FolderAccess.ReadWrite);
                var Trash = client.GetFolder(SpecialFolder.Trash);
                var Trashtems = Trash.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags).OrderByDescending(o => o.Date);
                CountMessages.TotalCount += Trashtems.Count();
                model.TrashCount = Trashtems.Count();
            });
        }
        private Task GetInboxMessages()
        {
            return Task.Run(() =>
            {
                IEnumerable<UniqueId> CollectionsIDs = model.InBox.Select(a => a.ID);
                client.Inbox.Open(FolderAccess.ReadWrite);
                IEnumerable<UniqueId> ServerIDs = client.Inbox.Search(SearchQuery.All);
                IEnumerable<UniqueId> RecentIDs = ServerIDs.Except(CollectionsIDs); // collection recent ID
                foreach (UniqueId item in ServerIDs.OrderByDescending(d => d.Id))
                {
                    if (CollectionsIDs.Count() == 0) SetInfoToInboxCollection(item);
                    else
                    {
                        foreach (UniqueId RecentIDitem in RecentIDs)
                        {
                            if (RecentIDitem.ToString() == item.ToString()) SetInfoToInboxCollection(item);
                        }
                    }
                }
            });
        }
        private Task GetDraftsMessages()
        {
            return Task.Run(() =>
            {
                IEnumerable<UniqueId> CollectionsIDs = model.Drafts.Select(a => a.ID);
                client.GetFolder(SpecialFolder.Drafts).Open(FolderAccess.ReadWrite);
                IMailFolder folder = client.GetFolder(SpecialFolder.Drafts);
                IEnumerable<UniqueId> Draftsitems = folder.Fetch(0, -1, MessageSummaryItems.UniqueId).Select(a => a.UniqueId);
                IEnumerable<UniqueId> RecentIDs = Draftsitems.Except(CollectionsIDs); // collection recent ID
                foreach (var item in Draftsitems.OrderByDescending(d => d.Id))
                {
                    if (CollectionsIDs.Count() == 0) SetInfoToDraftsCollection(item, folder);
                    else
                    {
                        foreach (UniqueId RecentIDitem in RecentIDs)
                        {
                            if (RecentIDitem.ToString() == item.ToString()) SetInfoToDraftsCollection(item, folder);
                        }
                    }
                }
            });
        }
        private Task GetSentMessages()
        {
            return Task.Run(() =>
            {
                IEnumerable<UniqueId> CollectionsIDs = model.Send.Select(a => a.ID);
                client.GetFolder(SpecialFolder.Sent).Open(FolderAccess.ReadWrite);
                IMailFolder folder = client.GetFolder(SpecialFolder.Sent);
                IEnumerable<UniqueId> Sentitems = folder.Fetch(0, -1, MessageSummaryItems.UniqueId).Select(a => a.UniqueId);
                IEnumerable<UniqueId> RecentIDs = Sentitems.Except(CollectionsIDs); // collection recent ID
                foreach (var item in Sentitems.OrderByDescending(d => d.Id))
                {
                    if (CollectionsIDs.Count() == 0) SetInfoToSendCollection(item, folder);
                    else
                    {
                        foreach (UniqueId RecentIDitem in RecentIDs)
                        {
                            if (RecentIDitem.ToString() == item.ToString()) SetInfoToSendCollection(item, folder);
                        }
                    }
                }
            });
        }
        private Task GetJunkMessages()
        {
            return Task.Run(() =>
            {
                IEnumerable<UniqueId> CollectionsIDs = model.Spam.Select(a => a.ID);
                client.GetFolder(SpecialFolder.Junk).Open(FolderAccess.ReadWrite);
                IMailFolder folder = client.GetFolder(SpecialFolder.Junk);
                IEnumerable<UniqueId> Spamitems = folder.Fetch(0, -1, MessageSummaryItems.UniqueId).Select(a => a.UniqueId);
                IEnumerable<UniqueId> RecentIDs = Spamitems.Except(CollectionsIDs); // collection recent ID
                foreach (var item in Spamitems.OrderByDescending(d => d.Id))
                {
                    if (CollectionsIDs.Count() == 0) SetInfoToSpamCollection(item, folder);
                    else
                    {
                        foreach (UniqueId RecentIDitem in RecentIDs)
                        {
                            if (RecentIDitem.ToString() == item.ToString()) SetInfoToSpamCollection(item, folder);
                        }
                    }
                }
            });
        }
        private Task GetTrashMessages()
        {
            return Task.Run(() =>
            {
                IEnumerable<UniqueId> CollectionsIDs = model.Trash.Select(a => a.ID);
                client.GetFolder(SpecialFolder.Trash).Open(FolderAccess.ReadWrite);
                IMailFolder folder = client.GetFolder(SpecialFolder.Trash);
                IEnumerable<UniqueId> Trashtems = folder.Fetch(0, -1, MessageSummaryItems.UniqueId).Select(a => a.UniqueId);
                IEnumerable<UniqueId> RecentIDs = Trashtems.Except(CollectionsIDs); // collection recent ID
                foreach (var item in Trashtems.OrderByDescending(d => d.Id))
                {
                    if (CollectionsIDs.Count() == 0) SetInfoToTrashCollection(item, folder);
                    else
                    {
                        foreach (UniqueId RecentIDitem in RecentIDs)
                        {
                            if (RecentIDitem.ToString() == item.ToString()) SetInfoToTrashCollection(item, folder);
                        }
                    }
                }
            });
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Content.ToString() == "Inbox:")
            {
                MyList.ItemsSource = model.InBox;
                model.Flag = 1;
            }
            if (button.Content.ToString() == "Drafts:")
            {
                MyList.ItemsSource = model.Drafts;
                model.Flag = 2;
            }
            if (button.Content.ToString() == "Send:")
            {
                MyList.ItemsSource = model.Send;
                model.Flag = 3;
            }
            if (button.Content.ToString() == "Spam:")
            {
                MyList.ItemsSource = model.Spam;
                model.Flag = 4;
            }
            if (button.Content.ToString() == "Trash:")
            {
                MyList.ItemsSource = model.Trash;
                model.Flag = 5;
            }
            if (MyList.SelectedItem != null)
            {
                MyList.SelectedItem = null;
            }
        }
        private void Card_Mouse_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (MyList.SelectedItem != null)
                {
                    ShowMessage gettextmail = (ShowMessage)MyList.SelectedItem;
                    MyWebBrowser.Navigate(null as Uri);
                    if (gettextmail.TextBody != "") { MyWebBrowser.NavigateToString(gettextmail.TextBody); }

                    ListBoxAttached.Items.Clear();
                    foreach (MimeEntity attachment in gettextmail.AttachedFiles)
                    {
                        string fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                        ListBoxAttached.Items.Add(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SetInfoToInboxCollection(UniqueId item)
        {
            MimeMessage message = client.Inbox.GetMessage(item);
            InBox inbox = new InBox();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                model.AddInBox(inbox);
            });
            inbox.ID = item;
            inbox.From = message.From.ToString();
            if (message.Subject != null) inbox.Subject = message.Subject;
            else inbox.Subject = "";
            //inbox.time = message.Date.DateTime.ToString();
            inbox.time = message.Date.DateTime.ToLongDateString();
            if (message.HtmlBody != null)
                inbox.TextBody = message.HtmlBody;
            else
                inbox.TextBody = message.TextBody;
            inbox.AttachedFiles = message.Attachments;
            foreach (MimeEntity attachment in message.Attachments)
            {
                inbox.attachedFile += attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                inbox.attachedFile += ", ";
            }
            inbox.Count++;
            model.modelCount++;
        }
        private void SetInfoToDraftsCollection(UniqueId item, IMailFolder folder)
        {
            MimeMessage message = folder.GetMessage(item);
            Drafts drafts = new Drafts();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                model.AddDrafts(drafts);
            });
            drafts.ID = item;
            drafts.From = message.From.ToString();
            if (message.Subject != null) drafts.Subject = message.Subject;
            else drafts.Subject = "";
            drafts.time = message.Date.DateTime.ToLongDateString();
            if (message.HtmlBody != null)
                drafts.TextBody = message.HtmlBody;
            else
                drafts.TextBody = message.TextBody;
            drafts.AttachedFiles = message.Attachments;
            foreach (MimeEntity attachment in message.Attachments)
            {
                drafts.attachedFile += attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                drafts.attachedFile += ", ";
            }
            drafts.Count++;
            model.modelCount++;
        }
        private void SetInfoToSendCollection(UniqueId item, IMailFolder folder)
        {
            MimeMessage message = folder.GetMessage(item);
            Send send = new Send();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                model.AddSend(send);
            });
            send.ID = item;
            send.From = message.From.ToString();
            if (message.Subject != null) send.Subject = message.Subject;
            else send.Subject = "";
            send.time = message.Date.DateTime.ToLongDateString();
            if (message.HtmlBody != null)
                send.TextBody = message.HtmlBody;
            else
                send.TextBody = message.TextBody;
            send.AttachedFiles = message.Attachments;
            foreach (MimeEntity attachment in message.Attachments)
            {
                send.attachedFile += attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                send.attachedFile += ", ";
            }
            send.Count++;
            model.modelCount++;
        }
        private void SetInfoToSpamCollection(UniqueId item, IMailFolder folder)
        {
            MimeMessage message = folder.GetMessage(item);
            Spam spam = new Spam();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                model.AddSpam(spam);
            });
            spam.ID = item;
            spam.From = message.From.ToString();
            if (message.Subject != null) spam.Subject = message.Subject;
            else spam.Subject = "";
            spam.time = message.Date.DateTime.ToLongDateString();
            if (message.HtmlBody != null)
                spam.TextBody = message.HtmlBody;
            else
                spam.TextBody = message.TextBody;
            spam.AttachedFiles = message.Attachments;
            foreach (MimeEntity attachment in message.Attachments)
            {
                spam.attachedFile += attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                spam.attachedFile += ", ";
            }
            spam.Count++;
            model.modelCount++;
        }
        private void SetInfoToTrashCollection(UniqueId item, IMailFolder folder)
        {
            MimeMessage message = folder.GetMessage(item);
            Trash trash = new Trash();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                model.AddTrash(trash);
            });
            trash.ID = item;
            trash.From = message.From.ToString();
            if (message.Subject != null) trash.Subject = message.Subject;
            else trash.Subject = "";
            trash.time = message.Date.DateTime.ToLongDateString();
            if (message.HtmlBody != null)
                trash.TextBody = message.HtmlBody;
            else
                trash.TextBody = message.TextBody;
            trash.AttachedFiles = message.Attachments;
            foreach (MimeEntity attachment in message.Attachments)
            {
                trash.attachedFile += attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                trash.attachedFile += ", ";
            }
            trash.Count++;
            model.modelCount++;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (model.Flag == 1) MyList.ItemsSource = model.InBox.Where(i => i.From.Contains(SearchBox.Text)).OrderByDescending(d => d.ID);
                if (model.Flag == 2) MyList.ItemsSource = model.Drafts.Where(i => i.From.Contains(SearchBox.Text)).OrderByDescending(d => d.ID);
                if (model.Flag == 3) MyList.ItemsSource = model.Send.Where(i => i.From.Contains(SearchBox.Text)).OrderByDescending(d => d.ID);
                if (model.Flag == 4) MyList.ItemsSource = model.Spam.Where(i => i.From.Contains(SearchBox.Text)).OrderByDescending(d => d.ID);
                if (model.Flag == 5) MyList.ItemsSource = model.Trash.Where(i => i.From.Contains(SearchBox.Text)).OrderByDescending(d => d.ID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Attached_Download_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxAttached.SelectedItem != null)
            {
                int save = 1;
                DownloadFile(save);
            }
        }
        private void Attached_Open_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxAttached.SelectedItem != null)
            {
                int open = 2;
                DownloadFile(open);
                if (model.Path.Contains(".png") || model.Path.Contains(".jpg")) Process.Start("explorer.exe", $@"{model.Path}");
                if (model.Path.Contains(".pdf")) Process.Start("explorer.exe", $@"{model.Path}");
            }
        }
        private void DownloadFile(int value)
        {
            try
            {
                if (ListBoxAttached.SelectedItem != null)
                {
                    if (value == 1)
                    {
                        model.Path = "";
                        CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                        dialog.IsFolderPicker = true;
                        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            model.Path = dialog.FileName + $@"\";
                        }
                    }
                    if (value == 2)
                    {
                        model.Path = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\";
                    }

                    string? fileName = ListBoxAttached.SelectedItem.ToString();
                    model.Path += fileName;
                    ShowMessage gettextmail = (ShowMessage)MyList.SelectedItem;
                    MyWebBrowser.NavigateToString(gettextmail.TextBody);
                    foreach (MimeEntity attachment in gettextmail.AttachedFiles.Where(n => n.ContentDisposition.FileName == fileName))
                    {
                        using (Stream stream = File.Create(model.Path))
                        {
                            if (attachment is MessagePart)
                            {
                                MessagePart rfc822 = (MessagePart)attachment;
                                rfc822.Message.WriteTo(stream);
                            }
                            else
                            {
                                MimePart part = (MimePart)attachment;
                                part.Content.DecodeTo(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SaveTextMenuItems_Click(object sender, RoutedEventArgs e)
        {
            if (MyList.SelectedItem != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.CreatePrompt = true;
                save.DefaultExt = ".txt";
                save.Filter = "Text documents (.txt)|*.txt";
                save.OverwritePrompt = true;
                if (save.ShowDialog() == true)
                {
                    if (MyList.SelectedItem != null)
                    {
                        var gettextmail = (ShowMessage)MyList.SelectedItem;
                        File.WriteAllText(save.FileName, gettextmail.TextBody);
                        MessageBox.Show("File saved", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        private async void DeletemessageMenuItems_Click(object sender, RoutedEventArgs e)
        {
            IMailFolder? folder = null;
            try
            {
                if (MyList.SelectedItem != null)
                {
                    if (client.IsConnected != true)
                    {
                        await Authenticate();
                    }
                    if (model.Flag == 1)
                    {
                        InBox gettextmail = (InBox)MyList.SelectedItem;
                        //Trash temp = model.Adapter(gettextmail);
                        //model.AddTrash(temp);
                        model.DeleteInbox(gettextmail);
                        client.Inbox.Open(FolderAccess.ReadWrite);
                        client.Inbox.MoveTo(gettextmail.ID, client.GetFolder(SpecialFolder.Trash));
                        await GetCountMessages();
                        await GetTrashMessages();
                    }
                    if (model.Flag == 2)
                    {
                        Drafts gettextmail = (Drafts)MyList.SelectedItem;
                        //Trash temp = model.Adapter(gettextmail);
                        //model.AddTrash(temp);
                        model.DeleteDrafts(gettextmail);
                        client.GetFolder(SpecialFolder.Drafts).Open(FolderAccess.ReadWrite);
                        folder = client.GetFolder(SpecialFolder.Drafts);
                        folder?.MoveTo(gettextmail.ID, client.GetFolder(SpecialFolder.Trash));
                        await GetCountMessages();
                        await GetTrashMessages();
                    }
                    if (model.Flag == 3)
                    {
                        Send gettextmail = (Send)MyList.SelectedItem;
                        //Trash temp = model.Adapter(gettextmail);
                        //model.AddTrash(temp);
                        model.DeleteSend(gettextmail);
                        client.GetFolder(SpecialFolder.Sent).Open(FolderAccess.ReadWrite);
                        folder = client.GetFolder(SpecialFolder.Sent);
                        folder?.MoveTo(gettextmail.ID, client.GetFolder(SpecialFolder.Trash));
                        await GetCountMessages();
                        await GetTrashMessages();
                    }
                    if (model.Flag == 4)
                    {
                        Spam gettextmail = (Spam)MyList.SelectedItem;
                        //Trash temp = model.Adapter(gettextmail);
                        //model.AddTrash(temp);
                        model.DeleteSpam(gettextmail);
                        client.GetFolder(SpecialFolder.Junk).Open(FolderAccess.ReadWrite);
                        folder = client.GetFolder(SpecialFolder.Junk);
                        folder?.MoveTo(gettextmail.ID, client.GetFolder(SpecialFolder.Trash));
                        await GetCountMessages();
                        await GetTrashMessages();
                    }
                    if (model.Flag == 5)
                    {
                        Trash gettextmail = (Trash)MyList.SelectedItem;
                        model.DeleteTrash(gettextmail);
                        client.GetFolder(SpecialFolder.Trash).Open(FolderAccess.ReadWrite);
                        folder = client.GetFolder(SpecialFolder.Trash);
                        folder?.AddFlags(gettextmail.ID, MessageFlags.Deleted, true);
                        folder?.Expunge();
                    }
                }
                else MessageBox.Show("Select a message", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                await Disconnect();
            }
        }
        private void ReplayMenuItems_Click(object sender, RoutedEventArgs e)
        {
            if (MyList.SelectedItem != null)
            {
                ShowMessage message = (ShowMessage)MyList.SelectedItem;
                MailWindow mailWindow = new MailWindow(message.From);
                mailWindow.Show();
            }
        }
        private void SortmessagesbySubjectMenuItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MyList.SelectedItem != null)
                {
                    IEnumerable<ShowMessage> gettextmail = (IEnumerable<ShowMessage>)MyList.ItemsSource;
                    IEnumerable<ShowMessage> SorderCollection = gettextmail.OrderBy(s => s.Subject);
                    MyList.ItemsSource = SorderCollection;
                    MyList.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SortmessagesbySenderMenuItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MyList.SelectedItem != null)
                {
                    IEnumerable<ShowMessage> gettextmail = (IEnumerable<ShowMessage>)MyList.ItemsSource;
                    IEnumerable<ShowMessage> SorderCollection = gettextmail.OrderBy(s => s.From);
                    MyList.ItemsSource = SorderCollection;
                    MyList.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SortmessagesbyDateMenuItems_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MyList.SelectedItem != null)
                {
                    IEnumerable<ShowMessage> gettextmail = (IEnumerable<ShowMessage>)MyList.ItemsSource;
                    IEnumerable<ShowMessage> SorderCollection = gettextmail.OrderByDescending(s => s.ID);
                    MyList.ItemsSource = SorderCollection;
                    MyList.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void InfoMenuItems_Click(object sender, RoutedEventArgs e)
        {
            if (MyList.SelectedItem != null)
            {
                ShowMessage message = (ShowMessage)MyList.SelectedItem;
                MessageBox.Show($@"{message.ID}" + "\n" + $@"{message.From}" + "\n" + $@"{message.Subject}" + "\n" + $@"{message.time}");
            }
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            model.LoadHTML(MyWebBrowser);
        }
        private void DarkMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                PaletteHelper palette = new PaletteHelper();
                ITheme theme = palette.GetTheme();

                if (DarkMode.IsChecked.Value)
                {
                    Themes();
                    ColorThemes.Value = true;
                }
                else
                {
                    Themes();
                    ColorThemes.Value = false;
                }
                palette.SetTheme(theme);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Themes()
        {
            try
            {
                PaletteHelper palette = new PaletteHelper();
                ITheme theme = palette.GetTheme();

                if (DarkMode.IsChecked.Value)
                {
                    BrushConverter bc = new BrushConverter();
                    MainBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    ImageBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    Button1Border.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    TextBoxBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    ThemesBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    BottomBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    CardBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    AttBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    MyWebBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                    theme.SetBaseTheme(Theme.Dark);
                }
                else
                {
                    AttBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    MainBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    ImageBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    Button1Border.Background = System.Windows.Media.Brushes.DarkGray;
                    TextBoxBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    ThemesBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    BottomBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    CardBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    AttBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    MyWebBorder.Background = System.Windows.Media.Brushes.DarkGray;
                    theme.SetBaseTheme(Theme.Light);
                }
                palette.SetTheme(theme);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
