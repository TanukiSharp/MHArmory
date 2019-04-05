using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MHArmory
{
    public struct SearchInfo
    {
        public readonly bool IsExact;
        public readonly string Text;

        public SearchInfo(bool isExact, string text)
        {
            IsExact = isExact;
            Text = text;
        }

        public bool IsMatching(string textToLower)
        {
            if (IsExact)
            {
                if (textToLower == Text)
                    return true;
            }
            else if (textToLower.Contains(Text))
                return true;

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is SearchInfo si)
                return si.IsExact == IsExact && si.Text == Text;
            return false;
        }

        public override int GetHashCode()
        {
            return $"{IsExact}:{Text}".GetHashCode();
        }

        public static bool operator ==(SearchInfo left, SearchInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SearchInfo left, SearchInfo right)
        {
            return !(left == right);
        }
    }

    public class SearchStatement
    {
        public readonly ReadOnlyCollection<SearchInfo> SearchInfo;

        public static SearchStatement Create(string searchText, IDictionary<string, string> aliases)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return null;

            return new SearchStatement(searchText, aliases);
        }

        private static readonly char[] searchInfoSeparators = new[] { ',', ';', '/', ':' };

        public SearchStatement(string searchText, IDictionary<string, string> aliases)
        {
            if (searchText == null)
            {
                IsEmpty = true;
                return;
            }

            var searchInfo = new List<SearchInfo>();

            foreach (string sub in searchText.Split(searchInfoSeparators))
            {
                string subText = sub.Trim();

                if (subText.Length == 0)
                    continue;

                subText = subText.ToLower();

                bool isExact = subText.StartsWith("=");

                if (isExact)
                    subText = subText.Substring(1).TrimStart();

                if (aliases != null)
                {
                    foreach (string key in aliases.Keys)
                    {
                        if (subText.Contains(key))
                            subText = Regex.Replace(subText, $"\\b{Regex.Escape(key)}\\b", aliases[key]);
                    }
                }

                searchInfo.Add(new SearchInfo(isExact, subText));
            }

            IsEmpty = searchInfo.Count == 0;

            SearchInfo = new ReadOnlyCollection<SearchInfo>(searchInfo);
        }

        public bool IsEmpty { get; }

        public bool IsMatching(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            string textToLower = text.Trim().ToLower();

            foreach (SearchInfo si in SearchInfo)
            {
                if (si.IsMatching(textToLower))
                    return true;
            }

            return false;
        }
    }
}
