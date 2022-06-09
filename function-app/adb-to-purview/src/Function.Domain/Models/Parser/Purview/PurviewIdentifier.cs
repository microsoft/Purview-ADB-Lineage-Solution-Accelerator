using Newtonsoft.Json;
namespace Function.Domain.Models.Purview
{
   public class PurviewIdentifier
    {
        // The QN returned will always be without a trailing slash - the validation code must remove
        // trailing slashes before searching Purview as Perview seems to be inconsistent with regard to
        // trailing slashes in QN names and custom sources could have differing rules.
        public string QualifiedName = "";
        public string PurviewType = ""; 
    }
}