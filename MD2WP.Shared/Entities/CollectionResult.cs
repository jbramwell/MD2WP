using System.Collections.Generic;

namespace MoonspaceLabs.Shared.Entities
{
    public class CollectionResult<T>
    {
        public int Count { get; set; }

        public List<T> Value { get; set; }
    }
}