using System.Collections.Generic;
using Function.Domain.Models.Purview;

namespace Function.Domain.Models.Settings
{
    public class NewParserSettings
    {
        public List<OlToPurviewMapping> OlToPurviewMappings = new List<OlToPurviewMapping>();
    }

    public class SettingsItem
    {
        public List<Condition> Conditions = new List<Condition>();
        public PurviewIdentifier Transformation = new PurviewIdentifier();  // used to build identifier
    }

    public class Condition
    {
        public string Op1 = "";  // Dictionary uses this as a key to find type value {prefix, constr, suffix, path, etc.}
        public string Op2 = "";
        public string Conditional = "";  // Supports "=" and "!="
    }

    public class PurviewIdentifierMapping
    {
        public string PurviewType = ""; 
        public string Prefix = ""; 
        public string Connection = ""; 
        public string Path = ""; 
    }

}