using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Shared.Dtos
{
    public class ProjectReportDto
    {
        public int Completed { get; set; }
        public int InProgress { get; set; }
    }
}
