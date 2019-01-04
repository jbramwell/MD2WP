using MD2WP.Shared.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FilenameHelperTests
    {
        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void FilenameDoesNotHaveIdTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "My Markdown File.md";
            var result = filenameHelper.HasId(filename);

            Assert.IsFalse(result);
        }

        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void FilenameHasIdTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "My Markdown File[_1021 ].md";
            var result = filenameHelper.HasId(filename);

            Assert.IsTrue(result);
        }

        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void GetIdTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "My Markdown File[_1021 ].md";
            var result = filenameHelper.GetId(filename);

            Assert.AreEqual("1021", result);
        }

        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void GetFilenameWithoutIdTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "My Markdown File[_1021].md";
            var result = filenameHelper.GetFilenameWithoutId(filename);

            Assert.AreEqual("My Markdown File.md", result);
        }

        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void SetIdTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "My Markdown File.md";
            var id = "2345";
            var result = filenameHelper.SetId(filename, id);

            Assert.AreEqual("My Markdown File[_2345].md", result);
        }

        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void SetIdWithSubfoldersTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "/Subfolder1/Subfolder2/My Markdown File.md";
            var id = "2345";
            var result = filenameHelper.SetId(filename, id);

            Assert.AreEqual("/Subfolder1/Subfolder2/My Markdown File[_2345].md", result);
        }

        [TestMethod, TestCategory("FilenameHelper"), TestCategory("Unit Tests")]
        public void ChangeIdTest()
        {
            var filenameHelper = new FilenameHelper();
            var filename = "My Markdown File[_1234].md";
            var id = "2345";
            var result = filenameHelper.SetId(filename, id);

            Assert.AreEqual("My Markdown File[_2345].md", result);
        }
    }
}
