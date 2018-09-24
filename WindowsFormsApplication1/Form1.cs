using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Exception = System.Exception;

namespace QMSEDX
{
    public partial class Form1 : Form
    {
        public Form1(string sApp, string sServer)
        {
            InitializeComponent();
            setEstadoBoton(false);
            setEstadoRecarga("Actualizando...");
            setServidor("");
            setAplicacion("");
            CheckForIllegalCrossThreadCalls = false;
        }

        public void setEstadoBoton(bool bEstado) {
            button3.Enabled = bEstado;
            this.ControlBox = bEstado;
        }

        public void setEstadoRecarga(string sEstado)
        {
            label4.Text = sEstado;
        }

        public void setServidor(string sServer)
        {
            label1.Text = sServer;
        }

        public void setAplicacion(string sNombre)
        {
            label5.Text = sNombre;
        }

        private void button3_Click(object sender, EventArgs e) {
            Application.Exit();
            Environment.Exit(1);
        }

        private void Form1_Closed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Environment.Exit(1);
        }
    }
}
