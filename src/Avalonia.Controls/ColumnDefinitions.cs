namespace Algel.Avalonia.Controls
{
    using System.Linq;

    /// <summary>
    /// A collection of <see cref="ColumnDefinition"/>s.
    /// </summary>
    public class ColumnDefinitions : DefinitionList<ColumnDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinitions"/> class.
        /// </summary>
        public ColumnDefinitions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinitions"/> class.
        /// </summary>
        /// <param name="s">A string representation of the column definitions.</param>
        public ColumnDefinitions(string s)
            : this()
        {
            //this.AddRange(GridLength.ParseLengths(s).Select(x => new ColumnDefinition(x)));
            AddRange(ParseRowColumnDefinitions(s).Select(e=>e.ToColumnDefinition()));
        }

        public override string ToString()
        {
            return string.Join(",", this.Select(x => x.Width));
        }

        /// <summary>
        /// Parses a string representation of column definitions collection.
        /// </summary>
        /// <param name="s">The column definitions string.</param>
        /// <returns>The <see cref="ColumnDefinitions"/>.</returns>
        public static ColumnDefinitions Parse(string s) => new(s);
    }
}
