using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentFinder
{
    public class SearchResults
    {
        public string FilePath { get; set; }

        public List<string> WordsFound { get; set; }

        public SearchResults()
        {
            WordsFound = new List<string>();
        }
    }
}
