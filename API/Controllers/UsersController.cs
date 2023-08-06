using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
   [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context; // how you gain access to the dB
        public UsersController(DataContext context)
        {
            _context = context;
        }

         //getting two end points, or two users
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() //returns a simple list with no extended functionality
        {
            return await _context.Users.ToListAsync(); //get users from a database aysnchronously into a list, it awaits after making a query
  
        }
        //you make it async to handle multiple inquiries/users
        //api/users/3  specific user
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id) //returns a user
        {
          return await _context.Users.FindAsync(id); //get users from a database into a list

        }
    }
}