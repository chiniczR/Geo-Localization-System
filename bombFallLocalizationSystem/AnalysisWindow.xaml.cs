using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bombFallLocalizationSystem;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using PL;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;


namespace PL
{
    /// <summary>
    /// Interaction logic for AnalysisWindow.xaml
    /// </summary>
    public partial class AnalysisWindow : Window
    {
        public string fullUserName { get; set; }
        public SeriesCollection Series { get; set; }
        
        public AnalysisWindow()
        {
            InitializeComponent();
            flagImg.Source = new BitmapImage(new Uri(MainWindow.imageUri, UriKind.Relative));

            #region User Set-Up
            // Setting up the user's full name and profile picture (locally store,
            // the entry only contains the full local path to it)
            string username = MainWindow.CurrentUser.username;
            MainWindow.userVM = new ViewModels.UserVM();
            MainWindow.CurrentUser = MainWindow.userVM.
                Users.Where(f => f.username.Trim() 
                == username.Trim()).FirstOrDefault();
            fullUserName = MainWindow.CurrentUser.firstname.Trim()
                + " " + MainWindow.CurrentUser.lastname.Trim();
            profileNameLabel.Content = fullUserName;
            if (MainWindow.CurrentUser.photoUrl.Trim() != "not-set")
            {
                try
                {
                    profilePic.Source = new BitmapImage(
                        new Uri(MainWindow.CurrentUser.photoUrl.Trim(),
                        UriKind.Absolute));
                }
                catch (Exception e)
                {
                    if (!e.Message.StartsWith("Could not find file"))
                    {
                        UpdateUserControl.showSomethingWrong();
                    }
                }
            }
            #endregion

            #region Big Line-Series Chart Set-Up
            // Setting up the inner charts for the big chart of falls by year
            ChartValues<double> chart1 = new ChartValues<double> { 0 };
            // First, all falls
            for (int i = 2003; i < 2020; i++)
            {
                chart1.Add(fallsPerYear(i));
            }
            // Then only those that have been exactly located (i.e. geotagged)
            ChartValues<double> chart2 = new ChartValues<double> { 0 };
            for (int i = 2003; i < 2020; i++)
            {
                chart2.Add(fallsPerYear(i, true));
            }
            // Putting it all together...
            Series = new SeriesCollection
            {
                new LineSeries
                {
                    Name = "Total",
                    Values = chart1,
                    Title = "Total"
                },
                new LineSeries
                {
                    Name = "Geotagged",
                    Values = chart2,
                    Title = "Geotagged"
                },
            };
            leChart.Series = Series;
            #endregion

            // That's the label on the bottom, rightmost corner
            avgActDistLabel.Content = calcAvgActDist();

            #region Doughnut Chart Set-Up
            // Now preparing the doughnut chart of exactly located falls, separated by
            // the distance between their estiamted and their actual locations
            var fallsByActDist = 
            MainWindow.GetCurrentVM().Falls.Where(
                f => f.isGeotagged).GroupBy(
                f => f.actDist
                );
            // Declaring the collection...
            SeriesCollection PieCollection = new SeriesCollection();
            foreach (var fallGroup in fallsByActDist)
            {
                PieCollection.Add(
                new PieSeries
                {
                    Title = fallGroup.Key.ToString(),
                    Values = new ChartValues<ObservableValue>
                    { new ObservableValue(fallGroup.Count()) },
                    DataLabels = true
                });
            }
            lePieChart.Series = PieCollection;
            #endregion

            #region Gauge Chart Set-Up
            // Finally, setting up the gauge chart for the system's accuracy rate accord-
            // ing to a user define (from the slider) desired error margin (deviation)
            double accuracyKm = slValue.Value;
            int pass = MainWindow.GetCurrentVM().Falls.Where(
                f => f.isGeotagged &&
                f.actDist <= accuracyKm).Count();
            int all = MainWindow.GetCurrentVM().Falls.Where
                (f => f.isGeotagged).Count();
            int rate = Convert.ToInt32(Math.Floor(((double)pass / all) * 100.0));
            GaugeChart.Value = rate;
            #endregion
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

        #region Distance-related Methods
        private string calcAvgActDist()
        {
            List<double> actDists = new List<double>();
            actDists = MainWindow.GetCurrentVM().Falls.Where(f => f.isGeotagged).Select(f => f.actDist).ToList();
            double avgActDist = actDists.Average();

            int n = 0;
            if (avgActDist / 10 < 1)
            { n = 5; }
            else { n = 6; }
            return avgActDist.ToString().Substring(0,n) + " Km";
        }
        private void MiRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            double.TryParse(avgActDistLabel.Content.ToString().Replace(" Km",""),
                out double dist);
            dist *= 0.62137119;
            int n = 0;
            if (dist / 10 < 1)
            { n = 5; }
            else { n = 6; }
            avgActDistLabel.Content = dist.ToString().Substring(0, n) + " Mi";
        }
        private void KmRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (avgActDistLabel == null)
            {
                return;
            }
            double.TryParse(avgActDistLabel.Content.ToString().Replace(" Mi", ""),
                            out double dist);
            dist = dist / 0.62137119;
            int n = 0;
            if (dist / 10 < 1)
            { n = 5; }
            else { n = 6; }
            avgActDistLabel.Content = dist.ToString().Substring(0, n) + " Km";
        }
        #endregion

        #region Falls/Year Calculator
        private int fallsPerYear(int year, bool onlyGeotagged=false)
        {
            List<DAL.Fall> falls = new List<DAL.Fall>();

            if (!onlyGeotagged)
            {
                falls = MainWindow.GetCurrentVM().Falls.Where
                        (f => f.date.ToString().Contains
                        (year.ToString())).ToList();
            }
            else
            {
                falls = MainWindow.GetCurrentVM().Falls.Where
                        (f => f.date.ToString().Contains
                        (year.ToString()) && 
                        f.isGeotagged).ToList();
            }

            return falls.Count();
        }
        #endregion

        #region Other Control Event Handlers
        private void SelectProfilePicButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            //For any other formats
            of.Filter = "Image Files (*.jpg;*.jpeg)|*.BMP;*.JPG;*.JPEG;*.PNG";
            try
            {   // Just in case...
                if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileInfo imageFile = new FileInfo(of.FileName);
                    profilePic.Source = new BitmapImage(new Uri(imageFile.FullName, UriKind.Absolute));
                    MainWindow.userVM.CurrentModel.UpdateUser(
                        MainWindow.CurrentUser.username.Trim(),
                        imageFile.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+"\n\t\t\t---\n"+ex.StackTrace);
            }
        }
        private void SlValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            slValue.IsEnabled = false;
            accuracyText.IsEnabled = false;
            double accuracyKm = slValue.Value;
            int pass = MainWindow.GetCurrentVM().Falls.Where(
                f => f.isGeotagged &&
                f.actDist <= accuracyKm).Count();
            int all = MainWindow.GetCurrentVM().Falls.Where
                (f => f.isGeotagged).Count();
            int rate = Convert.ToInt32(Math.Floor(((double)pass / all) * 100.0));
            GaugeChart.Value = rate;
            slValue.IsEnabled = true;
            accuracyText.IsEnabled = true;
        }
        #endregion
    }
}
