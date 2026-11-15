using Application.Features.Categories.DTOs;
using Domain.Entities;
using ErrorOr;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery(CategoryFilter Filter) : IRequest<ErrorOr<PagedCategoriesResponse>>;
