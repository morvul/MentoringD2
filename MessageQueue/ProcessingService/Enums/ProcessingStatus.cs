namespace MessageQueue.ProcessingService.Enums
{
    public enum ProcessingStatus
    {
        JustStarted,
        Initialization,
        Stopped,
        FileRecieving,
        FileProcessing,
        Idle,
        PdfFileGeneration
    }
}
