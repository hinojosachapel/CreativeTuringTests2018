using System.Collections.Generic;

namespace Apollo.Classes
{
    public class MarkovModel
    {
        public Dictionary<string, Node> PoemsGraph { get; private set; }
        public Dictionary<string, Node> TextsGraph { get; private set; }

        public Dictionary<string, AuxNode> AuxPoemsGraph { get; private set; }
        public Dictionary<string, AuxNode> AuxTextsGraph { get; private set; }

        public MarkovModel()
        {
            PoemsGraph = new Dictionary<string, Node>();
            TextsGraph = new Dictionary<string, Node>();

            AuxPoemsGraph = new Dictionary<string, AuxNode>();
            AuxTextsGraph = new Dictionary<string, AuxNode>();
        }
    }
}
