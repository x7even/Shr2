﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shr2.Interfaces;
using Shr2.Models;

namespace Shr2.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/V1")]
    public class V1Controller : ControllerBase
    {
        private readonly IConverter _converter;
        private readonly Config _config;
        private readonly ILogger<V1Controller>? _logger;

        public V1Controller(
            IConverter converter,
            IConfig configProvider,
            ILogger<V1Controller>? logger = null)
        {
            _converter = converter;
            _config = configProvider.GetConfig();
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to shorten a sample URL
        /// </summary>
        /// <returns>The shortened URL</returns>
        [HttpPost]
        [ActionName("Test")]
        [Route("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Test()
        {
            try
            {
                _logger?.LogInformation("Test endpoint called");
                var result = await _converter.TryEncodeUrl("http://www.google.com");

                if (result.Success)
                {
                    var shortUrl = _config.Domain + result.ShortCode;
                    _logger?.LogInformation("Test URL shortened: {ShortUrl}", shortUrl);
                    return Ok(shortUrl);
                }
                else
                {
                    _logger?.LogError("Error processing test URL: {Error}", result.Error);
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { error = "URL could not be processed, try again later." });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception in Test endpoint");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Takes GStyle model requests and returns model like Google URL shortener.
        /// </summary>
        /// <param name="request">The URL shortening request</param>
        /// <param name="key">Optional API key for authorization</param>
        /// <returns>The shortened URL information</returns>
        [HttpPost]
        [ActionName("Url")]
        [Route("url")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public new async Task<IActionResult> Url([FromBody] GStyleRequest request, [FromQuery] string key = "")
        {
            try
            {
                _logger?.LogInformation("URL shortening requested for: {LongUrl}", request.LongUrl);

                if (!ModelState.IsValid)
                {
                    _logger?.LogWarning("Invalid model state for URL request");
                    return BadRequest(new { error = "Invalid request format" });
                }

                if (_config.EncodeWithPermissionKey && !_config.PermissionKeys.Contains(key.Trim()))
                {
                    _logger?.LogWarning("Unauthorized access attempt with key: {Key}", key);
                    return Unauthorized(new { error = "Invalid API key" });
                }

                var result = await _converter.TryEncodeUrl(request.LongUrl);

                if (result.Success)
                {
                    var response = new
                    {
                        kind = "urlshortener#url",
                        id = (_config.Domain + result.ShortCode),
                        longUrl = request.LongUrl
                    };

                    _logger?.LogInformation("URL shortened successfully: {ShortUrl}", response.id);
                    return Ok(response);
                }
                else
                {
                    _logger?.LogError("Error processing URL: {LongUrl}, Error: {Error}", request.LongUrl, result.Error);
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { error = "URL could not be processed, try again later." });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Exception in Url endpoint for: {LongUrl}", request.LongUrl);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred" });
            }
        }
    }
}
