using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace testingtesting4
{
    internal class TaskFileManager
    {
        private const string FilePath = "MyTasks.json";
        public TaskFileManager()
        {
            
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, "");
            }
            
        }

        public ObservableCollection<Task>? GetAllTasks()
        {
            try
            {
                string jsonContent = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<ObservableCollection<Task>>(jsonContent);
            }
            catch (Exception)
            {
                return new ObservableCollection<Task>();
            }
        }

        //modify
        public int GetNextTaskID()
        {
            ObservableCollection<Task>? tasks = GetAllTasks() ?? new ObservableCollection<Task>();
            int id = tasks.Count > 1 ? tasks.Max(i => i.Id) + 1 : 1;
            return id;
        }

        public void CreateTask(Task newTask)
        {
            ObservableCollection<Task> tasks = GetAllTasks() ?? new ObservableCollection<Task>();
            tasks.Add(newTask);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(tasks));
        }

        public void UpdateTask(Task UpdatedTask)
        {
            ObservableCollection<Task> tasks = GetAllTasks() ?? new ObservableCollection<Task>();
            Task? task = tasks.FirstOrDefault(i => i.Id == UpdatedTask.Id);
            if (task != null)
            {
                UpdatedTask = task;
                File.WriteAllText(FilePath, JsonSerializer.Serialize(tasks));
            }
        }
        public void DeleteTask(Task DeletedTask)
        {
            ObservableCollection<Task> tasks = GetAllTasks() ?? new ObservableCollection<Task>();
            Task? task = tasks.FirstOrDefault(i => i.Id == DeletedTask.Id);
            if (task != null)
            {
                tasks.Remove(DeletedTask);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(tasks));
            }
        }

    }
}
