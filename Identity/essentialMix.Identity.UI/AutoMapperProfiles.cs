using AutoMapper;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Identity.Model;
using essentialMix.Identity.Model.TransferObjects;
using essentialMix.Patterns.Pagination;

namespace essentialMix.Identity.UI;

public class AutoMapperProfiles : Profile
{
	public AutoMapperProfiles()
	{
		CreateMap<SortablePagination, ListSettings>().ReverseMap();

		CreateMap<Country, CountryForList>();

		CreateMap<City, CityForList>();

		CreateMap<UserToRegister, User>().ReverseMap();
		CreateMap<UserToUpdate, User>().ReverseMap();
		CreateMap<User, UserForLoginDisplay>()
			.ForMember(e => e.CountryCode, opt => opt.MapFrom(e => e.City == null ? string.Empty : e.City.CountryCode))
			.ForMember(e => e.Country, opt => opt.MapFrom(e => e.City != null && e.City.Country != null ? e.City.Country.Name : string.Empty))
			.ForMember(e => e.City, opt => opt.MapFrom(e => e.City != null ? e.City.Name : string.Empty));
		CreateMap<User, UserForList>()
			.IncludeBase<User, UserForLoginDisplay>();
		CreateMap<User, UserForDetails>()
			.IncludeBase<User, UserForList>()
			.ForMember(e => e.Roles, opt => opt.MapFrom(e => e.UserRoles.Select(x => x.Role.Name).ToArray()));
		CreateMap<User, UserForSerialization>()
			.IncludeBase<User, UserForList>();
	}
}