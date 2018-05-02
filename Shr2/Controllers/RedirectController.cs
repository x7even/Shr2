using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Shr2.Interfaces;

namespace Shr2.Controllers
{
    [Produces("application/json")]
    [Route("")]
    public class RedirectController : Controller
    {
        private readonly IConverter _converter;

        public RedirectController(IConverter converter) => _converter = converter;

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> RedirectProcessor(string id)
        {
            if (string.IsNullOrEmpty(id) | id.Length < 3 | !Regex.IsMatch(id, @"^[a-zA-Z0-9]+$"))
                return new NotFoundResult();

            var result = await _converter.TryDecode(id);
            if (String.IsNullOrEmpty(result.url))
                return new NotFoundResult();
            else
                return new RedirectResult(result.url, result.permanent, result.preserveMethod);
        }

        /** Redirect Result(url, permanent, preserveMethod)
         *  'permanent: Boolean' 
            Specifies whether the redirect should be permanent (301) or temporary (302).
            'preserveMethod: Boolean' 
            If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the intial request method.
         * */
    }
}