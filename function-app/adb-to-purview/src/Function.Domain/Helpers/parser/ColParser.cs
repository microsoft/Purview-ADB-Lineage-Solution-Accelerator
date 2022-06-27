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

// This class will be used for the parsing code. 
        public ColumnMappingClass GetColIdentifiers(Outputs output)
        {
            var cl = output; 
            var response = new ColumnMappingClass();
            
            return response;
        }
    }
        
}