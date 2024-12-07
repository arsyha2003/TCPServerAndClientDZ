using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class RegistrationForm : Form
    {
        public bool isRegistration = false;
        public bool isLogin = false;
        public string login;
        public string password;
        public Action<string, string> register;
        public Action<string, string> log;
        public Action<string, string> sendToServer;
        public RegistrationForm(Action<string,string> reg, Action<string,string> l, Action<string, string> sendToServer)
        {
            InitializeComponent();
            register = reg; 
            log = l;
            this.sendToServer = sendToServer;
        }
        public void RegistrationButtonEvent(object sender, EventArgs e)
        {
            login = textBox1.Text;
            password = textBox2.Text;
            isRegistration = true;
            register.Invoke(login, password);
            sendToServer.Invoke(login, password);
            this.Close();
        }
        public void LoginButtonEvent(object sender, EventArgs e)
        {
            login = textBox1.Text;
            password = textBox2.Text;
            isLogin = true;
            log.Invoke(login, password);
            sendToServer.Invoke(login, password);
            this.Close();
        }
    }
}
