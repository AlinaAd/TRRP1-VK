using System.Windows;
using System.Windows.Navigation;

namespace TRRP1_VK
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private string site = "https://oauth.vk.com/authorize?client_id=7597011&display=page&redirect_uri=https://oauth.vk.com/blank.html&scope=status,offline&response_type=token&v=5.52";
        public string Token { get; private set; }
        public long UserID { get; private set; }

        public Authorization()
        {
            InitializeComponent();

            Webpage.Navigate(site);
            Webpage.Navigated += WebpageNavigated;
        }

        private void WebpageNavigated(object sender, NavigationEventArgs e)
        {
            string url = Webpage.Source.AbsoluteUri.ToString();
            if (url.Contains("#access_token="))
            {
                Webpage.Navigated -= WebpageNavigated;

                char[] symbols = { '=', '&' };
                string[] parts = url.Split(symbols);

                UserID = long.Parse(parts[5]);
                Token = parts[1];
                DialogResult = true;

                Close();
            }
        }
    }
}
