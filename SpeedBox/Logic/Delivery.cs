using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeedBox.Controllers;
using SpeedBox.Interfases;
using SpeedBox.Models;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

namespace SpeedBox.Logic
{
    public class Delivery : IDelivery
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HomeController> _logger;

        public Delivery(ILogger<HomeController> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }

        public async Task<Dictionary<string,int>> GetCodeCity(string cityFrom, string cityTo)
        {
            Dictionary<string, int> code = new Dictionary<string, int>
            {
                { "to", 0 },
                { "from", 0 } 
            };
            try
            {
                var responceCity = await _httpClient.GetAsync("https://api.cdek.ru/v2/location/cities/?country_codes=RU,TR");
                if (responceCity.IsSuccessStatusCode)
                {
                    string responceCityContent = await responceCity.Content.ReadAsStringAsync();
                    JArray dataResponceCity = JArray.Parse(responceCityContent);
                    bool keyExists = dataResponceCity.Children<JObject>().Any(o => o.ContainsKey("fias_guid"));

                  
                    foreach (var item in dataResponceCity)
                    {
                        if (item.ToString().Contains("fias_guid"))
                        {
                            if (item["fias_guid"].ToString() == cityFrom)
                            {
                                code["from"] = int.Parse(item["code"].ToString());
                            }
                            else if (item["fias_guid"].ToString() == cityTo)
                            {
                                code["to"] = int.Parse(item["code"].ToString());
                            }
                        }
                    }
                }             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return code;

        }

        public async Task<string> GetShippingCost(int weight, int length, int width, int height, string cityFrom, string cityTo)
        {
            try
            {
                await LogIn();

                var cities = await GetCodeCity(cityFrom, cityTo);

                FromLocation from_Location = new FromLocation { Code = cities["from"] };
                ToLocation to_Location = new ToLocation { Code = cities["to"] };

                packages packages = new packages
                {
                    Weight = weight,
                    Length = length,
                    Width = width,
                    Height = height,
                };

                Order order = new Order
                {
                    currency = 1,
                    packages = new List<packages> { packages },
                    to_location = to_Location,
                    from_location = from_Location
                };

                string jsonContent = JsonConvert.SerializeObject(order);          
                var contentRegisterOrder = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var registerOrderResponse = await _httpClient.PostAsync("https://api.edu.cdek.ru/v2/calculator/tarifflist", contentRegisterOrder);

                if (registerOrderResponse.IsSuccessStatusCode)
                {
                    string registerOrderRepsonseContent = await registerOrderResponse.Content.ReadAsStringAsync();
                    JObject dataRegister = JObject.Parse(registerOrderRepsonseContent);
                    int count = dataRegister["tariff_codes"].Count();
                    if (count > 0)
                    {
                        return dataRegister["tariff_codes"][0]["delivery_sum"].ToString();
                    }
                    else
                    {
                        return "Нельзя доставить посылку по этому адресу.";
                    }
                } 
                else
                {
                    return "Не удалось рассчитать стоимость доставки.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ex.Message;
            }
           
        }

        public async Task LogIn()
        {
            try
            {
                string tokenUrl = "https://api.edu.cdek.ru/v2/oauth/token";

                var authenticationData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", "EMscd6r9JnFiQ3bLoyjJY6eM78JrJceI"),
                    new KeyValuePair<string, string>("client_secret", "PjLZkKBHEiLK3YsjtNrt3TGNG0ahs3kG")
                });

                HttpResponseMessage authorizationResponse = await _httpClient.PostAsync(tokenUrl, authenticationData);
                if (authorizationResponse.IsSuccessStatusCode)
                {
                    string authorizationResponseContent = await authorizationResponse.Content.ReadAsStringAsync();

                    JObject dataAuthorization = JObject.Parse(authorizationResponseContent);

                    if (dataAuthorization.ContainsKey("access_token"))
                    {
                        string accsess_token = dataAuthorization["access_token"].ToString();

                        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accsess_token);
                    }
                    else
                    {
                        _logger.LogError("Не найден access_token");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
