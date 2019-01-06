using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonspaceLabs.Shared.BaseClasses;
using MoonspaceLabs.Shared.Helpers;
using System;
using System.Linq;
using MD2WP.Shared.Helpers;
using UnitTests.IntegrationTests;

namespace UnitTests
{
    [TestClass]
    public class VstsHelperTests
    {
        private readonly VstsHelper _vstsHelper;
        private readonly BasicAuthentication _authentication;
        private readonly string _accountUrl;
        private readonly string _accessToken;
        private readonly string _repoName;
        private readonly string _projectName;
        private readonly string _branchName;

        public VstsHelperTests()
        {
            var settings = IntegrationTestSettings.GetIntegrationTestSettings();

            _accountUrl = settings.AzureDevOps.AccountUrl;
            _accessToken = settings.AzureDevOps.PersonalAccessToken;
            _repoName = settings.AzureDevOps.RepoName;
            _projectName = settings.AzureDevOps.ProjectName;
            _branchName = settings.AzureDevOps.BranchName;

            _authentication = new BasicAuthentication(_accountUrl, string.Empty, _accessToken);
            _vstsHelper = new VstsHelper();
        }

        #region Helper Methods

        private void DeleteTestFile(string filename)
        {
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, filename, "Deleting integration test file.");
        }

        #endregion

        [TestMethod, TestCategory("VSTS Git File APIs"), TestCategory("Integration Tests")]
        public void CreateFileTest()
        {
            var filename = Guid.NewGuid() + ".txt";
            var expectedContents = "Hello, World!";

            // Create file
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, filename, expectedContents,
                "Creating integration test file. ***NO_CI***", true);

            // Pull file contents (assuming it was created)
            var actualContents = _vstsHelper.GetFileContents(_authentication, _projectName, _repoName, _branchName, filename);

            // Compare contents
            Assert.AreEqual(expectedContents, actualContents);

            // Delete test file
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, filename, "Deleting integration test file. ***NO_CI***");
        }

        [TestMethod, TestCategory("VSTS Git File APIs"), TestCategory("Integration Tests")]
        public void UpdateFileTest()
        {
            var filename = Guid.NewGuid() + ".txt";
            var expectedContents = "Hello, World!";

            // Create file
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, filename, expectedContents,
                "Creating integration test file. ***NO_CI***", true);

            // Update file
            expectedContents = "Now this is new!";
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, filename, expectedContents,
                "Updating integration test file. ***NO_CI***", false);

            // Pull file contents (assuming it was created)
            var actualContents = _vstsHelper.GetFileContents(_authentication, _projectName, _repoName, _branchName, filename);

            // Compare contents
            Assert.AreEqual(expectedContents, actualContents);

            // Delete test file
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, filename, "Deleting integration test file. ***NO_CI***");
        }

        [TestMethod, TestCategory("VSTS Git File APIs"), TestCategory("Integration Tests")]
        public void RenameFileTest()
        {
            var originalFilename = Guid.NewGuid() + ".txt";
            var newFilename = Guid.NewGuid() + ".md";
            var expectedContents = "Hello, World!";

            // Create file
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, originalFilename, expectedContents,
                "Creating integration test file. ***NO_CI***", true);

            // Rename file
            _vstsHelper.RenameFile(_authentication, _projectName, _repoName, _branchName, originalFilename, newFilename,
                "Renaming integration test file. ***NO_CI***");

            // Pull file contents of renamed file (assuming it was renamed)
            var actualContents = _vstsHelper.GetFileContents(_authentication, _projectName, _repoName, _branchName, newFilename);

            // Compare contents
            Assert.AreEqual(expectedContents, actualContents);

            // Delete test file
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, newFilename, "Deleting integration test file. ***NO_CI***");
        }

        [TestMethod, TestCategory("VSTS Git File APIs"), TestCategory("Integration Tests")]
        public void DeleteFileTest()
        {
            var filename = Guid.NewGuid() + ".txt";
            var expectedContents = "Hello, World!";

            // Create file
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, filename, expectedContents,
                "Creating integration test file. ***NO_CI***", true);

            // Delete test file
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, filename, "Deleting integration test file. ***NO_CI***");

            // Pull file contents of renamed file (assuming it was renamed)
            var actualContents = _vstsHelper.GetFileContents(_authentication, _projectName, _repoName, _branchName, filename);

            // Compare contents
            Assert.IsTrue(actualContents.Contains("GitItemNotFoundException"));
        }

        [TestMethod, TestCategory("VSTS Git File APIs"), TestCategory("Integration Tests")]
        public void GetFileListTest()
        {
            var filename = Guid.NewGuid() + ".filelisttest";
            var expectedContents = "Hello, World!";

            // Create file
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, filename, expectedContents,
                "Creating integration test file. ***NO_CI***", true);

            // Get list of files
            var fileList = _vstsHelper.GetFileList(_authentication, _projectName, _repoName, _branchName, "/", ".filelisttest", true);

            // Delete test file
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, filename, "Deleting integration test file. ***NO_CI***");

            // Compare contents
            Assert.IsTrue(fileList.FirstOrDefault(x => x.Path.Equals("/" + filename)) != null);
        }

        [TestMethod, TestCategory("VSTS Git File APIs"), TestCategory("Integration Tests")]
        public void GetCommitInfoTest()
        {
            var filename = Guid.NewGuid() + ".filelisttest";
            var expectedContents = "Hello, World!";

            // Create file
            _vstsHelper.SaveTextFile(_authentication, _projectName, _repoName, _branchName, filename, expectedContents,
                "Creating integration test file. ***NO_CI***", true);

            // Get list of files
            var fileList = _vstsHelper.GetFileList(_authentication, _projectName, _repoName, _branchName, "/", ".filelisttest", true);

            if (fileList != null && fileList.Count > 0)
            {
                // Get Commit Info
                var commitInfo = _vstsHelper.GetCommitInfo(_authentication, _projectName, _repoName, _branchName, fileList[0].CommitId);

                Assert.AreEqual(fileList[0].CommitId, commitInfo.commitId);
            }

            // Delete test file
            _vstsHelper.DeleteFile(_authentication, _projectName, _repoName, _branchName, filename, "Deleting integration test file. ***NO_CI***");
        }
    }
}