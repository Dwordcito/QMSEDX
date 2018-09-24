using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace QMSEDX
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void recargaThread(string[] args, int iCantidadApps, Form1 Principal)
        {
            string[] sVarValue;
            short sReturn;
            QMS.QMSClass Recarga = new QMS.QMSClass();
            int i;

            for (i = 0; i < iCantidadApps; i++)
            {
                sVarValue = new string[] { args[3 + (i * 4)] };
                sReturn = Recarga.TriggerAndMonitorTask(args[0 + (i * 4)], args[1 + (i * 4)], args[2 + (i * 4)], sVarValue, Principal);
                if (sReturn != 0)
                {
                    Principal.setEstadoBoton(true);
                    if (sReturn == 1)
                    {
                        //Principal.setEstadoRecarga("Fallo la actualizacion, no tiene permisos.");
                        return;
                    }
                    else if (sReturn == 2)
                    {
                        Principal.setEstadoRecarga("Fallo la actualizacion, Inicializacion.");
                        return;
                    }
                    else if (sReturn == 3)
                    {
                        Principal.setEstadoRecarga("Fallo la actualizacion, Conexion");
                        return;
                    }
                    else if (sReturn == 4)
                    {
                        Principal.setEstadoRecarga("Fallo la recarga, Script");
                        return;
                    }
                    else if (sReturn == 5)
                    {
                        Principal.setEstadoRecarga("La aplicacion ya estaba recargandose por otro usuario, intente mas tarde.");
                        return;
                    }

                }
            }
            
            Principal.setEstadoRecarga("Actualizacion Finalizada.");
            Principal.setEstadoBoton(true);
        }
        [STAThread]
        static void Main(string[] args){
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 Principal;

            int iCantidadApps;

            if (args.Count() > 1) Principal = new Form1(args[0], args[1]);
            else Principal = new Form1("", "");
            Principal.Show();
            Principal.Refresh();

            iCantidadApps = args.Count() / 4;

            if (args.Count() > 1)
            {
                ThreadStart starter = delegate { recargaThread(args, iCantidadApps,Principal); };
                Thread newThread = new Thread(starter);
                newThread.Start();
            }
            else
            {
                Principal.setEstadoRecarga("Fallo la actualizacion, Parametros");
            }
            
            Application.Run();
         }
    }
}
