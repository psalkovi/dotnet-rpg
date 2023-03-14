using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using dotnet_rpg.Services.CharacterService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;
        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDTO>>>> Get()
        {
            int userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
            return Ok(await _characterService.GetAllCharacters(userId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDTO>>>> GetSingle(int id)
        {
            return Ok(await _characterService.GetCharacterById(id));
        }

        [HttpPost]
        public async Task <ActionResult<ServiceResponse<List<GetCharacterDTO>>>> AddCharacter(AddCharacterDTO newCharacter)
        {
            return Ok(await _characterService.AddCharacter(newCharacter));            
        }
        [HttpPut]
        public async Task <ActionResult<ServiceResponse<List<GetCharacterDTO>>>> UpdateCharacter(UpdateCharacterDTO updatedCharacter)
        {   
            var response = await _characterService.UpdateCharacter(updatedCharacter);
            if (response.Data is null)
                return NotFound(response);
            return Ok(response);            
        }
         [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<List<GetCharacterDTO>>>> DeleteCharacter(int id)
        {
            var response = await _characterService.DeleteCharacter(id);
            if (response.Data is null)
                return NotFound(response);
            return Ok(response);
        }
    }
}