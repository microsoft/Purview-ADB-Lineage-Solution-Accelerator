using Newtonsoft.Json;


namespace Function.Domain.Models.Settings
{
    public class ParserCondition
    {
        [JsonProperty("op1")]
        public string OlOpRaw = "";  // Dictionary uses this as a key to find type value {prefix, constr, suffix, path, etc.}
        [JsonProperty("compare")]
        public string Compare = "";  // Supports "=", "!=", ">", "<", "contains"
        [JsonProperty("op2")]
        public string ValOp2 = ""; // Supports string or int
        public ConfigValue Op1 {
            get
            {
                return OlToPurviewMapping.GetConfigValue(OlOpRaw);
            }
        }
    }
}