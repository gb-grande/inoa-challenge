using System.CommandLine;
using InoaChallenge;
using Microsoft.Extensions.Configuration;
const string API = "https://brapi.dev/api/quote/";

//smtp client
var smtpConfig = new ConfigurationBuilder().AddJsonFile("SmtpConfig.json").Build();

Console.WriteLine(smtpConfig["host"]);
//reads command line arguments
var stockArg = new Argument<string>("stock"){Description = "The stock to monitor"};
var sellPriceArg = new Argument<double> ("SellPrice"){Description = "The price which triggers sell email"}; 
var buyPriceArg = new Argument<double>("BuyPrice"){Description = "The price which triggers buy email"};

var rootCommand = new RootCommand {Description = "Monitor a stock price"};

rootCommand.Add(stockArg);
rootCommand.Add(sellPriceArg);
rootCommand.Add(buyPriceArg);

var parseResult = rootCommand.Parse(args);

//if there are any errors, print the program tooltip
if (parseResult.Errors.Count != 0)
{
    Console.WriteLine("Program didn't receive the correct arguments");
    rootCommand.Parse("-h").Invoke();
    return 1;
}

//get the actual values from the args
var stock = parseResult.GetValue(stockArg);
var sellPrice = parseResult.GetValue(sellPriceArg);
var buyPrice = parseResult.GetValue(buyPriceArg);
if (stock is null)
{
    Console.WriteLine("Arguments can't be null");
    return 1;
}
//checks if any of the prices are negative
if (sellPrice < 0 || buyPrice < 0)
{
    Console.WriteLine("Stock price can't be negative");
    return 1;
}

//initialize the http client
var httpClient = new HttpClient(){BaseAddress = new Uri(API)};
StockWatcher.Client = httpClient;




//initialize stock water object
var watcher = new StockWatcher(stock, buyPrice, sellPrice, null);
await watcher.Monitor();

return 0;
