using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Fight
{
    public class AttackResultDTO
    {
        public string Attacker { get; set; } = string.Empty;
        public string Oponnent { get; set; } = string.Empty;
        public int AttackerHP { get; set; }
        public int OponnentHP { get; set; }
        public int Damage { get; set; }
    }
}