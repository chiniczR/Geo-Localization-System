using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Maps.MapControl.WPF;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using PL.Models;
using DAL;
using PL.ViewModels;
using PL;
using System.Xml.Linq;

namespace bombFallLocalizationSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        #region Static Variables and Atributes
        private static FallVM CurrentVM { get; set; }
        public static UserVM userVM { get; set; }

        public static user CurrentUser;

        public static string currentLang;

        public static bool loggedIn;

        public static AnalysisWindow aw;
        public static SearchWindow sw;
        public static AboutWindow abw;
        public static ShareWindow shw;
        public static QuestionsWindow qw;

        public static string langDictionary;
        public static string imageUri = "Resources/ukFlagIcon.png";

        private MainUserControl main { get; set; }

        private LoginUserControl login { get; set; }

        private ReportUserControl report { get; set; }

        private UpdateUserControl update { get; set; }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            #region Variables Initialization
            loggedIn = false;
            CurrentVM = new FallVM();
            userVM = new UserVM();
            currentLang = "Resources/ukFlagIcon.png";
            CurrentUser = new user();
            CurrentUser.firstname = "Guest";
            main = new MainUserControl();
            login = new LoginUserControl(this);
            report = new ReportUserControl(this);
            #endregion

            #region Preparing the Source of the Falls Listbox
            userControlGrid.Children.Add(main);
            foreach (var fall in CurrentVM.Falls)
            {
                Pushpin pushpin = new Pushpin();
                pushpin.Name = "pushpin" + fall.id.ToString();
                pushpin.Location = new Location(fall.x, fall.y);
                pushpin.Background = System.Windows.Media.Brushes.Orange;
                main.fallsView.Children.Add(pushpin);
            }
            main.fallsListBox.ItemsSource = CurrentVM.Falls.OrderByDescending(fall => fall.date);
            #endregion

            #region Handling the previous "Recent" Reports
            // We want to make sure that, if recentFalls still reports from a previous day(s),
            // it will be cleared before we start storing reports on it today.
            XElement prevFallsRoot = XElement.Load("recentFalls.xml");
            if (prevFallsRoot.HasElements)
            {
                IEnumerable<XElement> prevFallsList = prevFallsRoot.Elements("Fall").ToList();
                DateTime lastReport = new DateTime(0);
                foreach (XElement fallRep in prevFallsList)
                {
                    DateTime leDate = DateTime.Now;
                    DateTime.TryParse(fallRep.Element("Date").Value, out leDate);
                    if (leDate.CompareTo(lastReport) > 0)
                    {
                        lastReport = leDate;
                    }
                }
                if (lastReport.Day != DateTime.Now.Day || lastReport.Month != DateTime.Now.Month || lastReport.Year != DateTime.Now.Year)
                {
                    // This is where we'd first save the previous days reports on a more permanent
                    // storage on the cloud, say on Azure, if I had one
                    File.WriteAllText(@"recentFalls.xml", string.Empty);
                }
            }
            #endregion
        }

        #region Main Menu Buttons Click Handlers
        public void MainButton_Click(object sender, RoutedEventArgs e)
        {
            mainButton.BorderThickness = new Thickness(0,0,0,2);
            mainButton.FontWeight = FontWeights.ExtraBold;
            loginButton.BorderThickness = new Thickness(0,0,0,0);
            loginButton.FontWeight = FontWeights.Normal;
            reportButton.BorderThickness = new Thickness(0, 0, 0, 0);
            reportButton.FontWeight = FontWeights.Normal;
            updateButton.BorderThickness = new Thickness(0, 0, 0, 0);
            updateButton.FontWeight = FontWeights.Normal;
            userControlGrid.Children.RemoveAt(0);
            main = new MainUserControl();
            userControlGrid.Children.Add(main);
            CurrentVM = new FallVM();
            foreach (var fall in CurrentVM.Falls)
            {
                Pushpin pushpin = new Pushpin();
                pushpin.Name = "pushpin" + fall.id.ToString();
                pushpin.Location = new Location(fall.x, fall.y);
                pushpin.Background = System.Windows.Media.Brushes.Orange;
                main.fallsView.Children.Add(pushpin);
            }
            main.fallsListBox.ItemsSource = CurrentVM.Falls.OrderByDescending(fall => fall.date);
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser.firstname != "Guest")
            {
                LoginUserControl.showAlreadyLoggedIn();
                return;
            }
            loginButton.BorderThickness = new Thickness(0, 0, 0, 2);
            loginButton.FontWeight = FontWeights.ExtraBold;
            mainButton.BorderThickness = new Thickness(0, 0, 0, 0);
            mainButton.FontWeight = FontWeights.Normal;
            updateButton.BorderThickness = new Thickness(0, 0, 0, 0);
            updateButton.FontWeight = FontWeights.Normal;
            reportButton.BorderThickness = new Thickness(0, 0, 0, 0);
            reportButton.FontWeight = FontWeights.Normal;
            userControlGrid.Children.RemoveAt(0);
            login = new LoginUserControl(this);
            userControlGrid.Children.Add(login);
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser.firstname != "Guest")
            {
                showGoodbye();
                CurrentUser = new user();
                CurrentUser.firstname = "Guest"; 
                ChangeUser();
            }
            if (aw.IsLoaded) { aw.Close(); }
            if (userControlGrid.Children[0].GetType() == typeof(ReportUserControl)
                || userControlGrid.Children[0].GetType() == typeof(UpdateUserControl))
            {
                MainButton_Click(this, null);
            }
        }
        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser.firstname == "Guest")
            {
                showMustLogIn();
                return;
            }
            else if (MainWindow.CurrentUser.role.Trim() != "callcenter")
            {
                showMustBeCallcenter();
                return;
            }
            reportButton.BorderThickness = new Thickness(0, 0, 0, 2);
            reportButton.FontWeight = FontWeights.ExtraBold;
            mainButton.BorderThickness = new Thickness(0, 0, 0, 0);
            mainButton.FontWeight = FontWeights.Normal;
            loginButton.BorderThickness = new Thickness(0, 0, 0, 0);
            loginButton.FontWeight = FontWeights.Normal;
            updateButton.BorderThickness = new Thickness(0, 0, 0, 0);
            updateButton.FontWeight = FontWeights.Normal;
            userControlGrid.Children.RemoveAt(0);
            report = new ReportUserControl(this);
            userControlGrid.Children.Add(report);
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser.firstname == "Guest")
            {
                showMustLogIn();
                return;
            }
            else if (MainWindow.CurrentUser.role.Trim() != "callcenter")
            {
                showMustBeCallcenter();
                return;
            }
            // Must update after creating UpdateUserControl
            updateButton.BorderThickness = new Thickness(0, 0, 0, 2);
            updateButton.FontWeight = FontWeights.ExtraBold;
            reportButton.BorderThickness = new Thickness(0, 0, 0, 0);
            reportButton.FontWeight = FontWeights.Normal;
            mainButton.BorderThickness = new Thickness(0, 0, 0, 0);
            mainButton.FontWeight = FontWeights.Normal;
            loginButton.BorderThickness = new Thickness(0, 0, 0, 0);
            loginButton.FontWeight = FontWeights.Normal;
            userControlGrid.Children.RemoveAt(0);
            update = new UpdateUserControl();
            userControlGrid.Children.Add(update);
        }
        #endregion
        
        #region Language Buttons Click Handlers
        public void EnButton_Click(object sender, RoutedEventArgs e)
        {
            if (langDictionary == "AppStringsEN.xaml") { return; }
            langDictionary = "AppStringsEN.xaml";
            imageUri = "Resources/ukFlagIcon.png";
            changeLang();
        }

        public void PtButton_Click(object sender, RoutedEventArgs e)
        {
            if (langDictionary == "AppStringsPT.xaml") { return; }
            langDictionary = "AppStringsPT.xaml";
            imageUri = "Resources/brazilFlagIcon.png";
            changeLang();
        }

        public void HbButton_Click(object sender, RoutedEventArgs e)
        {
            if (langDictionary == "AppStringsHB.xaml") { return; }
            langDictionary = "AppStringsHB.xaml";
            imageUri = "Resources/israelFlagIcon.png";
            changeLang();
        }
        #endregion

        #region Helper Methods
        public void changeLang()
        {
            try
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("Resources/" + langDictionary, UriKind.Relative);

                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not dynamically laod styles. Here\'s what the exception had to say:\n\n" + e.Message);
            }
            currentLang = imageUri;
            ImageSource imgSrc = new BitmapImage(new Uri(imageUri, UriKind.Relative));
            this.flagImg.Source = imgSrc;
            this.ChangeUser();
        }
        public static FallVM GetCurrentVM() { return MainWindow.CurrentVM; }
        public void ChangeUser()
        {
            string welcomeStr = "";
            string userName = CurrentUser.firstname.Trim();
            switch (currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    welcomeStr = "Welcome, ";
                    break;
                case "Resources/brazilFlagIcon.png":
                    welcomeStr = "Bem vindo/a, ";
                    break;
                case "Resources/israelFlagIcon.png":
                    welcomeStr = "ברוך/ה הבא/ה, ";
                    break;
                default: break;
            }
            welcomeLabel.Content = welcomeStr + userName + "!";
        }
        public static string removeForbiddenChars(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c) && !Char.IsPunctuation(c))
                .ToArray());
        }
        #endregion

        #region Messangers
        public static void showMustLogIn()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "This operation requires a logon. Please log into your account before entering this page.";
                    title = "Must Login";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Esta operação requer login. Favor fazer login antes de entrar nesta página.";
                    title = "Login Necessário";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "פעולה זו דורשת התחברות למערכת. נא להתחבר לפני כניסה לעמוד הזה.";
                    title = "דרושה התחברות";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        public static void showMustBeCallcenter()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "This operation requires your employee role to be \"callcenter\", but yours is \"" +
                        CurrentUser.role.Trim() +
                        "\". If this is incorrect, please contact your administrator.";
                    title = "Must Be Call-Center Employee";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Esta operação requer que sua função seja \"callcenter\", mas a sua é \"" +
                        CurrentUser.role.Trim() +
                        "\". Se isto não está correto, favor entrar em contato com seu/ua administrador(a).";
                    title = "Função de Call-Center Necessária";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ".אם דבר זה לא נכון, נא לפנות למנהלך .\"analysis\" אבל תפקידך \"callcenter\" פעולה זו נדרשת תפקיד";
                    title = "Call-Center דרושה תפקיד";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showGoodbye()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Goodbye, " + CurrentUser.firstname.Trim() + "!";
                    title = "Have a Good Day!";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Adeus, " + CurrentUser.firstname.Trim() + "!";
                    title = "Bom dia!";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = " ,להתראות" + CurrentUser.firstname.Trim() + "!";
                    title = "!יום טוב";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }
}
