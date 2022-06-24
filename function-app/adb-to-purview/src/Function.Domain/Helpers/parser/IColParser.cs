using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;

namespace Function.Domain.Helpers
{
    public interface IColParser
     {
        public ColumnMappingClass GetColIdentifiers(Outputs output);
    }
}