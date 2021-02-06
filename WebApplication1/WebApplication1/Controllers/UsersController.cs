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
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private DataAccess Database;
        
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger) 
        {
            Database = new DataAccess("Data Source=localhost;Initial Catalog=UsersDB;Integrated Security=True");
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {

            //System.IO.Pipelines.PipeReader reader = Request.BodyReader;
            //Request.BodyReader
            //reader.ReadAsync();
            //reader.
            //int x = 0;
            Stream body = Request.Body;

            //Request.
            //Request.content
            //Request.in
            StreamReader reader = new StreamReader(body);
            List<byte> content = new List<byte>();
            
            string test = reader.ReadToEnd();

            /*int y = body.ReadByte();
            while (y!=-1)
            {
                x += 1;
                
                content.Add(Convert.ToByte(y));

            }*/

            //byte[] buffer = new byte[body.Length];
            //Request.Body.
            /*for (int x = body.ReadByte(); x != -1; x = body.ReadByte())
            {
                content.Add(Convert.ToByte(body.ReadByte()));
            }*/
            return Ok(test);
        }
        
        /*public IActionResult Get2()
        {
            string test = this.HttpContext.Request.Headers["Authorization"];
            return Ok(test);
            //return Ok();

            //System.Diagnostics.Debug.WriteLine("HEY THIS IS WHERE IT LOGS IT");
            //User user = new User();
            //user.FirstName = "bill";
            //user.Password = "password";
            //Database = new DataAccess("Data Source=localhost\\MSSQLSERVER04;Initial Catalog=StockDB;Integrated Security=True");
            
            
        }*/
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
