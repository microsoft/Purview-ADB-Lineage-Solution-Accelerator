using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class EnvironmentPropsParent
    {
        [JsonProperty("environment-properties")]
        public EnvironmentProps EnvironmentProperties = new EnvironmentProps();
    }
}