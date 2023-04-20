using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MailClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ColorThemes.Value = true;
            //DarkMode.IsChecked = ColorThemes.Value;
            Themes();
        }

        private void Enter_Button_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password.ToString() != "")
            {
                //Regex regex = new Regex(@"^([\w\.\-]+)@(gmail)\.(com)$");
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\.\-])");
                Match match = regex.Match(LoginTextBox.Text);
                if (match.Success)
                {
                    User.Login = LoginTextBox.Text;
                    User.Password = PasswordBox.Password.ToString();
                    InBoxWindow inboxWindow = new InBoxWindow();
                    inboxWindow.Show();
                    Close();
                }
                else MessageBox.Show("Enter correct @gmail.com address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Please, enter correct information");
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            string IMAPserverAddress = ConfigurationManager.AppSettings["IMAP_gmail_server"]!;
            short IMAPserverPort = short.Parse(ConfigurationManager.AppSettings["IMAP_gmail_port"]!);
            string SMTPgmail_server = ConfigurationManager.AppSettings["SMTP_gmail_server"]!;
            short SMTPserverPort = short.Parse(ConfigurationManager.AppSettings["SMTP_gmail_port"]!);
            MessageBox.Show($@"Mail receiving Server Address: {IMAPserverAddress}" + "\n" + $@"Mail receiving Server Port: {IMAPserverPort}" + "\n" + $@"Mail sending Server Address: {SMTPgmail_server}" + "\n" + $@"Mail sending Server Port: {SMTPserverPort}", "Sever Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
                LoginBoxBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                PasswordBoxBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                ButtonBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                ThemesBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                BottomBorder.Background = (System.Windows.Media.Brush)bc.ConvertFrom("#212429");
                theme.SetBaseTheme(Theme.Dark);
            }
            else
            {
                MainBorder.Background = System.Windows.Media.Brushes.DarkGray;
                ImageBorder.Background = System.Windows.Media.Brushes.DarkGray;
                LoginBoxBorder.Background = System.Windows.Media.Brushes.DarkGray;
                PasswordBoxBorder.Background = System.Windows.Media.Brushes.DarkGray;
                ButtonBorder.Background = System.Windows.Media.Brushes.DarkGray;
                ThemesBorder.Background = System.Windows.Media.Brushes.DarkGray;
                BottomBorder.Background = System.Windows.Media.Brushes.DarkGray;
                theme.SetBaseTheme(Theme.Light);
            }
            palette.SetTheme(theme);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You know everything");
        }
    }
}
