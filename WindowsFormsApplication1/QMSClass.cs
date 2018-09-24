using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Exception = System.Exception;
using QMSEDX;
using QMS.QMSAPI;
using QMS.ServiceSupport;


namespace QMS
{
    public class QMSClass
    {
        private int _timeout = 60;
        private int _pollIntervall = 2;
        const UInt32 WM_KEYDOWN = 0x0100;
        const int VK_F5 = 0x74;
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);
        [DllImport("kernel32.dll")]
        static extern uint GetLastError();
        
        public QMSClass()
        {

        }
        
        private void sendF5()
        {
            Process[] processes = Process.GetProcessesByName("iexplore");
            for (int i=0;i<processes.Count();i++)
            {
                PostMessage(processes[i].MainWindowHandle, WM_KEYDOWN, VK_F5, 0);
            }
        }

        private bool isPublisher(IQMS client)
        {
            try
            {
                License license = client.GetLicense(LicenseType.Publisher, Guid.Empty);
                return license.LEFOk;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }


        private byte getServerType(IQMS client)
        {
            if (isPublisher(client))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public short TriggerAndMonitorTask(string sApp, string sServer, string sVarName, string[] sVarValue, Form1 Principal) {

            string applicationTask;
            Uri _qms = new Uri("http://" + sServer + ":4799/QMS/Service");
            IQMS client = CreateQMSAPIClient(_qms, Principal);
            bool bResult=false;
            TriggerEDXTaskResult triggerResult;

            byte serverType = getServerType(client);
            applicationTask = sApp;
            if (serverType == 1)
            {
                applicationTask = applicationTask.Replace(".qvw", "");
                if (applicationTask.LastIndexOf('\\') > 0)
                {
                    applicationTask = applicationTask.Substring(applicationTask.LastIndexOf('\\') + 1);
                }
                Console.WriteLine(applicationTask);
            }

            if (client != null) {
                try
                {
                    triggerResult = client.TriggerEDXTask(Guid.Empty, applicationTask, string.Empty, sVarName, sVarValue);
                    if(triggerResult.EDXTaskStartResult == EDXTaskStartResult.TaskIsAlreadyRunning)
                    {
                        return 5;
                    }
                }
                catch(Exception ex)
                {
                    Principal.setEstadoRecarga(System.Security.Principal.WindowsIdentity.GetCurrent().Name + " " + ex.Message);
                    return 1; //error de permisos
                }
                if (triggerResult.EDXTaskStartResult == EDXTaskStartResult.Success) {
                    bResult = PollSingleTask(client, triggerResult.ExecId, _pollIntervall, _timeout);
                }
                else {
                    return 2; //error de recarga no se pudo iniciar la recarga o tarea no existe
                }
            }
            else {
                return 3; //error de conexion
            }
            if (bResult==true)
            {
                sendF5();
                return 0;
            }
            return 4; //error de recarga(Codigo) o se la cancelo de consola
        }

        private bool PollSingleTask(IQMS client, Guid execId, int pollInterval, int timeout)
        {
            EDXStatus result = null;
            do
            {
                //depurar aca
                try
                {
                    result = client.GetEDXTaskStatus(Guid.Empty, execId);
                } catch
                {
                    return false;
                }
                Thread.Sleep(pollInterval * 1000);
                if (result.TaskStatus == TaskStatusValue.Completed || result.TaskStatus == TaskStatusValue.Failed) break;
            } while (1==1);

            if (result.TaskStatus == TaskStatusValue.Failed)
            {
                return false;
            }

            return true;
        }
        private IQMS CreateQMSAPIClient(Uri uri, Form1 Principal)
        {
            QMSClient client = null;

            try
            {
                client = new QMSClient("BasicHttpBinding_IQMS", uri.AbsoluteUri);
                string key = client.GetTimeLimitedServiceKey();
                ServiceKeyClientMessageInspector.ServiceKey = key;
            }
            catch(System.Exception Ex)
            {
                Principal.setEstadoRecarga(Ex.Message);
                return null;
            }
            return client;
        }
    } 
}
