using System.Text.Json;
using System.Text.Json.Nodes;

namespace EInvoice.Lib.Models;

public record EInvoice(int InvoiceNumber, Customer Customer, DateTime InvoiceDate, List<Position> Positions)
{
    public string ToPdf24Json(string pdf24EInvoiceTemplateJson)
    {
        try
        {
            // Parse the JSON template as a mutable JsonNode
            var jsonObject = JsonNode.Parse(pdf24EInvoiceTemplateJson);

            if (jsonObject != null)
            {
                // Update number field
                jsonObject["number"] = InvoiceNumber;

                // Update issueDate field
                jsonObject["issueDate"] = InvoiceDate.ToString("yyyy-MM-dd");

                // Update buyer information
                var buyerNode = jsonObject["buyer"];
                if (buyerNode != null)
                {
                    buyerNode["name"] = Customer.Name;
                    buyerNode["electronicAddress"] = Customer.Email;
                    buyerNode["electronicAddressTypeCode"] = "EM";

                    // Update buyer address if available
                    if (buyerNode["address"] is JsonObject addressNode)
                    {
                        addressNode["city"] = Customer.Address.City;
                        addressNode["line1"] = Customer.Address.Street;
                        addressNode["postCode"] = Customer.Address.PostCode;
                    }
                }

                // Update items/positions information
                if (jsonObject["items"] is not { } itemsNode)
                    return jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

                // Clear existing items
                if (itemsNode is JsonArray itemsArray) itemsArray.Clear();

                // Add new items based on positions
                var items = new JsonArray();
                decimal itemsNetAmount = 0;

                foreach (var (name, quantity, netUnitPrice) in Positions)
                {
                    const decimal vatRate = 19m; // 19% VAT
                    var grossUnitPrice = netUnitPrice * 1.19m;
                    var positionNetAmount = netUnitPrice * quantity;
                    var grossAmount = grossUnitPrice * quantity;
                    var vatAmount = grossAmount - positionNetAmount;

                    // Add to total items net amount
                    itemsNetAmount += positionNetAmount;

                    var item = new JsonObject
                    {
                        ["sellerId"] = "",
                        ["buyerId"] = "",
                        ["name"] = name,
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
                if (jsonObject["taxes"] is not JsonObject taxesNode)
                    return jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

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

                // Serialize back to JSON
                return jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch
        {
            // If something went wrong, return the original template
        }

        return pdf24EInvoiceTemplateJson;
    }
}

public record Position(string Name, int Quantity, decimal NetUnitPrice);