using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace ClassLibrary
{
    public class Lib
    {
        private TraceSwitch traceSwitch;
        private EventLog eventLog;
        private string workingDirectory;
        private FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
        private SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["localDB"].ConnectionString);

        public void StartService()
        {
            workingDirectory = ConfigurationManager.AppSettings.Get("Sciezka");
            string sourceName = ConfigurationManager.AppSettings.Get("Zrodlo");
            string eventLogName = ConfigurationManager.AppSettings.Get("Dziennik");
            if (!EventLog.SourceExists(sourceName, "."))
            {
                EventLog.CreateEventSource(sourceName, eventLogName);
                throw new ArgumentException("Dziennik nie istnieje!");
            }
            eventLog = new EventLog(eventLogName, ".", sourceName);
            traceSwitch = new TraceSwitch("Logowanie", "Level of loging done on directory");
            fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = workingDirectory;
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess| NotifyFilters.LastWrite| NotifyFilters.FileName;
            fileSystemWatcher.Filter = "*.*";
            fileSystemWatcher.IncludeSubdirectories = true;
            conn.Open();

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
                 
                    using (SqlCommand command = new SqlCommand("insert into Tab(Nazwa, Rozmiar, Typ, DataUtworzenia, CzasVideo) " +
                    "values(@name, @size, @type, @date, @timespan)", conn))
                    {
                        var info = new FileInfo(e.FullPath);
                        command.Parameters.AddWithValue("@name", Path.GetFileNameWithoutExtension(e.FullPath));
                        command.Parameters.AddWithValue("@size", info.Length);
                        command.Parameters.AddWithValue("@type", e.FullPath.Substring(e.FullPath.LastIndexOf(".")));
                        command.Parameters.AddWithValue("@date", info.CreationTime);
                        command.Parameters.AddWithValue("@timespan", GetVideoDuration(e.FullPath));
                        command.ExecuteNonQuery();
                        command.Dispose();
                    }
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
            if(conn.State == ConnectionState.Closed)
            {
                throw new Exception("Już zamknięte!");
            }
            conn.Close();
            fileSystemWatcher.Dispose();
            eventLog.Dispose();
        }
        //static?
        public TimeSpan GetVideoDuration(string filePath)
        {
            using (var shell = ShellObject.FromParsingName(filePath))
            {
                IShellProperty prop = shell.Properties.System.Media.Duration;
                if (prop.ValueAsObject == null)
                {
                    return new TimeSpan(0);
                }
                var t = (ulong)prop.ValueAsObject;
                TimeSpan ts = TimeSpan.FromTicks((long)t);
      
                return new TimeSpan(ts.Hours, ts.Minutes, ts.Seconds);
                
            }
        }
    }
}
