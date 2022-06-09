using Function.Domain.Models.Purview;

namespace Function.Domain.Helpers
{
    public interface IQnParser
    {
        public PurviewIdentifier GetIdentifiers(string nameSpace, string name);
    }
}