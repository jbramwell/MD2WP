using MD2WP.Shared.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.UnitTests
{
    [TestClass]
    public class EncryptionHelperTests
    {
        [TestMethod, TestCategory("Encryption"), TestCategory("Unit Tests")]
        public void EncryptDecryptTest()
        {
            var plainText = "Secrets, secrets and more secrets!";
            var sharedSecret = "Shhh!!! Don't tell!!!";

            var cipherText = EncryptionHelper.EncryptStringAES(plainText, sharedSecret);

            Assert.AreNotEqual(plainText, cipherText);

            var decryptedText = EncryptionHelper.DecryptStringAES(cipherText, sharedSecret);

            Assert.AreEqual(plainText, decryptedText);
        }
    }
}