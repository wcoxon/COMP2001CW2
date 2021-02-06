using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private DataAccess Database;
        
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger) 
        {
            Database = new DataAccess("Data Source=localhost;Initial Catalog=UsersDB;Integrated Security=True");
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(User user)
        {

            bool Validated = Database.Validate(user);

            return Ok(Validated);
        }
        
        
        [HttpDelete("{userID}")]
        public IActionResult Delete(int userID)
        {
            
            Database.Delete(userID);
            return Ok();
        }

        [HttpPut("{userID}")]
        public IActionResult Put(int userID, User user)
        {
            Database.Update(user, userID);
            return Ok();
        }

        [HttpPost]
        public IActionResult Post(User user)
        {
            
            string response = Database.RegisterUser(user);
            
            string[] responseArgs = response.Split(",");
            if (responseArgs.Length == 2)
            {
                return Ok(Int32.Parse(responseArgs[1]));
            }
            else
            {
                return BadRequest();
            }
            
        }
    }
}
