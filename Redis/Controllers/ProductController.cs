using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Redis.Models;
using Newtonsoft.Json;
using System.Text;

namespace Redis.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        readonly IDistributedCache _distributedCache;
        public ProductController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache; 
            
        }

        [HttpGet("[action]/{key}")]
        public async Task<List<Product>> Get(string key)
        {
           /* --- begin: json serialize --- */
            string stringProduct = await _distributedCache.GetStringAsync("product:1");
            // product nesnesine deserialize edecek
            Product product1 = JsonConvert.DeserializeObject<Product>(stringProduct); 


            /* --- end: json serialize --- */


            /* --- begin: binary serialize --- */
            Byte[] binaryProduct = await _distributedCache.GetAsync("product:2");
            string stringProduct2 = Encoding.UTF8.GetString(binaryProduct);
            Product product2 = JsonConvert.DeserializeObject<Product>(stringProduct2);

            /* --- end: binary serialize --- */

            return new List<Product>
            {
                product1,
                product2
            };
            

            
        }

        [HttpGet("[action]/{key}/{value}")]
        public async void Set()
        {
            Product product = new()
            {
                Id = 1,
                Name = "Apple Computer",
                Price = 2500
            };

            /* --- begin: json serialize --- */
            string jsonProduct = JsonConvert.SerializeObject(product); // objeyi serialize edip string döner

            // string'e cevrildiği için setString ile kayıt edilir.
            await _distributedCache.SetStringAsync("product:1", jsonProduct,options: new()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });
            // product objesini string olarak tutar

            /* --- end: json serialize --- */


            /* --- begin: binary serialize --- */
            Byte[] byteProduct = Encoding.UTF8.GetBytes(jsonProduct); // byte slice'a dönüştürülür
            await _distributedCache.SetAsync("product:2", byteProduct);

            
            /* --- end: binary serialize --- */

        }

        [HttpGet("[action]")]
        public async void SetImageCache()
        {
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/car.jpg");
            byte[] imageByteSlice = await System.IO.File.ReadAllBytesAsync(imagePath);
            await _distributedCache.SetAsync("image:car", imageByteSlice);
        }

        [HttpGet("[action]")]
        public async Task<FileContentResult> GetImageCache()
        {
            byte[] imageByteSlice = await _distributedCache.GetAsync("image:car");
            return File(imageByteSlice, "image/jpg");
        }
    }
}
