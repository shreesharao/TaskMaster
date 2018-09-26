using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TaskMaster
{
    public class TaskHandler
    {
        /// <summary>
        /// Creats a task. Visible only to Admin users
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="duedate"></param>
        /// <param name="assignedGroup"></param>
        /// <returns></returns>
        public async Task<string> CreateTaskAsync(string name, string description, string duedate, string assignedGroup, string requiredSkill)
        {
            var date = DateTime.Now;
            UserGroup userGroup;
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(description) && string.IsNullOrEmpty(duedate) && string.IsNullOrEmpty(assignedGroup) && string.IsNullOrEmpty(requiredSkill))
            {
                return "Please provide all the values";
            }

            if (!DateTime.TryParse(duedate, out date))
            {
                return "Due Date not in correct format. Task can not be created.";
            }

            if (!Enum.TryParse<UserGroup>(assignedGroup, out userGroup))
            {
                return "User Group not valid. Task can not be created.";
            }

            try
            {
                var project = new Project(name, description, date, userGroup, requiredSkill, ProjectStatus.Assigned);
                var projects = await LoadProjectsAsync();

                //check if project already exists
                if (projects.Find(projectToCheck => projectToCheck.Name == project.Name && projectToCheck.Status != ProjectStatus.Completed) != null)
                {
                    return "Task with this name already exists.Please try again";
                }

                projects.Add(project);
                var result = await PersistProjectsAsync(projects);

                if (result)
                {
                    return "Task created successfully.";
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return "Task creation failed with unknown reason";

        }

        /// <summary>
        /// Get all assinged task based on user group
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns></returns>
        public async Task<List<Project>> GetAllAssingedTaskAsync(UserGroup userGroup)
        {
            var projects = await LoadProjectsAsync();

            var assingedTasks = projects.FindAll(project => project.AssignedUserGroup == userGroup && project.Status == ProjectStatus.Assigned);

            return assingedTasks;
        }

        public async Task<List<Project>> GetAllAssingedTaskAsync(string userName)
        {
            var projects = await LoadProjectsAsync();

            var assingedTasks = projects.FindAll(project => project.CurrentWorkingUsers[0].Name == userName && project.Status == ProjectStatus.Started);

            return assingedTasks;
        }

        /// <summary>
        /// Get all tasks pending review. Visible only to admin users
        /// </summary>
        /// <returns></returns>
        public async Task<List<Project>> GetAllTasksPendingReviewAsync()
        {
            var projects = await LoadProjectsAsync();

            var tasksPendingReview = projects.FindAll(project => project.Status == ProjectStatus.ReviewPending);

            return tasksPendingReview;
        }

        /// <summary>
        /// Move the task to reviewpending state
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public async Task<string> SubmitTaskAsync(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return "Please enter valid Task name..";
            }

            try
            {
                var projects = await LoadProjectsAsync();

                var projectToSubmit = projects.Find(project => project.Name == projectName && project.Status != ProjectStatus.Completed);

                if (projectToSubmit == null)
                {
                    return "There is no matching task. Please try again..";
                }
                projects.Remove(projectToSubmit);

                projectToSubmit.Status = ProjectStatus.ReviewPending;

                projects.Add(projectToSubmit);

                await PersistProjectsAsync(projects);

                return "Task submitted for review..";
            }
            catch (Exception ex)
            {
                return "An unexpected error occured" + ex.Message;
            }

        }

        /// <summary>
        /// Accept a task and mark it completed
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public async Task<string> AcceptTaskAsync(string projectName, double compensation)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                return "Please enter valid Task name";
            }

            var projects = await LoadProjectsAsync();
            var users = await LoadUsersAsync();

            var projectToAccept = projects.Find(project => project.Name == projectName);

            var user = projectToAccept.CurrentWorkingUsers[0];
            users.Remove(user);

            user.TotalCompensation += compensation;

            users.Add(user);

            var persistUserResult = await PersistUsersAsync(users);

            if (!persistUserResult)
            {
                return "Failed to accept the Task.Please try again..";
            }
            projects.Remove(projectToAccept);

            projectToAccept.Status = ProjectStatus.Completed;

            projects.Add(projectToAccept);

            var result = await PersistProjectsAsync(projects);

            if (result)
            {
                return "Task accepted and moved to Completed state";
            }

            return "Failed to accept the Task.Please try again..";
        }

        public async Task<string> RegisterNewUserAsync(string name, string email, string userGroup, params string[] skills)
        {
            UserGroup group;
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(userGroup) && skills.Length <= 0)
            {
                return "Please provide all the values..";
            }

            if (!Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            {
                return "Please enter a valid email address..";
            }

            if (!Enum.TryParse<UserGroup>(userGroup, out group))
            {
                return "User Group not valid. Task can not be created.";
            }


            var user = new User(name, email, group, 0, skills);
            var users = await LoadUsersAsync();

            users.Add(user);
            var result = await PersistUsersAsync(users);
            if (result)
            {
                return "User registered with TaskMaster successfully..";
            }

            return "User Registration failed with unknown reason";
        }

        public async Task<string> StartWorkingOnTheTaskAsync(string userName, string taskName)
        {
            var users = await LoadUsersAsync();
            var currentUser = users.Find(user => user.Name == userName);

            if (currentUser == null)
            {
                return "User can not be found. Please check the user name..";
            }

            var tasks = await LoadProjectsAsync();
            var currentTask = tasks.Find(task => task.Name == taskName);

            if (currentTask == null)
            {
                return "Task can not be found. Please check the Task name..";
            }

            if (!currentUser.ProgrammingSkills.Contains(currentTask.RequiredSkill))
            {
                return "Your skill set does not match the required skill of the Task..";
            }

            if (currentUser.UserGroup != currentTask.AssignedUserGroup)
            {
                return "This task is not assigned to your user group. Please verify..";
            }

            tasks.Remove(currentTask);

            currentTask.Status = ProjectStatus.Started;
            currentTask.CurrentWorkingUsers = new List<User>((new User[] { currentUser }));

            tasks.Add(currentTask);

            var result = await PersistProjectsAsync(tasks);
            if (result)
            {
                return string.Format("Task '{0}' assigned to User '{1}' successfully..", currentTask.Name, currentUser.Name);
            }
            return "Task assignment failed..";
        }

        public async Task<List<Project>> GetAllTasksExpiredDueDateAsync()
        {
            var allProjects = await LoadProjectsAsync();
            var projects = allProjects.FindAll(project => project.DueDate.Date < DateTime.Today);
            return projects;
        }

        public async Task<string> ProgressAllTasksExpiredDueDateAsync()
        {
            var allProjects = await LoadProjectsAsync();
            var projects = allProjects.FindAll(project => project.DueDate.Date < DateTime.Today);
            var tempProjects = projects;
            foreach (var project in tempProjects)
            {
                projects.Remove(project);

                if (project.AssignedUserGroup == UserGroup.ProUsers)
                {
                    project.AssignedUserGroup = UserGroup.PublicUsers;
                    project.DueDate = DateTime.Now.Add(TimeSpan.FromDays(7));
                }

                else if (project.AssignedUserGroup == UserGroup.PublicUsers)
                {
                    project.AssignedUserGroup = UserGroup.InternalBacklog;
                    project.DueDate = DateTime.Now.Add(TimeSpan.FromDays(7));
                }

                tempProjects.Add(project);
            }
            projects = tempProjects;
            PersistProjectsAsync(projects);
            return "Progressed all tasks expired due date..";
        }


        private async Task<bool> PersistProjectsAsync(List<Project> projects)
        {
            using (StreamWriter fileWriter = File.CreateText("projects.json"))
            {
                var json = JsonConvert.SerializeObject(projects);
                await fileWriter.WriteAsync(json);
            }
            return true;
        }

        private async Task<List<Project>> LoadProjectsAsync()
        {
            if (!File.Exists("projects.json"))
            {
                return new List<Project>();
            }
            var json = File.ReadAllText("projects.json");
            List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(json);
            return projects;
        }

        private async Task<bool> PersistUsersAsync(List<User> users)
        {
            using (StreamWriter fileWriter = File.CreateText("users.json"))
            {
                var json = JsonConvert.SerializeObject(users);
                await fileWriter.WriteAsync(json);
            }
            return true;
        }

        private async Task<List<User>> LoadUsersAsync()
        {
            if (!File.Exists("users.json"))
            {
                return new List<User>();
            }
            var json = File.ReadAllText("users.json");
            List<User> users = JsonConvert.DeserializeObject<List<User>>(json);
            return users;
        }
    }
}
