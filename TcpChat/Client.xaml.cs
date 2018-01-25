using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Input;
using System.Media;
using System.Windows.Controls;

namespace TcpChat
{
    delegate void del();
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Client : Window
    {
        string name, message;
        IPAddress ip;
        NetworkStream stream;
        TcpClient client;
        SoundPlayer sp;

        public Client(string name, string ip)
        {
            this.name = (name != "" && name != "Name") ? name : "NoNeedName";
            this.ip = IPAddress.Parse(ip);
            InitializeComponent();
            Application.Current.Windows[Application.Current.Windows.Count - 1].Height += 6;
            Application.Current.Windows[Application.Current.Windows.Count - 1].Width += 6;
            sp = new SoundPlayer(Properties.Resources.Sound_4589_Wav_Library_Net_);
            client = new TcpClient();
            client.BeginConnect(ip, 3329, CallBack, null);       
        }

        //Метод, вызываемый по заверешению соединения с сервером
        //Пытается получить поток от сервера и в случае успеха отправляет ему все необходимые начальные данные
        private void CallBack(IAsyncResult result)
        {
            try
            {
                stream = client.GetStream();
                SendMessage("/#/00" + name);
                StartReceiving();
            }
            catch (InvalidOperationException)
            {
                Suspend("Connection with server isn't established.");
                Reconnect();
            }
        }
//
//-----------------------------------------------------------------------------------------------------------------------------------------
//      
        //Начинает асинхронно ожидать данные от сервера и после обработки отсылает их остальным подключённым клиентам.
        //   /#/ - знак того, что данные информационные (после этого знака идут две цифры, сообщающие тип информации)
        //   /#/00 - пользователь с таким именем уже в сети
        //   /#/01 - добавление(удаление) пользователя
        async private void StartReceiving()
        {
            byte[] buff = new byte[256];
            try
            {
                while (true)
                {
                    int count = await stream.ReadAsync(buff, 0, buff.Length);
                    message = Encoding.GetEncoding(1251).GetString(buff, 0, count);
                    switch (message.Substring(0, 5))
                    {
                        case "/#/00":
                            Suspend("User with such name already exists. Please, reconnect to the server.");
                            Reconnect();
                            return;
                        case "/#/01":
                            ChangeUsers(message.Split(new string[] { "/#/01" }, StringSplitOptions.None)[2]);
                            message = message.Split(new string[] { "/#/01" }, StringSplitOptions.None)[1];
                            Append(message, Brushes.Blue);
                            break;
                        default:
                            Append(message.Substring(0, message.IndexOf(':') + 1), Brushes.LightGreen);
                            Append(message.Substring(message.IndexOf(':') + 2));
                            break;
                    }
                }
            }
            catch (IOException)
            {
                Suspend("Connection with server is interrupted.");
            }
        }

        //Отправляет асинхронно данные серверу
        async private void SendMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                byte[] buff = Encoding.GetEncoding(1251).GetBytes(message);
                await stream.WriteAsync(buff, 0, message.Length);
            }
        }
//
//-----------------------------------------------------------------------------------------------------------------------------------------
//
        
        private void ChangeUsers(string names)
        {
            UsersField.Dispatcher.Invoke(new del(() =>
            {
                UsersField.Document.Blocks.Clear();
                UsersField.AppendText(names);
            }));
        }
        private void Append(string message)
        {
            OutputField.Dispatcher.Invoke(new del(() =>
            {
                OutputField.AppendText(message + "\n");
                if (!IsActive)
                    sp.Play();
            }));
        }
        private void Append(string message, Brush brush)
        {
            OutputField.Dispatcher.Invoke(new del(() =>
            {
                TextRange tr = new TextRange(OutputField.Document.ContentEnd, OutputField.Document.ContentEnd);
                tr.Text = message + "\n";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                if (!IsActive)
                    sp.Play();
            }));
        }
        private void Suspend(string causeMessage)
        {
            Append(causeMessage, Brushes.Red);
            EnterField.Dispatcher.Invoke(new del(() =>
            {
                EnterField.IsEnabled = false;
                button.IsEnabled = false;
            }));
        }
        private void Reconnect()
        {
            EnterField.Dispatcher.Invoke(new del(() =>
            {
                button.IsEnabled = true;
                button.Content = "Reconnect";
                button.Foreground = Brushes.Red;
            }));
        }
//
//-----------------------------------------------------------------------------------------------------------------------------------------
//
        private void EnterField_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
                {
                    EnterField.AppendText("\r\n");
                    EnterField.SelectionStart = EnterField.Text.Length;
                }
                else
                {
                    SendMessage(EnterField.Text);
                    EnterField.Text = "";
                }
            }
            else if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                EnterField.SelectAll();
            }
        }
        private void OutputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            OutputField.ScrollToEnd();
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((string)button.Content == "Send")
            {
                EnterField.Text = "";
                SendMessage(EnterField.Text);
            }
            else
            {
                LoginWindow lw = new LoginWindow();
                lw.Show();
                Close();
            }
        }
    }
}
