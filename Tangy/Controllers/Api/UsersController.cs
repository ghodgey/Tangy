using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tangy.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tangy.Controllers.Api
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {

        private ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }



        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string type, string query = null)
        {
            //type used if you want to add cellphone searching etc.

            if(type.Equals("email") && query != null)
            {
                var customerQuery = _db.Users.Where(u => u.Email.ToLower().Contains(query.ToLower()));

                return Ok(customerQuery.ToList());
            } else if(type.Equals("phone") && query != null)
            {
                var customerQuery = _db.Users.Where(u => u.PhoneNumber.ToLower().Contains(query.ToLower()));

                return Ok(customerQuery.ToList());
            }
            return Ok();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
