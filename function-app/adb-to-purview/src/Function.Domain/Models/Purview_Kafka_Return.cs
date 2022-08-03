using System.Collections.Generic;

namespace Function.Domain.Models
{
    public class Version
    {
        public string? version { get; set; }
        public List<int?>? versionParts { get; set; }
    }

    public class Attributes
    {
        public string? qualifiedName { get; set; }
        public string? name { get; set; }
    }

    public class Classification
    {
        public string? typeName { get; set; }
        public string? lastModifiedTS { get; set; }
        public string? entityGuid { get; set; }
        public string? entityStatus { get; set; }
    }

    public class Entity
    {
        public string? typeName { get; set; }
        public Attributes? attributes { get; set; }
        public string? guid { get; set; }
        public string? displayText { get; set; }
        public List<string?>? classificationNames { get; set; }
        public List<Classification?>? classifications { get; set; }
        public bool isIncomplete {get;set;}
    }

    public class Message
    {
        public string? type { get; set; }
        public Entity? entity { get; set; }
        public string? operationType { get; set; }
        public long? eventTime { get; set; }
    }

    public class Purview_Kafka_Return
    {
        public Version? version { get; set; }
        public string? msgCompressionKind { get; set; }
        public int? msgSplitIdx { get; set; }
        public int? msgSplitCount { get; set; }
        public string? msgSourceIP { get; set; }
        public string? msgCreatedBy { get; set; }
        public long? msgCreationTime { get; set; }
        public Message? message { get; set; }
    }

}