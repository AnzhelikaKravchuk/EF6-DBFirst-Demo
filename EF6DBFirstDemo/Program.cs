using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Spatial;
using System.IO;
using System.Linq;

namespace EF6DBFirstDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //using (var writer = Console.Out)
            using (var writer = new StreamWriter(File.Open("LogFile.txt", FileMode.OpenOrCreate)))
            {
                AddUpdateDeleteEntityInConnectedScenario(writer);
                AddUpdateEntityInDisconnectedScenario(writer);
                LinqToEntitiesQueries(writer);
                FindEntity(writer);
                LazyLoading(writer);
                ExplicitLoading(writer);
                ExecuteRawSQLusingSqlQuery(writer);
                ExecuteSqlCommand(writer);
                DynamicProxy(writer);
                ReadDataUsingStoredProcedure(writer);
                SpatialDataType(writer);
                EntityEntry(writer);
                OptimisticConcurrency(writer);
                TransactionSupport(writer);
                SetEntityState(writer);
            }
            Console.WriteLine("Demo done!");
        }
        
        public static void AddUpdateDeleteEntityInConnectedScenario(TextWriter writer)
        {
            writer.WriteLine("*** AddUpdateDeleteEntityInConnectedScenario Starts ***");

            using (var context = new SchoolDBEntities())
            {
                //Log DB commands to writer
                context.Database.Log = writer.Write;

                //Add a new student and address
                var newStudent = context.Students.Add(new Student()
                {
                    StudentName = "Jonathan",
                    StudentAddress = new StudentAddress()
                    {
                        Address1 = "1, Harbourside",
                        City = "Jersey City",
                        State = "NJ"
                    }
                });

                context.SaveChanges(); // Executes Insert command

                //Edit student name
                newStudent.StudentName = "Alex";

                //context.SaveChanges(); // Executes Update command

                //Remove student
                context.Students.Remove(newStudent);

                context.SaveChanges(); // Executes Delete command
            }

            writer.WriteLine("*** AddUpdateDeleteEntityInConnectedScenario Ends ***");
        }

        public static void AddUpdateEntityInDisconnectedScenario(TextWriter writer)
        {
            writer.WriteLine("*** AddUpdateEntityInDisconnectedScenario Starts ***");

            // disconnected entities
            var newStudent = new Student()
            {
                StudentName = "Bill"
            };
            var existingStudent = new Student()
            {
                StudentID = 10,
                StudentName = "Chris"
            };

            using (var context = new SchoolDBEntities())
            {
                //Log DB commands to writer
                context.Database.Log = writer.WriteLine;

                context.Entry(newStudent).State = newStudent.StudentID == 0 ? EntityState.Added : EntityState.Modified;
                context.Entry(existingStudent).State = existingStudent.StudentID == 0 ? EntityState.Added : EntityState.Modified;

                context.SaveChanges(); // Executes Delete command
            }

            writer.WriteLine("*** AddUpdateEntityInDisconnectedScenario Ends ***");
        }

        public static void LinqToEntitiesQueries(TextWriter writer)
        {
            writer.WriteLine("*** LinqToEntitiesQueries Starts  ***");

            using (var context = new SchoolDBEntities())
            {
                //Log DB commands to writer
                context.Database.Log = writer.WriteLine;

                //Retrieve students whose name is Bill - Linq-to-Entities Query Syntax
                var students = (from s in context.Students
                                where s.StudentName == "Bill"
                                select s).ToList();

                //Retrieve students with the same name - Linq-to-Entities Method Syntax
                var studentsWithSameName = context.Students
                    .GroupBy(s => s.StudentName)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                writer.WriteLine("Students with same name");
                foreach (var stud in studentsWithSameName)
                {
                    writer.WriteLine(stud);
                }

                //Retrieve students of standard 1
                var standard1Students = context.Students
                    .Where(s => s.StandardId == 1)
                    .ToList();
            }

            writer.WriteLine("*** LinqToEntitiesQueries Ends ***");
        }

        public static void FindEntity(TextWriter writer)
        {
            writer.WriteLine("*** FindEntity Starts  ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                var student = context.Students.Find(1);

                if (student != null)
                {
                    writer.WriteLine(student.StudentName + " found");
                }
            }

            writer.WriteLine("*** FindEntity Ends ***");
        }

        public static void LazyLoading(TextWriter writer)
        {
            writer.WriteLine("*** LazyLoading Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                Student student = context.Students
                    .FirstOrDefault(s => s.StudentID == 1);

                writer.WriteLine("*** Retrieve standard from the database ***");
                if (student != null)
                {
                    Standard std = student.Standard;
                }
            }

            writer.WriteLine("*** LazyLoading Ends ***");
        }

        public static void ExplicitLoading(TextWriter writer)
        {
            writer.WriteLine("*** ExplicitLoading Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                Student std = context.Students
                    .FirstOrDefault(s => s.StudentID == 1);

                //Loading Standard for Student (seperate SQL query)
                context.Entry(std).Reference(s => s.Standard).Load();

                //Loading Courses for Student (seperate SQL query)
                context.Entry(std).Collection(s => s.Courses).Load();
            }

            writer.WriteLine("*** ExplicitLoading Ends ***");
        }

        public static void ExecuteRawSQLusingSqlQuery(TextWriter writer)
        {
            writer.WriteLine("*** ExecuteRawSQLusingSqlQuery Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                var studentList = context.Students.SqlQuery("Select * from Student").ToList<Student>();

                var student = context.Students.SqlQuery("Select StudentId, StudentName, StandardId, RowVersion from Student where StudentId = 1").ToList();
            }

            writer.WriteLine("*** ExecuteRawSQLusingSqlQuery Ends ***");
        }

        public static void ExecuteSqlCommand(TextWriter writer)
        {
            writer.WriteLine("*** ExecuteSqlCommand Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                //Insert command
                int noOfRowInsert = context.Database.ExecuteSqlCommand("insert into student(studentname) values('Robert')");

                //Update command
                int noOfRowUpdate = context.Database.ExecuteSqlCommand("Update student set studentname ='Mark' where studentname = 'Robert'");

                //Delete command
                int noOfRowDeleted = context.Database.ExecuteSqlCommand("delete from student where studentname = 'Mark'");
            }

            writer.WriteLine("*** ExecuteSqlCommand Ends ***");
        }

        public static void DynamicProxy(TextWriter writer)
        {
            writer.WriteLine("*** DynamicProxy Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                var student = context.Students
                        .FirstOrDefault(s => s.StudentName == "Bill");

                writer.WriteLine("Proxy Type: {0}", student.GetType().Name);
                writer.WriteLine("Underlying Entity Type: {0}", student.GetType().BaseType);

                //Disable Proxy creation
                context.Configuration.ProxyCreationEnabled = false;

                writer.WriteLine("Proxy Creation Disabled");

                var student1 = context.Students
                        .FirstOrDefault(s => s.StudentName == "Steve");

                writer.WriteLine("Entity Type: {0}", student1.GetType().Name);
            }

            writer.WriteLine("*** DynamicProxy Ends ***");
        }

        public static void ReadDataUsingStoredProcedure(TextWriter writer)
        {
            writer.WriteLine("*** ReadDataUsingStoredProcedure Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                //get all the courses of student whose id is 1
                var courses = context.GetCoursesByStudentId(1);
                //Set Course entity as return type of GetCoursesByStudentId function
                //Open ModelBrowser -> Function Imports -> Right click on GetCoursesByStudentId and Edit
                //Change Returns a Collection of to Course Entity from Complex Type
                //uncomment following lines
                //foreach (Course cs in courses)
                //    writer.WriteLine(cs.CourseName);
            }

            writer.WriteLine("*** ReadDataUsingStoredProcedure Ends ***");
        }

        public static void ChangeTracker(TextWriter writer)
        {
            writer.WriteLine("*** ChangeTracker Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                context.Configuration.ProxyCreationEnabled = false;

                var student = context.Students.Add(new Student() { StudentName = "Mili" });
                DisplayTrackedEntities(context,writer);

                writer.WriteLine("Retrieve Student");
                var existingStudent = context.Students.Find(1);

                DisplayTrackedEntities(context, writer);

                writer.WriteLine("Retrieve Standard");
                var standard = context.Standards.Find(1);

                DisplayTrackedEntities(context, writer);

                writer.WriteLine("Editing Standard");
                standard.StandardName = "Grade 5";

                DisplayTrackedEntities(context, writer);

                writer.WriteLine("Remove Student");
                context.Students.Remove(existingStudent);
                DisplayTrackedEntities(context, writer);
            }

            writer.WriteLine("*** ChangeTracker Ends ***");
        }

        private static void DisplayTrackedEntities(SchoolDBEntities context, TextWriter writer)
        {
            context.Database.Log = writer.WriteLine;
            writer.WriteLine("Context is tracking {0} entities.", context.ChangeTracker.Entries().Count());
            DbChangeTracker changeTracker = context.ChangeTracker;
            var entries = changeTracker.Entries();
            foreach (var entry in entries)
            {
                writer.WriteLine("Entity Name: {0}", entry.Entity.GetType().FullName);
                writer.WriteLine("Status: {0}", entry.State);
            }
        }

        public static void SpatialDataType(TextWriter writer)
        {
            writer.WriteLine("*** SpatialDataType Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                //Add Location using System.Data.Entity.Spatial.DbGeography
                context.Courses.Add(new Course() { CourseName = "New Course from SpatialDataTypeDemo", Location = DbGeography.FromText("POINT(-122.360 47.656)") });

                context.SaveChanges();
            }

            writer.WriteLine("*** SpatialDataTypeDemo Ends ***");
        }

        public static void EntityEntry(TextWriter writer)
        {
            writer.WriteLine("*** EntityEntry Starts ***");

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                //get student whose StudentId is 1
                var student = context.Students.Find(1);

                //edit student name
                student.StudentName = "Monica";

                //get DbEntityEntry object for student entity object
                var entry = context.Entry(student);

                //get entity information e.g. full name
                writer.WriteLine("Entity Name: {0}", entry.Entity.GetType().FullName);

                //get current EntityState
                writer.WriteLine("Entity State: {0}", entry.State);

                writer.WriteLine("********Property Values********");

                foreach (var propertyName in entry.CurrentValues.PropertyNames)
                {
                    writer.WriteLine("Property Name: {0}", propertyName);

                    //get original value
                    var orgVal = entry.OriginalValues[propertyName];
                    writer.WriteLine("     Original Value: {0}", orgVal);

                    //get current values
                    var curVal = entry.CurrentValues[propertyName];
                    writer.WriteLine("     Current Value: {0}", curVal);
                }
            }

            writer.WriteLine("*** EntityEntryDemo Ends ***");
        }

        public static void TransactionSupport(TextWriter writer)
        {
            writer.WriteLine("*** TransactionSupport Starts ***");

            using (var context = new SchoolDBEntities())
            {
                writer.WriteLine("Built-in Transaction");
                context.Database.Log = writer.WriteLine;

                //Add a new student and address
                context.Students.Add(new Student() { StudentName = "Kapil" });

                var existingStudent = context.Students.Find(10);
                //Edit student name
                existingStudent.StudentName = "Alex";

                context.SaveChanges(); // Executes Insert & Update command under one transaction
            }

            writer.WriteLine("External Transaction");
            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                using (DbContextTransaction transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.Students.Add(new Student()
                        {
                            StudentName = "Arjun"
                        });
                        context.SaveChanges();

                        context.Courses.Add(new Course() { CourseName = "Entity Framework" });
                        context.SaveChanges();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        writer.WriteLine("Error occurred.");
                    }
                }
            }

            writer.WriteLine("*** TransactionSupport Ends ***");
        }

        public static void SetEntityState(TextWriter writer)
        {
            writer.WriteLine("*** SetEntityState Starts ***");

            var student = new Student()
            {
                StudentID = 1, // root entity with key
                StudentName = "Bill",
                StandardId = 1,
                Standard = new Standard()   //Child entity (with key value)
                {
                    StandardId = 1,
                    StandardName = "Grade 1"
                },
                Courses = new List<Course>() {
                    new Course(){  CourseName = "Machine Language" }, //Child entity (empty key)
                    new Course(){  CourseId = 2 } //Child entity (with key value)
                }
            };

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                context.Entry(student).State = EntityState.Modified;

                foreach (var entity in context.ChangeTracker.Entries())
                {
                    writer.WriteLine("{0}: {1}", entity.Entity.GetType().Name, entity.State);
                }
            }

            writer.WriteLine("*** SetEntityState Ends ***");
        }

        public static void OptimisticConcurrency(TextWriter writer)
        {
            writer.WriteLine("*** OptimisticConcurrency Starts ***");

            Student student = null;

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;
                student = context.Students.First();
            }

            //Edit student name
            student.StudentName = "Robin";

            using (var context = new SchoolDBEntities())
            {
                context.Database.Log = writer.WriteLine;

                try
                {
                    context.Entry(student).State = EntityState.Modified;
                    context.SaveChanges();

                    writer.WriteLine("Student saved successfully.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    writer.WriteLine("Concurrency Exception Occurred.");
                }
            }

            writer.WriteLine("*** OptimisticConcurrency Ends ***");
        }
    }
}