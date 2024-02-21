using System.Collections.Generic;

namespace Apollo.Classes
{
    public class AuxNode
    {
        public List<int> Links { get; private set; }

        public AuxNode()
        {
            Links = new List<int>();
        }
    }
}
