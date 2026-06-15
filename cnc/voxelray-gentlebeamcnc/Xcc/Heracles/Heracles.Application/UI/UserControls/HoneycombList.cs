using Heracles.Application.Models;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Heracles.Application.UI.UserControls
{
    public class HoneycombList : ListBox
    {
        #region Constructors
        public HoneycombList()
        {
            Loaded += (s, e) => UpdateItemsSource();
        }
        static HoneycombList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HoneycombList), new FrameworkPropertyMetadata(typeof(HoneycombList)));
        }
        #endregion Constructors


        #region Properties
        bool SelectionChangedFromBinding { get; set; } = false;
        #endregion Properties


        #region Private methods
        private void UpdateItemsSource()
        {
            IsEnabled = TargetType != TargetType.TargetType_None;

            ItemsSource = TreatmentField.GetTreatmentFieldCollection(TargetType);
                UpdateActiveTreatmentFields();

            InvalidateArrange();
        }

        private void ArrangeItems(Size finalSize)
        {
            if (Items is null || Items.Count == 0)
                return;

            switch (TargetType)
            {
                case TargetType.TargetType_None:
                    break;
                case TargetType.TargetType_61_Fields:
                    {
                        ArrangeHoneycombCollimator(finalSize, 9, 9);
                        break;
                    }
                case TargetType.TargetType_50mm_SSD_13_Fields:
                    {
                        ArrangeTargetType13CellsCentralLarge(finalSize);
                        break;
                    }
                case TargetType.TargetType_50mm_SSD_15mm_Field:
                {
                    ArrangeTargetTypeCircular(finalSize, sizeMultiplier: 0.25);
                    break;
                }
                case TargetType.TargetType_50mm_SSD_20mm_Field:
                {
                    ArrangeTargetTypeCircular(finalSize, sizeMultiplier: 0.33);
                    break;
                }
                case TargetType.TargetType_50mm_SSD_30mm_Field:
                {
                    ArrangeTargetTypeCircular(finalSize, sizeMultiplier: 0.5);
                    break;
                }
                case TargetType.TargetType_50mm_SSD_40mm_Field:
                {
                    ArrangeTargetTypeCircular(finalSize, sizeMultiplier: 0.66);
                    break;
                }
                case TargetType.TargetType_50mm_SSD_50mm_Field:
                {
                    ArrangeTargetTypeCircular(finalSize, sizeMultiplier: 0.83);
                    break;
                }
                case TargetType.TargetType_30mm_SSD_7_Fields:
                    ArrangeHoneycombCollimator(finalSize, 3, 3);
                    break;
            }
        }

        private void ArrangeHoneycombCollimator(Size finalSize, int rowCount, int columnCount)
        {
            //honeycomb size
            var radius = rowCount / 2;
            var cellMargin = new Point(2, 2); // margin between hexagonal cells

            // calculates size of the honeycomb cell and offset of honeycomb
            var honey = GetPossibleSize(finalSize, rowCount, columnCount, cellMargin);
            Size size = honey.Item1;
            Point offset = honey.Item2;

            int index = 0;
            for (var row = 0; row < rowCount; row++)
            {
                // honeycomb pattern calculations
                var emptyCount = Math.Abs(row - radius); // number of empty cells in the row
                var columnCellCount = columnCount - emptyCount; // number of filled cells in the row

                var startColumn = emptyCount / 2; // start index of the filled cell in the row

                // start index correction based on oddness
                if (IsOdd(emptyCount) && !IsOdd(radius + 1))
                    startColumn++;

                // vertical offset of each cell in a row
                var verticalOffset = -1 * row * size.Height / 4 + cellMargin.Y * row;

                for (var column = startColumn; column < startColumn + columnCellCount; column++)
                {
                    if (this.ItemContainerGenerator.ContainerFromIndex(index) is not ListBoxItem container)
                        return;

                    if (container.Content is not IHasTreatmentFieldName)
                        throw new Exception("Only items of type ITreatmentField allowed for this list.");

                    double horisontalOffset;
                    if (IsOdd(radius))
                    {
                        //if honeycomb radius is odd, every even row should be moved left
                        horisontalOffset = row % 2 == 0 ? -(size.Width / 2 + cellMargin.X / 2) : 0;
                    }
                    else
                    {
                        //if honeycomb radius is even, every odd row should be moved right
                        horisontalOffset = row % 2 != 0 ? size.Width / 2 + cellMargin.X / 2 : 0;
                    }
                    horisontalOffset += cellMargin.X * column;

                    // sets the size of the hexagon cell
                    container.Width = size.Width;
                    container.Height = size.Height;
                    container.FontSize = Math.Max(Math.Min(container.Width, container.Height) / 2.2, 0.1);

                    // positions the cell on the panel
                    Canvas.SetLeft(container, column * size.Width + horisontalOffset + offset.X);
                    Canvas.SetTop(container, row * size.Height + verticalOffset + offset.Y);

                    //container.Arrange(new Rect(cols * size.Width + colummOffset, rows * size.Height + rowOffset, size.Width, size.Height));

                    index++;
                }
            }
        }

        private void ArrangeTargetType13CellsCentralLarge(Size finalSize)
        {
            //honeycomb size
            var rowCount = 5;
            var columnCount = 5;
            var radius = rowCount / 2;
            var cellMargin = new Point(2, 2); // margin between hexagonal cells

            // calculates size of the honeycomb cell
            var honey = GetPossibleSize(finalSize, rowCount, columnCount, cellMargin);
            Size size = honey.Item1;
            Point offset = honey.Item2;

            int index = 0;
            for (var row = 0; row < rowCount; row++)
            {
                // honeycomb pattern calculations
                var emptyCount = Math.Abs(row - radius); // number of empty cells in the row
                var columnCellCount = columnCount - emptyCount; // number of filled cells in the row

                var startColumn = emptyCount / 2; // start index of the filled cell in the row

                // start index correction based on oddness
                if (IsOdd(emptyCount) && !IsOdd(radius + 1))
                    startColumn++;

                for (var column = startColumn; column < startColumn + columnCellCount; column++)
                {
                    if (this.ItemContainerGenerator.ContainerFromIndex(index) is not ListBoxItem container)
                        return;

                    if (container.Content is not IHasTreatmentFieldName treatmentField)
                        throw new Exception("Only items of type ITreatmentField allowed for this list.");

                    // skips all cells where there should be a large central cell
                    if (row > 0 && row < rowCount - 1)
                    {
                        if (column > startColumn && column < startColumn + columnCellCount - 1)
                        {
                            if (treatmentField.Name != TreatmentFieldName.PlusC)
                            {
                                continue;
                            }
                        }
                    }

                    // horisontal offset of each cell in a row
                    double horisontalOffset;
                    if (IsOdd(radius))
                    {
                        //if honeycomb radius is odd, every even row should be moved left
                        horisontalOffset = row % 2 == 0 ? -(size.Width / 2 + cellMargin.X / 2) : 0;
                    }
                    else
                    {
                        //if honeycomb radius is even, every odd row should be moved right
                        horisontalOffset = row % 2 != 0 ? size.Width / 2 + cellMargin.X / 2 : 0;
                    }
                    horisontalOffset += cellMargin.X * column;

                    if (treatmentField.Name == TreatmentFieldName.PlusC)
                    {
                        // sets the size of the large central cell 
                        container.Width = size.Width * 3 + 2 * cellMargin.X;
                        container.Height = size.Height * 2.5 + 2 * cellMargin.Y;
                        container.FontSize = Math.Max(Math.Min(container.Width, container.Height) / 6.0, 0.1);

                        // vertical offset of the large central cell
                        var verticalOffset = -1 * container.Height / 2 + cellMargin.Y * row;

                        // positions the cell on the panel
                        Canvas.SetLeft(container, column * size.Width + horisontalOffset + offset.X);
                        Canvas.SetTop(container, row * size.Height + verticalOffset + offset.Y);

                        // places the cell above the other cells
                        Panel.SetZIndex(container, 10);

                        //container.Arrange(new Rect(cols * size.Width, rows * size.Height + rowOffset, size.Width * 3, size.Height * 2.5));
                    }
                    else
                    {
                        // sets the size of the hexagon cell 
                        container.Width = size.Width;
                        container.Height = size.Height;
                        container.FontSize = Math.Max(Math.Min(container.Width, container.Height) / 3, 0.1);

                        // vertical offset of each cell in a row
                        var verticalOffset = -1 * row * size.Height / 4 + cellMargin.Y * row;

                        // positions the cell on the panel
                        Canvas.SetLeft(container, column * size.Width + horisontalOffset + offset.X);
                        Canvas.SetTop(container, row * size.Height + verticalOffset + offset.Y);

                        //container.Arrange(new Rect(cols * size.Width + colummOffset, rows * size.Height + rowOffset, size.Width, size.Height));
                    }

                    index++;
                }
            }
        }

        private void ArrangeTargetTypeCircular(Size finalSize, double sizeMultiplier)
        {
            //honeycomb size
            var rowCount = 1;
            var columnCount = 1;
            var cellMargin = new Point(0, 0); // margin between hexagonal cells

            // calculates size of the honeycomb cell
            var honey = GetPossibleSize(finalSize, rowCount, columnCount, cellMargin);
            Size size = honey.Item1;

            if (this.ItemContainerGenerator.ContainerFromIndex(0) is not ListBoxItem container)
                return;

            var maximumSize = Math.Max(size.Width, size.Height);
            container.Width = maximumSize * sizeMultiplier;
            container.Height = maximumSize * sizeMultiplier;
            container.FontSize = Math.Max(Math.Min(container.Width, container.Height) / 7, 0.1);

            Canvas.SetLeft(container, finalSize.Width / 2 - container.Width / 2);
            Canvas.SetTop(container, finalSize.Height / 2 - container.Height / 2);
        }

        private void UpdateSelectedItems()
        {
            SelectionChangedFromBinding = true;

            //SelectedItems.Clear();
            var itemsToSelect = new List<IHasTreatmentFieldName>();

            if (SelectedTargets is null)
            {
                SelectionChangedFromBinding = false;
                return;
            }

            foreach (var selectedTarget in SelectedTargets.Cast<IHasTreatmentFieldName>())
            {
                foreach (var item in Items)
                {
                    if (item is not IHasTreatmentFieldName target)
                        throw new Exception("Only items of type ITreatmentField allowed for this list.");

                    if (selectedTarget.Name == target.Name)
                        //SelectedItems.Add(item);
                        itemsToSelect.Add(item as IHasTreatmentFieldName);
                }
            }
            SetSelectedItems(itemsToSelect);

            SelectionChangedFromBinding = false;
        }

        private static int GetCellsCount(int radius) => 3 * radius * radius - 3 * radius + 1; // honeycomb cells count formula 3r² - 3r + 1

        private static bool IsOdd(int number) => number % 2 != 0;

        private static (Size, Point) GetPossibleSize(Size size, int rowCount, int columnCount, Point cellMargin)
        {
            var eatenWidth = cellMargin.X * (columnCount - 1); // width 'eaten' by horizontal margin
            var eatenHeight = cellMargin.Y * (rowCount - 1); // height 'eaten' by vertical margin
            var availableWidth = size.Width - eatenWidth;
            if (availableWidth < 0) throw new Exception("Horizontal cell margin is too big. The width of a hexagon cannot be negative.");

            var availableHeight = size.Height - eatenHeight;
            if (availableHeight < 0) throw new Exception("Vertical cell margin is too big. The height of a hexagon cannot be negative.");

            double hexagonRatio = Math.Sqrt(3) / 2; // the width of the vertical-oriented hexagon with height equal to 1
            int subRows = 3 * rowCount + 1; // number of honeycomb subrows
            int subColumns = 2 * columnCount + (1 - columnCount % 2); // number of honeycomb subcolumns

            double normalizedHoneycombWidth = hexagonRatio * columnCount;
            double normalizedHoneycombHeight = subRows / 4d;
            double honeycombRatio = normalizedHoneycombHeight / normalizedHoneycombWidth;

            if (availableHeight / availableWidth < honeycombRatio)
            {
                double h = 4 * (availableHeight / subRows); // final height of hexagonal cell

                var hexagonSize = new Size(hexagonRatio * h, h);
                var honeycombOffset = new Point((size.Width - size.Height / honeycombRatio) / 2, 0); // offset, to place honeycomb into the center of the control

                return (hexagonSize, honeycombOffset);
            }
            else
            {
                double w = 2 * (availableWidth / subColumns); // final width of hexagonal cell

                var hexagonSize = new Size(w, w / hexagonRatio);
                var honeycombOffset = new Point(0, (size.Height - size.Width * honeycombRatio) / 2); // offset, to place honeycomb into the center of the control

                return (hexagonSize, honeycombOffset);
            }
        }

        private void ActiveTreatmentFieldsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var collimatorCells = TreatmentField.GetTreatmentFieldCollection(TargetType);

            // applicator doesn't exist yet
            if (collimatorCells.Count < 1)
                return;

            // collection cleared
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var cell in collimatorCells)
                    cell.IsActive = false;
            }

            // items removed from collection 
            if (e.OldItems is not null)
            {
                foreach (var treatmentField in e.OldItems.Cast<IHasTreatmentFieldName>())
                {
                    var collimatorCell = collimatorCells.FirstOrDefault(cell => cell.Name == treatmentField.Name);

                    if (collimatorCell is not null)
                    {
                        collimatorCell.IsActive = false;
                    }
                }
            }

            // items added to collection
            if (e.NewItems is not null)
            {
                foreach (var treatmentField in e.NewItems.Cast<IHasTreatmentFieldName>())
                {
                    var collimatorCell = collimatorCells.FirstOrDefault(cell => cell.Name == treatmentField.Name);

                    if (collimatorCell is not null)
                    {
                        collimatorCell.IsActive = true;
                    }
                }
            }
        }

        private void ClearActiveTreatmentFields()
        {
            var collimatorCells = TreatmentField.GetTreatmentFieldCollection(TargetType);

            foreach (var cell in collimatorCells)
                cell.IsActive = false;
        }

        private void UpdateActiveTreatmentFields()
        {
            var collimatorCells = TreatmentField.GetTreatmentFieldCollection(TargetType);

            ClearActiveTreatmentFields();

            if (ActiveTreatmentFields is null)
                return;


            foreach (var cell in collimatorCells)
            {
                foreach (var treatmentField in ActiveTreatmentFields)
                {
                    if (cell.Name == treatmentField.Name)
                    {
                        cell.IsActive = true;
                    }
                }
            }
        }
        #endregion Private methods


        #region ListBox overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            ArrangeItems(size);
            return size;
        }

        //protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        //{
        //    base.OnSelectionChanged(e);

        //    if (SelectionMode == SelectionMode.Single)
        //        throw new NotSupportedException("SelectionMode.Single is not supported for this control.");

        //    if (SelectionChangedFromBinding)
        //        return;

        //    if (SelectedItems.Count == 0)
        //    {
        //        SelectedTargets = null;
        //    }
        //    else
        //    {
        //        var selectedTargets = new List<ITreatmentField>();

        //        foreach (ITreatmentField item in this.SelectedItems)
        //            selectedTargets.Add(item);

        //        SelectedTargets = selectedTargets;
        //    }
        //}
        #endregion ListBox overrides


        #region Dependency properties
        public IList<ITreatmentFieldEntry> SelectedTargets
        {
            get => (IList<ITreatmentFieldEntry>)GetValue(SelectedTargetsProperty);
            set => SetValue(SelectedTargetsProperty, value);
        }

        public static readonly DependencyProperty SelectedTargetsProperty =
            DependencyProperty.Register(
                "SelectedTargets",
                typeof(IList<ITreatmentFieldEntry>),
                typeof(HoneycombList),
                new FrameworkPropertyMetadata(
                    default,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (obj, e) =>
                    {
                        (obj as HoneycombList).UpdateSelectedItems();
                    }));

        public TargetType TargetType
        {
            get => (TargetType)GetValue(TargetTypeProperty);
            set => SetValue(TargetTypeProperty, value);
        }

        public static readonly DependencyProperty TargetTypeProperty =
            DependencyProperty.Register(
                "TargetType",
                typeof(TargetType),
                typeof(HoneycombList),
                new FrameworkPropertyMetadata(
                    TargetType.TargetType_None,
                    (obj, e) =>
                    {
                        (obj as HoneycombList).UpdateItemsSource();
                    }));

        public IEnumerable<IHasTreatmentFieldName> ActiveTreatmentFields
        {
            get => (IEnumerable<IHasTreatmentFieldName>)GetValue(ActiveTreatmentFieldsProperty);
            set => SetValue(ActiveTreatmentFieldsProperty, value);
        }

        public static readonly DependencyProperty ActiveTreatmentFieldsProperty =
            DependencyProperty.Register(
                "ActiveTreatmentFields",
                typeof(IEnumerable<IHasTreatmentFieldName>),
                typeof(HoneycombList),
                new FrameworkPropertyMetadata(
                    default,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (obj, e) =>
                    {
                        if (obj is HoneycombList control)
                        {
                            if (e.NewValue is INotifyCollectionChanged newActualFields)
                            {
                                newActualFields.CollectionChanged += control.ActiveTreatmentFieldsChanged;
                            }

                            if (e.OldValue is INotifyCollectionChanged oldActualFields)
                            {
                                oldActualFields.CollectionChanged -= control.ActiveTreatmentFieldsChanged;
                            }

                            control.ClearActiveTreatmentFields();
                            control.UpdateActiveTreatmentFields();
                        }
                    }));
        #endregion Dependency properties
    }
}
