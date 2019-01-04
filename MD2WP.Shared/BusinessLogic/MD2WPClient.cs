using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using Markdig;
using MD2WP.Shared.Helpers;
using MoonspaceLabs.Shared.BaseClasses;
using MoonspaceLabs.Shared.Entities;
using MoonspaceLabs.Shared.Helpers;
using Newtonsoft.Json;
using WordPressSharp;

namespace MoonspaceLabs.Shared.BusinessLogic
{
    public class MD2WPClient
    {
        #region Private Attributes

        private BasicAuthentication _vstsAuthentication;
        private VstsHelper _vstsHelper;
        private WordPressHelper _wpHelper;
        private List<ItemDescriptor> _markdownFileList;
        private WordPressSiteConfig _siteConfig;
        private WordPressClient _client;
        private List<DocumentMetadata> _metadata;
        private bool _metadataFileCreated;
        private string _accountUrl;
        private string _project;
        private string _repoName;
        private string _branch;
        private string _accessToken;
        private string _metadataFilename;
        private bool _embedExternalImages;
        private bool _publishAsCommitter;
        private bool _processSubfolders;
        private bool _useFolderNameAsCategory;
        private bool _useFolderNameAsTag;
        private bool _publishNewPostsAsDraft;
        private bool _trackPostIdInFilename;
        private bool _addEditLink;
        private string _editLinkText;
        private string _editLinkStyle;

        #endregion

        #region Properties

        private WordPressSiteConfig SiteConfig
        {
            get
            {
                return _siteConfig;
            }
        }

        private WordPressClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new WordPressClient(SiteConfig);
                }

                return _client;
            }
        }

        #endregion

        #region Constructor(s)

        public MD2WPClient(string baseUrl, string userName, string password, string accountUrl,
            string project, string repoName, string branch, string accessToken, string metadataFilename,
            bool embedExternalImages, bool publishAsCommitter, bool processSubfolders, bool useFolderNameAsCategory,
            bool useFolderNameAsTag, bool publishNewPostsAsDraft, bool trackPostIdInFilename,
            bool addEditLink, string editLinkText, string editLinkStyle)
        {
            Logger.LogMessage("MDWWPClient::ctor");
            Logger.LogMessage($"  BaseUrl = {baseUrl}");
            Logger.LogMessage($"  UserName = {userName}");
            Logger.LogMessage($"  Password = ********");
            Logger.LogMessage($"  Account URL = {accountUrl}");
            Logger.LogMessage($"  Project = {project}");
            Logger.LogMessage($"  RepoName = {repoName}");
            Logger.LogMessage($"  Branch = {branch}");
            Logger.LogMessage($"  AccessToken = ********");
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

            _vstsAuthentication = new BasicAuthentication(accountUrl, string.Empty, accessToken);
            _vstsHelper = new VstsHelper();
            _wpHelper = new WordPressHelper(baseUrl, userName, password);

            _siteConfig = new WordPressSiteConfig()
            {
                BaseUrl = baseUrl,
                Username = userName,
                Password = password
            };

            _accountUrl = accountUrl;
            _project = project;
            _repoName = repoName;
            _branch = branch;
            _accessToken = accessToken;
            _metadataFilename = metadataFilename.TrimStart('\\');
            _embedExternalImages = embedExternalImages;
            _publishAsCommitter = publishAsCommitter;
            _processSubfolders = processSubfolders;
            _useFolderNameAsCategory = useFolderNameAsCategory;
            _useFolderNameAsTag = useFolderNameAsTag;
            _publishNewPostsAsDraft = publishNewPostsAsDraft;
            _trackPostIdInFilename = trackPostIdInFilename;
            _addEditLink = addEditLink;
            _editLinkText = editLinkText;
            _editLinkStyle = editLinkStyle;
        }

        #endregion

        #region HTML Methods

        private byte[] GetImageFromUrl(string url)
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
            Logger.LogMessage($"Converting markdown to HTML");

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
            Logger.LogMessage($"Converting image links to embedded image data (where applicable)");

            var htmlDocument = new HtmlAgilityPack.HtmlDocument();

            htmlDocument.LoadHtml(fileContents);

            var htmlNavigator = htmlDocument.CreateNavigator();
            var imageLinks = htmlNavigator.Select(".//img");

            foreach (HtmlNodeNavigator image in imageLinks)
            {
                var imageSource = image.CurrentNode.Attributes["src"];
                var imageUrl = imageSource.Value;
                var isExternal = false;
                var imageExtension = string.Empty;

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
                    if (_embedExternalImages)
                    {
                        var imageBase64 = Convert.ToBase64String(GetImageFromUrl(imageUrl));

                        imageSource.Value = $"data:image/{imageExtension};base64,{imageBase64}";
                    }
                }
                else
                {
                    // Download image from VSTS via REST API
                    var imageBase64 = Convert.ToBase64String(
                        _vstsHelper.DownloadBinaryFile(_vstsAuthentication, _project, _repoName, _branch, imageUrl.TrimStart('.')));

                    imageSource.Value = $"data:image/{imageExtension};base64,{imageBase64}";
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

            var text = _vstsHelper.GetFileContents(_vstsAuthentication, _project, _repoName, _branch, _metadataFilename);

            if (text.Contains("GitItemNotFoundException"))
            {
                Logger.LogMessage("Metadata file does not exist so creating it");

                _metadata = new List<DocumentMetadata>();

                // Track this as being created so we can perform an "add" instead of the usual "edit"
                _metadataFileCreated = true;
            }
            else
            {
                _metadata = JsonConvert.DeserializeObject<List<DocumentMetadata>>(text);

                if (_metadata == null)
                {
                    _metadata = new List<DocumentMetadata>();
                }
            }
        }

        private void ClearMetadataReconcileFlag()
        {
            if (_metadata != null)
            {
                foreach (var item in _metadata)
                {
                    item.IsReconciled = false;
                }
            }
        }

        private void RemoveOrphanedMetadataEntries()
        {
            Logger.LogMessage("Removing 'orphaned' entries from metadata file.");

            _metadata.RemoveAll(x => !x.IsReconciled);
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

            foreach (var metadataEntry in _metadata)
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

            foreach (var itemDescriptor in _markdownFileList)
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

            var json = JsonConvert.SerializeObject(_metadata, Formatting.Indented);

            _vstsHelper.SaveTextFile(_vstsAuthentication, _project, _repoName, _branch, filename, json, "Updated by Markdown to WordPress. ***NO_CI***", _metadataFileCreated);
        }

        public void LoadMarkdownFileList()
        {
            Logger.LogMessage("Loading list of markdown files...");

            _markdownFileList = _vstsHelper.GetFileList(_vstsAuthentication, _project, _repoName, _branch, Path.GetDirectoryName(_metadataFilename), ".md", _processSubfolders);
        }

        public void PublishMarkdownFiles()
        {
            var publishOccurred = false;

            LoadMetadata();
            LoadMarkdownFileList();
            ClearMetadataReconcileFlag(); // So we can track "orphaned" entries (i.e. a .md file has been deleted)

            foreach (var markdownFile in _markdownFileList)
            {
                // Check Commit ID to see if it's different from what was stored. If not, then nothing has
                // changed so no need to publish (again)
                var documentMetadata = GetDocumentMetadata(markdownFile.Path);
                var commitId = GetMarkdownFileCommitId(markdownFile.Path);
                var markdownFileSourceUrl = $"{_vstsAuthentication.AccountUrl}/{_project}/_git/{_repoName}?path={markdownFile.Path}&version=GB{_branch}&editMode=true";

                if (documentMetadata == null)
                {
                    Logger.LogMessage($"{markdownFile.Path} is new... creating initial metadata entry");

                    // There is no metedata defined for the markdown file being processed so
                    // let's create a default entry (that will be published as a Draft)
                    documentMetadata = new DocumentMetadata(markdownFile.Path);
                    _metadata.Add(documentMetadata);
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
                    var fileContents = _vstsHelper.GetFileContents(_vstsAuthentication, _project, _repoName, _branch, markdownFile.Path);
                    var commitInfo = _vstsHelper.GetCommitInfo(_vstsAuthentication, _project, _repoName, _branch, markdownFile.CommitId);

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
                    if (_addEditLink)
                    {
                        fileContents += $"<br/><br/><a style=\"{_editLinkStyle}\" href=\"{markdownFileSourceUrl}\">{_editLinkText}</a>";
                    }

                    // Convert image links to embedded images
                    fileContents = ConvertHtmlToEmbeddedImages(fileContents);

                    // Set the post title because it's possible the file has been renamed
                    documentMetadata.PostTitle = Path.GetFileNameWithoutExtension(filenameHelper.GetFilenameWithoutId(markdownFile.Path));

                    if (_useFolderNameAsCategory)
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

                    if (_useFolderNameAsTag)
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
                    var postStatus = _wpHelper.CreatePost(
                        documentMetadata.PostTitle,
                        fileContents,
                        documentMetadata.DocumentId,
                        documentMetadata.DocumentType,
                        documentMetadata.Categories,
                        documentMetadata.Tags,
                        !documentMetadata.IsPublic,
                        _publishNewPostsAsDraft,
                        documentMetadata.AuthorName,
                        documentMetadata.AuthorEmail,
                        _publishAsCommitter);

                    documentMetadata.DocumentId = postStatus.Id;
                    documentMetadata.IsPublic = !postStatus.IsDraft; // Track draft/public
                    documentMetadata.LastObjectId = commitId;

                    publishOccurred = true;

                    if (_trackPostIdInFilename)
                    {
                        documentMetadata.DocumentFilename =
                            UpdateFilenameWithPostID(markdownFile.Path, documentMetadata.DocumentId);
                    }
                }
                else
                {
                    Logger.LogMessage($"The markdown file '{markdownFileSourceUrl}' has not been updated since it was last published. Skipping this file.");
                    //Logger.LogMessage($"The markdown file '{markdownFile.Path}' has not been updated since it was last published. Skipping this file.");
                }
            }

            // No point saving anything if nothing was changed/published
            if (publishOccurred || !_metadata.TrueForAll(x => x.IsReconciled))
            {
                RemoveOrphanedMetadataEntries();
                SaveMetadata(_metadataFilename);
            }
        }

        private string UpdateFilenameWithPostID(string documentFilename, int documentId)
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

            _vstsHelper.RenameFile(_vstsAuthentication, _project, _repoName, _branch, documentFilename, newFilename, "Renaming file to include Post ID. ***NO_CI***");

            return newFilename;
        }

        #endregion
    }
}