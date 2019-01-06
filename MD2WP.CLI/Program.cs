using System;
using MD2WP.Shared.BusinessLogic;
using MD2WP.Shared.Helpers;

namespace MoonspaceLabs.Shared.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // Arguments:
            //   Debug Mode (i.e. verbose logging)
            //   WordPress Site URL
            //   WordPress User Name
            //   WordPress Password
            //   VSTS Account
            //   VSTS Project Name
            //   VSTS Repo Name
            //   VSTS Branch Name
            //   VSTS Personal Access Token
            //   Mapping File
            //   Embed External Images (true/false)
            //   Publish as Committer (true/false)
            //   Process Subfolders (true/false)
            //   Use Folder Name as Category (true/false)
            //   Publish New Posts as Draft (true/false)
            //   Track Post ID in Filename (true/false)
            //   Add Edit Link (true/false)
            //   Edit Link Text
            //   Edit Link Style (CSS)

            if (args.Length > 0)
            {
                // Turn verbose logging on/off
                Logger.DebugMode = GetArgument(args, 0).Equals("true", StringComparison.InvariantCultureIgnoreCase);
            }

            if (args.Length != 20)
            {
                Logger.LogError($"Number of arguments passed in: {args.Length}");
                foreach (var arg in args)
                {
                    Logger.LogError(arg);
                }

                Logger.LogError("An invalid number of command-line arguments were provided.");

            }
            else
            {
                Logger.LogMessage("Entering MD2WP.CLI.exe.");

                try
                {
                    var client = new Md2WpClient(
                        GetArgument(args, 1),
                        GetArgument(args, 2),
                        GetArgument(args, 3),
                        GetArgument(args, 4),
                        GetArgument(args, 5),
                        GetArgument(args, 6),
                        GetArgument(args, 7),
                        GetArgument(args, 8),
                        GetArgument(args, 9),
                        GetArgument(args, 10).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 11).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 12).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 13).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 14).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 15).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 16).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 17).Equals("true", StringComparison.InvariantCultureIgnoreCase),
                        GetArgument(args, 18),
                        GetArgument(args, 19));

                    client.PublishMarkdownFiles();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                    throw;
                }

                Logger.LogMessage("Exiting MD2WP.CLI.exe.");
            }
        }

        private static string GetArgument(string[] args, int index)
        {
            // Remove beginning and ending quotes from argument
            return args[index].TrimStart('"').TrimEnd('"');
        }
    }
}