using Function.Domain.Models.Settings;
using Function.Domain.Models.OL;
using Function.Domain.Models.Purview;
using Function.Domain.Constants;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
// using System.Text.Json;
// using System.Text.Json.Serialization;
using Newtonsoft.Json;


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
        
        public List<ColumnLevelAttributes> GetColIdentifiers()
        {
            
            var col = new List<ColumnLevelAttributes>();
            foreach(Outputs colId in _olEvent.Outputs)
            {   
                foreach(KeyValuePair<string, ColumnLineageInputFieldClass> colInfo in colId.Facets.ColFacets.fields)
                {
                  var dataSet = new DatasetMappingClass();
                    //dataSet.sink = $"{colId.NameSpace}, {colId.Name}";
                    dataSet.sink = _qnParser.GetIdentifiers(colId.NameSpace, colId.Name).QualifiedName;
                    var columnLevels = new List<ColumnMappingClass>();
                    foreach (ColumnLineageIdentifierClass colInfo2 in colInfo.Value.inputFields)
                    {
                        var columnLevel = new ColumnMappingClass();
                        //get sources for column level list 
                        foreach(var x in _olEvent.Inputs)
                        {
                            if(colInfo2.name.Contains(x.Name))
                            {
                                colInfo2.name = x.Name;
                            }
                        }
                        dataSet.source = _qnParser.GetIdentifiers(colInfo2.nameSpace, colInfo2.name).QualifiedName;
                        //dataSet.source = "*";
                        columnLevel.source = colInfo2.field;
                        columnLevel.sink = colInfo.Key;
                        columnLevels.Add(columnLevel);
                    }
                    
                    var cla = new ColumnLevelAttributes(); 
                    if( col.Count == 0)
                    {
                        cla.datasetMapping = dataSet;
                        cla.columnMapping = columnLevels;
                    }
                    foreach(var ob in col)
                    {
                        if(ob.datasetMapping.source == dataSet.source && col.Count >= 1)
                        {
                            var cla2 = new ColumnLevelAttributes(); 
                            cla2.columnMapping = columnLevels;
                            ob.columnMapping.Add(cla2.columnMapping[0]);
                            break;
                        }
                        else
                        {
                            cla.datasetMapping = dataSet;
                            cla.columnMapping = columnLevels;
                            break;
                        }
                    }
                    //Add data to list
                    if(cla.datasetMapping.source != string.Empty && cla.datasetMapping.sink != string.Empty)
                    {
                        col.Add(cla);
                    }
                }
            }
            string jsonString2 = JsonConvert.SerializeObject(col);
            return col; 
        }
    }
}
