using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly List<SearchInfo> searchInfo = new List<SearchInfo>();

        public static SearchStatement Create(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return null;

            return new SearchStatement(searchText);
        }

        public SearchStatement(string searchText)
        {
            if (searchText == null)
            {
                IsEmpty = true;
                return;
            }

            foreach (string sub in searchText.Split(',', ';', '/', ':'))
            {
                string subText = sub.Trim();

                if (subText.Length == 0)
                    continue;

                subText = subText.ToLower();

                bool isExact = subText.StartsWith("=");

                if (isExact)
                    subText = subText.Substring(1).TrimStart();

                searchInfo.Add(new SearchInfo(isExact, subText));
            }

            IsEmpty = searchInfo.Count == 0;
        }

        public bool IsEmpty { get; }

        public bool IsMatching(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            string textToLower = text.Trim().ToLower();

            foreach (SearchInfo si in searchInfo)
            {
                if (si.IsExact)
                {
                    if (textToLower == si.Text)
                        return true;
                }
                else if (textToLower.Contains(si.Text))
                    return true;
            }

            return false;
        }
    }
}
