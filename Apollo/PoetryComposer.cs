//
// Literary Creative Turing Tests 2018
// http://bregman.dartmouth.edu/turingtests/Literary2018
//
// LyriX – This is an "Open Format / Literary Metacreation" challenge. Entries can work
// from a noun prompt or can generate an original short poem based on some other
// mechanism. Regardless, the machine will need to have the ability to produce
// effectively an infinite number of poems. Entries cannot exceed 30 lines and
// cannot exceed 80 characters per line. Poems will be evaluated for their "artistry".
// All entries to PoetiX and LimeriX will automatically be entered in Lyrix.
//
// Author: Rubén Hinojosa Chapel
// contact@hinojosachapel.com
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Apollo.Classes;

namespace Apollo
{
    public static class PoetryComposer
    {
        private const int MAX_LINE = 30; // Entries cannot exceed 30 lines
        private const int MAX_CHARS_PER_LINE = 30; // cannot exceed 80 characters per line
        private const int MIN_LINE = 12;
        private const string CR_LF = "\r\n";
        private const string LINEFEED = "\n";
        private const string LEFT_WORD_PREFIX = "left-";
        private const string RIGHT_WORD_PREFIX = "right-";

        private readonly static string[] TokenSplitArg1 = new string[] { " " };
        private readonly static string[] TokenSplitArg2 = new string[] { " ", CR_LF };
        private readonly static string[] TokenSplitArg3 = new string[] { " ", CR_LF, LINEFEED, ".", "?", "!", ";", ":", "," };
        private readonly static string[] NewlineSplitArg = new string[] { CR_LF, LINEFEED };
        private readonly static Regex IsNumber = new Regex("^[0-9]+[\r,\n,.,-]$");
        private readonly static List<string> PunctuationMarks = new List<string> { ".", "?", "!", ";", ",", ":" };

        private static Random Rnd;

        public static MarkovModel Model { get; set; } = new MarkovModel();

        public static string GetLastWord(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return String.Empty;
            }

            var tokens = Tokenize(text, true);
            return tokens[tokens.Count - 1];
        }

        public static async Task TrainPoem(string poemText)
        {
            await Task.Factory.StartNew(() =>
            {
                TrainPoemSync(poemText);
            });
        }

        public static async Task TrainText(string text)
        {
            await Task.Factory.StartNew(() =>
            {
                TrainTextSync(text);
            });
        }

        public static async Task<string> GetNewPoem(string phrase = "")
        {
            string result = String.Empty;

            await Task.Factory.StartNew(() =>
            {
                result = GetNewPoemSync(phrase);
            });

            return result;
        }

        private static void TrainPoemSync(string poemText)
        {
            poemText = PrepareForTokenize(poemText);
            var tokens = Tokenize(poemText, true);
            int tokensCount = tokens.Count();

            List<string> ngramList = new List<string>();
            string ngram;

            Node currentNode = null;
            Node nextNode = null;
            string currentNgram;
            string nextNgram = String.Empty;

            // Use 3-gram nodes
            for (int i = tokensCount - 3; i >= 0; i -= 3)
            {
                ngram = Concat(tokens[i], tokens[i + 1], tokens[i + 2]);
                ngramList.Add(ngram);
            }

            int mod = tokensCount % 3;

            if (mod == 1) // One token left
            {
                ngram = tokens[0];
                ngramList.Add(ngram);
            }
            else if (mod == 2) // Two tokens left
            {
                ngram = Concat(tokens[0], tokens[1]);
                ngramList.Add(ngram);
            }

            currentNgram = ngramList[0];
            currentNode = GetNode(currentNgram, Model.PoemsGraph);
            currentNode.IsEndGram = true;

            for (int i = 0; i < ngramList.Count - 1; i++)
            {
                currentNgram = ngramList[i];
                currentNode = GetNode(currentNgram, Model.PoemsGraph);

                nextNgram = ngramList[i + 1];
                nextNode = GetNode(nextNgram, Model.PoemsGraph);
                currentNode.Links.Add(nextNode.Id);

                InsertNgramInAuxGraph(currentNgram, Model.AuxPoemsGraph, currentNode.Id);
            }

            if (tokensCount % 3 == 0)
            {
                currentNode.IsStartGram = true;
            }

            nextNode.IsStartGram = true;
            InsertNgramInAuxGraph(nextNgram, Model.AuxPoemsGraph, nextNode.Id);

            int newFirstPos = ngramList.Count;

            // Use 2-gram nodes
            for (int i = tokensCount - 2; i >= 0; i -= 2)
            {
                ngram = Concat(tokens[i], tokens[i + 1]);
                ngramList.Add(ngram);
            }

            if (tokensCount % 2 > 0) // One token left
            {
                ngram = tokens[0];
                ngramList.Add(ngram);
            }

            currentNgram = ngramList[newFirstPos];
            currentNode = GetNode(currentNgram, Model.PoemsGraph);
            currentNode.IsEndGram = true;

            for (int i = newFirstPos; i < ngramList.Count - 1; i++)
            {
                currentNgram = ngramList[i];
                currentNode = GetNode(currentNgram, Model.PoemsGraph);

                nextNgram = ngramList[i + 1];
                nextNode = GetNode(nextNgram, Model.PoemsGraph);
                currentNode.Links.Add(nextNode.Id);

                InsertNgramInAuxGraph(currentNgram, Model.AuxPoemsGraph, currentNode.Id);
            }

            nextNode.IsStartGram = true;
            InsertNgramInAuxGraph(nextNgram, Model.AuxPoemsGraph, nextNode.Id);
        }

        private static void TrainTextSync(string text)
        {
            text = PrepareForTokenize(text);
            var tokens = Tokenize(text, true);
            int tokensCount = tokens.Count();

            List<string> ngramList = new List<string>();
            string ngram;

            Node currentNode = null;
            Node nextNode = null;
            string currentNgram;
            string nextNgram = String.Empty;

            // Use 3-gram nodes
            for (int i = tokensCount - 3; i >= 0; i -= 3)
            {
                ngram = Concat(tokens[i], tokens[i + 1], tokens[i + 2]);
                ngramList.Add(ngram);
            }

            int mod = tokensCount % 3;

            if (mod == 1) // One token left
            {
                ngram = tokens[0];
                ngramList.Add(ngram);
            }
            else if (mod == 2) // Two tokens left
            {
                ngram = Concat(tokens[0], tokens[1]);
                ngramList.Add(ngram);
            }

            currentNgram = ngramList[0];
            currentNode = GetNode(currentNgram, Model.TextsGraph);
            currentNode.IsEndGram = true;

            for (int i = 0; i < ngramList.Count - 1; i++)
            {
                currentNgram = ngramList[i];
                currentNode = GetNode(currentNgram, Model.TextsGraph);

                if (currentNgram.EndsWith(".\r\n") || currentNgram.EndsWith("!\r\n"))
                {
                    currentNode.IsEndGram = true;
                }

                nextNgram = ngramList[i + 1];
                nextNode = GetNode(nextNgram, Model.TextsGraph);
                currentNode.Links.Add(nextNode.Id);

                if (nextNgram.EndsWith(".\r\n") || nextNgram.EndsWith("!\r\n"))
                {
                    currentNode.IsStartGram = true;
                }

                InsertNgramInAuxGraph(nextNgram, Model.AuxTextsGraph, nextNode.Id);
            }

            nextNode.IsStartGram = true;
            InsertNgramInAuxGraph(nextNgram, Model.AuxTextsGraph, nextNode.Id);
        }

        private static string GetNextTextNgram(string ngram)
        {
            Node node;
            string nextNgram = String.Empty;
            int nextIndex;

            ngram = ngram.ToLower();

            if (Model.TextsGraph.ContainsKey(ngram))
            {
                node = Model.TextsGraph[ngram];

                if (node.Links.Count == 0)
                {
                    // The node has no edges
                    // Get one of the last words
                    var lastWords = (from Node n in Model.TextsGraph.Values
                                     where n.IsEndGram
                                     select n).ToList();

                    node = lastWords[Rnd.Next(lastWords.Count)];
                }
                else if (node.Links.Count == 1)
                {
                    // The node has only one edge
                    nextIndex = node.Links[0];
                    node = Model.TextsGraph.Values.ElementAt(nextIndex);
                }
                else
                {
                    // The node has more than one edge
                    nextIndex = node.Links[Rnd.Next(node.Links.Count)];
                    node = Model.TextsGraph.Values.ElementAt(nextIndex);
                }

                nextNgram = Model.TextsGraph.Keys.ElementAt(node.Id);
            }
            else
            {
                if (!HasPunctuationMark(ngram))
                {
                    foreach (string sign in PunctuationMarks)
                    {
                        string newNgram = ngram + sign;
                        nextNgram = GetNextTextNgram(newNgram);

                        if (!String.IsNullOrWhiteSpace(nextNgram))
                        {
                            // Next token found
                            break;
                        }
                    }
                }
            }

            return nextNgram;
        }

        private static string GetFirstWordFromPhrase(string phrase)
        {
            string word;
            int posSpace = phrase.IndexOf(" ");

            if (posSpace == -1)
            {
                word = phrase;
            }
            else
            {
                word = phrase.Substring(0, posSpace);
            }

            return word;
        }

        // Given a word, returns a node that holds an n-gram that contains that word.
        // If the n-gram starts with that word, returns a next node.
        private static Node GetNodeFromGraph(string word, Dictionary<string, AuxNode> auxGraph, Dictionary<string, Node> graph, List<Node> lastWords)
        {
            Node node = null;
            AuxNode auxNode = null;
            AuxNode auxNode1 = null;
            AuxNode auxNode2 = null;
            bool isLeftWord = false;

            // Just for removing all signs
            var words = word.Split(TokenSplitArg3, StringSplitOptions.RemoveEmptyEntries);

            string leftWord = LEFT_WORD_PREFIX + words[0].ToLower();

            // Search tokens starting with word
            if (auxGraph.ContainsKey(leftWord))
            {
                auxNode1 = auxGraph[leftWord];
            }

            // Search tokens ending with word
            string rightWord = RIGHT_WORD_PREFIX + words[words.Length - 1].ToLower();

            if (auxGraph.ContainsKey(rightWord))
            {
                auxNode2 = auxGraph[rightWord];
            }

            if ((auxNode1 != null) && (auxNode2 != null))
            {
                // Randomly take auxNode1 or auxNode2
                if (DateTime.Now.Ticks % 2 == 0)
                {
                    auxNode = auxNode1;
                    isLeftWord = true;
                }
                else
                {
                    auxNode = auxNode2;
                    isLeftWord = false;
                }
            }
            else
            {
                if (auxNode2 == null)
                {
                    auxNode = auxNode1;
                    isLeftWord = true;
                }
                else
                {
                    auxNode = auxNode2;
                    isLeftWord = false;
                }
            }

            if (auxNode == null)
            {
                // token not found
                return null;
            }

            // Randomly select one of the nodes found.
            int pos = Rnd.Next(auxNode.Links.Count - 1);
            int id = auxNode.Links[pos];

            string ngram = graph.Keys.ElementAt(id);
            node = graph[ngram];

            // When the token starts with the word, return the next token,
            // else return the current one.  
            if (isLeftWord)
            {
                int nextIndex;

                if (node.Links.Count == 0)
                {
                    // The node has no edges.
                    // Get one of the last words.
                    node = lastWords[Rnd.Next(lastWords.Count)];
                }
                else if (node.Links.Count == 1)
                {
                    // The node has only one edge.
                    nextIndex = node.Links[0];
                    node = graph.Values.ElementAt(nextIndex);
                }
                else
                {
                    // The node has more than one edge.
                    nextIndex = node.Links[Rnd.Next(node.Links.Count)];
                    node = graph.Values.ElementAt(nextIndex);
                }
            }

            return node;
        }

        private static bool HasPunctuationMark(string word)
        {
            bool result = PunctuationMarks.Any(mark => word.EndsWith(mark));
            return result;
        }

        // Splits a string into verses.
        private static string GetVerses(string phrase)
        {
            string result = string.Empty;
            var words = Tokenize(phrase, true);
            int lines = 1;

            int i = words.Count - 1;
            result = words[i];

            while (i > 0)
            {
                if (result.Length > lines * MAX_CHARS_PER_LINE)
                {
                    result = CR_LF + result;
                    lines++;
                }

                result = String.Format("{0} {1}", words[--i], result);
            }

            return result;
        }

        private static string GetNewPoemSync(string phrase)
        {
            Rnd = new Random((int)DateTime.Now.Ticks);

            var lastPoemsWords = (from Node n in Model.PoemsGraph.Values
                                  where n.IsEndGram
                                  select n).ToList();

            var lastTextsWords = (from Node n in Model.TextsGraph.Values
                                  where n.IsEndGram
                                  select n).ToList();

            Node node = null;
            Node textNode = null;
            string poem = String.Empty;
            bool isPoeticProse = false;
            int lineNumber = 1;

            phrase = RemovePunctuationMarks(phrase);

            if (String.IsNullOrWhiteSpace(phrase))
            {
                textNode = lastTextsWords[Rnd.Next(lastTextsWords.Count)];
                phrase = Model.TextsGraph.Keys.ElementAt(textNode.Id);
            }

            poem = GetVerses(phrase);
            string currentWord = GetFirstWordFromPhrase(phrase);

            // Search the word in the graphs
            while (node == null)
            {
                // Look at first in the texts graph.
                string nextWord = String.Empty;
                textNode = GetNodeFromGraph(currentWord, Model.AuxTextsGraph, Model.TextsGraph, lastTextsWords);

                if (textNode != null)
                {
                    nextWord = Model.TextsGraph.Keys.ElementAt(textNode.Id);
                }

                if (!String.IsNullOrWhiteSpace(nextWord))
                {
                    poem = String.Format("{0} {1}", nextWord, poem);

                    // Entries cannot exceed 80 characters per line.
                    if (poem.Length > lineNumber * (MAX_CHARS_PER_LINE - 10))
                    {
                        poem = CR_LF + poem;
                        lineNumber++;

                        // Entries cannot exceed 30 lines. This is unlikely to happen.
                        if (lineNumber >= MAX_LINE)
                        {
                            isPoeticProse = true; // At the end we built the poem only from words in non poetic text files.
                            break;
                        }
                    }

                    node = GetNodeFromGraph(nextWord, Model.AuxPoemsGraph, Model.PoemsGraph, lastPoemsWords);
                    currentWord = nextWord;
                }
                else
                {
                    // Word not found in the texts graph. Search in the poems graph.
                    node = GetNodeFromGraph(currentWord, Model.AuxPoemsGraph, Model.PoemsGraph, lastPoemsWords);

                    if (node == null)
                    {
                        currentWord = RemovePunctuationMarks(currentWord);

                        // Search again in the poems graph.
                        node = GetNodeFromGraph(currentWord, Model.AuxPoemsGraph, Model.PoemsGraph, lastPoemsWords);
                    }

                    if (node == null)
                    {
                        // Word not found. Start with any Last Word.
                        node = lastPoemsWords[Rnd.Next(lastPoemsWords.Count)];
                    }
                }
            }

            if (!isPoeticProse)
            {
                Node lastNode = node;

                string ngram = Model.PoemsGraph.Keys.ElementAt(node.Id);
                int nextIndex = 0;

                while (lineNumber < MAX_LINE) // Entries cannot exceed 30 lines
                {
                    if ((node.IsStartGram) && (lineNumber > MIN_LINE))
                    {
                        lineNumber = MAX_LINE;
                    }

                    if (ngram.IndexOf(LINEFEED) > 0)
                    {
                        lineNumber++;
                    }

                    int ngramLength = ngram.Length - ngram.IndexOf(LINEFEED) - 1;
                    int posPoemLineFeed = poem.IndexOf(LINEFEED);
                    int verseLength = posPoemLineFeed > 0 ? posPoemLineFeed - 1 : poem.Length;

                    // Entries cannot exceed 80 characters per line.
                    if (verseLength + ngramLength > MAX_CHARS_PER_LINE)
                    {
                        poem = ngram + CR_LF + poem;
                        lineNumber++;
                    }
                    else
                    {
                        poem = String.Format("{0} {1}", ngram, poem);
                    }

                    if (node.Links.Count == 0)
                    {
                        // The node has no edges.
                        // Get one of the last words and insert a new line.
                        node = lastPoemsWords[Rnd.Next(lastPoemsWords.Count)];

                        poem = CR_LF + poem;
                        lineNumber++;
                    }
                    else if (node.Links.Count == 1)
                    {
                        // The node has only one edge.
                        nextIndex = node.Links[0];
                        node = Model.PoemsGraph.Values.ElementAt(nextIndex);
                    }
                    else
                    {
                        // The node has more than one edge.
                        nextIndex = node.Links[Rnd.Next(node.Links.Count)];
                        node = Model.PoemsGraph.Values.ElementAt(nextIndex);
                    }

                    ngram = Model.PoemsGraph.Keys.ElementAt(node.Id);
                    lastNode = node;
                }
            }

            string[] verses = poem.Split(NewlineSplitArg, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();

            // Remove first empty verses.
            int firstVerse = 0;
            while (String.IsNullOrWhiteSpace(verses[firstVerse]))
            {
                firstVerse++;
            }

            int versesCount = verses.Count();
            string lastVerse = String.Empty;

            for (int i = firstVerse; i < versesCount; i++)
            {
                // Remove double words.
                verses[i] = RemoveDoubleWords(verses[i], lastVerse);
                lastVerse = verses[i];
            }

            // Get the title.
            string title = verses[firstVerse];

            // Avoid short titles and verses.
            int minLen = 10;
            if (title.Length < minLen)
            {
                title = String.Format("{0} {1}", title, verses[firstVerse + 1]);
            }

            // Remove the ending signs.
            title = title.TrimEnd('.', ',', ';', ':').ToUpper();

            sb.AppendFormat("{0}{1}{2}", title, CR_LF, CR_LF);

            lastVerse = String.Empty;
            string currentVerse = String.Empty;

            for (int i = firstVerse; i < versesCount; i++)
            {
                currentVerse = verses[i].ToLower();

                if ((currentVerse.Length > 0) && (currentVerse.Length < minLen))
                {
                    lastVerse = currentVerse;
                    sb.Append(lastVerse + " ");
                }
                else if ((lastVerse != currentVerse) && (currentVerse.Length > 0))
                {
                    lastVerse = currentVerse;
                    sb.AppendLine(currentVerse);
                }
            }

            poem = sb.ToString().Trim();

            if (!HasPunctuationMark(poem))
            {
                if (poem.EndsWith(CR_LF))
                {
                    poem = poem.Substring(0, poem.Length - 2);
                }

                string sign = PunctuationMarks[Rnd.Next(3)];
                poem = poem + sign;
            }

            return poem;
        }

        private static string Concat(string s1, string s2, string s3 = "")
        {
            if (String.IsNullOrWhiteSpace(s3))
            {
                return String.Format("{0} {1}", s1, s2);
            }

            return String.Format("{0} {1} {2}", s1, s2, s3);
        }

        private static Node GetNode(string ngram, Dictionary<string, Node> graph)
        {
            Node node;

            if (graph.ContainsKey(ngram))
            {
                node = graph[ngram];
            }
            else
            {
                node = new Node(graph.Count);
                graph[ngram] = node;
            }

            return node;
        }

        // Save words of n-gram in the auxiliary dictionary for later fast retrieval.
        private static void InsertNgramInAuxGraph(string ngram, Dictionary<string, AuxNode> auxGraph, int position)
        {
            AuxNode node;

            var words = ngram.Split(TokenSplitArg3, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 0)
            {
                return;
            }

            string lowerWord = LEFT_WORD_PREFIX + words[0].ToLower();

            if (auxGraph.ContainsKey(lowerWord))
            {
                node = auxGraph[lowerWord];
            }
            else
            {
                node = new AuxNode();
                auxGraph[lowerWord] = node;
            }

            node.Links.Add(position);

            if (words.Length > 1)
            {
                lowerWord = RIGHT_WORD_PREFIX + words[words.Length - 1].ToLower();

                if (auxGraph.ContainsKey(lowerWord))
                {
                    node = auxGraph[lowerWord];
                }
                else
                {
                    node = new AuxNode();
                    auxGraph[lowerWord] = node;
                }

                node.Links.Add(position);
            }
        }

        private static string PrepareForTokenize(string text)
        {
            // Remove the title.
            int posFirstNewLine = text.IndexOf(CR_LF + CR_LF);
            int chars = 4;

            if (posFirstNewLine == -1)
            {
                chars = 2;
                posFirstNewLine = text.IndexOf(LINEFEED + LINEFEED);
            }

            if (posFirstNewLine >= 0)
            {
                text = text.Remove(0, posFirstNewLine + chars);
            }

            // Make some cleaning.
            text = CleanText(text);

            // Include space character after each newline for future word spliting in Tokenize().
            // For instance, "sing myself,\r\nAnd what I" will become "sing myself,\r\n And what I".
            text = text.Replace(LINEFEED, LINEFEED + " ");

            return text;
        }

        private static List<string> Tokenize(string text, bool addNewLine = false)
        {
            string[] tokens;
            string[] separator;

            if (addNewLine)
            {
                text = text + CR_LF;
                separator = TokenSplitArg1;
                tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                separator = TokenSplitArg2;
                tokens = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            List<string> result = new List<string>();

            for (int i = 0; i < tokens.Count(); i++)
            {
                if (IsNumber.Match(tokens[i]).Success)
                {
                    continue;
                }

                if ((tokens[i] != CR_LF) && (tokens[i] != LINEFEED))
                {
                    result.Add(tokens[i]);
                }
            }

            return result;
        }

        private static string CleanText(string text)
        {
            text = text.Replace("-", " ").Replace("–", " ").Replace("—", " ").Replace("_", " ");
            text = text.Replace("(", String.Empty).Replace(")", String.Empty);
            text = text.Replace("[", String.Empty).Replace("]", String.Empty);
            text = text.Replace("«", String.Empty).Replace("»", String.Empty);
            text = text.Replace("*", String.Empty).Replace("\"", String.Empty);
            text = text.Replace("'.", ".").Replace("',", ",").Replace("'\r", "\r").Replace("'\n", "\n");
            text = text.Replace("'?", "?").Replace("'!", "!");
            text = text.Replace("\n'", "\n").Replace(" '", " ").Replace("' ", " ").Replace("\t", " ");

            return text;
        }

        private static string RemoveDoubleWords(string text, string lastText)
        {
            StringBuilder sb = new StringBuilder();
            var words = Tokenize(text, false);
            var lastTextWords = Tokenize(lastText, false);
            string lastWord = lastTextWords.Count > 0 ? lastTextWords[lastTextWords.Count - 1].ToLower() : lastText;

            foreach (string word in words)
            {
                string currentWord = word.ToLower();

                if (lastWord != currentWord)
                {
                    sb.AppendFormat(" {0}", word);
                    lastWord = currentWord;
                }
            }

            return sb.ToString().Trim(' ');
        }

        public static string RemovePunctuationMarks(string text)
        {
            while ((text.Length > 0) && HasPunctuationMark(text))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }
    }
}