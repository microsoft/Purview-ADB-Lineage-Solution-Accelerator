using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;

namespace Function.Domain.Helpers
{
    //Interface for ColParser.cs
    public interface IColParser
     {
        public ColumnLevelAttributes GetColIdentifiers();
    }
}