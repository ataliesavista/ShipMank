using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ShipMank_WPF.Models.Services
{
    public class MidtransServices
    {
        private string _serverKey;
        private string _baseUrl;

        public MidtransServices()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var config = builder.Build();
                _serverKey = config["Midtrans:ServerKey"];
                bool isProd = bool.Parse(config["Midtrans:IsProduction"]);
                _baseUrl = isProd ? "https://api.midtrans.com/v2" : "https://api.sandbox.midtrans.com/v2";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Config Error: " + ex.Message);
            }
        }

        public async Task<string> CreateVaAsync(string bank, long amount, string orderId, string type)
        {
            using (var client = new HttpClient())
            {
                var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(_serverKey + ":"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authString);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                object requestData;

                if (type == "echannel") // KHUSUS MANDIRI
                {
                    requestData = new
                    {
                        payment_type = "echannel",
                        transaction_details = new { order_id = orderId, gross_amount = amount },
                        echannel = new { bill_info1 = "Payment For:", bill_info2 = "Ship Ticket" }
                    };
                }
                else // BCA, BNI, BRI
                {
                    requestData = new
                    {
                        payment_type = "bank_transfer",
                        transaction_details = new { order_id = orderId, gross_amount = amount },
                        bank_transfer = new { bank = bank }
                    };
                }

                var jsonContent = JsonConvert.SerializeObject(requestData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string url = $"{_baseUrl}/charge";
                var response = await client.PostAsync(url, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                if (response.IsSuccessStatusCode)
                {
                    if (type == "echannel")
                    {
                        return $"{jsonResponse.biller_code} - {jsonResponse.bill_key}";
                    }
                    else
                    {
                        if (jsonResponse.va_numbers != null)
                            return jsonResponse.va_numbers[0].va_number.ToString();
                        else if (jsonResponse.permata_va_number != null)
                            return jsonResponse.permata_va_number.ToString();
                    }
                    throw new Exception("Nomor VA tidak ditemukan di response.");
                }
                else
                {
                    string errMsg = jsonResponse.status_message ?? response.ReasonPhrase;
                    throw new Exception($"Midtrans Error: {errMsg}");
                }
            }
        }
    }
}