using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.VisualBasic;

namespace testingtesting4
{
    public partial class MainWindow : Window
    {
        private DateTime currentWeekStart;
        private const int HOURS_IN_DAY = 24;
        private TaskFileManager taskFileManager;
        private ObservableCollection<MyTask>? tasks;

        public MainWindow()
        {
            InitializeComponent();
            taskFileManager = new TaskFileManager();
            InitializeDateSelectors();
            InitializeWeekGrid();
            LoadTasks();
        }

        private void InitializeDateSelectors()
        {
            // Populate the MonthComboBox with month names
            for (int i = 1; i <= 12; i++)
            {
                MonthComboBox.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
            }

            // Set the current month as selected
            MonthComboBox.SelectedIndex = DateTime.Now.Month - 1;

            // Populate the YearComboBox with a range of years (from 10 years ago to 10 years in the future)
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 10; i <= currentYear + 10; i++)
            {
                YearComboBox.Items.Add(i.ToString());
            }

            // Set the current year as selected
            YearComboBox.SelectedItem = currentYear.ToString();

            // Set the current week's start date (Sunday)
            currentWeekStart = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
        }

        private void InitializeWeekGrid()
        {
            WeekGrid.Children.Clear();
            WeekGrid.RowDefinitions.Clear();
            WeekGrid.ColumnDefinitions.Clear();

            // Create 24 rows for the hours
            for (int i = 0; i <= HOURS_IN_DAY; i++)
            {
                WeekGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Create an additional column for the hours
            WeekGrid.ColumnDefinitions.Add(new ColumnDefinition()); // For the hour labels

            // Create 7 columns for the days of the week
            for (int i = 0; i < 7; i++)
            {
                WeekGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Set column headers (dates)
            for (int day = 0; day < 7; day++)
            {
                DateTime currentDate = currentWeekStart.AddDays(day);
                TextBlock header = new TextBlock
                {
                    Text = currentDate.ToString("dd/MM - ddd"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, day + 1); // Shift right by one to accommodate the hour column
                WeekGrid.Children.Add(header);
            }

            // Add hour labels to the first column
            for (int hour = 0; hour < HOURS_IN_DAY; hour++)
            {
                // Create a Border for each hour label
                Border hourLabelBorder = new Border
                {
                    BorderBrush = Brushes.Gray, // Set the border color
                    BorderThickness = new Thickness(0.2), // Set the border thickness
                    Background = Brushes.Transparent, // Set background color
                    Child = new TextBlock // Place the TextBlock inside the border
                    {
                        Text = $"{hour:00}:00", // Format hour as 00:00
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                Grid.SetRow(hourLabelBorder, hour + 1); // Shift down by one for each hour
                Grid.SetColumn(hourLabelBorder, 0); // First column for hour labels
                WeekGrid.Children.Add(hourLabelBorder);
            }
            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < HOURS_IN_DAY; hour++)
                {
                    // Create a Border for each cell
                    Border cellBorder = new Border
                    {
                        BorderBrush = Brushes.Gray, // Set the border color
                        BorderThickness = new Thickness(0.2), // Set the border thickness
                        Background = Brushes.Transparent, // Set background color
                    };

                    // Handle mouse enter event for highlighting
                    cellBorder.MouseEnter += (s, e) =>
                    {
                        cellBorder.Background = Brushes.LightBlue; // Change background on hover
                    };

                    // Handle mouse leave event to remove highlight
                    cellBorder.MouseLeave += (s, e) =>
                    {
                        cellBorder.Background = Brushes.Transparent; // Reset background
                    };

                    // Handle mouse left button down event for clicking
                    cellBorder.MouseLeftButtonDown += (s, e) =>
                    {
                        // Perform the action you want on cell click                   
                    };

                    Grid.SetRow(cellBorder, hour + 1); // Position according to hour
                    Grid.SetColumn(cellBorder, day + 1); // Position according to day
                    WeekGrid.Children.Add(cellBorder);
                }
            }

            HighlightCurrentHour();
        }

       

        private void BackToToday_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            InitializeWeekGrid();
            LoadTasks();
            UpdateComboBoxes();

        }

        // Button Click Events for Next and Previous Week
        private void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(-7);
            InitializeWeekGrid();
            LoadTasks();
            UpdateComboBoxes();
        }

        private void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(7);
            InitializeWeekGrid();
            LoadTasks();
            UpdateComboBoxes();
        }

        private void UpdateComboBoxes()
        {
            // Update MonthComboBox
            MonthComboBox.SelectedIndex = currentWeekStart.Month - 1; // Months are 1-based
            YearComboBox.SelectedItem = currentWeekStart.Year; // Set the year

            // Update the CurrentMonthYear TextBlock
            CurrentMonthYear.Text = $"{MonthComboBox.SelectedItem} {YearComboBox.SelectedItem}";
        }

        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MonthComboBox.SelectedItem != null)
            {
                int selectedMonth = MonthComboBox.SelectedIndex + 1; // Index is 0-based
                if (currentWeekStart.Month != selectedMonth)
                {
                    DateTime firstDayOfSelectedMonth = new DateTime(DateTime.Now.Year, selectedMonth, 1);
                    currentWeekStart = firstDayOfSelectedMonth.StartOfWeek(DayOfWeek.Sunday);
                    InitializeWeekGrid();
                    LoadTasks();
                }
                UpdateCurrentMonthYear();
            }
        }

        private void YearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (YearComboBox.SelectedItem != null)
            {
                int selectedYear = int.Parse(YearComboBox.SelectedItem.ToString());
                if (currentWeekStart.Year != selectedYear)
                {
                    DateTime firstDayOfSelectedMonth = new DateTime(selectedYear, DateTime.Now.Month, 1);
                    currentWeekStart = firstDayOfSelectedMonth.StartOfWeek(DayOfWeek.Sunday);
                    InitializeWeekGrid();
                    LoadTasks();
                }
                UpdateCurrentMonthYear();
            }
        }

        private void UpdateCurrentMonthYear()
        {
            // Get selected month and year
            string selectedMonth = MonthComboBox.SelectedItem as string;
            string selectedYear = YearComboBox.SelectedItem as string;

            // Update the TextBlock if both selections are made
            if (selectedMonth != "" && selectedYear != "")
            {
                CurrentMonthYear.Text = $"{selectedMonth} {selectedYear}";
            }
        }

        // Method to highlight the current hour with a horizontal line
        private void HighlightCurrentHour()
        {
            DateTime now = DateTime.Now;

            // Check if current date falls within the current week
            if (now >= currentWeekStart && now <= currentWeekStart.AddDays(6))
            {
                int currentHour = now.Hour;
                int currentDay = (int)(now.DayOfWeek == DayOfWeek.Sunday ? 0 : now.DayOfWeek - DayOfWeek.Sunday);

                Rectangle highlightLine = new Rectangle
                {
                    Fill = Brushes.LightBlue,
                    Opacity = 0.3,
                    Height = 30 // Set this based on the row height
                };

                Grid.SetRow(highlightLine, currentHour + 1);
                Grid.SetColumnSpan(highlightLine, 8); // Span across all days
                WeekGrid.Children.Add(highlightLine);
            }
        }
        // Load tasks into the grid
        private void LoadTasks()
        {
            //tasks = taskFileManager.GetAllTasks();
            //if (tasks != null)
            //{
            //    foreach (var task in tasks)
            //    {
            //        DrawTask(task);
            //    }
            //}
        }

        private void DrawTask(MyTask task)
        {
            DateTime taskDate = task.TaskTime.Date;
            if (taskDate >= currentWeekStart && taskDate <= currentWeekStart.AddDays(6))
            {
                int dayIndex = (int)(taskDate.DayOfWeek == DayOfWeek.Sunday ? 0 : taskDate.DayOfWeek - DayOfWeek.Sunday);
                int hourIndex = task.TaskTime.Hour;

                double taskHeight = 30; // Base height for a task
                double taskWidth = (double)task.Duration.TotalHours / HOURS_IN_DAY * 100; // Width based on duration

                Polygon triangle = new Polygon
                {
                    Fill = Brushes.LightCoral,
                    Points = new PointCollection
                    {
                        new Point(0, taskHeight),
                        new Point(taskWidth, taskHeight),
                        new Point(taskWidth / 2, 0)
                    },
                    Tag = task
                };

                triangle.MouseLeftButtonDown += Task_Clicked;

                Grid.SetRow(triangle, hourIndex + 1);
                Grid.SetColumn(triangle, dayIndex + 1);
                WeekGrid.Children.Add(triangle);
            }
        }

        private void Task_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Polygon taskPolygon && taskPolygon.Tag is MyTask task)
            {
                // Open an edit dialog or form here
                //EditTask(task);
                LoadTasks(); // Reload tasks after editing
            }
        }

        private void EditTask(MyTask task)
        {
            //    // Implement the editing logic here (e.g., open a new window to edit task details)
            //    // After editing, refresh the tasks
            //    taskFileManager.UpdateTask(task); // Save updated task
        }

        private void CreateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            //    // Logic to create a new task
            //    // Show a dialog or form to get task details, then call CreateTask
            //    MyTask newTask = new MyTask(taskFileManager.GetNextTaskID(), "New Task", "Description", "Location", DateTime.Now, null);
            //    taskFileManager.CreateTask(newTask);
            //    LoadTasks(); // Refresh the task list
        }

    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
