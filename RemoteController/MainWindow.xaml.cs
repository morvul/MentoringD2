using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageQueue.RemoteController.Models;

namespace MessageQueue.RemoteController
{
    public partial class MainWindow
    {
        private readonly DcsRemoteControl _dcsRemoteControl;

        public MainWindow()
        {
            InitializeComponent();
            var remoteControlRecallDelay = int.Parse(ConfigurationManager.AppSettings["RemoteControlRecallDelay"]);
            var remoteControlQueueName = ConfigurationManager.AppSettings["RemoteControlQueueName"];
            _dcsRemoteControl = new DcsRemoteControl(remoteControlQueueName, remoteControlRecallDelay);
            LoadProcessingServiceData();
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

        private async void LoadProcessingServiceData()
        {
            ConfigForm.IsEnabled = false;
            var processingServiceData = await Task.Run(() => _dcsRemoteControl.GetProcessingServiceData());
            OutputDirectory.Text = processingServiceData.Settings.OutputDirectory;
            TrashDirectory.Text = processingServiceData.Settings.TrashDirectory;
            SequanceTime.Text = processingServiceData.Settings.SequanceTime.ToString();
            ProcessingServiceStatus.Text = processingServiceData.Status;
            ConfigForm.IsEnabled = true;
        }

        private async void UpdateProcessingServiceSettings()
        {
            ConfigForm.IsEnabled = false;
            var processingServiceSettings = new ProcessingServiceSettings
            {
                OutputDirectory = OutputDirectory.Text,
                TrashDirectory = TrashDirectory.Text,
                SequanceTime = int.Parse(SequanceTime.Text)
            };

            await Task.Run(() => _dcsRemoteControl.SetProcessingServiceSettings(processingServiceSettings));
            ConfigForm.IsEnabled = true;
        }
    }
}
