﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Shr2.Interfaces;

namespace Shr2.Controllers
{
    [ApiController]
    [Route("")]
    public class RedirectController : ControllerBase
    {
        private readonly IConverter _converter;
        private readonly ILogger<RedirectController>? _logger;
        private static readonly Regex _validIdRegex = new Regex(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled);

        public RedirectController(
            IConverter converter,
            ILogger<RedirectController>? logger = null)
        {
            _converter = converter;
            _logger = logger;
        }

        /// <summary>
        /// Processes a short URL ID and redirects to the original URL
        /// </summary>
        /// <param name="id">The short URL identifier</param>
        /// <returns>A redirect to the original URL or a 404 if not found</returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status307TemporaryRedirect)]
        [ProducesResponseType(StatusCodes.Status308PermanentRedirect)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RedirectProcessor(string id)
        {
            try
            {
                _logger?.LogInformation("Redirect requested for ID: {Id}", id);
                
                if (string.IsNullOrEmpty(id) || id.Length < 3 || !_validIdRegex.IsMatch(id))
                {
                    _logger?.LogWarning("Invalid redirect ID format: {Id}", id);
                    return NotFound();
                }

                var result = await _converter.TryDecode(id);
                
                if (string.IsNullOrEmpty(result.Item1))
                {
                    _logger?.LogWarning("No URL found for ID: {Id}", id);
                    return NotFound();
                }
                
                _logger?.LogInformation("Redirecting {Id} to {Url} (Permanent: {Permanent}, PreserveMethod: {PreserveMethod})", 
                    id, result.Item1, result.Item2, result.Item3);
                
                return new RedirectResult(result.Item1, result.Item2, result.Item3);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing redirect for ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
