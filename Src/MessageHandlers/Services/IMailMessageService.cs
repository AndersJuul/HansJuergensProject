namespace HansJuergenWeb.MessageHandlers.Services
{
    public interface IMailMessageService
    {
        string GetTemplateBasedMailBody(string templatePath, string messageDataFolder, string searchPattern, string messageEmail);
    }
}