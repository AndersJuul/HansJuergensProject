using System;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using DbUp;

namespace Migrations
{
    class Program
    {
        static int Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings["FlowCytoConnection"];

            var upgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();
            Debug.WriteLine(upgradeEngine);

            var result = upgradeEngine.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return 1;
            }

            return 0;
        }
    }
}
