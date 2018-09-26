using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TaskMaster
{
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public UserGroup AssignedUserGroup { get; set; }
        public string RequiredSkill { get; set; }
        public ProjectStatus Status { get; set; }
        public List<User> CurrentWorkingUsers { get; set; }

        public Project(string name, string description, DateTime duedate, UserGroup assignedGroup, string requiredSkill, ProjectStatus projectStatus)
        {
            Name = name;
            Description = description;
            DueDate = duedate;
            AssignedUserGroup = assignedGroup;
            RequiredSkill = requiredSkill;
            Status = projectStatus;
            CurrentWorkingUsers = GetUsersFromUserGroupBasedOnSkill(assignedGroup,requiredSkill);
        }

        private List<User> GetUsersFromUserGroupBasedOnSkill(UserGroup userGroup,string skill)
        {
            if (!File.Exists("users.json"))
            {
                return new List<User>();
            }
            var json = File.ReadAllText("users.json");
            List<User> users = JsonConvert.DeserializeObject<List<User>>(json);
            return users.FindAll(user=>user.UserGroup==userGroup&&user.ProgrammingSkills.Contains(skill));
        }
    }
}
