namespace Algel.Avalonia.Controls
{
    using System;
    using System.Linq;

    using global::Avalonia;
    using global::Avalonia.Controls;

    using JetBrains.Annotations;

    /// <summary>
    /// Allows you to block the automatic increase in the width of the control as the content content (e.g. TextBox tends to stretch in width as you type text)
    /// The problem is relevant when the element is in the grid, and is a column with Width="Auto" (if the element spans multiple columns, the problem occurs if at least one column has Width="Auto")
    /// <example>
    /// <code>
    /// <![CDATA[
    /// <Grid>
    ///    <Grid.ColumnDefinitions>
    ///         <ColumnDefinition Width="Auto"/>
    ///         <ColumnDefinition Width="100"/>
    ///         <ColumnDefinition Width="Auto"/>
    ///    </Grid.ColumnDefinitions>
    /// 
    ///     <Label Grid.Column="0" Content="..." Target="{x:Reference myTextBox}/>
    ///     <TextBox x:Name="myTextBox" Grid.Column="1" Grid.ColumnSpan="2" />
    ///     <awt:ControlMaxWidthLimiter Target={x:Reference myTextBox}/>
    /// </Grid>
    /// 
    /// <!--OR-->
    /// 
    /// <TextBox awt:ControlMaxWidthLimiter.FixAutoGrowMaxWidthBehavior="True"/>
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    public class ControlMaxWidthLimiter : Control
    {
        #region Fields

        /// <summary>
        /// Identifies the WpfToolset.Windows.Controls.ControlMaxWidthLimiter.TargetProperty dependency property.
        /// </summary>
        public static readonly StyledProperty<Control> TargetProperty = AvaloniaProperty.Register<ControlMaxWidthLimiter, Control>(nameof(Target));

        /// <summary>
        /// Setting the value to True will automatically add in Grid and set the object ControlMaxWidthLimiter
        /// </summary>
        public static readonly AttachedProperty<bool> FixAutoGrowMaxWidthProperty = AvaloniaProperty.RegisterAttached<ControlMaxWidthLimiter, Control, bool>("FixAutoGrowMaxWidth");

        #endregion

        static ControlMaxWidthLimiter()
        {
            FixAutoGrowMaxWidthProperty.Changed.AddClassHandler<Control>(OnFixAutoGrowMaxWidthPropertyChanged);
        }

        #region Properties

        /// <summary>
        /// Get or set the item for which you want to control the width
        /// </summary>
        public Control Target
        {
            get => GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        #endregion

        #region Methods

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            if (change.Property == TargetProperty)
                OnTargetPropertyChanged(change.OldValue.GetValueOrDefault<Control>(), change.NewValue.GetValueOrDefault<Control>());
            else if (change.Property == BoundsProperty)
                OnActualWidthPropertyChanged(change.NewValue.GetValueOrDefault<Rect>().Width);
            base.OnPropertyChanged(change);
        }

        private void OnActualWidthPropertyChanged(double newValue)
        {
            if (Target != null)
                Target.MaxWidth = newValue;
        }

        private IDisposable? marginPropertySubscription;
        private IDisposable? gridColummPropertySubscription;
        private IDisposable? gridColummSpanPropertySubscription;

        private void OnTargetPropertyChanged(Control? oldValue, Control? newValue)
        {
            if (oldValue != null)
            {
                marginPropertySubscription?.Dispose();
                gridColummPropertySubscription?.Dispose();
                gridColummSpanPropertySubscription?.Dispose();
            }
            if (newValue != null)
            {
                marginPropertySubscription = this.Bind(MarginProperty, newValue.GetObservable(MarginProperty));
                gridColummPropertySubscription = this.Bind(Grid.ColumnProperty, newValue.GetObservable(Grid.ColumnProperty));
                gridColummSpanPropertySubscription = this.Bind(Grid.ColumnSpanProperty, newValue.GetObservable(Grid.ColumnSpanProperty));

                OnActualWidthPropertyChanged(Bounds.Width);
            }
        }

        private static void OnFixAutoGrowMaxWidthPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var element = d as Control;
            if (element?.Parent is Grid grid)
            {
                if ((bool)e.NewValue)
                {
                    var item = new ControlMaxWidthLimiter();
                    item.SetValue(MarginProperty, element.GetValue(MarginProperty));
                    item.SetValue(Grid.ColumnProperty, element.GetValue(Grid.ColumnProperty));
                    item.SetValue(Grid.ColumnSpanProperty, element.GetValue(Grid.ColumnSpanProperty));
                    grid.Children.Add(item);

                    item.Target = element;
                }
                else
                {
                    var item = grid.Children.OfType<ControlMaxWidthLimiter>().FirstOrDefault(x => Equals(x.Target, element));
                    if (item != null)
                    {
                        grid.Children.Remove(item);
                        item.Target = null;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the value of the WpfToolset.Windows.Controls.ControlMaxWidthLimiter.FixAutoGrowMaxWidth attached property to a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        [PublicAPI]
        public static void SetFixAutoGrowMaxWidth(Control element, bool value)
        {
            element.SetValue(FixAutoGrowMaxWidthProperty, value);
        }

        /// <summary>
        /// Gets the value of the WpfToolset.Windows.Controls.ControlMaxWidthLimiter.FixAutoGrowMaxWidth attached property from a given System.Windows.UIElement.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>The value of the WpfToolset.Windows.Controls.StackGrid.IsRowBreak attached property.</returns>
        [PublicAPI]
        public static bool GetFixAutoGrowMaxWidth(Control element)
        {
            return element.GetValue(FixAutoGrowMaxWidthProperty);
        }

        #endregion
    }
}
