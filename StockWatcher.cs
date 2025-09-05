using System.Net.Http.Json;

namespace InoaChallenge;

//class which makes api calls and notifies the user 
public class StockWatcher
{
    public double BuyPrice { get; set; }
    public bool BuyTriggered { get; set; }
    public double SellPrice { get; set; }
    public bool SellTriggered { get; set; }
    public string Stock { get; set; }
    public int SecondsPeriod { get; set; }
    public static HttpClient? Client {get; set;}
    
    public StockWatcher(string stock, double buyPrice, double sellPrice, int? secondsPeriod )
    {   
        Stock = stock;
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        SecondsPeriod = secondsPeriod ?? 60;
    }
    //function which makes the object periodically make api calls and notify the user if conditions are met
    public async Task Monitor()
    {
        //http client must be set up before monitoring object
        if (Client is null)
        {
            throw new InvalidOperationException("HttpClient is not initialized");
        }
        while (true)
        {   
            //makes request
            using var response = await Client.GetAsync(Stock);
            //if there is an error, log it on console and stop monitoring
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
                return;
            }
            //extract price from response
            var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            var price = json.GetProperty("results")[0].GetProperty("regularMarketPrice");
            Console.WriteLine(price.ToString());
            Thread.Sleep(TimeSpan.FromSeconds(SecondsPeriod));
        }
        
    }
}