using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MSOAgent
{
    public partial class MSOAgentService : ServiceBase
    {
        private AutoMailer mailer;

        public MSOAgentService()
        {
            InitializeComponent();
            mailer= new AutoMailer();
        }

        

        protected override void OnStart(string[] args)
        {

        }

        protected override void OnStop()
        {
            mailer.Stop();
        }
    }
}
