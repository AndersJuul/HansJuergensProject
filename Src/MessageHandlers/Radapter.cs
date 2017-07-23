using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace HansJuergenWeb.MessageHandlers
{
    public class Radapter : IRadapter
    {
        public void BatchProcess(string pathToScript, Guid messageId)
        {
            Log.Logger.Information("Starting R processing...");

            var uploadDir = @"c:\temp\hjuploads\";
            var pathToData = Path.Combine(uploadDir, messageId.ToString());

            var pathToR = @"C:\Program Files\R\R-3.4.1\bin\R.exe";
            var fileName = $"\"{pathToR}\"";
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

            Log.Logger.Information("Ended R processing...");
        }
    }
}