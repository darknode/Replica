using System;
using System.Collections.Generic;

namespace Replica.Tests
{
	public sealed class Person
	{
		public int Id;
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int? IdentityId { get; set; }
		public DateTime BirthDate { get; set; }
		public List<Order> Orders { get; set; }
	}

	public sealed class Order
	{
		public int ProductsCount { get; set; }
		public decimal Amount { get; set; }
		public List<OrderProduct> OrderProducts { get; set; }
	}

	public sealed class OrderProduct
	{
		public int Count { get; set; }
		public Product Product { get; set; }
	}

	public sealed class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
	}
}
