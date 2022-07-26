# Gallery of Mappings

This directory contains a "gallery" of sample OpenLineage to Purview Mappings that can be used in the `OlToPurviewMappings` App Setting on the solution's Azure Function.

## Azure SQL

* [Prioritize Azure SQL Non DBO](./az-sql-non-dbo.json)
    * Default mappings treat an Azure SQL table named myschema.mytable as schema of myschema and table of mytable.
    * Using this OpenLineage to Purview mapping would default assume dbo is the default schema instead.
