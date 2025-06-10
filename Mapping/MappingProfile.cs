using System;
using AutoMapper;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.Entity;

namespace InventoryManagementSystem.Mapping;

public class MappingProfile :Profile
{
public MappingProfile()
    {
        CreateMap<Purchase, PurchaseDto>()
            .ForMember(dest => dest.PurchaseDetails, opt => opt.MapFrom(src => src.PurchaseDetails));

        CreateMap<PurchaseDetails, PurchaseDetailDto>();

        // Add reverse mappings if needed, e.g.
        CreateMap<PurchaseDto, Purchase>();
        CreateMap<PurchaseDetailDto, PurchaseDetails>();
    }
}
