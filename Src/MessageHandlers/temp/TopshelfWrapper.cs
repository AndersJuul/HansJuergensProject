//using System;
//using Ajf.Nuget.Logging;
//using Serilog;
//using Topshelf;
//using Topshelf.ServiceConfigurators;

//namespace HansJuergenWeb.MessageHandlers.temp
//{
//    public abstract class BaseWorker
//    {
//        public abstract void Start();
//        public abstract void Stop();
//    }
//    public class TopshelfWrapper<T> : IDisposable where T:BaseWorker
//    {
//        private readonly Action _setupAction;
//        private readonly Action<ServiceConfigurator<T>> _runAction;

//        public TopshelfWrapper(Action setupAction, Action<ServiceConfigurator<T>> runAction)
//        {
//            _setupAction = setupAction;
//            _runAction = runAction;
//        }

//        public void Dispose()
//        {
//        }

//        public void Run()
//        {
//            _setupAction();

//            try
//            {
//                var serviceSettingsFromConfigFile = new ServiceSettingsFromConfigFile();

//                HostFactory.Run(x => //1
//                {
//                    x.Service<T>(s => //2
//                    {
//                        try
//                        {
//                            _runAction(s);
//                            s.WhenStarted(tc =>
//                            {
//                                Log.Logger.Information("Starting service");
//                                tc.Start();
//                                Log.Logger.Information("Service started.");
//                            }); //4
//                            s.WhenStopped(tc =>
//                            {
//                                Log.Logger.Information("Stopping service.");
//                                tc.Stop();
//                                Log.Logger.Information("Service stopped.");
//                            }); //5
//                            s.WhenPaused(tc =>
//                            {
//                                Log.Logger.Information("Pausing service.");
//                                tc.Stop();
//                                Log.Logger.Information("Service paused.");
//                            }); //5

//                            s.WhenContinued(tc =>
//                            {
//                                Log.Logger.Information("Continuing service.");
//                                tc.Start();
//                                Log.Logger.Information("Service continued.");
//                            }); //5
//                            s.WhenSessionChanged((w, sca) =>
//                            {
//                                Log.Logger.Information("Session changed: " + w);
//                                Log.Logger.Information("Session changed: " + sca);
//                            });
//                        }
//                        catch (Exception ex)
//                        {
//                            Log.Error(ex, "Fra program");
//                            throw;
//                        }
//                    });
//                    x.RunAs(serviceSettingsFromConfigFile.RunAsUserName, serviceSettingsFromConfigFile.RunAsPassword); //6

//                    x.SetDescription(serviceSettingsFromConfigFile.Description); //7
//                    x.SetDisplayName(serviceSettingsFromConfigFile.DisplayName); //8
//                    x.SetServiceName(serviceSettingsFromConfigFile.ServiceName); //9
//                }); //10        }
//            }
//            catch (Exception ex)
//            {
//                Log.Error(ex, "Yderste");
//                throw;
//            }
//        }
//    }
//}