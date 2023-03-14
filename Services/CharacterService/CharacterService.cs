using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Models;
using AutoMapper;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Data;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public CharacterService(IMapper mapper,DataContext context)
        {
            _context = context;
            _mapper = mapper;   
        }
        public async Task<ServiceResponse<List<GetCharacterDTO>>> AddCharacter(AddCharacterDTO newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDTO>>();
            var character = _mapper.Map<Character>(newCharacter);
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();
            serviceResponse.Data = await _context.Characters.Select(c => _mapper.Map<GetCharacterDTO>(c)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDTO>>> GetAllCharacters(int userId)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDTO>>();
            var dbCharacters = await _context.Characters.Where(c => c.User!.Id == userId).ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDTO>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDTO>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDTO>();
            var dbCharacter = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id);
            serviceResponse.Data = _mapper.Map<GetCharacterDTO>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDTO>> UpdateCharacter(UpdateCharacterDTO updatedCharacter)
        {   
            var serviceResponse = new ServiceResponse<GetCharacterDTO>();
            try
            {
                var dbCharacter = await _context.Characters.FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
                if(dbCharacter is null)
                    throw new Exception($"Character with Id '{updatedCharacter.Id}' not found");
                dbCharacter.Name = updatedCharacter.Name;
                dbCharacter.HitPoints = updatedCharacter.HitPoints;
                dbCharacter.Intelligence = updatedCharacter.Intelligence;
                dbCharacter.Defense = updatedCharacter.Defense;
                dbCharacter.Strength = updatedCharacter.Strength;
                dbCharacter.Class = updatedCharacter.Class;
                await _context.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetCharacterDTO>(dbCharacter);
            }

            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }


        public async Task<ServiceResponse<List<GetCharacterDTO>>> DeleteCharacter(int id)
        {
             var serviceResponse = new ServiceResponse<List<GetCharacterDTO>>();
            try
            {

                var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id);
                if(character is null)
                    throw new Exception($"Character with Id '{id}' not found");

                _context.Remove(character);
                await _context.SaveChangesAsync();

                serviceResponse.Data = await _context.Characters.Select(c => _mapper.Map<GetCharacterDTO>(c)).ToListAsync();
            }

            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}