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
            var dataSetList = new DatasetMappingClass();
            var columnLevelList = new List<ColumnMappingClass>();
            var dataSet = new DatasetMappingClass();
            var cla = new ColumnLevelAttributes();
            foreach(Outputs colId in _olEvent.Outputs)
            {   
                foreach(KeyValuePair<string, ColumnLineageInputFieldClass> colInfo in colId.Facets.ColFacets.fields)
                {
                    
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
                        //dataSetList.source.GroupBy(x=>x.Equals(dataSet.source));
                        // dataSetList.Add(dataSet);
                        //dataSetList = dataSet;
                        //columnLevelList.Add(columnLevel);
                       
                    }
                    //Add data to list 
                    cla.datasetMapping = dataSet;
                    cla.columnMapping.Add(columnLevel);
                    col.Add(cla);
                }
            }
            // var test = col.datasetMapping.source.GroupBy(x=>x.Equals(col.datasetMapping.sink));
            // //var test = col.datasetMapping.source.GroupBy(x);
            //var test2 = col.GroupBy(x=>x.datasetMapping.sink);

            //string jsonString = JsonSerializer.Serialize(col);
            string jsonString2 = JsonConvert.SerializeObject(col);
           // string jsonString3 = JsonConvert.SerializeObject(test);
           // string jsonString = JsonConvert.SerializeObject(test2);
            return col; 
        }
    }
}