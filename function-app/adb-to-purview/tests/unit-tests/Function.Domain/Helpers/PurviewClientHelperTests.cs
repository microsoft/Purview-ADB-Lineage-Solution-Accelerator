using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using System.Net.Http;
using Function.Domain.Providers;
using Function.Domain.Helpers;
using Function.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;

namespace UnitTests.Function.Domain.Helpers
{
    public class PurviewClientHelperTests
    {
        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IHttpClientManager> httpClientManagerMock;
        private readonly PurviewClientHelper purviewClientHelper;
        
    public PurviewClientHelperTests()
    {
        this.loggerMock = new Mock<ILogger>();
        this.httpClientManagerMock = new Mock<IHttpClientManager> { CallBase = true };
        this.httpClientManagerMock.Setup(a => a.PostAsync(It.IsAny<Uri>(),It.IsAny<HttpContent>(),It.IsAny<string>(), default)).ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        this.purviewClientHelper = new PurviewClientHelper(httpClientManagerMock.Object, loggerMock.Object);
    }

        [Theory]
        [AutoMoqData]
        public async Task PublishEntitiesToPurview_Success()
        {
            var correlationId = Guid.NewGuid().ToString();
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCIsImtpZCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCJ9.eyJhdWQiOiI3M2MyOTQ5ZS1kYTJkLTQ1N2EtOTYwNy1mY2M2NjUxOTg5NjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNjQyMTE2MDI2LCJuYmYiOjE2NDIxMTYwMjYsImV4cCI6MTY0MjIwMjcyNiwiYWlvIjoiRTJaZ1lKaFRiWjNGci9PcGZNS0M1Ry9IWmp3MkJ3QT0iLCJhcHBpZCI6IjIzOWYxNGU3LTAyNTMtNGIwNi1hOWQwLWFlNDFhNTVjMWUzOCIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0LzcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0Ny8iLCJvaWQiOiIyYjY0ZDdiZi0yYmQ3LTQ4OTEtOWJiOC1kNjZlODU3OTFlNmQiLCJyaCI6IjAuQVFFQXY0ajVjdkdHcjBHUnF5MTgwQkhiUi1jVW55TlRBZ1pMcWRDdVFhVmNIamdhQUFBLiIsInN1YiI6IjJiNjRkN2JmLTJiZDctNDg5MS05YmI4LWQ2NmU4NTc5MWU2ZCIsInRpZCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsInV0aSI6InNkQUhyYXFlRTBHT3F4WWwxWE1qQUEiLCJ2ZXIiOiIxLjAifQ.HUqOuTTP1VgHlj_AJt6txjpJZdJ6oypBUZL1CHL6heNK4M0LwvEfqASReXzVEXiaTVtWgHIWRCEEux17HOF5NIXX2iVVPdHulO39i39UXyB9oCzmzZG6-raJIVZ1B7PbjPVmKZmnSZYBeIHU0UHZD4wQyZpA73otPWzGJaO5jegBxnMYFhv3Gd5oGogA6fHkPzw6rXk_5dAxHgptNyed8E0bBfq-M1GVt1gXVY7WnVQ2N7qUMYYuow5IMAFa6NeYDP5G4mydavyNdeoEZNOHjDnYYAGbVySVbeAIA6y7FQoZc-nREn3JqZH1L0gmvS-0HixejU4neaxKSFZg3gYK-w";
            var bulkUpdateEndpoint = "https://adb-to-purview.catalog.purview.azure.com/api/atlas/v2/entity/bulk";
            //Arrange
            string jsonData = "{\"entities\":[{\"typeName\":\"spark_application\",\"guid\":-1000,\"attributes\":{\"name\":\"S01DimOrg_raw2feature\",\"qualifiedName\":\"notebook://Shared/Cummins/S01DimOrg_raw2feature\",\"data_type\":\"spark_application\",\"description\":\"DataAssetsS01DimOrg_raw2feature\"},\"relationshipAttributes\":{}},{\"typeName\":\"spark_process\",\"guid\":-1001,\"attributes\":{\"name\":\"/Shared/Cummins/S01DimOrg_raw2feature/DimOrganization.parquet\",\"qualifiedName\":\"sparkprocess://https://purviewexamplessa.dfs.core.windows.net/rawdata/DimOrganization.parquet/:https://purviewexamplessa.dfs.core.windows.net/rawdata/DimOrganization.parquet/\",\"columnMapping\":\"\",\"executionId\":\"0\",\"currUser\":\"marktayl@microsoft.com\",\"sparkPlanDescription\":\"{\\\"_producer\\\":\\\"https://github.com/OpenLineage/OpenLineage/tree/0.5.0-SNAPSHOT/integration/spark\\\",\\\"_schemaURL\\\":\\\"https://openlineage.io/spec/1-0-2/OpenLineage.json#/$defs/RunFacet\\\",\\\"plan\\\":[{\\\"class\\\":\\\"org.apache.spark.sql.execution.datasources.LogicalRelation\\\",\\\"num-children\\\":0,\\\"relation\\\":null,\\\"output\\\":[[{\\\"class\\\":\\\"org.apache.spark.sql.catalyst.expressions.AttributeReference\\\",\\\"num-children\\\":0,\\\"name\\\":\\\"OrganizationKey\\\",\\\"dataType\\\":\\\"integer\\\",\\\"nullable\\\":true,\\\"metadata\\\":{},\\\"exprId\\\":{\\\"product-class\\\":\\\"org.apache.spark.sql.catalyst.expressions.ExprId\\\",\\\"id\\\":171,\\\"jvmId\\\":\\\"ff36bd74-981c-4aa5-9924-cab43c9a7c4e\\\"},\\\"qualifier\\\":[]}],[{\\\"class\\\":\\\"org.apache.spark.sql.catalyst.expressions.AttributeReference\\\",\\\"num-children\\\":0,\\\"name\\\":\\\"ParentOrganizationKey\\\",\\\"dataType\\\":\\\"integer\\\",\\\"nullable\\\":true,\\\"metadata\\\":{},\\\"exprId\\\":{\\\"product-class\\\":\\\"org.apache.spark.sql.catalyst.expressions.ExprId\\\",\\\"id\\\":172,\\\"jvmId\\\":\\\"ff36bd74-981c-4aa5-9924-cab43c9a7c4e\\\"},\\\"qualifier\\\":[]}],[{\\\"class\\\":\\\"org.apache.spark.sql.catalyst.expressions.AttributeReference\\\",\\\"num-children\\\":0,\\\"name\\\":\\\"PercentageOfOwnership\\\",\\\"dataType\\\":\\\"double\\\",\\\"nullable\\\":true,\\\"metadata\\\":{},\\\"exprId\\\":{\\\"product-class\\\":\\\"org.apache.spark.sql.catalyst.expressions.ExprId\\\",\\\"id\\\":173,\\\"jvmId\\\":\\\"ff36bd74-981c-4aa5-9924-cab43c9a7c4e\\\"},\\\"qualifier\\\":[]}],[{\\\"class\\\":\\\"org.apache.spark.sql.catalyst.expressions.AttributeReference\\\",\\\"num-children\\\":0,\\\"name\\\":\\\"OrganizationName\\\",\\\"dataType\\\":\\\"string\\\",\\\"nullable\\\":true,\\\"metadata\\\":{},\\\"exprId\\\":{\\\"product-class\\\":\\\"org.apache.spark.sql.catalyst.expressions.ExprId\\\",\\\"id\\\":174,\\\"jvmId\\\":\\\"ff36bd74-981c-4aa5-9924-cab43c9a7c4e\\\"},\\\"qualifier\\\":[]}],[{\\\"class\\\":\\\"org.apache.spark.sql.catalyst.expressions.AttributeReference\\\",\\\"num-children\\\":0,\\\"name\\\":\\\"CurrencyKey\\\",\\\"dataType\\\":\\\"integer\\\",\\\"nullable\\\":true,\\\"metadata\\\":{},\\\"exprId\\\":{\\\"product-class\\\":\\\"org.apache.spark.sql.catalyst.expressions.ExprId\\\",\\\"id\\\":175,\\\"jvmId\\\":\\\"ff36bd74-981c-4aa5-9924-cab43c9a7c4e\\\"},\\\"qualifier\\\":[]}]],\\\"isStreaming\\\":false}]}\",\"inputs\":[{\"typeName\":\"purview_custom_connector_generic_entity_with_columns\",\"uniqueAttributes\":{\"qualifiedName\":\"https://purviewexamplessa.dfs.core.windows.net/rawdata/DimOrganization.parquet/\"}}],\"outputs\":[{\"typeName\":\"purview_custom_connector_generic_entity_with_columns\",\"uniqueAttributes\":{\"qualifiedName\":\"https://purviewexamplessa.dfs.core.windows.net/outputdata/DimOrganization.parquet/\"}}]},\"relationshipAttributes\":{\"application\":{\"qualifiedName\":\"notebook://Shared/Cummins/S01DimOrg_raw2feature\",\"guid\":-1000}}}],\"referredEntities\":{}}";
            
            dynamic? requestModel = JsonConvert.DeserializeObject<ExpandoObject>(jsonData, new ExpandoObjectConverter());

                
            //Generate Response For Mock Object
            var responseModel = new HttpResponseMessage();

            //Need to get the Bulk Update Endpoint from the KeyVault using function config

            //Actual
            var actual = await purviewClientHelper.PostEntitiesToPurview(correlationId,token,requestModel,bulkUpdateEndpoint);

            //Assert
            Xunit.Assert.Equal(actual.IsSuccessStatusCode, true);
        }

        [Theory]
        [AutoMoqData]
        public async Task GetEntitiesFromPurview_Success()
        {
            //Arrange
            var correlationId = Guid.NewGuid().ToString();
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCIsImtpZCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCJ9.eyJhdWQiOiI3M2MyOTQ5ZS1kYTJkLTQ1N2EtOTYwNy1mY2M2NjUxOTg5NjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNjQyMTE2MDI2LCJuYmYiOjE2NDIxMTYwMjYsImV4cCI6MTY0MjIwMjcyNiwiYWlvIjoiRTJaZ1lKaFRiWjNGci9PcGZNS0M1Ry9IWmp3MkJ3QT0iLCJhcHBpZCI6IjIzOWYxNGU3LTAyNTMtNGIwNi1hOWQwLWFlNDFhNTVjMWUzOCIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0LzcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0Ny8iLCJvaWQiOiIyYjY0ZDdiZi0yYmQ3LTQ4OTEtOWJiOC1kNjZlODU3OTFlNmQiLCJyaCI6IjAuQVFFQXY0ajVjdkdHcjBHUnF5MTgwQkhiUi1jVW55TlRBZ1pMcWRDdVFhVmNIamdhQUFBLiIsInN1YiI6IjJiNjRkN2JmLTJiZDctNDg5MS05YmI4LWQ2NmU4NTc5MWU2ZCIsInRpZCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsInV0aSI6InNkQUhyYXFlRTBHT3F4WWwxWE1qQUEiLCJ2ZXIiOiIxLjAifQ.HUqOuTTP1VgHlj_AJt6txjpJZdJ6oypBUZL1CHL6heNK4M0LwvEfqASReXzVEXiaTVtWgHIWRCEEux17HOF5NIXX2iVVPdHulO39i39UXyB9oCzmzZG6-raJIVZ1B7PbjPVmKZmnSZYBeIHU0UHZD4wQyZpA73otPWzGJaO5jegBxnMYFhv3Gd5oGogA6fHkPzw6rXk_5dAxHgptNyed8E0bBfq-M1GVt1gXVY7WnVQ2N7qUMYYuow5IMAFa6NeYDP5G4mydavyNdeoEZNOHjDnYYAGbVySVbeAIA6y7FQoZc-nREn3JqZH1L0gmvS-0HixejU4neaxKSFZg3gYK-w";
            var qualifiedName = "Sample Qualified Name";
            var purviewSearchEndpoint = "https://adb-to-purview.catalog.purview.azure.com/api/atlas/v2/search/advanced";
            var totalEntities = 0;

            //Generate Response For Mock Object
            var responseModel = new PurviewEntitySearchResponseModel();
            //Need to get the Entity Search Endpoint from the KeyVault using function config

            //Actual
            var actual = await purviewClientHelper.GetEntitiesFromPurview(correlationId,qualifiedName,purviewSearchEndpoint,token);

            //Assert
            Xunit.Assert.Equal(actual.SearchCount, totalEntities);
        }

        [Theory]
        [AutoMoqData]
        public void DeleteEntityByGuidInPurview_Success()
        {
            //Arrange
            var correlationId = Guid.NewGuid().ToString();
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCIsImtpZCI6Ik1yNS1BVWliZkJpaTdOZDFqQmViYXhib1hXMCJ9.eyJhdWQiOiI3M2MyOTQ5ZS1kYTJkLTQ1N2EtOTYwNy1mY2M2NjUxOTg5NjciLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNjQyMTE2MDI2LCJuYmYiOjE2NDIxMTYwMjYsImV4cCI6MTY0MjIwMjcyNiwiYWlvIjoiRTJaZ1lKaFRiWjNGci9PcGZNS0M1Ry9IWmp3MkJ3QT0iLCJhcHBpZCI6IjIzOWYxNGU3LTAyNTMtNGIwNi1hOWQwLWFlNDFhNTVjMWUzOCIsImFwcGlkYWNyIjoiMSIsImlkcCI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0LzcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0Ny8iLCJvaWQiOiIyYjY0ZDdiZi0yYmQ3LTQ4OTEtOWJiOC1kNjZlODU3OTFlNmQiLCJyaCI6IjAuQVFFQXY0ajVjdkdHcjBHUnF5MTgwQkhiUi1jVW55TlRBZ1pMcWRDdVFhVmNIamdhQUFBLiIsInN1YiI6IjJiNjRkN2JmLTJiZDctNDg5MS05YmI4LWQ2NmU4NTc5MWU2ZCIsInRpZCI6IjcyZjk4OGJmLTg2ZjEtNDFhZi05MWFiLTJkN2NkMDExZGI0NyIsInV0aSI6InNkQUhyYXFlRTBHT3F4WWwxWE1qQUEiLCJ2ZXIiOiIxLjAifQ.HUqOuTTP1VgHlj_AJt6txjpJZdJ6oypBUZL1CHL6heNK4M0LwvEfqASReXzVEXiaTVtWgHIWRCEEux17HOF5NIXX2iVVPdHulO39i39UXyB9oCzmzZG6-raJIVZ1B7PbjPVmKZmnSZYBeIHU0UHZD4wQyZpA73otPWzGJaO5jegBxnMYFhv3Gd5oGogA6fHkPzw6rXk_5dAxHgptNyed8E0bBfq-M1GVt1gXVY7WnVQ2N7qUMYYuow5IMAFa6NeYDP5G4mydavyNdeoEZNOHjDnYYAGbVySVbeAIA6y7FQoZc-nREn3JqZH1L0gmvS-0HixejU4neaxKSFZg3gYK-w";
            var entityGuid = Guid.NewGuid().ToString();
            var purviewDeleteEndpoint = "https://adb-to-purview.catalog.purview.azure.com/api/atlas/v2/entity/guid/";


            //Need to get the Entity Delete Endpoint from the KeyVault using function config

            //Actual
            var actual = purviewClientHelper.DeleteEntityByGuidInPurview(correlationId,token,entityGuid,purviewDeleteEndpoint);

            //Assert
            Xunit.Assert.Equal(true, actual.IsCompletedSuccessfully);
        }
    }
}
