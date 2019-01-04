using MD2WP.Shared.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests.IntegrationTests
{
    [TestClass]
    public class WordPressHelperTests
    {
        #region Private Attributes

        private readonly string _baseUrl;
        private readonly string _userName;
        private readonly string _password;

        #endregion

        #region Constructor(s)

        public WordPressHelperTests()
        {
            var settings = IntegrationTestSettings.GetIntegrationTestSettings();

            _baseUrl = settings.WordPress.BaseUrl;
            _userName = settings.WordPress.UserName;
            _password = settings.WordPress.Password;
        }

        #endregion

        #region Author Tests

        [TestMethod, TestCategory("Authors"), TestCategory("Integration Tests")]
        public void GetAuthorIdTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedId = "15648409";
            var actualId = helper.GetAuthorId("jeff@moonspace.net");

            Assert.AreEqual(expectedId, actualId);
        }

        #endregion

        #region Category Tests

        [TestMethod, TestCategory("Categories"), TestCategory("Integration Tests")]
        public void GetCategoryExistsTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = "TestCategory";
            var taxonomy = "category";
            var actualTerm = helper.GetTerm(expectedTermName, taxonomy, false);

            Assert.AreEqual(expectedTermName, actualTerm.Name);
        }

        [TestMethod, TestCategory("Categories"), TestCategory("Integration Tests")]
        public void GetCategoryDoesNotExistsTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = Guid.NewGuid().ToString();
            var taxonomy = "category";
            var actualTerm = helper.GetTerm(expectedTermName, taxonomy, false);

            Assert.IsNull(actualTerm);
        }

        [TestMethod, TestCategory("Categories"), TestCategory("Integration Tests")]
        public void CreateCategoryTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = Guid.NewGuid().ToString();
            var taxonomy = "category";
            var actualId = helper.CreateTerm(expectedTermName, taxonomy);

            Assert.IsNotNull(actualId);
            Assert.IsTrue(int.Parse(actualId) > 0);

            helper.DeleteTerm(int.Parse(actualId), taxonomy);
        }

        [TestMethod, TestCategory("Categories"), TestCategory("Integration Tests")]
        public void DeleteCategoryTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = Guid.NewGuid().ToString();
            var taxonomy = "category";
            var actualId = helper.CreateTerm(expectedTermName, taxonomy);

            Assert.IsNotNull(actualId);
            Assert.IsTrue(int.Parse(actualId) > 0);

            helper.DeleteTerm(int.Parse(actualId), taxonomy);

            var term = helper.GetTerm(expectedTermName, taxonomy, false);

            Assert.IsNull(term);
        }

        #endregion

        #region Tag Tests

        [TestMethod, TestCategory("Tags"), TestCategory("Integration Tests")]
        public void GetTagExistsTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = "TestTag";
            var taxonomy = "post_tag";
            var actualTerm = helper.GetTerm(expectedTermName, taxonomy, false);

            Assert.AreEqual(expectedTermName, actualTerm.Name);
        }

        [TestMethod, TestCategory("Tags"), TestCategory("Integration Tests")]
        public void GetTagDoesNotExistsTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = Guid.NewGuid().ToString();
            var taxonomy = "post_tag";
            var actualTerm = helper.GetTerm(expectedTermName, taxonomy, false);

            Assert.IsNull(actualTerm);
        }

        [TestMethod, TestCategory("Tags"), TestCategory("Integration Tests")]
        public void CreateTagTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = Guid.NewGuid().ToString();
            var taxonomy = "post_tag";
            var actualId = helper.CreateTerm(expectedTermName, taxonomy);

            Assert.IsNotNull(actualId);
            Assert.IsTrue(int.Parse(actualId) > 0);

            helper.DeleteTerm(int.Parse(actualId), taxonomy);
        }

        [TestMethod, TestCategory("Tags"), TestCategory("Integration Tests")]
        public void DeleteTagTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var expectedTermName = Guid.NewGuid().ToString();
            var taxonomy = "post_tag";
            var actualId = helper.CreateTerm(expectedTermName, taxonomy);

            Assert.IsNotNull(actualId);
            Assert.IsTrue(int.Parse(actualId) > 0);

            helper.DeleteTerm(int.Parse(actualId), taxonomy);

            var term = helper.GetTerm(expectedTermName, taxonomy, false);

            Assert.IsNull(term);
        }

        #endregion

        #region Post Tests

        [TestMethod, TestCategory("Posts"), TestCategory("Integration Tests")]
        public void CreateNewPublicPostTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);

            // Arrange
            var postTitle = "Integration Test Post";
            var publishNewPostAsDraft = false;
            var isDraft = true;
            var publishAsCommitter = true;

            // Act
            var postStatus = helper.CreatePost(postTitle,
                "This is an integration test...",
                0,
                "post",
                null,
                null,
                isDraft,
                publishNewPostAsDraft,
                "Jeff Bramwell",
                "jeff@moonspace.net",
                publishAsCommitter);

            // Assert
            Assert.IsNotNull(postStatus);
            Assert.IsFalse(postStatus.IsDraft);

            if (postStatus != null)
            {
                var post = helper.GetPost(postStatus.Id);

                // Delete the post before doing assertions
                helper.DeletePost(postStatus.Id);

                Assert.AreEqual(postTitle, post.Title);
                Assert.AreEqual("publish", post.Status);
            }
        }

        [TestMethod, TestCategory("Posts"), TestCategory("Integration Tests")]
        public void CreateNewDraftPostTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);

            // Arrange
            var postTitle = "Integration Test Post";
            var publishNewPostAsDraft = true;
            var isDraft = true;
            var publishAsCommitter = true;

            // Act
            var postStatus = helper.CreatePost(postTitle,
                "This is an integration test...",
                0,
                "post",
                null,
                null,
                isDraft,
                publishNewPostAsDraft,
                "Jeff Bramwell",
                "jeff@moonspace.net",
                publishAsCommitter);

            // Assert
            Assert.IsNotNull(postStatus);
            Assert.IsTrue(postStatus.IsDraft);

            if (postStatus != null)
            {
                var post = helper.GetPost(postStatus.Id);

                // Delete the post before doing assertions
                helper.DeletePost(postStatus.Id);

                Assert.AreEqual(postTitle, post.Title);
                Assert.AreEqual("draft", post.Status);
            }
        }

        [TestMethod, TestCategory("Posts"), TestCategory("Integration Tests")]
        public void CreateNewPublicAsDraftPostTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);

            // Arrange
            var postTitle = "Integration Test Post";
            var publishNewPostAsDraft = true;
            var isDraft = false;
            var publishAsCommitter = true;

            // Act
            var postStatus = helper.CreatePost(postTitle,
                "This is an integration test...",
                0,
                "post",
                null,
                null,
                isDraft,
                publishNewPostAsDraft,
                "Jeff Bramwell",
                "jeff@moonspace.net",
                publishAsCommitter);

            // Assert
            Assert.IsNotNull(postStatus);
            Assert.IsTrue(postStatus.IsDraft);

            if (postStatus != null)
            {
                var post = helper.GetPost(postStatus.Id);

                // Delete the post before doing assertions
                helper.DeletePost(postStatus.Id);

                Assert.AreEqual(postTitle, post.Title);
                Assert.AreEqual("draft", post.Status);
            }
        }

        [TestMethod, TestCategory("Posts"), TestCategory("Integration Tests")]
        public void DeletePostTest()
        {
            var helper = new WordPressHelper(_baseUrl, _userName, _password);
            var postTitle = "Integration Test Post";
            var postStatus = helper.CreatePost(postTitle, "This is an integration test...",
                0, "post",
                null, null, true, true,
                "Jeff Bramwell", "jeff@moonspace.net", true);

            Assert.IsNotNull(postStatus);

            if (postStatus != null)
            {
                helper.DeletePost(postStatus.Id);

                var post = helper.GetPost(postStatus.Id);

                Assert.AreEqual("trash", post.Status);
            }
        }

        #endregion
    }
}