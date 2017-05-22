using System.Linq;
using System.Messaging;
using System.Threading;
using System.Windows;
using MessageQueue.RemoteController.Enums;
using MessageQueue.RemoteController.Models;

namespace MessageQueue.RemoteController
{
    public class DcsRemoteControl
    {
        private readonly string _remoteControlQueueName;
        private readonly int _callDelay;

        public DcsRemoteControl(string remoteControlQueueName, int callDelay)
        {
            _remoteControlQueueName = remoteControlQueueName;
            _callDelay = callDelay;
        }

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
                    if (processingServiceData != null)
                    {
                        remoteControlQueue.Receive();
                        return processingServiceData;
                    }

                    Thread.Sleep(_callDelay);
                } while (true);
            }
        }

        public void SetProcessingServiceSettings(ProcessingServiceSettings processingServiceSettings)
        {
            using (
                var remoteControlQueue = new System.Messaging.MessageQueue(_remoteControlQueueName,
                    QueueAccessMode.SendAndReceive))
            {
                var processingServiceDataRequest = new RemoteControlCommand
                {
                    Code = RemoteControlCommandCode.SetProcessingServiceSettings,
                    Data = processingServiceSettings
                };
                remoteControlQueue.Send(new Message(processingServiceDataRequest));
                var processingServiceMessage = remoteControlQueue.Receive();
                var result = processingServiceMessage?.Body as RemoteControlResult;
                if (result == null)
                {
                    MessageBox.Show("Wrong data recieved!");
                    return;
                }

                if (result.Code == RemoteControlResultCode.Error)
                {
                    MessageBox.Show($"Settings weren't applied...\n\"{result.Message}\"", "MSMQ communication",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (result.Code == RemoteControlResultCode.Success)
                {
                    MessageBox.Show("Settings were successfully applied!");
                }
            }
        }
    }
}
