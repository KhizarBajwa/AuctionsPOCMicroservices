using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelper;

namespace SearchService.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {

        // This endpoint responds to HTTP GET requests
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            // Initialize a paged search query on the MongoDB 'Item' collection
            // The first type is the entity, the second is the return type (no projection here, so both are Item)
            var query = DB.PagedSearch<Item, Item>();

            // If the user has provided a search term, apply full-text search
            // This uses MongoDB text indexes and ranks results by relevance
            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            // Apply sorting based on the OrderBy parameter using a switch expression
            // If "make", sort ascending by Make
            // If "new", sort descending by CreatedAt (newest first)
            // Otherwise, sort ascending by AuctionEnd (soonest ending first)
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(x => x.Make)),
                "new" => query.Sort(x => x.Descending(x => x.CreatedAt)),
                _ => query.Sort(x => x.Ascending(x => x.AuctionEnd))
            };

            // Apply additional filtering based on the FilterBy parameter
            // If "finished", show auctions ending in the future
            // If "endingSoon", show auctions ending within the next 6 hours
            // Otherwise, default to showing all future auctions
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd > DateTime.UtcNow),
                "endingSoon" => query.Match(x =>
                    x.AuctionEnd <= DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            // If a Seller is specified, filter the auctions to include only those created by that seller
            if (!string.IsNullOrWhiteSpace(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }

            // If a Winner is specified, filter the auctions to include only those won by that user
            if (!string.IsNullOrWhiteSpace(searchParams.Winner))
            {
                query.Match(x => x.Winner == searchParams.Winner);
            }

            // Set the page number for pagination based on the query param
            query.PageNumber(searchParams.PageNumber);

            // Set the page size (how many items per page)
            query.PageSize(searchParams.PageSize);

            // Execute the query asynchronously and retrieve the results
            var result = await query.ExecuteAsync();

            // Return the paged results with metadata: items, total pages, and total count
            return Ok(new
            {
                Items = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }


    }
}
