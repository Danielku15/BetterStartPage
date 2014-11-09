using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace BetterStartPage.Control
{
    class GridFileDragAndDropDecorator : BaseDecorator
    {
        private bool _isMouseDown;
        private bool _isDragging;
        private DragAdorner _itemAdorner;

        public static readonly DependencyProperty DefaultDropEffectProperty = DependencyProperty.Register(
            "DefaultDropEffect", typeof(DragDropEffects), typeof(GridFileDragAndDropDecorator), new PropertyMetadata(default(DragDropEffects)));

        public DragDropEffects DefaultDropEffect
        {
            get { return (DragDropEffects)GetValue(DefaultDropEffectProperty); }
            set { SetValue(DefaultDropEffectProperty, value); }
        }

        public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.Register(
            "DataTemplate", typeof (DataTemplate), typeof (GridFileDragAndDropDecorator), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate DataTemplate
        {
            get { return (DataTemplate) GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        public static readonly DependencyProperty FilesDroppedCommandProperty = DependencyProperty.Register(
            "FilesDroppedCommand", typeof (ICommand), typeof (GridFileDragAndDropDecorator), new PropertyMetadata(default(ICommand)));

        public ICommand FilesDroppedCommand
        {
            get { return (ICommand) GetValue(FilesDroppedCommandProperty); }
            set { SetValue(FilesDroppedCommandProperty, value); }
        }

        public static readonly DependencyProperty FilesDroppedCommandParameterProperty = DependencyProperty.Register(
            "FilesDroppedCommandParameter", typeof (object), typeof (GridFileDragAndDropDecorator), new PropertyMetadata(default(object)));

        public object FilesDroppedCommandParameter
        {
            get { return (object) GetValue(FilesDroppedCommandParameterProperty); }
            set { SetValue(FilesDroppedCommandParameterProperty, value); }
        }

        public GridFileDragAndDropDecorator()
            : base()
        {
            _isMouseDown = false;
            _isDragging = false;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!(base.DecoratedUIElement is Grid))
            {
                throw new InvalidCastException(string.Format("GridFileDragAndDropDecorator cannot have child of type {0}", Child.GetType()));
            }
            var grid = (Grid)DecoratedUIElement;
            var allowDropBinding = new Binding("AllowDrop");
            allowDropBinding.Source = this;
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
            if (!AllowDrop) return; var grid = (Grid)sender;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = (string[]) e.Data.GetData(DataFormats.FileDrop);
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
            if (DataTemplate != null)
            {
                if (_itemAdorner == null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(grid);
                    if (grid != null)
                    {
                        _itemAdorner = new DragAdorner(dragData, DataTemplate,
                                grid, adornerLayer);

                    }
                }
            }
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
            _isMouseDown = false;
            _isDragging = false;
            grid.AllowDrop = AllowDrop;
        }
    }
}
