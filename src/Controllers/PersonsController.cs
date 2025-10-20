using Microsoft.AspNetCore.Mvc;
using BadApiExample.DTOs;
using BadApiExample.Services;

namespace BadApiExample.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonsController> _logger;

    public PersonsController(IPersonService personService, ILogger<PersonsController> logger)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a person by ID
    /// </summary>
    /// <param name="id">The person ID</param>
    /// <returns>The person if found</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<PersonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid ID provided"));
        }

        var result = await _personService.GetByIdAsync(id);
        
        if (!result.Success)
        {
            return result.Message.Contains("not found") 
                ? NotFound(result) 
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets all persons with pagination
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <returns>Paginated list of persons</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResultDto<PersonResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid request parameters", 
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _personService.GetAllAsync(request);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Searches persons with filters
    /// </summary>
    /// <param name="searchDto">Search criteria</param>
    /// <param name="pageRequest">Pagination parameters</param>
    /// <returns>Filtered and paginated list of persons</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResultDto<PersonResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromBody] PersonSearchDto searchDto, [FromQuery] PagedRequestDto pageRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid request parameters", 
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _personService.SearchAsync(searchDto, pageRequest);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Creates a new person
    /// </summary>
    /// <param name="createDto">Person creation data</param>
    /// <returns>The created person</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<PersonResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] PersonCreateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid request data", 
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _personService.CreateAsync(createDto);
        
        if (!result.Success)
        {
            return result.Message.Contains("already exists") 
                ? Conflict(result) 
                : BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Updates an existing person
    /// </summary>
    /// <param name="id">The person ID</param>
    /// <param name="updateDto">Person update data</param>
    /// <returns>The updated person</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<PersonResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] PersonUpdateDto updateDto)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid ID provided"));
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid request data", 
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _personService.UpdateAsync(id, updateDto);
        
        if (!result.Success)
        {
            return result.Message.Contains("not found") 
                ? NotFound(result) 
                : result.Message.Contains("already exists") 
                    ? Conflict(result) 
                    : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Deletes a person (soft delete)
    /// </summary>
    /// <param name="id">The person ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Invalid ID provided"));
        }

        var result = await _personService.DeleteAsync(id);
        
        if (!result.Success)
        {
            return result.Message.Contains("not found") 
                ? NotFound(result) 
                : BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Checks if a person exists
    /// </summary>
    /// <param name="id">The person ID</param>
    /// <returns>True if person exists, false otherwise</returns>
    [HttpHead("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Exists(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var result = await _personService.ExistsAsync(id);
        
        return result.Success && result.Data == true ? Ok() : NotFound();
    }

    /// <summary>
    /// Checks if an email is already in use
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <param name="excludeId">Optional ID to exclude from check</param>
    /// <returns>True if email exists, false otherwise</returns>
    [HttpGet("email-exists")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EmailExists([FromQuery] string email, [FromQuery] int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult("Email is required"));
        }

        var result = await _personService.EmailExistsAsync(email, excludeId);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }
}