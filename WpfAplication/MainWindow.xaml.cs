using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;


namespace WpfAplication
{
    public partial class MainWindow : Window
    {
        public ServiceController serviceController;
        private SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["localDB"].ConnectionString);
        private SqlDataAdapter adapter = new SqlDataAdapter();
        private DataTable tab;
        public MainWindow()
        {
            //Height = (SystemParameters.PrimaryScreenHeight * 0.9);
            //Width = (SystemParameters.PrimaryScreenWidth * 0.9);
            InitializeComponent();

            if (ServiceController.GetServices().Any(serviceController =>
                 serviceController.ServiceName.Equals("UslugaProjektu")) == true)
            {
                serviceController = new ServiceController("UslugaProjektu");

                var config = ConfigurationManager.OpenExeConfiguration(@".\MyService.exe");
                FolderBox.Text = config.AppSettings.Settings["Sciezka"].Value;

                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    StartButton.IsEnabled = false;
                    label.Content = "Usługa: Uruchomiona";
                }
                else if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    StopButton.IsEnabled = false;
                    label.Content = "Usługa: Zatrzymana";
                }
            }
            else
            {
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                label.Content = "Usługa: Nie istnieje";
            }
            ShowTab();
        }

        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            conn.Close();
            SqlConnection.ClearAllPools();
            
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running);

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            AddButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            RefreshButton.IsEnabled = false;
            FolderButton.IsEnabled = false;
            label.Content = "Usługa: Uruchomiona";
        }

        public void StopButton_Click(object sender, RoutedEventArgs e)
        {      
            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            AddButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
            RefreshButton.IsEnabled = true;
            FolderButton.IsEnabled = true;
            label.Content = "Usługa: Zatrzymana";
        }

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if(NameBox.Text != "" && int.TryParse(SizeBox.Text, out _) == true &&
                (DateTime.TryParseExact(DateBox.Text, "dd/MM/yyyy HH:mm:ss", 
                new CultureInfo("en-GB"), DateTimeStyles.None, out _) == true || DateBox.Text == ""))
            {
                SqlConnection.ClearAllPools();
                conn.Open();
                using (SqlCommand command = new SqlCommand("insert into Tab(Nazwa, Rozmiar, Typ, DataUtworzenia, CzasVideo) " +
                    "values(@name, @size, @type, @date, @timespan)", conn))
                {
                    command.Parameters.AddWithValue("@name", NameBox.Text);
                    command.Parameters.AddWithValue("@size", SizeBox.Text);
                    command.Parameters.AddWithValue("@type", TypeBox.Text);
                    if (DateBox.Text == "")
                    {
                        command.Parameters.AddWithValue("@date", DateTime.Now);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@date",
                            DateTime.ParseExact(DateBox.Text, "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-GB")));
                    }
                    if (TimeSpanBox.Text == "")
                    {
                        command.Parameters.AddWithValue("@timespan", new TimeSpan(0));
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@timespan", TimeSpan.ParseExact(TimeSpanBox.Text, @"h\:mm\:ss", null));
                    }
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                ShowTab();
            }
            else
            {
                MessageBox.Show("Błędnie wprowadzone dane!");
            }
        }
        public void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selected = dataGrid.SelectedItem as DataRowView;
            if (selected != null)
            {
                SqlConnection.ClearAllPools();
                conn.Open();
                string cell = selected.Row.ItemArray[0].ToString();
                int id = int.Parse(cell);

                using (SqlCommand command = new SqlCommand("delete from Tab where Id=@id", conn))
                {      
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                ShowTab();
            }
        }
        public void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTab();
        }
        public void ShowTab()
        {
            tab = new DataTable("Tab");
            using (SqlCommand command = new SqlCommand("select * from Tab", conn))
            {
                adapter = new SqlDataAdapter(command);
            }
                 
            adapter.Fill(tab);
            dataGrid.ItemsSource = tab.DefaultView;

            conn.Close();
            adapter.Dispose();
            tab.Dispose();
        }
        public void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy HH:mm:ss";
        }

        public void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                FolderBox.Text = dialog.SelectedPath;

                var config = ConfigurationManager.OpenExeConfiguration(@".\MyService.exe");
                config.AppSettings.Settings["Sciezka"].Value = dialog.SelectedPath;
                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}
