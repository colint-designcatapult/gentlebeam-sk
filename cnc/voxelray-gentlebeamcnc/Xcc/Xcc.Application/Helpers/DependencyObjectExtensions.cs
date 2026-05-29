using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Xcc.Application.Helpers
{
    public static partial class DependencyObjectExtensions
    {
        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="name">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter or null if not found</returns> 
        public static T? GetChildOfType<T>(this DependencyObject parent, string? name = null, bool isFirstEntry = true) where T : DependencyObject
        {
            if (parent is null)
                return null;

            T? childAsT = null;

            if (parent is T t && !isFirstEntry)
            {
                if (string.IsNullOrEmpty(name) || parent is FrameworkElement frameworkElement && frameworkElement.Name == name)
                    childAsT = t;
            }

            isFirstEntry = false;

            if (childAsT is null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                    childAsT = GetChildOfType<T>(child, name, isFirstEntry);

                    if(childAsT is not null)
                        return childAsT;
                }
            }

            return childAsT;
        }


        /// <summary>
        /// Finds all Childs of a given item in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried items.</param>
        /// <typeparam name="T">The type of the queried items.</typeparam>
        /// <returns>Sequence of items that matches the submitted type parameter or empty sequence.</returns> 
        public static IEnumerable<T> GetChildsOfType<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield return (T)Enumerable.Empty<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is null)
                    continue;

                if (child is T t)
                    yield return t;

                foreach (T childOfChild in GetChildsOfType<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
