using AutoMapper;
using DomainRule.Models.EMRDB;

namespace DomainRule.Mappings
{
    public class Mch_emrProfile : Profile
    {
        public Mch_emrProfile()
        {
            CreateMap<Mch_emr_Upload, Models.EMRDB_1522011080.Mch_emr>();
        }
    }
}
