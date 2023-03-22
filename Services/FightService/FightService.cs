using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        public FightService(DataContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<AttackResultDTO>> SkillAttack(SkillAttackDTO request)
        {
            var response = new ServiceResponse<AttackResultDTO>();
            try
            {
                var attacker = await _context.Characters.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == request.OponnentId);

                if(attacker is null ||opponent is null || attacker.Skills is null)
                {
                     throw new Exception("No attacker, opponent or attacker skills");   
                }
                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);
                if(skill is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't have that skill";
                    return response;
                }
                int damage = skill.Damage + (new Random().Next(attacker.Intelligence));
                damage -= new Random().Next(opponent.Defeats);
                if(damage > 0)
                    opponent.HitPoints -= damage;
                if(opponent.HitPoints <= 0)
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
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<AttackResultDTO>> WeaponAttack(WeaponAttackDTO request)
        {
            var response = new ServiceResponse<AttackResultDTO>();
            try
            {
                var attacker = await _context.Characters.Include(c => c.Weapon).FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters.Include(c => c.Weapon).FirstOrDefaultAsync(c => c.Id == request.OponnentId);

                if(attacker is null ||opponent is null || attacker.Weapon is null)
                {
                     throw new Exception("No attacker, opponent or attacker weapon");   
                }

                int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
                damage -= new Random().Next(opponent.Defeats);
                if(damage > 0)
                    opponent.HitPoints -= damage;
                if(opponent.HitPoints <= 0)
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
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}