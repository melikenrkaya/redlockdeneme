using Microsoft.AspNetCore.Mvc;
using RedlockDeneme.Common.Extensions;
using RedlockDeneme.Data.Context;
using RedlockDeneme.Data.Models;
using RedlockDeneme.Services;

namespace RedlockDeneme.Controller
{
    [Route("Api/Stok")]
    [ApiController]
    public class StokController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IStok _stokServices;

        public StokController(ApplicationDBContext context, IStok stokServices)
        {
            _stokServices = stokServices;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetALL()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stok = await _stokServices.GetAllAsync();
            var stokDto = stok.Select(s => s.ToStokDto());
            return Ok(stok);
        }


        [HttpGet("{id:int}")]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stok = await _stokServices.GetByIdAsync(id);
            if (stok == null)
            {
                return NotFound();
            }
            return Ok(stok.ToStokDto());
        }
        // POST: api/stok/siparis/5?ad=melike
        [HttpPost("siparis/{stokId}")]
        public async Task<IActionResult> SiparisVer(int stokId, [FromQuery] string ad)
        {
            var sonuc = await _stokServices.SiparisVerAsync(stokId, ad);
            return Ok(new { mesaj = sonuc });
        }

        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CreateStokRequestDto stokDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stokModel = stokDto.ToStokFromCreatedDTO();
            await _stokServices.CreateAsync(stokModel);
            return CreatedAtAction(nameof(GetById), new { id = stokModel.StokId }, stokModel.ToStokDto());
        }

        [HttpPut]
        [Route("{id:int}")]

        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStokRequestDto updatestokDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stokModel = await _stokServices.UpdateAsync(id, updatestokDto);
            if (stokModel == null)
            {
                return NotFound();
            }
            return Ok(stokModel.ToStokDto());
        }

        [HttpDelete]
        [Route("{id:int}")]

        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stokModel = await _stokServices.DeleteAsync(id);
            if (stokModel == null)
            {
                return NotFound();
            }
            return NoContent();
        }
    
    }
}
