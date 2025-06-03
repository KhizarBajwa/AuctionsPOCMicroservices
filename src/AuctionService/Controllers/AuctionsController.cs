using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _dbContext;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
        {
            var auctions = await _dbContext.Auctions
                .Include(a => a.Item)
                .OrderBy(a => a.Item.Make)
                .ToListAsync();

            return Ok(_mapper.Map<List<AuctionDto>>(auctions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(int id)
        {
            var auction = await _dbContext.Auctions
                .Include(a => a.Item)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<AuctionDto>(auction));
        }


        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            //TODO: add current user as seller
            auction.Seller = User.Identity?.Name ?? "anonymous";


            _dbContext.Auctions.Add(auction);
            var result = await _dbContext.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Failed to create auction");
            }


            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(int id, UpdateAuctionDto auctionDto)
        {
            var auction = await _dbContext.Auctions.Include(x => x.Item).FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            // ToDO: check seller == username
            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage > 0 ? auctionDto.Mileage : auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year > 0 ? auctionDto.Year : auction.Item.Year;


            var result = await _dbContext.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("Failed to update auction");
            }

            return Ok(_mapper.Map<AuctionDto>(auction));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction(int id)
        {
            var auction = await _dbContext.Auctions.FindAsync(id);

            if (auction == null)
            {
                return NotFound();
            }
            //TODO: check seller == username
            _dbContext.Auctions.Remove(auction);
            var result = await _dbContext.SaveChangesAsync() > 0;
            if (!result)
            {
                return BadRequest("Failed to delete auction");
            }
            return NoContent();

        }

    }

}
