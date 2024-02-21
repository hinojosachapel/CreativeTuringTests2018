using System.Collections.Generic;

namespace Apollo.Classes
{
    public class Node
    {
        public int Id { get; private set; }
        public bool IsStartGram { get; set; }
        public bool IsEndGram { get; set; }
        public List<int> Links { get; private set; }

        public Node(int id)
        {
            Id = id;
            IsStartGram = false;
            IsEndGram = false;
            Links = new List<int>();
        }
    }
}
