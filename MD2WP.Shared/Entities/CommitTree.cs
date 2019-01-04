using System;

namespace MoonspaceLabs.Shared.Entities
{
    public class CommitTree
    {
        public string treeId { get; set; }
        public CommitPush push { get; set; }
        public string commitId { get; set; }
        public Author author { get; set; }
        public Committer committer { get; set; }
        public string comment { get; set; }
        public string[] parents { get; set; }
        public string url { get; set; }
        public string remoteUrl { get; set; }
        public _Links _links { get; set; }
    }

    public class CommitPush
    {
        public Pushedby pushedBy { get; set; }
        public int pushId { get; set; }
        public DateTime date { get; set; }
    }

    public class Pushedby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    public class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
        public Repository repository { get; set; }
        public Changes changes { get; set; }
        public Web web { get; set; }
        public Tree tree { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Repository
    {
        public string href { get; set; }
    }

    public class Changes
    {
        public string href { get; set; }
    }

    public class Web
    {
        public string href { get; set; }
    }

    public class Tree
    {
        public string href { get; set; }
    }
}