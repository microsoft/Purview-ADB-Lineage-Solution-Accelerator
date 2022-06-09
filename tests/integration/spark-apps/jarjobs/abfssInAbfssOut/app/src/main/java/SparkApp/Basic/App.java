package SparkApp.Basic;

import org.apache.spark.sql.Dataset;
import org.apache.spark.sql.Row;
import org.apache.spark.sql.SparkSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import com.databricks.dbutils_v1.DBUtilsHolder;
import com.databricks.dbutils_v1.DBUtilsV1;



public class App {
    private static final Logger log = LoggerFactory.getLogger("MyLogger");
    
    public String getGreeting() {
        return "Hello World!";
    }

    public static void main(String[] args) {
      String storageServiceName = System.getenv("STORAGE_SERVICE_NAME");
        String storageContainerName = "rawdata";
        String ouptutContainerName = "outputdata";
        String abfssRootPath = "abfss://"+storageContainerName+"@"+storageServiceName+".dfs.core.windows.net";
        String outputRootPath = "abfss://"+ouptutContainerName+"@"+storageServiceName+".dfs.core.windows.net";

      SparkSession spark = SparkSession
      .builder()
      .appName("JavaSparkPi")
      .getOrCreate();
        DBUtilsV1 dbutils = DBUtilsHolder.dbutils();
        System.out.println(new App().getGreeting());
        

        String storageKey = dbutils.secrets().get("purview-to-adb-scope", "storage-service-key");

        spark.conf().set("fs.azure.account.key."+storageServiceName+".dfs.core.windows.net", storageKey);

        Dataset<Row> df = spark.read().format("csv")
        .option("header", true)
        .option("inferSchema", true)
        .load(abfssRootPath+"/testcase/eighteen/exampleInputA/exampleInputA.csv");

        df.repartition(1).write().mode("overwrite").format("csv").save(outputRootPath+"/testcase/eighteen/abfss-in-abfss-out-java-jar-app/");

    }
}
