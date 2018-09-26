using System.Collections.Generic;

namespace TaskMaster
{
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public UserGroup UserGroup { get; set; }
        public double TotalCompensation { get; set; }
        public List<string> ProgrammingSkills { get; set; }

        public User(string name,string email,UserGroup userGroup,double totalCompensation,string[] programmingSkills)
        {
            Name = name;
            Email = email;
            UserGroup = userGroup;
            ProgrammingSkills = new List<string>(programmingSkills);
        }
    }
}
