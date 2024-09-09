using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace CW_09_09
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DbManager dbM = new DbManager();
            //dbM.EnsurePopulate();


            //var user = dbM.GetUserById(1);
            //var product = new Product { Name = "New product", Description = "123" };
            //var newOrder = new Order { UserId = user.Id,User = user, Products = new List<Product> { product } };

            //dbM.AddOrder(newOrder,user.Id);


            var order = dbM.GetOrderById(1);
            dbM.GetProductAndUserByOrder(order);

            db.DeleteOrder(1);

        }


        public class ApplicationContext : DbContext
        {

            public DbSet<User> Users { get; set; }

            public DbSet<Category> Categories { get; set; }

            public DbSet<Product> Products { get; set; }

            public DbSet<Order> Orders { get; set; }


            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-R3LQDV9;Database = testDB1;Trusted_Connection =True;TrustServerCertificate=True");
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Category>().HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(c => c.CategoryId);

                modelBuilder.Entity<Product>().HasMany(p => p.Orders).WithMany(o => o.Products);

                modelBuilder.Entity<Order>().HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
            }
        }


        public class DbManager
        {
            public void EnsurePopulate()
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var categories = new List<Category>
                    {
                        new Category { Name = "Category1 ", Description = "1" },
                        new Category { Name = "Category2 ", Description = "2" }

                    };


                    var product = new List<Product>
                    {
                        new Product { Name = "Product1", Description = "prod1", Category = categories[0] },
                        new Product{ Name = "Product2" ,Description = "prod2",Category = categories[1]},

                    };


                    var users = new List<User>
                    {
                        new User {FullName = "Tom",Adress = "West str. 12" },
                        new User {FullName = "Tony",Adress = "East str.13"}
                    };

                    var orders = new List<Order>
                    {
                        new Order{User = users[0],Products = new(){ product[0], product[1] } },
                        new Order{User = users[1],Products = new(){product[1] } }
                    };


                    db.Categories.AddRange(categories);
                    db.Products.AddRange(product);
                    db.Users.AddRange(users);
                    db.Orders.AddRange(orders);

                    db.SaveChanges();

                }
            }


            public User? GetUserById(int Id)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    return db.Users.FirstOrDefault(u => u.Id == Id);
                }
            }

            public Product? GetProductById(int id)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    return db.Products.FirstOrDefault(p => p.Id == id);
                }
            }

            public Order? GetOrderById(int id)
            {
                using(ApplicationContext db = new ApplicationContext())
                {
                    return db.Orders.FirstOrDefault(o=>o.Id == id);
                }
            }

            public void AddOrder(Order order,int id)
            {
                using(ApplicationContext db = new ApplicationContext())
                {


                    db.Orders.Add(order);

                    db.SaveChanges();

                    
                    
                }
            }
            
            public void DeleteOrder(int orderId)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                    var order = GetOrderById(orderId);

                    db.Orders.Remove(order);

                    db.SaveChanges();

                }
            }

            public void GetProductAndUserByOrder(Order order)
            {
                using (ApplicationContext db = new ApplicationContext())
                {
                   
                    var result = db.Products.Where(p => p.Orders.Any(o => o.Id == order.Id)).Include(p => p.Orders).ThenInclude(o => o.User).ToList();

                }
            }

            
        }



        public class Category
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string? Description { get; set; }

            public List<Product> Products { get; set; } = new();

        }


        public class Product
        {
            public int Id { get; set; }

            public string Name { get; set; }
            public string? Description { get; set; }

            public double Price { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; } = new();

            public List<Order> Orders { get; set; } = new();


        }


        public class Order
        {
            public int Id { get; set; }

            public int UserId { get; set; }

            public User User { get; set; }

            public List<Product> Products { get; set; } = new();


        }


        public class User
        {
            public int Id { get; set; }

            public string FullName { get; set; }

            public string Adress { get; set; }

            public List<Order> Orders { get; set; } = new();
        }
    }
}
