namespace Algel.Avalonia.Controls
{
    using System.Linq;

    /// <summary>
    /// A collection of <see cref="RowDefinition"/>s.
    /// </summary>
    public class RowDefinitions : DefinitionList<RowDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowDefinitions"/> class.
        /// </summary>
        public RowDefinitions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowDefinitions"/> class.
        /// </summary>
        /// <param name="s">A string representation of the row definitions.</param>
        public RowDefinitions(string s)
            : this()
        {
            //this.AddRange(GridLength.ParseLengths(s).Select(x => new RowDefinition(x)));
            AddRange(ParseRowColumnDefinitions(s).Select(e=>e.ToRowDefinition()));
        }

        public override string ToString()
        {
            return string.Join(",", this.Select(x => x.Height));
        }

        /// <summary>
        /// Parses a string representation of row definitions collection.
        /// </summary>
        /// <param name="s">The row definitions string.</param>
        /// <returns>The <see cref="RowDefinitions"/>.</returns>
        public static RowDefinitions Parse(string s) => new(s);
    }
}
