using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceProcess;
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

namespace WpfAplication
{
    public partial class MainWindow : Window
    {
        private ServiceController serviceController;
        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
            AttachDbFilename=C:\Users\Admin\Desktop\infa_studia\5semestr\C#\MyService\WpfAplication\Database.mdf;
            Integrated Security=True";
        private SqlConnection conn = new SqlConnection(connectionString);
        private SqlCommand command = new SqlCommand();
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable tab = new DataTable("Tab");
        public MainWindow()
        {
            InitializeComponent();
            if (ServiceController.GetServices().Any(serviceController =>
                 serviceController.ServiceName.Equals("UslugaProjektu")) == true)
            {
                serviceController = new ServiceController("UslugaProjektu");
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    StartButton.IsEnabled = false;
                    label.Content = "Usługa: Włączona";
                }
                else if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    StopButton.IsEnabled = false;
                    label.Content = "Usługa: Wyłączona";
                }
            }
            else
            {
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                label.Content = "Usługa: Nie istnieje";
            }
            
            conn.Open();
            ShowTab();

        }
            
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            serviceController.Start();
            serviceController.WaitForStatus(ServiceControllerStatus.Running);
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            label.Content = "Usługa: Włączona";
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            serviceController.Stop();
            serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            label.Content = "Usługa: Wyłączona";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if(NameBox.Text != "" && int.TryParse(SizeBox.Text, out _) != false)
            {
                command = new SqlCommand("insert into Tab(Nazwa, Rozmiar, Typ, DataUtworzenia) " +
                    "values(@name, @size, @type, @date)", conn);
                command.Parameters.AddWithValue("@name", NameBox.Text);
                command.Parameters.AddWithValue("@size", SizeBox.Text);
                command.Parameters.AddWithValue("@type", TypeBox.Text);
                if(DateBox.Text == "")
                {
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                }
                else
                {
                    command.Parameters.AddWithValue("@date", DateBox.Text);
                }
               
                command.ExecuteNonQuery();
                ShowTab();
            }
            else
            {
                InfoBox.Text = "Błędnie wprowadzone dane!";
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selected = dataGrid.SelectedItem as DataRowView;
            if (selected != null)
            {
                string cell = selected.Row.ItemArray[0].ToString();
                int id = int.Parse(cell);

                command = new SqlCommand("delete from Tab where Id=@id", conn);
                command.Parameters.AddWithValue("@id", id);

                command.ExecuteNonQuery();
                ShowTab();
            }

        }
        public void ShowTab()
        {
            InfoBox.Text = "";
            tab.Clear();
            command = new SqlCommand("select * from Tab", conn); ;
            adapter = new SqlDataAdapter(command);
            adapter.Fill(tab);

            dataGrid.ItemsSource = tab.DefaultView;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTab();
        }

      
    }
}
