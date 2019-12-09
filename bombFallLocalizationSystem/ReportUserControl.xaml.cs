using bombFallLocalizationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http.Headers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using DAL;
using System.Net;

namespace PL
{
    /// <summary>
    /// Interaction logic for ReportUserControl.xaml
    /// </summary>
    public partial class ReportUserControl : UserControl
    {
        public ReportUserControl(MainWindow main)
        {
            InitializeComponent();
        }

        #region Validator for Numerical Text
        private void PrevFallTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion

        private void ActualReportButton_Click(object sender, RoutedEventArgs e)
        {
            prevFallTextBox.IsEnabled = false;
            addressBox.IsEnabled = false;
            prevFallCheck.IsEnabled = false;

            #region Local Variables Set Up
            float lat = 0, lon = 0;
            int prevMinutes = 0;
            List<Fall> prevFalls = new List<Fall>();
            List<Fall> fs = new List<Fall>();
            XElement prevFallsRoot = XElement.Load("recentFalls.xml");
            int nextId = 0;
            double[][] toBeClustered;
            #endregion

            #region Validating What the User Entered
            if (addressBox.Text == null || addressBox.Text == "")
            {
                showNoAddressEntered();
                enableThings();
                return;
            }
            else if (addressBox.Text != null && addressBox.Text != "")
            {
                // With HERE Geocoder API
                string appID = "AWOngGwwid3peeor8FB8";
                string appCode = "Aiq7YVehgUTJxBl9eukLQg";
                string formattedAddr = addressBox.Text.Replace(", ", "%20").Replace(" ", "%20");
                string url = string.Format("https://geocoder.api.here.com/6.2/geocode.xml?app_id={0}&app_code={1}&searchtext={2}",appID,appCode,formattedAddr);

                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                XDocument xdoc = XDocument.Load(response.GetResponseStream());
                try
                {
                    XElement result = xdoc.Root.Element("Response").Element("View").Element("Result");
                    float.TryParse(result.Element("Location").Element("NavigationPosition").Element("Latitude").Value.ToString(), out lat);
                    float.TryParse(result.Element("Location").Element("NavigationPosition").Element("Longitude").Value.ToString(), out lon);
                    lat = (float)System.Math.Round(lat, 3);
                    lon = (float)System.Math.Round(lon, 3);
                }
                catch(Exception)
                {
                    showInvalidAddress();
                    enableThings();
                    return;
                }
            }
            string confirm = "Lat: " + lat.ToString()
                + ", Lon: " + lon.ToString() + ",\nDate: "
                + DateTime.Now.AddMinutes(-prevMinutes).ToString();

            string[] messageTitle = showConfirm(confirm);

            MessageBoxResult result1 = MessageBox.Show(messageTitle[0], messageTitle[1], MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result1 != MessageBoxResult.OK)
            {
                enableThings();
                return;
            }
            if ((prevFallTextBox.Text == null || prevFallTextBox.Text == "") && (bool)prevFallCheck.IsChecked)
            {
                showNoPrevEntered();
                enableThings();
                return;
            }
            #endregion

            #region Case: Fall From X Minutes Ago
            else if ((bool)prevFallCheck.IsChecked && (prevFallTextBox.Text != null || prevFallTextBox.Text != ""))
            {
                int.TryParse(prevFallTextBox.Text, out prevMinutes);

                // We want to give the user a ten minutes error margin, i.e. assume that when they say
                // the fall happened x minutes ago, we include all falls between x+10 minutes ago and x-10
                // minutes ago
                List<string> prevPosDates = new List<string>();
                prevPosDates.Add(DateTime.Now.AddMinutes(prevMinutes).ToString());
                int previ = 1; int prevj = 1;
                while ( previ < 11 )
                {
                    prevPosDates.Add(DateTime.Now.AddMinutes(previ - prevMinutes).ToString());
                    while (prevj < 60)
                    {
                        prevPosDates.Add(DateTime.Now.AddMinutes(previ - prevMinutes).AddSeconds(prevj).ToString());
                        prevPosDates.Add(DateTime.Now.AddMinutes(previ - prevMinutes).AddSeconds(-prevj).ToString());
                        prevPosDates.Add(DateTime.Now.AddMinutes(-prevMinutes).AddSeconds(prevj).ToString());
                        prevPosDates.Add(DateTime.Now.AddMinutes(-prevMinutes).AddSeconds(-prevj).ToString());
                        prevPosDates.Add(DateTime.Now.AddMinutes(-previ - prevMinutes).AddSeconds(prevj).ToString());
                        prevPosDates.Add(DateTime.Now.AddMinutes(-previ - prevMinutes).AddSeconds(-prevj).ToString());
                        prevj++;
                    }
                    prevPosDates.Add(DateTime.Now.AddMinutes(-previ - prevMinutes).ToString());
                    prevj = 1;
                    previ++;
                }
                MessageBox.Show("prevPosDates format:\"\n\t" + prevPosDates.FirstOrDefault()
                    + "\"\nprevFallsRoot.Elements(\"Fall\")...Element(\"Date\").Value format:\"\n" +
                    prevFallsRoot.Elements("Fall").FirstOrDefault().Element("Date").Value +"\"");
                IEnumerable<XElement> falls =
                    from f2 in prevFallsRoot.Elements("Fall")
                    where prevPosDates.Contains(f2.Element("Date").Value)
                    select f2;
                toBeClustered = new double[falls.Count()+1][];
                int k = 0;
                foreach (var fall in falls)
                {
                    double y = 0.0, x = 0.0;
                    double.TryParse(fall.Element("X").Value.ToString(), out x);
                    double.TryParse(fall.Element("Y").Value.ToString(), out y);
                    toBeClustered[k] = new double[] { y, x };
                    k++;
                }

                if (falls.Count() < 1)
                {
                    // In case this is a report refers to a "new" fall, i.e. one that has not been
                    // reported yet => one that needs a new Id for the recent falls file
                    goto NEW_FALL;
                }
                else
                {
                    if (falls.Any
                        (elem =>
                        {
                            bool t;
                            bool.TryParse(elem.Element("IsGeotagged").Value.ToString(), out t);
                            return t;
                        }))
                    {
                        showAlreadyLocated();
                        enableThings();
                        return;
                    }
                    var f1 = falls.FirstOrDefault();
                    // We'll assign them the same Id so that - in the recent falls xml file - the Id
                    // serves as an indication of which reports should get clustered together. Only
                    // when actually entering the report into the DB will we set a correct ID.
                    var newFall1 = new XElement(
                        "Fall", new XElement("Date", DateTime.Now.AddMinutes(-prevMinutes).ToString()),
                        new XElement("Id", f1.Element("Id").Value), new XElement("X", lon),
                        new XElement("Y", lat));
                    prevFallsRoot.Add(newFall1);
                    int.TryParse(f1.Element("Id").Value.ToString(), out nextId);
                    // Recording the falls that will have to be altered after we re-calculate
                    prevFalls = MainWindow.GetCurrentVM().Falls.Where(fall => prevPosDates.Contains(fall.date.ToString())).ToList();
                }
                goto RECALC_OLD_FALLS;
            }
            // If we're dealing with a new report, that may refer to a fall from 10 minutes ago
            // that may or may not already have been reported...
            // We'll check if there have been other reports from the last 10 minutes, and if
            // yes, assign the current report the same Id.
            List<string> posDates = new List<string>();
            var currentRep = new Fall();
            posDates.Add(DateTime.Now.ToString());
            int i = 1; int j = 1;
            while (i < 11)
            {
                posDates.Add(DateTime.Now.AddMinutes(i).ToString());
                while (j < 60)
                {
                    posDates.Add(DateTime.Now.AddMinutes(i).AddSeconds(j).ToString());
                    posDates.Add(DateTime.Now.AddMinutes(i).AddSeconds(-j).ToString());
                    posDates.Add(DateTime.Now.AddMinutes(-i).AddSeconds(j).ToString());
                    posDates.Add(DateTime.Now.AddMinutes(-i).AddSeconds(-j).ToString());
                    j++;
                }
                posDates.Add(DateTime.Now.AddMinutes(-i).ToString());
                j = 1;
                i++;
            }
            #endregion

            #region Case New Fall Report
            List<XElement> xfs = prevFallsRoot.Elements("Fall").Where(f3 => posDates.Contains(f3.Element("Date").Value.ToString())).ToList();
            Fall fall1 = MainWindow.GetCurrentVM().Falls.Where(f3 => { return posDates.Contains(f3.date.ToString()); }).OrderByDescending(f7 => f7.id).FirstOrDefault();
            foreach (var xf in xfs)
            {
                Fall toAdd = new Fall();DateTime toAddDate = new DateTime(0);
                float toAddX = 0.0f; float toAddY = 0.0f;
                int toAddId = 
                    MainWindow.GetCurrentVM().Falls.Where(f6 => f6.id.ToString() == xf.Element("Id").Value.ToString()).FirstOrDefault().id;
                DateTime.TryParse(xf.Element("Date").Value, out toAddDate);
                float.TryParse(xf.Element("X").Value, out toAddX);
                float.TryParse(xf.Element("Y").Value, out toAddY);
                toAdd.id = toAddId; toAdd.date = toAddDate; toAdd.x = toAddX; toAdd.y = toAddY;
                fs.Add(toAdd);
            }
            // If there where no other falls recorded in the past ten minutes
            if (fs.Count() < 1) { goto NEW_FALL; }
            // Otherwise, we continue:
            var f = fs.FirstOrDefault();
            // Adding the current report to the recent falls document
            var newFall = new XElement(
                        "Fall", new XElement("Date", DateTime.Now.AddMinutes(-prevMinutes).ToString()),
                        new XElement("Id", f.id), new XElement("X", lon),
                        new XElement("Y", lat));
            prevFallsRoot.Add(newFall);
            prevFallsRoot.Save("recentFalls.xml");
            int.TryParse(f.id.ToString(), out nextId);
            // Putting the fall report in the list to be re-calculated
            currentRep.id = nextId;
            currentRep.isGeotagged = f.isGeotagged;
            currentRep.x = lon;
            currentRep.y = lat;
            fs.Add(currentRep);
            toBeClustered = new double[fs.Count()+1][];
            for (int l = 0; l < fs.Count(); l++)
            {
                toBeClustered[l] = new double[]
                    { fs.ElementAtOrDefault(l).y, fs.ElementAtOrDefault(l).x };
            }
            #endregion

            #region Either It's Grouped With Other Recents Falls
            RECALC_OLD_FALLS:;
            int numOfClusters = 1;
            if (toBeClustered.Length - 1 < 8 && toBeClustered.Length - 1 > 3)
            { numOfClusters = 2; }
            else if (toBeClustered.Length - 1 > 9)
            { numOfClusters = 3; }
            // Clustering
            List<KeyValuePair<int, float[]>> newClusters = new List<KeyValuePair<int, float[]>>();
            if(fs.Count() == 2)
            {
                double dNewX = (toBeClustered[0][1] + toBeClustered[1][1]) / 2.0;
                double dNewY = (toBeClustered[0][0] + toBeClustered[1][0]) / 2.0;
                float newX = 0.0f; float newY = 0.0f;
                float.TryParse(dNewX.ToString(), out newX);
                float.TryParse(dNewY.ToString(), out newY);
                newClusters.Add(new KeyValuePair<int, float[]>(0, new float[]{newX, newY}));
            }
            else
            {
               newClusters = KMeansClustering.ClustersAndMeans(toBeClustered, numOfClusters);
            }
            // Removing the old falls from the DB...
            foreach (Fall fall in fs)
            {
                try
                {
                    //MainWindow.GetCurrentVM().CurrentModel.db = new Model1();
                    if (MainWindow.GetCurrentVM().CurrentModel.db.falls.Where(fa => fa.id == fall.id ).Count() > 0)
                        MainWindow.GetCurrentVM().CurrentModel.Remove(fall.id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n\tBest regards,\n\t" + ex.Source,"Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            // ... and adding the new ones
            foreach (KeyValuePair<int, float[]> kv in newClusters)
            {
                try
                {
                    MainWindow.GetCurrentVM().addFallCommand.Execute(kv.Value[0], kv.Value[1], DateTime.Now.AddMinutes(-prevMinutes));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n\tBest regards,\n\t" + ex.StackTrace.Split().ElementAt(0), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    enableThings();
                    return;
                }
            }
            showClusteredSuccess();
            enableThings();
            return;
            #endregion

            #region ... Or it's a Completely New One
            NEW_FALL:;  // A previously unreported fall
            nextId = 0;
            foreach (var f4 in prevFallsRoot.Elements("Fall"))
            {
                int temp = 0;
                int.TryParse(f4.Element("Id").Value, out temp);
                if (temp > nextId) { nextId = temp; }
            }
            nextId++;
            // Adding the current report to the recent falls document
            var newFall2 = new XElement(
                        "Fall", new XElement("Date", DateTime.Now.AddMinutes(-prevMinutes).ToString()),
                        new XElement("Id", nextId), new XElement("X", lon),
                        new XElement("Y", lat));
            prevFallsRoot.Add(newFall2);
            prevFallsRoot.Save("recentFalls.xml");
            // Adding the currently reported fall to the DB
            try
            {
                MainWindow.GetCurrentVM().addFallCommand.Execute(lat, lon, DateTime.Now.AddMinutes(-prevMinutes));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\tBest regards,\n\t" + ex.TargetSite, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                enableThings();
                return;
            }
            showNewSuccess();
            enableThings();
            #endregion
        }

        #region Helpers and Handlers to Enable Controls
        private void enableThings()
        {
            addressBox.IsEnabled = true;
            if ((bool)prevFallCheck.IsChecked)
            {
                prevFallTextBox.IsEnabled = true;
            }
            prevFallCheck.IsEnabled = true;
        }
        private void PrevFallCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (prevFallCheck.IsChecked.Value)
            {
                prevFallTextBox.IsEnabled = true;
            }
            else
            {
                prevFallTextBox.IsEnabled = false;
                prevFallTextBox.Text = "";
            }
        }
        #endregion

        #region Messangers
        private void showNoAddressEntered()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please make sure you\'ve specified the address of the report.";
                    title = "Something\'s Missing...";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Por favor verifique-se que você especificou o endereço do relatório.";
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
        private string[] showConfirm(string confirm)
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "This is the fall report you entered:\n"
                        + confirm + "\nDo you confirm its accuracy and validity?";
                    title = "Confirm";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Este é o relato de queda que você entrou\n"
                        + confirm + "\nVocê confirma sua precisão e vericidade?";
                    title = "Confirmar";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = ":זה דיווח נפילה שרשמתה\n" + confirm + "\n?הנך מאשר אותו";
                    title = "אישור";
                    break;
                default: break;
            }
            return new string[] { message, title };
        }
        private void showNoPrevEntered()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please make sure you\'ve specified the amount of minutes.";
                    title = "Something\'s Missing...";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Por favor verifique-se que você especificou a quantidade de minutos.";
                    title = "Está faltando algo...";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "נא לוודא שרשמת מספר דקות.";
                    title = "חסר משהו...";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        public static void showInvalidAddress()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "Please make sure you\'ve entered a valid address.";
                    title = "Invalid Address";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Por favor verifique-se que você entrou um endereço valido.";
                    title = "Endereço Invalido";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "נא לוודא שרשמת כתובת תקינה.";
                    title = "כתובת לא תקינה";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showAlreadyLocated()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "It looks like the fall you wish to report has already been (exactly) located.";
                    title = "Already Located";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Parece que a queda que você deseja relatar já foi (exatamente) localizada.";
                    title = "Já Localizada";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "כניראה הנפילה שברצונך לדווח עליה כבר היתה מתוארת.";
                    title = "כבר מתואר";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        private void showClusteredSuccess()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "You reported a fall that had already been reported. Your report has been successfuly clustered with previous reports and the localization of the fall has been updated.";
                    title = "Location Updated Successfuly";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Você relatou uma queda que já havia sido relatada. Seu relátorio foi aglomerado a outros relátorios com sucesso e a localização da queda foi recalculada.";
                    title = "Localização Atualizada com Sucesso";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "דיווחת על נפילה שכבר דיווחו עליה. דוחך היה הושם באשכול עם דיווחים אחרים בהמלחה ואתר הנפילה היה  מחושב מחדש.";
                    title = "אתר מעודכן בהצלחה";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            addressBox.Clear();
        }
        private void showNewSuccess()
        {
            string message = "";
            string title = "";
            switch (MainWindow.currentLang)
            {
                case "Resources/ukFlagIcon.png":
                    message = "You reported a new fall and it was sucessfully added to the system.";
                    title = "Location Added Succesfully";
                    break;
                case "Resources/brazilFlagIcon.png":
                    message = "Você relatou uma nova queda e ela foi adicionada ao sistema com sucesso.";
                    title = "Localização Adicionada com Sucesso";
                    break;
                case "Resources/israelFlagIcon.png":
                    message = "דיווחת על נפילה חדשה והיא נוספה למערכת הצלחה.";
                    title = "אתר נוספה בהצלחה";
                    break;
                default: break;
            }
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            addressBox.Clear();
        }
        #endregion
    }
}
