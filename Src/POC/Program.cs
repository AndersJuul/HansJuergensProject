using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EasyNetQ;
using HansJuergenWeb.Contracts;
using Newtonsoft.Json;
using System.IO;

namespace POC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new HttpClient();
            var file = @"c:\temp\dummy.txt";
            File.WriteAllText(file, "The quick brown fox jumps ...");
            var stream = File.Open(file, FileMode.Open);

            do
            {
                Console.WriteLine("Press enter");
                Console.ReadLine();

                //var httpClient = new HttpClient(new HttpClientHandler());
                //httpClient.BaseAddress = new Uri("http://localhost:53411/");

                //var uploadModel = new UploadModel {Allergene = "Anders"};
                //var stringContent = new StringContent(uploadModel.ToString());
                //httpClient.
                //var postAsync = httpClient.PostAsync(new Uri("/Upload/post",UriKind.Relative), stringContent).Result;


                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));

                    var byteArrayContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                    byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv");

                    var uploadModel = new UploadModel { Allergene = "Anders", Description = "Desc", Email = "and@juu.com" };

                    //var response = httpClient.PostAsync("http://localhost:53411/Upload/Post", new MultipartFormDataContent
                    //{
                    //    {new StringContent(JsonConvert.SerializeObject( uploadModel), Encoding.UTF8, "application/json")},
                    //    {byteArrayContent, "\"file\"", "\"feedback.csv\""}
                    //}).Result;

var content = new StringContent(JsonConvert.SerializeObject(uploadModel), Encoding.UTF8, "application/json");
var multipartFormDataContent = new MultipartFormDataContent
{
    {content},
    {byteArrayContent, "\"file\"", "\"feedback.csv\""}
};
var result = httpClient.PostAsync("http://localhost:53411/Upload/Post", multipartFormDataContent).Result;
                }

                //var file = @"c:\temp\dummy.txt";
                //File.WriteAllText(file,"The quick brown fox jumps ...");

            } while (true);

            //using (var bus = RabbitHutch.CreateBus("host=ajf-elastic-01;username=anders;password=21Bananer;timeout=10"))
            //{
            //    bus.Publish(new FileUploadedEvent
            //    {
            //        FileNames = new[] {"dummy.txt"},
            //        Email = "foo@bar.org",
            //        Description = "Lorem ipsum"
            //    });
            //}
        }
    }
}