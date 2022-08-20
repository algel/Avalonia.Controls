namespace Algel.Avalonia.Controls;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Security;

using global::Avalonia.Collections;

public abstract class DefinitionList<T> : AvaloniaList<T> where T : DefinitionBase
{
    private Grid? _parent;

    internal bool IsDirty = true;

    protected DefinitionList()
    {
        ResetBehavior = ResetBehavior.Remove;
        CollectionChanged += OnCollectionChanged;
    }

    internal Grid? Parent
    {
        get => _parent;
        set => SetParent(value);
    }

    private void SetParent(Grid? value)
    {
        _parent = value;

        var idx = 0;

        foreach (var definition in this)
        {
            definition.Parent = value;
            definition.Index = idx++;
        }
    }

    internal void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var idx = 0;

        foreach (var definition in this)
        {
            definition.Index = idx++;
        }

        UpdateDefinitionParent(e.NewItems, false);
        UpdateDefinitionParent(e.OldItems, true);

        IsDirty = true;
    }

    private void UpdateDefinitionParent(IList? items, bool wasRemoved)
    {
        if (items is null)
        {
            return;
        }

        var count = items.Count;

        for (var i = 0; i < count; i++)
        {
            var definition = (DefinitionBase)items[i]!;

            if (wasRemoved)
            {
                definition.OnExitParentTree();
            }
            else
            {
                definition.Parent = Parent;
                definition.OnEnterParentTree();
            }
        }
    }

    private static IEnumerable<string> ExpandStringDefinition(string source)
    {
        if (!source.StartsWith("["))
            return new[] { source };

        var i2 = source.IndexOf("]", StringComparison.Ordinal);
        var cnt = int.Parse(source.Substring(1, i2 - 1));
        var sourceWithoutCount = source.Substring(i2 + 1).Trim();

        return Enumerable.Repeat(sourceWithoutCount, cnt);
    }

    protected IEnumerable<RowColumnDefinition> ParseRowColumnDefinitions(string source)
    {
        return source.Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .SelectMany(ExpandStringDefinition)
                     .Select(RowColumnDefinition.FromString);
    }

    protected struct RowColumnDefinition
    {
        private double MinHeightWidth { get; set; }

        private double MaxHeightWidth { get; set; }

        private GridLength HeightWidth { get; set; }

        private string SharedSizeGroup { get; set; }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> containing a fully qualified type name.
        /// </returns>
        public override string ToString()
        {
            var lengthConverter = new LengthConverter();
            var lst = new List<string>();
            if (!double.IsPositiveInfinity(MaxHeightWidth) || MinHeightWidth > 0.0)
            {
                lst.Add(lengthConverter.ConvertToInvariantString(MinHeightWidth));
                lst.Add(HeightWidth.ToString());
                lst.Add(lengthConverter.ConvertToInvariantString(MaxHeightWidth));
            }
            else
            {
                lst.Add(HeightWidth.ToString());
            }

            if (!string.IsNullOrWhiteSpace(SharedSizeGroup))
            {
                lst.Add(SharedSizeGroup);
            }

            return string.Join(" ", lst);
        }

        /// <exception cref="ArgumentException">The input string was in not correct format.</exception>
        public static RowColumnDefinition FromString(string info)
        {
            var lengthConverter = new LengthConverter();
            var splittedParts = info.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                // ReSharper disable PossibleNullReferenceException
                switch (splittedParts.Length)
                {
                    case 1:
                    {
                        return new RowColumnDefinition
                        {
                            MinHeightWidth = 0,
                            HeightWidth = GridLength.Parse(splittedParts[0]),
                            MaxHeightWidth = double.PositiveInfinity
                        };
                    }
                    case 2:
                    {
                        return new RowColumnDefinition
                        {
                            MinHeightWidth = 0,
                            HeightWidth = GridLength.Parse(splittedParts[0]),
                            MaxHeightWidth = double.PositiveInfinity,
                            SharedSizeGroup = splittedParts[1]
                        };
                    }
                    case 3:
                    {
                        return new RowColumnDefinition
                        {
                            MinHeightWidth = (double)lengthConverter.ConvertFromInvariantString(splittedParts[0]),
                            HeightWidth = GridLength.Parse(splittedParts[1]),
                            MaxHeightWidth = (double)lengthConverter.ConvertFromInvariantString(splittedParts[2])
                        };
                    }
                    case 4:
                    {
                        return new RowColumnDefinition
                        {
                            MinHeightWidth = (double)lengthConverter.ConvertFromInvariantString(splittedParts[0]),
                            HeightWidth = GridLength.Parse(splittedParts[1]),
                            MaxHeightWidth = (double)lengthConverter.ConvertFromInvariantString(splittedParts[2]),
                            SharedSizeGroup = splittedParts[3]
                        };
                    }

                    default:
                        throw new ArgumentException("The input string was in not correct format", nameof(info));
                    // ReSharper restore PossibleNullReferenceException
                }
            }
            catch (NullReferenceException e)
            {
                throw new ArgumentException($"The input string was in not correct format (info={info}).", nameof(info), e);
            }
        }

        public static RowColumnDefinition FromColumnDefinition(ColumnDefinition cd)
        {
            return new RowColumnDefinition
            {
                MinHeightWidth = cd.MinWidth,
                HeightWidth = cd.Width,
                MaxHeightWidth = cd.MaxWidth,
                SharedSizeGroup = cd.SharedSizeGroup
            };
        }

        public static RowColumnDefinition FromRowDefinition(RowDefinition cd)
        {
            return new RowColumnDefinition
            {
                MinHeightWidth = cd.MinHeight,
                HeightWidth = cd.Height,
                MaxHeightWidth = cd.MaxHeight,
                SharedSizeGroup = cd.SharedSizeGroup
            };
        }

        public ColumnDefinition ToColumnDefinition()
        {
            return new ColumnDefinition
            {
                MinWidth = MinHeightWidth,
                Width = HeightWidth,
                MaxWidth = MaxHeightWidth,
                SharedSizeGroup = SharedSizeGroup
            };
        }

        public RowDefinition ToRowDefinition()
        {
            return new RowDefinition
            {
                MinHeight = MinHeightWidth,
                Height = HeightWidth,
                MaxHeight = MaxHeightWidth,
                SharedSizeGroup = SharedSizeGroup
            };
        }
    }
}

/// <summary>
///     Converts instances of other types to and from instances of a <see cref="T:System.Double" /> that represent an
///     object's length.
/// </summary>
public class LengthConverter : TypeConverter
{
    private static readonly string[] PixelUnitStrings = new string[4]
    {
        "px",
        "in",
        "cm",
        "pt"
    };

    private static readonly double[] PixelUnitFactors = new double[4]
    {
        1.0,
        96.0,
        4800.0 / sbyte.MaxValue,
        4.0 / 3.0
    };

    /// <summary>
    ///     Determines whether conversion is possible from a specified type to a <see cref="T:System.Double" /> that
    ///     represents an object's length.
    /// </summary>
    /// <param name="typeDescriptorContext">Provides contextual information about a component.</param>
    /// <param name="sourceType">Identifies the data type to evaluate for conversion.</param>
    /// <returns>
    ///     <see langword="true" /> if conversion is possible; otherwise, <see langword="false" />.
    /// </returns>
    public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
    {
        switch (Type.GetTypeCode(sourceType))
        {
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.String:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    ///     Determines whether conversion is possible to a specified type from a <see cref="T:System.Double" /> that
    ///     represents an object's length.
    /// </summary>
    /// <param name="typeDescriptorContext">Provides contextual information about a component.</param>
    /// <param name="destinationType">Identifies the data type to evaluate for conversion.</param>
    /// <returns>
    ///     <see langword="true" /> if conversion to the <paramref name="destinationType" /> is possible; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
    {
        return destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string);
    }

    /// <summary>
    ///     Converts instances of other data types into instances of <see cref="T:System.Double" /> that represent an
    ///     object's length.
    /// </summary>
    /// <param name="typeDescriptorContext">Provides contextual information about a component.</param>
    /// <param name="cultureInfo">Represents culture-specific information that is maintained during a conversion.</param>
    /// <param name="source">Identifies the object that is being converted to <see cref="T:System.Double" />.</param>
    /// <returns>An instance of <see cref="T:System.Double" /> that is the value of the conversion.</returns>
    /// <exception cref="T:System.ArgumentNullException">Occurs if the <paramref name="source" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///     Occurs if the <paramref name="source" /> is not <see langword="null" />
    ///     and is not a valid type for conversion.
    /// </exception>
    public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
    {
        if (source == null)
        {
            throw GetConvertFromException(source);
        }

        return source is string ? FromString((string)source, cultureInfo) : (object)Convert.ToDouble(source, cultureInfo);
    }

    /// <summary>Converts other types into instances of <see cref="T:System.Double" /> that represent an object's length. </summary>
    /// <param name="typeDescriptorContext">
    ///     Describes context information of a component, such as its container and
    ///     <see cref="T:System.ComponentModel.PropertyDescriptor" />.
    /// </param>
    /// <param name="cultureInfo">
    ///     Identifies culture-specific information, including the writing system and the calendar that
    ///     is used.
    /// </param>
    /// <param name="value">Identifies the <see cref="T:System.Object" /> that is being converted.</param>
    /// <param name="destinationType">The data type that this instance of <see cref="T:System.Double" /> is being converted to.</param>
    /// <returns>A new <see cref="T:System.Object" /> that is the value of the conversion.</returns>
    /// <exception cref="T:System.ArgumentNullException">Occurs if the <paramref name="value" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    ///     Occurs if the <paramref name="value" /> is not <see langword="null" /> and
    ///     is not a <see cref="T:System.Windows.Media.Brush" />, or the <paramref name="destinationType" /> is not valid.
    /// </exception>
    [SecurityCritical]
    public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object? value, Type destinationType)
    {
        if (destinationType == null)
        {
            throw new ArgumentNullException(nameof(destinationType));
        }

        if (value is double num)
        {
            if (destinationType == typeof(string))
            {
                return double.IsNaN(num) ? "Auto" : (object)Convert.ToString(num, cultureInfo);
            }

            if (destinationType == typeof(InstanceDescriptor))
            {
                return new InstanceDescriptor(typeof(double).GetConstructor(new[] { typeof(double) }), new[] { (object)num });
            }
        }

        throw GetConvertToException(value, destinationType);
    }

    internal static double FromString(string s, CultureInfo cultureInfo)
    {
        var str1 = s.Trim();
        var lowerInvariant = str1.ToLowerInvariant();
        var length = lowerInvariant.Length;
        var num1 = 0;
        var num2 = 1.0;
        if (lowerInvariant == "auto")
        {
            return double.NaN;
        }

        for (var index = 0; index < PixelUnitStrings.Length; ++index)
        {
            if (lowerInvariant.EndsWith(PixelUnitStrings[index], StringComparison.Ordinal))
            {
                num1 = PixelUnitStrings[index].Length;
                num2 = PixelUnitFactors[index];
                break;
            }
        }

        var str2 = str1.Substring(0, length - num1);
        try
        {
            return Convert.ToDouble(str2, cultureInfo) * num2;
        }
        catch (FormatException ex)
        {
            throw new FormatException($"LengthFormatError {str2}", ex);
        }
    }

    internal static string ToString(double l, CultureInfo cultureInfo)
    {
        return double.IsNaN(l) ? "Auto" : Convert.ToString(l, cultureInfo);
    }
}
