using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ajf.Nuget.Logging;
using HansJuergenWeb.MessageHandlers;
using Serilog;
using Topshelf;

namespace MessageHandlers
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = StandardLoggerConfigurator.GetEnrichedLogger();

            try
            {
                var appSettings = new AppSettings();

                HostFactory.Run(x => //1
                {
                    x.Service<Worker>(s => //2
                    {
                        try
                        {
                            s.ConstructUsing(name => new Worker(appSettings)); //3
                            s.WhenStarted(tc =>
                            {
                                Log.Logger.Information("Starting service");
                                tc.Start();
                                Log.Logger.Information("Service started.");
                            }); //4
                            s.WhenStopped(tc =>
                            {
                                Log.Logger.Information("Stopping service.");
                                tc.Stop();
                                Log.Logger.Information("Service stopped.");
                            }); //5
                            s.WhenPaused(tc =>
                            {
                                Log.Logger.Information("Pausing service.");
                                tc.Stop();
                                Log.Logger.Information("Service paused.");
                            }); //5

                            s.WhenContinued(tc =>
                            {
                                Log.Logger.Information("Continuing service.");
                                tc.Start();
                                Log.Logger.Information("Service continued.");
                            }); //5
                            s.WhenSessionChanged((w, sca) =>
                            {
                                Log.Logger.Information("Session changed: " + w);
                                Log.Logger.Information("Session changed: " + sca);
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Fra program");
                            throw;
                        }
                    });
                    x.RunAs(appSettings.RunAsUserName,appSettings.RunAsPassword); //6

                    x.SetDescription(appSettings.Description); //7
                    x.SetDisplayName(appSettings.DisplayName); //8
                    x.SetServiceName(appSettings.ServiceName); //9
                }); //10        }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Yderste");
                throw;
            }
        }
    }
}
//_bus.SubscribeAsync<MessageType>("Queue_Identifier",
//message => Task.Factory.StartNew(() =>
//{
//// Perform some actions here
//// If there is a exception it will result in a task complete but task faulted which
//// is dealt with below in the continuation
//}).ContinueWith(task =>
//{
//if (task.IsCompleted && !task.IsFaulted)
//{
//// Everything worked out ok
//}
//else
//{
//// Dont catch this, it is caught further up the heirarchy and results in being sent to the default error queue
//// on the broker
//throw new EasyNetQException("Message processing exception - look in the default error queue (broker)");
//}
//}));