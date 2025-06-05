using Microsoft.AspNetCore.Mvc;
using RedlockDeneme.Common.Extensions;
using RedlockDeneme.Data.Context;
using RedlockDeneme.Data.Entity;
using RedlockDeneme.Data.Models;
using RedlockDeneme.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace RedlockDeneme.Controller
{

    [Route("Api/Sepet")]
    [ApiController]
    public class SepetController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ISepet _sepetServices;

        public SepetController(ApplicationDBContext context, ISepet sepetServices)
        {
            _sepetServices = sepetServices;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetALL()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sepet = await _sepetServices.GetAllAsync();
            return Ok(sepet);
        }


        [HttpGet("{id:int}")]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sepet = await _sepetServices.GetByIdAsync(id);
            if (sepet == null)
            {
                return NotFound();
            }
            return Ok(sepet.ToSepetDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSepetRequestDTO sepetDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sepetModel = sepetDto.ToSepetFromCreatedDTO();
            await _sepetServices.CreateAsync(sepetModel);
            return CreatedAtAction(nameof(GetById), new { id = sepetModel.UrunId }, sepetModel.ToSepetDto());
        }
        [HttpPut]
        [Route("{id:int}")]

        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSepetRequestDTO updatesepetDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sepetModel = await _sepetServices.UpdateAsync(id, updatesepetDto);
            if (sepetModel == null)
            {
                return NotFound();
            }
            return Ok(sepetModel);
        }


        [HttpDelete("temizle/stogu-bitmis")]
        public async Task<IActionResult> TemizleStoguBitmis()
        {
            var kalanlar = await _sepetServices.TemizleStoguBitmisUrunleriAsync();
            return Ok(kalanlar);
        }

    }
}























//        [HttpGet]
//        public async Task<IActionResult> GetSepet()
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            string key = "sepet:1"; // sabit ID kullanılıyor
//            var sepetData = await _redisDb.StringGetAsync(key);

//            if (sepetData.IsNullOrEmpty)
//                return NotFound("Sepet verisi bulunamadı.");

//            var sepet = JsonSerializer.Deserialize<List<Stok>>(sepetData);
//            return Ok(sepet);
//        }



//        [HttpGet("get-sepet")]
//        public async Task<IActionResult> GetSepet(string sepetId)
//        {
//            string key = $"sepet:{sepetId}";
//            var data = await _redisDb.StringGetAsync(key);

//            if (data.IsNullOrEmpty)
//                return NotFound("Sepet bulunamadı.");

//            var sepetList = JsonSerializer.Deserialize<List<Sepet>>(data);
//            return Ok(sepetList);
//        }


//        [HttpPost("post-sepet")]
//        public async Task<IActionResult> MockSepet([FromBody] Sepet request)
//        {
//            string key = $"sepet:{request.SepetId}";

//            var existingData = await _redisDb.StringGetAsync(key);
//            var sepetList = string.IsNullOrEmpty(existingData)
//                ? new List<Sepet>()
//                : JsonSerializer.Deserialize<List<Sepet>>(existingData);

//            // Aynı ürün varsa, adedi arttır
//            var existing = sepetList.FirstOrDefault(x => x.UrunId == request.UrunId);
//            if (existing != null)
//            {
//                existing.Adet += request.Adet;
//            }
//            else
//            {
//                sepetList.Add(request);
//            }

//            await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(sepetList));

//            return Ok(new { mesaj = "Sepet güncellendi.", key });
//        }



//    }
//}

