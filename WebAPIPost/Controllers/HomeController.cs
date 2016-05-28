using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPIPost.Controllers
{
    public class HomeController : ApiController
    {
        [HttpPost]
        public IHttpActionResult SimpleModelBinding(string data, Guid? data2)
        {
            return Ok($"{data}  {data2} {Request.Content.Headers.ContentType}");
        }

        [HttpPost]
        public IHttpActionResult ComplexModelBinding(ComplexModel model)
        {
            return Ok($"{model.data}  {model.data2} {Request.Content.Headers.ContentType}");
        }

        public class ComplexModel
        {
            public string data { get; set; }
            public Guid? data2 { get; set; }
        }
    }
}
