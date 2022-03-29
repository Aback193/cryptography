using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DocumentFinder
{
    public class SearchResults
    {
        public string FilePath { get; set; }

        public string FileName { get; set; }

        public List<string> WordsFound { get; set; }

        public string Display { get; set; }

        public ImageSource Image { get; set; }

        public string OriginalPath { get; set; }

        public SearchResults()
        {
            WordsFound = new List<string>();
            OriginalPath = "";
        }
    }
}
