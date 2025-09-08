using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Shared.Dtos
{
    public class ProjectProgressDto
    {
        public string Name { get; set; }
        public int ProgressPercent { get; set; } // % hoàn thành
    }
}
