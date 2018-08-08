using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory
{
    public struct SearchInfo
    {
        public bool IsExact;
        public string Text;
    }

    public class SearchText
    {
        public SearchText(string searchText)
        {
            if (searchText == null)
                throw new ArgumentNullException(nameof(searchText));

            foreach (string sub in searchText.Split(',', ';', '/', ':').Select(x => x.Trim().ToLower()))
            {

            }
        }
    }

    public class SearchUtilities
    {
    }
}
