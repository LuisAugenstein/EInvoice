namespace EInvoice.Lib.Models;

public class Customer
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required Address Address { get; set; }
}

public class Address
{
    public required string City { get; set; }
    public required string Street { get; set; }
    public required int PostCode { get; set; }
}