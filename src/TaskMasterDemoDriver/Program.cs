using System;
using System.Threading.Tasks;
using TaskMaster;
namespace TaskMasterDemoDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            Task mainTask = MainAsync(args);
            mainTask.Wait();
        }
        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("====Welcome to TaskMaster Demo Driver====");

            Console.WriteLine("Please enter your user group -\n1.Administrator\t\t2.Pro Users\t3.Public Users\t4.Not a Member? Register yourself with TaskMaster :");
            try
            {
                var user = Int32.Parse(Console.ReadLine());

                switch (user)
                {
                    case 1:
                        while (true)
                        {
                            Console.WriteLine("Please choose the operation you want to perform");
                            Console.WriteLine("1.Create Task\t2.Review and Accept Task\t3.Progress Tasks Expired Due Date\t4.Exit");
                            var option = Int32.Parse(Console.ReadLine());

                            switch (option)
                            {
                                case 1:
                                    await CreateTaskAsync();
                                    break;
                                case 2:
                                    await ReviewAndAcceptTaskAsync();
                                    break;
                                case 3:
                                    await ProgressTasksExpiredDueDateAsync();
                                    break;
                                case 4:
                                    break;
                                default:
                                    WriteExceptionToConsole("please enter valid option");
                                    break;
                            }
                            if (option == 4)
                            {
                                break;
                            }
                        }
                        break;

                    case 2:
                        while (true)
                        {
                            Console.WriteLine("Please choose the operation you want to perform");
                            Console.WriteLine("1.View Tasks Assigned to this group\t2.Start Working on the task\t3.Submit Task\t4.Exit");
                            var proOption = Int32.Parse(Console.ReadLine());
                            switch (proOption)
                            {
                                case 1:
                                    await ViewAssignedTaskAsync(UserGroup.ProUsers);
                                    break;
                                case 2:
                                    await StartWorkingOnTheTaskAsync();
                                    break;
                                case 3:
                                    await SubmitTaskAsync();
                                    break;
                                case 4:
                                    break;
                                default:
                                    WriteExceptionToConsole("please enter valid option");
                                    break;
                            }
                            if (proOption == 4)
                            {
                                break;
                            }
                        }

                        break;

                    case 3:
                        while (true)
                        {
                            Console.WriteLine("Please choose the operation you want to perform");
                            Console.WriteLine("1.View Tasks Assigned to this group\t2.Start Working on the Task\t3.Submit Task\t4.Exit");
                            var publicOption = Int32.Parse(Console.ReadLine());
                            switch (publicOption)
                            {
                                case 1:
                                    await ViewAssignedTaskAsync(UserGroup.PublicUsers);
                                    break;
                                case 2:
                                    await StartWorkingOnTheTaskAsync();
                                    break;
                                case 3:
                                    await SubmitTaskAsync();
                                    break;
                                case 4:
                                    break;
                                default:
                                    WriteExceptionToConsole("please enter valid option");
                                    break;
                            }
                            if (publicOption == 4)
                            {
                                break;
                            }
                        }

                        break;

                    case 4:
                        await RegisterNewUserAsync();
                        break;
                    default:
                        WriteExceptionToConsole("Please enter valid Option");
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteExceptionToConsole(string.Format("An exception has occured:{0}", ex.Message));
            }
            Exit();
        }



        private static async Task StartWorkingOnTheTaskAsync()
        {
            Console.WriteLine("Please enter your name");
            var userName = Console.ReadLine();

            Console.WriteLine("Please enter the Task name you want to start working on");
            var taskName = Console.ReadLine();
            var result = await new TaskHandler().StartWorkingOnTheTaskAsync(userName, taskName);
            WriteResultToConsole(result);

        }

        private static async Task SubmitTaskAsync()
        {
            var taskHandler = new TaskHandler();
            var count = 0;
            Console.WriteLine("Please enter your name:");
            var userName = Console.ReadLine();
            var assignedTasks = await taskHandler.GetAllAssingedTaskAsync(userName);
            if (assignedTasks.Count == 0)
            {
                WriteResultToConsole("There are no tasks to display..");
                return;
            }
            WriteResultToConsole("Below are the Tasks you are currently working on..");
            WriteResultToConsole("Index | Name | Description | Due Date | RequiredSkill");
            foreach (var project in assignedTasks)
            {
                count++;
                WriteResultToConsole(string.Format("{0} | {1} | {2} | {3} | {4}", count, project.Name, project.Description, project.DueDate, project.RequiredSkill));
            }

            Console.WriteLine("Please enter the Task name to submit for review:");
            var projectName = Console.ReadLine();
            var result = taskHandler.SubmitTaskAsync(projectName);
            WriteResultToConsole(result.Result);
        }



        private static async Task ViewAssignedTaskAsync(UserGroup userGroup)
        {
            var taskHandler = new TaskHandler();
            var count = 0;
            var result = await taskHandler.GetAllAssingedTaskAsync(userGroup);
            if (result.Count == 0)
            {
                WriteResultToConsole("There are no tasks to display..");
                return;
            }
            WriteResultToConsole("Index | Name | Description | Due Date | RequiredSkill");
            foreach (var project in result)
            {
                count++;
                WriteResultToConsole(string.Format("{0} | {1} | {2} | {3} | {4}", count, project.Name, project.Description, project.DueDate, project.RequiredSkill));
            }
        }

        private static async Task ReviewAndAcceptTaskAsync()
        {
            var taskHandler = new TaskHandler();
            var count = 0;
            var result = await taskHandler.GetAllTasksPendingReviewAsync();
            if (result.Count == 0)
            {
                WriteResultToConsole("There are no tasks pending review..");
                return;
            }
            WriteResultToConsole("Index | Name | Description | Due Date | Assigned Group | Submitted By User");
            foreach (var project in result)
            {
                count++;
                WriteResultToConsole(string.Format("{0} | {1} | {2} | {3} | {4} | {5}", count, project.Name, project.Description, project.DueDate, project.AssignedUserGroup, project.CurrentWorkingUsers[0]));
            }

            Console.WriteLine("Please enter the Task name to accept:");
            var projectName = Console.ReadLine();

            Console.WriteLine("Please enter the compensation amount:");
            var compensation = double.Parse(Console.ReadLine());

            var taskResult = await taskHandler.AcceptTaskAsync(projectName, compensation);

            WriteResultToConsole(taskResult);
        }

        private static async Task ProgressTasksExpiredDueDateAsync()
        {
            var taskHandler = new TaskHandler();
            var count = 0;
            var tasks = await taskHandler.GetAllTasksExpiredDueDateAsync();
            if (tasks.Count == 0)
            {
                WriteResultToConsole("There are no tasks to display..");
                return;
            }
            WriteResultToConsole("Below are the tasks expired due date..");
            WriteResultToConsole("Index | Name | Description | Due Date | Assigned Group");
            foreach (var project in tasks)
            {
                count++;
                WriteResultToConsole(string.Format("{0} | {1} | {2} | {3} | {4}", count, project.Name, project.Description, project.DueDate, project.AssignedUserGroup));
            }
            Console.WriteLine("Press Enter to progress all tasks expired due date to the group next in hierarchy");
            Console.Read();
            var result = await taskHandler.ProgressAllTasksExpiredDueDateAsync();
            WriteResultToConsole(result);
        }

        private static async Task CreateTaskAsync()
        {
            Console.WriteLine("Please enter the task details (Name, Description, Due Date(in MM/DD/YYYY format),Assigned User Group(accepted values- ProUsers, PublicUsers), Required Skill for the Task) in comma separated values:\nEx : Task1,Task1 description,10/01/2018,ProUsers,c#");
            var taskDetailsLine = Console.ReadLine();
            var taskDetails = taskDetailsLine.Split(',');
            var taskHandler = new TaskHandler();
            var result = await taskHandler.CreateTaskAsync(taskDetails[0], taskDetails[1], taskDetails[2], taskDetails[3], taskDetails[4]);
            WriteResultToConsole(result);

        }

        private static async Task RegisterNewUserAsync()
        {
            Console.WriteLine("Please enter the User details (Name, Email, User Group(accepted values- ProUsers, PublicUsers), Programming skills(separated by ';')) in comma separated values:\nEx : User1,User@demo.com,ProUsers,c#;asp.net;.net");
            var userDetailsLine = Console.ReadLine();
            var userDetails = userDetailsLine.Split(',');
            var taskHandler = new TaskHandler();
            var result = await taskHandler.RegisterNewUserAsync(userDetails[0], userDetails[1], userDetails[2], userDetails[3].Split(';'));
            WriteResultToConsole(result);
        }

        private static void Exit()
        {
            Console.WriteLine("Press any key to exit..");
            Console.Read();
        }

        private static void WriteResultToConsole(string result)
        {
            ConsoleColor defaultForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(result);
            Console.ForegroundColor = defaultForegroundColor;
        }
        private static void WriteExceptionToConsole(string error)
        {
            ConsoleColor defaultForegroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = defaultForegroundColor;
        }
    }
}
