using System.CommandLine;
using InoaChallenge;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using System.Globalization;
const string API = "https://brapi.dev/api/quote/";

//sets culture so double won't be interpreted with a comma
var culture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
//smtp config json reading
var smtpConfig = new ConfigurationBuilder().AddJsonFile("SmtpConfig.json").Build();

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
StockWatcher.HttpClient = httpClient;
//initialize  the SmtpCLient
var smtpClient = new SmtpClient();
smtpClient.Connect(smtpConfig["host"], Convert.ToInt32(smtpConfig["port"]), Convert.ToBoolean(smtpConfig["UseSsl"]));
smtpClient.Authenticate(smtpConfig["User"], smtpConfig["Password"]);
StockWatcher.SmtpClient = smtpClient;
//initialize stock watcher object
var watcher = new StockWatcher(stock, buyPrice, sellPrice, null, "gustavobgrande@gmail.com", "gustavobgrande@gmail.com");
await watcher.Monitor();

return 0;
