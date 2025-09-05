using System.Net.Http.Json;
using MailKit.Net.Smtp; 
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
    public string FromEmail { get; set; }
    public string ToEmail { get; set; }
    public static HttpClient? HttpClient {get; set;}
    public static SmtpClient? SmtpClient {get; set;}
    public static double Tolerance {get; set;}
    protected string Token {get; set;}
    public StockWatcher(string stock, double buyPrice, double sellPrice, int? secondsPeriod, string fromEmail, string toEmail, string token)
    {   
        Stock = stock;
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        SecondsPeriod = secondsPeriod ?? 60;
        FromEmail = fromEmail;
        ToEmail = toEmail;
        Token = token;
    }
    //function which makes the object periodically make api calls and notify the user if conditions are met
    public async Task Monitor()
    {
        //http client and smtp must be set up before monitoring
        if (HttpClient is null)
        {
            throw new InvalidOperationException("HttpClient is not initialized");
        }

        if (SmtpClient is null)
        {
            throw new InvalidOperationException("SmtpClient is not initialized");
        }
        while (true)
        {   
            //makes request
            var request = new HttpRequestMessage(HttpMethod.Get, Stock);
            //if there is a token specified, adds it to request
            if (!string.IsNullOrEmpty(Token))
            {
                request.Headers.Add("Authorization", $"Bearer {Token}");
                
            }

            using var response = await HttpClient.SendAsync(request);
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
            var price = json.GetProperty("results")[0].GetProperty("regularMarketPrice").GetDouble();
            Console.WriteLine($"Current price: {price}");
            //logic for sending email
            //price just reached the minimum for buying
            if (price <= BuyPrice && !BuyTriggered)
            {
                Console.WriteLine($"{price} <= {BuyPrice} sending buy email");
                BuyTriggered = true;
                var buyEmail = EmailFactory.CreateBuyEmail(FromEmail, ToEmail, Stock, BuyPrice);
                await SmtpClient.SendAsync(buyEmail);
            }
            //price went back up, but it was triggered before, so it needs to be updated if it drops again
            //3% tolerance to avoid spamming emails if price fluctuates
            else if (Math.Abs(price - BuyPrice) > Tolerance * BuyPrice && BuyTriggered)
            {
                BuyTriggered = false;
            }
            //price has reached minimum for selling 
            if (price >= SellPrice && !SellTriggered)
            {
                Console.WriteLine($"{price} >= {SellPrice} sending sell email");
                SellTriggered = true;
                var sellEmail = EmailFactory.CreateSellEmail(FromEmail, ToEmail, Stock, SellPrice);
                await SmtpClient.SendAsync(sellEmail);
            }
            //same logic as before
            else if (Math.Abs(price - SellPrice) > Tolerance * SellPrice && SellTriggered)
            {
                SellTriggered = false;
            }
            //sleeps until the next api call
            Thread.Sleep(TimeSpan.FromSeconds(SecondsPeriod));
        }
        
    }
}