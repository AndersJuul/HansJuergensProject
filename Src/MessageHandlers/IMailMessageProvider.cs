namespace HansJuergenWeb.MessageHandlers
{
    public interface IMailMessageProvider
    {
        string GetTemplateBasedMailBody(string templatePath, string messageDataFolder, string searchPattern);
    }
}