// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Function.Domain.Models.OL
{
    public class Inputs: IInputsOutputs
    {
        public string Name { get; set; } = "";
        [JsonProperty("namespace")]
        public string NameSpace { get; set; } = "";

        public override bool Equals(object obj) {
            if (obj is Inputs other)
            {
                if (Name == other.Name && NameSpace == other.NameSpace)
                    return true;
            }
            return false;        
        }

        public override int GetHashCode() {
            return Name.GetHashCode() ^
                NameSpace.GetHashCode();
        }
    }
}