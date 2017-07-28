using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HansJuergenWeb.MessageHandlers.Settings;
using Serilog;

namespace HansJuergenWeb.MessageHandlers.Adapters
{
    public class Radapter : IRadapter
    {
        private readonly IAppSettings _appSettings;

        public Radapter(IAppSettings appSettings)
        {
            _appSettings = appSettings;

            // Verify that at the very least, R is installed in supplied location
            if(!File.Exists(appSettings.PathToR))
                throw new Exception($"R could not be found in position {appSettings.PathToR}");
        }

        public void BatchProcess(string pathToScript, Guid messageId, string uploadDir)
        {
            Log.Logger.Information("Starting R processing...");

            var pathToData = Path.Combine(uploadDir, messageId.ToString());

            var fileName = $"\"{_appSettings.PathToR}\"";
            var arguments = $"CMD BATCH --vanilla --slave \"{pathToScript}\"";

            var stringBuilder = new List<string>();
            stringBuilder.Add($"copy .\\TheScript.R {pathToData}");
            stringBuilder.Add($"cd {pathToData}");
            stringBuilder.Add($"{fileName} {arguments}");
            //stringBuilder.Add($"pause");

            var pathToCmd = Path.Combine( pathToData , "process.cmd");
            File.WriteAllLines(pathToCmd,stringBuilder);

            var process = Process.Start(pathToCmd);
            process.WaitForExit(30000);

            if(process.ExitCode==0)
            Log.Logger.Information("Ended successful R processing...");
            else
            {
                Log.Logger.Information($"Ended failed R processing : {process.ExitCode}");
            }
        }
    }
}