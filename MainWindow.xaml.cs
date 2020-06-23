using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MySecureData
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer TimerLoginStart = new DispatcherTimer();

        string PmPath = "";
        List<CPwDat> ListPw = new List<CPwDat>();

        string LoginImage = "";

        public MainWindow()
        {
            InitializeComponent();
            ImageClock.Visibility = Visibility.Hidden;

            SetWindowLayout();

            TimerLoginStart.Interval = TimeSpan.FromSeconds(1);
            TimerLoginStart.Tick += TimerLoginStart_Tick;
        }

        private void TimerLoginStart_Tick(object sender, EventArgs e)
        {
            TimerLoginStart.Stop();
            Login();
        }

        private void SetWindowLayout()
        {
            string w = GetSetting("Width");

            if (w != "")
                WindowMySecureData.Width = Convert.ToDouble(w);
            else
                WindowMySecureData.Width = System.Windows.SystemParameters.PrimaryScreenWidth;


            string h = GetSetting("Height");

            if (h != "")
                WindowMySecureData.Height = Convert.ToDouble(h);
            else
                WindowMySecureData.Height = System.Windows.SystemParameters.PrimaryScreenHeight / 4.0;

            string t = GetSetting("Top");

            if (t != "")
                WindowMySecureData.Top = Convert.ToDouble(t);

            string l = GetSetting("Left");

            if (l != "")
                WindowMySecureData.Left = Convert.ToDouble(l);

        }
        private void WindowMySecureData_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanelUser.Visibility = Visibility.Visible;
            StackPanelPath.Visibility = Visibility.Visible;
            StackPanelMainLeft.Visibility = Visibility.Collapsed;
            StackPanelMainLeft2.Visibility = Visibility.Collapsed;
            StackPanelMainRight.Visibility = Visibility.Collapsed;
            ButtonDataCount.Visibility = Visibility.Collapsed;
            ButtonChangeDirMain.Visibility = Visibility.Collapsed;
            StackPanelFilter.Visibility = Visibility.Collapsed;
            StackPanelHelp.Visibility = Visibility.Collapsed;
            ListViewPwDat.Visibility = Visibility.Collapsed;
            ImagePw.Visibility = Visibility.Visible;


            ButtonPath.Visibility = Visibility.Hidden;
            TextBoxPath.Visibility = Visibility.Hidden;
        }


        private void WindowMySecureData_LocationChanged(object sender, EventArgs e)
        {
            SetSetting("Top", WindowMySecureData.Top.ToString());
            SetSetting("Left", WindowMySecureData.Left.ToString());
        }

        private void WindowMySecureData_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSetting("Height", WindowMySecureData.ActualHeight.ToString());
            SetSetting("Width", WindowMySecureData.ActualWidth.ToString());
        }






        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {

            if (!CheckUsePwLen(true))
                return;

            ImagePw.Visibility = Visibility.Hidden;

            PmPath = GetSetting("PmPath");
            if (PmPath == "" || !File.Exists(PmPath + "\\" + "MyPW.txt"))
            {
                MessageBox.Show("PasswordFile '\n" + PmPath + "\\" + "  'MyPW.txt' " + "\nkann nicht gefunden werden\n\n" +
                                "- Mit 'Change Directory' einen Ordner wählen, in dem die 'MyPW.txt'-Datei des Users gespeichert ist\n" +
                                "oder\n" +
                                "- Mit 'Change Directory'  einen leeren Ordners anwählen, es wird eine neue 'MyPW.txt'-Datei erstellt");

                ButtonPath.Visibility = Visibility.Visible;
                TextBoxPath.Visibility = Visibility.Visible;
                return;
            }

            TextBoxPath.Text = PmPath + "\\" + "MyPW.txt";
            TextBoxPath.Visibility = Visibility.Visible;
            TextBoxPath.Background = System.Windows.Media.Brushes.LightGreen;
            ButtonDataCount.Visibility = Visibility.Visible;
            ButtonChangeDirMain.Visibility = Visibility.Visible;
            ImageClock.Visibility = Visibility.Visible;
            TimerLoginStart.Start();

        }

        private void ButtonPath_Click(object sender, RoutedEventArgs e)
        {

            if (!CheckUsePwLen(true))
                return;

            PmPath = GetPath();


            if (PmPath == "")
            {
                MessageBox.Show("Kein  Ordner ausgewählt - Bitte wiederholen");
                return;
            }
            else
            {
                SetSetting("PmPath", PmPath);
            }
            SelectPasswordFile();

        }

        private void ButtonInputStore_Click(object sender, RoutedEventArgs e)
        {
            string titleUp = TextBoxTitelIn.Text.Trim().ToUpper();


            if (titleUp == "")
            {
                MessageBox.Show("Titel im Datensatz fehlt");
                return;
            }

            // Durchsuchen der Liste nach dem neuen Titel
            CPwDat dat = ListPw.Find(x => x.Title.ToUpper() == titleUp);

            if (dat != null)
            {
                MessageBoxResult result =
                MessageBox.Show("Es gibt schon einen Datensatz mit dem Titel\n     " + dat.Title +
                                "\nÜberschreiben", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;

                ListPw.Remove(dat);
            }

            CPwDat newDat = new CPwDat { Title = TextBoxTitelIn.Text.Trim(), WebAdr = TextBoxWebAdrIn.Text.Trim(), User = TextBoxUserIn.Text.Trim(), PW = TextBoxPwIn.Text.Trim(), Opt1 = TextBoxOpt1In.Text.Trim(), Opt2 = TextBoxOpt2In.Text.Trim() };

            ListPw.Add(newDat);

            PwDatToListView(TextBoxFilter.Text);

            EncryptFile();

            WriteOutputFields(newDat);
        }


        private void ButtonFilterClear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxFilter.Text = "";
            TextBoxFilter.Focus();
            PwDatToListView("");
        }

        private void ButtonDataCount_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", PmPath);
        }

        private void ButtonExImport_Click(object sender, RoutedEventArgs e)
        {
            WindowExImport winExIm = new WindowExImport(TextBoxUser.Text, MyPasswordBox.Password);
            bool? result = winExIm.ShowDialog();



            if (result != true)
                return;

            string temp = System.IO.Path.GetTempPath();

            if (DoExport)
            {
                MessageBox.Show("Die Daten werden in lesbarer Form (TXT-Datei) ausgegeben\n" +
                                "Bitte die Datei\n" +
                                "PASSWORT.TXT\n" +
                                "aus dem angezeigten Ordner nach Gebrauch\n" +
                                "sofort löschen !!! ");


                string tempPath = System.IO.Path.GetTempPath();


                try
                {
                    using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(tempPath, "PASSWORT.TXT")))
                    {
                        foreach (CPwDat item in ListPw)
                        {
                            if (item.Title.StartsWith("_") && item.Opt2.StartsWith("_"))
                                continue;
                            string line = item.Title + ";" + item.WebAdr + ";" + item.User + ";" + item.PW + ";" + item.Opt1 + ";" + item.Opt2;
                            outputFile.WriteLine(line);
                        }
                    }
                }
                catch (Exception)
                {

                    MessageBox.Show("Fehler beim Schreiben von PASSWORT.TXT");
                }

                if (Directory.Exists(temp))
                    Process.Start("explorer.exe", tempPath);

            }



            if (DoImport)
            {

                MessageBoxResult doImport =
                MessageBox.Show("Die importiere Datei überschreibt alle Daten\nWirklich importieren ?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning);


                if (doImport != MessageBoxResult.Yes)
                    return;

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.ShowDialog();

                if (ofd.FileName == "")
                    return;

                SimpleTxtToListPw(ofd.FileName);

                PwDatToListView("");

                EncryptFile();

            }


        }

        private void ButtonHelp_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo("Resources\\Manual.pdf")
                {
                    UseShellExecute = true
                };

                p.Start();
            }
            catch (Exception)
            {

                MessageBox.Show("Resources\\Installation.pdf" + "\nkann nicht gestartet werden");
            }


            return;



            //string tempPath = System.IO.Path.GetTempPath();

            //Random rnd = new Random();
            //try
            //{
            //    using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(tempPath, "PASSWORTTEST.TXT")))
            //    {
            //        foreach (CPwDat item in ListPw)
            //        {

            //            string Title = "";
            //            string WebAdr = "";
            //            string User = "";
            //            string PW = "";
            //            string Opt1 = "";
            //            string Opt2 = "";

            //            Title = item.Title;
            //            WebAdr = item.WebAdr;

            //            string[] u = item.User.Split(":");
            //            if (u.Length > 1)
            //            {
            //                int l = (u[1].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                User = u[0] + ":" + CMyRandom.Chars[from..(from + l)];

            //            }
            //            else
            //            {
            //                int l = (u[0].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                User = CMyRandom.Chars[from..(from + l)];
            //            }

            //            string[] p = item.PW.Split(":");
            //            if (p.Length > 1)
            //            {
            //                int l = (p[1].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                PW = p[0] + ":" + CMyRandom.Chars[from..(from + l)];

            //            }
            //            else
            //            {
            //                int l = (p[0].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                PW = CMyRandom.Chars[from..(from + l)];
            //            }

            //            string[] o = item.Opt1.Split(":");
            //            if (o.Length > 1)
            //            {
            //                int l = (o[1].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                Opt1 = o[0] + ":" + CMyRandom.Chars[from..(from + l)];

            //            }
            //            else
            //            {
            //                int l = (o[0].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                Opt1 = CMyRandom.Chars[from..(from + l)];
            //            }

            //            string[] o2 = item.Opt2.Split(":");
            //            if (o2.Length > 1)
            //            {
            //                int l = (o2[1].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                Opt2 = o2[0] + ":" + CMyRandom.Chars[from..(from + l)];

            //            }
            //            else
            //            {
            //                int l = (o2[0].Trim().Length);
            //                int from = rnd.Next(CMyRandom.Chars.Length - l - 1);
            //                Opt2 = CMyRandom.Chars[from..(from + l)];
            //            }


            //            string line = Title + ";" + WebAdr + ";" + User + ";" + PW + ";" + Opt1 + ";" + Opt2;
            //            outputFile.WriteLine(line);
            //        }
            //    }
            //}
            //catch (Exception)
            //{

            //    MessageBox.Show("Fehler beim Schreiben von PASSWORT.TXT");
            //}

            //if (Directory.Exists(tempPath))
            //    Process.Start("explorer.exe", tempPath);

        }

        private void ButtonChangeDirMain_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result =
            MessageBox.Show("Um einen neuen Ablageort einzurichten wird das Programm runtergefahren\n" +
                            "Es muss dann neu gestartet werden\n\nSoll dies ausgeführt werden ?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            SetSetting("PmPath", "");
            Environment.Exit(0);
        }




        #region Button OUT
        private void ButtonTitleOut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, TextBoxTitelOut.Text);
        }
        private void ButtonTitleOut_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxTitelOut.Background = Brushes.Gray;
        }
        private void ButtonTitleOut_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxTitelOut.Background = Brushes.White;
        }


        private void ButtonWebAdrOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo(TextBoxWebAdrOut.Text)
                {
                    UseShellExecute = true
                };

                p.Start();
            }
            catch (Exception)
            {

                MessageBox.Show(TextBoxWebAdrOut.Text + "\nkann nicht gestartet werden");
            }



        }
        private void ButtonWebAdrOut_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxWebAdrOut.Background = Brushes.Gray;
        }
        private void ButtonWebAdrOut_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxWebAdrOut.Background = Brushes.White;
        }

        private void ButtonUserOut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, TextBoxUserOut.Text);
        }
        private void ButtonUserOut_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxUserOut.Background = Brushes.Gray;
        }
        private void ButtonUserOut_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxUserOut.Background = Brushes.White;
        }

        private void ButtonPwOut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, TextBoxPwOut.Text);
        }
        private void ButtonPwOut_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxPwOut.Background = Brushes.Gray;
        }
        private void ButtonPwOut_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxPwOut.Background = Brushes.White;
        }

        private void ButtonOpt1Out_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, TextBoxOpt1Out.Text);
        }
        private void ButtonOpt1Out_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxOpt1Out.Background = Brushes.Gray;
        }
        private void ButtonOpt1Out_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxOpt1Out.Background = Brushes.White;
        }

        private void ButtonOpt2Out_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, TextBoxOpt2Out.Text);
        }
        private void ButtonOpt2Out_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBoxOpt2Out.Background = Brushes.Gray;
        }
        private void ButtonOpt2Out_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBoxOpt2Out.Background = Brushes.White;
        }

        #endregion

        #region Button Delete
        private void ButtonTitleDelete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxTitelIn.Text = "";
        }

        private void ButtonWebAdrDelete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxWebAdrIn.Text = "";
        }

        private void ButtonUser1Delete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxUserIn.Text = "";
        }

        private void ButtonPwDelete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxPwIn.Text = "";
        }

        private void ButtonOpt1Delete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxOpt1In.Text = "";
        }

        private void ButtonOpt2Delete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxOpt2In.Text = "";
        }

        private void ButtonInputDelete_Click(object sender, RoutedEventArgs e)
        {
            TextBoxTitelIn.Text = "";
            TextBoxWebAdrIn.Text = "";
            TextBoxUserIn.Text = "";
            TextBoxPwIn.Text = "";
            TextBoxOpt1In.Text = "";
            TextBoxOpt2In.Text = "";
        }

        #endregion

        private void MyPasswordBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonLogin_Click(null, null);
            }
        }

   

        private void ListViewPwDat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewPwDat.SelectedItem == null)
                return;

            CPwDat selItem = (CPwDat)ListViewPwDat.SelectedItem;
            WriteInputFields(selItem);
            WriteOutputFields(selItem);
        }
        private void WriteInputFields(CPwDat selItem)
        {
            TextBoxTitelIn.Text = selItem.Title;
            TextBoxWebAdrIn.Text = selItem.WebAdr;
            TextBoxUserIn.Text = selItem.User;
            TextBoxPwIn.Text = selItem.PW;
            TextBoxOpt1In.Text = selItem.Opt1;
            TextBoxOpt2In.Text = selItem.Opt2;
        }

        private void WriteOutputFields(CPwDat selItem)
        {
            TextBoxTitelOut.Text = selItem.Title;


            string s = selItem.WebAdr;
            if (s.Contains(":") && !s.ToUpper().Contains("WWW.") && !s.ToUpper().Contains("HTTP"))
            {
                string[] sSplit = s.Split(':');
                LabelWebAdrOut.Content = sSplit[0];
                TextBoxWebAdrOut.Text = s.Substring(sSplit[0].Length + 1).Trim();
            }
            else
            {
                LabelWebAdrOut.Content = "Web-Adr";
                TextBoxWebAdrOut.Text = s;
            }

            s = selItem.User;
            if (s.Contains(":"))
            {
                string[] sSplit = s.Split(':');
                LabelUserOut.Content = sSplit[0];
                TextBoxUserOut.Text = s.Substring(sSplit[0].Length + 1).Trim();
            }
            else
            {
                LabelUserOut.Content = "User";
                TextBoxUserOut.Text = s;
            }

            s = selItem.PW;
            if (s.Contains(":"))
            {
                string[] sSplit = s.Split(':');
                LabelPwOut.Content = sSplit[0];
                TextBoxPwOut.Text = s.Substring(sSplit[0].Length + 1).Trim();
            }
            else
            {
                LabelPwOut.Content = "Password";
                TextBoxPwOut.Text = s;
            }

            s = selItem.Opt1;
            if (s.Contains(":"))
            {
                string[] sSplit = s.Split(':');
                LabelOpt1Out.Content = sSplit[0];
                TextBoxOpt1Out.Text = s.Substring(sSplit[0].Length + 1).Trim();
            }
            else
            {
                LabelOpt1Out.Content = "Opt1";
                TextBoxOpt1Out.Text = s;
            }

            s = selItem.Opt2;
            if (s.Contains(":"))
            {
                string[] sSplit = s.Split(':');
                LabelOpt2Out.Content = sSplit[0];
                TextBoxOpt2Out.Text = s.Substring(sSplit[0].Length + 1).Trim();
            }
            else
            {
                LabelOpt2Out.Content = "Opt2";
                TextBoxOpt2Out.Text = s;
            }
        }

        private void ListViewPwDat_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ListViewPwDat.SelectedItem == null)
                return;


            CPwDat selItem = (CPwDat)ListViewPwDat.SelectedItem;

            MessageBoxResult result =
           MessageBox.Show($"Datensatz '{selItem.Title}' löschen ?", "Achtung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;




            ListPw.Remove(selItem);

            PwDatToListView(TextBoxFilter.Text);

            EncryptFile();

            //Löschen der Ausgangsanzeige
            CPwDat dat = new CPwDat { Title = "", WebAdr = "", User = "", PW = "", Opt1 = "", Opt2 = "" };
            WriteOutputFields(dat);
        }

        private void LabelVersion_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var p = new Process();
                p.StartInfo = new ProcessStartInfo("Resources\\SettingDat.txt")
                {
                    UseShellExecute = true
                };

                p.Start();
            }
            catch (Exception)
            {

                MessageBox.Show("Resources\\SettingDat.txt" + "\nkann nicht gestartet werden");
            }
        }



        private void ImagePw_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = Mouse.GetPosition(ImagePw);

            double xd = ImagePw.ActualWidth / 44;
            double yd = ImagePw.ActualHeight / 12;

            int x = (int)(position.X / xd);
            int y = (int)(position.Y / yd);


            if (LoginImage.Length == 12)
                LoginImage = "";


            LoginImage += x.ToString("00") + y.ToString("00");

            MyPasswordBox.Password = LoginImage;

            //String loginImage = LoginImage + "                      ";
            //ButtonLogin.Content = loginImage[0..4] + " " + loginImage[4..8] + " " + loginImage[8..12] ;

        }

      

        private void TextBoxFilter_KeyUp(object sender, KeyEventArgs e)
        {
            PwDatToListView(TextBoxFilter.Text);
        }

    
        private bool CheckUsePwLen(bool msgYes)
        {
            if (TextBoxUser.Text.Length < 2 || TextBoxUser.Text.Length > 12)
            {
                if (!msgYes)
                    return false;
                MessageBox.Show("Eingabe im LOGIN:\n'User'  muss 2 bis 12 Zeichen haben");
                return false;
            }
            if (MyPasswordBox.Password.Length < 8 || MyPasswordBox.Password.Length > 16)
            {
                if (!msgYes)
                    return false;
                MessageBox.Show("Eingabe im LOGIN:\n'Password'  muss 8 bis 16 Zeichen haben");
                return false;
            }
            return true;
        }
        private void Login()
        {
            ImageClock.Visibility = Visibility.Hidden;


            if (DecryptFile())
            {

                StackPanelUser.Visibility = Visibility.Collapsed;
                StackPanelPath.Visibility = Visibility.Collapsed;
                StackPanelMainLeft.Visibility = Visibility.Visible;
                StackPanelMainLeft2.Visibility = Visibility.Visible;
                StackPanelMainRight.Visibility = Visibility.Visible;
                ButtonDataCount.Visibility = Visibility.Visible;
                ButtonChangeDirMain.Visibility = Visibility.Visible;
                StackPanelFilter.Visibility = Visibility.Visible;
                StackPanelHelp.Visibility = Visibility.Visible;
                ListViewPwDat.Visibility = Visibility.Visible;


                PwDatToListView("");

            }

        }
        private void PwDatToListView(string filterIn)
        {
            string filter = filterIn.ToUpper();
            int underlineCount = 0;

            ListViewPwDat.Items.Clear();

            ListPw.Sort();

            if (filter == "")
            {
                foreach (CPwDat item in ListPw)
                {
                    if (item.Title.StartsWith("_") && item.Opt2.StartsWith("_"))  //erster Random-Datensatz
                    {
                        underlineCount++;
                        continue;
                    }

                    ListViewPwDat.Items.Add(new CPwDat { Title = item.Title, User = item.User, PW = item.PW, Opt1 = item.Opt1, Opt2 = item.Opt2, WebAdr = item.WebAdr });
                }
            }
            else
            {
                foreach (CPwDat item in ListPw)
                {
                    if (item.ToString().ToUpper().Contains(filter))
                    {
                        if (item.Title.StartsWith("_") && item.Opt2.StartsWith("_"))  //erster Random-Datensatz
                        {
                            underlineCount++;
                            continue;
                        }
                        ListViewPwDat.Items.Add(new CPwDat { Title = item.Title, User = item.User, PW = item.PW, Opt1 = item.Opt1, Opt2 = item.Opt2, WebAdr = item.WebAdr });
                    }

                }
            }

            ButtonDataCount.Content = "Records: " + (ListPw.Count - underlineCount).ToString();

        }

        public static bool DoExport = false;
        public static bool DoImport = false;

        private bool DecryptFile()
        {
            bool ok = true;

            // ---------------     Entschlüsseln    -------------------------------
            // Erstelle eine Instanz von AES und weise ihr einen Schlüssel und Initialisierungsvektor zu
            // Wichtig ist hier das Padding, welches wir unbedingt definieren müssen.
            Aes AESCrypto = Aes.Create();
            AESCrypto.Padding = PaddingMode.Zeros;
            AESCrypto.Key = DoExtendKey((MyPasswordBox.Password + TextBoxUser.Text + CMyRandom.MoreKey).Substring(0, 32), 32);
            AESCrypto.IV = DoCreateBlocksize((TextBoxUser.Text + CMyRandom.MoreIV).Substring(0, 16), 16);


            try
            {
                // Die neue Datei hat den gleichen Namen, allerdings die Dateiendung .txt
                // Achtung: Die Originaldatei wird überschrieben, wenn diese sich im gleichen Pfad befindet!
                //     string neueDatei = System.IO.Path.ChangeExtension(openFileDialog1.FileName, ".txt");
                // Erstelle einen Inputstream, Outputstream und Cryptostream

                FileStream inputStream = new FileStream(PmPath + "\\" + "MyPW.txt", FileMode.Open);



                //FileStream outputStream = new FileStream(PmPath + "\\" + "Mp2.txt", FileMode.Create);
                CryptoStream cStream = new CryptoStream(inputStream, AESCrypto.CreateDecryptor(), CryptoStreamMode.Read);

                // Entschlüssele nun jedes Byte bis zum Dateiende
                int data;
                //while ((data = cStream.ReadByte()) != -1)
                //    outputStream.WriteByte((byte)data);

                StringBuilder sb = new StringBuilder();

                while ((data = cStream.ReadByte()) != -1)
                {
                    sb.Append(((char)data).ToString());
                }

                string[] s = sb.ToString().Split('\n');

                ListPw.Clear();

                foreach (string item in s)
                {
                    string[] b = item.Split(';');
                    if (b.Length == 6)
                    {
                        CPwDat pw = new CPwDat();
                        pw.Title = b[0].Trim();
                        pw.WebAdr = b[1].Trim();
                        pw.User = b[2].Trim();
                        pw.PW = b[3].Trim();
                        pw.Opt1 = b[4].Trim();
                        pw.Opt2 = b[5].Trim();
                        ListPw.Add(pw);
                    }
                    else
                    {
                        if (ListPw.Count == 0)
                        {
                            MessageBox.Show("Passwort passt nicht\n\nPasswort oder Directory ändern\n\n" +
                                "Um die Daten neu anzulegen, muss die Datei 'MyPW.txt' gelöscht werden\n" +
                                "Der Zugang zur Datei ist durch Klicken auf 'Datensätze' möglich");

                            ButtonPath.Visibility = Visibility.Visible;
                            TextBoxPath.Text = PmPath + "\\" + "MyPW.txt";
                            TextBoxPath.Visibility = Visibility.Visible;
                            ButtonDataCount.Visibility = Visibility.Visible;
                            ButtonChangeDirMain.Visibility = Visibility.Visible;
                            TextBoxPath.Background = System.Windows.Media.Brushes.LightGreen;
                            ImagePw.Visibility = Visibility.Visible;
                            ok = false;
                            break;
                        }
                        //else
                        //{
                        //    MessageBox.Show("Fehler bei Datensatz " + ListPw.Count, "", MessageBoxButton.OK, MessageBoxImage.Error);
                        //}

                    }

                }


                inputStream.Close();
                cStream.Close();


            }
            catch
            {
                MessageBox.Show("Ein Fehler ist aufgetreten!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ok;
        }

        private void EncryptFile()
        {

            // Ändern der Random-Daten - Verschlüsselung verbessert
            DeleteRandomData();
            AddRandomData();




            // ---------------     Verschlüsseln    -------------------------------
            // Erstelle eine Instanz von AES und weise ihr einen Schlüssel und Initialisierungsvektor zu
            Aes AESCrypto = Aes.Create();
            AESCrypto.Key = DoExtendKey((MyPasswordBox.Password + TextBoxUser.Text + CMyRandom.MoreKey).Substring(0, 32), 32);
            AESCrypto.IV = DoCreateBlocksize((TextBoxUser.Text + CMyRandom.MoreIV).Substring(0, 16), 16);


            try
            {






                // Die neue Datei hat den gleichen Namen, allerdings die Dateiendung .crypt
                //  string neueDatei = System.IO.Path.ChangeExtension(openFileDialog1.FileName, ".crypt");
                // Erstelle einen Inputstream, Outputstream und Cryptostream
                //FileStream inputStream = new FileStream(PmPath + "\\" + "MpCopy.txt", FileMode.Open);
                FileStream outputStream = new FileStream(PmPath + "\\" + "MyPW.txt", FileMode.Create);
                CryptoStream cStream = new CryptoStream(outputStream, AESCrypto.CreateEncryptor(), CryptoStreamMode.Write);

                // Verschlüssele nun jedes Byte bis zum Dateiende
                //int data;
                //while ((data = inputStream.ReadByte()) != -1)
                //    cStream.WriteByte((byte)data);

                foreach (CPwDat item in ListPw)
                {
                    string strPw = item.ToString() + "\n";

                    foreach (byte b in strPw)
                    {
                        cStream.WriteByte(b);
                    }
                }


                //inputStream.Close();
                cStream.Close();
                outputStream.Close();

            }
            catch
            {
                MessageBox.Show("Ein Fehler ist aufgetreten!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }



        /// <summary>
        /// Erweitert zu kurze Kennwörter mit Nullen
        /// </summary>
        /// <param name="key">Kennwort</param>
        /// <param name="newKeyLength">Neue Kennwortlänge</param>
        /// <returns></returns>
        private byte[] DoExtendKey(string key, int newKeyLength)
        {
            byte[] bKey = new byte[newKeyLength];
            byte[] tmpKey = Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < key.Length; i++)
            {
                bKey[i] = tmpKey[i];
            }

            return bKey;
        }

        /// <summary>
        /// Erweitert zu kurze Initialisierungsvektoren mit Nullen.
        /// </summary>
        /// <param name="newBlockSize">Neue Blockgröße</param>
        /// <returns></returns>
        private byte[] DoCreateBlocksize(string iv, int newBlockSize)
        {

            byte[] bIv = new byte[newBlockSize];
            byte[] tmpIv = Encoding.UTF8.GetBytes(iv);

            for (int i = 0; i < iv.Length; i++)
            {
                bIv[i] = tmpIv[i];
            }

            return bIv;


        }

        private void SelectPasswordFile()
        {

            TextBoxPath.Text = PmPath + "\\" + "MyPW.txt";

            if (!File.Exists(PmPath + "\\" + "MyPW.txt"))
            {
                TextBoxPath.Background = System.Windows.Media.Brushes.LightPink;

                MessageBoxResult result =
                MessageBox.Show("Die PasswordDatei 'MyPW.txt'\nexisiert noch nicht\nNeu erstellen mit OK\nOder nach CANCEL neuen Pfad anwählen", "Achtung", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        //AddRandomData();

                        File.WriteAllText(PmPath + "\\" + "MyPW.txt", "");

                        EncryptFile();

                        TextBoxPath.Background = System.Windows.Media.Brushes.LightGreen;
                        Login();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Fehler beim Schreiben der Datei");
                    }

                }

            }
            else
            {
                TextBoxPath.Background = System.Windows.Media.Brushes.LightGreen;
                Login();

            }
        }
        private string GetPath()
        {
            PmPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            MessageBox.Show("Wähle den Ordner für die Daten aus");

            //System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            //ofd.ShowDialog();



            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowDialog();

            return fbd.SelectedPath;

        }

        private void AddRandomData()
        {

            Random rnd = new Random();

            //ToDo File neu beschreiben
            for (int k = 0; k < 2; k++)
            {
                string[] term = new string[6];

                for (int i = 0; i < 6; i++)
                {

                    int len = rnd.Next(8, 32);
                    int start = rnd.Next(CMyRandom.Chars.Length - len - 1);
                    term[i] = CMyRandom.Chars.Substring(start, len);
                }

                ListPw.Add(new CPwDat { Title = "_" + term[0], WebAdr = term[1], User = term[2], PW = term[3], Opt1 = term[4], Opt2 = "_" + term[5] });
            }
        }

        private void DeleteRandomData()
        {
            for (int i = ListPw.Count - 1; i >= 0; i--)
            {
                if (ListPw[i].Title.StartsWith("_") && ListPw[i].Opt2.StartsWith("_"))
                {
                    ListPw.RemoveAt(i);
                }
            }


        }

        private void SimpleTxtToListPw(string pathAndFile)
        {
            ListPw.Clear();

            //AddRandomData();

            string[] s = File.ReadAllLines(pathAndFile);

            //string[] b = s[0].Split(";");

            foreach (string item in s)
            {
                string[] b = item.Split(';');
                CPwDat pw = new CPwDat();
                pw.Title = b[0];
                pw.WebAdr = b[1];
                pw.User = b[2];
                pw.PW = b[3];
                pw.Opt1 = b[4];
                pw.Opt2 = b[5];
                ListPw.Add(pw);
            }
        }


        private void CheckBoxOnTop_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxOnTop.IsChecked == true)
                WindowMySecureData.Topmost = true;
            else
                WindowMySecureData.Topmost = false;
        }

        private string GetSetting(string key)
        {

            string[] s = File.ReadAllLines("Resources\\SettingDat.txt");

            try
            {
                foreach (string item in s)
                {
                    if (item.StartsWith(key))
                    {
                        return item.Split(';')[1];
                    }
                }
                return "";
            }
            catch (Exception)
            {

                return "";
            }

        }

        private void SetSetting(string key, string val)
        {
            string[] s = File.ReadAllLines("Resources\\SettingDat.txt");
            bool found = false;

            try
            {
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i].StartsWith(key))
                    {
                        string[] keyVal = s[i].Split(';');

                        keyVal[1] = val;
                        s[i] = keyVal[0] + ";" + keyVal[1];
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Array.Resize(ref s, s.Length + 1);

                    s[s.Length - 1] = key + ";" + val;
                }

                File.WriteAllLines("Resources\\SettingDat.txt", s);



            }
            catch (Exception)
            {

                System.Windows.MessageBox.Show("Fehler beim schreiben des SettingFiles");
            }

        }
    }
}
