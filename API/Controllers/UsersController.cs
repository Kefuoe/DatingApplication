using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using AutoMapper;
using API.DTOs;

namespace API.Controllers
{
   [Authorize]
    public class UsersController : BaseApiController
    {
        
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;

        }

         //getting two end points, or two users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers() //returns a simple list with no extended functionality
        {
           var users = await _userRepository.GetMemberAsync();
   
           return Ok(users);
  
        }
        //you make it async to handle multiple inquiries/users
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) //returns a user
        {
          return await _userRepository.GetMemberAsync(username);// getting user from repository


        }
    }
}