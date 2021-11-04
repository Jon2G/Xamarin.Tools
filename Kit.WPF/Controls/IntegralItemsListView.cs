using System.Windows;
using System.Windows.Controls;

namespace Kit.WPF.Controls
{
    public sealed class IntegralItemsListView : ListView
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            double height = 0;
            if (Items != null)
            {
                for (int i = 0; i < Items.Count; ++i)
                {
                    UIElement itemContainer = (UIElement)ItemContainerGenerator.ContainerFromIndex(i);
                    if (itemContainer == null)
                    {
                        break;
                    }

                    itemContainer.Measure(availableSize);
                    double childHeight = itemContainer.DesiredSize.Height;
                    if (height + childHeight > size.Height)
                    {
                        break;
                    }

                    height += childHeight;
                }
            }

            size.Height = height;
            return size;
        }
    }
}
