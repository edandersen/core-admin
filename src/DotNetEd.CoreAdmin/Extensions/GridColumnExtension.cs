using Microsoft.AspNetCore.Html;
using NonFactors.Mvc.Grid;

namespace DotNetEd.CoreAdmin.Extensions
{
	public static class GridColumnExtension
	{
		public static IHtmlContent GetValue(this IGridColumn column, IGridRow<object> row)
		{
			var properties = row.Model.GetType().GetProperties();
			foreach (var property in properties)
			{
				if (column.Name != property.Name)
				{
					continue;
				}

				var propertyValue = property.GetValue(row.Model, null);
				if (propertyValue != null)
				{
					var nestedProperties = property.PropertyType.GetProperties();
					foreach (var nestedProperty in nestedProperties)
					{
						if (nestedProperty.Name.ToLower().Contains("name"))
						{
							var nestedValue = nestedProperty.GetValue(propertyValue, null);
							if (nestedValue != null)
							{
								return new HtmlString(nestedValue.ToString());
							}
						}
					}
				}
			}
			return column.ValueFor(row);
		}
	}
}
