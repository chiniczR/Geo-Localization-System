using bombFallLocalizationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Interaction logic for LoginUserControl.xaml
    /// </summary>
    public partial class LoginUserControl : UserControl
    {
        public MainWindow win;

        public LoginUserControl(MainWindow window)
        {
            InitializeComponent();
            win = window;
            if(MainWindow.CurrentUser.firstname != "Guest")
            {
                showAlreadyLoggedIn();
            }
        }

        private void ActualLoginButton_Click(object sender, RoutedEventArgs e)
        {
            actualLoginButton.IsEnabled = false;
            usernameTextBox.IsEnabled = false;
            passwordBox.IsEnabled = false;

            string username, password;
            username = usernameTextBox.Text;
            password = passwordBox.Password;
            if(username == null || username == "" || password == null || password == "")
            {
                showMissingSomething();
            }
            else
            {
                validateUser(username, password);
            }

            passwordBox.IsEnabled = true;
            usernameTextBox.IsEnabled = true;
            actualLoginButton.IsEnabled = true;
        }

        #region Messangers
        private void showMissingSomething()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please make sure you\'ve entered something in either the username or the password field(s).";
                    title = "Something\'s Missing...";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Por favor verifique-se que você digitou algo no(s) campo(s) nome de usuário ou senha.";
                    title = "Está faltando algo...";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "נא לוודא שרשמת משהו בשדה(ות) שם משתמש או סיסמה.";
                    title = "חסר משהו...";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showSuccess()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "You have successfully logged in.";
                    title = "Success";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Entrada ao sistema realizada com sucesso.";
                    title = "Sucesso";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "התחברת בהצלחה למערכת.";
                    title = "הצלחה";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void showAlreadyLoggedIn()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "You are already logged in. Please log out before logging in with a different account.";
                    title = "Already Logged In";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Você já está logado. Favor sair do sistema antes de entrar de novo com outra conta.";
                    title = "Já Logado";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "את/ה כבר מחובר למערכת. נא לצאת לפני להתחבר עם חשבון אחר.";
                    title = "כבר מחובר";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        #endregion

        #region Validate and Set Up User
        private void validateUser(string username, string password)
        {
            DAL.user leUser = new DAL.user();
            leUser.firstname = "Guest";
            foreach(DAL.user user in MainWindow.userVM.Users)
            {
                if(user.username.Trim() == username && user.password.Trim() == password)
                {
                    leUser = user;
                    break;
                }
            }
            if(leUser.firstname != "Guest")
            {
                MainWindow.CurrentUser = leUser;
                invalidTryLabel.Visibility = Visibility.Hidden;
                showSuccess();
                win.ChangeUser();
                usernameTextBox.Clear();
                passwordBox.Clear();
                win.MainButton_Click(win.mainButton, null);
            }
            else
            {
                invalidTryLabel.Visibility = Visibility.Visible;
            }
        }
        #endregion
    }
}
