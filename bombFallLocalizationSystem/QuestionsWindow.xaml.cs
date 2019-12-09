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
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class QuestionsWindow : Window
    {
        public QuestionsWindow()
        {
            InitializeComponent();
            flagImg.Source = new BitmapImage(new Uri(MainWindow.imageUri, UriKind.Relative));
        }

        #region Language Buttons Click Handlers
        private void EnButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.langDictionary == "AppStringsEN.xaml") { return; }
            MainWindow.langDictionary = "AppStringsEN.xaml";
            MainWindow.imageUri = "Resources/ukFlagIcon.png";
            try
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("Resources/" + MainWindow.langDictionary, UriKind.Relative);

                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not dynamically laod styles. Here\'s what the exception had to say:\n\n" + ex.Message);
            }
            MainWindow.currentLang = MainWindow.imageUri;
            ImageSource imgSrc = new BitmapImage(new Uri(MainWindow.imageUri, UriKind.Relative));
            this.flagImg.Source = imgSrc;
        }

        private void PtButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.langDictionary == "AppStringsPT.xaml") { return; }
            MainWindow.langDictionary = "AppStringsPT.xaml";
            MainWindow.imageUri = "Resources/brazilFlagIcon.png";
            try
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("Resources/" + MainWindow.langDictionary, UriKind.Relative);

                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not dynamically laod styles. Here\'s what the exception had to say:\n\n" + ex.Message);
            }
            MainWindow.currentLang = MainWindow.imageUri;
            ImageSource imgSrc = new BitmapImage(new Uri(MainWindow.imageUri, UriKind.Relative));
            flagImg.Source = imgSrc;
        }

        private void HbButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.langDictionary == "AppStringsHB.xaml") { return; }
            MainWindow.langDictionary = "AppStringsHB.xaml";
            MainWindow.imageUri = "Resources/israelFlagIcon.png";
            try
            {
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri("Resources/" + MainWindow.langDictionary, UriKind.Relative);

                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not dynamically laod styles. Here\'s what the exception had to say:\n\n" + ex.Message);
            }
            MainWindow.currentLang = MainWindow.imageUri;
            ImageSource imgSrc = new BitmapImage(new Uri(MainWindow.imageUri, UriKind.Relative));
            flagImg.Source = imgSrc;
        }
        #endregion

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(emailAddress.Content.ToString());
        }

        private void GmailButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.gmail.com");
        }
    }
}
