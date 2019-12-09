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
    /// Interaction logic for ShareWindow.xaml
    /// </summary>
    public partial class ShareWindow : Window
    {
        public ShareWindow()
        {
            InitializeComponent();
            flagImg.Source = new BitmapImage(new Uri(MainWindow.imageUri, UriKind.Relative));
            webBrowser.Navigate(new Uri("https://www.google.com"));
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
        
        #region Back and Forward Buttons Click Handlers
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (webBrowser.CanGoBack)
            {
                webBrowser.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if(webBrowser.CanGoForward)
            {
                webBrowser.GoForward();
            }
        }
        #endregion

        #region Social Media Buttons Click Handlers
        private void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            // The intent was to take the user to a specific twitter account
            webBrowser.Navigate(new Uri("https://www.twitter.com"));
        }
        private void FacebookButton_Click(object sender, RoutedEventArgs e)
        {
            // The intent was to take the user to a specific facebook account
            webBrowser.Navigate(new Uri("https://www.facebook.com"));
        }
        private void RedditButton_Click(object sender, RoutedEventArgs e)
        {
            // The intent was to take the user to a specific subreddit
            webBrowser.Navigate(new Uri("https://www.reddit.com"));
        }
        private void InstagramButton_Click(object sender, RoutedEventArgs e)
        {
            // The intent was to take the user to a specific instagram account
            webBrowser.Navigate(new Uri("https://www.instagram.com"));
        }
        private void LinkedinButton_Click(object sender, RoutedEventArgs e)
        {
            // Would take the user to the system's linkedin page, if it had one
            webBrowser.Navigate(new Uri("https://www.linkedin.com"));
        }
        #endregion
    }
}
