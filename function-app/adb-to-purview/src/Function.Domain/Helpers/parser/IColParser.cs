using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;
using System.Collections.Generic;

namespace Function.Domain.Helpers
{
    //Interface for ColParser.cs
    public interface IColParser
     {
        public List<ColumnLevelAttributes> GetColIdentifiers();
    }
}