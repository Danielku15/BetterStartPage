using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace BetterStartPage.View
{
    internal class GridFileDragAndDropDecorator : Decorator
    {
        private DragAdorner _itemAdorner;

        public static readonly DependencyProperty DefaultDropEffectProperty = DependencyProperty.Register(
            "DefaultDropEffect", typeof(DragDropEffects), typeof(GridFileDragAndDropDecorator), new PropertyMetadata(default(DragDropEffects)));

        public DragDropEffects DefaultDropEffect
        {
            get => (DragDropEffects)GetValue(DefaultDropEffectProperty);
            set => SetValue(DefaultDropEffectProperty, value);
        }

        public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.Register(
            "DataTemplate", typeof(DataTemplate), typeof(GridFileDragAndDropDecorator), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate DataTemplate
        {
            get => (DataTemplate)GetValue(DataTemplateProperty);
            set => SetValue(DataTemplateProperty, value);
        }

        public static readonly DependencyProperty FilesDroppedCommandProperty = DependencyProperty.Register(
            "FilesDroppedCommand", typeof(ICommand), typeof(GridFileDragAndDropDecorator), new PropertyMetadata(default(ICommand)));

        public ICommand FilesDroppedCommand
        {
            get => (ICommand)GetValue(FilesDroppedCommandProperty);
            set => SetValue(FilesDroppedCommandProperty, value);
        }

        public static readonly DependencyProperty FilesDroppedCommandParameterProperty = DependencyProperty.Register(
            "FilesDroppedCommandParameter", typeof(object), typeof(GridFileDragAndDropDecorator), new PropertyMetadata(default(object)));

        public object FilesDroppedCommandParameter
        {
            get => GetValue(FilesDroppedCommandParameterProperty);
            set => SetValue(FilesDroppedCommandParameterProperty, value);
        }

        public GridFileDragAndDropDecorator()
        {
            Loaded += OnLoaded;
        }

        protected UIElement DecoratedUIElement => Child is GridFileDragAndDropDecorator decorator ? decorator.DecoratedUIElement : Child;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(DecoratedUIElement is Grid))
            {
                throw new InvalidCastException(string.Format("GridFileDragAndDropDecorator cannot have child of type {0}", Child.GetType()));
            }
            var grid = (Grid)DecoratedUIElement;
            var allowDropBinding = new Binding("AllowDrop")
            {
                Source = this
            };
            grid.SetBinding(AllowDropProperty, allowDropBinding);
            grid.PreviewDrop += OnPreviewDrop;
            grid.PreviewQueryContinueDrag += OnPreviewQueryContinueDrag;
            grid.PreviewDragEnter += OnPreviewDragEnter;
            grid.PreviewDragOver += OnPreviewDragOver;
            grid.PreviewDragLeave += OnPreviewDragLeave;
        }

        private void OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            if (!AllowDrop) return;
            DetachDragAdorner();
            e.Handled = true;
        }

        private void OnPreviewDragEnter(object sender, DragEventArgs e)
        {
            if (!AllowDrop) return;

            var grid = (Grid)sender;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = (string[])e.Data.GetData(DataFormats.FileDrop);
                InitializeDragAdorner(grid, data);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var data = e.Data.GetData(DataFormats.Text)
                    ?.ToString()
                    .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                InitializeDragAdorner(grid, data);
            }
            e.Handled = true;
        }

        private void OnPreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (!AllowDrop) return;
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                ResetState((Grid)sender);
                DetachDragAdorner();
                e.Handled = true;
            }
        }

        private void OnPreviewDrop(object sender, DragEventArgs e)
        {
            if (!AllowDrop) return;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var itemsToAdd = (string[])e.Data.GetData(DataFormats.FileDrop);
                e.Effects = DefaultDropEffect;

                var data = new FilesDroppedEventArgs(FilesDroppedCommandParameter, itemsToAdd);
                if (FilesDroppedCommand != null && FilesDroppedCommand.CanExecute(data))
                {
                    FilesDroppedCommand.Execute(data);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var itemsToAdd = e.Data.GetData(DataFormats.Text).ToString()
                                    .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                e.Effects = DefaultDropEffect;

                var data = new FilesDroppedEventArgs(FilesDroppedCommandParameter, itemsToAdd);
                if (FilesDroppedCommand != null && FilesDroppedCommand.CanExecute(data))
                {
                    FilesDroppedCommand.Execute(data);
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            DetachDragAdorner();
            e.Handled = true;
        }

        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            if (!AllowDrop) return;
            e.Handled = true;
        }

        private void InitializeDragAdorner(Grid grid, object dragData)
        {
            if (DataTemplate == null || dragData == null || _itemAdorner != null) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(grid);
            _itemAdorner = new DragAdorner(dragData, DataTemplate,
                grid, adornerLayer);
        }

        private void DetachDragAdorner()
        {
            if (_itemAdorner != null)
            {
                _itemAdorner.Destroy();
                _itemAdorner = null;
            }
        }

        private void ResetState(Grid grid)
        {
            grid.AllowDrop = AllowDrop;
        }
    }
}
