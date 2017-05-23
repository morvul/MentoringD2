using System;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MessageQueue.RemoteController.Enums;
using MessageQueue.RemoteController.Models;

namespace MessageQueue.RemoteController
{
    public class DcsRemoteControl
    {
        private readonly string _remoteControlQueueName;
        private readonly int _callDelay;
        private readonly CancellationTokenSource _processingServiceCancTokenSource;
        private Func<ProcessingServiceData> _processingServiceDataProvider;

        public DcsRemoteControl(string remoteControlQueueName, int callDelay)
        {
            _remoteControlQueueName = remoteControlQueueName;
            _callDelay = callDelay;
            _processingServiceCancTokenSource = new CancellationTokenSource();
        }

        public event Action<ProcessingServiceSettings> OnProcessingServiceSettingsChanged;

        public ProcessingServiceData GetProcessingServiceData()
        {
            if (!System.Messaging.MessageQueue.Exists(_remoteControlQueueName))
            {
                System.Messaging.MessageQueue.Create(_remoteControlQueueName);
            }

            using (var remoteControlQueue = new System.Messaging.MessageQueue(_remoteControlQueueName, QueueAccessMode.SendAndReceive))
            {
                remoteControlQueue.Formatter = new XmlMessageFormatter(new[] { typeof(ProcessingServiceData), typeof(RemoteControlCommand) });
                var processingServiceDataRequest = new RemoteControlCommand
                {
                    Code = RemoteControlCommandCode.GetProcessingServiceData
                };
                remoteControlQueue.Send(new Message(processingServiceDataRequest));
                do
                {
                    var processingServiceMessage = remoteControlQueue.GetAllMessages().FirstOrDefault();
                    var processingServiceData = processingServiceMessage?.Body as ProcessingServiceData;
                    if (processingServiceData == null)
                    {
                        Thread.Sleep(_callDelay);
                        continue;
                    }

                    remoteControlQueue.Receive();
                    return processingServiceData;
                } while (true);
            }
        }

        public void SetProcessingServiceSettings(ProcessingServiceSettings processingServiceSettings)
        {
            using (var remoteControlQueue = new System.Messaging.MessageQueue(_remoteControlQueueName, QueueAccessMode.SendAndReceive))
            {
                remoteControlQueue.Formatter = new XmlMessageFormatter(new[] { typeof(RemoteControlResult), typeof(ProcessingServiceSettingsCommand) });
                var processingServiceDataRequest = new ProcessingServiceSettingsCommand
                {
                    Settings = processingServiceSettings
                };
                remoteControlQueue.Send(new Message(processingServiceDataRequest));
                do
                {
                    var processingServiceMessage = remoteControlQueue.GetAllMessages().FirstOrDefault();
                    var result = processingServiceMessage?.Body as RemoteControlResult;
                    if (result == null)
                    {
                        Thread.Sleep(_callDelay);
                        continue;
                    }

                    if (result.Code == RemoteControlResultCode.Error)
                    {
                        MessageBox.Show($"Settings weren't applied...\n\"{result.Message}\"", "MSMQ communication",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    if (result.Code == RemoteControlResultCode.Success)
                    {
                        MessageBox.Show("Settings were successfully applied!");
                    }

                    remoteControlQueue.Receive();
                    return;
                } while (true);
            }
        }

        private void ProcessingServiceCommandsMonitor(CancellationToken cancToken)
        {
            using (var remoteControlQueue = new System.Messaging.MessageQueue(_remoteControlQueueName, QueueAccessMode.SendAndReceive))
            {
                remoteControlQueue.Formatter = new XmlMessageFormatter(new[] { typeof(ProcessingServiceSettings), typeof(RemoteControlCommand), typeof(ProcessingServiceSettingsCommand),
                    typeof(ProcessingServiceData), typeof(RemoteControlResult) });
                do
                {
                    var processingServiceMessage = remoteControlQueue.GetAllMessages().FirstOrDefault();
                    var settingsCommand = processingServiceMessage?.Body as ProcessingServiceSettingsCommand;
                    if (settingsCommand != null)
                    {
                        remoteControlQueue.Receive();
                        var settings = settingsCommand.Settings;
                        var result = new RemoteControlResult();
                        try
                        {
                            OnProcessingServiceSettingsChanged?.Invoke(settings);
                            result.Code = RemoteControlResultCode.Success;
                        }
                        catch (Exception e)
                        {
                            result.Code = RemoteControlResultCode.Error;
                            result.Message = e.Message;
                        }
                        
                        remoteControlQueue.Send(new Message(result));
                        continue;
                    }

                    var command = processingServiceMessage?.Body as RemoteControlCommand;
                    if (command != null)
                    {
                        remoteControlQueue.Receive();
                        var data = _processingServiceDataProvider();
                        remoteControlQueue.Send(new Message(data));
                        continue;
                    }

                    Thread.Sleep(_callDelay);
                } while (!cancToken.IsCancellationRequested);
            }
        }

        public void StartProcessingServicesettingsMonitor(Func<ProcessingServiceData> processingServiceDataProvider)
        {
            _processingServiceDataProvider = processingServiceDataProvider;
            var cancToken = _processingServiceCancTokenSource.Token;
            Task.Run(() => ProcessingServiceCommandsMonitor(cancToken), cancToken);
        }


        public void StopProcessingServicesettingsMonitor()
        {
            _processingServiceCancTokenSource.Cancel();
        }
    }
}
