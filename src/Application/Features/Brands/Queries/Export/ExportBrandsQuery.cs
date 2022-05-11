﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlazorHero.CleanArchitecture.Application.Extensions;
using BlazorHero.CleanArchitecture.Application.Interfaces.Repositories;
using BlazorHero.CleanArchitecture.Application.Interfaces.Services;
using BlazorHero.CleanArchitecture.Application.Specifications.Catalog;
using BlazorHero.CleanArchitecture.Domain.Entities.Catalog;
using BlazorHero.CleanArchitecture.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BlazorHero.CleanArchitecture.Application.Features.Brands.Queries.Export
{
    public class ExportBrandsQuery : IRequest<Result<string>>
    {
        public string SearchString { get; set; }

        public ExportBrandsQuery(string searchString = "")
        {
            SearchString = searchString;
        }
    }

    internal class ExportBrandsQueryHandler : IRequestHandler<ExportBrandsQuery, Result<string>>
    {
        private readonly IExcelService _excelService;
        private readonly IUnitOfWork<int> _unitOfWork;
        private readonly IStringLocalizer<ExportBrandsQueryHandler> _localizer;

        public ExportBrandsQueryHandler(IExcelService excelService
            , IUnitOfWork<int> unitOfWork
            , IStringLocalizer<ExportBrandsQueryHandler> localizer)
        {
            _excelService = excelService;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(ExportBrandsQuery request, CancellationToken cancellationToken)
        {
            var brandFilterSpec = new BrandFilterSpecification(request.SearchString);
            var brands = await _unitOfWork.Repository<Brand>().Entities
                .Specify(brandFilterSpec)
                .ToListAsync(cancellationToken);
            var data = await _excelService.ExportAsync(brands, mappers: new Dictionary<string, Func<Brand, object>>
            {
                { _localizer["Id"], item => item.Id },
                { _localizer["Name"], item => item.Name },
                { _localizer["Description"], item => item.Description },
                { _localizer["Tax"], item => item.Tax }
            }, sheetName: _localizer["Brands"]);

            return await Result<string>.SuccessAsync(data: data);
        }
    }
}
