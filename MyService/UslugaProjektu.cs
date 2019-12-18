using ClassLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MyService
{
    class UslugaProjektu : ServiceBase
    {
        
        private Lib lib = new Lib();
        public UslugaProjektu()
        {
            this.ServiceName = ConfigurationManager.AppSettings.Get("NazwaUslugi");
            
        }
        protected override void OnStart(string[] args)
        {
            lib.StartService();
        }
        protected override void OnStop()
        {
            lib.StopService();
        }
    }
}
