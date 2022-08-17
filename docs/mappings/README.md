# Gallery of Mappings

This directory contains a "gallery" of sample OpenLineage to Purview Mappings that can be used in the `OlToPurviewMappings` App Setting on the solution's Azure Function.

## Azure Data Lake Gen 1

* [ADLS Gen 1 Path](./adlsg1.json)
    * Supports mapping the `adl://` path to an ADLS Gen 1 Path in Purview.

## Azure SQL

* [Prioritize Azure SQL Non DBO](./az-sql.json)
    * Default mappings treat an Azure SQL table named `myschema.mytable` as schema of `myschema` and table of `mytable`.
    * If you remove the `azureSQLNonDboNoDotsInNames` mapping, the above example would default to `dbo.[myschema.mytable]`.
