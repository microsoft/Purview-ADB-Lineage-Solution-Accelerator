using Function.Domain.Models.Settings;
using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;
using Function.Domain.Constants;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Function.Domain.Helpers
{
    /// <summary>
    /// This helper parses the OpenLineage name and nameSpace values, and turns them into a Purview QualifiedName
    /// </summary>
    public class ColParser:IColParser
    {
        private List<MountPoint> _mountsInfo;
        private ParserSettings _configuration;
        private ILogger _logger;


        /// <summary>
        /// Parses the NameSpace and Name from the OpenLineage message and returns an object with
        /// organized subparts to be used to construct the names and allow for support to be added in
        /// configuration.
        /// </summary>
        /// <param name="nameSpace">The job/namespace from the OpenLineage message</param>
        /// <param name="name">The job/name from the OpenLineage message</param>
        /// <returns>PurviewIdentifier that is used to construct the Purview Qualified name</returns>
        public ColumnMappingClass GetColIdentifiers(Outputs output)
        {
            var cl = output; 
            var response = new ColumnMappingClass();
            
            return response;
        }
    }
        
}