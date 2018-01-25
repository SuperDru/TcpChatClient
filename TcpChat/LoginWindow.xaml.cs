using System.Collections;
using System.Windows;
using System.IO;


namespace TcpChat
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        ArrayList parms = new ArrayList();
        string ip;
        
        //Конструктор считывает данные с конфига и парсит их в массив параметров
        public LoginWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.Height += 6;
            Application.Current.MainWindow.Width += 6;
            using (StreamReader sr = new StreamReader("Config.txt")){
                while (!sr.EndOfStream)
                {
                    parms.Add(sr.ReadLine());
                }
                foreach (string parm in parms)
                {
                    switch (parm.Split('=')[0])
                    {
                        case "name":
                            name.Text = parm.Split('\"')[1];
                            break;
                        case "ip":
                            ip = parm.Split('\"')[1];
                            break;
                    }
                }
            }
        }
       
        private void button_Click(object sender, RoutedEventArgs e)
        {
            Client win = new Client(name.Text, ip);
            win.Show();
            Close();
        }

        private void name_GotFocus(object sender, RoutedEventArgs e)
        {
            name.Text = "";
        }

        private void name_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Client win = new Client(name.Text, ip);
                win.Show();
                Close();
            }
        }
    }
}
