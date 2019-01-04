using System.Collections.Generic;

namespace MoonspaceLabs.Shared.Entities
{
    public class Push
    {
        public List<RefUpdate> refUpdates { get; set; }

        public List<Commit> commits { get; set; }

        public Push()
        {
            refUpdates = new List<RefUpdate>();
            commits = new List<Commit>();
        }
    }

    public class RefUpdate
    {
        public string name { get; set; }
        public string oldObjectId { get; set; }
    }

    public class Commit
    {
        public string comment { get; set; }

        public List<Change> changes { get; set; }

        public Commit()
        {
            changes = new List<Change>();
        }
    }

    public class Change
    {
        public string changeType { get; set; }

        public string sourceServerItem { get; set; }

        public Item item { get; set; }

        public NewContent newContent { get; set; }

        public Change()
        {
            item = new Item();
            //newContent = new NewContent();
        }
    }

    public class Item
    {
        public string path { get; set; }
    }

    public class NewContent
    {
        public string content { get; set; }

        public string contentType { get; set; }
    }
}