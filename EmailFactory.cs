namespace InoaChallenge;
using MimeKit;

//helper class to create emails depending on the type
public static class EmailFactory
{
    public static MimeMessage CreateBuyEmail(string from, string to, string stock, double buyPrice)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Stock Monitor",from));
        email.To.Add(new MailboxAddress("Client",to));
        email.Subject = $"You should buy {stock}";
        email.Body = new TextPart("plain")
        {
            Text = $"The stock you were monitoring ({stock}) has reached the price of {buyPrice:N}. You should buy it."
        };
        return email;
    }
    public static MimeMessage CreateSellEmail(string from, string to, string stock, double sellPrice)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Stock Monitor",from));
        email.To.Add(new MailboxAddress("Client",to));
        email.Subject = $"You should sell {stock}";
        email.Body = new TextPart("plain")
        {
            Text = $"The stock you were monitoring ({stock}) has reached the price of {sellPrice:N}. You should sell it."
        };
        return email;
    }
}