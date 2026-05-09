namespace ProjectEmptiness.Data;

public class TradeGood
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public GoodCategory Category { get; set; }
    public float BasePrice { get; set; }
    public float PriceVolatility { get; set; } = 0.2f; // how much price fluctuates
    public int CargoSize { get; set; } = 1;
    public bool IsIllegal { get; set; }
    public string Description { get; set; } = "";
}
