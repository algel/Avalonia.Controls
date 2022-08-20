namespace Algel.Avalonia.Controls
{
    using System;

    using global::Avalonia;
    using global::Avalonia.Controls;

    using JetBrains.Annotations;

    public class StackGrid : Grid
    {
        #region Fields

        /// <summary>
        /// Sign line feed. I.e. even if the current row is not filled cells, then the following items need to be placed on a new line
        /// </summary>
        public static readonly AttachedProperty<bool> IsRowBreakProperty = AvaloniaProperty.RegisterAttached<StackGrid, Control, bool>("IsRowBreak");

        /// <summary>
        /// To disable the automatic arrangement in lines and columns
        /// </summary>
        public static readonly AttachedProperty<bool> DisableAutoAllocationProperty = AvaloniaProperty.RegisterAttached<StackGrid, Control, bool>("DisableAutoAllocation");

        /// <summary>
        /// To put ColumnSpan in such a way that the element is stretched up to the rightmost column
        /// </summary>
        public static readonly AttachedProperty<bool> StretchToLastColumnProperty = AvaloniaProperty.RegisterAttached<StackGrid, Control, bool>("StretchToLastColumn");


        /// <summary>
        /// Optimization for skip already allocated childs
        /// </summary>
        private static readonly AttachedProperty<bool> IsAutoAllocatedProperty = AvaloniaProperty.RegisterAttached<StackGrid, Control, bool>("IsAutoAllocated");

        /// <summary>
        /// Identifies the <see cref="AutoGenerateRows"/> dependency property.
        /// </summary>
        public static readonly StyledProperty<bool> AutoGenerateRowsProperty = AvaloniaProperty.Register<StackGrid, bool>(nameof(AutoGenerateRows));

        #endregion

        #region Constructor

        #endregion

        #region Method

        /// <inheritdoc />
        protected override Size MeasureOverride(Size constraint)
        {
            SetPositionForAllChildren();
            return base.MeasureOverride(constraint);
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            SetPositionForAllChildren();
            return base.ArrangeOverride(arrangeSize);
        }

        /// <inheritdoc />
        protected override void OnColumnDefinitionsChanged()
        {
            base.OnColumnDefinitionsChanged();
            SetPositionForAllChildren(true);
        }

        private void SetPositionForAllChildren(bool forced = false)
        {
            IControl? previous = null;
            foreach (var child in Children)
            {
                if (IsAllowPositioning(child))
                {
                    SetPositionForElement(child, previous, forced);
                    previous = child;
                }
            }
        }

        private static bool IsAllowPositioning(IControl element)
        {
            if (GetDisableAutoAllocation(element) || element is ControlMaxWidthLimiter)
                return false;
            return true;
        }

        private void SetPositionForElement(IControl element, IControl? previewsElement, bool forced)
        {
            if ((!forced && GetIsAutoAllocated(element))
                || GetDisableAutoAllocation(element)
                || element is ControlMaxWidthLimiter)
                return;

            var columnCount = Math.Max(ColumnDefinitions.Count, 1);
            var currentColumn = 0;
            var currentRow = 0;

            if (previewsElement != null)
            {
                var pColumn = GetColumn(previewsElement);
                var pColumnSpan = GetColumnSpan(previewsElement);
                var pRow = GetRow(previewsElement);

                if (GetIsRowBreak(previewsElement) || GetStretchToLastColumn(previewsElement) || pColumn + pColumnSpan >= columnCount)
                {
                    currentColumn = 0;
                    currentRow = pRow + 1;
                }
                else
                {
                    currentColumn = pColumn + pColumnSpan;
                    currentRow = pRow;
                }
            }

            SetRow(element, currentRow);

            if (element is EmptyRow)
            {
                SetColumn(element, 0);
                SetColumnSpan(element, columnCount);
            }
            else
            {
                SetColumn(element, currentColumn);

                if (GetStretchToLastColumn(element))
                    SetColumnSpan(element, columnCount - currentColumn);
            }

            SetIsAutoAllocated(element, true);

            if (AutoGenerateRows && RowDefinitions.Count == currentRow)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
        }

        /// <summary>
        /// Sets the value of the WpfToolset.Windows.Controls.StackGrid.IsRowBreak attached property to a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        [PublicAPI]
        public static void SetIsRowBreak(IControl element, bool value)
        {
            element.SetValue(IsRowBreakProperty, value);
        }

        /// <summary>
        /// Gets the value of the WpfToolset.Windows.Controls.StackGrid.IsRowBreak attached property from a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The value of the WpfToolset.Windows.Controls.StackGrid.IsRowBreak attached property.</returns>
        [PublicAPI]
        public static bool GetIsRowBreak(IControl element)
        {
            return (bool)element.GetValue(IsRowBreakProperty);
        }

        /// <summary>
        /// Sets the value of the WpfToolset.Windows.Controls.StackGrid.DisableAutoAllocation attached property to a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        [PublicAPI]
        public static void SetDisableAutoAllocation(IControl element, bool value)
        {
            element.SetValue(DisableAutoAllocationProperty, value);
        }

        /// <summary>
        /// Gets the value of the WpfToolset.Windows.Controls.StackGrid.DisableAutoAllocation attached property from a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The value of the  WpfToolset.Windows.Controls.StackGrid.DisableAutoAllocation attached property.</returns>
        [PublicAPI]
        public static bool GetDisableAutoAllocation(IControl element)
        {
            return element.GetValue(DisableAutoAllocationProperty);
        }

        /// <summary>
        /// Sets the value of the WpfToolset.Windows.Controls.StackGrid.StretchToLastColumn attached property to a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        [PublicAPI]
        public static void SetStretchToLastColumn(IControl element, bool value)
        {
            element.SetValue(StretchToLastColumnProperty, value);
        }

        /// <summary>
        /// Gets the value of the WpfToolset.Windows.Controls.StackGrid.StretchToLastColumn attached property from a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The value of the  WpfToolset.Windows.Controls.StackGrid.StretchToLastColumn attached property.</returns>
        [PublicAPI]
        public static bool GetStretchToLastColumn(IControl element)
        {
            return element.GetValue(StretchToLastColumnProperty);
        }

        private static bool GetIsAutoAllocated(IControl element)
        {
            return element.GetValue(IsAutoAllocatedProperty);
        }

        private static void SetIsAutoAllocated(IControl element, bool value)
        {
            element.SetValue(IsAutoAllocatedProperty, value);
        }

        #endregion

        /// <summary>
        /// Allows to describe the definition of lines, the algorithm automatically determines the required number of rows and adds them to the definition.
        /// </summary>
        [PublicAPI]
        public bool AutoGenerateRows
        {
            get => GetValue(AutoGenerateRowsProperty);
            set => SetValue(AutoGenerateRowsProperty, value);
        }
    }
}
