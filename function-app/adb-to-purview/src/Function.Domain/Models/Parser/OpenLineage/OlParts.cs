using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Function.Domain.Models.OL
{
    /// <summary>
    /// Represents a breakdown of the OL NameSpace and Name values into parts which can be referenced dynamically from configuration.
    /// </summary>
    public class OlParts
    {
        private OlNameParts _olNameParts = new OlNameParts();
        private OlNameSpaceParts _olNameSpaceParts = new OlNameSpaceParts();
        /// <summary>
        /// Constructor for OlParts objects
        /// </summary>
        /// <param name="nameSpace">The NameSpace value from OpenLineage</param>
        /// <param name="name">The Name Value from OpenLineage</param>
        public OlParts(string nameSpace, string name)
        {
            // Divide connection string from prefix
            (string olPrefix, string olConnection) = GetPrefixConn(nameSpace);
            this.Prefix = olPrefix;

            char[] seperators = { '@', ':', '/', ';', ',' };
            var initSplits = olConnection.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var initSplit in initSplits)
            {
                if (!AddConStringNameValues(initSplit, ref this._olNameSpaceParts.NameSpaceConnNameValues))
                {
                    this._olNameSpaceParts.NameSpaceBodyParts.Add(initSplit);
                    var periodSplits = initSplit.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    if (periodSplits.Length > 1)
                    {
                        foreach (var periodSplit in periodSplits)
                        {
                            this._olNameSpaceParts.NameSpaceConnParts.Add(periodSplit);
                        }
                    }
                }
            }
            var rgex = new Regex(@"(?<=\[).+?(?=\])");
            var rerslt = rgex.Matches(name);
            if (rerslt.Count > 0)
            {
                foreach (var match in rerslt)
                {
                    this._olNameParts.NameGroups.Add(new OlNameParts.NameGroup(match.ToString() ?? ""));
                }
            }
            else
            {
                this._olNameParts.NameGroups.Add(new OlNameParts.NameGroup(name));
            }
        }
        
        private bool AddConStringNameValues(string inString, ref Dictionary<string, string> valDict)
        {
            var splitString = inString.Split('=');
            if (splitString.Length == 2)
            {
                valDict.Add(splitString[0], splitString[1]);
                return true;
            }
            return false;
        }
        // Returns the prefix and connection strings
        // Looks for the "//" and deletes the preceding character
        // This is done instead of "://" as some sources use other characters such as "@//"
        private (string, string) GetPrefixConn(string olCon)
        {
            string olPrefix = "", olConnection = "";

            int colon = olCon.IndexOf("//");
            if (colon != -1)
            {
                olPrefix = olCon.Substring(0, colon - 1);
                olConnection = olCon.Substring(colon + 2);
            }
            else
            {
                olPrefix = olCon;
            }
            return (olPrefix, olConnection);
        }
        /// <summary>
        /// Gets an object as a Dictionary<string, object> to allow dynamic access based on configuration string indexes
        /// </summary>
        /// <param name="keys">The string identifiers from configuration that will represent the object</param>
        /// <returns>A Dictionary with string keys representing each of the OpenLineage Name / Namespace referencable parts</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public Dictionary<string, object> GetDynamicPairs(string[] keys)
        {
            if (keys == null || keys.Length != 5)
            {
                throw new System.ArgumentException("keys must be an array of length 5");
            }
            var pairs = new Dictionary<string, object>();
            pairs.Add(keys[0], this.Prefix);
            pairs.Add(keys[1], this.OlNameSpaceParts.NameSpaceConnParts);
            pairs.Add(keys[2], this.OlNameSpaceParts.NameSpaceBodyParts);
            pairs.Add(keys[3], this.OlNameSpaceParts.NameSpaceConnNameValues);
            pairs.Add(keys[4], this.OlNameParts.NameGroups);
            
            return pairs;
        }
        public OlNameParts OlNameParts => _olNameParts;
        public OlNameSpaceParts OlNameSpaceParts => _olNameSpaceParts;
        public string Prefix;
    }

    public class OlNameSpaceParts
    {
        // Splits all the parts of the namespace except the connection in order by @:/;, symbols
        public List<string> NameSpaceBodyParts = new List<string>();
        // Splits the connection part of the namespace by . symbol
        public List<string> NameSpaceConnParts = new List<string>();
        // Splits out any name value pairs as identified by = symbol
        public Dictionary<string, string> NameSpaceConnNameValues = new Dictionary<string, string>();
    }
    
    public class OlNameParts
    {
        // Splits out any groupings as identified by [] or () or full name as NameGroups[0]
        public List<NameGroup> NameGroups = new List<NameGroup>();


        public class NameGroup
        {
            private string _name;
            // Further splits individual groups by . or / symbols
            public NameGroup(string name)
            {
                _name = name;
                NameParts = name.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            public string Name => _name;
            public List<string> NameParts = new List<string>();
        }
    }
}