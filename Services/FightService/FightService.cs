using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public FightService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<FightResultDTO>> Fight(FightRequestDTO request)
        {
            var response = new ServiceResponse<FightResultDTO>
            {
                Data = new FightResultDTO()
            };
            try
            {
                var characters = await _context.Characters.Include(c => c.Weapon).Include(c => c.Skills)
                .Where(c => request.CharacterIds.Contains(c.Id))
                .ToListAsync();

                bool defeated = false;
                while(!defeated)
                {
                    foreach(var attacker in characters)
                    {
                        var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;
                        if(useWeapon && attacker.Weapon is not null)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker,opponent);

                        }
                        else if(!useWeapon && attacker.Skills is not null)
                        {
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker,opponent,skill);
                        }
                        else 
                        {
                            response.Data.Log.Add($"{attacker.Name} wasn't able to attack");
                            continue;
                        }
                        response.Data.Log.Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} with {(damage >= 0 ? damage : 0)} damage");
                        if(opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            response.Data.Log.Add($"{opponent.Name} has been defeated!");
                            response.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                            break;
                        }
                    }
                    characters.ForEach(c => {
                        c.Fights++;
                        c.HitPoints = 100;
                    });
                    await _context.SaveChangesAsync();
                }
            }
            catch(Exception	ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<AttackResultDTO>> SkillAttack(SkillAttackDTO request)
        {
            var response = new ServiceResponse<AttackResultDTO>();
            try
            {
                var attacker = await _context.Characters.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == request.OponnentId);

                if (attacker is null || opponent is null || attacker.Skills is null)
                {
                    throw new Exception("No attacker, opponent or attacker skills");
                }
                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);
                if (skill is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't have that skill";
                    return response;
                }
                int damage = DoSkillAttack(attacker, opponent, skill);
                if (opponent.HitPoints <= 0)
                    response.Message = $"{opponent.Name} has been defeated";

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDTO
                {
                    Attacker = attacker.Name!,
                    Oponnent = opponent.Name!,
                    AttackerHP = attacker.HitPoints,
                    OponnentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            int damage = skill.Damage + (new Random().Next(attacker.Intelligence));
            damage -= new Random().Next(opponent.Defeats);
            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<AttackResultDTO>> WeaponAttack(WeaponAttackDTO request)
        {
            var response = new ServiceResponse<AttackResultDTO>();
            try
            {
                var attacker = await _context.Characters.Include(c => c.Weapon).FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters.Include(c => c.Weapon).FirstOrDefaultAsync(c => c.Id == request.OponnentId);

                if (attacker is null || opponent is null || attacker.Weapon is null)
                {
                    throw new Exception("No attacker, opponent or attacker weapon");
                }

                int damage = DoWeaponAttack(attacker, opponent);
                if (opponent.HitPoints <= 0)
                    response.Message = $"{opponent.Name} has been defeated";

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDTO
                {
                    Attacker = attacker.Name!,
                    Oponnent = opponent.Name!,
                    AttackerHP = attacker.HitPoints,
                    OponnentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            if (attacker.Weapon is null)
                throw new Exception("Attacker has no weapon");

            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defeats);
            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<List<HighscoreDTO>>> GetHighscore()
        {
            var characters = await _context.Characters.Where(c => c.Fights > 0).OrderByDescending(c => c.Victories).ThenBy(c => c.Defeats).ToListAsync();
            var serviceResponse = new ServiceResponse<List<HighscoreDTO>>()
            {
                Data = characters.Select(c => _mapper.Map<HighscoreDTO>(c)).ToList()
            };
            return serviceResponse;
        }
    }
}