using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory
{
    public static class ReorganizableCollectionUtilities<T> where T : class
    {
        public static bool MoveUp(ObservableCollection<T> source, T item)
        {
            return MoveUp(source, item, null, null);
        }

        public static bool MoveUp(ObservableCollection<T> source, T item, Func<T> getSelected, Action<T> setSelected)
        {
            return Move(source, item, -1, getSelected, setSelected);
        }

        public static bool MoveDown(ObservableCollection<T> source, T item)
        {
            return MoveDown(source, item, null, null);
        }

        public static bool MoveDown(ObservableCollection<T> source, T item, Func<T> getSelected, Action<T> setSelected)
        {
            return Move(source, item, +1, getSelected, setSelected);
        }

        private static bool Move(ObservableCollection<T> source, T item, int delta, Func<T> getSelected, Action<T> setSelected)
        {
            if (item == null)
                return false;

            int index = source.IndexOf(item);
            if (index + delta < 0 || index + delta > source.Count - 1)
                return false;

            T selected = null;

            if (getSelected != null && setSelected != null)
                selected = getSelected();

            source.RemoveAt(index);
            source.Insert(index + delta, item);

            if (getSelected != null && setSelected != null)
                setSelected(selected);

            return true;
        }

        public static bool Remove(ObservableCollection<T> source, T item)
        {
            return Remove(source, item, null, null);
        }

        public static bool Remove(ObservableCollection<T> source, T item, Predicate<T> isSelected, Action clearSelection)
        {
            if (item == null)
                return false;

            int index = source.IndexOf(item);
            if (index < 0 || index > source.Count - 1)
                return false;

            if (isSelected != null && clearSelection != null)
            {
                if (isSelected(item))
                    clearSelection();
            }

            source.RemoveAt(index);

            return true;
        }
    }
}
