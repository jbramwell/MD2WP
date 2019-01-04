using System.IO;

namespace MD2WP.Shared.Helpers
{
    public class FilenameHelper
    {
        public bool HasId(string filename)
        {
            return GetId(filename) != null;
        }

        public string GetId(string filename)
        {
            string id = null;
            var startIndex = filename.IndexOf("[_");

            if (startIndex >= 0)
            {
                var endIndex = filename.IndexOf("]", startIndex);

                if (endIndex > startIndex)
                {
                    id = filename.Substring(startIndex + 2, (endIndex - startIndex) - 2).Trim();
                }
            }

            return id;
        }

        public string SetId(string filename, string id, bool useForwardSlashSeparator = true)
        {
            var filenameWithoutId = GetFilenameWithoutId(filename);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithoutId);
            var rootFolders = Path.GetDirectoryName(filenameWithoutId);
            var fileExtension = Path.GetExtension(filenameWithoutId);

            var newFilename = Path.Combine(rootFolders, $"{filenameWithoutExtension}[_{id}]{fileExtension}");

            if (useForwardSlashSeparator)
            {
                newFilename = newFilename.Replace('\\', '/');
            }

            return newFilename;
        }

        public string GetFilenameWithoutId(string filename)
        {
            string filenameWithoutId = null;

            if (!HasId(filename))
            {
                filenameWithoutId = filename;
            }
            else
            {
                // Locate start of ID marker
                var startIndex = filename.IndexOf("[_");

                if (startIndex >= 0)
                {
                    // Locate end of ID marker
                    var endIndex = filename.IndexOf("]", startIndex);

                    if (endIndex > startIndex)
                    {
                        // Locate start of filename extension (e.g. ".md")
                        endIndex = filename.IndexOf(".", endIndex);

                        if (endIndex > startIndex)
                        {
                            filenameWithoutId = (filename.Substring(0, startIndex) + filename.Substring(endIndex)).Trim();
                        }
                    }
                }
            }

            return filenameWithoutId;
        }
    }
}