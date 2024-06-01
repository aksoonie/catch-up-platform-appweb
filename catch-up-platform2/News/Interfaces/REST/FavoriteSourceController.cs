using System.Net.Mime;
using catch_up_platform2.News.Domain.Services;
using catch_up_platform2.News.Model.Queries;
using catch_up_platform2.News.Interfaces.REST.Resources;
using catch_up_platform2.News.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tsp;

namespace catch_up_platform2.News.Interfaces.REST;

/// <summary>
/// Necesarias para indicar de que va a ser un api
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class FavoriteSourceController(IFavoriteSourceCommandService favoriteSourceCommandService,
    IFavoriteSourceQueryService favoriteSourceQueryService) : ControllerBase
{
    //solo los endpointtientn HttpGet, HttpPost, HttpPut, HttpDelete
    [HttpPost] // POST api/v1/FavoriteSource
    public async Task<ActionResult> CreateFavoriteSource([FromBody] CreateFavoriteSourceResource resource)
    {
        var createFavoriteSourceCommand =
            CreateFavoriteSourceCommandFromResourceAssembler.toCommandFromResource(resource);
        var result = await favoriteSourceCommandService.Handle(createFavoriteSourceCommand);
        //si el resultado es nulo, devolver un bad request, es solo ub objeto (puede ser nulo o puede existir)
        if (result is null) return BadRequest();
        return CreatedAtAction(nameof(GetFavoriteSourceById), new { id = result.Id },
            FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpGet("{id}")] // GET api/v1/FavoriteSource/{id}
    public async Task<ActionResult> GetFavoriteSourceById(int id)
    {
        var getFavoriteSourceByIdQuery = new GetFavoriteSourceByIdQuery(id);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceByIdQuery);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }
    private async Task<ActionResult> GetFavoriteSourcesByNewsApiKey(string newsApiKey)
    {
        var getFavoriteSourcesByNewsApiKeyQuery = new GetAllFavoriteSourcesByNewsApiKeyQuery(newsApiKey);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourcesByNewsApiKeyQuery);
        var resources = result.Select(FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
    
    private async Task<ActionResult> GetFavoriteSourceByNewsApiKeyAndSourceId(string newsApiKey, string sourceId)
    {
        var getFavoriteSourceByNewsApiKeyAndSourceIdQuery = new GetFavoriteSourceByNewsApiKeyAndSourceIdQuery(newsApiKey, sourceId);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceByNewsApiKeyAndSourceIdQuery);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }
    private async Task<ActionResult> GetAllFavoriteSources(string newsApiKey)
    {
        var getAllFavoriteSourcesQuery = new GetAllFavoriteSourcesByNewsApiKeyQuery(newsApiKey);
        var result = await favoriteSourceQueryService.Handle(getAllFavoriteSourcesQuery);
        var resources = result.Select(FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
    [HttpGet] // GET api/v1/FavoriteSource
    public async Task<ActionResult> GetFavoriteSourceFromQuery([FromQuery] string newsApiKey, 
                                                            [FromQuery] string sourceId = "")
    {
        return string.IsNullOrEmpty(sourceId)
            ? await GetFavoriteSourcesByNewsApiKey(newsApiKey)
            : await GetFavoriteSourceByNewsApiKeyAndSourceId(newsApiKey, sourceId);
    }

}