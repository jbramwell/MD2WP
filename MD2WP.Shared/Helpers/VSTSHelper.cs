using MoonspaceLabs.Shared.BaseClasses;
using MoonspaceLabs.Shared.Entities;
using MoonspaceLabs.Shared.Helpers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace MD2WP.Shared.Helpers
{
    public class VstsHelper
    {
        /// <summary>
        /// Downloads a single file from VSTS and saves it to the specified location.
        /// </summary>
        /// <param name="authentication">The user's authentication credentials.</param>
        /// <param name="project">The VSTS (team) Project.</param>
        /// <param name="repo">The name of the Git Repo containing the file.</param>
        /// <param name="branch"></param>
        /// <param name="fileToDownload">The Path to the file (within the Repo) to be downloaded.</param>
        /// <returns><c>true</c> if the file is downloaded successfully; otherwise, <c>false</c>.</returns>
        public byte[] DownloadBinaryFile(BasicAuthentication authentication, string project, string repo, string branch, string fileToDownload)
        {
            var restHttpClient = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/items?api-version=1.0&versionType=branch&version={branch}&scopePath={fileToDownload}";
            var buffer = new byte[0x4000];
            byte[] data = null;

            using (var response = restHttpClient.RequestFile(authentication, url))
            {
                if (response != null)
                {

                    using (var responseStream = response.GetResponseStream())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;

                            while (responseStream != null && (read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }

                            data = ms.ToArray();
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Downloads a single file from VSTS and saves it to the specified location.
        /// </summary>
        /// <param name="authentication">The user's authentication credentials.</param>
        /// <param name="project">The VSTS (team) Project.</param>
        /// <param name="repo">The name of the Git Repo containing the file.</param>
        /// <param name="branch"></param>
        /// <param name="fileToDownload">The Path to the file (within the Repo) to be downloaded.</param>
        public string GetFileContents(BasicAuthentication authentication, string project, string repo, string branch, string fileToDownload)
        {
            Logger.LogMessage($"Retrieving file contents for '{fileToDownload}'");

            var fileContents = string.Empty;
            var restHttpClient = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/items?api-version=1.0&versionType=branch&version={branch}&scopePath={fileToDownload}";

            using (var response = restHttpClient.RequestFile(authentication, url))
            {
                if (response != null)
                {
                    fileContents = GetResponseText(response);
                }
            }

            return fileContents;
        }

        /// <summary>
        /// Returns the object ID for the specified branch.
        /// </summary>
        /// <param name="authentication"></param>
        /// <param name="project"></param>
        /// <param name="repo"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public string GetBranchObjectId(BasicAuthentication authentication, string project, string repo, string branch)
        {
            var restHttpClient = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/refs/heads/{branch}?api-version=1.0";
            var objectId = string.Empty;

            using (var response = restHttpClient.GetAsync<CollectionResult<Ref>>(authentication, url))
            {
                if (response.Result.Value.Count > 0)
                {
                    foreach (var branchItem in response.Result.Value)
                    {
                        if (branchItem.name.Equals($"refs/heads/{branch}", System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            objectId = branchItem.objectId;
                            break;
                        }
                    }
                }
            }

            return objectId;
        }

        public async void SaveTextFile(BasicAuthentication authentication, string project, string repo, string branch, string filename, string contents, string commitComment, bool isNewFile)
        {
            var client = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/pushes?api-version=2.0-preview&versionType=branch&version={branch}&scopePath={filename}";
            var filePush = new Push();
            var filePushRefUpdates = new RefUpdate
            {
                name = $"refs/heads/{branch}",
                oldObjectId = GetBranchObjectId(authentication, project, repo, branch)
            };


            filePush.refUpdates.Add(filePushRefUpdates);

            var commit = new Commit();
            var change = new Change
            {
                changeType = isNewFile ? "add" : "edit",
                item = { path = filename },
                newContent = new NewContent
                {
                    content = contents,
                    contentType = "rawtext"
                }
            };

            commit.changes.Add(change);

            commit.comment = commitComment;

            filePush.commits.Add(commit);

            await client.PostAsync<object>(authentication, url, filePush);
        }

        public async void RenameFile(BasicAuthentication authentication, string project, string repo, string branch, string oldFilename, string newFilename, string commitComment)
        {
            var client = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/pushes?api-version=2.0-preview&versionType=branch&version={branch}";
            var filePush = new Push();
            var filePushRefUpdates = new RefUpdate
            {
                name = $"refs/heads/{branch}",
                oldObjectId = GetBranchObjectId(authentication, project, repo, branch)
            };

            filePush.refUpdates.Add(filePushRefUpdates);

            var commit = new Commit();
            var change = new Change
            {
                changeType = "rename",
                sourceServerItem = oldFilename,
                item = { path = newFilename }
            };

            commit.changes.Add(change);

            commit.comment = commitComment;

            filePush.commits.Add(commit);

            await client.PostAsync<object>(authentication, url, filePush);
        }

        public async void DeleteFile(BasicAuthentication authentication, string project, string repo, string branch, string filename, string commitComment)
        {
            var client = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/pushes?api-version=2.0-preview&versionType=branch&version={branch}";
            var filePush = new Push();
            var filePushRefUpdates = new RefUpdate
            {
                name = $"refs/heads/{branch}",
                oldObjectId = GetBranchObjectId(authentication, project, repo, branch)
            };

            filePush.refUpdates.Add(filePushRefUpdates);

            var commit = new Commit();
            var change = new Change
            {
                changeType = "delete",
                item = { path = filename }
            };

            commit.changes.Add(change);

            commit.comment = commitComment;

            filePush.commits.Add(commit);

            await client.PostAsync<object>(authentication, url, filePush);
        }

        /// <summary>
        /// Obtains the String results from an HttpWebResponse.
        /// </summary>
        /// <param name="response">The HttpWebResponse to obtain the text from.</param>
        /// <returns>A String from the HttpWebResponse that can be read.</returns>
        private static string GetResponseText(HttpWebResponse response)
        {
            var responseText = string.Empty;

            using (var responseStream = response.GetResponseStream())
            {
                var streamToRead = responseStream;

                if (streamToRead != null)
                {
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }

                    using (var streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        responseText = streamReader.ReadToEnd();

                        streamToRead.Close();
                    }
                }
            }

            return responseText;
        }

        public List<ItemDescriptor> GetFileList(BasicAuthentication authentication, string project, string repo, string branch, string rootPath, string fileExtension, bool fullRecursion)
        {
            var restHttpClient = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/items?api-version=1.0&recursionLevel={(fullRecursion ? "Full" : "1")}&versionType=branch&version={branch}&scopePath={rootPath}";
            var itemList = new List<ItemDescriptor>();

            using (var response = restHttpClient.GetAsync<CollectionResult<ItemDescriptor>>(authentication, url))
            {
                if (response.Result.Value.Count > 0)
                {
                    foreach (var itemDescriptor in response.Result.Value)
                    {
                        if (!itemDescriptor.IsFolder && itemDescriptor.Path.EndsWith(fileExtension, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            itemList.Add(itemDescriptor);
                        }
                    }
                }
            }

            return itemList;
        }

        public CommitTree GetCommitInfo(BasicAuthentication authentication, string project, string repo, string branch, string commitId)
        {
            Logger.LogMessage($"Retrieving commit information for '{commitId}'");

            var restHttpClient = new RestHttpClient();
            var url = $"{authentication.AccountUrl}/{project}/_apis/git/repositories/{repo}/commits/{commitId}?api-version=1.0&versionType=branch&version={branch}";
            CommitTree results;

            using (var response = restHttpClient.GetAsync<CommitTree>(authentication, url))
            {
                results = response.Result;
            }

            return results;
        }
    }
}