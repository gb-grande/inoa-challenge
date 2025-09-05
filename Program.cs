using System.CommandLine;

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
if (parseResult.Errors.Count != 0){
    Console.WriteLine("Program didn't receive the correct arguments");
    rootCommand.Parse("-h").Invoke();
    return 1;
}
//TODO
//checks if any of the prices are negative




return 0;
