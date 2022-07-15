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
        private ParserSettings _configuration;
        private ILogger _logger;
        private IQnParser _qnParser;
        private Event _olEvent;

        public ColParser(ParserSettings configuration, ILoggerFactory logger, Event olEvent, IQnParser qnParser)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger<ColParser>();
            _olEvent = olEvent;
            _qnParser = qnParser;
        }

        /// <summary>
        /// This class will be used for the parsing code. 
        /// </summary>
        /// <returns></returns>
        
        public ColumnLevelAttributes GetColIdentifiers()
        {
            
            var col = new ColumnLevelAttributes();
            var dataSetList = new List<DatasetMappingClass>();
            var columnLevelList = new List<ColumnMappingClass>();

            foreach(Outputs colId in _olEvent.Outputs)
            {   
                foreach(KeyValuePair<string, ColumnLineageInputFieldClass> colInfo in colId.Facets.ColFacets.fields)
                {
                    var dataSet = new DatasetMappingClass();
                    //Set sink name 
                    dataSet.sink = _qnParser.GetIdentifiers(colId.NameSpace, colId.Name).QualifiedName;
                    var columnLevel = new ColumnMappingClass();
                    foreach(ColumnLineageIdentifierClass colInfo2 in colInfo.Value.inputFields)
                    {
                        //get sources for column level list 
                        dataSet.source = _qnParser.GetIdentifiers(colInfo2.nameSpace, colInfo2.name).QualifiedName;
                        columnLevel.source = colInfo2.field;
                        columnLevel.sink = colInfo.Key;
                        //ADD data to model
                        dataSetList.GroupBy(x=>x.source);
                        dataSetList.Add(dataSet);
                        columnLevelList.Add(columnLevel);
                       
                    }
                    //Add data to list 
                    col.datasetMapping.Add(dataSet);
                    col.columnMapping.Add(columnLevel);
                }
            }
            var test = col.datasetMapping.GroupBy(x => x.source);
            return col; 
        }
    }
}