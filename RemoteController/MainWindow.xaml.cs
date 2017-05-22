using System;
using System.Configuration;
using System.Messaging;
using System.Windows;
using System.Windows.Forms;
using MessageQueue.RemoteController.Models;
using MessageBox = System.Windows.MessageBox;

namespace MessageQueue.RemoteController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _remoteControlQueueName;

        public MainWindow()
        {
            InitializeComponent();
            LoadProcessingServiceData();
            _remoteControlQueueName = ConfigurationManager.AppSettings["RemoteControlQueueName"];
        }

        private void SelectOutputDirCommand(object sender, RoutedEventArgs e)
        {
            using(var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.SelectedPath = OutputDirectory.Text;
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    OutputDirectory.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void SelectTrashDirCommand(object sender, RoutedEventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.SelectedPath = TrashDirectory.Text;
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    TrashDirectory.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void SaveProcessingSettingsCommand(object sender, RoutedEventArgs e)
        {
            UpdateProcessingServiceSettings();
        }

        private void RecieveDateCommand(object sender, RoutedEventArgs e)
        {
            LoadProcessingServiceData();
        }

        private void LoadProcessingServiceData()
        {
            using (var remoteControlQueue = new System.Messaging.MessageQueue(_remoteControlQueueName, QueueAccessMode.SendAndReceive))
            {
                var processingServiceDataRequest = new RemoteControlCommand();
                remoteControlQueue.Send(new System.Messaging.Message(processingServiceDataRequest));
                var processingServiceMessage = remoteControlQueue.Receive();
                var processingServiceData = processingServiceMessage?.Body as ProcessingServiceData;
                if (processingServiceData == null)
                {
                    MessageBox.Show("Wrong data recieved!");
                    return;
                }

                OutputDirectory.Text = processingServiceData.OutputDirectory;
                TrashDirectory.Text = processingServiceData.TrashDirectory;
                SequanceTime.Text = processingServiceData.SequanceTime.ToString();
                ProcessingServiceStatus.Text = processingServiceData.Status;
            }
        }

        private void UpdateProcessingServiceSettings()
        {
            throw new NotImplementedException();
        }
    }
}
