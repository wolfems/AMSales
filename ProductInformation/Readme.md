# Amazon DynamoDB Blog API Serverless Application Project

This starter project consists of:
* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* Function.cs - class file containing the C# methods mapped to the Serverless functions declared in the template file
* Blog.cs - file containing a C# class representing a blog entry in the DynamoDB table
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS

You may also have a test project depending on the options selected.

The generated project contains a Serverless template declaration for a simple web API for blogging with the blog data stored in a DynamoDB table. The blogging API functions are hosted as a set of AWS Lambda functions that will be exposed through Amazon API Gateway as HTTP operations. Edit the template to customize the functions or add more functions and other resources needed by your application, and edit the function code in Function.cs/Blog.cs. You can then deploy your Serverless application.

## Here are some steps to follow from Visual Studio:

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "ProductInformation/test/ProductInformation.Tests"
    dotnet test
```

Deploy application
```
    cd "ProductInformation/src/ProductInformation"
    dotnet lambda deploy-serverless
```



Removed from serverless.template 10/8

    

    "AddBlog" : {
      "Type" : "AWS::Serverless::Function",
      "Properties": {
        "Handler": "ProductInformation::ProductInformation.Functions::AddBlogAsync",
        "Runtime": "dotnetcore2.1",
        "CodeUri": "",
        "Description": "Function to add a blog",
        "MemorySize": 256,
        "Timeout": 30,
        "Role": null,
        "Policies": [ "AWSLambdaFullAccess" ],
        "Environment" : {
          "Variables" : {
            "BlogTable" : { "Fn::If" : ["CreateBlogTable", {"Ref":"BlogTable"}, { "Ref" : "BlogTableName" } ] }
          }
        },
        "Events": {
          "PutResource": {
            "Type": "Api",
            "Properties": {
              "Path": "/",
              "Method": "PUT"
            }
          }
        }
      }
    },

    "RemoveBlog" : {
      "Type" : "AWS::Serverless::Function",
      "Properties": {
        "Handler": "ProductInformation::ProductInformation.Functions::RemoveBlogAsync",
        "Runtime": "dotnetcore2.1",
        "CodeUri": "",
        "Description": "Function to remove a blog",
        "MemorySize": 256,
        "Timeout": 30,
        "Role": null,
        "Policies": [ "AWSLambdaFullAccess" ],
        "Environment" : {
          "Variables" : {
            "BlogTable" : { "Fn::If" : ["CreateBlogTable", {"Ref":"BlogTable"}, { "Ref" : "BlogTableName" } ] }
          }
        },
        "Events": {
          "PutResource": {
            "Type": "Api",
            "Properties": {
              "Path": "/",
              "Method": "DELETE"
            }
          }
        }
      }
    },
