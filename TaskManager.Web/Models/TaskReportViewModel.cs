namespace TaskManager.Web.Models
{
    public class TaskReportViewModel
    {
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public string ChartTitle => "Tỷ lệ công việc";

    }
}
