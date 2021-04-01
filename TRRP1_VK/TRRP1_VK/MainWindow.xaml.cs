using System;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.IO;
using VkNet;
using VkNet.Exception;
using VkNet.Model;


namespace TRRP1_VK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string FileName = "token.txt";
        private long StatususerID;
        private VkApi StatusVkApi;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonDoor(object sender, RoutedEventArgs e)
        {
            if (ButtonEntryExit.Content.ToString() == "Войти")
            {
                if (!IsFileExistsAndNotEmpty())
                {
                    var auth = new Authorization();
                    auth.ShowDialog();

                    if (auth.DialogResult == true)
                    {
                        WriteDataToFile(auth.Token, auth.UserID);
                    }
                    else
                    {
                        return;
                    }
                }

                var token = GetTokenFromFile();
                var userID = GetUserIDFromFile();

                Authorize(token, userID);
            }
            else
            {
                deleteCookie();
                File.Delete(FileName);
                Close();
            }
        }

        private void Authorize(string token, long userID)
        {
            try
            {
                VkApi vkApi = new VkApi();
                vkApi.Authorize(new ApiAuthParams
                {
                    AccessToken = token
                });
                MessageBox.Show("Вход выполнен!");
                ExternalChanges(userID, vkApi);
            }
            catch (VkAuthorizationException er)
            {
                MessageBox.Show("Ошибка " + er);
            }
        }

        private void ExternalChanges(long userID, VkApi vkApi)
        {
            ButtonEntryExit.Content = "Выход";

            var name = vkApi.Users.Get(new long[] { userID }).FirstOrDefault();
            LabelWelcom.Text = "Привет, " + name.FirstName + "!";

            string textStatus = vkApi.Status.Get(userID).Text;

            if (textStatus == "")
                TextStatus.Text = "Пока статус пуст.";
            else TextStatus.Text = textStatus;

            Block2.Visibility = Visibility.Hidden;
            Block3.Visibility = Visibility.Visible;

            StatususerID = userID;
            StatusVkApi = vkApi;
        }

        private void WriteDataToFile(string token, long userID)
        {
            using (var writeFile = new StreamWriter(FileName))
            {
                writeFile.WriteLine(Cryptographer.Encrypt(token));
                writeFile.Write(Cryptographer.Encrypt(userID.ToString()));
            }
        }

        private bool IsFileExistsAndNotEmpty()
        {
            if (!File.Exists(FileName)) return false;
            using (var file = File.OpenRead(FileName))
            {
                return file.Length != 0;
            }
        }

        private string GetTokenFromFile()
        {
            using (var readFile = new StreamReader(FileName))
            {
                var token = readFile.ReadLine();
                return Cryptographer.Decrypt(token);
            }
        }

        private long GetUserIDFromFile()
        {
            using (var readFile = new StreamReader(FileName))
            {
                readFile.ReadLine();
                var readID = readFile.ReadLine();
                var userID = Cryptographer.Decrypt(readID);
                return long.Parse(userID);
            }
        }

        private void deleteCookie()
        {
            Process.Start("cmd.exe", "/C RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 255");
            string[] cookie = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            foreach (string CookieFile in cookie)
            {
                try
                {
                    File.Delete(CookieFile);
                }
                catch
                {
                    return;
                }
            }
        }

        private void ButtonNewStatus(object sender, RoutedEventArgs e)
        {
            StatusVkApi.Status.Set(WriteTextStatus.Text);
            WriteTextStatus.Text = "";
            TextStatus.Text = StatusVkApi.Status.Get(StatususerID).Text;
        }

        private void ButtonDeleteStatus(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить статус", "Мой статус", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    StatusVkApi.Status.Set("");
                    string text_status = StatusVkApi.Status.Get(StatususerID).Text;
                    if (text_status == "")
                        TextStatus.Text = "Пока статус пуст.";
                    else
                    {
                        MessageBox.Show("Статус не пуст!");
                        TextStatus.Text = text_status;
                    }
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        private void ButtonEditStatus(object sender, RoutedEventArgs e)
        {
            if (EditStatus.Content.ToString() == "Редактировать статус")
            {
                EditStatus.Content = "Сохранить";
                WriteTextStatus.Text = StatusVkApi.Status.Get(StatususerID).Text;
                NewStatus.IsEnabled = false;
                DeleteStatus.IsEnabled = false;
            }
            else
            {
                StatusVkApi.Status.Set(WriteTextStatus.Text);
                WriteTextStatus.Text = "";
                TextStatus.Text = StatusVkApi.Status.Get(StatususerID).Text;
                EditStatus.Content = "Редактировать статус";
                NewStatus.IsEnabled = true;
                DeleteStatus.IsEnabled = true;
            }
        }
    }
}
