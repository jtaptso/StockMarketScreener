using System.Text.Json;
using AutoMapper;
using StockScreener.Application.DTOs;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Stock ─────────────────────────────────────────────────────────────
        CreateMap<Stock, StockDto>();

        // ── PriceHistory ──────────────────────────────────────────────────────
        // Computed properties (PriceChange, PriceChangePercent, Range) are
        // read-only members on the entity, so AutoMapper maps them by name.
        CreateMap<PriceHistory, PriceHistoryDto>();

        // ── Fundamentals ──────────────────────────────────────────────────────
        CreateMap<Fundamentals, FundamentalsDto>();

        // ── Watchlist ─────────────────────────────────────────────────────────
        CreateMap<WatchlistItem, WatchlistItemDto>()
            .ForMember(dest => dest.Symbol,       opt => opt.MapFrom(src => src.Stock.Symbol))
            .ForMember(dest => dest.CompanyName,  opt => opt.MapFrom(src => src.Stock.CompanyName))
            .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Stock.CurrentPrice));

        CreateMap<Watchlist, WatchlistDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // ── FilterPreset ──────────────────────────────────────────────────────
        // FilterJson is a serialized ScreenerFilterDto — deserialize it here.
        CreateMap<FilterPreset, FilterPresetDto>()
            .ForMember(dest => dest.Filter, opt => opt.MapFrom(src =>
                JsonSerializer.Deserialize<ScreenerFilterDto>(src.FilterJson,
                    JsonSerializerOptions.Web) ?? new ScreenerFilterDto()));
    }
}
