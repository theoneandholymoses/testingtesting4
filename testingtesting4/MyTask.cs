using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testingtesting4
{
    internal class MyTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime TaskTime { get; set; }
        public TimeSpan Duration { get; set; }

        public MyTask(int id, string title, string description, string location, DateTime taskTime, TimeSpan? duration = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Location = location;
            TaskTime = taskTime;
            Duration = duration ?? TimeSpan.FromHours(0.5);
        }
    }
}
