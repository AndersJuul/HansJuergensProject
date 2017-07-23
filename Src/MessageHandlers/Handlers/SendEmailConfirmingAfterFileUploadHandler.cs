//using Ajf.Nuget.Logging;
//using EasyNetQ.AutoSubscribe;
//using HansJuergenWeb.Contracts;
//using Serilog;

//namespace HansJuergenWeb.MessageHandlers.Handlers
//{
//    public class SendEmailConfirmingAfterFileUploadHandler : IConsume<FileUploadedEvent>
//    {
//        private readonly IMailSender _mailSender;

//        public SendEmailConfirmingAfterFileUploadHandler(IMailSender mailSender)
//        {
//            _mailSender = mailSender;
//        }
//        [AutoSubscriberConsumer(SubscriptionId = "SendEmailConfirmingUpload")]
//        public void Consume(FileUploadedEvent fileUploadedEvent)
//        {
//            Log.Logger.Information("Message received in SendEmailConfirmingUpload: {@fileUploadedEvent}", fileUploadedEvent);

//            var ajf = "andersjuulsfirma@gmail.com";
//            _mailSender.SendMailAsync(ajf, null, ajf, "Notification: Files uploaded", "")
//                .Wait();
//        }
//    }
//}