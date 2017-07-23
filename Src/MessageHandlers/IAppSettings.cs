namespace HansJuergenWeb.MessageHandlers
{
    public interface IAppSettings
    {
        string EasyNetQConfig { get; set; }
        string CcAddress { get; set; }
        string SenderAddress { get; set; }
        string Subject { get; set; }
    }
}