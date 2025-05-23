﻿//using Microsoft.Data.Sqlite;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Data.Common;
//using System.IO;
//using System.Threading.Tasks;
//using 随机抽取学号_Avalonia.Views;

//namespace 随机抽取学号_Avalonia.Classes
//{
//    public class Student
//    {
//        public int StudentNumber { get; set; }//学号
//        public string Name { get; set; }//姓名
//        public int Gender { get; set; }//性别
//        public string PhotoPath { get; set; }//图片路径
//    }
//    public static class StudentManager
//    {
//        public static string StudentDataBasePath;// 记录学生信息数据库路径
//        public static string CheckedStudentDataBasePath;// 记录抽取范围中的学生信息数据库路径
//        public static int SaveStudentsProcess;// 记录保存学生信息进度
//        public static int SaveCheckedStudentsProcess;// 记录保存抽取范围中的学生信息进度
//        public static ObservableCollection<Student> StudentList = new ObservableCollection<Student>();// 记录所有学生信息
//        public static List<ItemIndexRange> SelectedRanges = new List<ItemIndexRange>();// 记录抽取范围中的连续范围，如[1,1],[2,3]等
//        public static List<int> CheckedStudents = new List<int>();// 记录抽取范围中的学生的学号
//        public static async Task InitializeDatabase()
//        {
//            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
//            StorageFile StudentDBFile = Task.Run(async () => await localFolder.CreateFileAsync("students.db", CreationCollisionOption.OpenIfExists)).Result;
//            StudentDataBasePath = StudentDBFile.Path;
//            StorageFile CheckedStudentDBFile = Task.Run(async () => await localFolder.CreateFileAsync("checkedstudents.db", CreationCollisionOption.OpenIfExists)).Result;
//            CheckedStudentDataBasePath = CheckedStudentDBFile.Path;
//            await CreateTablesAsync();
//        }
//        private static async Task CreateTablesAsync()
//        {
//            try
//            {
//                using (SqliteConnection db = new SqliteConnection($"Filename={StudentDataBasePath}"))
//                {
//                    await db.OpenAsync();
//                    // 创建 Students 表，StudentNumber 作为主键
//                    string createStudentsTable = "CREATE TABLE IF NOT EXISTS Students (StudentNumber INT PRIMARY KEY, Name NVARCHAR(2048), Gender INT, PhotoPath NVARCHAR(2048))";
//                    SqliteCommand createStudentsTableCmd = new SqliteCommand(createStudentsTable, db);
//                    await createStudentsTableCmd.ExecuteNonQueryAsync();
//                }

//                using (SqliteConnection db = new SqliteConnection($"Filename={CheckedStudentDataBasePath}"))
//                {
//                    await db.OpenAsync();
//                    string createStudentsTable = "CREATE TABLE IF NOT EXISTS CheckedStudents (FirstIndex INT,LastIndex INT)";
//                    SqliteCommand createStudentsTableCmd = new SqliteCommand(createStudentsTable, db);
//                    await createStudentsTableCmd.ExecuteNonQueryAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                PopupNotice popupNotice = new PopupNotice("创建数据库失败" + ex.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//        }

//        public static async Task<ObservableCollection<Student>> LoadStudentsAsync()
//        {
//            try
//            {
//                ObservableCollection<Student> students = new ObservableCollection<Student>();
//                using (SqliteConnection db = new SqliteConnection($"Filename={StudentDataBasePath}"))
//                {
//                    await db.OpenAsync();
//                    string selectCommand = "SELECT * FROM Students";
//                    SqliteCommand selectCmd = new SqliteCommand(selectCommand, db);
//                    using (SqliteDataReader query = await selectCmd.ExecuteReaderAsync())
//                    {
//                        while (await query.ReadAsync())
//                        {
//                            Student student = new Student
//                            {
//                                StudentNumber = query.GetInt32(0),
//                                Name = query.GetString(1),
//                                Gender = query.GetInt32(2),
//                                PhotoPath = query.GetString(3)
//                            };
//                            students.Add(student);
//                        }
//                    }
//                }
//                return students;
//            }
//            catch (SqliteException sqliteEx)
//            {
//                //处理数据库异常
//                PopupNotice popupNotice = new PopupNotice("加载学生信息失败,发生数据库异常" + sqliteEx.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            catch (UnauthorizedAccessException unauthorizedEx)
//            {
//                // 处理权限不足异常
//                PopupNotice popupNotice = new PopupNotice("加载学生信息失败,权限不足" + unauthorizedEx.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            catch (IOException ioEx)
//            {
//                // 处理输入输出异常，如文件损坏
//                PopupNotice popupNotice = new PopupNotice("加载学生信息失败,发生文件操作异常" + ioEx.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            catch (Exception ex)
//            {
//                PopupNotice popupNotice = new PopupNotice("加载学生信息失败,发生未知错误" + ex.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            return new ObservableCollection<Student>();
//        }

//        public static async Task SaveStudentsAsync(ObservableCollection<Student> students)
//        {
//            using (SqliteConnection connection = new SqliteConnection($"Filename={StudentDataBasePath}"))
//            {
//                try
//                {
//                    await connection.OpenAsync();
//                    // 开始事务
//                    using (DbTransaction transaction = connection.BeginTransaction())
//                    {
//                        try
//                        {
//                            // 清空数据库中的数据
//                            string clearQuery = "DELETE FROM Students";
//                            using (SqliteCommand clearCommand = new SqliteCommand(clearQuery, connection, (SqliteTransaction)transaction))
//                            {
//                                await clearCommand.ExecuteNonQueryAsync();
//                            }
//                            string insertQuery = "INSERT INTO Students (StudentNumber, Name, Gender, PhotoPath) VALUES (@StudentNumber, @Name, @Gender, @PhotoPath)";
//                            using (SqliteCommand insertCommand = new SqliteCommand(insertQuery, connection, (SqliteTransaction)transaction))
//                            {
//                                insertCommand.Parameters.Add("@StudentNumber", SqliteType.Integer);
//                                insertCommand.Parameters.Add("@Name", SqliteType.Text);
//                                insertCommand.Parameters.Add("@Gender", SqliteType.Integer);
//                                insertCommand.Parameters.Add("@PhotoPath", SqliteType.Text);

//                                int totalCount = students.Count;
//                                if (totalCount != 0)
//                                {
//                                    for (int i = 0; i < totalCount; i++)
//                                    {
//                                        Student student = students[i];
//                                        insertCommand.Parameters["@StudentNumber"].Value = student.StudentNumber;
//                                        insertCommand.Parameters["@Name"].Value = student.Name;
//                                        insertCommand.Parameters["@Gender"].Value = student.Gender;
//                                        insertCommand.Parameters["@PhotoPath"].Value = student.PhotoPath;

//                                        await insertCommand.ExecuteNonQueryAsync();
//                                        // 计算并报告进度
//                                        SaveStudentsProcess = (int)((i + 1) * 100.0 / totalCount);
//                                    }
//                                }
//                                else
//                                {
//                                    SaveStudentsProcess = -1;
//                                }
//                            }
//                            // 提交事务
//                            transaction.Commit();
//                        }
//                        catch (SqliteException sqliteEx)
//                        {
//                            transaction.Rollback(); // 发生错误时回滚事务
//                                                    //处理数据库异常
//                            PopupNotice popupNotice = new PopupNotice("保存学生信息失败,发生数据库异常" + sqliteEx.Message);
//                            popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                            popupNotice.ShowPopup();
//                        }
//                        catch (UnauthorizedAccessException unauthorizedEx)
//                        {
//                            transaction.Rollback(); // 发生错误时回滚事务
//                                                    // 处理权限不足异常
//                            PopupNotice popupNotice = new PopupNotice("保存学生信息失败,权限不足" + unauthorizedEx.Message);
//                            popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                            popupNotice.ShowPopup();
//                        }
//                        catch (IOException ioEx)
//                        {
//                            transaction.Rollback(); // 发生错误时回滚事务
//                                                    // 处理输入输出异常，如文件损坏
//                            PopupNotice popupNotice = new PopupNotice("保存学生信息失败,发生文件操作异常" + ioEx.Message);
//                            popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                            popupNotice.ShowPopup();
//                        }
//                        catch (Exception ex)
//                        {
//                            transaction.Rollback(); // 发生错误时回滚事务
//                            PopupNotice popupNotice = new PopupNotice("保存学生信息数据库失败" + ex.Message);
//                            popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                            popupNotice.ShowPopup();
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    PopupNotice popupNotice = new PopupNotice("连接学生信息数据库失败" + ex.Message);
//                    popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                    popupNotice.ShowPopup();
//                }
//            }
//        }

//        public static async Task SaveCheckedStudentsAsync(List<ItemIndexRange> checkedstudents)
//        {
//            using (var connection = new SqliteConnection($"Filename={CheckedStudentDataBasePath}"))
//            {
//                await connection.OpenAsync();

//                // 开始事务
//                using (var transaction = connection.BeginTransaction())
//                {
//                    try
//                    {
//                        // 先清空旧数据
//                        var clearCommand = connection.CreateCommand();
//                        clearCommand.CommandText = "DELETE  FROM CheckedStudents";
//                        await clearCommand.ExecuteNonQueryAsync();

//                        string insertQuery = "INSERT INTO CheckedStudents (FirstIndex, LastIndex) VALUES (@FirstIndex, @LastIndex)";
//                        using (SqliteCommand insertCommand = new SqliteCommand(insertQuery, connection, (SqliteTransaction)transaction))
//                        {
//                            insertCommand.Parameters.Add("@FirstIndex", SqliteType.Integer);
//                            insertCommand.Parameters.Add("@LastIndex", SqliteType.Integer);
//                            int totalCount = checkedstudents.Count;
//                            if (totalCount != 0)
//                            {
//                                for (int i = 0; i < totalCount; i++)
//                                {
//                                    ItemIndexRange range = checkedstudents[i];
//                                    insertCommand.Parameters["@FirstIndex"].Value = range.FirstIndex;
//                                    insertCommand.Parameters["@LastIndex"].Value = range.LastIndex;
//                                    await insertCommand.ExecuteNonQueryAsync();
//                                    // 计算并报告进度
//                                    SaveCheckedStudentsProcess = (int)((i + 1) * 100.0 / totalCount);
//                                }
//                            }
//                            else
//                            {
//                                SaveCheckedStudentsProcess = -1;
//                            }
//                            //提交事务
//                            transaction.Commit();
//                        }
//                    }
//                    catch (SqliteException sqliteEx)
//                    {
//                        transaction.Rollback(); // 发生错误时回滚事务
//                                                //处理数据库异常
//                        PopupNotice popupNotice = new PopupNotice("保存抽取范围信息失败,发生数据库异常" + sqliteEx.Message);
//                        popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                        popupNotice.ShowPopup();
//                    }
//                    catch (UnauthorizedAccessException unauthorizedEx)
//                    {
//                        transaction.Rollback(); // 发生错误时回滚事务
//                                                // 处理权限不足异常
//                        PopupNotice popupNotice = new PopupNotice("保存抽取范围信息失败,权限不足" + unauthorizedEx.Message);
//                        popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                        popupNotice.ShowPopup();
//                    }
//                    catch (IOException ioEx)
//                    {
//                        transaction.Rollback(); // 发生错误时回滚事务
//                                                // 处理输入输出异常，如文件损坏
//                        PopupNotice popupNotice = new PopupNotice("保存抽取范围信息失败,发生文件操作异常" + ioEx.Message);
//                        popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                        popupNotice.ShowPopup();
//                    }
//                    catch (Exception ex)
//                    {
//                        transaction.Rollback(); // 发生错误时回滚事务
//                        PopupNotice popupNotice = new PopupNotice("保存抽取范围信息数据库失败" + ex.Message);
//                        popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                        popupNotice.ShowPopup();
//                    }
//                }
//            }

//        }

//        public static async Task<List<ItemIndexRange>> LoadCheckedStudentsAsync()
//        {
//            try
//            {
//                var ranges = new List<ItemIndexRange>();

//                using (var connection = new SqliteConnection($"Filename={CheckedStudentDataBasePath}"))
//                {
//                    await connection.OpenAsync();

//                    var command = connection.CreateCommand();
//                    command.CommandText = "SELECT * FROM CheckedStudents";

//                    using (var reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            int firstIndex = reader.GetInt32(0);
//                            int length = reader.GetInt32(1) - firstIndex + 1;
//                            ranges.Add(new ItemIndexRange(firstIndex, (uint)length));
//                        }
//                    }
//                }

//                return ranges;
//            }
//            catch (SqliteException sqliteEx)
//            {
//                //处理数据库异常
//                PopupNotice popupNotice = new PopupNotice("加载抽取范围信息失败,发生数据库异常" + sqliteEx.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            catch (UnauthorizedAccessException unauthorizedEx)
//            {
//                // 处理权限不足异常
//                PopupNotice popupNotice = new PopupNotice("加载抽取范围信息失败,权限不足" + unauthorizedEx.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            catch (IOException ioEx)
//            {
//                // 处理输入输出异常，如文件损坏
//                PopupNotice popupNotice = new PopupNotice("加载抽取范围信息失败,发生文件操作异常" + ioEx.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            catch (Exception ex)
//            {
//                PopupNotice popupNotice = new PopupNotice("加载抽取范围信息失败,发生未知错误" + ex.Message);
//                popupNotice.PopupContent.Severity = InfoBarSeverity.Error;
//                popupNotice.ShowPopup();
//            }
//            return new List<ItemIndexRange>();
//        }
//    }
//}
