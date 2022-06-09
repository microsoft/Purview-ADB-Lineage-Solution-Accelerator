package SparkApp.ReadWrite;

import org.apache.spark.sql.Dataset;
import org.apache.spark.sql.Row;
import org.apache.spark.sql.SparkSession;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class App {
    private static final Logger log = LoggerFactory.getLogger("MyLogger");
    
    public String getGreeting() {
        return "Hello World!";
    }

    public static void main(String[] args) {
      
      SparkSession spark = SparkSession
      .builder()
      .appName("readWriteSample")
      .getOrCreate();
        System.out.println(new App().getGreeting());
        

      Dataset<Row> df = spark.read().format("csv")
        .option("header", true)
        .option("inferSchema", true)
        .load("/mnt/rawdata/testcase/nineteen/exampleInputA");

        df.repartition(1).write().mode("overwrite").format("csv").save("/mnt/rawdata/testcase/nineteen/output");

    }
}
