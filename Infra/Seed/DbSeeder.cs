using Bogus;
using Domain.Enums;
using Domain.Models.Entities;
using Infra.Data;

namespace Infra.Seed
{
    public class DbSeeder(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        public void Seed(bool clearData = false)
        {
            if (clearData)
                ClearAllData();

            SeedClients();
            SeedCategories();
            SeedProducts();
            SeedOrders();
            SeedOrderItems();
        }

        private void ClearAllData()
        {
            _context.Clients.RemoveRange(_context.Clients);
            _context.Categories.RemoveRange(_context.Categories);
            _context.Images.RemoveRange(_context.Images);
            _context.Products.RemoveRange(_context.Products);
            _context.Orders.RemoveRange(_context.Orders);
            _context.OrderItems.RemoveRange(_context.OrderItems);
            _context.SaveChanges();
        }

        private void SeedClients()
        {
            if (_context.Clients.Count() >= 50) return;

            var existingEmails = _context.Clients.Select(c => c.Email).ToHashSet();

            var faker = new Faker<Client>()
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name))
                .RuleFor(c => c.Telephone, f => f.Phone.PhoneNumber("(##) #####-####"))
                .RuleFor(c => c.BirthDate, f => f.Date.Past(60, DateTime.Now.AddYears(-18)));

            var newClients = new List<Client>();

            while (_context.Clients.Count() + newClients.Count < 50)
            {
                var client = faker.Generate();
                if (!existingEmails.Contains(client.Email))
                {
                    existingEmails.Add(client.Email);
                    newClients.Add(client);
                }
            }

            _context.Clients.AddRange(newClients);
            _context.SaveChanges();
        }

        private void SeedCategories()
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
            _context.SaveChanges();
        }

        private void SeedProducts()
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
                _context.SaveChanges();

                var product = new Product
                {
                    Name = productName,
                    Description = faker.Commerce.ProductDescription(),
                    Price = double.Parse(faker.Commerce.Price(10, 1000)),
                    StockQuantity = faker.Random.Int(0, 100),
                    CategoryId = faker.PickRandom(categoryIds),
                    ImageId = image.Id,
                    Image = image
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                image.EntityId = product.Id;
                _context.Images.Update(image);
                _context.SaveChanges();
            }
        }

        private void SeedOrders()
        {
            if (_context.Orders.Count() >= 80) return;

            var faker = new Faker();
            var clientIds = _context.Clients.Select(c => c.Id).ToList();
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
            _context.SaveChanges();
        }

        private void SeedOrderItems()
        {
            if (_context.OrderItems.Count() > 0) return;

            var faker = new Faker();
            var orders = _context.Orders.ToList();
            var products = _context.Products.ToList();
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
            _context.SaveChanges();
        }
    }
}
