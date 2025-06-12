using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata;
using WebApiCRUDOps.DataUtility.Model;
using WebApiCRUDOps.DataUtility.Model.Dto;

namespace WebApiCRUDOps
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<PersonDto, Person>();
                config.CreateMap<Person, PersonDto>();
                config.CreateMap<PdfDto,PdfUpload>()
                .ForMember(dest => dest.OriginalPdf, opt => opt.Ignore())  // handled manually
                .ForMember(dest => dest.CompressedPdf, opt => opt.Ignore()) // handled manually
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore()) // let EF set it
                .ForMember(dest => dest.Person, opt => opt.Ignore()) // navigation
                .ForMember(dest=>dest.Id, opt => opt.Ignore());//ignore Id to not create problen

                config.CreateMap<PdfUpload, PdfDto>()
                .ForMember(dest => dest.PdfFile, opt => opt.Ignore()) // IFormFile can't be mapped back
                .ForMember(dest => dest.CompressionLevel, opt => opt.Ignore()); // optional
                //Learning Model LLM
                config.CreateMap<LearningPdfDto, LearningPdf>()
                .ForMember(dest => dest.Pdf, opt => opt.Ignore())
                .ForMember(dest => dest.CreateDateTime, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Title,
                 opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Title)));

            });
            return mappingConfig;
        }
    }
}
