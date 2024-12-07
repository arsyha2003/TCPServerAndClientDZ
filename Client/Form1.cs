using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{

    public partial class Form1 : Form
    {
        public Client client;
        public Action<string> showResponce;
        public Action<string> showPares;
        public Action<string> showInfo;
        public Action<string, string> register;
        public Action<string, string> login;
        public Action<string, string> sendInfoToServer;
        public List<string> pares;
        public UserInfo userInfo;
        public Form1()
        {
            InitializeComponent();
            client = new Client();
            userInfo = new UserInfo(string.Empty, string.Empty);
            pares = new List<string>();
            sendInfoToServer = (string login, string password) =>
            {
                if (userInfo.isReg)
                {
                    client.SendRequest(showInfo, showResponce, $"/reg{login} {password}");
                }
                else if (userInfo.isLog)
                {
                    client.SendRequest(showInfo, showResponce, $"/log{login} {password}");
                }
            };
            register = (string login, string password) =>
            {
                userInfo.isReg = true;
                userInfo.password = password;
                userInfo.login = login;
            };
            login = (string login, string password) =>
            {
                userInfo.isLog = true;
                userInfo.password = password;
                userInfo.login = login;
            };
            showResponce = (string message) =>
            {
                textBox2.Text = message;
            };
            showInfo = (string message) =>
            {
                MessageBox.Show(message);
            };
            showPares = (string message) =>
            {
                pares = message.Split().ToList();
                comboBox1.Items.AddRange(pares.ToArray());
            };
        }
        public async void SendRequestButtonEvent(object sender, EventArgs e)
        {
            client.SendRequest(showInfo, showResponce, "/pareInfo"+textBox1.Text);
        }
        public async void GetParesFromServer(object sender, EventArgs e)
        {
            client.SendRequest(showInfo, showPares, "/getPares");
        }
        public async void SelectPare(object sender, EventArgs e)
        {
            textBox1.Text = comboBox1.SelectedItem.ToString();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            RegistrationForm form = new RegistrationForm(register, login, sendInfoToServer);
            form.Show();
        }
    }
    public class Client
    {
        public TcpClient client;
        public NetworkStream stream;
        public Client()
        {
            client = new TcpClient();
            client.ConnectAsync(IPAddress.Loopback, 8080);
            stream = client.GetStream();
        }
        public async Task SendRequest(Action<string> show, Action<string> getResponse, string message)
        {
            try
            {
                message += '\n';
                stream.WriteAsync(Encoding.UTF8.GetBytes(message));
                List<byte> buffer = new List<byte>();
                int byteRead = 0;
                while (byteRead != '\n')
                {
                    byteRead = stream.ReadByte();
                    buffer.Add((byte)byteRead);
                }
                var answer = Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count);
                getResponse.Invoke(answer);
            }
            catch (Exception ex) { show.Invoke(ex.Message); }
        }
    }
    public class UserInfo
    {
        public string password;
        public string login;
        public bool isReg = false;
        public bool isLog = false;
        public UserInfo(string password, string login)
        {
            this.password = password;
            this.login = login;
        }
        public override string ToString()
        {
            return $"{login} {password}";
        }
    }
}
