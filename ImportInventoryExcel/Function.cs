using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

namespace ImportInventoryExcel
{
    public class Function
    {
        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main(string[] args)
        {
            Func<Newtonsoft.Json.Linq.JObject, ILambdaContext, string> func = FunctionHandler;
            using(var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func, new JsonSerializer()))
            using(var bootstrap = new LambdaBootstrap(handlerWrapper))
            {
                await bootstrap.RunAsync();
            }
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        ///
        /// To use this handler to respond to an AWS event, reference the appropriate package from 
        /// https://github.com/aws/aws-lambda-dotnet#events
        /// and change the string input parameter to the desired event type.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string FunctionHandler(Newtonsoft.Json.Linq.JObject evnt, ILambdaContext context)
        {
            var response = evnt.ToString();
            context.Logger.Log("Complete JSON of the event is:"+evnt.ToString());
            var records = evnt.Property("Records");
            context.Logger.Log("\nRecord Array:"+records.Value);
            // TODO check to see if only one, handle multiple
            Newtonsoft.Json.Linq.JObject record = (Newtonsoft.Json.Linq.JObject )records.First[0];
            var awsRegionString = record.Property("awsRegion").Value;
            context.Logger.Log("\nAWS Region String is:" + awsRegionString);

            Newtonsoft.Json.Linq.JObject tmpObject = (Newtonsoft.Json.Linq.JObject)record.Property("s3").Value;
            Newtonsoft.Json.Linq.JObject tmpObject2 = (Newtonsoft.Json.Linq.JObject)tmpObject.Property("bucket").Value;
            string bucketName = tmpObject2.Property("name").Value.ToString();
            context.Logger.Log("\nBucket Property Name:" + bucketName);

            tmpObject2 = (Newtonsoft.Json.Linq.JObject)tmpObject.Property("object").Value;
            string keyName= tmpObject2.Property("key").Value.ToString();
            context.Logger.Log("\nKey Property Name:" + keyName);

            // TODO assign a region based on text found
            Amazon.RegionEndpoint awsRegion = Amazon.RegionEndpoint.USEast1;


            ReadObjectDataAsync(awsRegion, bucketName, keyName).Wait();


            return response;
        }

        static async Task ReadObjectDataAsync(Amazon.RegionEndpoint awsRegion, string bucketName, string keyName)
        {
            IAmazonS3 client = new AmazonS3Client(awsRegion);
            
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = await client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (MemoryStream memStream = new MemoryStream())
                {
                    responseStream.CopyTo(memStream);
                    ParseExcelStream(memStream);
                    
                //using (StreamReader reader = new StreamReader(responseStream))
                //{
                //    string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
                //    string contentType = response.Headers["Content-Type"];
                //    Console.WriteLine("Object metadata, Title: {0}", title);
                //    Console.WriteLine("Content type: {0}", contentType);

                //    responseBody = reader.ReadToEnd(); // Now you process the response body.
                }
                //processed/test.txt
                //S3FileInfo currentObject = new S3FileInfo(client, bucketName, keyName);
                CopyObjectRequest cpreq = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = keyName,
                    DestinationBucket = bucketName,
                    DestinationKey = "processed/" + keyName.Replace(".xlsx", ".processed")
                };
                CopyObjectResponse cpresp = await client.CopyObjectAsync(cpreq);

                var req2 = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                var resp2 = await client.DeleteObjectAsync(req2);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading an object", e.Message);
            }
        }
        static void ParseExcelStream(Stream fileStream)
        {
            ISheet sheet;
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
            clientConfig.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
            AmazonDynamoDBClient client = new AmazonDynamoDBClient(clientConfig);
            Table table = Table.LoadTable(client, "AMProducts");
            fileStream.Position = 0;
            XSSFWorkbook hssfwb = new XSSFWorkbook(fileStream); //This will read 2007 Excel format  
            sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;
                if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                //for (int j = row.FirstCellNum; j < cellCount; j++)
                if (row.GetCell(row.FirstCellNum) != null)
                {
                    if (int.TryParse(row.GetCell(row.FirstCellNum).ToString(), out int tmpkey))
                    {
                        if (row.GetCell(row.FirstCellNum + 3) != null)
                        {
                            var meat = new Document();
                            meat["SKU"] = tmpkey;
                            meat["Name"] = row.GetCell(row.FirstCellNum + 1).ToString();
                            meat["Quantity"] = row.GetCell(row.FirstCellNum + 3).ToString();
                            float inv = 0;
                            if(float.TryParse(row.GetCell(row.FirstCellNum + 3).ToString(), out inv))
                            {
                                if (inv > 0)
                                    meat["InStock"] = "true";
                                else
                                    meat["InStock"] = "false";
                            }
                            table.UpdateItemAsync(meat).Wait();
                        }
                        else
                            Console.WriteLine("\nQuantity cell was null");

                    }
                    else
                        Console.WriteLine("\nRow's cell did not contain an integer");
                }
            }
        }
    }
}
