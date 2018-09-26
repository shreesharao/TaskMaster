using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskMaster;

namespace TaskMasterTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreateTaskTest()
        {
            TaskHandler taskHandler = new TaskHandler();
            var result= taskHandler.CreateTaskAsync("project2", "project1", "09/20/2018", "ProUsers","c#");

            Assert.AreSame("Task created successfully.", result.Result);
        }

        [TestMethod]
        public void SubmitTaskTest()
        {
            TaskHandler taskHandler = new TaskHandler();
            var result = taskHandler.SubmitTaskAsync("project2");

            Assert.AreSame("Task submitted for review.", result.Result);
        }

        [TestMethod]
        public void RegisterNewUserTest()
        {
            TaskHandler taskHandler = new TaskHandler();
            var result = taskHandler.RegisterNewUserAsync("shr","shr@gmail.com","ProUsers",new string[] {"c#","asp.net"});

            Assert.AreSame("User created successfully..", result.Result);
        }
    }
}
