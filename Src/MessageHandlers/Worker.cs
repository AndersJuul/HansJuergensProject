using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.MessageVersioning;
using HansJuergenWeb.Contracts;
using MessageHandlers;
using RabbitMQ.Client;
using Serilog;

namespace HansJuergenWeb.MessageHandlers
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private BackgroundWorker _backgroundWorker;
        private IModel _model;
        private IBus _bus;

        public Worker(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
                SetupSubscriptions();
                //_backgroundWorker = new BackgroundWorker
                //{
                //    WorkerSupportsCancellation = true
                //};
                //_backgroundWorker.DoWork += _backgroundWorker_DoWork;
                //_backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
                //_backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        //private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    WorkDone = true;
        //}

        //private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    Log.Logger.Information("Doing work");

        //    try
        //    {
        //        DoWorkInternal(sender);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "During do work", new object[0]);
        //        throw;
        //    }
        //}

        private void SetupSubscriptions()
        {
            _bus = RabbitHutch.CreateBus("host=ajf-elastic-01;username=anders;password=21Bananer;timeout=30");

            _bus.SubscribeAsync<FileUploadedEvent>("Queue_Identifier",
                message => Task.Factory.StartNew(() => { Log.Logger.Information("Message received"); }).ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        // Everything worked out ok
                        Log.Logger.Information("Everything worked out ok");
                    }
                    else
                    {
                        // Dont catch this, it is caught further up the heirarchy and results in being sent to the default error queue
                        // on the broker
                        Log.Logger.Information("Message exception");

                        throw new EasyNetQException("Message processing exception - look in the default error queue (broker)");
                    }
                }));
        }

        //private void DoWorkInternal(object sender)
        //{
        //    WorkDone = false;

        //    var lastSend = DateTime.MinValue;

        //    while (true)
        //    {
        //        var backgroundWorker = sender as BackgroundWorker;
        //        if (backgroundWorker == null || backgroundWorker.CancellationPending)
        //        {
        //            Log.Information("backgroundworker.CancellationPending: {@backgroundWorker}", backgroundWorker);
        //            return;
        //        }

        //        Thread.Sleep(1 * 1000);
        //    }
        //}



        public void Stop()
        {
            //if (_backgroundWorker != null)
            //{
            //    _backgroundWorker.CancelAsync();

            //    while (!WorkDone)
            //    {
            //        Thread.Sleep(500);
            //    }
            //}
        }
    }
}