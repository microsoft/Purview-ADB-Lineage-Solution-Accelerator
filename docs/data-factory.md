# Data Factory and Databricks Notebook Lineage

The solution accelerator supports capturing lineage for Databricks Notebook activities in Azure Data Factory (ADF). After running a notebook through ADF on an interactive or job cluster, you will see a Databricks Job asset in Microsoft Purview with a name similar to `ADF_<factory name>_<pipeline name>`. For each Databricks notebook activity, you will also see a Databricks Task with a name similar to `ADF_<factory name>_<pipeline name>_<activity name>`.

* At this time, the Microsoft Purview view of Azure Data Factory lineage will not contain these tasks unless the Databricks Task uses or feeds a data source to a Data Flow or Copy activity.
* Copy Activities may not show lineage connecting to these Databricks tasks since it emits individual file assets rather than folder or resource set assets.

## Enable Collecting Data Factory Lineage

To enable Data Factory lineage, you must add the [Service Principal to the Databricks Workspace](./databricks-jobs-service-principal.md) and add it to at least the `users` group.
