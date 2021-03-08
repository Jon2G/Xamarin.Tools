using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.Sql.Base;
using SyncTest.Models;

namespace SyncTest
{
	public class UpdateTest
	{
		public async void Update (SqlBase con)
		{
			var products = con.Table<Prods> ().ToList ();
			foreach (var product in products) {
				product.Descrip += " Updated";
                await Task.Delay(100);
				con.Update (product);
			}

		}
	}
}
