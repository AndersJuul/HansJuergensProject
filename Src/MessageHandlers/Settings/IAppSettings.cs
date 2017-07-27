namespace HansJuergenWeb.MessageHandlers.Settings
{
    public interface IAppSettings
    {
        string EasyNetQConfig { get; set; }
        string CcAddress { get; set; }
        string SenderAddress { get; set; }
        string SubjectConfirmation { get; set; }
        string SubjectResults { get; set; }
        string UploadDir { get; set; }
        string FlowCytoConnection { get; set; }
        string PathToR { get; set; }
    }
}