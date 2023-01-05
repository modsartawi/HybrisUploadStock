using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

var folderPath = @"C:\Users\sarta\Desktop\eriba\StockBatch";
var baseUrl = "https://dawaa-commerce-test.it-cpi018-rt.cfapps.eu10-003.hana.ondemand.com/http/update_stock";
int batchSize = 8; // count of files
int batchCount = 10; // count of batches
using var client = new HttpClient();
client.BaseAddress = new Uri(baseUrl);

#region Auth

var userName = "cx.implementation@dbsmena.com";
var password = "Dawaa_123";
var authenticationString = $"{userName}:{password}";
var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
// client.DefaultRequestHeaders.Add("Authorization", $"Basic {base64EncodedAuthenticationString}");
client.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

#endregion
// #####################
// ##  Hybris Push Stock  ## 
// ##  2022-12-08   ##
// #####################


var files = GetFiles(folderPath);
files = files.ToList();
int currentIndex = 0;
int counter = 0;
while (true)
{
    counter++;
    if (counter>batchCount)
    {
        break;
    }
    Console.WriteLine($"Batch No# {counter}");

   
    var filesToBeProcessed = files.Skip(currentIndex).Take(batchSize);
    var stopwatch = Stopwatch.StartNew();
    await ProcessFiles(filesToBeProcessed, client);
    stopwatch.Stop();
    Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds} Seconds");
    currentIndex += batchSize;
    await Task.Delay(1000); // delay 1 second
}


// #####################
List<string> GetFiles(string folderPath) => System.IO.Directory.GetFiles(folderPath).ToList();
Task<string> JsonString(string path) => File.ReadAllTextAsync(path);

async Task ProcessFiles(IEnumerable<string> files, HttpClient client)
{
    int index = 0;
    
    var taskList = new List<Task>();
    foreach (var file in files)
    {
        taskList.Add(
            Task.Run(async () =>
            {
                try
                {
                    var jsonString = await JsonString(file);
                    var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("", stringContent)!;
                    var responseString = await response.Content.ReadAsStringAsync();
                    var count = Interlocked.Increment(ref index);
                    Console.WriteLine(count);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
               
            }));

    }

    await Task.WhenAll(taskList);
}