using bombFallLocalizationSystem;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using DAL;
using System.IO;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;
using ExifLib;

namespace PL
{
    /// <summary>
    /// Interaction logic for UpdateUserControl.xaml
    /// </summary>
    public partial class UpdateUserControl : UserControl
    {
        public UpdateUserControl()
        {
            InitializeComponent();
            List<string> currentFalls = new List<string>();
            currentFalls.Add("---");
            foreach (Fall fall in MainWindow.GetCurrentVM().Falls)
            {
                if (!fall.isGeotagged)
                {
                    currentFalls.Add(fall.ToString());
                } 
            }
            fallSelector.ItemsSource = currentFalls;
            fallSelector.SelectedItem = fallSelector.Items.GetItemAt(0);
        }

        #region Selection Changed Handler
        private void ComboBoxItem_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (fallSelector.SelectedItem == null)
            {
                return;
            }

            picSelectButton.IsEnabled = false;
            if (fallSelector.SelectedItem.ToString() == "---")
            {
                selectedFallMap.Children.Clear();
                if (latText.Text != "?")
                {
                    Pushpin pushpin = new Pushpin();
                    pushpin.Name = "pushpin1";
                    double la = 0.0, lo = 0.0;
                    double.TryParse(latText.Text, out la);
                    double.TryParse(lonText.Text, out lo);
                    pushpin.Location = new Location(la, lo);
                    pushpin.Background = System.Windows.Media.Brushes.Gainsboro;
                    selectedFallMap.Children.Add(pushpin);
                }
                // (32.082253, 34.7795), TLV => Default center
                selectedFallMap.Center = new Location(32.082253, 34.7795);
            }
            else
            {
                Fall leFall = new Fall();
                foreach (Fall f in MainWindow.GetCurrentVM().Falls)
                {
                    if (f.ToString() == fallSelector.SelectedItem.ToString())
                    {
                        leFall = f;
                    }
                }
                Pushpin pushpin = new Pushpin();
                pushpin.Name = "pushpin" + leFall.id.ToString();
                pushpin.Location = new Location(leFall.x, leFall.y);
                pushpin.Background = System.Windows.Media.Brushes.Orange;
                selectedFallMap.Children.Clear();
                selectedFallMap.Children.Add(pushpin);
                if (latText.Text != "?")
                {
                    pushpin = new Pushpin();
                    pushpin.Name = "pushpin1";
                    double la = 0.0, lo = 0.0;
                    double.TryParse(latText.Text, out la);
                    double.TryParse(lonText.Text, out lo);
                    pushpin.Location = new Location(la, lo);
                    pushpin.Background = System.Windows.Media.Brushes.Gainsboro;
                    selectedFallMap.Children.Add(pushpin);
                }
                selectedFallMap.Center = new Location(leFall.x, leFall.y);
            }
            picSelectButton.IsEnabled = true;
        }
        #endregion

        #region Buttons Click Handlers
        private void PicSelectButton_Click(object sender, RoutedEventArgs e)
        {
            fallSelector.IsEnabled = false;
            string oldLat = latText.Text;
            string oldLon = lonText.Text;
            float ola = 0.0f, olo = 0.0f;
            float.TryParse(oldLat, out ola);
            float.TryParse(oldLon, out olo);
            OpenFileDialog of = new OpenFileDialog();
            //For any other formats
            of.Filter = "Image Files (*.jpg;*.jpeg)|*.BMP;*.JPG;*.JPEG;*.PNG";
            if (of.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileInfo imageFile = new FileInfo(of.FileName);
                    ExifReader reader = new ExifReader(imageFile.FullName);

                    // EXIF lat/long tags stored as [Degree, Minute, Second]
                    double[] latitudeComponents;
                    double[] longitudeComponents;

                    string latitudeRef; // "N" or "S" ("S" will be negative latitude)
                    string longitudeRef; // "E" or "W" ("W" will be a negative longitude)

                
                    if (reader.GetTagValue(ExifTags.GPSLatitude, out latitudeComponents)
                        && reader.GetTagValue(ExifTags.GPSLongitude, out longitudeComponents)
                        && reader.GetTagValue(ExifTags.GPSLatitudeRef, out latitudeRef)
                        && reader.GetTagValue(ExifTags.GPSLongitudeRef, out longitudeRef))
                    {

                        var latitude = ConvertDegreeAngleToDouble(latitudeComponents[0], latitudeComponents[1], latitudeComponents[2], latitudeRef);
                        var longitude = ConvertDegreeAngleToDouble(longitudeComponents[0], longitudeComponents[1], longitudeComponents[2], longitudeRef);
                        latText.Text = string.Format(latitude.ToString(),"F3");
                        lonText.Text = string.Format(longitude.ToString(),"F3");
                        Fall leFall = new Fall();
                        foreach (Fall f in MainWindow.GetCurrentVM().Falls)
                        {
                            if (f.ToString() == fallSelector.SelectedItem.ToString())
                            {
                                leFall = f;
                            }
                        }
                        if (fallSelector.SelectedItem.ToString() != "---")
                        {
                            Pushpin pushpin = new Pushpin();
                            pushpin.Name = "pushpin" + leFall.id.ToString();
                            pushpin.Location = new Location(leFall.x, leFall.y);
                            pushpin.Background = System.Windows.Media.Brushes.Orange;
                            selectedFallMap.Children.Clear();
                            selectedFallMap.Children.Add(pushpin);
                        }
                        
                        if (latText.Text != "?")
                        {
                            if (oldLat != "?")
                            {
                                Pushpin pushpin1 = new Pushpin();
                                pushpin1.Name = "pushpin1";
                                pushpin1.Location = new Location(ola, olo);
                                pushpin1.Background = System.Windows.Media.Brushes.Gainsboro;
                                try
                                {
                                    selectedFallMap.Children.Remove(pushpin1);
                                }
                                catch (Exception) { }
                            }
                            Pushpin pushpin = new Pushpin();
                            pushpin.Name = "pushpin1";
                            pushpin.Location = new Location(latitude, longitude);
                            pushpin.Background = System.Windows.Media.Brushes.Gainsboro;
                            selectedFallMap.Children.Add(pushpin);
                        }
                    }
                    geoPic.Source = new BitmapImage(new Uri(imageFile.FullName, UriKind.Absolute));
                }
                catch (Exception)
                {
                    showUnableToExtract();
                    latText.Text = "?";
                    lonText.Text = "?";
                    geoPic.Source = new BitmapImage(new Uri(@"Resources/demoPic.png", UriKind.Relative));
                    if (oldLat != "?")
                    {
                        Pushpin pushpin1 = new Pushpin();
                        pushpin1.Name = "pushpin1";
                        pushpin1.Location = new Location(ola, olo);
                        pushpin1.Background = System.Windows.Media.Brushes.Gainsboro;
                        try
                        {
                            selectedFallMap.Children.Remove(pushpin1);
                        }
                        catch (Exception) { }
                    }
                }
            }
            fallSelector.IsEnabled = true;
        }
        private void ActualUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (fallSelector.SelectedItem.ToString() == "---")
            {
                showSelectFall();
                return;
            }
            if (latText.Text == "?")
            {
                showSelectGeotaggedPic();
                return;
            }

            fallSelector.IsEnabled = false;
            picSelectButton.IsEnabled = false;

            try
            {
                // Finding the fall we want to update
                Fall leFall = new Fall();
                foreach (Fall f in MainWindow.GetCurrentVM().Falls)
                {
                    if (f.ToString() == fallSelector.SelectedItem.ToString())
                    {
                        leFall = f;
                    }
                }
                // Setting up the variables for the new coordinates
                float NewX = 0.0f, NewY = 0.0f;
                float.TryParse(latText.Text, out NewX);
                float.TryParse(lonText.Text, out NewY);

                string confirm = "Lat1: " + leFall.x.ToString() +
                    " -> Lat2: " + latText.Text + "\nLon1: " +
                    leFall.y.ToString() + " -> Lon2: " + lonText.Text;

                string[] messageTitle = showConfirm(confirm, leFall.ToString());

                MessageBoxResult result1 = System.Windows.MessageBox.Show(messageTitle[0], messageTitle[1], MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result1 != MessageBoxResult.OK)
                {
                    fallSelector.IsEnabled = true;
                    picSelectButton.IsEnabled = true;
                    return;
                }

                // Calling the model to deal with the actual data
                MainWindow.GetCurrentVM().CurrentModel.UpdateFall(leFall.id, NewX, NewY);

                // Re-setting the source of the selector...
                List<string> currentFalls = new List<string>();
                currentFalls.Add("---");
                foreach (Fall fall in MainWindow.GetCurrentVM().Falls)
                {
                    if (fall.isGeotagged == null || !fall.isGeotagged)
                    {
                        currentFalls.Add(fall.ToString());
                    }
                }
                fallSelector.ItemsSource = currentFalls;
                fallSelector.SelectedItem = fallSelector.Items.GetItemAt(0);
                selectedFallMap.Children.Clear();
                // ... and re-setting the demo photo and the lat and lon texts
                latText.Text = "?";
                lonText.Text = "?";
                geoPic.Source = new BitmapImage(new Uri(@"Resources/demoPic.png", UriKind.Relative));

                showSuccess();
            }
            catch (Exception)
            {
                showSomethingWrong();
            }
            
            fallSelector.IsEnabled = true;
            picSelectButton.IsEnabled = true;
        }
        #endregion

        #region Helper functions for the extraction of the coordinated from the geotag
        public static double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds, string latLongRef)
        {
            double result = ConvertDegreeAngleToDouble(degrees, minutes, seconds);
            if (latLongRef == "S" || latLongRef == "W")
            {
                result *= -1;
            }
            return result;
        }
        public static double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds)
        {
            return degrees + (minutes / 60) + (seconds / 3600);
        }
        #endregion

        #region Messangers
        private void showUnableToExtract()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Unable to extract geo-coordinates from selected photo.\nMake sure the photo is geotagged...";
                    title = "Unable to Extract";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Não foi possível extrair as geo-coordenadas da foto selecionada.\nVerifique que a foto tem geotag...";
                    title = "Extração Sem Sucesso";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "כשלון בחילוץ הגאו-קורדנטות מהתמונה הנבחרת\nבדק/י שיש לתמונה ג'אוטג";
                    title = "כשלון בחילוץ";
                    break;
                default: break;
            }
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showSelectFall()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please select a fall to update.";
                    title = "No Fall Selected";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Favor selecionar uma queda para atualizar.";
                    title = "Nenhuma Queda Selecionada";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ".נא לבחור נפילה לעדכון";
                    title = "אף נפילה נבחרה";
                    break;
                default: break;
            }
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showSelectGeotaggedPic()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please select a valid geotagged photo.";
                    title = "No Geotagged Photo";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Favor selecionar uma foto com geotag válida.";
                    title = "Sem Foto com Geotag";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ".נא לבחור תמונה עם ג'אוטג תקינה";
                    title = "תמונה עם ג'אוטג לא נבחרה";
                    break;
                default: break;
            }
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        public static void showSomethingWrong()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Houston, we have a problem... Please try again later.";
                    title = "Oops :(";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Algo de errado não está certo aqui... Favor tenat novamente mais tarde.";
                    title = "Opa :(";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "משהו לא נכון פה... נא לנסות שוב בזמן אחר";
                    title = "אופס :(";
                    break;
                default: break;
            }
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showSuccess()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "The fall's location has been updated successfully.";
                    title = "Successful Update";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "A localização da queda foi atualizada com sucesso.";
                    title = "Atualização com Sucesso";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ".איתור הנפילה מעודכן בהצלחה";
                    title = "עדכון מוצלח";
                    break;
                default: break;
            }
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private string[] showConfirm(string confirm, string leFall)
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Are you sure you want to do the following update:\n"
                        + confirm + "\nOn fall:\n" + leFall + "?";
                    title = "Confirm";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Tem certeza que deseja realizar a seguinte atualização:\n"
                        + confirm + "\nNa queda:\n" + leFall + "?";
                    title = "Confirmar";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ":ברצונך להפעיל העדכון הבא\n" + confirm +
                        "\n:על הנפילה\n" + leFall + "?";
                    title = "אישור";
                    break;
                default: break;
            }
            return new string[] { message, title };
        }
        #endregion
    }
}
