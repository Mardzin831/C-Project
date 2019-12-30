using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ClassLibrary
{
    public class Lib
    {

        private static TimeSpan GetVideoDuration(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                var t = (ulong)prop.ValueAsObject;
                return TimeSpan.FromTicks((long)t);
            }
        }
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
            
            if (traceSwitch.TraceInfo)
            {
                fileSystemWatcher.Changed += (Object sender, FileSystemEventArgs e) =>
                {
                    eventLog.WriteEntry(e.Name + " :changed\n");
                };
            }
            if (traceSwitch.TraceWarning)
            {
                fileSystemWatcher.Renamed += (object sender, RenamedEventArgs e) =>
                {
                    eventLog.WriteEntry(e.Name + " :renamed\n");
                };
            }
            if (traceSwitch.TraceError)
            {
                fileSystemWatcher.Created += (Object sender, FileSystemEventArgs e) =>
                {
                    eventLog.WriteEntry(e.Name + " :created\n");

                    conn.Open();
                    var info = new FileInfo(e.FullPath);
                    command = new SqlCommand("insert into Tab(Nazwa, Rozmiar, Typ, DataUtworzenia) " +
                    "values(@name, @size, @type, @date)", conn);
                    command.Parameters.AddWithValue("@name", Path.GetFileNameWithoutExtension(e.FullPath));
                    command.Parameters.AddWithValue("@size", info.Length);
                    command.Parameters.AddWithValue("@type", e.FullPath.Substring(e.FullPath.LastIndexOf(".")));
                    command.Parameters.AddWithValue("@date", info.CreationTime);

                    command.ExecuteNonQuery();
           
                    

                };
                fileSystemWatcher.Deleted += (Object sender, FileSystemEventArgs e) =>
                {
                    eventLog.WriteEntry(e.Name + " :deleted\n");
                };
            }
            fileSystemWatcher.EnableRaisingEvents = true;
            
        }
        public void StopService()
        {
            fileSystemWatcher.Dispose();
            conn.Close();
            Thread.Sleep(1);

        }

        
    }
}
