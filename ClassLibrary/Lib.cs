using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary
{
    public class Lib
    {
        private TraceSwitch traceSwitch;
        private EventLog eventLog;
        private string workingDirectory;
        private FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;
        AttachDbFilename=D:\infa_studia\5semestr\C#\MyService\WpfAplication\Database.mdf;
        Integrated Security=True";
        private SqlConnection conn = new SqlConnection(connectionString);
        private SqlCommand command;

        public void StartService()
        {
            workingDirectory = ConfigurationManager.AppSettings.Get("Sciezka");
            String sourceName = ConfigurationManager.AppSettings.Get("Zrodlo");
            String eventLogName = ConfigurationManager.AppSettings.Get("Dziennik");
            if (!EventLog.SourceExists(sourceName, "."))
            {
                EventLog.CreateEventSource(sourceName, eventLogName);
            }
            eventLog = new EventLog(eventLogName, ".", sourceName);
            traceSwitch = new TraceSwitch("Logowanie", "Level of loging done on directory");
            fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = workingDirectory;

            fileSystemWatcher.Filter = "*.*";
            fileSystemWatcher.IncludeSubdirectories = true;

            if (this.traceSwitch.TraceInfo)
            {
                fileSystemWatcher.Changed += (Object sender, FileSystemEventArgs e) =>
                {
                    this.eventLog.WriteEntry(e.Name + " :changed\n");
                };
            }
            if (this.traceSwitch.TraceWarning)
            {
                fileSystemWatcher.Renamed += (object sender, RenamedEventArgs e) =>
                {
                    this.eventLog.WriteEntry(e.Name + " :renamed\n");
                };
            }
            if (this.traceSwitch.TraceError)
            {
                fileSystemWatcher.Created += (Object sender, FileSystemEventArgs e) =>
                {
                    this.eventLog.WriteEntry(e.Name + " :created\n");

                    conn.Open();
                    command = new SqlCommand("insert into Table1(xd) values(@xd)", conn);
                    command.Parameters.AddWithValue("@xd", "lol");

                    command.ExecuteNonQuery();
                    
                    command.Dispose();
                    conn.Close();

                };
                fileSystemWatcher.Deleted += (Object sender, FileSystemEventArgs e) =>
                {
                    this.eventLog.WriteEntry(e.Name + " :deleted\n");
                };
            }
            fileSystemWatcher.EnableRaisingEvents = true;
            
        }
        public void StopService()
        {
            fileSystemWatcher.Dispose();
            
        }

    
    }
}
