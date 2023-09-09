using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using AutoMapper;
using API.DTOs;
using System.Security.Claims;
using API.Extensions;
using API.Entities;

namespace API.Controllers
{
   [Authorize]
    public class UsersController : BaseApiController
    {
        
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, 
        IPhotoService photoService)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _photoService = photoService;

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
        
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) 
        {
    
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user);

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto (IFormFile file)
        {
           var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

           if (user == null) return NotFound();

           var result = await _photoService.AddPhotoAsync(file);

           if (result.Error != null) return BadRequest(result.Error.Message);

           var photo = new Photo
           {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
           };    

           if (user.Photos.Count == 0) photo.IsMain = true;

           user.Photos.Add(photo); // entity framework is tracking photos in memory

           if (await _userRepository.SaveAllAsync()) 
           {
              return CreatedAtAction(nameof(GetUser),
                  new {username = user.UserName}, _mapper.Map<PhotoDto>(photo)); // once a photo is uploaded it sends a 201 created msg
           } 

           return BadRequest("Problem adding photo");

        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId) 
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("this is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting the main photo");   
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto (int photoId) 
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            
            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");

        }
    }
}