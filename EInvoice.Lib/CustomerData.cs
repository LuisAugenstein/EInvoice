using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EInvoice.Lib
{
    public class CustomerData
    {
        public string Name { get; set; } = string.Empty;
        public string ElectronicAddress { get; set; } = string.Empty;

        public static List<CustomerData> LoadFromJson(string jsonContent)
        {
            try
            {
                var customers = JsonSerializer.Deserialize<List<CustomerData>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return customers ?? new List<CustomerData>();
            }
            catch
            {
                return new List<CustomerData>();
            }
        }
    }
}