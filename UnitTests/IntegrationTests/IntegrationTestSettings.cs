using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnitTests.IntegrationTests
{
    internal class AzureDevOps
    {
        public string AccountUrl { get; set; }
        public string PersonalAccessToken { get; set; }
        public string RepoName { get; set; }
        public string ProjectName { get; set; }
        public string BranchName { get; set; }
    }

    internal class WordPress
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    internal class IntegrationTestSettings
    {
        public AzureDevOps AzureDevOps { get; set; }
        public WordPress WordPress { get; set; }

        public static IntegrationTestSettings GetIntegrationTestSettings()
        {
            // If the specific settings file does not exist, copy over the default as a starting point
            if (!File.Exists(@"IntegrationTests\IntegrationTestSettings.json"))
            {
                File.Copy(@"IntegrationTests\IntegrationTestSettings.Default.json", @"IntegrationTests\IntegrationTestSettings.json");
            }

            var settings = JsonConvert.DeserializeObject<IntegrationTestSettings>(
                File.ReadAllText(@"IntegrationTests\IntegrationTestSettings.json"));

            return settings;
        }
    }
}