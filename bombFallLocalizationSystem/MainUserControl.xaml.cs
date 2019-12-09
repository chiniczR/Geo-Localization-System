using bombFallLocalizationSystem;
using DAL;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        public MainUserControl()
        {
            InitializeComponent();
        }

        #region Selection Change Handler
        private void FallsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> selectedNames = new List<string>();

            foreach (var fall in fallsListBox.SelectedItems)
            {
                selectedNames.Add("pushpin" + ((Fall)fall).id.ToString());
            }

            foreach (var pushpin in fallsView.Children)
            {
                if (pushpin is Pushpin)
                {
                    Pushpin pp = (Pushpin)pushpin;
                    if (selectedNames.Contains(pp.Name))
                    {
                        pp.Background = System.Windows.Media.Brushes.Red;
                        fallsView.Center = pp.Location;
                    }
                    else
                    {
                        pp.Background = System.Windows.Media.Brushes.Orange;
                    }
                }
            }
        }
        #endregion

        #region Personal Menu Buttons Click Handlers
        private void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser.firstname == "Guest")
            {
                MainWindow.showMustLogIn();
                return;
            }
            else if (MainWindow.CurrentUser.role.Trim() != "analysis")
            {
                showMustBeAnalysis();
                return;
            }
            MainWindow.aw = new AnalysisWindow();
            MainWindow.aw.Show();
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.sw = new SearchWindow();
            MainWindow.sw.Show();
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.abw = new AboutWindow();
            MainWindow.abw.Show();
        }
        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.shw = new ShareWindow();
            MainWindow.shw.Show();
        }
        private void QuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.qw = new QuestionsWindow();
            MainWindow.qw.Show();
        }
#endregion

        #region Messangers
        public static void showMustBeAnalysis()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "This operation requires your employee role to be \"analysis\", but yours is \"" +
                        MainWindow.CurrentUser.role.Trim() +
                        "\". If this is incorrect, please contact your administrator.";
                    title = "Must Be Analysis Employee";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Esta operação requer que sua função seja \"analysis\", mas a sua é \"" +
                        MainWindow.CurrentUser.role.Trim() +
                        "\". Se isto não está correto, favor entrar em contato com seu/ua administrador(a).";
                    title = "Função de Análise Necessária";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ".אם דבר זה לא נכון, נא לפנות למנהלך .\"callcenter\" אבל תפקידך \"analysis\" פעולה זו נדרשת תפקיד";
                    title = "Analysis דרושה תפקיד";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        #endregion
    }
}
