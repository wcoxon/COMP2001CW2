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
            
            Database = new DataAccess("Data Source=socem1.uopnet.plymouth.ac.uk;Database=COMP2001_WCoxon;User Id=WCoxon;Password=UgmH957*");
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(User user)
        {
            //attempt to register the user credentials provided in the POST request
            string response = Database.Register(user);

            string[] responseArgs = response.Split(",");

            if (responseArgs[0] == "200")
            {
                return Ok(Int32.Parse(responseArgs[1]));
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpGet]
        public IActionResult Get(User user)
        {
            //attempt to validate the user credentials provided in the GET request
            bool Validated = Database.Validate(user);
            return Ok(Validated);
        }

        [HttpPut("{userID}")]
        public IActionResult Put(int userID, User user)
        {
            //attempt to update the user with the ID provided in the url, using the credentials provided in the PUT request
            Database.Update(user, userID);
            return Ok();
        }

        [HttpDelete("{userID}")]
        public IActionResult Delete(int userID)
        {
            //attempt to delete the user with the ID provided in the url
            Database.Delete(userID);
            return Ok();
        }

    }
}
