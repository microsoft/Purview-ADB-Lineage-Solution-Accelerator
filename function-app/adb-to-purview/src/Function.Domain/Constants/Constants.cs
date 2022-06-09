using System.Diagnostics.CodeAnalysis;

namespace Function.Domain.Constants
{
public struct AuthenticationConstants
{
    public const string Audience = "Authentication:Audience";
    public const string Authority = "Authentication:Authority";
    public const string Domain = "Authentication:Domain";
    public const string Bearer = "Bearer";

}
public struct PurviewAPIConstants
{ 
    //Purview API Constants
    public const string DefaultSearchLimit = "1000";
    public const string DefaultOffset = "100";
}

 public struct HttpConstants
    {
        public const int ClientTimeoutHour = 0;
        public const int ClientTimeoutMinutes = 10;
        public const int ClientTimeoutSeconds = 0;
        public const string ContentType = "Content-Type";
        public const string ContentTypeJson = "application/json";
    }

public struct ParserConstants
    {
        public const string DBFS = "dbfs";
        public const string DBFS2 = "file";
    }
   
}