using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Fight
{
    public class WeaponAttackDTO
    {
        public int AttackerId { get; set; }
        public int OponnentId { get; set; }
    }
}