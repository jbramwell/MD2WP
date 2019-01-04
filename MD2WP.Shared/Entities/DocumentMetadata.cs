using System;
using System.Collections.Generic;
using System.IO;

namespace MoonspaceLabs.Shared.Entities
{
    public class DocumentMetadata
    {
        public int DocumentId { get; set; }

        // Post or Page
        public string DocumentType { get; set; }

        public List<string> Categories { get; set; }

        public List<string> Tags { get; set; }

        public string PostTitle { get; set; }

        public string DocumentFilename { get; set; }

        public string LastObjectId { get; set; }

        public bool IsPublic { get; set; }

        public string AuthorName { get; set; }

        public string AuthorEmail { get; set; }

        public DateTime AuthorDate { get; set; }

        public bool IsReconciled { get; set; }

        public DocumentMetadata(string filename, string documentType = "post", string defaultCategory = null, string defaultTag = null)
        {
            DocumentType = documentType;
            DocumentFilename = filename;
            PostTitle = Path.GetFileNameWithoutExtension(filename);
            Categories = new List<string>();
            Tags = new List<string>();

            if (defaultCategory != null)
            {
                Categories.Add(defaultCategory);
            }

            if (defaultTag != null)
            {
                Tags.Add(defaultTag);
            }
        }
    }
}