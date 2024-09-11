using Microsoft.Data.SqlClient;

public class User
{
    public int ID { get; set; }
    public string UserName { get; set; }
    public string UserSurname { get; set; }
    public int RoleID { get; set; }
}

public class UserRole
{
    public int UserID { get; set; }
    public int RoleID { get; set; }
}



public class UserRoleMapping
{
    public int UserID { get; set; }
    public int RoleID { get; set; }
}


public class Building
{
    public int ID { get; set; }
    public string Title { get; set; }
}

public class Desicion
{
    public int ID { get; set; }
    public int BuildingID { get; set; }
    public DateTime DesicionDate { get; set; }
    public string DesicionText { get; set; }
}

public class Conclusion
{
    public int ID { get; set; }
    public int BuildingID { get; set; }
    public DateTime ConclusionDate { get; set; }
    public string ConclusionText { get; set; }
}

class DatabaseHelper
{
    private string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Student\\Downloads\\snake\\Hakaton\\Hakaton\\Database1.mdf;Integrated Security=True";

    public async Task<List<User>> GetUsersAsync()
    {
        var users = new List<User>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("SELECT U.ID, U.UserName, U.UserSurname, R.RoleID FROM UserInfo U JOIN UserRole R ON U.ID = R.UserID", connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            ID = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            UserSurname = reader.GetString(2),
                            RoleID = reader.GetInt32(3)
                        });
                    }
                }
            }
        }
        return users;
    }

    public async Task<List<UserRole>> GetUserRolesAsync()
    {
        var userRoles = new List<UserRole>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("SELECT UserID, RoleID FROM UserRole", connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        userRoles.Add(new UserRole
                        {
                            UserID = reader.GetInt32(0),
                            RoleID = reader.GetInt32(1)
                        });
                    }
                }
            }
        }

        return userRoles;
    }

    public async Task<string> GetRoleNameAsync(int roleId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("SELECT UserRole FROM UserRole WHERE ID = @RoleID", connection))
            {
                command.Parameters.AddWithValue("@RoleID", roleId);
                return (string)await command.ExecuteScalarAsync();
            }
        }
    }


    public async Task<List<UserRoleMapping>> GetUserRoleMappingsAsync()
    {
        var userRoleMappings = new List<UserRoleMapping>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("SELECT UserID, RoleID FROM UserRole", connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        userRoleMappings.Add(new UserRoleMapping
                        {
                            UserID = reader.GetInt32(0),
                            RoleID = reader.GetInt32(1)
                        });
                    }
                }
            }
        }

        return userRoleMappings;
    }

    public async Task AddObjectAsync(string title, string desicionText, string conclusionText)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
                    DECLARE @BuildingID INT;

                    INSERT INTO Buildings (Title)
                    VALUES (@Title);

                    SET @BuildingID = SCOPE_IDENTITY();

                    INSERT INTO Desicion (BuildingID, DesicionDate, DesicionText)
                    VALUES (@BuildingID, GETDATE(), @DesicionText);

                    INSERT INTO Conclusion (BuildingID, ConclusionDate, ConclusionText)
                    VALUES (@BuildingID, GETDATE(), @ConclusionText);
                ";

                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@DesicionText", desicionText);
                command.Parameters.AddWithValue("@ConclusionText", conclusionText);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task EditObjectAsync(int id, string title, string decisionText, string conclusionText)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
                    UPDATE Buildings
                    SET Title = @Title
                    WHERE ID = @ID;

                    UPDATE Desicion
                    SET DesicionText = @DesicionText
                    WHERE BuildingID = @ID;

                    UPDATE Conclusion
                    SET ConclusionText = @ConclusionText
                    WHERE BuildingID = @ID;
                ";

                command.Parameters.AddWithValue("@ID", id);
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@DesicionText", decisionText);
                command.Parameters.AddWithValue("@ConclusionText", conclusionText);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteObjectAsync(int id)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
                DELETE FROM ConnectionBuilding WHERE BuildingID = @ID;
                DELETE FROM Desicion WHERE BuildingID = @ID;
                DELETE FROM Conclusion WHERE BuildingID = @ID;
                DELETE FROM Buildings WHERE ID = @ID;";

                command.Parameters.AddWithValue("@ID", id);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task AddUserAsync(string userName, string userSurname, int roleId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("INSERT INTO UserInfo (UserName, UserSurname) VALUES (@UserName, @UserSurname); SELECT SCOPE_IDENTITY();", connection))
            {
                command.Parameters.AddWithValue("@UserName", userName);
                command.Parameters.AddWithValue("@UserSurname", userSurname);
                int userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                using (SqlCommand roleCommand = new SqlCommand("INSERT INTO UserRole (UserID, RoleID) VALUES (@UserID, @RoleID)", connection))
                {
                    roleCommand.Parameters.AddWithValue("@UserID", userId);
                    roleCommand.Parameters.AddWithValue("@RoleID", roleId);
                    await roleCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }

    public async Task UpdateUserRoleAsync(int userId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("UPDATE UserRole SET RoleID = CASE WHEN RoleID = 1 THEN 2 ELSE 1 END WHERE UserID = @UserID", connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteUserAsync(int userId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SqlCommand command = new SqlCommand("DELETE FROM UserRole WHERE UserID = @UserID", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        await command.ExecuteNonQueryAsync();
                    }

                    using (SqlCommand command = new SqlCommand("DELETE FROM UserInfo WHERE ID = @UserID", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        await command.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }


    public async Task<User> AuthenticateUserAsync(string userName, string userSurname)
    {
        var users = await GetUsersAsync();

        return users.FirstOrDefault(u => u.UserName == userName && u.UserSurname == userSurname);
    }

    public async Task<UserRole> GetUserRoleAsync(int userId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = new SqlCommand("SELECT RoleID FROM UserRole WHERE UserID = @UserID", connection))
            {
                command.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new UserRole
                        {
                            UserID = userId,
                            RoleID = reader.GetInt32(0)
                        };
                    }
                }
            }
        }
        return null;
    }


    public async Task<List<Building>> GetBuildingsAsync()
    {
        var buildings = new List<Building>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand("SELECT ID, Title FROM Buildings", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                buildings.Add(new Building
                {
                    ID = reader.GetInt32(0),
                    Title = reader.GetString(1)
                });
            }
        }

        return buildings;
    }

    public async Task<List<Desicion>> GetDesicionsAsync()
    {
        var desicions = new List<Desicion>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand("SELECT ID, BuildingID, DesicionDate, DesicionText FROM Desicion", connection);

            SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                desicions.Add(new Desicion
                {
                    ID = reader.GetInt32(0),
                    BuildingID = reader.GetInt32(1),
                    DesicionDate = reader.GetDateTime(2),
                    DesicionText = reader.GetString(3)
                });
            }
        }

        return desicions;
    }

    public async Task<List<Conclusion>> GetConclusionsAsync()
    {
        var conclusions = new List<Conclusion>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            SqlCommand command = new SqlCommand("SELECT ID, BuildingID, ConclusionDate, ConclusionText FROM Conclusion", connection);
            SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                conclusions.Add(new Conclusion
                {
                    ID = reader.GetInt32(0),
                    BuildingID = reader.GetInt32(1),
                    ConclusionDate = reader.GetDateTime(2),
                    ConclusionText = reader.GetString(3)
                });
            }
        }

        return conclusions;
    }
}

class Interface
{
    public int GetUserAction(int maxOption)
    {
        int userAct;
        string actStr;
        do
        {
            Console.Write(">> ");
            actStr = Console.ReadLine()?.Trim();
        } while (!int.TryParse(actStr, out userAct) || userAct < 0 || userAct > maxOption);

        return userAct;
    }


    private string GetObjectFeatures(string text)
    {
        Console.Write($"{text}: ");
        string userAct = Console.ReadLine();

        return userAct;
    }

    public void DisplayMenu(string title, string[] options, bool isMain)
    {
        Console.WriteLine($"\n{title}");
        for (int i = 0; i < options.Length; i++)
        {
            Console.WriteLine($"{i + 1} - {options[i]}");
        }
        Console.WriteLine(isMain ? "0 - Выход" : "0 - Назад");
    }

    public async Task CardObjectsAsync(DatabaseHelper dbHelper, UserRole userRole)
    {
        while (true)
        {
            DisplayMenu("РАБОТА С КАРТОЧКАМИ ОБЪЕКТОВ:", new[]
            {
                "Просмотреть список карточек",
                "Добавить новый объект",
                "Редактировать существующий объект",
                "Удалить объект",
            }, false);

            int userAct = GetUserAction(4);

            switch (userAct)
            {
                case 1:
                    short count = 1;
                    Console.WriteLine("Список карточек объектов:\n");
                    List<Building> buildings = await dbHelper.GetBuildingsAsync();
                    List<Desicion> desicions = await dbHelper.GetDesicionsAsync();
                    List<Conclusion> conclusions = await dbHelper.GetConclusionsAsync();

                    foreach (var building in buildings)
                    {
                        Console.WriteLine($"#{count}");
                        Console.WriteLine($"Имя объекта: {building.Title}");
                        Console.WriteLine($"ID объекта: {building.ID}\n");

                        foreach (var desicionItem in desicions)
                        {
                            if (desicionItem.BuildingID == building.ID)
                            {
                                Console.WriteLine($"Решение: {desicionItem.DesicionText}");
                                Console.WriteLine($"Дата решения: {desicionItem.DesicionDate}\n");

                                foreach (var conclusionItem in conclusions)
                                {
                                    if (conclusionItem.BuildingID == building.ID)
                                    {
                                        Console.WriteLine($"Вывод: {conclusionItem.ConclusionText}");
                                        Console.WriteLine($"Дата вывода: {conclusionItem.ConclusionDate}");
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < 50; i++) Console.Write('_');
                        Console.WriteLine();
                        Thread.Sleep(500);

                        count++;
                    }
                    break;
                case 2:
                    if (userRole.RoleID == 2)
                    {
                        Console.WriteLine("Добавление нового объекта:\n");

                        string title = GetObjectFeatures("Введите заголовок объекта");
                        string desicion = GetObjectFeatures("Введите текст решения");
                        string conclusion = GetObjectFeatures("Введите текст вывода");

                        await dbHelper.AddObjectAsync(title, desicion, conclusion);

                        Console.WriteLine("Объект успешно добавлен.");
                    }
                    else
                    {
                        Console.WriteLine("У Вас недостаточно прав на выполнение данной операции.");
                    }
                    break;
                case 3:
                    if (userRole.RoleID == 2)
                    {
                        Console.WriteLine("Редактирование существующего объекта:\n");

                        buildings = await dbHelper.GetBuildingsAsync();
                        int editId;
                        int act;
                        string act_str_2;

                        Console.Write("Введите ID объекта для редактирования: ");
                        string editId_str = Console.ReadLine();

                        while (!int.TryParse(editId_str, out editId))
                        {
                            Console.Write("Неверный ввод, повторите попытку: ");
                            editId_str = Console.ReadLine();
                        }

                        var buildingToEdit = buildings.FirstOrDefault(b => b.ID == editId);

                        if (buildingToEdit == null)
                        {
                            Console.WriteLine("Объект с указанным ID не найден.");
                            break;
                        }

                        string newTitle = GetObjectFeatures("Введите новый заголовок объекта");
                        string newDecision = GetObjectFeatures("Введите новый текст решения");
                        string newConclusion = GetObjectFeatures("Введите новый текст вывода");


                        Console.WriteLine($"Сохранить изменения?");
                        Console.WriteLine("1 - Да, сохранить");
                        Console.WriteLine("0 - Нет, не сохранять");

                        do
                        {
                            Console.Write(">> ");
                            act_str_2 = Console.ReadLine();
                        } while (!int.TryParse(act_str_2, out act) || act < 0 || act > 1);

                        if (act == 1)
                        {
                            await dbHelper.EditObjectAsync(editId, newTitle, newDecision, newConclusion);
                            Console.WriteLine("Объект успешно отредактирован.");
                        }
                        else
                        {
                            Console.WriteLine("Редактирование отменено.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("У Вас недостаточно прав на выполнение данной операции.");
                    }

                    break;
                case 4:
                    if (userRole.RoleID == 2)
                    {
                        Console.WriteLine("Удаление объекта:\n");

                        buildings = await dbHelper.GetBuildingsAsync();
                        int deleteId;
                        int act_2;
                        string act_str;

                        Console.Write("Введите ID объекта для удаления: ");
                        string deletedId_str = Console.ReadLine();

                        while (!int.TryParse(deletedId_str, out deleteId))
                        {
                            Console.Write("Неверный ввод, повторите попытку: ");
                            deletedId_str = Console.ReadLine();
                        }

                        var buildingToDelete = buildings.FirstOrDefault(b => b.ID == deleteId);

                        if (buildingToDelete != null)
                        {
                            Console.WriteLine($"Вы уверены, что хотите удалить объект '{buildingToDelete.Title}'?");
                            Console.WriteLine("1 - Да, удалить");
                            Console.WriteLine("0 - Нет, не удалять");

                            do
                            {
                                Console.Write(">> ");
                                act_str = Console.ReadLine();
                            } while (!int.TryParse(act_str, out act_2) || act_2 < 0 || act_2 > 1);

                            if (act_2 == 1)
                            {
                                await dbHelper.DeleteObjectAsync(deleteId);
                                Console.WriteLine("Объект успешно удален.");
                            }
                            else
                            {
                                Console.WriteLine("Удаление отменено.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Объект с указанным ID не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("У Вас недостаточно прав на выполнение данной операции.");
                    }

                    break;
                case 0:
                    Console.WriteLine("Возврат в главное меню...");
                    return;
            }
        }
    }

    public async Task ManageUsersAsync(DatabaseHelper dbHelper, UserRole userRole)
    {
        while (true)
        {
            DisplayMenu("УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ", new[]
            {
            "Просмотреть список пользователя",
            "Добавить нового пользователя",
            "Сменить права для пользователя",
            "Удалить пользователя",
            }, false);

            int userAct = GetUserAction(5);


            switch (userAct)
            {
                case 1:
                    Console.WriteLine("\nСписок пользователей:\n");

                    List<User> users = await dbHelper.GetUsersAsync();
                    short count = 1;

                    foreach (var user in users)
                    {
                        string role = user.RoleID == 1 ? "Worker" : "Admin";
                        Console.WriteLine($"#{count}.");
                        Console.WriteLine($"Имя: {user.UserName}");
                        Console.WriteLine($"Фамилия: {user.UserSurname}");
                        Console.WriteLine($"ID: {user.ID}");
                        Console.WriteLine($"Роль: {role}");

                        for (int i = 0; i < 50; i++) Console.Write('_');
                        Console.WriteLine();

                        count++;
                    }

                    break;
                case 2:
                    if (userRole.RoleID == 2)
                    {
                        int newUserRole;
                        string newUserRoleStr;

                        Console.Write("Введите имя нового пользователя: ");
                        string newUserName = Console.ReadLine();
                        Console.Write("Введите фамилию нового пользователя: ");
                        string newUserSurname = Console.ReadLine();
                        do
                        {
                            Console.Write("Введите роль нового пользователя (1 - worker, 2 - admin): ");
                            newUserRoleStr = Console.ReadLine();
                        } while (!int.TryParse(newUserRoleStr, out newUserRole) || (newUserRole != 1 && newUserRole != 2));

                        await dbHelper.AddUserAsync(newUserName, newUserSurname, newUserRole);
                        Console.WriteLine("Новый пользователь успешно добавлен.");
                    }
                    else
                    {
                        Console.WriteLine("У вас нет прав на выполнение данной операции.");
                    }

                    break;
                case 3:
                    if (userRole.RoleID == 2)
                    {
                        Console.Write("Введите ID пользователя для смены прав: ");
                        string userIdToUpdateStr = Console.ReadLine();

                        if (int.TryParse(userIdToUpdateStr, out int userIdToUpdate))
                        {
                            await dbHelper.UpdateUserRoleAsync(userIdToUpdate);
                            Console.WriteLine("Права пользователя успешно изменены.");
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод ID. Попробуйте снова.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("У вас нет прав на выполнение данной операции.");
                    }
                    break;
                case 4:
                    if (userRole.RoleID == 2)
                    {
                        Console.Write("Введите ID пользователя для удаления: ");
                        string userIdToDeleteStr = Console.ReadLine();

                        if (int.TryParse(userIdToDeleteStr, out int userIdToDelete))
                        {
                            await dbHelper.DeleteUserAsync(userIdToDelete);
                            Console.WriteLine("Пользователь успешно удален.");
                        }
                        else
                        {
                            Console.WriteLine("Неверный ввод ID. Попробуйте снова.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("У вас нет прав на выполнение данной операции.");
                    }
                    break;
                case 0:
                    Console.WriteLine("Возврат в главное меню...");
                    return;
            }
        }
    }

}

class Program
{
    static async Task MainMenuAsync(DatabaseHelper dbHelper)
    {
        Interface cumOnMyFace = new Interface();
        DatabaseHelper db = new DatabaseHelper();

        int act;

        User authenticatedUser = null;

        while (authenticatedUser == null)
        {
            Console.Write("\nВведите имя: ");
            string userName = Console.ReadLine();

            Console.Write("Введите фамилию: ");
            string userSurname = Console.ReadLine();

            authenticatedUser = await db.AuthenticateUserAsync(userName, userSurname);
            if (authenticatedUser == null)
            {
                Console.WriteLine("Неверное имя или фамилия. Пожалуйста, попробуйте снова.");
            }
        }

        Console.WriteLine($"\nЗдравствуйте, {authenticatedUser.UserName}!");

        var userRole = await dbHelper.GetUserRoleAsync(authenticatedUser.ID);
        bool isAdmin = userRole != null && userRole.RoleID == 2;

        while (true)
        {
            cumOnMyFace.DisplayMenu("ГЛАВНОЕ МЕНЮ:", new[]
            {
                "Работа с карточками объектов",
                "Управление пользователями"
            }, true);

            act = cumOnMyFace.GetUserAction(2);

            switch (act)
            {
                case 0: return;
                case 1: await cumOnMyFace.CardObjectsAsync(dbHelper, userRole); break;
                case 2: await cumOnMyFace.ManageUsersAsync(dbHelper, userRole); break;
            }
        }
    }
    static async Task Main(string[] args)
    {
        DatabaseHelper db = new DatabaseHelper();

        Console.WriteLine("===ИНТЕРФЕЙС ПО РАБОТЕ С КАРТОЧКАМИ===");

        await MainMenuAsync(db);
    }
}