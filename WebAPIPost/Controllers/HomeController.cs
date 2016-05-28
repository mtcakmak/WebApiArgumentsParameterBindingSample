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
        public IHttpActionResult SimpleModelBinding2(string data, int data2)
        {
            return Ok($"{data}  {data2} {Request.Content.Headers.ContentType}");
        }

        [HttpPost]
        public IHttpActionResult SimpleModelBinding(string data, Guid? data2)
        {
            return Ok($"{data}  {data2} {Request.Content.Headers.ContentType}");
        }

        [HttpPost]
        public IHttpActionResult ComplexModelBinding(MyClass t)
        {
            return Ok($"{t.data}  {t.data2} {Request.Content.Headers.ContentType}");
        }

        public class MyClass
        {
            public string data { get; set; }
            public Guid? data2 { get; set; }
        }
    }
}
