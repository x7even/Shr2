using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shr2.Interfaces;
using Shr2.Models;


namespace Shr2.Controllers
{
    [Produces("application/json")]
    [Route("api/V1")]
    public class V1Controller : Controller
    {
        private readonly IConverter _transposer;
        private readonly Config config;

        public V1Controller(IConverter transposer, IConfig iconfig)
        {
            _transposer = transposer;
            config = iconfig.GetConfig();

            
        }

        [HttpPost]
        [ActionName("Test")]
        [Route("test")]
        public async Task<IActionResult> Test()
        {
            var result = await _transposer.TryEncodeUrl("http://www.google.com");
            if (!String.IsNullOrEmpty(result))
            {
                return Json(config.Domain + result);
            }
            else if (result == "error")
            {
                throw new Exception("Url could not be processed, try again later.");
            }
            else
                return new BadRequestResult();
        }


        /// <summary>
        /// Takes GStyle model requests returns model like googe url shortener.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Url")]
        [Route("url")]
        public async Task<IActionResult> Url([FromBody]GStyleRequest request, string key = "")
        {
            if (!ModelState.IsValid)
                return new BadRequestResult();

            if(config.EncodeWithPermissionKey && !config.PermissionKeys.Contains(key.Trim()))
                    return new UnauthorizedResult();


            var result = await _transposer.TryEncodeUrl(request.LongUrl);
            if (!String.IsNullOrEmpty(result))
            {
                return Json(new {
                    kind = "urlshortener#url",
                    id = (config.Domain + result),
                    longUrl = request.LongUrl });
            }
            else if (result == "error")
            {
                throw new Exception("Url could not be processed, try again later.");
            }
            else
                return new BadRequestResult();

        }


        /** Google Return Json reference (fm: https://developers.google.com/url-shortener/v1/getting_started)
         * {
         *  "kind": "urlshortener#url",
         *  "id": "http://goo.gl/fbsS",
         *  "longUrl": "http://www.google.com/"
         *   }
         * */
    }
}