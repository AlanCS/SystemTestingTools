using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace MoviaProjectFramework.Web.Controllers
{
    public class ValuesController : ApiController
    {
        public ValuesController()
        {

        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            LogLevel logLevel;
            var test = HttpContext.Current.Request.Url;
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
