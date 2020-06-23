using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MySecureData
{
    /// <summary>
    /// Interaktionslogik für WindowExImport.xaml
    /// </summary>
    public partial class WindowExImport : Window
    {
        string user;
        string pw;
        string LoginImageHlp = "";

        public WindowExImport(string user, string pw)
        {
            this.user = user;
            this.pw = pw;
            MainWindow.DoExport = false;
            MainWindow.DoImport = false;

            InitializeComponent();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxUserHlp.Text != user || MyPasswordBoxHlp.Password != pw)
            {
                MessageBox.Show("User/Passwort ist falsch");
                return;
            }

            MainWindow.DoExport = true;
            DialogResult = true;



        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxUserHlp.Text != user || MyPasswordBoxHlp.Password != pw)
            {
                MessageBox.Show("User/Passwort ist falsch");
                return;
            }

            MainWindow.DoImport = true;
            DialogResult = true;
        }

        private void ImagePw_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point position = Mouse.GetPosition(ImagePw);

            double xd = ImagePw.ActualWidth / 44;
            double yd = ImagePw.ActualHeight / 12;

            int x = (int)(position.X / xd);
            int y = (int)(position.Y / yd);


            if (LoginImageHlp.Length == 12)
                LoginImageHlp = "";


            LoginImageHlp += x.ToString("00") + y.ToString("00");

            MyPasswordBoxHlp.Password = LoginImageHlp;

            //String loginImage = LoginImageHlp + "                      ";

            //ButtonExport.Content = loginImage[0..4] + " " + loginImage[4..8] + " " + loginImage[8..12];
        }
    }
}
