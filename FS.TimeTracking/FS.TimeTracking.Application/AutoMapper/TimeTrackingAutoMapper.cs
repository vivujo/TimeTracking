﻿using AutoMapper;
using FS.TimeTracking.Shared.DTOs.TimeTracking;
using FS.TimeTracking.Shared.Models.TimeTracking;

namespace FS.TimeTracking.Application.AutoMapper
{
    /// <summary>
    /// Configuration profile for auto mapper.
    /// </summary>
    /// <seealso cref="Profile" />
    public class TimeTrackingAutoMapper : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeTrackingAutoMapper"/> class.
        /// </summary>
        /// <autogeneratedoc />
        public TimeTrackingAutoMapper()
        {
            ClearPrefixes();

            CreateMap<Customer, CustomerDto>()
                .ReverseMap();

            CreateMap<Project, ProjectDto>()
                .ReverseMap();

            CreateMap<Order, OrderDto>()
                .ReverseMap()
                .ForMember(x => x.StartDateOffset, x => x.Ignore())
                .ForMember(x => x.DueDateOffset, x => x.Ignore());

            CreateMap<Activity, ActivityDto>()
                .ReverseMap();

            CreateMap<TimeSheet, TimeSheetDto>()
                .ReverseMap()
                .ForMember(x => x.StartDateOffset, x => x.Ignore())
                .ForMember(x => x.EndDateOffset, x => x.Ignore());

            CreateMap<Customer, CustomerListDto>();

            CreateMap<Project, ProjectListDto>();

            CreateMap<Order, OrderListDto>();

            CreateMap<Activity, ActivityListDto>()
                .ForMember(x => x.CustomerTitle, x => x.MapFrom(activity => activity.Project.Customer.Title));

            CreateMap<TimeSheet, TimeSheetListDto>()
                .ForMember(x => x.CustomerTitle, x => x.MapFrom(timeSheet => timeSheet.Project.Customer.Title))
                .ForMember(x => x.Duration, x => x.MapFrom(timeSheet => timeSheet.EndDate - timeSheet.StartDate));
        }
    }
}
