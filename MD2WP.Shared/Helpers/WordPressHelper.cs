using System;
using System.Collections.Generic;
using MD2WP.Shared.BusinessLogic;
using WordPressSharp;
using WordPressSharp.Models;

namespace MD2WP.Shared.Helpers
{
    public class WordPressHelper
    {
        private WordPressSiteConfig _siteConfig;
        private WordPressClient _client;

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

        public WordPressHelper(string baseUrl, string userName, string password)
        {
            _siteConfig = new WordPressSiteConfig()
            {
                BaseUrl = baseUrl,
                Username = userName,
                Password = password
            };
        }

        #endregion

        public string GetAuthorId(string email)
        {
            var authorId = 0;
            var filter = new UserFilter
            {
                Role = "author",
                Who = "authors",
                OrderBy = "Role",
                Order = "Asc"
            };

            var authors = Client.GetUsers(filter);

            // Let's locatet the specific author we're looking for, if we can...
            if (authors != null && authors.Length > 0)
            {
                foreach (var author in authors)
                {
                    if (author.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(author.Id, out authorId);
                        break;
                    }
                }
            }

            return authorId.ToString();
        }

        private List<Term> GetTerms(List<string> categories, List<string> tags, bool createMissingTerms)
        {
            // Configure tags and categories
            var terms = new List<Term>();

            // Add tags
            if (tags != null)
            {
                foreach (var tagName in tags)
                {
                    var tag = GetTerm(tagName, "post_tag", createMissingTerms);

                    terms.Add(tag);
                }
            }

            // Add categories
            if (categories != null)
            {
                foreach (var categoryName in categories)
                {
                    var category = GetTerm(categoryName, "category", createMissingTerms);

                    terms.Add(category);
                }
            }

            return terms;
        }

        /// <summary>
        /// Returns the <see cref="Term"/> associated with the name passed in.
        /// </summary>
        /// <param name="name">The name of the term to return.</param>
        /// <returns>Returns the <see cref="Term"/> associated with the name passed in. If the
        /// term does not exist, it will be created.</returns>
        public Term GetTerm(string name, string taxonomy, bool createMissingTerm)
        {
            Term term = null;

            // Retrieve the current set of terms (categories or tags) from WordPress
            var terms = Client.GetTerms(taxonomy, new TermFilter());

            // If terms exist, let's see if we can find what we're looking for...
            if (terms != null && terms.Length > 0)
            {
                foreach (var termCandidate in terms)
                {
                    if (termCandidate.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        term = termCandidate;
                        break;
                    }
                }
            }

            if (createMissingTerm && term == null)
            {
                term = new Term
                {
                    Name = name,
                    Slug = name,
                    Taxonomy = taxonomy
                };

                term.Id = CreateTerm(name, taxonomy);
            }

            return term;
        }

        /// <summary>
        /// Creates a new <see cref="Term"/> associated with the name passed in.
        /// </summary>
        /// <param name="name">The name of the term to create.</param>
        /// <returns>The ID of the new term.</returns>
        public string CreateTerm(string name, string taxonomy)
        {
            // If the term was not located, let's go ahead and create it
            var term = new Term
            {
                Name = name,
                Slug = name,
                Taxonomy = taxonomy
            };

            return Client.NewTerm(term);
        }

        /// <summary>
        /// Deletes the term associated with the ID passed in.
        /// </summary>
        /// <param name="name">The ID of the term to delete.</param>
        public bool DeleteTerm(int id, string taxonomy)
        {
            return Client.DeleteTerm(id, taxonomy);
        }

        public PostStatus CreatePost(string title, string body, int postId, string documentType, List<string> categories,
            List<string> tags, bool isDraft, bool publishNewPostAsDraft, string authorName, string authorEmail, bool publishAsCommitter)
        {
            Logger.LogMessage($"Creating post in WordPress");

            var postStatus = new PostStatus();
            var postIsNew = false;
            Post post = null;

            // Let's attempt to retrieve an existing post if a post ID was provided
            if (postId > 0)
            {
                try
                {
                    // Wrapped in a try/catch 'cause if the specified post ID does
                    // not exist, an exception is raised
                    post = Client.GetPost(postId);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("[401]"))
                    {
                        Logger.LogError($"The WordPress account configured in the build parameters must be in the Administrator role.");
                        throw;
                    }
                    else if (!ex.Message.Contains("Invalid post ID"))
                    {
                        Logger.LogMessage($"The post with ID '{postId}' was not found. This post must have been previously deleted. We will create a new post with the same ID.");
                    }
                }
            }

            // If a post was not found then we'll create a new one
            if (post == null)
            {
                postIsNew = true;

                post = new Post
                {
                    PostType = documentType, // post or page
                    PublishDateTime = DateTime.Now,
                    Status = publishNewPostAsDraft ? "draft" : "publish" // draft or publish
                };
            }

            post.Title = title;
            post.Content = body;

            if (!postIsNew)
            {
                post.Status = isDraft ? "draft" : "publish"; // draft or publish
            }

            if (publishAsCommitter)
            {
                post.Author = GetAuthorId(authorEmail);  // If author is not located, the signed in user will be used
                Logger.LogMessage($"Publishing as {authorEmail} [Author ID: {post.Author}]");
            }

            // Add categories and tags
            var terms = GetTerms(categories, tags, true);

            if (terms != null && terms.Count > 0)
            {
                Logger.LogMessage("Adding terms (categories and tags)");
                post.Terms = terms.ToArray();
            }

            // Add/update post
            if (post.Id == null)
            {
                // Create a new post
                try
                {
                    postStatus.Id = int.Parse(Client.NewPost(post));
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                    throw;
                }

                Logger.LogMessage($"New post published with ID: {postStatus.Id}");
            }
            else
            {
                // Post previously existed so let's edit it
                if (Client.EditPost(post))
                {
                    postStatus.Id = int.Parse(post.Id);

                    Logger.LogMessage($"Existing post updated with ID: {postStatus.Id}");
                }
            }

            // We have to pass the status back out so we know what to persist in the metadata
            postStatus.IsDraft = post.Status.Equals("draft", StringComparison.InvariantCultureIgnoreCase);

            return postStatus;
        }

        public void DeletePost(int id)
        {
            Logger.LogMessage($"Deleting post ID #{id}");

            Client.DeletePost(id);
        }

        public Post GetPost(int id)
        {
            return Client.GetPost(id);
        }
    }
}