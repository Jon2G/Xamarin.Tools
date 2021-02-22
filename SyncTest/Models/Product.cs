using System;
using System.Collections.Generic;
using System.Text;
using Kit.Sql.Attributes;


namespace SyncTest.Models
{
	public class Product 
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Name { get; set; }

		public Product ()
		{

		}
	}
}
