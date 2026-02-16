using System.Net;
using System.Text;
using System.Text.Json;
using BiogenomTestTask.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BiogenomTestTaskTests
{
    [TestFixture]
    public class BiogenomTestTaskTests
    {
        private WebApplicationFactory<Program> factory;
        private HttpClient client;
        private const string CheckItemPath = "/api/ImageAnalysis/CheckItems";
        private const string CheckMaterialsPath = "/api/ImageAnalysis/CheckMaterials";
        private const string TestImgUrl =
            "https://mossklad.ru/upload/iblock/680/snc00v17ci0ziids6wot990fd42jv0gv/sls_6_locksmith_table_20_03.jpg";

        [SetUp]
        public void Setup()
        {
            factory = new WebApplicationFactory<Program>();
            client = factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
            factory.Dispose();
        }
        
        [Test]
        public async Task CheckItems_WhenAllIsFine_Returns200()
        {
            var response = await CheckItemAsync();
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        
        [Test]
        public async Task CheckItems_WhenAllIsFine_ReturnsCorrectItems()
        {
            var expectedItems = new[]
            {
                "стол",
                "инструмент",
                "люди",
                "станок"
            };
            var acceptableDiscrepancies = (int)(expectedItems.Length * 0.7);
            
            var response = await CheckItemAsync();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CheckItemResponse>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            var currentDiscrepancies = 
                result.DetectedItems.Count(item => !expectedItems.Contains(item));
            
            Assert.That(acceptableDiscrepancies > currentDiscrepancies);
        }
        
        [Test]
        public async Task CheckMaterials_WhenAllIsFine_Returns200()
        {
            var response = await CheckItemAsync();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CheckItemResponse>(content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            response = await CheckMaterialsAsync(result);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        
        private async Task<HttpResponseMessage> CheckMaterialsAsync(CheckItemResponse checkItemResponse)
        {
            var request = new CheckMaterialsRequest
            {
                Id = checkItemResponse.ResponseId,
                DetectedItems = checkItemResponse.DetectedItems
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                CheckMaterialsPath,  
                content);

            return response;
        }
        
        private async Task<HttpResponseMessage> CheckItemAsync()
        {
            var content = new StringContent(
                JsonSerializer.Serialize(TestImgUrl),
                Encoding.UTF8,
                "application/json");
            
            var response = await client.PostAsync(
                CheckItemPath,
                content);

            return response;
        }
    }
}