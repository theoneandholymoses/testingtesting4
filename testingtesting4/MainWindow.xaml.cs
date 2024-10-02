using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.VisualBasic;
using System.Text.Json;

namespace testingtesting4
{
    public partial class MainWindow : Window
    {
        private DateTime currentWeekStart;
        private const int HOURS_IN_DAY = 24;
        private TaskFileManager taskFileManager = new TaskFileManager();
        private ObservableCollection<MyTask>? tasks;

        public MainWindow()
        {
            InitializeComponent();
            InitializeDateSelectors();
            InitializeWeekGrid();
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

        private async void InitializeWeekGrid()
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
                    cellBorder.MouseLeftButtonUp += (s, e) =>
                    {
                        int columnIndex = Grid.GetColumn(cellBorder);
                        int rowIndex = Grid.GetRow(cellBorder);

                        // Determine the selected day and hour
                        if (columnIndex > 0 && rowIndex > 0) // Column 0 is for hours, Column 1 is for the first day
                        {
                            DateTime selectedDate = currentWeekStart.AddDays(columnIndex - 1); // Adjust for zero-based index
                            int selectedHour = rowIndex - 1; // Adjust for zero-based index

                            // Set the DatePicker to the selected date
                            TaskDatePicker.SelectedDate = selectedDate;

                            // Set the start time ComboBox to the selected hour
                            TaskStartTimeComboBox.SelectedItem = TaskStartTimeComboBox.Items[selectedHour];
                            TaskNameBox.Text = "";
                            TaskIDBox.Text = "";
                            TaskDescriptionBox.Text = "";
                            TaskLocationBox.Text = "";
                            AllDayCheckBox.IsChecked = false; // Reset the all-day checkbox

                            // Open the pop-up
                            TaskPopup.IsOpen = true;
                        };
                    };

                    Grid.SetRow(cellBorder, hour + 1); // Position according to hour
                    Grid.SetColumn(cellBorder, day + 1); // Position according to day
                    WeekGrid.Children.Add(cellBorder);
                }
            }
            await AddTasksToGrid();
            HighlightCurrentHour();
            HighlightCurrentDate();
        }

        private async Task AddTasksToGrid()
        {
            // Sample JSON parsing
            ObservableCollection<MyTask> tasks = taskFileManager.GetAllTasks();
            foreach (MyTask task in tasks)
            {
                // Ensure TaskTime is parsed correctly
                DateTime taskTime = task.TaskTime;
                // Directly use TimeSpan.Parse, assuming Duration is in "hh:mm:ss" format
                TimeSpan taskDuration = task.Duration;

                // Calculate the starting hour and duration in rows
                int startHourRow = taskTime.Hour;
                // Assuming a 24-hour format
                double temp = taskDuration.TotalHours;
                int durationInRows = (int)taskDuration.TotalHours;
                if((int)temp != temp)
                {
                    durationInRows = (int)temp + 1;
                }

                // Ensure duration does not exceed available rows
                if (durationInRows + startHourRow > HOURS_IN_DAY)
                {
                    durationInRows = HOURS_IN_DAY - startHourRow; // Limit to available rows
                }
                int columnIndex = (taskTime - currentWeekStart).Days; // This assumes startOfWeek is the first day of your grid

                // Ensure the column index is within bounds
                if (columnIndex < 0 || columnIndex >= 7) // Assuming you have 7 columns for the week
                {
                    continue; // Skip if the date is outside the week's range
                }
                // Create a Border for the task rectangle
                Border taskBorder = new Border
                {
                    Name = "TaskBorderWeek",
                    Background = GetRandomBrush(), // Set random background color
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5) // Optional for rounded corners
                };
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = task.Title, // Set the task title
                    Tag = task.Id,
                    Foreground = Brushes.White, // Set text color
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };

                // Add the TextBlock to the Border
                taskBorder.Child = titleTextBlock;
                taskBorder.MouseLeftButtonUp += (s, e) =>
                {
                    Task_Clicked(s, e);
                };

                // Set the Grid position for the task
                Grid.SetRow(taskBorder, startHourRow + 1); // Row + 1 to account for the header
                Grid.SetColumn(taskBorder, columnIndex + 1); // Assuming you want to display it on the first day (column 1)

                // Set the RowSpan to stretch according to the duration
                Grid.SetRowSpan(taskBorder, durationInRows);

                // Add the task rectangle to the grid
                Panel.SetZIndex(taskBorder, 1); // Ensure it's above other elements
                WeekGrid.Children.Add(taskBorder);

            }

        }


        private void BackToToday_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            InitializeWeekGrid();
            UpdateComboBoxes();

        }

        // Button Click Events for Next and Previous Week
        private void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(-7);
            InitializeWeekGrid();
            UpdateComboBoxes();
        }

        private void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            currentWeekStart = currentWeekStart.AddDays(7);
            InitializeWeekGrid();
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
                Panel.SetZIndex(highlightLine, -1); // Ensure it stays behind other elements
                WeekGrid.Children.Add(highlightLine);
            }
        }

        private void HighlightCurrentDate()
        {
            DateTime now = DateTime.Now;

            // Check if current date falls within the current week
            if (now >= currentWeekStart && now <= currentWeekStart.AddDays(6))
            {
                int currentDay = (int)(now.DayOfWeek == DayOfWeek.Sunday ? 0 : now.DayOfWeek - DayOfWeek.Sunday);

                Rectangle highlightColumn = new Rectangle
                {
                    Fill = Brushes.LightBlue,
                    Opacity = 0.3, // Set this based on the row height
                };

                Grid.SetColumn(highlightColumn, currentDay + 1);
                Grid.SetRow(highlightColumn, 0);
                Panel.SetZIndex(highlightColumn, -1); // Ensure it stays behind other elements
                Grid.SetRowSpan(highlightColumn,24); // Span across all days
                WeekGrid.Children.Add(highlightColumn);
            }
        }


        private void Task_Clicked(object sender, MouseButtonEventArgs e)
        {
            // Assuming the sender is a Border with task information
            Border taskBorder = sender as Border;

            if (taskBorder != null)
            {
                // Find the task associated with this border
                TextBlock titleTextBlock = taskBorder.Child as TextBlock;

                // You might want to get the title from the taskBorder or a data context
                int taskId = int.Parse(titleTextBlock.Tag.ToString()); // Assuming the title is stored here
                // Retrieve the task details from your task collection
                MyTask selectedTask = taskFileManager.GetAllTasks().FirstOrDefault(t => t.Id == taskId);

                if (selectedTask != null)
                {
                    // Populate the fields in the TaskPopup with the task details
                    TaskIDBox.Text = selectedTask.Id.ToString();
                    TaskNameBox.Text = selectedTask.Title;
                    TaskDescriptionBox.Text = selectedTask.Description;
                    TaskLocationBox.Text = selectedTask.Location;
                    TaskDatePicker.SelectedDate = selectedTask.TaskTime; // Assuming this is the date
                    TaskStartTimeComboBox.SelectedItem = TaskStartTimeComboBox.Items
                                    .Cast<ComboBoxItem>()
                                    .FirstOrDefault(item => item.Tag.ToString() == $"{selectedTask.TaskTime.Hour:00}:00");
                    if (selectedTask.Duration.Minutes % 10 == 0)
                    {
                        TaskEndTimeComboBox.SelectedItem = TaskEndTimeComboBox.Items
                                         .Cast<ComboBoxItem>()
                                         .FirstOrDefault(item => item.Tag.ToString() == $"{(selectedTask.Duration.Hours + selectedTask.TaskTime.Hour):00}:00");
                    }
                    else
                    {
                        TaskEndTimeComboBox.SelectedItem = TaskEndTimeComboBox.Items
                                         .Cast<ComboBoxItem>()
                                         .FirstOrDefault(item => item.Tag.ToString() == "23:59");
                    }
                    AllDayCheckBox.IsChecked = selectedTask.Duration == new TimeSpan(23,59,00); // Assuming this property exists

                    // Open the TaskPopup
                    TaskPopup.IsOpen = true;
                }
            }
        }

        private void CreateTaskButton_Click(object sender, RoutedEventArgs e)
        {
            TaskNameBox.Text = "";
            TaskIDBox.Text = "";
            TaskDescriptionBox.Text = "";
            TaskLocationBox.Text = "";
            AllDayCheckBox.IsChecked = false;
            TaskPopup.IsOpen = true;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int? taskID = int.Parse(TaskIDBox.Text);
            taskFileManager.DeleteTask((int)taskID);
            TaskPopup.IsOpen = false;
            TaskIDBox.Text = null;
            InitializeWeekGrid();
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Get task details from the textboxes
            int? taskID = null;
            if (TaskIDBox.Text != "")
            {
                taskID = int.Parse(TaskIDBox.Text);
            }
            string taskName = TaskNameBox.Text;
            string taskDescription = TaskDescriptionBox.Text;
            string taskLocation = TaskLocationBox.Text;

            // Get the selected date from the DatePicker
            DateTime taskDate = TaskDatePicker.SelectedDate ?? DateTime.Now;

            // Get start and end time from the ComboBoxes
            string startTimeString = (TaskStartTimeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
            string endTimeString = (TaskEndTimeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            DateTime taskStartTime = taskDate.Date.Add(TimeSpan.Parse(startTimeString ?? "00:00"));
            DateTime taskEndTime = taskDate.Date.Add(TimeSpan.Parse(endTimeString ?? "00:00"));

            // Check if "All Day" is checked
            if (AllDayCheckBox.IsChecked == true)
            {
                taskStartTime = taskDate.Date; // Start at 00:00
                taskEndTime = taskDate.Date.AddDays(1).AddTicks(-1); // End at 23:59:59.9999999
            }

            // Create a new MyTask object
            if (taskID != null && taskFileManager.GetAllTasks().Any(t => t.Id == taskID))
            {
                MyTask updatedTask = new MyTask((int)taskID, taskName, taskDescription, taskLocation, taskStartTime, taskEndTime - taskStartTime);
                taskFileManager.UpdateTask(updatedTask);
                MessageBox.Show("updated successfully " + taskName + taskID);
            }
            else
            {

                MyTask newTask = new MyTask(0, taskName, taskDescription, taskLocation, taskStartTime, taskEndTime - taskStartTime);
                newTask.Id = taskFileManager.GetNextTaskID();
                // (Optional) Add logic to store or display the new task
                taskFileManager.CreateTask(newTask);
            }
            // Close the popup after submission
            TaskPopup.IsOpen = false;
            TaskIDBox.Text = null;
            InitializeWeekGrid();
            // Clear the fields for next input
            TaskNameBox.Text = "";
            TaskDescriptionBox.Text = "";
            TaskLocationBox.Text = "";
            TaskDatePicker.SelectedDate = null;
            TaskStartTimeComboBox.SelectedIndex = -1; // Reset the start time selection
            TaskEndTimeComboBox.SelectedIndex = -1; // Reset the end time selection
            AllDayCheckBox.IsChecked = false; // Reset the all-day checkbox

        }

        // Event handlers for All Day checkbox
        private void AllDayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TaskStartTimeComboBox.IsEnabled = false;
            TaskEndTimeComboBox.IsEnabled = false;
        }

        private void AllDayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TaskStartTimeComboBox.IsEnabled = true;
            TaskEndTimeComboBox.IsEnabled = true;
        }
        private SolidColorBrush GetRandomBrush()
        {
            Random random = new Random();
            Color randomColor = Color.FromArgb(
                255, // Alpha
                (byte)random.Next(256), // Red
                (byte)random.Next(256), // Green
                (byte)random.Next(256)  // Blue
            );
            return new SolidColorBrush(randomColor); // Return a SolidColorBrush
        }



        //*************************************************************************************************
        // intercets with - adjust the width 
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
