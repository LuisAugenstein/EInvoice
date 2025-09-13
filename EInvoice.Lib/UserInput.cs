using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EInvoice.Lib
{
    public class UserInput
    {
        public string Number { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public Buyer Buyer { get; set; } = new Buyer();

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
    }
}