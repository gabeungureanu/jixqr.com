using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTypes.ModelDataTypes.ContentManager
{
    public class AIConfigurationSetting
    {
        public Guid ID { get; set; }
        public Guid ContentTypeID { get; set; }
        public string? TextboxOne { get; set; }
        public string? TextboxTwo { get; set; }
        public string? TextboxThree { get; set; }
        public string? TextboxFour { get; set; }
        public string? TextboxFive { get; set; }
        public string? TextboxSix { get; set; }
    }
}
