﻿using BAExamApp.Dtos.Classrooms;

namespace BAExamApp.Business.Profiles;

public class ClassroomProfile : Profile
{
    public ClassroomProfile()
    {
        CreateMap<Classroom, ClassroomDto>();

        CreateMap<Classroom, ClassroomListDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.StudentCount, opt => opt.MapFrom(src => src.StudentClassrooms.Where(c => c.Status != Core.Enums.Status.Deleted).Count()));

        CreateMap<Classroom, ClassroomDetailsForAdminDto>()
            .ForMember(dest => dest.GroupTypeName, opt => opt.MapFrom(src => src.GroupType.Name))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.ProductNames, opt => opt.MapFrom(src => src.ClassroomProducts.Select(x => x.Product.Name)))
            .ForMember(dest => dest.ProductIds, opt => opt.MapFrom(src => src.ClassroomProducts.Select(x=>x.ProductId)))
            .ForMember(dest => dest.StudentClassrooms, opt => opt.MapFrom(src => src.StudentClassrooms.Where(x=>x.Status != Core.Enums.Status.Deleted)));


        CreateMap<Classroom, ClassroomDetailsDto>()
            .ForMember(dest => dest.GroupTypeName, opt => opt.MapFrom(src => src.GroupType.Name))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.StudentClassroomList, opt => opt.MapFrom(src => src.StudentClassrooms.Where(x=> x.Status != Core.Enums.Status.Deleted)))
            .ForMember(dest => dest.TrainerNames, opt => opt.MapFrom(src => src.TrainerClassrooms.Select(x => x.Trainer.FullName)))
            .ForMember(dest => dest.ProductNames, opt => opt.MapFrom(src => src.ClassroomProducts.Select(x => x.Product.Name)));

        CreateMap<ClassroomCreateDto, Classroom>();
        CreateMap<ClassroomUpdateDto, Classroom>();
    }
}
