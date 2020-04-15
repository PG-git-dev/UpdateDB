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

namespace BaseUpdate
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class ConnSettWindow : Window
    {
        public ConnSettWindow()
        {
            InitializeComponent();

            IniFile INI = new IniFile("dbConf.ini");

            serverNameTextBox.Text = INI.ReadINI("DB_Connection", "Source");

            DbTextBox.Text = INI.ReadINI("DB_Connection", "Catalog");
            SaveButton.IsEnabled = false;
        }

        private void ServerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }

        private void TextBox_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButton.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            IniFile toIni = new IniFile("dbConf.ini");
            toIni.Write("DB_Connection", "Source", serverNameTextBox.Text);
            toIni.Write("DB_Connection", "Catalog", DbTextBox.Text);
            SaveButton.IsEnabled = false;
        }
    }
}
