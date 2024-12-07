

using System.Net.Sockets;
using System.Net;
using System.Text;
using Bybit.Net;
using Bybit.Net.Clients;
using Bybit.Net.Objects.Models.V5;
using Newtonsoft.Json.Linq;
using CryptoExchange.Net.Interfaces;
using System.Data.SqlClient;

class ExchangeData
{
    public BybitRestClient client;
    public List<string> pares;
    public ExchangeData()
    {
        client = new BybitRestClient();
    }
    public void GetPares()
    {
        pares = new List<string>();
        var bybitSymbols = client.V5Api.ExchangeData.GetSpotSymbolsAsync().Result.Data.List;
        foreach (var pare in bybitSymbols)
        {
            pares.Add(pare.Name); 
        }
    }
    public (decimal, decimal) GetPricePerPare(string pare)
    {
        try
        {
            var orderBook = client.V5Api.ExchangeData.GetOrderbookAsync(Bybit.Net.Enums.Category.Spot, pare, 5).Result;
            if (orderBook.Success)
            {
                var bids = orderBook.Data.Bids.ToList().First().Price;
                var asks = orderBook.Data.Asks.ToList().First().Price;
                return (bids, asks);
            }
        }
        catch { return (0, 0); }
        return (0, 0);
    }
}
class User
{
    public DateTime connectionTime;
    public DateTime disconnectTime;
    public EndPoint point;
    public int messagesPerHour = 5;
    public string password;
    public string login;
    public bool isAuthorized = false;
    public User(EndPoint point)
    {
        this.point = point;
    }
    public void MinusMessage()=>messagesPerHour--;
    public void Register(string password, string login)
    {
        this.password = password;
        this.login = login;
    }
    public override string ToString()
    {
        return $"Пользователь: {point}";
    }

}
class SQL
{
    private string connectionString;

    public SQL(string connectionString)
    {
        this.connectionString = connectionString;
    }
    public (List<string>, List<string>, List<string>) GetDataAsync(string query)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var adapter = new SqlDataAdapter(query, connection);
        var command = adapter.SelectCommand;
        command.CommandText = query;
        (List<string>, List<string>, List<string>) tuple = (new List<string>(), new List<string>(), new List<string>());
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string password = reader.GetString(0);
                string login = reader.GetString(1);
                string endPoint = reader.GetString(2);
                tuple.Item1.Add(password);
                tuple.Item2.Add(login);
                tuple.Item3.Add(endPoint);
            }
        }
        return tuple;
    }
    public void AddDataAsync(string query)
    {
        using var connection = new SqlConnection(connectionString);
        using var adapter = new SqlDataAdapter();
        try
        {
            using SqlCommand command = new SqlCommand(query, connection);
            adapter.InsertCommand = command;

            connection.Open();
            var rowsAffected = adapter.InsertCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
class Server
{
    public List<string> LogInfo = new List<string>();
    public Action<string> show;
    public List<User> connectedUsers;
    public TcpListener listener;
    public ExchangeData bybit;
    public List<User> usersData;
    public SQL sql = new SQL(@"Server=(localdb)\MSSQLLocalDB;Database=User;Trusted_Connection=True;");
    public Server()
    {
        if (File.Exists("log.txt"))
        {
            Thread.Sleep(500);
            File.Delete("log.txt");
            Thread.Sleep(1000);
            File.Create("log.txt");
        }
        else
        {
            File.Create("log.txt");
        }
        show = (string message) =>
        {
            Console.WriteLine(message);
        };
        usersData = new List<User>();
        bybit = new ExchangeData(); 
        bybit.GetPares();
        listener = new TcpListener(new IPEndPoint(IPAddress.Any, 8080));
        connectedUsers = new List<User>();
    }
    public void Log(List<string> msg)
    {
        using(FileStream fs = new FileStream("log.txt",FileMode.Open, FileAccess.Write))
        {
            using(StreamWriter sw = new StreamWriter(fs))
            {
                foreach(string s in msg)
                {
                    sw.WriteLine(s);    
                }
            }
        }
    }
    public async Task ListenAsync()
    {
        listener.Start();
        try
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => ClientHandler(client));
            }
        }
        catch (Exception ex)
        {
            show.Invoke(ex.Message);
        }
        finally
        {
            listener?.Stop();
        }
    }
    public bool CheckUserUnique(User user)
    {
        var ipParts = user.point.ToString().Split(new char[] { '.', ':' });
        string userIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.{ipParts[3]}";
        foreach (User us in connectedUsers)
        {
            ipParts = us.point.ToString().Split(new char[] { '.', ':' });
            string tmpIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.{ipParts[3]}";
            if (tmpIp == userIp) return false;
        }
        return true;
    }
    public void PushUserToDataBase(User user)
    {
        sql.AddDataAsync($"insert into UserInfo (Password, Login, IPEndPoint) " +
            $"values ('{user.password}', '{user.login}', '{user.point}')");
        Console.WriteLine("Пользователь зарегистрирован");
    }
    public bool CheckUserIsLogined(User user)
    {
        (List<string>,List<string>,List<string>) data = sql.GetDataAsync("select * from UserInfo");
        bool isLogged = false;
        for(int i = 0; i < data.Item2.Count; i++)
        {
            if(user.password.Replace("\n",string.Empty).Trim() == data.Item1[i].Replace("\n", string.Empty).Trim() && user.login.Replace("\n", string.Empty).Trim() == data.Item2[i].Replace("\n", string.Empty).Trim())
            {
                Console.WriteLine(data.Item1[i]+" " + data.Item2[i]);
                isLogged = true;
                break;
            }
        }
        return isLogged;
    }
    public async void ClientHandler(TcpClient client)
    {
        User user = new User(client.Client.RemoteEndPoint);
        try
        {       
            var stream = client.GetStream();
            if (connectedUsers.Count < 5)
            {
                if (CheckUserUnique(user) == true) connectedUsers.Add(user);
                else
                {
                    stream.Write(Encoding.UTF8.GetBytes("Ты уже подключен какашак!" + '\n'));
                    client.Close();
                }
                show.Invoke($"Подключен {user.ToString()} {user.connectionTime}");
                LogInfo.Append($"Подключен {user.ToString()} {user.connectionTime}");
                List<byte> buffer = new List<byte>();

                while (true)
                {
                    int readedByte = stream.ReadByte();
                    if (readedByte == -1) continue;
                    if (readedByte == '\n')
                    {
                        var message = Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count).Replace("\n", string.Empty);
                        if (message.Contains("/reg"))
                        {
                            var tmp = message.Replace("/reg", string.Empty).Replace("\n", string.Empty).Split(" ");
                            string login = tmp[0];
                            string password = tmp[1];
                            Console.WriteLine(login+" "+password);
                            user.Register(login, password);
                            PushUserToDataBase(user);
                            user.isAuthorized = true;  
                            LogInfo.Add($"Пользователь зарегистрировался {user}");
                            stream.Write(Encoding.UTF8.GetBytes("Вы успешно зарегистрировались!" + '\n'));
                        }
                        if (message.Contains("/log"))
                        {
                            var tmp = message.Replace("/log", string.Empty).Replace("\n", string.Empty).Split(" ");
                            string login = tmp[0];
                            string password = tmp[1];
                            Console.WriteLine(login+" "+password);
                            if(CheckUserIsLogined(user) == true)
                            {
                                user.isAuthorized = true;
                                stream.Write(Encoding.UTF8.GetBytes("Вы успешно вошли в систему!" + '\n'));
                                LogInfo.Add($"Пользователь вошел в систему {user}");
                            }
                            else
                            {
                                LogInfo.Add($"Пользователь ввел неверные данные {user}");
                                stream.Write(Encoding.UTF8.GetBytes("Данные введены неверно, попробуйте подключиться еще раз" + '\n'));
                                client.Close();
                            }
                        }
                        if (message.Contains("/getPares"))
                        {
                            if (user.isAuthorized == false) client.Close();
                            if (user.messagesPerHour <= 0) client.Close();
                            user.MinusMessage();
                            show.Invoke("Запрос на получение пар");
                            string tempMsg = string.Empty;
                            foreach (var p in bybit.pares)
                            {
                                tempMsg += p + " ";
                            }
                            stream.Write(Encoding.UTF8.GetBytes(tempMsg + '\n'));
                            show.Invoke("Пары отправлены");
                        }
                        if (message.Contains("/pareInfo"))
                        {
                            if (user.isAuthorized == false) client.Close();
                            if(user.messagesPerHour<= 0) client.Close();
                            user.MinusMessage();
                            if (bybit.pares.Contains(message.Replace("\n", string.Empty).Replace("/pareInfo",string.Empty)))
                            {
                                LogInfo.Append($"{user.ToString()} {user.disconnectTime} запрос на получение цен по паре {message.Replace("\n", string.Empty).Replace("/pareInfo", string.Empty)}");
                                show.Invoke("Запрос на получение цен");
                                var prices = bybit.GetPricePerPare(message.Replace("\n", string.Empty).Replace("/pareInfo", string.Empty));
                                string responce = $"{message.Replace("\n", string.Empty).Replace("/pareInfo", string.Empty)} | Покупка: {prices.Item1}$ Продажа: {prices.Item2}$";
                                stream.Write(Encoding.UTF8.GetBytes(responce + '\n'));
                            }
                            else
                            {
                                stream.Write(Encoding.UTF8.GetBytes("Пара введена неверно" + '\n'));
                            }
                        }
                        buffer.Clear();
                    }
                    buffer.Add((byte)readedByte);
                }
            }
            else
            {
                LogInfo.Append($"Перегрузка сервера {DateTime.Now}");
                stream.Write(Encoding.UTF8.GetBytes("Сервер перегружен, попробуйте позже..." + '\n'));
                client.Close();
            }
        }
        catch (Exception)
        {
            show.Invoke("Пользователь отключился из за своей причины");
            try
            {
                Log(LogInfo);
            }
            catch { }
            LogInfo = new List<string>();
            connectedUsers.Remove(user);
        }
        finally
        {
            client.Close();
        }
    }
}
internal class Program
{
    static async Task Main(string[] args)
    {
        Server server = new Server();
        Task t1 = server.ListenAsync();
        t1.Wait();
    }
}