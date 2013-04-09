using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Replica.Tests
{
	[TestFixture]
	public sealed class ObjectGraphTests
	{
		[Test]
		public void Graph_Level1()
		{
			var person = new Person
				{
					Id = 1,
					FirstName = "John",
					LastName = "Neimann",
					IdentityId = 13231,
					BirthDate = new DateTime(1932, 6, 15),
					Orders = new List<Order>
						{
							new Order
								{
									ProductsCount = 4,
									Amount = 1300,
									OrderProducts = new List<OrderProduct>
										{
											new OrderProduct
												{
													Count = 2,
													Product = new Product
														{
															Id = 1,
															Name = "Halo 3",
															Price = 50
														}
												},
											new OrderProduct
												{
													Count = 1,
													Product = new Product
														{
															Id = 2,
															Name = "XBox",
															Price = 1000
														}
												},
											new OrderProduct
												{
													Count = 1,
													Product = new Product
														{
															Id = 3,
															Name = "Kinect",
															Price = 200
														}
												}
										}
								}
						}
				};

			var person2 = CloneHelper.Clone(person);

			Assert.AreEqual(person.Id, person2.Id);
			Assert.AreEqual(person.FirstName, person2.FirstName);
			Assert.AreEqual(person.LastName, person2.LastName);
			Assert.AreEqual(person.IdentityId, person2.IdentityId);
			Assert.AreEqual(person.BirthDate, person2.BirthDate);

			Assert.AreNotSame(person.Orders, person2.Orders);
			Assert.AreEqual(person.Orders.Count, person2.Orders.Count);
			for (int i = 0; i < person.Orders.Count; i++)
			{
				var order1 = person.Orders[i];
				var order2 = person2.Orders[i];
				Assert.AreNotSame(order1, order2);
				Assert.AreEqual(order1.ProductsCount, order2.ProductsCount);
				Assert.AreEqual(order1.Amount, order2.Amount);

				Assert.AreNotSame(order1.OrderProducts, order2.OrderProducts);
				Assert.AreEqual(order1.OrderProducts.Count, order2.OrderProducts.Count);

				for (int j = 0; j < order1.OrderProducts.Count; j++)
				{
					var productOrder1 = order1.OrderProducts[j];
					var productOrder2 = order2.OrderProducts[j];
					Assert.AreNotSame(productOrder1, productOrder2);

					Assert.AreEqual(productOrder1.Count, productOrder2.Count);
					Assert.AreNotSame(productOrder1.Product, productOrder2.Product);

					Assert.AreEqual(productOrder1.Product.Id, productOrder2.Product.Id);
					Assert.AreEqual(productOrder1.Product.Name, productOrder2.Product.Name);
					Assert.AreEqual(productOrder1.Product.Price, productOrder2.Product.Price);
				}
			}
		}
	}
}