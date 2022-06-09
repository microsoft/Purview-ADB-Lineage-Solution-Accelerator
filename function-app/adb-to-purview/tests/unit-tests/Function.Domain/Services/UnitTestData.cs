using System.Collections;
using System.Collections.Generic;
using Function.Domain.Models.Settings;
using Function.Domain.Models;

namespace UnitTests.Function.Domain.Services
{
    public class FilterOlEventTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // StartFullMessage - false as it has no environment facet
            yield return new object[] {"StartFullMessage: 2022-01-12T00:05:53.282 [Information] OpenLineageIn:{\"eventType\":\"START\",\"eventTime\":\"2022-01-25T17:52:53.363Z\",\"inputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/raw/DimProduct.parquet\"}],\"outputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/destination/DimProduct.parquet\"}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , false};
            // CompleteNoOutputsInputsFullMessage
            yield return new object[] {"CompleteNoOutputsInputsFullMessage: 2022-01-12T00:05:56.318 [Information] OpenLineageIn:{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-25T17:52:53.363Z\",\"inputs\":[],\"outputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/raw/DimProduct.parquet\"}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , false};
            // CompleteOutputsAndInputsFullMessage
            yield return new object[] {"CompleteOutputsAndInputsFullMessage: 2022-01-12T00:19:41.550 [Information] OpenLineageIn:{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-25T17:52:53.363Z\",\"inputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/raw/DimProduct.parquet\"}],\"outputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/destination/DimProduct.parquet\"}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , true};
            // CompleteOutputsAndInputsSame
            yield return new object[] {"{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-25T17:52:53.363Z\",\"inputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/raw/DimProduct.parquet\"}],\"outputs\":[{\"namespace\":\"dbfs\",\"name\":\"/mnt/raw/DimProduct.parquet\"}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , false};
            // Garbage
            /* cspell: disable-next-line */
            yield return new object[] {"jfinaalksk)(^%(%6#%^&[];;asoidhntggpa;phgaqpirgp"
            , false};
            // SameInOut
            yield return new object[] {"{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-11T09:52:25.344Z\",\"run\":{},\"job\":{},\"inputs\":[{\"namespace\":\"wasbs://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"foo://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"outputs\":[{\"namespace\":\"wasbs://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"foo://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , false};
            // SameInOutDiffOrder
            yield return new object[] {"{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-11T09:52:25.344Z\",\"run\":{},\"job\":{},\"inputs\":[{\"namespace\":\"wasbs://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"foo://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"outputs\":[{\"namespace\":\"foo://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"wasbs://outputdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{},\"outputFacets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , false};
            // DiffInOut
            yield return new object[] {"{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-11T09:52:25.344Z\",\"run\":{},\"job\":{},\"inputs\":[{\"namespace\":\"wasbs://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"foo://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"outputs\":[{\"namespace\":\"wasbs://outputdata@examplessa.blob.core.windows.net\",\"name\":\"/retail/wasbdemo_updated\",\"facets\":{},\"outputFacets\":{}},{\"namespace\":\"wasbs://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , true};
            // SameInOutCasingOrderSlash
            yield return new object[] {"{\"eventType\":\"COMPLETE\",\"eventTime\":\"2022-01-11T09:52:25.344Z\",\"run\":{},\"job\":{},\"inputs\":[{\"namespace\":\"wasbs://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"foo://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"bar://rawdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"outputs\":[{\"namespace\":\"foo://rawdata@Examplessa.blob.core.windows.net/\",\"name\":\"/retail\",\"facets\":{}},{\"namespace\":\"wasbs://outputdata@examplessa.blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{},\"outputFacets\":{}},{\"namespace\":\"bar://rawdata@examplessa.Blob.core.windows.net\",\"name\":\"/retail\",\"facets\":{}}],\"producer\":\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\",\"schemaURL\":\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunEvent\"}"
            , false};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}