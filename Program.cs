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
//handles any possible null value
if (smtpConfig["host"] is null || smtpConfig["port"] is null)
{
    Console.WriteLine("Please specify a SMTP server address and port in the SmtpConfig.json file.");
    return 1;
}
var host = smtpConfig["host"] ?? "";
var port = Convert.ToInt32(smtpConfig["port"] ?? "");
if (smtpConfig["User"] is null || smtpConfig["Password"] is null)
{
    Console.WriteLine("Please specify a SMTP username and password in the SmtpConfig.json file.");
    return 1;
}
var user = smtpConfig["User"] ?? "";
var password = smtpConfig["Password"] ?? "";
if (smtpConfig["From"] is null || smtpConfig["To"] is null)
{
    Console.WriteLine("Please specify the sender address and the receiver address in the SmtpConfig.json file.");
    return 1;
}
var from = smtpConfig["From"] ?? "";
var to = smtpConfig["To"] ?? "";
if (smtpConfig["UseSsl"] is null)
{
    Console.WriteLine("Please specify is SSL should be used");
    return 1;
}
var useSsl = Convert.ToBoolean(smtpConfig["UseSsl"] ?? "");
//reads command line arguments
var stockArg = new Argument<string>("stock"){Description = "The stock to monitor"};
var sellPriceArg = new Argument<double> ("SellPrice"){Description = "The price which triggers sell email"}; 
var buyPriceArg = new Argument<double>("BuyPrice"){Description = "The price which triggers buy email"};

//optional time argument
var timeArg = new Option<int>("--time"){Description = "The time period (in seconds) to make an api call", DefaultValueFactory = _ => 60};
var tokenArg = new Option<string>("--token"){Description = "The brapi token to use if necessary", DefaultValueFactory = _ => string.Empty};
var rootCommand = new RootCommand {Description = "Monitor a stock price"};

rootCommand.Add(stockArg);
rootCommand.Add(sellPriceArg);
rootCommand.Add(buyPriceArg);
rootCommand.Add(timeArg);
rootCommand.Add(tokenArg);
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
var time = parseResult.GetValue(timeArg);
var token = parseResult.GetValue(tokenArg) ?? "";
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
smtpClient.Connect(host, port, useSsl);
smtpClient.Authenticate(user, password);
StockWatcher.SmtpClient = smtpClient;
//initialize stock watcher object
var watcher = new StockWatcher(stock, buyPrice, sellPrice, time, from, to, token);
await watcher.Monitor();

return 0;
