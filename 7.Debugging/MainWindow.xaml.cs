using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;

namespace Debugging
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        private void GenerateCommand(object sender, RoutedEventArgs e)
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().First();
            var networkBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
            var dateBytes = BitConverter.GetBytes(DateTime.Now.Date.ToBinary());
            var resultBytes = networkBytes.Select((a, b) => a ^ dateBytes[b]);
            Password.Text = string.Join("-", resultBytes.Select(b => b >= 999 ? b : b * 10));
        }
    }
}