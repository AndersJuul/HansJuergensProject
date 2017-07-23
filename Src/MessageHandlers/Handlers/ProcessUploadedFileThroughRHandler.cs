//using EasyNetQ;
//using EasyNetQ.AutoSubscribe;
//using HansJuergenWeb.Contracts;
//using Serilog;

//namespace HansJuergenWeb.MessageHandlers.Handlers
//{
//    public class ProcessUploadedFileThroughRHandler : IConsume<FileUploadedEvent>
//    {
//        private readonly IRadapter _radapter;
//        private readonly IBus _bus;

//        public ProcessUploadedFileThroughRHandler(IRadapter radapter, IBus bus)
//        {
//            _radapter = radapter;
//            _bus = bus;
//        }

//        [AutoSubscriberConsumer(SubscriptionId = "ProcessUploadedFileThroughR")]
//        public void Consume(FileUploadedEvent message)
//        {
//            Log.Logger.Information("Message received ProcessUploadedFileThroughR");

//            _radapter.BatchProcess(@".\TheScript.R", message.Id);

//            _bus.PublishAsync(new FileProcessedEvent
//            {
//                Description = message.Description,
//                Email = message.Email,
//                FileNames = message.FileNames,
//                DataFolder = message.DataFolder
//            });
//        }
//    }
//}