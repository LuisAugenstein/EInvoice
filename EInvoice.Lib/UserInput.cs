using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EInvoice.Lib
{
    public class UserInput
    {
        public string Number { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public Buyer Buyer { get; set; } = new Buyer();
        public List<InvoicePosition> Positions { get; set; } = new List<InvoicePosition>();

        public string UpdateInvoiceTemplate(string templateJson)
        {
            try
            {
                // Parse the JSON template as a mutable JsonNode
                var jsonObject = JsonNode.Parse(templateJson);
                
                if (jsonObject != null)
                {
                    // Update number field
                    jsonObject["number"] = Number ?? string.Empty;
                    
                    // Update issueDate field
                    jsonObject["issueDate"] = IssueDate.ToString("yyyy-MM-dd");
                    
                    // Update buyer information
                    var buyerNode = jsonObject["buyer"];
                    if (buyerNode != null)
                    {
                        buyerNode["name"] = Buyer.Name ?? string.Empty;
                        buyerNode["electronicAddress"] = Buyer.ElectronicAddress ?? string.Empty;
                        buyerNode["electronicAddressTypeCode"] = "EM";
                        
                        // Update buyer address if available
                        if (buyerNode["address"] is JsonObject addressNode)
                        {
                            addressNode["city"] = Buyer.Address.City ?? string.Empty;
                            addressNode["line1"] = Buyer.Address.Line1 ?? string.Empty;
                            addressNode["postCode"] = Buyer.Address.PostCode ?? string.Empty;
                        }
                    }
                    
                    // Update items/positions information
                    if (jsonObject["items"] is JsonNode itemsNode)
                    {
                        // Clear existing items
                        if (itemsNode is JsonArray itemsArray)
                        {
                            itemsArray.Clear();
                        }
                        
                        // Add new items based on positions
                        var items = new JsonArray();
                        decimal itemsNetAmount = 0;
                        
                        foreach (var position in Positions)
                        {
                            var vatRate = 19m; // 19% VAT
                            var netUnitPrice = position.NetUnitPrice;
                            var grossUnitPrice = netUnitPrice * 1.19m;
                            var quantity = position.Quantity;
                            var positionNetAmount = netUnitPrice * quantity;
                            var grossAmount = grossUnitPrice * quantity;
                            var vatAmount = grossAmount - positionNetAmount;
                            
                            // Add to total items net amount
                            itemsNetAmount += positionNetAmount;
                            
                            var item = new JsonObject
                            {
                                ["sellerId"] = "",
                                ["buyerId"] = "",
                                ["name"] = position.Name ?? "",
                                ["description"] = "",
                                ["orderPosition"] = "",
                                ["basisQuantity"] = 1,
                                ["quantity"] = quantity,
                                ["quantityUnit"] = "H87",
                                ["quantityUnitSymbol"] = "",
                                ["vatCode"] = "S",
                                ["vatRate"] = vatRate,
                                ["netUnitPrice"] = netUnitPrice,
                                ["grossUnitPrice"] = grossUnitPrice,
                                ["netAmount"] = positionNetAmount,
                                ["grossAmount"] = grossAmount,
                                ["vatAmount"] = vatAmount,
                                ["billingPeriodStart"] = "",
                                ["billingPeriodEnd"] = "",
                                ["objectReferences"] = new JsonArray(),
                                ["allowances"] = new JsonArray(),
                                ["charges"] = new JsonArray(),
                                ["enteredUnitPrice"] = "net"
                            };
                            
                            items.Add(item);
                        }
                        
                        jsonObject["items"] = items;
                        
                        // Update totals
                        var netAmount = itemsNetAmount;
                        var vatAmountTotal = netAmount * 0.19m;
                        var grossAmountTotal = netAmount + vatAmountTotal;
                        
                        if (jsonObject["totals"] is JsonObject totalsNode)
                        {
                            totalsNode["allowancesNetAmount"] = 0;
                            totalsNode["chargesNetAmount"] = 0;
                            totalsNode["dueAmount"] = grossAmountTotal;
                            totalsNode["grossAmount"] = grossAmountTotal;
                            totalsNode["itemsNetAmount"] = itemsNetAmount;
                            totalsNode["netAmount"] = netAmount;
                            totalsNode["paidAmount"] = 0;
                            totalsNode["roundingAmount"] = 0;
                            totalsNode["vatAmount"] = vatAmountTotal;
                        }
                        
                        // Update taxes
                        if (jsonObject["taxes"] is JsonObject taxesNode)
                        {
                            var taxEntry = new JsonObject
                            {
                                ["code"] = "S",
                                ["rate"] = 19,
                                ["netAmount"] = netAmount,
                                ["vatAmount"] = vatAmountTotal,
                                ["exemptionReason"] = "",
                                ["exemptionReasonCode"] = ""
                            };
                            
                            taxesNode["S-19"] = taxEntry;
                        }
                    }
                    
                    // Serialize back to JSON
                    return jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch
            {
                // If something went wrong, return the original template
            }
            
            return templateJson;
        }
    }

    public class Buyer
    {
        public string Name { get; set; } = string.Empty;
        public string ElectronicAddress { get; set; } = string.Empty;
        public Address Address { get; set; } = new EInvoice.Lib.Address();
    }

    public class InvoicePosition
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal NetUnitPrice { get; set; }
    }
}