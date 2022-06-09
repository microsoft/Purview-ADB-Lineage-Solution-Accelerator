using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Function.Domain.Models.Settings
{
    /// <summary>
    /// Represents the mapping configuration between OpenLineage sources and purview sources.
    /// </summary>
    public class OlToPurviewMapping
    {
        public string Name = "";
        [JsonProperty("parserConditions")]
        public List<ParserCondition> ParserConditions = new List<ParserCondition>();
        public string QualifiedName = "";
        public string PurviewDataType = "";

        /// <summary>
        /// Looks at the QualifiedName string from the configuration maping and gets all the tokens defined in it.
        /// For example nameSpcBodyParts[0] from "/{nameSpcBodyParts[0]}/value/"
        /// </summary>
        /// <returns>List of tokens from the QualifiedName string in the mapping configuration</returns>
        public List<ConfigValue> GetQnTokens()
        {
            List<ConfigValue> rtrn = new List<ConfigValue>();
            var rgex = new Regex(@"(?<=\{).+?(?=\})");
            var matchCollection = rgex.Matches(this.QualifiedName);

            foreach (Match match in matchCollection)
            {
                rtrn.Add(OlToPurviewMapping.GetConfigValue(match.Value));
            }

            return rtrn;
        }

        /// <summary>
        /// Returns a ConfigValue object representing the token with string '[]' values setting the hasIndex bool, and '.' values
        /// divided into child operations.
        /// </summary>
        /// <param name="value">the token to convert</param>
        /// <returns>A ConfigValue object</returns>
        public static ConfigValue GetConfigValue(string value)
        {
            var rgex = new Regex(@"(?<=\[).+?(?=\])");
            var matchCollection = rgex.Matches(value);

            if (matchCollection.Count == 0)
            {
                return new ConfigValue(false, value);
            }
            var op1 = value.Split('[', StringSplitOptions.RemoveEmptyEntries)[0];
            // get the ".parts" section of the value
            var op2Split = value.Split(']', StringSplitOptions.RemoveEmptyEntries);
            string op2 = "";
            if (op2Split.Count() > 1)
            { op2 = op2Split[1]; }

            var op = op1 + op2;

            List<ConfigValue.IndexVal> index = new List<ConfigValue.IndexVal>();
            for (int cnt = 0; cnt < matchCollection.Count ; cnt++)
            {
                try
                { index.Add(new ConfigValue.IndexVal(int.Parse(matchCollection[cnt].Value))); }
                catch (FormatException)
                { index.Add(new ConfigValue.IndexVal(val : matchCollection[cnt].Value.Trim('\'').Trim('"'))); }
            } 
            return new ConfigValue(true, op, index);
        }
    }

    /// <summary>
    /// Representing the token with string '[]' values setting the hasIndex bool, and '.' values
    /// divided into child operations.
    /// </summary>
    public class ConfigValue
    {
        private bool _isIndexed;
        private string _op;
        private string _childOp = "";
        private List<IndexVal>? _indexVals;

        /// <summary>
        /// Constructor for the ConfigValue class
        /// </summary>
        /// <param name="hasIndex">Indicates whether a token string has an index ("[]")</param>
        /// <param name="baseOp">The string token</param>
        /// <param name="indexVals">An optional list of string indexes</param>
        public ConfigValue(bool hasIndex, string baseOp, List<IndexVal>? indexVals = null)
        {
            _isIndexed = hasIndex;
            // one class, NameGroups contains another Parts class
            var ops = baseOp.Split('.');
            _op = ops[0];
            if (ops.Length > 1)
            { _childOp = ops[1]; }
            _indexVals = indexVals;
        }
        public bool HasIndex => _isIndexed;
        public string BaseOp => _op;
        public string ChildOp => _childOp;
        public List<IndexVal>? IndexVals => _indexVals;

        /// <summary>
        /// IndexVal class for the ConfigValue class.  Indexes can be strings for dictionary keys, or integers for array indexes.
        /// </summary>
        public class IndexVal
        {
            public IndexVal(int index = -1, string val = "")
            {
                Index = index;
                Value = val;
            }
            public int Index;
            public string Value;
        }
    }
}