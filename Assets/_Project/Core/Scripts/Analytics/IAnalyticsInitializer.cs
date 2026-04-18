namespace ANut.Core.Analytics
{
    public interface IAnalyticsInitializer
    {
        void Initialize(bool dataSendingEnabled, bool isFirstLaunch);
        void SetDataSendingEnabled(bool enabled);
    }
}