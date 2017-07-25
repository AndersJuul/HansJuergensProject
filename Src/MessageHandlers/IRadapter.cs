using System;

namespace HansJuergenWeb.MessageHandlers
{
    public interface IRadapter
    {
        void BatchProcess(string pathToScript, Guid messageId, string uploadDir);
    }
}