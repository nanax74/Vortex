using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Vortex.ViewModels
{
    public static class TabDragBehavior
    {
        public static readonly AttachedProperty<bool> IsDraggableProperty =
            AvaloniaProperty.RegisterAttached<TabControl, Control, bool>("IsDraggable");

        public static void SetIsDraggable(AvaloniaObject element, bool value) =>
            element.SetValue(IsDraggableProperty, value);

        public static bool GetIsDraggable(AvaloniaObject element) =>
            element.GetValue(IsDraggableProperty);

        static TabDragBehavior()
        {
            IsDraggableProperty.Changed.AddClassHandler<TabControl>((tabControl, e) =>
            {
                if (e.NewValue is bool isDraggable && isDraggable)
                {
                    tabControl.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
                    tabControl.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble);
                }
            });
        }

        private static Point _startPoint;
        private static object? _draggedItem;

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            Debug.WriteLine("AAAAAAAAA");
            if (sender is TabControl tabControl && tabControl.SelectedItem != null)
            {
                _startPoint = e.GetPosition(tabControl);
                _draggedItem = tabControl.SelectedItem;
                Debug.WriteLine($"_draggedItem set: {_draggedItem} (Type={_draggedItem?.GetType().Name})");
            }
            else
            {
                Debug.WriteLine("PointerPressed: sender is not TabControl or SelectedItem is null");
            }
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            Debug.WriteLine($"PointerReleased fired! sender={sender?.GetType().Name}, Source={e.Source?.GetType().Name}");
            if (_draggedItem == null)
            {
                Debug.WriteLine("_draggedItem is null, nothing to do");
                return;
            }

            // TabControl を取得
            TabControl? tabControl = null;
            if (sender is TabControl tc)
                tabControl = tc;
            else if (e.Source is Control c)
                tabControl = FindParent<TabControl>(c);

            if (tabControl == null)
                Debug.WriteLine("No parent TabControl found");

            var pos = e.GetPosition(tabControl);
            Debug.WriteLine($"PointerReleased position={pos}, start={_startPoint}, dragged={_draggedItem}");
        }

        private static void OpenNewWindow(object item, IDataTemplate? itemTemplate, IDataTemplate? contentTemplate)
        {
            var window = new Window
            {
                Title = item.ToString() ?? "New Tab",
                Width = 400,
                Height = 300
            };

            var newItems = new ObservableCollection<object> { item };

            var newTabControl = new TabControl
            {
                ItemsSource = newItems,
                ItemTemplate = itemTemplate,
                ContentTemplate = contentTemplate
            };

            window.Content = newTabControl;
            window.Show();
        }

        private static void RemoveTab(TabControl tabControl, object item)
        {
            if (tabControl.ItemsSource is IList list && list.Contains(item))
            {
                list.Remove(item);
            }
        }

        private static T? FindParent<T>(Control? c) where T : Control
        {
            while (c != null && c is not T)
                c = c.Parent as Control;
            return c as T;
        }
    }
}
