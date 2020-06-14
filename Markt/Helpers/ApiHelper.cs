using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Markt.Helpers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiHelper : ControllerBase
    {
        protected IActionResult Do(Func<object> func)
        {
            try
            {
                var output = func();

                if (output is string || output.GetType().IsValueType)
                {
                    return Ok(new { Value = output });
                }

                return Ok(output);
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ApiError(exception.Message));
            }
            catch (DbUpdateException exception)
            {
                return BadRequest(new ApiError(exception.GetExceptionMessage()));
            }
            catch (Exception exception)
            {
                return BadRequest(new ApiError(exception.Message));
            }
        }

        protected async Task<IActionResult> Do(Func<Task<object>> func)
        {
            try
            {
                var output = await func();

                if (output is string || output.GetType().IsValueType)
                {
                    return Ok(new { Value = output });
                }

                return Ok(output);
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ApiError(exception.Message));
            }
            catch (DbUpdateException exception)
            {
                return BadRequest(new ApiError(exception.GetExceptionMessage()));
            }
            catch (Exception exception)
            {
                return BadRequest(new ApiError(exception.Message));
            }
        }

        protected async Task<IActionResult> Do(Func<Task> func)
        {
            try
            {
                await func();

                return NoContent();
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ApiError(exception.Message));
            }
            catch (DbUpdateException exception)
            {
                return BadRequest(new ApiError(exception.GetExceptionMessage()));
            }
            catch (Exception exception)
            {
                return BadRequest(new ApiError(exception.Message));
            }
        }

        protected async Task<IActionResult> Create(string uri, Func<Task<object>> func)
        {
            try
            {
                var result = await func();

                return Created(uri, new { Value = result });
            }
            catch (AuthenticationException)
            {
                return Unauthorized();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ApiError(exception.Message));
            }
            catch (DbUpdateException exception)
            {
                return BadRequest(new ApiError(exception.GetExceptionMessage()));
            }
            catch (Exception exception)
            {
                return BadRequest(new ApiError(exception.Message));
            }
        }
    }
}