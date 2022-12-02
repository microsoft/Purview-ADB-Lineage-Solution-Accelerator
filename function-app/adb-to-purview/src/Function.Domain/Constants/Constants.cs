// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    // Setting to 1000 (the max) will force the search scores to be 1 and thus suboptimal search results
    // Reducing from 1000 to 100 will enable better search results
    public const string DefaultSearchLimit = "100";
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