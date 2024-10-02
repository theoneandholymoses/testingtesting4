using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
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

        public ObservableCollection<MyTask>? GetAllTasks()
        {
            try
            {
                string jsonContent = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<ObservableCollection<MyTask>>(jsonContent);
            }
            catch (Exception)
            {
                return new ObservableCollection<MyTask>();
            }
        }

        //modify
        public int GetNextTaskID()
        {
            ObservableCollection<MyTask>? tasks = GetAllTasks() ?? new ObservableCollection<MyTask>();
            int id = tasks.Count > 1 ? tasks.Max(i => i.Id) + 1 : 1;
            return id;
        }

        public void CreateTask(MyTask newTask)
        {
            ObservableCollection<MyTask> tasks = GetAllTasks() ?? new ObservableCollection<MyTask>();
            tasks.Add(newTask);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(tasks));
        }

        public void UpdateTask(MyTask updatedTask)
        {
            ObservableCollection<MyTask> tasks = GetAllTasks() ?? new ObservableCollection<MyTask>();
            MyTask? task = tasks.FirstOrDefault(i => i.Id == updatedTask.Id);
            if (task != null)
            {
                tasks.Remove(task);
                tasks.Add(updatedTask);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(tasks));
            }
        }
        public void DeleteTask(int id)
        {
            ObservableCollection<MyTask> tasks = GetAllTasks() ?? new ObservableCollection<MyTask>();
            MyTask? task = tasks.FirstOrDefault(i => i.Id == id);
            if (task != null)
            {
                tasks.Remove(task);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(tasks));
            }
        }

    }
}
