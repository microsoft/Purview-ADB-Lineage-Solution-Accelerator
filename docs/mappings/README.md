# Gallery of Mappings

This directory contains a "gallery" of sample OpenLineage to Purview Mappings that can be used in the `OlToPurviewMappings` App Setting on the solution's Azure Function.

## Azure SQL

* [Prioritize Azure SQL Non DBO](./az-sql.json)
    * Default mappings treat an Azure SQL table named `myschema.mytable` as schema of `myschema` and table of `mytable`.
    * If you remove the `azureSQLNonDboNoDotsInNames` mapping, the above example would default to `dbo.[myschema.mytable]`.
