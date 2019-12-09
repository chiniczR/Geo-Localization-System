using bombFallLocalizationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Xml.Linq;
using DAL;
using Microsoft.Maps.MapControl.WPF;

namespace PL
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
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

        #region Validators for the Textboxes and the Datepicker
        private void LatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Decimal dummy;
            e.Handled = !Decimal.TryParse(e.Text, out dummy);
        }
        private void TimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void LeDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime.TryParse("2000-01-01", out DateTime minDate);
            if (leDatePicker.SelectedDate.Value.Ticks <= minDate.Ticks)
            {
                leDatePicker.Foreground = System.Windows.Media.Brushes.Crimson;
                leDatePicker.ToolTip = new ToolTip
                {
                    Content = badDate()
                };
            }
            else
            {
                leDatePicker.ToolTip = new ToolTip { Content = "OK" };
                leDatePicker.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
        #endregion

        #region Search Button Click Handler
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            disableThings();

            string addr = "";
            double lat = 0.0, lon = 0.0;
            int dev = 1;
            if ((leAddressTextBox.Text == "" || leAddressTextBox.Text == null)
                && (bool)byAddressRadio.IsChecked)
            {
                showNoAddress();
                goto END;
            }
            else if ((bool)byAddressRadio.IsChecked)
            {
                addr = leAddressTextBox.Text;
                // With HERE Geocoder API
                string appID = "AWOngGwwid3peeor8FB8";
                string appCode = "Aiq7YVehgUTJxBl9eukLQg";
                string formattedAddr = addr.Replace(", ", "%20").Replace(" ", "%20");
                string url = string.Format("https://geocoder.api.here.com/6.2/geocode.xml?app_id={0}&app_code={1}&searchtext={2}", appID, appCode, formattedAddr);

                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                XDocument xdoc = XDocument.Load(response.GetResponseStream());
                try
                {
                    XElement result = xdoc.Root.Element("Response").Element("View").Element("Result");
                    float.TryParse(result.Element("Location").Element("NavigationPosition").Element("Latitude").Value.ToString(), out float latF);
                    float.TryParse(result.Element("Location").Element("NavigationPosition").Element("Longitude").Value.ToString(), out float lonF);
                    lat = (double)System.Math.Round(latF, 3);
                    lon = (double)System.Math.Round(lonF, 3);
                }
                catch (Exception)
                {
                    ReportUserControl.showInvalidAddress();
                    goto END;
                }
            }

            if(((latTextBox.Text == "" || latTextBox.Text == null) ||
                (lonTextBox.Text == "" || lonTextBox.Text == null)) &&
                (bool)byCoordsRadio.IsChecked)
            {
                showNoCoords();
                goto END;
            }
            else if (byCoordsRadio.IsChecked.Value)
            {
                double.TryParse(latTextBox.Text, out lat);
                double.TryParse(lonTextBox.Text, out lon);
            }

            DateTime? leDate = leDatePicker.SelectedDate;
            DateTime.TryParse("1997-01-01", out DateTime minDate);
            bool noDate = (leDate == null || leDate.Value.Ticks <= minDate.Ticks);

            if (errorMarginTextBox.Text != null &&
                errorMarginTextBox.Text != "")
            {
                int.TryParse(errorMarginTextBox.Text, out dev);
            }

            List<Fall> falls = new List<Fall>();
            falls = MainWindow.GetCurrentVM().Falls.Where
                (f =>
                {
                    if (!noDate)    // If there is a date
                    {
                        int year = leDate.Value.Year;
                        int month = leDate.Value.Month;
                        int day = leDate.Value.Day;
                        if (f.date.Day != day ||
                        f.date.Month != month ||
                        f.date.Year != year)
                        {
                            return false;
                        }
                    }
                    // If only geotagged
                    if (geotagCheck.IsChecked.Value
                    && !f.isGeotagged)  
                    {
                        return false;
                    }
                    double d = calculateActDist(f, lat, lon);
                    return (d <= dev);
                }).ToList();

            fallsListBox.ItemsSource = new List<string>();
            fallsView.Children.Clear();

            // If there were no results, this is where we want to stop
            if (falls.Count() < 1) { goto END; }

            fallsListBox.ItemsSource = falls.OrderBy(f => f.date).Select(f => f.ToString());
            foreach (var fall in falls)
            {
                fallsView.Children.Add(
                    new Pushpin
                    {
                        Name = "pushpin" + fall.id.ToString(),
                        Location = new Location(fall.x, fall.y),
                        Background = System.Windows.Media.Brushes.Orange
            });
            }
            fallsView.Center = ((Pushpin)fallsView.Children[0]).Location;
            fallsView.ZoomLevel = 4 + (falls.Count() % 3) + 1;

            END:;
            enableThings();
        }
        #endregion

        #region Distance Calculation Helpers
        private double calculateActDist(Fall fall, double NewX, double NewY)
        {
            // Haversine formula:
            //  a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
            //  c = 2 ⋅ atan2( √a, √(1−a) )
            //  d = R ⋅ c   <-- Spherical distance

            double oldX = fall.x, oldY = fall.y;

            var R = 6371e3; // Metres
            var φ1 = degreeToRadian(oldX);
            var φ2 = degreeToRadian(NewX);
            var Δφ = degreeToRadian(NewX - oldX);
            var Δλ = degreeToRadian(NewY - oldY);

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double dist = (R * c) / 1000; // Back to Km

            // Since we only really care about the first three digits
            // and to prevent overloadings:
            if (dist < 0.001)
            {
                dist = 0.0;
            }
            return dist;
        }
        private double degreeToRadian(double angle)
        {
            // Needed because Math.<trig_functions> expect radians
            return Math.PI * angle / 180.0;
        }
        #endregion

        #region Address and Coordenates Check Handlers
        private void ByCoordsRadio_Checked(object sender, RoutedEventArgs e)
        {
            leAddressTextBox.IsEnabled = false;
            leAddressTextBox.Text = "";
            latTextBox.IsEnabled = true;
            lonTextBox.IsEnabled = true;
        }
        private void ByAddressRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (leAddressTextBox == null)
            { return; }

            leAddressTextBox.IsEnabled = true;
            latTextBox.IsEnabled = false;
            latTextBox.Text = "";
            lonTextBox.IsEnabled = false;
            lonTextBox.Text = "";
        }
        #endregion

        #region Selection Change Handler
        private void FallsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> selectedNames = new List<string>();

            foreach (var fall in fallsListBox.SelectedItems)
            {
                int.TryParse(fall.ToString().Split(':').ElementAt(1).
                    Replace(", LAT", ""), out int leId);
                selectedNames.Add("pushpin" + leId);
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

        #region Messangers
        private void showNoAddress()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please make sure you\'ve specified the address.";
                    title = "Something\'s Missing...";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Por favor verifique-se que você especificou o endereço.";
                    title = "Está faltando algo...";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "נא לוודא שרשמת את הכתובת.";
                    title = "חסר משהו...";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showNoCoords()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please make sure you\'ve specified the coordinates.";
                    title = "Something\'s Missing...";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Por favor verifique-se que você especificou as coordenadas.";
                    title = "Está faltando algo...";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "נא לוודא שרשמת את קורדינטות.";
                    title = "חסר משהו...";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private string badDate()
        {
            string message = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Valid dates start from 01/01/1997.\nDates until then will simply be ignored.";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Datas válidas começam a partir de 01/01/1997.\nDatas anteriores serão ignoradas.";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ".תאריך תקינים מתחילים מ-01.01.1997. לפני תאריך זה, המערכת תתעלם מהתאריך";
                    break;
                default: break;
            }
            return message;
        }
        #endregion

        #region Helpers to Enable/Disable the Controls While a Search Being Made
        private void disableThings()
        {
            byAddressRadio.IsEnabled = false;
            byCoordsRadio.IsEnabled = false;
            leAddressTextBox.IsEnabled = false;
            latTextBox.IsEnabled = false;
            lonTextBox.IsEnabled = false;
            errorMarginTextBox.IsEnabled = false;
            leDatePicker.IsEnabled = false;
            geotagCheck.IsEnabled = false;
            SearchButton.IsEnabled = false;
        }

        private void enableThings()
        {
            byAddressRadio.IsEnabled = true;
            byCoordsRadio.IsEnabled = true;
            if (byAddressRadio.IsChecked.Value)
            {
                leAddressTextBox.IsEnabled = true;
            }
            else
            {
                latTextBox.IsEnabled = true;
                lonTextBox.IsEnabled = true;
            }
            errorMarginTextBox.IsEnabled = true;
            leDatePicker.IsEnabled = true;
            geotagCheck.IsEnabled = true;
            SearchButton.IsEnabled = true;
        }
        #endregion
    }
}
