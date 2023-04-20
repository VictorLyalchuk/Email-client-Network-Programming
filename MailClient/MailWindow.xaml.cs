using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace MailClient
{
    public partial class MailWindow : Window
    {
        string serverAddress;
        short serverPort;
        List<string> AttachmentPuth = new List<string>();
        public MailWindow()
        {
            InitializeComponent();
            From.Text = User.Login;
            serverAddress = ConfigurationManager.AppSettings["SMTP_gmail_server"]!;
            serverPort = short.Parse(ConfigurationManager.AppSettings["SMTP_gmail_port"]!);
            DarkMode.IsChecked = ColorThemes.Value;
            Themes();
        }
        public MailWindow(string ReplayEMail)
        {
            InitializeComponent();
            From.Text = User.Login;
            To.Text = ReplayEMail;
            serverAddress = ConfigurationManager.AppSettings["SMTP_gmail_server"]!;
            serverPort = short.Parse(ConfigurationManager.AppSettings["SMTP_gmail_port"]!);
            DarkMode.IsChecked = ColorThemes.Value;
            Themes();
        }
        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            SendButton.IsEnabled = false;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\.\-])");
            Match match = regex.Match(To.Text);
            if (match.Success)
            {
                string richText = new TextRange(MyRichText.Document.ContentStart, MyRichText.Document.ContentEnd).Text;
                MailMessage mailMessage = new MailMessage(From.Text, To.Text, Subject.Text, richText);
                string Priority = ((ListBoxItem)ListBoxChip.SelectedItem).Name.ToString();
                switch (Priority)
                {
                    case "Low":
                        mailMessage.Priority = MailPriority.Low;
                        break;
                    case "Normal":
                        mailMessage.Priority = MailPriority.Normal;
                        break;
                    case "High":
                        mailMessage.Priority = MailPriority.High;
                        break;
                    default:
                        mailMessage.Priority = MailPriority.Normal;
                        break;
                }
                foreach (var puthFile in AttachmentPuth)
                {
                    mailMessage.Attachments.Add(new Attachment($@"{puthFile}"));
                }
                SmtpClient smtpClient = new SmtpClient(serverAddress, serverPort);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(User.Login, User.Password);
                smtpClient.SendCompleted += Client_SendCompleted;
                smtpClient.SendAsync(mailMessage, mailMessage);
            }
            else MessageBox.Show("Enter correct mail address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Att_File_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                //attachedFiles.Items.Add(dialog.SafeFileName);
                attachedFiles.Items.Add(dialog.FileName);
                AttachmentPuth.Add(dialog.FileName);
            }
        }
        private void Client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            MailMessage state = (MailMessage)e.UserState;
            MessageBox.Show($"Message was sent! Subject: {state?.Subject}!");
            SendButton.IsEnabled = true;
            Close();
        }
        private void ContextDeleteMenuItems_Click(object sender, RoutedEventArgs e)
        {
            AttachmentPuth.Remove(attachedFiles.SelectedItem.ToString()!);
            attachedFiles.Items.Remove(attachedFiles.SelectedItem);
        }

        private void DarkMode_Checked(object sender, RoutedEventArgs e)
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
        private void Themes()
        {
            PaletteHelper palette = new PaletteHelper();

            ITheme theme = palette.GetTheme();

            if (DarkMode.IsChecked.Value)
            {
                BrushConverter bc = new BrushConverter();
                MainBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                ImageBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                Button1Border.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                Button2Border.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                SetBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                ThemesBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                BottomBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                SetBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                PriorityBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                RichTextBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                theme.SetBaseTheme(Theme.Dark);

            }
            else
            {
                MainBorder.Background = System.Windows.Media.Brushes.DarkGray;
                ImageBorder.Background = System.Windows.Media.Brushes.DarkGray;
                Button1Border.Background = System.Windows.Media.Brushes.DarkGray;
                Button2Border.Background = System.Windows.Media.Brushes.DarkGray;
                SetBorder.Background = System.Windows.Media.Brushes.DarkGray;
                ThemesBorder.Background = System.Windows.Media.Brushes.DarkGray;
                BottomBorder.Background = System.Windows.Media.Brushes.DarkGray;
                SetBorder.Background = System.Windows.Media.Brushes.DarkGray;
                PriorityBorder.Background = System.Windows.Media.Brushes.DarkGray;
                RichTextBorder.Background = System.Windows.Media.Brushes.DarkGray;
                theme.SetBaseTheme(Theme.Light);
            }
            palette.SetTheme(theme);
        }
    }
}
