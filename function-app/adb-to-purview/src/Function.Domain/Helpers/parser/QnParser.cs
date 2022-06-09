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
    public class QnParser: IQnParser
    {
        private List<MountPoint> _mountsInfo;
        private ParserSettings _configuration;
        private ILogger _logger;

        private readonly string[] JSON_KEY_NAMES = { "prefix", "nameSpcConParts", "nameSpcBodyParts", "nameSpcNameVals", 
                                                     "nameGroups"};

        /// <summary>
        /// Constructor for QnParser
        /// </summary>
        /// <param name="mountsInfo">List of ADB mount points and targets from OpenLineage run/facets/environment-properties/environment-properties/mountPoints </param>
        /// <param name="configuration">Function configuration from DI</param>
        /// <param name="loggerFactory">Function loggerfactory from DI</param>
        public QnParser(ParserSettings configuration, ILoggerFactory loggerFactory, List<MountPoint>? mountsInfo = null)
        {
            _logger = loggerFactory.CreateLogger<QnParser>();
            _mountsInfo = mountsInfo ?? new List<MountPoint>();
            _configuration = configuration;
        }

        /// <summary>
        /// Parses the NameSpace and Name from the OpenLineage message and returns an object with
        /// organized subparts to be used to construct the names and allow for support to be added in
        /// configuration.
        /// </summary>
        /// <param name="nameSpace">The job/namespace from the OpenLineage message</param>
        /// <param name="name">The job/name from the OpenLineage message</param>
        /// <returns>PurviewIdentifier that is used to construct the Purview Qualified name</returns>
        public PurviewIdentifier GetIdentifiers(string nameSpace, string name)
        {
            var purviewIdentifier = new PurviewIdentifier();

            // If this is an ADB mountpoint, resolve and continue
            if ((nameSpace == ParserConstants.DBFS || nameSpace == ParserConstants.DBFS2) && name.Substring(0, 4) == "/mnt")
            {
                (string nmSpace, string nm) = GetMountPoint(name);
                nameSpace = nmSpace;
                name = nm;
            }
            // Break the name and nameSpace values into their individual / referencable parts
            var olParts = new OlParts(nameSpace, name);

            // Get a dictionary assigning the configuration string keys to each of the olParts
            var olDynParts = olParts.GetDynamicPairs(JSON_KEY_NAMES);

            // Retrieve the relevant mapping of for the particular nameSpace and name from the configuration, by 
            // matching based on the conditions specified in the configuration.
            var mapping = GetMapping(olDynParts);

            if (mapping is null)
            {
                var ex = new MissingCriticalDataException("Missing Ol to Purview mapping data for this source");
                _logger.LogError(ex, "Missing Ol to Purview mapping data");
                throw ex;
            }

            // Use the relevant configuration mapping and the olParts to construct the PurviewIdentifier
            purviewIdentifier = GetPurviewIdentifier(mapping, olDynParts);

            return purviewIdentifier;
        }

        private (string, string) GetMountPoint(string name)
        {
            // because it is unknown what part of the string is the mountpoint
            // and what part is the path following the mountpoint, we have to try to get the mount point
            // for each token
            string[] potentialMounts = name.Split('/');
            string potentialMountPoint = "";
            string path = "";
            MountPoint? mountPoint;
            string mountPointSource = "";
            bool gotMountPoint = false;
            foreach (var dir in potentialMounts)
            {
                if (dir != "")
                {
                    if (!gotMountPoint)
                    {
                        potentialMountPoint += $"/{dir}";
                        if ((mountPoint = _mountsInfo.Where(m => m.MountPointName.Trim('/') == potentialMountPoint.Trim('/')).SingleOrDefault()) != null)
                        {
                            mountPointSource = mountPoint.Source;
                            gotMountPoint = true;
                        }
                    }
                    else
                    {
                        path += $"/{dir}";
                    }
                }
            }
            return (mountPointSource.Trim('/'), path);
        }



        private OlToPurviewMapping? GetMapping(Dictionary<string,object> olDynParts)
        {
            foreach (var map in _configuration.OlToPurviewMappings)
            {
                if (EvaluateConditions(map.ParserConditions, olDynParts))
                {
                    return map;
                }
            }
            return null;
        }

        private PurviewIdentifier GetPurviewIdentifier(OlToPurviewMapping mapping, Dictionary<string, object> olDynParts)
        {
            var purviewIdentifier = new PurviewIdentifier();
            // Replaces all the values in the QualifiedName of the OlToPurviewMapping string that are within brackets with 
            // the values from the OlParts
            var regEx = new Regex("{.*?}");

            if (mapping != null)
            {
                purviewIdentifier.PurviewType = mapping.PurviewDataType;

                string QN = mapping.QualifiedName;
                var configValues = mapping.GetQnTokens();
                foreach (var value in configValues)
                {
                    QN = regEx.Replace(QN, GetDynamicValue(value, olDynParts), 1);
                }
                purviewIdentifier.QualifiedName = QN.TrimEnd('/');
            }
            return purviewIdentifier;
        }

        private bool EvaluateConditions(List<ParserCondition> parserConditions, Dictionary<string,object> olDynParts)
        {
            foreach (var condition in parserConditions)
            {
                if (!EvaluateCondition(condition, olDynParts))
                {
                    return false;
                }
            }
            return true;
        }

        private bool EvaluateCondition(ParserCondition parserCondition, Dictionary<string,object> olDynParts)
        {
            var firstOp = GetDynamicValue(parserCondition.Op1, olDynParts);
            switch (parserCondition.Compare)
            {
                case "=":
                    return firstOp == parserCondition.ValOp2;
                case "!=":
                    return firstOp != parserCondition.ValOp2;
                case "contains":
                    return firstOp.Contains(parserCondition.ValOp2);
                case ">":
                    return int.Parse(firstOp) > int.Parse(parserCondition.ValOp2);
                case "<":
                    return int.Parse(firstOp) < int.Parse(parserCondition.ValOp2);                    
            }
            return false;
        }

        private string GetDynamicValue(ConfigValue configValue, Dictionary<string,object> olDynParts)
        {
            string? rtrn = "";
            object op1 = olDynParts[configValue.BaseOp];
            if (op1.GetType() == typeof(List<string>))
            {
                var typedOp1 = (op1 as List<string>);
                if (typedOp1 is null)
                {
                    var ex = new MissingCriticalDataException("Error getting dynamic value");
                    _logger.LogError(ex, "Error getting dynamic value");
                    throw ex;
                }
                if (configValue.IndexVals != null)
                {
                    rtrn = typedOp1[configValue.IndexVals[0].Index];
                }
                else
                {
                    rtrn = typedOp1.Count.ToString();
                }
            }
            else if (op1.GetType() == typeof(List<OlNameParts.NameGroup>))
            {
                var typedOp1 = (op1 as List<OlNameParts.NameGroup>);
                if (typedOp1 is null)
                {
                    var ex = new MissingCriticalDataException("Error getting dynamic value");
                    _logger.LogError(ex, "Error getting dynamic value");
                    throw ex;
                }

                if (configValue.ChildOp != "")
                {
                    if (configValue.IndexVals is null)
                    {
                        var ex = new MissingCriticalDataException("Error getting dynamic value");
                        _logger.LogError(ex, "Error getting dynamic value");
                        throw ex;                        
                    }
                    if (configValue.IndexVals.Count() > 1)
                    {
                        rtrn = typedOp1[configValue.IndexVals[0].Index].NameParts[configValue.IndexVals[1].Index];
                    }
                    else
                    {
                        rtrn = typedOp1[configValue.IndexVals[0].Index].NameParts.Count.ToString();
                    }
                    
                }
                else
                {
                    // If the childOp is not specified, then we are looking at the count of the nameGroup
                    if (configValue.HasIndex)
                    {
                        if (configValue.IndexVals is null)
                        {
                            var ex = new MissingCriticalDataException("Error getting dynamic value");
                            _logger.LogError(ex, "Error getting dynamic value");
                            throw ex;                        
                        }
                        rtrn = typedOp1[configValue.IndexVals[0].Index].Name;
                    }
                    else
                    {
                        var typedOp = (op1 as List<OlNameParts.NameGroup>);
                        if (typedOp is null)
                        {
                            var ex = new MissingCriticalDataException("Error getting dynamic value");
                            _logger.LogError(ex, "Error getting dynamic value");
                            throw ex;                        
                        }
                        rtrn = typedOp.Count.ToString();
                    }
                    
                }
            }
            else if (op1.GetType() == typeof(Dictionary<string, string>))
            {
                var typedOp = (op1 as Dictionary<string,string>);
                if (typedOp is null)
                {
                    var ex = new MissingCriticalDataException("Error getting dynamic value");
                    _logger.LogError(ex, "Error getting dynamic value");
                    throw ex;                        
                }
                try
                {
                    if (configValue?.IndexVals?[0]?.Value == null)
                    {
                        var ex = new MissingCriticalDataException("Error getting dynamic value");
                        _logger.LogError(ex, "Error getting dynamic value");
                        throw ex;                        
                    }
                    rtrn = typedOp[configValue.IndexVals[0].Value];
                }
                catch (KeyNotFoundException ex)
                {
                    if (configValue.IndexVals is null)
                    {
                        var ex2 = new MissingCriticalDataException("Error getting dynamic value");
                        _logger.LogError(ex2, "Error getting dynamic value");
                        throw ex2;                        
                    }
                    throw new ConfigMismatchException($"Key {configValue?.IndexVals?[0]?.Value ?? ""} not found in dictionary for source prefix {olDynParts["prefix"]}. Make sure your configuration matches the OpenLineage output.", ex);
                }
            }
            else if (op1.GetType() == typeof(string))
            {
                rtrn = op1 as string;
            }
            else
            {
                rtrn = "";
            }
            return rtrn!.Trim('/');
        }
    }
}