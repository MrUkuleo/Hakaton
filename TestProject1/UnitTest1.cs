using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace YourProject.Tests
{
    [TestClass]
    public class ProgramTests
    {
        private List<User> GetUserList()
        {
            return new List<User>
            {
                new User { ID = 1, UserName = "John", UserSurname = "Doe" },
                new User { ID = 2, UserName = "Jane", UserSurname = "Smith" }
            };
        }

        [TestMethod]
        public void Test_AuthenticateUser()
        {
            // Arrange
            var users = GetUserList();
            var dbHelper = new DatabaseHelper(users);

            // Act
            var user = dbHelper.AuthenticateUser("John", "Doe");

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual("John", user.UserName);
            Assert.AreEqual("Doe", user.UserSurname);
        }

        [TestMethod]
        public void Test_GetUserRole()
        {
            // Arrange
            var roles = new List<UserRole>
            {
                new UserRole { UserID = 1, RoleID = 2 },
                new UserRole { UserID = 2, RoleID = 1 }
            };
            var dbHelper = new DatabaseHelper(roles: roles);

            // Act
            var userRole = dbHelper.GetUserRole(1);

            // Assert
            Assert.IsNotNull(userRole);
            Assert.AreEqual(1, userRole.UserID);
            Assert.AreEqual(2, userRole.RoleID);
        }

        [TestMethod]
        public void Test_GetBuildings()
        {
            // Arrange
            var buildings = new List<Building>
            {
                new Building { ID = 1, Name = "Building1" },
                new Building { ID = 2, Name = "Building2" }
            };
            var dbHelper = new DatabaseHelper(buildings: buildings);

            // Act
            var result = dbHelper.GetBuildings();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Building1", result[0].Name);
        }
    }

    public class DatabaseHelper
    {
        private readonly List<User> _users;
        private readonly List<UserRole> _roles;
        private readonly List<Building> _buildings;

        public DatabaseHelper(List<User> users = null, List<UserRole> roles = null, List<Building> buildings = null)
        {
            _users = users ?? new List<User>();
            _roles = roles ?? new List<UserRole>();
            _buildings = buildings ?? new List<Building>();
        }

        public User AuthenticateUser(string userName, string userSurname)
        {
            return _users.FirstOrDefault(u => u.UserName == userName && u.UserSurname == userSurname);
        }

        public UserRole GetUserRole(int userId)
        {
            return _roles.FirstOrDefault(r => r.UserID == userId);
        }

        public List<Building> GetBuildings()
        {
            return _buildings;
        }
    }

    public class User
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
    }

    public class UserRole
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
    }

    public class Building
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}