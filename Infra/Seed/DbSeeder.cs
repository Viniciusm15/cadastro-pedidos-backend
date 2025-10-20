using Bogus;
using Domain.Enums;
using Domain.Models.Entities;
using Infra.Data;
using Microsoft.AspNetCore.Identity;

namespace Infra.Seed
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbSeeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Seed(bool clearData = false)
        {
            if (clearData)
                await ClearAllData();

            await SeedRoles();
            await SeedUsersAndClients();
            await SeedCategories();
            await SeedProducts();
            await SeedOrders();
            await SeedOrderItems();
        }

        private async Task ClearAllData()
        {
            _context.OrderItems.RemoveRange(_context.OrderItems);
            _context.Orders.RemoveRange(_context.Orders);
            _context.Clients.RemoveRange(_context.Clients);
            _context.Products.RemoveRange(_context.Products);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Images.RemoveRange(_context.Images);

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                await _userManager.DeleteAsync(user);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedRoles()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
            }
        }

        private async Task SeedUsersAndClients()
        {
            if (_context.Clients.Count() >= 50) return;

            var existingEmails = _context.Clients.Select(c => c.Email).ToHashSet();
            var userFaker = new Faker<ApplicationUser>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.UserName, (f, u) => u.Email)
                .RuleFor(u => u.EmailConfirmed, f => true)
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("(##) #####-####"));

            var clientFaker = new Faker<Client>()
                .RuleFor(c => c.Name, (f, c) => $"{f.Name.FirstName()} {f.Name.LastName()}")
                .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name))
                .RuleFor(c => c.Telephone, f => f.Phone.PhoneNumber("(##) #####-####"))
                .RuleFor(c => c.BirthDate, f => f.Date.Past(60, DateTime.Now.AddYears(-18)))
                .RuleFor(c => c.CreatedAt, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now));

            var newClients = new List<Client>();

            while (_context.Clients.Count() + newClients.Count < 50)
            {
                var user = userFaker.Generate();
                var result = await _userManager.CreateAsync(user, "Senha123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");

                    var client = clientFaker.Generate();
                    client.ApplicationUserId = user.Id;
                    client.ApplicationUser = user;
                    client.Email = user.Email;

                    if (!existingEmails.Contains(client.Email))
                    {
                        existingEmails.Add(client.Email);
                        newClients.Add(client);
                    }
                }
            }

            _context.Clients.AddRange(newClients);
            await _context.SaveChangesAsync();

            var adminUser = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "System",
                Email = "admin@example.com",
                UserName = "admin@example.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            var adminResult = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (adminResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");

                var adminClient = new Client
                {
                    Name = "Admin System",
                    Email = adminUser.Email,
                    Telephone = "(11) 99999-9999",
                    BirthDate = new DateTime(1980, 1, 1),
                    ApplicationUserId = adminUser.Id,
                    ApplicationUser = adminUser,
                    IsActive = true,
                    DeletedAt = null
                };

                _context.Clients.Add(adminClient);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedCategories()
        {
            if (_context.Categories.Count() >= 8) return;

            var faker = new Faker<Category>()
                .RuleFor(c => c.Name, (f, c) => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Lorem.Sentence());

            var categories = new List<Category>();

            var categoryNames = new HashSet<string>();
            while (categories.Count < 8)
            {
                var category = faker.Generate();
                if (categoryNames.Add(category.Name))
                {
                    categories.Add(category);
                }
            }

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProducts()
        {
            if (_context.Products.Count() >= 100) return;

            var categoryIds = _context.Categories.Select(c => c.Id).ToList();
            var faker = new Faker();
            var usedProductNames = new HashSet<string>();

            for (int i = 0; i < 100; i++)
            {
                string productName;
                do
                {
                    productName = faker.Commerce.ProductName();
                } while (!usedProductNames.Add(productName));

                var image = new Image
                {
                    ImageData = faker.Random.Bytes(200),
                    ImageMimeType = "image/png",
                    Description = faker.Commerce.ProductAdjective(),
                    EntityType = nameof(Product)
                };
                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                var product = new Product
                {
                    Name = productName,
                    Description = faker.Commerce.ProductDescription(),
                    Price = double.Parse(faker.Commerce.Price(10, 1000)),
                    StockQuantity = faker.Random.Int(0, 100),
                    CategoryId = faker.PickRandom(categoryIds),
                    ImageId = image.Id,
                    Image = image,
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                image.EntityId = product.Id;
                _context.Images.Update(image);
                await _context.SaveChangesAsync();
            }

        }

        private async Task SeedOrders()
        {
            if (_context.Orders.Count() >= 80) return;

            var faker = new Faker();
            var clientIds = _context.Clients.Where(c => c.IsActive).Select(c => c.Id).ToList();
            var orders = new List<Order>();

            for (int i = 0; i < 80; i++)
            {
                var order = new Order
                {
                    ClientId = faker.PickRandom(clientIds),
                    OrderDate = faker.Date.Past(1),
                    Status = faker.PickRandom<OrderStatus>(),
                    TotalValue = 0.0
                };

                orders.Add(order);
            }

            _context.Orders.AddRange(orders);
            await _context.SaveChangesAsync();
        }

        private async Task SeedOrderItems()
        {
            if (_context.OrderItems.Count() > 0) return;

            var faker = new Faker();
            var orders = _context.Orders.ToList();
            var products = _context.Products.Where(p => p.IsActive).ToList();
            var orderItems = new List<OrderItem>();

            foreach (var order in orders)
            {
                int itemCount = faker.Random.Int(1, 5);
                var usedProductIds = new HashSet<int>();
                double orderTotal = 0;

                for (int i = 0; i < itemCount; i++)
                {
                    Product product;
                    do
                    {
                        product = faker.PickRandom(products);
                    } while (!usedProductIds.Add(product.Id));

                    int quantity = faker.Random.Int(1, 10);
                    double itemTotal = product.Price * quantity;

                    var item = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitaryPrice = product.Price
                    };

                    orderItems.Add(item);
                    orderTotal += itemTotal;
                }

                order.TotalValue = Math.Round(orderTotal, 2);
                _context.Orders.Update(order);
            }

            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();
        }
    }
}
