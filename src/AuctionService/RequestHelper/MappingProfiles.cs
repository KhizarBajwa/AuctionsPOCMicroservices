using System;
using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.RequestHelper;

public class MappingProfiles : AutoMapper.Profile
{
    public MappingProfiles()
    {
        CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();

        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(d => d.Item, o => o.MapFrom(s => s));

        CreateMap<CreateAuctionDto, Item>();
    }
}
