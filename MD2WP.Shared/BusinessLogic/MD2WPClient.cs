using HtmlAgilityPack;
using Markdig;
using MD2WP.Shared.Helpers;
using MoonspaceLabs.Shared.BaseClasses;
using MoonspaceLabs.Shared.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MD2WP.Shared.BusinessLogic
{
    public class Md2WpClient
    {
        #region Private Attributes

        private readonly BasicAuthentication vstsAuthentication;
        private readonly VstsHelper vstsHelper;
        private readonly WordPressHelper wpHelper;
        private List<ItemDescriptor> markdownFileList;
        //private readonly WordPressSiteConfig siteConfig;
        private List<DocumentMetadata> metadata;
        private bool metadataFileCreated;
        //private string accountUrl;
        private readonly string project;
        private readonly string repoName;
        private readonly string branch;
        //private readonly string accessToken;
        private readonly string metadataFilename;
        private readonly bool embedExternalImages;
        private readonly bool publishAsCommitter;
        private readonly bool processSubfolders;
        private readonly bool useFolderNameAsCategory;
        private readonly bool useFolderNameAsTag;
        private readonly bool publishNewPostsAsDraft;
        private readonly bool trackPostIdInFilename;
        private readonly bool addEditLink;
        private readonly string editLinkText;
        private readonly string editLinkStyle;

        #endregion

        #region Constructor(s)

        public Md2WpClient(string baseUrl, string userName, string password, string accountUrl,
            string project, string repoName, string branch, string accessToken, string metadataFilename,
            bool embedExternalImages, bool publishAsCommitter, bool processSubfolders, bool useFolderNameAsCategory,
            bool useFolderNameAsTag, bool publishNewPostsAsDraft, bool trackPostIdInFilename,
            bool addEditLink, string editLinkText, string editLinkStyle)
        {
            Logger.LogMessage("MDWWPClient::ctor");
            Logger.LogMessage($"  BaseUrl = {baseUrl}");
            Logger.LogMessage($"  UserName = {userName}");
            Logger.LogMessage("  Password = ********");
            Logger.LogMessage($"  Account URL = {accountUrl}");
            Logger.LogMessage($"  Project = {project}");
            Logger.LogMessage($"  RepoName = {repoName}");
            Logger.LogMessage($"  Branch = {branch}");
            Logger.LogMessage("  AccessToken = ********");
            Logger.LogMessage($"  MetadataFilename = {metadataFilename}");
            Logger.LogMessage($"  EmbedExternalImages = {embedExternalImages}");
            Logger.LogMessage($"  PublishAsCommitter = {publishAsCommitter}");
            Logger.LogMessage($"  ProcessSubfolders = {processSubfolders}");
            Logger.LogMessage($"  UseFolderNameAsCategory = {useFolderNameAsCategory}");
            Logger.LogMessage($"  UseFolderNameAsTag = {useFolderNameAsTag}");
            Logger.LogMessage($"  PublishNewPostsAsDraft = {publishNewPostsAsDraft}");
            Logger.LogMessage($"  TrackPostIdInFilename = {trackPostIdInFilename}");
            Logger.LogMessage($"  AddEditLink = {addEditLink}");
            Logger.LogMessage($"  EditLinkText = {editLinkText}");
            Logger.LogMessage($"  editLinkStyle = {editLinkStyle}");

            vstsAuthentication = new BasicAuthentication(accountUrl, string.Empty, accessToken);
            vstsHelper = new VstsHelper();
            wpHelper = new WordPressHelper(baseUrl, userName, password);

            //siteConfig = new WordPressSiteConfig()
            //{
            //    BaseUrl = baseUrl,
            //    Username = userName,
            //    Password = password
            //};

            //this.accountUrl = accountUrl;
            this.project = project;
            this.repoName = repoName;
            this.branch = branch;
            //this.accessToken = accessToken;
            this.metadataFilename = metadataFilename.TrimStart('\\');
            this.embedExternalImages = embedExternalImages;
            this.publishAsCommitter = publishAsCommitter;
            this.processSubfolders = processSubfolders;
            this.useFolderNameAsCategory = useFolderNameAsCategory;
            this.useFolderNameAsTag = useFolderNameAsTag;
            this.publishNewPostsAsDraft = publishNewPostsAsDraft;
            this.trackPostIdInFilename = trackPostIdInFilename;
            this.addEditLink = addEditLink;
            this.editLinkText = editLinkText;
            this.editLinkStyle = editLinkStyle;
        }

        #endregion

        #region HTML Methods

        private static byte[] GetImageFromUrl(string url)
        {
            byte[] imageBytes;

            using (var webClient = new WebClient())
            {
                imageBytes = webClient.DownloadData(url);
            }

            return imageBytes;
        }

        public string ConvertMarkdownToHtml(string markdown)
        {
            Logger.LogMessage("Converting markdown to HTML");

            // Configure the pipeline with various extensions active as defined here: https://github.com/lunet-io/markdig
            var pipeline = new MarkdownPipelineBuilder().
                UseAdvancedExtensions().
                UseDefinitionLists().
                UseFootnotes().
                UseAutoIdentifiers().
                UseMediaLinks().
                UseAbbreviations().
                UseFigures().
                Build();
            var html = Markdown.ToHtml(markdown, pipeline);

            return html;
        }

        private string ConvertHtmlToEmbeddedImages(string fileContents)
        {
            Logger.LogMessage("Converting image links to embedded image data (where applicable)");

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(fileContents);

            var htmlNavigator = htmlDocument.CreateNavigator();

            if (htmlNavigator != null)
            {
                var imageLinks = htmlNavigator.Select(".//img");

                foreach (HtmlNodeNavigator image in imageLinks)
                {
                    var imageSource = image.CurrentNode.Attributes["src"];
                    var imageUrl = imageSource.Value;
                    bool isExternal;
                    string imageExtension;

                    // Start by looking for non-relative paths. Relative paths are assumed to be "internal" URLs
                    if (imageUrl.StartsWith("http:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var imageUri = new Uri(imageUrl);

                        isExternal = !imageUri.Host.EndsWith(".visualstudio.com", StringComparison.InvariantCultureIgnoreCase);
                        imageExtension = Path.GetExtension(imageUri.AbsolutePath).TrimStart('.');
                    }
                    else
                    {
                        isExternal = false;
                        imageExtension = Path.GetExtension(imageUrl).TrimStart('.');
                    }

                    if (isExternal)
                    {
                        // We might not want to embed external images, e.g. for CDN/bandwidth reasons
                        if (embedExternalImages)
                        {
                            var imageBase64 = Convert.ToBase64String(GetImageFromUrl(imageUrl));

                            imageSource.Value = $"data:image/{imageExtension};base64,{imageBase64}";
                        }
                    }
                    else
                    {
                        // Download image from VSTS via REST API
                        var imageBase64 = Convert.ToBase64String(
                            vstsHelper.DownloadBinaryFile(vstsAuthentication, project, repoName, branch, imageUrl.TrimStart('.')));

                        imageSource.Value = $"data:image/{imageExtension};base64,{imageBase64}";
                    }
                }
            }

            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);

            htmlDocument.Save(stringWriter);

            return stringBuilder.ToString();
        }

        #endregion

        #region VSTS Methods

        private void LoadMetadata()
        {
            Logger.LogMessage("Loading metadata file...");

            var text = vstsHelper.GetFileContents(vstsAuthentication, project, repoName, branch, metadataFilename);

            if (text.Contains("GitItemNotFoundException"))
            {
                Logger.LogMessage("Metadata file does not exist so creating it");

                metadata = new List<DocumentMetadata>();

                // Track this as being created so we can perform an "add" instead of the usual "edit"
                metadataFileCreated = true;
            }
            else
            {
                metadata = JsonConvert.DeserializeObject<List<DocumentMetadata>>(text) ?? new List<DocumentMetadata>();
            }
        }

        private void ClearMetadataReconcileFlag()
        {
            if (metadata != null)
            {
                foreach (var item in metadata)
                {
                    item.IsReconciled = false;
                }
            }
        }

        private void RemoveOrphanedMetadataEntries()
        {
            Logger.LogMessage("Removing 'orphaned' entries from metadata file.");

            metadata.RemoveAll(x => !x.IsReconciled);
        }

        private DocumentMetadata GetDocumentMetadata(string filename)
        {
            Logger.LogMessage($"Get document metadata for '{filename}'");

            DocumentMetadata documentMetadata = null;
            var filenameHelper = new FilenameHelper();
            var hasId = filenameHelper.HasId(filename);
            var id = 0;

            if (hasId)
            {
                id = int.Parse(filenameHelper.GetId(filename));
            }

            foreach (var metadataEntry in metadata)
            {
                if ((hasId && id == metadataEntry.DocumentId) ||
                    (metadataEntry.DocumentFilename.Equals(filename, StringComparison.InvariantCultureIgnoreCase)))
                {
                    documentMetadata = metadataEntry;
                    break;
                }
            }

            return documentMetadata;
        }

        private string GetMarkdownFileCommitId(string filename)
        {
            Logger.LogMessage($"Get markdown file Commit ID for '{filename}'");

            var commitId = string.Empty;

            foreach (var itemDescriptor in markdownFileList)
            {
                if (itemDescriptor.Path.Equals(filename, StringComparison.InvariantCultureIgnoreCase))
                {
                    commitId = itemDescriptor.ObjectId;
                    break;
                }
            }

            return commitId;
        }

        public void SaveMetadata(string filename)
        {
            Logger.LogMessage($"Saving metadata file: '{filename}'");

            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);

            vstsHelper.SaveTextFile(vstsAuthentication, project, repoName, branch, filename, json, "Updated by Markdown to WordPress. ***NO_CI***", metadataFileCreated);
        }

        public void LoadMarkdownFileList()
        {
            Logger.LogMessage("Loading list of markdown files...");

            markdownFileList = vstsHelper.GetFileList(vstsAuthentication, project, repoName, branch, Path.GetDirectoryName(metadataFilename), ".md", processSubfolders);
        }

        public void PublishMarkdownFiles()
        {
            var publishOccurred = false;

            LoadMetadata();
            LoadMarkdownFileList();
            ClearMetadataReconcileFlag(); // So we can track "orphaned" entries (i.e. a .md file has been deleted)

            foreach (var markdownFile in markdownFileList)
            {
                // Check Commit ID to see if it's different from what was stored. If not, then nothing has
                // changed so no need to publish (again)
                var documentMetadata = GetDocumentMetadata(markdownFile.Path);
                var commitId = GetMarkdownFileCommitId(markdownFile.Path);
                var markdownFileSourceUrl = $"{vstsAuthentication.AccountUrl}/{project}/_git/{repoName}?path={markdownFile.Path}&version=GB{branch}&editMode=true";

                if (documentMetadata == null)
                {
                    Logger.LogMessage($"{markdownFile.Path} is new... creating initial metadata entry");

                    // There is no metedata defined for the markdown file being processed so
                    // let's create a default entry (that will be published as a Draft)
                    documentMetadata = new DocumentMetadata(markdownFile.Path);
                    metadata.Add(documentMetadata);
                }

                // Track as having a matching .md file
                documentMetadata.IsReconciled = true;

                // Is the file new or has it been updated and/or renamed?
                if ((documentMetadata.LastObjectId == null) ||
                    (!documentMetadata.LastObjectId.Equals(commitId, StringComparison.InvariantCultureIgnoreCase)) ||
                    (!documentMetadata.DocumentFilename.Equals(markdownFile.Path, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Logger.LogMessage($"{documentMetadata.DocumentFilename} is new or has been updated so continue with publish");

                    var filenameHelper = new FilenameHelper();

                    // The Commit IDs are different so the file is either new or it has been updated so let's publish!
                    var fileContents = vstsHelper.GetFileContents(vstsAuthentication, project, repoName, branch, markdownFile.Path);
                    var commitInfo = vstsHelper.GetCommitInfo(vstsAuthentication, project, repoName, branch, markdownFile.CommitId);

                    if (commitInfo != null)
                    {
                        // Track author information
                        documentMetadata.AuthorName = commitInfo.author.name;
                        documentMetadata.AuthorEmail = commitInfo.author.email;
                        documentMetadata.AuthorDate = commitInfo.author.date;
                    }

                    if (fileContents == null)
                    {
                        Logger.LogError($"##ERROR: The contents of the file '{documentMetadata.DocumentFilename}' could not be loaded.");

                        throw new InvalidOperationException($"##ERROR: The contents of the file '{documentMetadata.DocumentFilename}' could not be loaded.");
                    }

                    fileContents = ConvertMarkdownToHtml(fileContents);

                    // Add an "Edit this page" link if it is requested
                    if (addEditLink)
                    {
                        fileContents += $"<br/><br/><a style=\"{editLinkStyle}\" href=\"{markdownFileSourceUrl}\">{editLinkText}</a>";
                    }

                    // Convert image links to embedded images
                    fileContents = ConvertHtmlToEmbeddedImages(fileContents);

                    // Set the post title because it's possible the file has been renamed
                    documentMetadata.PostTitle = Path.GetFileNameWithoutExtension(filenameHelper.GetFilenameWithoutId(markdownFile.Path));

                    if (useFolderNameAsCategory)
                    {
                        var folderPath = Path.GetDirectoryName(documentMetadata.DocumentFilename);

                        // If the path is empty or contains only a leading backslash '\' then we'll skip this step (i.e. doesn't apply to root folders)
                        if (folderPath.Length > 1)
                        {
                            var folderName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);

                            if (documentMetadata.Categories.IndexOf(folderName) < 0)
                            {
                                Logger.LogMessage($"Adding Category '{folderName}' to {documentMetadata.DocumentFilename}");

                                documentMetadata.Categories.Add(folderName);
                            }
                        }
                    }

                    if (useFolderNameAsTag)
                    {
                        var folderPath = Path.GetDirectoryName(documentMetadata.DocumentFilename);

                        // If the path is empty or contains only a leading backslash '\' then we'll skip this step (i.e. doesn't apply to root folders)
                        if (folderPath.Length > 1)
                        {
                            var folderName = folderPath.Substring(folderPath.LastIndexOf('\\') + 1);

                            if (documentMetadata.Tags.IndexOf(folderName) < 0)
                            {
                                Logger.LogMessage($"Adding Tag '{folderName}' to {documentMetadata.DocumentFilename}");

                                documentMetadata.Tags.Add(folderName);
                            }
                        }
                    }

                    // Create the actual post/page
                    var postStatus = wpHelper.CreatePost(
                        documentMetadata.PostTitle,
                        fileContents,
                        documentMetadata.DocumentId,
                        documentMetadata.DocumentType,
                        documentMetadata.Categories,
                        documentMetadata.Tags,
                        !documentMetadata.IsPublic,
                        publishNewPostsAsDraft,
                        documentMetadata.AuthorName,
                        documentMetadata.AuthorEmail,
                        publishAsCommitter);

                    documentMetadata.DocumentId = postStatus.Id;
                    documentMetadata.IsPublic = !postStatus.IsDraft; // Track draft/public
                    documentMetadata.LastObjectId = commitId;

                    publishOccurred = true;

                    if (trackPostIdInFilename)
                    {
                        documentMetadata.DocumentFilename =
                            UpdateFilenameWithPostId(markdownFile.Path, documentMetadata.DocumentId);
                    }
                }
                else
                {
                    Logger.LogMessage($"The markdown file '{markdownFileSourceUrl}' has not been updated since it was last published. Skipping this file.");
                    //Logger.LogMessage($"The markdown file '{markdownFile.Path}' has not been updated since it was last published. Skipping this file.");
                }
            }

            // No point saving anything if nothing was changed/published
            if (publishOccurred || !metadata.TrueForAll(x => x.IsReconciled))
            {
                RemoveOrphanedMetadataEntries();
                SaveMetadata(metadataFilename);
            }
        }

        private string UpdateFilenameWithPostId(string documentFilename, int documentId)
        {
            string newFilename = null;
            var filenameHelper = new FilenameHelper();

            if (filenameHelper.HasId(documentFilename))
            {
                // We have an ID so let's see what it is
                var id = int.Parse(filenameHelper.GetId(documentFilename));

                if (id != documentId)
                {
                    // ID has been set but does not match the current ID in the filename so let's change it
                    newFilename = filenameHelper.SetId(documentFilename, documentId.ToString());
                }
            }
            else
            {
                // The filename does not currently contain an ID so let's set it
                newFilename = filenameHelper.SetId(documentFilename, documentId.ToString());
            }

            // Now let's rename the file in version control
            if (newFilename == null)
            {
                return documentFilename;
            }

            vstsHelper.RenameFile(vstsAuthentication, project, repoName, branch, documentFilename, newFilename, "Renaming file to include Post ID. ***NO_CI***");

            return newFilename;
        }

        #endregion
    }
}