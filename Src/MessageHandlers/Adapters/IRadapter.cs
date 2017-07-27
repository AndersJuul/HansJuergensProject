using System;

namespace HansJuergenWeb.MessageHandlers.Adapters
{
    public interface IRadapter
    {
        void BatchProcess(string pathToScript, Guid messageId, string uploadDir);
    }
}