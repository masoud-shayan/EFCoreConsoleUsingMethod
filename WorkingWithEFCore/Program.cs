using System;
using System.Collections.Generic;
using static System.Console;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WorkingWithEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            // QueryingCategories(); 
            // QueryingProducts();
            // QueryingWithLike();
            // AddProduct(6, "masoud shayan", 500M);
            // IncreaseProductPrice("maso" , 10);
            // DeleteProducts("maso");
            // JoinCategoriesAndProducts();
            GroupJoinCategoriesAndProducts();
        }


        //select - where
        static void QueryingCategories()
        {
            using (var db = new Northwind())
            {
                WriteLine(
                    "Categories and how many products they have:"); // a query to get all categories and their related products
                IQueryable<Category> cats = db.Categories
                    .Include(c => c.Products); // does not need to use include(join) in lazy loading pattern
                foreach (Category c in cats)
                {
                    WriteLine($"{c.CategoryName} has {c.Products.Count}products.");
                }
            }
        }


        //select - where
        static void QueryingProducts()
        {
            using (var db = new Northwind())
            {
                WriteLine("Products that cost more than a price, highest at top.");

                string input;
                decimal price;

                do
                {
                    WriteLine("Enter a product price :");
                    input = ReadLine();
                } while (!decimal.TryParse(input, out price));

                Console.WriteLine(price);

                // server-side evaluating in EF core 3.1 but does not work on SQlite because unknown Decimal type

                // IQueryable<Product> prods =  db.Products
                //     .Where(product => product.Cost.GetValueOrDefault() > price)
                //     .OrderByDescending(product => product.Cost);


                // client-side evaluating in EF core 3.1 

                // IEnumerable<Product> prods = db.Products
                //     .AsEnumerable()
                //     .Where(product => product.Cost > price)
                //     .OrderByDescending(product => product.Cost)
                //     .ToList();


                // server-side evaluating in EF core 3.1 and works on SQlite because we convert decimal to double type

                IQueryable<Product> prods = db.Products
                    .Where(product => (double) product.Cost > (double) price)
                    .OrderByDescending(product => (double) product.Cost);


                foreach (var product in prods)
                {
                    WriteLine(
                        $"pId : {product.ProductID} - pName : {product.ProductName} - pCost : {product.Cost:$#} - PUnit : {product.Stock} in Stock");
                }
            }
        }


        //select - where with like
        static void QueryingWithLike()
        {
            using (var db = new Northwind())
            {
                WriteLine("Enter part of a product name: ");
                string input = ReadLine();

                IQueryable<Product> prods = db.Products
                    .Where(product => EF.Functions.Like(product.ProductName, $"%{input}%"));

                foreach (var product in prods)
                {
                    WriteLine(
                        $"name : {product.ProductName} - stock : {product.Stock} - discounted : {product.Discontinued}");
                }
            }
        }


        // select - orderByDescending 
        static void ListProducts()
        {
            using (var db = new Northwind())
            {
                WriteLine("{0,-3} {1,-35} {2,8} {3,5} {4}",
                    "ID", "Product Name", "Cost", "Stock", "Disc.");

                IQueryable<Product> prods = db.Products
                    .OrderByDescending(p => (double) p.Cost);

                foreach (var item in prods)
                {
                    WriteLine("{0:000} {1,-35} {2,8:$#,##0.00} {3,5} {4}",
                        item.ProductID, item.ProductName, item.Cost,
                        item.Stock, item.Discontinued);
                }
            }
        }


        // insert
        static void AddProduct(int catergoryID, string productName, decimal? price)
        {
            using (var db = new Northwind())
            {
                var newProduct = new Product
                {
                    CategoryID = catergoryID,
                    ProductName = productName,
                    Cost = price
                };

                db.Products.Add(newProduct);

                int affected = db.SaveChanges();


                if (affected == 1)
                {
                    ListProducts();
                }
                else
                {
                    WriteLine("the last transaction did not execute");
                }
            }
        }


        // update
        static void IncreaseProductPrice(string name, decimal amount)
        {
            using (var db = new Northwind())
            {
                // get first product whose name starts with name

                Product updateProduct = db.Products
                    .First(product => product.ProductName.StartsWith(name));

                updateProduct.Cost += amount;

                int affected = db.SaveChanges();

                if (affected == 1)
                {
                    ListProducts();
                }
                else
                {
                    WriteLine("the last transaction did not execute");
                }
            }
        }


        //delete
        static void DeleteProducts(string name)
        {
            using (var db = new Northwind())
            {
                IQueryable<Product> deleteProds = db.Products
                    .Where(product => product.ProductName.StartsWith(name));

                db.Products.RemoveRange(deleteProds);

                int deleted = db.SaveChanges();
                if (deleted > 0)
                {
                    WriteLine($"{deleted} product(s) were deleted.");
                    ListProducts();
                }
                else
                {
                    WriteLine("the last transaction did not execute");
                }
            }
        }

        
        
        
        // Inner Join 
        static void JoinCategoriesAndProducts()
        {
            using (var db = new Northwind())
            {
                var query = db.Categories
                    .Join(
                        db.Products,
                        category => category.CategoryID,
                        product => product.CategoryID,
                        (category, product) => new
                        {
                            category.CategoryName,
                            product.ProductName,
                            product.ProductID
                        });


                foreach (var item in query)
                {
                    Console.WriteLine($"{item.ProductID}: {item.ProductName} is in {item.CategoryName}.");
                }
            }
        }

        
        
        // Left Outer Join
        static void GroupJoinCategoriesAndProducts()
        {
            using (var db = new Northwind())
            {
                // group all products by their category to return 8 matches
                var queryGroup = db.Categories.AsEnumerable()
                    .GroupJoin(
                        db.Products,
                        category => category.CategoryID,
                        product => product.CategoryID,
                        (c, matchingProducts)
                            => new
                            {
                                c.CategoryName,
                                Products = matchingProducts.OrderBy(p => p.ProductName)
                            });
                
                
                foreach (var item in queryGroup)
                {
                    Console.WriteLine($"{item.CategoryName} has {item.Products.Count()} products.");
                    
                    foreach (var product in item.Products)
                    {
                        Console.WriteLine($" {product.ProductName}");
                    }
                }
            }
        }
        
    }
}